using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Project_SSAAC.GameObjects;
using Project_SSAAC.World;
using System.Diagnostics;
using System.Linq;


namespace Project_SSAAC
{
    public partial class Form1 : Form
    {
        private Player player;
        private List<Enemy> enemies = new List<Enemy>();
        private List<Projectile> projectiles = new List<Projectile>();
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private Timer gameTimer;
        private Stopwatch gameStopwatch;
        private float lastFrameTime = 0f;
        private float shootCooldownTimer = 0f;
        private const float SHOOT_COOLDOWN = 0.3f;
        private Level currentLevel;
        private LevelGenerator levelGenerator;

        private string currentPlayerPuzzleInput = "";
        private float currentPuzzleTimeLeft = 0f;
        private bool isInPuzzleInputMode = false;
        private Room activePuzzleRoom = null;

        private float currentSurvivalTimeLeft = 0f;
        private Room activeSurvivalRoom = null;

        public Form1()
        {
            Debug.WriteLine("[Form1] Constructor - Start");
            InitializeComponent();

            LoadControl(new HomeControl(this)); // from character branch

            this.DoubleBuffered = true;
            Debug.WriteLine("[Form1] Constructor - DoubleBuffered set");

            InitializeGame();
            Debug.WriteLine("[Form1] Constructor - InitializeGame finished");
            InitializeTimer();
            Debug.WriteLine("[Form1] Constructor - InitializeTimer finished. Constructor End.");
        }

        public void LoadControl(UserControl control)
        {
            //  유저 컨트롤을 로드함
            panelMain.Controls.Clear();
            control.Dock = DockStyle.Fill;
            panelMain.Controls.Add(control);
        }

        private void InitializeGame()
        {

            Debug.WriteLine("[Form1] InitializeGame - Start");
            this.ClientSize = new Size(1024, 576); // 화면 크기 1024x576 설정
            this.Text = "Project SSAAC (1024x576)";

            try
            {
                levelGenerator = new LevelGenerator(this.ClientSize);
                Debug.WriteLine("[Form1] InitializeGame - LevelGenerator created with ClientSize: " + this.ClientSize);
                currentLevel = levelGenerator.GenerateLevel(numberOfRooms: 7);
                Debug.WriteLine($"[Form1] InitializeGame - Level generated. Rooms: {currentLevel?.Rooms?.Count}, CurrentRoom: {currentLevel?.CurrentRoom?.GridPosition}");


                if (currentLevel == null || currentLevel.Rooms.Count == 0 || currentLevel.CurrentRoom == null)
                {
                    string errorDetail = $"Level: {(currentLevel == null ? "null" : "OK")}, Rooms.Count: {(currentLevel?.Rooms == null ? "N/A" : currentLevel.Rooms.Count.ToString())}, CurrentRoom: {(currentLevel?.CurrentRoom == null ? "null" : "OK")}";
                    MessageBox.Show($"레벨 생성 실패 또는 유효하지 않은 레벨 상태.\n{errorDetail}", "초기화 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SafeApplicationExit(); return;
                }

                SizeF playerSize = new SizeF(32, 32);
                player = new Player(new PointF(
                    this.ClientSize.Width / 2f - playerSize.Width / 2f,
                    this.ClientSize.Height / 2f - playerSize.Height / 2f
                ));
                Debug.WriteLine("[Form1] InitializeGame - Player created");

                LoadCurrentRoomObjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"게임 초기화 중 예외 발생: {ex.Message}\n{ex.StackTrace}", "초기화 심각한 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SafeApplicationExit(); return;
            }

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            Debug.WriteLine("[Form1] InitializeGame - End");
        }

        private void InitializeTimer()
        {
            Debug.WriteLine("[Form1] InitializeTimer - Start");
            gameTimer = new Timer { Interval = 16 };
            gameTimer.Tick += GameTimer_Tick;
            gameStopwatch = Stopwatch.StartNew();
            lastFrameTime = (float)gameStopwatch.Elapsed.TotalSeconds;
            gameTimer.Start();
            Debug.WriteLine($"[Form1] InitializeTimer - Timer started. Enabled: {gameTimer.Enabled}");
        }

        private void LoadCurrentRoomObjects()
        {
            if (currentLevel == null || currentLevel.CurrentRoom == null)
            {
                Debug.WriteLine("[Form1] LoadCurrentRoomObjects - CRITICAL: currentLevel or CurrentRoom is null.");
                enemies.Clear(); projectiles.Clear(); return;
            }

            Room newCurrentRoom = currentLevel.CurrentRoom;
            Debug.WriteLine($"[Form1] LoadCurrentRoomObjects - Entering Room: {newCurrentRoom.GridPosition} ({newCurrentRoom.Type}), IsCleared: {newCurrentRoom.IsCleared}, PuzzleSolved: {newCurrentRoom.IsPuzzleSolved}, SurvivalCompleted: {newCurrentRoom.IsSurvivalCompleted}");

            if (activePuzzleRoom != null && activePuzzleRoom.IsPuzzleActive && activePuzzleRoom != newCurrentRoom)
            {
                activePuzzleRoom.IsPuzzleActive = false;
                // isInPuzzleInputMode는 새 방의 상태에 따라 아래에서 결정됨
                Debug.WriteLine($"[Form1] Deactivated puzzle in previous room {activePuzzleRoom.GridPosition} upon leaving.");
            }
            if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive && activeSurvivalRoom != newCurrentRoom)
            {
                activeSurvivalRoom.IsSurvivalActive = false;
                Debug.WriteLine($"[Form1] Deactivated survival in previous room {activeSurvivalRoom.GridPosition} upon leaving.");
            }

            activePuzzleRoom = null; // 일단 null로 초기화 후, 현재 방이 퍼즐방이면 다시 할당
            activeSurvivalRoom = null; // 일단 null로 초기화 후, 현재 방이 생존방이면 다시 할당
            isInPuzzleInputMode = false; // 기본적으로 퍼즐 입력 모드 해제

            enemies.Clear();
            projectiles.Clear();
            currentPlayerPuzzleInput = "";

            if (newCurrentRoom.Type == RoomType.Puzzle && !newCurrentRoom.IsPuzzleSolved)
            {
                newCurrentRoom.IsPuzzleActive = true;
                isInPuzzleInputMode = true;
                currentPuzzleTimeLeft = newCurrentRoom.PuzzleTimeLimit;
                activePuzzleRoom = newCurrentRoom;
                Debug.WriteLine($"[Form1] PuzzleRoom {newCurrentRoom.GridPosition} - Puzzle Activated. Time: {currentPuzzleTimeLeft}s. Q: {newCurrentRoom.PuzzleQuestion}");
            }
            else if (newCurrentRoom.Type == RoomType.Survival && !newCurrentRoom.IsSurvivalCompleted && !newCurrentRoom.IsCleared)
            {
                newCurrentRoom.IsSurvivalActive = true;
                // isInPuzzleInputMode = false; // 이미 위에서 false로 설정됨
                currentSurvivalTimeLeft = newCurrentRoom.SurvivalTimeDuration;
                activeSurvivalRoom = newCurrentRoom;
                Debug.WriteLine($"[Form1] SurvivalRoom {newCurrentRoom.GridPosition} - Survival Activated. Duration: {currentSurvivalTimeLeft}s");
            }

            if (!newCurrentRoom.IsCleared)
            {
                bool shouldSpawnEnemies = true;
                if (newCurrentRoom.Type == RoomType.Puzzle && newCurrentRoom.IsPuzzleSolved) shouldSpawnEnemies = false;
                if (newCurrentRoom.Type == RoomType.Survival && newCurrentRoom.IsSurvivalCompleted) shouldSpawnEnemies = false;

                if (shouldSpawnEnemies)
                {
                    for (int i = 0; i < newCurrentRoom.EnemyTypesToSpawn.Count; i++)
                    {
                        Type enemyType = newCurrentRoom.EnemyTypesToSpawn[i];
                        PointF spawnPos = newCurrentRoom.EnemySpawnPositions[i];
                        if (enemyType == typeof(BasicEnemy)) enemies.Add(new BasicEnemy(spawnPos));
                    }
                }
            }
            Debug.WriteLine($"[Form1] LoadCurrentRoomObjects - Finished. Enemies: {enemies.Count}. PuzzleInputMode: {isInPuzzleInputMode}. ActivePuzzleRoom: {activePuzzleRoom?.GridPosition}. ActiveSurvivalRoom: {activeSurvivalRoom?.GridPosition}");
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                float currentTime = (float)gameStopwatch.Elapsed.TotalSeconds;
                float deltaTime = currentTime - lastFrameTime;
                lastFrameTime = currentTime;
                deltaTime = Math.Min(deltaTime, 0.1f);

                if (shootCooldownTimer > 0f) shootCooldownTimer -= deltaTime;

                HandleInput(deltaTime);
                UpdateGameObjects(deltaTime);
                CheckCollisions();
                CleanupObjects();
                this.Invalidate();
            }
            catch (Exception ex)
            {
                gameTimer?.Stop();
                Debug.WriteLine($"[Form1] GameTimer_Tick - CRITICAL EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"게임 루프 중 심각한 오류 발생: {ex.Message}\n게임을 종료합니다.", "런타임 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SafeApplicationExit();
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isInPuzzleInputMode && activePuzzleRoom != null && activePuzzleRoom.IsPuzzleActive &&
                !activePuzzleRoom.IsPuzzleSolved && player != null && player.CurrentHealth > 0)
            {
                bool inputProcessedThisKey = true;
                if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
                {
                    currentPlayerPuzzleInput += (e.KeyCode - Keys.D0).ToString();
                }
                else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
                {
                    currentPlayerPuzzleInput += (e.KeyCode - Keys.NumPad0).ToString();
                }
                else if (e.KeyCode == Keys.Back && currentPlayerPuzzleInput.Length > 0)
                {
                    currentPlayerPuzzleInput = currentPlayerPuzzleInput.Substring(0, currentPlayerPuzzleInput.Length - 1);
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    Debug.WriteLine($"[Form1] Puzzle Answer Submitted: '{currentPlayerPuzzleInput}' for Q: '{activePuzzleRoom.PuzzleQuestion}' (Expected: '{activePuzzleRoom.PuzzleAnswer}')");
                    if (currentPlayerPuzzleInput == activePuzzleRoom.PuzzleAnswer)
                    {
                        activePuzzleRoom.IsPuzzleSolved = true;
                        activePuzzleRoom.IsPuzzleActive = false;
                        isInPuzzleInputMode = false;
                        Debug.WriteLine($"[Form1] Puzzle SOLVED for room {activePuzzleRoom.GridPosition}!");
                        activePuzzleRoom = null; // 더 이상 활성 퍼즐방 아님

                        if (enemies.Any())
                        {
                            Debug.WriteLine($"[Form1] Killing all {enemies.Count} enemies due to puzzle solve.");
                            for (int i = enemies.Count - 1; i >= 0; i--) enemies.RemoveAt(i);
                        }
                        currentLevel.CurrentRoom.ClearRoom(); // IsCleared = true 설정
                    }
                    else
                    {
                        Debug.WriteLine($"[Form1] Puzzle WRONG! Player takes 1 damage.");
                        player?.TakeDamage(1);
                        currentPlayerPuzzleInput = "";
                    }
                }
                else
                {
                    inputProcessedThisKey = false; // 퍼즐 관련 키가 아니었음
                }

                if (inputProcessedThisKey)
                {
                    this.Invalidate();
                    e.Handled = true;
                    e.SuppressKeyPress = true; // 숫자 외 문자 입력 방지 등
                    return;
                }
            }

            if (!pressedKeys.Contains(e.KeyCode))
            {
                pressedKeys.Add(e.KeyCode);
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
        }

        private void HandleInput(float deltaTime)
        {
            if (player == null || player.CurrentHealth <= 0)
            {
                if (player != null) player.Velocity = PointF.Empty;
                return;
            }

            if (isInPuzzleInputMode)
            {
                player.Velocity = PointF.Empty;
                return;
            }

            PointF moveDirection = PointF.Empty;
            if (pressedKeys.Contains(Keys.W)) moveDirection.Y -= 1;
            if (pressedKeys.Contains(Keys.S)) moveDirection.Y += 1;
            if (pressedKeys.Contains(Keys.A)) moveDirection.X -= 1;
            if (pressedKeys.Contains(Keys.D)) moveDirection.X += 1;
            player.Velocity = !moveDirection.IsEmpty ? Normalize(moveDirection, player.Speed) : PointF.Empty;

            bool canAttack = true;
            if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive && !activeSurvivalRoom.IsSurvivalCompleted)
            {
                canAttack = false;
            }

            if (canAttack)
            {
                PointF shootDirection = PointF.Empty;
                if (pressedKeys.Contains(Keys.Up)) shootDirection.Y -= 1;
                if (pressedKeys.Contains(Keys.Down)) shootDirection.Y += 1;
                if (pressedKeys.Contains(Keys.Left)) shootDirection.X -= 1;
                if (pressedKeys.Contains(Keys.Right)) shootDirection.X += 1;
                if (!shootDirection.IsEmpty && shootCooldownTimer <= 0f)
                {
                    Projectile p = player.Shoot(shootDirection);
                    if (p != null) { projectiles.Add(p); shootCooldownTimer = SHOOT_COOLDOWN; }
                }
            }
        }

        private void UpdateGameObjects(float deltaTime)
        {
            player?.Update(deltaTime);
            CheckPlayerRoomTransition();

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i].IsAlive) enemies[i].UpdateEnemy(deltaTime, player.Position);
                else enemies.RemoveAt(i);
            }
            for (int i = projectiles.Count - 1; i >= 0; i--) projectiles[i].Update(deltaTime);

            if (activePuzzleRoom != null && activePuzzleRoom.IsPuzzleActive && !activePuzzleRoom.IsPuzzleSolved && player.CurrentHealth > 0)
            {
                currentPuzzleTimeLeft -= deltaTime;
                if (currentPuzzleTimeLeft <= 0)
                {
                    currentPuzzleTimeLeft = 0;
                    Debug.WriteLine($"[Form1] Puzzle TIMEOUT for room {activePuzzleRoom.GridPosition}! Player dies.");
                    player?.TakeDamage(player.MaxHealth * 10);

                    activePuzzleRoom.IsPuzzleActive = false;
                    isInPuzzleInputMode = false;
                    // activePuzzleRoom = null; // 방을 나가야 null로 설정됨, 여기서는 유지하여 UI가 타임오버 상태 표시 가능
                }
            }

            if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive && !activeSurvivalRoom.IsSurvivalCompleted && player.CurrentHealth > 0)
            {
                currentSurvivalTimeLeft -= deltaTime;
                if (currentSurvivalTimeLeft <= 0)
                {
                    currentSurvivalTimeLeft = 0;
                    Debug.WriteLine($"[Form1] Survival COMPLETED for room {activeSurvivalRoom.GridPosition}! All enemies die.");

                    activeSurvivalRoom.IsSurvivalCompleted = true;
                    activeSurvivalRoom.IsSurvivalActive = false;

                    if (enemies.Any())
                    {
                        Debug.WriteLine($"[Form1] Killing all {enemies.Count} enemies after survival.");
                        for (int i = enemies.Count - 1; i >= 0; i--) enemies.RemoveAt(i);
                    }
                    activeSurvivalRoom.ClearRoom();
                    activeSurvivalRoom = null;
                }
            }

            if (currentLevel?.CurrentRoom != null && !currentLevel.CurrentRoom.IsCleared && player?.CurrentHealth > 0)
            {
                bool combatRoomNeedsClearing = currentLevel.CurrentRoom.Type == RoomType.Normal ||
                                               currentLevel.CurrentRoom.Type == RoomType.Boss ||
                                               currentLevel.CurrentRoom.Type == RoomType.MiniBoss;
                if (combatRoomNeedsClearing &&
                    currentLevel.CurrentRoom.EnemyTypesToSpawn.Count > 0 && enemies.Count == 0)
                {
                    currentLevel.CurrentRoom.ClearRoom();
                }
            }
        }

        private void CheckPlayerRoomTransition()
        {
            if (player == null || currentLevel == null || currentLevel.CurrentRoom == null ||
                isInPuzzleInputMode || // 퍼즐 입력 중 방 이동 X
               (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive && !activeSurvivalRoom.IsSurvivalCompleted && player.CurrentHealth > 0)) // 생존 미완료 시 방 이동 X (플레이어 생존 시)
            {
                return;
            }

            RectangleF playerBounds = player.Bounds;
            Room currentRoomLogic = currentLevel.CurrentRoom;
            bool moved = false;
            float doorW = 50, doorH = 50, doorInteractionThreshold = 5;
            string attemptedDirection = "NONE";

            RectangleF topDoorRect = new RectangleF(ClientRectangle.Width / 2f - doorW / 2f, 0, doorW, doorH);
            RectangleF bottomDoorRect = new RectangleF(ClientRectangle.Width / 2f - doorW / 2f, ClientRectangle.Height - doorH, doorW, doorH);
            RectangleF leftDoorRect = new RectangleF(0, ClientRectangle.Height / 2f - doorH / 2f, doorW, doorH);
            RectangleF rightDoorRect = new RectangleF(ClientRectangle.Width - doorW, ClientRectangle.Height / 2f - doorH / 2f, doorW, doorH);

            if (playerBounds.Top < doorInteractionThreshold && playerBounds.IntersectsWith(topDoorRect))
            {
                attemptedDirection = "TOP";
                if (currentRoomLogic.HasTopDoor && currentLevel.TryMoveToRoom(currentRoomLogic.GridPosition, 0, -1, player)) moved = true;
            }
            else if (playerBounds.Bottom > ClientRectangle.Height - doorInteractionThreshold && playerBounds.IntersectsWith(bottomDoorRect))
            {
                attemptedDirection = "BOTTOM";
                if (currentRoomLogic.HasBottomDoor && currentLevel.TryMoveToRoom(currentRoomLogic.GridPosition, 0, 1, player)) moved = true;
            }
            else if (playerBounds.Left < doorInteractionThreshold && playerBounds.IntersectsWith(leftDoorRect))
            {
                attemptedDirection = "LEFT";
                if (currentRoomLogic.HasLeftDoor && currentLevel.TryMoveToRoom(currentRoomLogic.GridPosition, -1, 0, player)) moved = true;
            }
            else if (playerBounds.Right > ClientRectangle.Width - doorInteractionThreshold && playerBounds.IntersectsWith(rightDoorRect))
            {
                attemptedDirection = "RIGHT";
                if (currentRoomLogic.HasRightDoor && currentLevel.TryMoveToRoom(currentRoomLogic.GridPosition, 1, 0, player)) moved = true;
            }

            if (moved)
            {
                Debug.WriteLine($"[Form1] Successfully moved via {attemptedDirection} door from {currentRoomLogic.GridPosition}. Loading new room objects.");
                LoadCurrentRoomObjects();
            }
            else
            {
                if (attemptedDirection != "NONE")
                {
                    PointF correctedPosition = player.Position;
                    switch (attemptedDirection)
                    {
                        case "TOP": correctedPosition.Y = doorInteractionThreshold; break;
                        case "BOTTOM": correctedPosition.Y = ClientRectangle.Height - doorInteractionThreshold - player.Size.Height; break;
                        case "LEFT": correctedPosition.X = doorInteractionThreshold; break;
                        case "RIGHT": correctedPosition.X = ClientRectangle.Width - doorInteractionThreshold - player.Size.Width; break;
                    }
                    player.SetPosition(correctedPosition);
                    player.Velocity = PointF.Empty;
                }
                else
                {
                    PointF currentPosition = player.Position;
                    float newX = Math.Max(0, Math.Min(currentPosition.X, ClientRectangle.Width - player.Size.Width));
                    float newY = Math.Max(0, Math.Min(currentPosition.Y, ClientRectangle.Height - player.Size.Height));
                    if (Math.Abs(newX - currentPosition.X) > 0.001f || Math.Abs(newY - currentPosition.Y) > 0.001f)
                    {
                        player.SetPosition(new PointF(newX, newY));
                    }
                }
            }
        }

        private void CheckCollisions()
        {
            if (player == null || player.CurrentHealth <= 0 || currentLevel == null || currentLevel.CurrentRoom == null) return;
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var p = projectiles[i]; if (!p.IsPlayerProjectile) continue;
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    var e = enemies[j]; if (!e.IsAlive) continue;
                    if (p.Bounds.IntersectsWith(e.Bounds)) { e.TakeDamage(p.Damage); projectiles.RemoveAt(i); break; }
                }
            }
            foreach (var e in enemies)
                if (e.IsAlive && e.Bounds.IntersectsWith(player.Bounds)) player.TakeDamage(e.CollisionDamage);
        }

        private void CleanupObjects()
        {
            projectiles.RemoveAll(p => p.ShouldBeRemoved(this.ClientRectangle));
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.FromArgb(30, 30, 30));
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            try
            {
                currentLevel?.DrawCurrentRoom(g, this.ClientRectangle);
                foreach (var enemy in enemies) enemy.Draw(g);
                foreach (var projectile in projectiles) projectile.Draw(g);
                player?.Draw(g);
                DrawUI(g);
            }
            catch (Exception ex) { Debug.WriteLine($"[Form1] Form1_Paint - EXCEPTION: {ex.Message}\n{ex.StackTrace}"); }
        }

        /// <summary>
        /// 게임의 사용자 인터페이스(UI) 요소들을 그립니다.
        /// (예: 플레이어 체력, 현재 방 정보, 미니맵, 특수방 UI, 게임 오버 화면 등)
        /// </summary>
        /// <param name="g">그리기에 사용될 Graphics 객체입니다.</param>
        private void DrawUI(Graphics g)
        {
            // UI에 사용할 기본 폰트 (화면 세로 크기에 비례하도록 설정)
            Font uiFont = new Font("Arial", ClientSize.Height / 50f, FontStyle.Bold);
            uiFont = new Font(uiFont.FontFamily, Math.Max(8f, uiFont.Size)); // 최소 폰트 크기 8pt 보장

            // 플레이어 정보 표시 (체력 등)
            if (player != null)
            {
                g.DrawString($"Health: {player.CurrentHealth} / {player.MaxHealth}", uiFont, Brushes.White, 10, 10);
                // 디버깅용 현재 방 정보 표시
                if (currentLevel?.CurrentRoom != null)
                {
                    string roomDebugInfo = $"Room: {currentLevel.CurrentRoom.GridPosition} ({currentLevel.CurrentRoom.Type})";
                    if (currentLevel.CurrentRoom.IsCleared) roomDebugInfo += " [C]";
                    if (currentLevel.CurrentRoom.Type == RoomType.Puzzle) roomDebugInfo += $" (PuzS: {currentLevel.CurrentRoom.IsPuzzleSolved})";
                    if (currentLevel.CurrentRoom.Type == RoomType.Survival) roomDebugInfo += $" (SurvC: {currentLevel.CurrentRoom.IsSurvivalCompleted})";

                    g.DrawString(roomDebugInfo, uiFont, Brushes.LightYellow, 10, ClientSize.Height - uiFont.GetHeight() - 5);
                }
            }

            // 퍼즐방 UI 표시 로직
            if (isInPuzzleInputMode && activePuzzleRoom != null && !activePuzzleRoom.IsPuzzleSolved && player != null && player.CurrentHealth > 0)
            {
                Font puzzleFont = new Font("Consolas", ClientSize.Height / 30f, FontStyle.Bold);
                Font inputFont = new Font("Consolas", ClientSize.Height / 28f, FontStyle.Bold);
                Font timerFont = new Font("Arial", ClientSize.Height / 35f, FontStyle.Italic | FontStyle.Bold);

                string question = activePuzzleRoom.PuzzleQuestion;
                string cursor = (gameStopwatch.ElapsedMilliseconds / 400 % 2 == 0) ? "_" : " "; // 깜빡이는 입력 커서 효과
                string currentAnswerDisplay = $"Input: {currentPlayerPuzzleInput}{cursor}";
                string timerDisplay = $"Time: {Math.Max(0, currentPuzzleTimeLeft):F1} s";
                if (currentPuzzleTimeLeft <= 0 && activePuzzleRoom.IsPuzzleActive) // 시간이 다 되었고 아직 활성 상태라면
                {
                    timerDisplay = "TIME OVER!";
                }

                // UI 박스 크기 및 위치 계산
                float boxPadding = ClientSize.Width * 0.015f;
                float textVMargin = ClientSize.Height * 0.01f;

                SizeF qSize = g.MeasureString(question, puzzleFont);
                SizeF iSize = g.MeasureString($"Input: {new string('0', Math.Max(currentPlayerPuzzleInput.Length, activePuzzleRoom.PuzzleAnswer.Length) + 2)}", inputFont); // 충분한 너비 확보
                SizeF tSize = g.MeasureString(timerDisplay, timerFont);

                float boxWidth = Math.Max(Math.Max(qSize.Width, iSize.Width), tSize.Width) + boxPadding * 2;
                boxWidth = Math.Min(boxWidth, ClientSize.Width * 0.9f); // 화면 너비의 90%를 넘지 않도록
                float boxHeight = qSize.Height + iSize.Height + tSize.Height + textVMargin * 2 + boxPadding * 2;

                float boxX = ClientSize.Width / 2f - boxWidth / 2f;
                float boxY = ClientSize.Height / 3.5f - boxHeight / 2f; // 화면 중앙보다 약간 위

                // UI 박스 배경 및 테두리
                g.FillRectangle(new SolidBrush(Color.FromArgb(220, 10, 25, 55)), boxX, boxY, boxWidth, boxHeight);
                g.DrawRectangle(Pens.LightSkyBlue, boxX, boxY, boxWidth, boxHeight);

                // 텍스트 그리기
                float currentYDraw = boxY + boxPadding;
                g.DrawString(question, puzzleFont, Brushes.White, boxX + boxPadding, currentYDraw);
                currentYDraw += qSize.Height + textVMargin;
                g.DrawString(currentAnswerDisplay, inputFont, Brushes.LimeGreen, boxX + boxPadding, currentYDraw);
                currentYDraw += iSize.Height + textVMargin;

                // 타이머 색상 결정 (Color 타입으로)
                Color timerColorForPuzzle;
                if (currentPuzzleTimeLeft <= 0)
                    timerColorForPuzzle = Color.DarkRed; // 타임 오버
                else if (currentPuzzleTimeLeft < 10.0f && (int)(gameStopwatch.Elapsed.TotalSeconds * 3.33) % 2 == 0) // 10초 미만 깜빡임
                    timerColorForPuzzle = Color.Red;
                else
                    timerColorForPuzzle = Color.OrangeRed; // 기본 타이머 색상

                g.DrawString(timerDisplay, timerFont, new SolidBrush(timerColorForPuzzle), boxX + boxPadding, currentYDraw);
            }
            // 생존방 UI 표시 로직
            else if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive && !activeSurvivalRoom.IsSurvivalCompleted && player != null && player.CurrentHealth > 0)
            {
                Font survivalFont = new Font("Impact", ClientSize.Height / 15f, FontStyle.Regular);
                Font survivalTimerFont = new Font("Consolas", ClientSize.Height / 20f, FontStyle.Bold);

                string surviveText = "SURVIVE!";
                string survivalTimerDisplay = $"{Math.Max(0, currentSurvivalTimeLeft):F1}";
                if (currentSurvivalTimeLeft <= 0 && activeSurvivalRoom.IsSurvivalActive) // 시간이 다 되었고 아직 활성 상태라면 (곧 완료 처리됨)
                {
                    // 실제 완료 처리는 UpdateGameObjects에서 일어나지만, UI는 즉시 반영될 수 있도록
                    // 여기서는 아직 "SURVIVED!"를 표시하지 않고 타이머가 0.0을 가리키도록 둠.
                    // IsSurvivalCompleted가 true가 되면 이 UI 블록 자체가 실행되지 않음.
                }

                SizeF surviveTextSize = g.MeasureString(surviveText, survivalFont);
                SizeF survivalTimerDisplaySize = g.MeasureString(survivalTimerDisplay, survivalTimerFont);

                // "SURVIVE!" 텍스트 위치 (화면 상단 중앙)
                float surviveX = ClientSize.Width / 2f; // TextRenderer는 중앙 정렬 지원
                float surviveY = ClientSize.Height * 0.1f;

                // 타이머 텍스트 위치 ("SURVIVE!" 아래)
                float timerX = ClientSize.Width / 2f;
                float timerY = surviveY + surviveTextSize.Height + ClientSize.Height * 0.01f;

                // TextFormatFlags.HorizontalCenter를 사용하여 텍스트 중앙 정렬
                TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsTranslateTransform;

                // "SURVIVE!" 텍스트 (외곽선 효과)
                TextRenderer.DrawText(g, surviveText, survivalFont, new Point((int)surviveX + 2, (int)surviveY + 2), Color.Black, flags);
                TextRenderer.DrawText(g, surviveText, survivalFont, new Point((int)surviveX, (int)surviveY), Color.OrangeRed, flags);

                // 생존 타이머 텍스트 (외곽선 효과 및 시간 부족 시 깜빡임)
                Color survivalTimerColor;
                if (currentSurvivalTimeLeft <= 0) // 아직 IsSurvivalCompleted가 false지만 시간이 0이 된 직후
                    survivalTimerColor = Color.Yellow; // 완료 직전 색상 (또는 다른 적절한 색)
                else if (currentSurvivalTimeLeft < 5.0f && (int)(gameStopwatch.Elapsed.TotalSeconds * 2.5) % 2 == 0) // 5초 미만 깜빡임
                    survivalTimerColor = Color.Red;
                else
                    survivalTimerColor = Color.White;

                TextRenderer.DrawText(g, survivalTimerDisplay, survivalTimerFont, new Point((int)timerX + 2, (int)timerY + 2), Color.Black, flags);
                TextRenderer.DrawText(g, survivalTimerDisplay, survivalTimerFont, new Point((int)timerX, (int)timerY), survivalTimerColor, flags);

                // "공격 불가!" 메시지
                if (player.CurrentHealth > 0 && currentSurvivalTimeLeft > 0)
                {
                    string noAttackMsg = "공격 불가!";
                    SizeF noAttackMsgSize = g.MeasureString(noAttackMsg, uiFont);
                    g.DrawString(noAttackMsg, uiFont, Brushes.Gold, ClientSize.Width / 2f - noAttackMsgSize.Width / 2f, ClientSize.Height * 0.9f);
                }
            }

            // 미니맵 그리기
            DrawMinimap(g);

            // 게임 오버 화면
            if (player != null && player.CurrentHealth <= 0)
            {
                Font gameOverFont = new Font("Impact", (float)ClientSize.Height / 7f, FontStyle.Bold);
                string gameOverText = "GAME OVER";
                SizeF gameOverSize = g.MeasureString(gameOverText, gameOverFont);
                // 반투명 검은 배경
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), 0, 0, ClientSize.Width, ClientSize.Height);
                // 그림자 효과
                g.DrawString(gameOverText, gameOverFont, Brushes.Black, ClientSize.Width / 2f - gameOverSize.Width / 2f + 5, ClientSize.Height / 2f - gameOverSize.Height / 2f + 5);
                // 메인 텍스트
                g.DrawString(gameOverText, gameOverFont, Brushes.Firebrick, ClientSize.Width / 2f - gameOverSize.Width / 2f, ClientSize.Height / 2f - gameOverSize.Height / 2f);
            }
        }

        private void DrawMinimap(Graphics g)
        {
            if (currentLevel == null || currentLevel.Rooms.Count == 0 || currentLevel.CurrentRoom == null) return;
            int minimapMargin = 15;
            int roomCellSize = (int)(Math.Min(ClientSize.Width, ClientSize.Height) / 60.0);
            roomCellSize = Math.Max(6, Math.Min(11, roomCellSize));
            int roomCellPadding = 2;

            HashSet<Room> roomsToDisplay = new HashSet<Room>();
            List<Room> discoveredRooms = currentLevel.Rooms.Values.Where(r => r.IsDiscovered).ToList();

            if (currentLevel.CurrentRoom.IsDiscovered) roomsToDisplay.Add(currentLevel.CurrentRoom);
            foreach (var room in discoveredRooms) roomsToDisplay.Add(room);

            foreach (var discRoom in discoveredRooms)
            {
                Point currentGridPos = discRoom.GridPosition;
                Point[] neighborOffsets = { new Point(0, -1), new Point(0, 1), new Point(-1, 0), new Point(1, 0) };
                bool[] doorsExist = { discRoom.HasTopDoor, discRoom.HasBottomDoor, discRoom.HasLeftDoor, discRoom.HasRightDoor };
                for (int i = 0; i < neighborOffsets.Length; i++)
                    if (doorsExist[i] && currentLevel.Rooms.TryGetValue(new Point(currentGridPos.X + neighborOffsets[i].X, currentGridPos.Y + neighborOffsets[i].Y), out Room adjRoom))
                        roomsToDisplay.Add(adjRoom);
            }

            if (!roomsToDisplay.Any()) return;

            int minX = roomsToDisplay.Min(r => r.GridPosition.X);
            int maxX = roomsToDisplay.Max(r => r.GridPosition.X);
            int minY = roomsToDisplay.Min(r => r.GridPosition.Y);
            int maxY = roomsToDisplay.Max(r => r.GridPosition.Y);

            int mapW = (maxX - minX + 1) * (roomCellSize + roomCellPadding) - roomCellPadding;
            int mapH = (maxY - minY + 1) * (roomCellSize + roomCellPadding) - roomCellPadding;
            mapW = Math.Max(mapW, roomCellSize); mapH = Math.Max(mapH, roomCellSize);

            float mapAreaX = ClientSize.Width - mapW - minimapMargin;
            float mapAreaY = minimapMargin;

            g.FillRectangle(new SolidBrush(Color.FromArgb(160, 10, 15, 35)), mapAreaX - roomCellPadding, mapAreaY - roomCellPadding, mapW + roomCellPadding * 2, mapH + roomCellPadding * 2);

            foreach (var room in roomsToDisplay)
            {
                float cellX = mapAreaX + (room.GridPosition.X - minX) * (roomCellSize + roomCellPadding);
                float cellY = mapAreaY + (room.GridPosition.Y - minY) * (roomCellSize + roomCellPadding);
                Brush roomBrush; Pen borderPen = Pens.DarkSlateGray;

                if (room.IsDiscovered)
                {
                    if (room.GridPosition == currentLevel.CurrentRoom.GridPosition) { roomBrush = Brushes.LimeGreen; borderPen = Pens.WhiteSmoke; }
                    else
                    {
                        switch (room.Type)
                        {
                            case RoomType.Start: roomBrush = Brushes.SkyBlue; break;
                            case RoomType.Boss: roomBrush = Brushes.IndianRed; break;
                            case RoomType.Treasure: roomBrush = Brushes.Gold; break;
                            case RoomType.Shop: roomBrush = Brushes.MediumPurple; break;
                            case RoomType.Secret: roomBrush = Brushes.LightGray; break;
                            case RoomType.MiniBoss: roomBrush = Brushes.OrangeRed; break;
                            case RoomType.Puzzle: roomBrush = Brushes.Turquoise; break;
                            case RoomType.Survival: roomBrush = Brushes.Coral; break;
                            default: roomBrush = Brushes.Gray; break;
                        }
                    }
                }
                else
                {
                    borderPen = Pens.SlateGray;
                    switch (room.Type)
                    {
                        case RoomType.Boss: roomBrush = Brushes.Maroon; break;
                        case RoomType.Treasure: roomBrush = Brushes.DarkGoldenrod; break;
                        case RoomType.Shop: roomBrush = Brushes.Indigo; break;
                        case RoomType.Secret: roomBrush = Brushes.DarkSlateGray; break;
                        case RoomType.MiniBoss: roomBrush = Brushes.DarkOrange; break;
                        case RoomType.Puzzle: roomBrush = Brushes.Teal; break;
                        case RoomType.Survival: roomBrush = Brushes.Firebrick; break;
                        default: roomBrush = Brushes.DimGray; break;
                    }
                }
                g.FillRectangle(roomBrush, cellX, cellY, roomCellSize, roomCellSize);
                g.DrawRectangle(borderPen, cellX, cellY, roomCellSize, roomCellSize);
            }
        }

        private void SafeApplicationExit()
        {
            if (gameTimer != null) { gameTimer.Stop(); gameTimer.Dispose(); }
            if (gameStopwatch != null) gameStopwatch.Stop();
            // 메시지 루프가 이미 종료되었거나 종료 중일 때 Application.Exit() 호출 시 문제 방지
            if (this.IsHandleCreated && !this.IsDisposed && Application.MessageLoop)
            {
                Application.Exit();
            }
            else if (!Application.MessageLoop) // 메시지 루프가 아예 시작 안 된 경우 등
            {
                Environment.Exit(1);
            }
        }

        // Normalize 메서드는 Form1 클래스 내에 있어야 합니다.
        private PointF Normalize(PointF vec, float magnitude)
        {
            float length = (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
            if (length > 0) return new PointF((vec.X / length) * magnitude, (vec.Y / length) * magnitude);
            return PointF.Empty;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void RegisterCharacter(PictureBox pic)
        {
            player.MainCharacter = pic;
        }

        public void LoadCharacterTo(UserControl targetUC)
        {
            if (player.MainCharacter == null) return;

            // 기존 부모에서 제거
            if (player.MainCharacter.Parent != null)
                player.MainCharacter.Parent.Controls.Remove(player.MainCharacter);

            player.MainCharacter.SizeMode = PictureBoxSizeMode.Zoom;    //  사이즈 모드 설정

            //  메인캐릭터 나타낼 화면 위치 
            int width = (int)player.Size.Width;    //  픽쳐박스 폭
            int height = (int)player.Size.Height;   //  픽쳐박스 높이 

            player.MainCharacter.SetBounds(this.Width / 2 - width / 2, this.Height / 2 - height / 2, width, height);

            // 새로운 유저컨트롤에 추가
            targetUC.Controls.Add(player.MainCharacter);
            player.MainCharacter.BringToFront();

            
        }
    }
}