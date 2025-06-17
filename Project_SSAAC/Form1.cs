// 파일 위치: Project_SSAAC/Form1.cs
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
    public enum Direction { Up, Down, Left, Right }

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
        private const float SHOOT_COOLDOWN = 0.3f; // 초 단위
        private Level currentLevel;
        private LevelGenerator levelGenerator;

        private string currentPlayerPuzzleInput = "";
        private float currentPuzzleTimeLeft = 0f;
        private bool isInPuzzleInputMode = false;
        private Room activePuzzleRoom = null;

        private float currentSurvivalTimeLeft = 0f;
        private Room activeSurvivalRoom = null;

        private Keys moveUpKey = Keys.W;
        private Keys moveDownKey = Keys.S;
        private Keys moveLeftKey = Keys.A;
        private Keys moveRightKey = Keys.D;

        private Keys shootUpKey = Keys.Up;
        private Keys shootDownKey = Keys.Down;
        private Keys shootLeftKey = Keys.Left;
        private Keys shootRightKey = Keys.Right;

        public Form1()
        {
            Debug.WriteLine("[Form1] Constructor - Start");
            InitializeComponent();


            this.DoubleBuffered = true;

            Debug.WriteLine("[Form1] Constructor - DoubleBuffered set");

            InitializeGame();
            Debug.WriteLine("[Form1] Constructor - InitializeGame finished");
            InitializeTimer();
            Debug.WriteLine("[Form1] Constructor - InitializeTimer finished. Constructor End.");

            LoadControl(new HomeControl(this, player));
        }

        public void StartGame()
        {
            panelMain.Visible = false;
            this.Focus();
        }

        public void LoadControl(UserControl control)
        {
            panelMain.Visible = true;
            panelMain.Controls.Clear();
            control.Dock = DockStyle.Fill;
            panelMain.Controls.Add(control);
        }

        private void InitializeGame()
        {
            Debug.WriteLine("[Form1] InitializeGame - Start");
            this.ClientSize = new Size(1024, 576);
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
                    MessageBox.Show($"레벨 생성 실패 또는 유효하지 않은 레벨 상태.\n{errorDetail}\n게임을 종료합니다.", "초기화 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"게임 초기화 중 예외 발생: {ex.Message}\n{ex.StackTrace}\n게임을 종료합니다.", "초기화 심각한 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            Debug.WriteLine($"[Form1] InitializeTimer - Timer started. Interval: {gameTimer.Interval}ms, Enabled: {gameTimer.Enabled}");
        }

        private void LoadCurrentRoomObjects()
        {
            if (currentLevel == null || currentLevel.CurrentRoom == null)
            {
                Debug.WriteLine("[Form1] LoadCurrentRoomObjects - CRITICAL: currentLevel or CurrentRoom is null. Cannot load objects.");
                enemies.Clear(); projectiles.Clear(); return;
            }

            Room newCurrentRoom = currentLevel.CurrentRoom;
            Debug.WriteLine($"[Form1] LoadCurrentRoomObjects - Entering Room: {newCurrentRoom.GridPosition} ({newCurrentRoom.Type}), IsCleared: {newCurrentRoom.IsCleared}, PuzzleSolved: {newCurrentRoom.IsPuzzleSolved}, SurvivalCompleted: {newCurrentRoom.IsSurvivalCompleted}");

            if (activePuzzleRoom != null && activePuzzleRoom.IsPuzzleActive && activePuzzleRoom != newCurrentRoom)
            {
                activePuzzleRoom.IsPuzzleActive = false;
                Debug.WriteLine($"[Form1] Deactivated puzzle in previous room {activePuzzleRoom.GridPosition} upon leaving.");
            }
            if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive && activeSurvivalRoom != newCurrentRoom)
            {
                activeSurvivalRoom.IsSurvivalActive = false;
                Debug.WriteLine($"[Form1] Deactivated survival in previous room {activeSurvivalRoom.GridPosition} upon leaving.");
            }

            activePuzzleRoom = null;
            activeSurvivalRoom = null;
            isInPuzzleInputMode = false;

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

                        // enemyType을 직접 비교하여 적절한 적 생성
                        if (enemyType == typeof(BasicEnemy)) enemies.Add(new BasicEnemy(spawnPos));
                        else if (enemyType == typeof(RangedEnemy)) enemies.Add(new RangedEnemy(spawnPos));
                        else if (enemyType == typeof(ChargerEnemy)) enemies.Add(new ChargerEnemy(spawnPos));
                    }
                }
            }
            Debug.WriteLine($"[Form1] LoadCurrentRoomObjects - Finished. Enemies in room: {enemies.Count}. PuzzleInputMode: {isInPuzzleInputMode}. ActivePuzzleRoom: {activePuzzleRoom?.GridPosition}. ActiveSurvivalRoom: {activeSurvivalRoom?.GridPosition}");
        }

        int animationTimerCounter = 0;

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                float currentTime = (float)gameStopwatch.Elapsed.TotalSeconds;
                float deltaTime = currentTime - lastFrameTime;
                lastFrameTime = currentTime;

                deltaTime = Math.Min(deltaTime, 0.1f);

                if (shootCooldownTimer > 0f) shootCooldownTimer -= deltaTime;

                if (player.Velocity.X == 0 && player.Velocity.Y == 0)
                {
                    player.IsRun = false;
                }
                else
                {
                    player.IsRun = true;

                    if (player.Velocity.X > 0) { player.facingRight = true; player.IsRun = true; }
                    else if (player.Velocity.X < 0) { player.facingRight = false; player.IsRun = true; }
                }

                animationTimerCounter += 16;
                if (animationTimerCounter >= 100)
                {
                    player.frameIndex = (player.frameIndex + 1) % 11;
                    enemies.ForEach(enemy => enemy.frameIndex = (enemy.frameIndex + 1) % 10);
                    animationTimerCounter = 0;
                }

                HandleInput(deltaTime);
                UpdateGameObjects(deltaTime);
                CheckPlayerRoomTransition();
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

                        if (enemies.Any())
                        {
                            Debug.WriteLine($"[Form1] Killing all {enemies.Count} enemies due to puzzle solve.");
                            for (int i = enemies.Count - 1; i >= 0; i--) enemies.RemoveAt(i);
                        }
                        currentLevel.CurrentRoom.ClearRoom();
                        activePuzzleRoom = null;
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
                    inputProcessedThisKey = false;
                }

                if (inputProcessedThisKey)
                {
                    this.Invalidate();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
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
            if (pressedKeys.Contains(moveUpKey)) moveDirection.Y -= 1;
            if (pressedKeys.Contains(moveDownKey)) moveDirection.Y += 1;
            if (pressedKeys.Contains(moveLeftKey)) moveDirection.X -= 1;
            if (pressedKeys.Contains(moveRightKey)) moveDirection.X += 1;

            player.Velocity = !moveDirection.IsEmpty ? Normalize(moveDirection, player.Speed) : PointF.Empty;

            bool canAttack = true;
            if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive &&
                !activeSurvivalRoom.IsSurvivalCompleted)
            {
                canAttack = false;
            }

            if (canAttack)
            {
                PointF shootDirection = PointF.Empty;
                if (pressedKeys.Contains(shootUpKey)) shootDirection.Y -= 1;
                if (pressedKeys.Contains(shootDownKey)) shootDirection.Y += 1;
                if (pressedKeys.Contains(shootLeftKey)) shootDirection.X -= 1;
                if (pressedKeys.Contains(shootRightKey)) shootDirection.X += 1;

                if (!shootDirection.IsEmpty && shootCooldownTimer <= 0f)
                {
                    Projectile p = player.Shoot(shootDirection);
                    if (p != null)
                    {
                        projectiles.Add(p);
                        shootCooldownTimer = SHOOT_COOLDOWN;
                    }
                }
            }
        }

        private void UpdateGameObjects(float deltaTime)
        {
            player?.Update(deltaTime);

            if (player != null && currentLevel?.CurrentRoom != null)
            {
                foreach (var obstacle in currentLevel.CurrentRoom.Obstacles)
                {
                    if (obstacle.BlocksMovement && player.Bounds.IntersectsWith(obstacle.Bounds))
                    {
                        RectangleF intersection = RectangleF.Intersect(player.Bounds, obstacle.Bounds);
                        PointF playerCenter = new PointF(player.Bounds.Left + player.Bounds.Width / 2, player.Bounds.Top + player.Bounds.Height / 2);
                        PointF obstacleCenter = new PointF(obstacle.Bounds.Left + obstacle.Bounds.Width / 2, obstacle.Bounds.Top + obstacle.Bounds.Height / 2);

                        float pushX = 0;
                        float pushY = 0;

                        if (intersection.Width < intersection.Height)
                        {
                            pushX = (playerCenter.X < obstacleCenter.X) ? -intersection.Width : intersection.Width;
                        }
                        else
                        {
                            pushY = (playerCenter.Y < obstacleCenter.Y) ? -intersection.Height : intersection.Height;
                        }

                        player.SetPosition(new PointF(player.Position.X + pushX, player.Position.Y + pushY));
                    }
                }
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i].IsAlive)
                {
                    var enemy = enemies[i];
                    // <<-- 수정: 적 업데이트 후 반환된 투사체 처리 -->>
                    Projectile newProjectile = enemy.UpdateEnemy(deltaTime, player.Position);
                    if (newProjectile != null)
                    {
                        projectiles.Add(newProjectile);
                    }

                    if (currentLevel?.CurrentRoom != null)
                    {
                        foreach (var obstacle in currentLevel.CurrentRoom.Obstacles)
                        {
                            if (obstacle.BlocksMovement && enemy.Bounds.IntersectsWith(obstacle.Bounds))
                            {
                                RectangleF intersection = RectangleF.Intersect(enemy.Bounds, obstacle.Bounds);
                                PointF enemyCenter = new PointF(enemy.Bounds.Left + enemy.Bounds.Width / 2, enemy.Bounds.Top + enemy.Bounds.Height / 2);
                                PointF obstacleCenter = new PointF(obstacle.Bounds.Left + obstacle.Bounds.Width / 2, obstacle.Bounds.Top + obstacle.Bounds.Height / 2);

                                float pushX = 0;
                                float pushY = 0;

                                if (intersection.Width < intersection.Height)
                                {
                                    pushX = (enemyCenter.X < obstacleCenter.X) ? -intersection.Width : intersection.Width;
                                }
                                else
                                {
                                    pushY = (enemyCenter.Y < obstacleCenter.Y) ? -intersection.Height : intersection.Height;
                                }

                                enemy.SetPosition(new PointF(enemy.Position.X + pushX, enemy.Position.Y + pushY));
                            }
                        }
                    }
                }
                else
                {
                    enemies.RemoveAt(i);
                }
            }

            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update(deltaTime);
            }

            if (activePuzzleRoom != null &&
                activePuzzleRoom.IsPuzzleActive &&
                !activePuzzleRoom.IsPuzzleSolved &&
                player != null && player.CurrentHealth > 0)
            {
                currentPuzzleTimeLeft -= deltaTime;

                if (currentPuzzleTimeLeft <= 0)
                {
                    currentPuzzleTimeLeft = 0;
                    Debug.WriteLine($"[Form1] Puzzle TIMEOUT for room {activePuzzleRoom.GridPosition}! Player instant death.");

                    player?.InstantKill();

                    activePuzzleRoom.IsPuzzleActive = false;
                    isInPuzzleInputMode = false;
                }
            }

            if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive
                && !activeSurvivalRoom.IsSurvivalCompleted && player != null && player.CurrentHealth > 0)
            {
                currentSurvivalTimeLeft -= deltaTime;
                if (currentSurvivalTimeLeft <= 0)
                {
                    currentSurvivalTimeLeft = 0;
                    Debug.WriteLine($"[Form1] Survival COMPLETED for room {activeSurvivalRoom.GridPosition}!" +
                        $" All enemies (if any) die.");

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

                if (combatRoomNeedsClearing && enemies.Count == 0)
                {
                    currentLevel.CurrentRoom.ClearRoom();
                    Debug.WriteLine($"[Form1] Combat Room {currentLevel.CurrentRoom.GridPosition} ({currentLevel.CurrentRoom.Type}) cleared because no enemies are present.");
                }
            }
        }

        private void CheckPlayerRoomTransition()
        {
            if (player == null || currentLevel == null || currentLevel.CurrentRoom == null || player.CurrentHealth <= 0)
            {
                return;
            }

            RectangleF playerBounds = player.Bounds;
            Room currentRoomLogic = currentLevel.CurrentRoom;
            bool moved = false;

            float doorInteractionThreshold = player.Size.Width / 2f;

            string attemptedDirection = "NONE";

            if (playerBounds.Top < doorInteractionThreshold)
            {
                attemptedDirection = "TOP";
                if (currentRoomLogic.HasTopDoor && currentLevel.TryMoveToRoom(currentRoomLogic.GridPosition, 0, -1, player)) moved = true;
            }
            else if (playerBounds.Bottom > ClientRectangle.Height - doorInteractionThreshold)
            {
                attemptedDirection = "BOTTOM";
                if (currentRoomLogic.HasBottomDoor && currentLevel.TryMoveToRoom(currentRoomLogic.GridPosition, 0, 1, player)) moved = true;
            }
            else if (playerBounds.Left < doorInteractionThreshold)
            {
                attemptedDirection = "LEFT";
                if (currentRoomLogic.HasLeftDoor && currentLevel.TryMoveToRoom(currentRoomLogic.GridPosition, -1, 0, player)) moved = true;
            }
            else if (playerBounds.Right > ClientRectangle.Width - doorInteractionThreshold)
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
                PointF currentPosition = player.Position;
                float newX = Math.Max(0, Math.Min(currentPosition.X, ClientRectangle.Width - player.Size.Width));
                float newY = Math.Max(0, Math.Min(currentPosition.Y, ClientRectangle.Height - player.Size.Height));

                if (attemptedDirection != "NONE" && !moved)
                {
                    switch (attemptedDirection)
                    {
                        case "TOP": newY = Math.Max(newY, doorInteractionThreshold); break;
                        case "BOTTOM": newY = Math.Min(newY, ClientRectangle.Height - player.Size.Height - doorInteractionThreshold); break;
                        case "LEFT": newX = Math.Max(newX, doorInteractionThreshold); break;
                        case "RIGHT": newX = Math.Min(newX, ClientRectangle.Width - player.Size.Width - doorInteractionThreshold); break;
                    }
                }

                if (Math.Abs(newX - currentPosition.X) > 0.001f || Math.Abs(newY - currentPosition.Y) > 0.001f)
                {
                    player.SetPosition(new PointF(newX, newY));
                }
            }
        }

        private void CheckCollisions()
        {
            if (player == null || player.CurrentHealth <= 0 || currentLevel == null || currentLevel.CurrentRoom == null) return;

            var obstacles = currentLevel.CurrentRoom.Obstacles;

            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var p = projectiles[i];

                // <<-- 수정: 투사체 주체에 따라 충돌 로직 분리 -->>
                if (p.IsPlayerProjectile)
                {
                    // 플레이어 투사체는 적과 충돌
                    bool projectileHit = false;
                    foreach (var obstacle in obstacles)
                    {
                        if (obstacle.BlocksProjectiles && p.Bounds.IntersectsWith(obstacle.Bounds))
                        {
                            projectiles.RemoveAt(i);
                            projectileHit = true;
                            break;
                        }
                    }
                    if (projectileHit) continue;

                    for (int j = enemies.Count - 1; j >= 0; j--)
                    {
                        var e = enemies[j];
                        if (e.IsAlive && p.Bounds.IntersectsWith(e.Bounds))
                        {
                            e.TakeDamage(p.Damage);
                            projectiles.RemoveAt(i);
                            break;
                        }
                    }
                }
                else
                {
                    // 적 투사체는 플레이어와 충돌
                    if (p.Bounds.IntersectsWith(player.Bounds))
                    {
                        player.TakeDamage(p.Damage);
                        projectiles.RemoveAt(i);
                    }
                }
            }

            foreach (var e in enemies)
            {
                if (e.IsAlive && e.Bounds.IntersectsWith(player.Bounds))
                {
                    player.TakeDamage(e.CollisionDamage);
                }
            }

            foreach (var obstacle in obstacles)
            {
                if (obstacle.CollisionDamage > 0 && player.Bounds.IntersectsWith(obstacle.Bounds))
                {
                    player.TakeDamage(obstacle.CollisionDamage);
                }
            }
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

                if (currentLevel?.CurrentRoom != null)
                {
                    foreach (var obstacle in currentLevel.CurrentRoom.Obstacles)
                    {
                        obstacle.Draw(g);
                    }
                }

                foreach (var enemy in enemies) enemy.Draw(g);
                foreach (var projectile in projectiles) projectile.Draw(g);
                player?.Draw(g);

                DrawUI(g);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Form1] Form1_Paint - EXCEPTION: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void DrawUI(Graphics g)
        {
            Font uiFont = new Font("Arial", Math.Max(8f, ClientSize.Height / 50f), FontStyle.Bold);

            if (player != null)
            {
                g.DrawString($"Health: {player.CurrentHealth} / {player.MaxHealth}", uiFont, Brushes.White, 10, 10);

                if (currentLevel?.CurrentRoom != null)
                {
                    string roomDebugInfo = $"Room: {currentLevel.CurrentRoom.GridPosition} ({currentLevel.CurrentRoom.Type})";
                    if (currentLevel.CurrentRoom.IsCleared) roomDebugInfo += " [C]";
                    if (currentLevel.CurrentRoom.Type == RoomType.Puzzle) roomDebugInfo += $" (PuzS: {currentLevel.CurrentRoom.IsPuzzleSolved}, Active: {currentLevel.CurrentRoom.IsPuzzleActive})";
                    if (currentLevel.CurrentRoom.Type == RoomType.Survival) roomDebugInfo += $" (SurvC: {currentLevel.CurrentRoom.IsSurvivalCompleted}, Active: {currentLevel.CurrentRoom.IsSurvivalActive})";
                    if (player.IsInvincible) roomDebugInfo += " [INV]";

                    g.DrawString(roomDebugInfo, uiFont, Brushes.LightYellow, 10, ClientSize.Height - uiFont.GetHeight() * 2 - 10);
                }
            }

            if (isInPuzzleInputMode && activePuzzleRoom != null && !activePuzzleRoom.IsPuzzleSolved && player != null && player.CurrentHealth > 0)
            {
                Font puzzleFont = new Font("Consolas", Math.Max(10f, ClientSize.Height / 30f), FontStyle.Bold);
                Font inputFont = new Font("Consolas", Math.Max(10f, ClientSize.Height / 28f), FontStyle.Bold);
                Font timerFont = new Font("Arial", Math.Max(9f, ClientSize.Height / 35f), FontStyle.Italic | FontStyle.Bold);

                string question = activePuzzleRoom.PuzzleQuestion;
                string cursor = (gameStopwatch.ElapsedMilliseconds / 400 % 2 == 0) ? "_" : " ";
                string currentAnswerDisplay = $"Input: {currentPlayerPuzzleInput}{cursor}";
                string timerDisplay = $"Time: {Math.Max(0, currentPuzzleTimeLeft):F1} s";
                if (currentPuzzleTimeLeft <= 0 && activePuzzleRoom.IsPuzzleActive) timerDisplay = "TIME OVER!";

                float boxPadding = ClientSize.Width * 0.015f;
                float textVMargin = ClientSize.Height * 0.01f;

                SizeF qSize = g.MeasureString(question, puzzleFont);
                SizeF iSize = g.MeasureString($"Input: {new string('0', Math.Max(currentPlayerPuzzleInput.Length, activePuzzleRoom.PuzzleAnswer?.Length ?? 0) + 2)}", inputFont);
                SizeF tSize = g.MeasureString(timerDisplay, timerFont);

                float boxWidth = Math.Max(Math.Max(qSize.Width, iSize.Width), tSize.Width) + boxPadding * 2;
                boxWidth = Math.Min(boxWidth, ClientSize.Width * 0.9f);
                float boxHeight = qSize.Height + iSize.Height + tSize.Height + textVMargin * 2 + boxPadding * 2;

                float boxX = ClientSize.Width / 2f - boxWidth / 2f;
                float boxY = ClientSize.Height / 3.5f - boxHeight / 2f;

                g.FillRectangle(new SolidBrush(Color.FromArgb(220, 10, 25, 55)), boxX, boxY, boxWidth, boxHeight);
                g.DrawRectangle(Pens.LightSkyBlue, boxX, boxY, boxWidth, boxHeight);

                float currentYDraw = boxY + boxPadding;
                g.DrawString(question, puzzleFont, Brushes.White, boxX + boxPadding, currentYDraw);
                currentYDraw += qSize.Height + textVMargin;
                g.DrawString(currentAnswerDisplay, inputFont, Brushes.LimeGreen, boxX + boxPadding, currentYDraw);
                currentYDraw += iSize.Height + textVMargin;

                Brush timerBrush = (currentPuzzleTimeLeft <= 0) ? Brushes.DarkRed : (currentPuzzleTimeLeft < 10.0f && (int)(gameStopwatch.Elapsed.TotalSeconds * 2) % 2 == 0) ? Brushes.Red : Brushes.OrangeRed;
                g.DrawString(timerDisplay, timerFont, timerBrush, boxX + boxPadding, currentYDraw);
            }
            else if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive && !activeSurvivalRoom.IsSurvivalCompleted && player != null && player.CurrentHealth > 0)
            {
                Font survivalFont = new Font("Impact", Math.Max(12f, ClientSize.Height / 15f), FontStyle.Regular);
                Font survivalTimerFont = new Font("Consolas", Math.Max(11f, ClientSize.Height / 20f), FontStyle.Bold);

                string surviveText = "SURVIVE!";
                string survivalTimerDisplay = $"{Math.Max(0, currentSurvivalTimeLeft):F1}";

                SizeF surviveTextSize = g.MeasureString(surviveText, survivalFont);

                float surviveX = ClientSize.Width / 2f - surviveTextSize.Width / 2f;
                float surviveY = ClientSize.Height * 0.1f;

                float timerX = ClientSize.Width / 2f;
                float timerY = surviveY + surviveTextSize.Height + ClientSize.Height * 0.01f;

                TextRenderer.DrawText(g, surviveText, survivalFont, new Point((int)surviveX + 2, (int)surviveY + 2), Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, surviveText, survivalFont, new Point((int)surviveX, (int)surviveY), Color.OrangeRed, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

                Color survivalTimerColor;
                if (currentSurvivalTimeLeft <= 0) survivalTimerColor = Color.Yellow;
                else if (currentSurvivalTimeLeft < 5.0f && (int)(gameStopwatch.Elapsed.TotalSeconds * 2.5) % 2 == 0) survivalTimerColor = Color.Red;
                else survivalTimerColor = Color.White;

                TextRenderer.DrawText(g, survivalTimerDisplay, survivalTimerFont, new Point((int)timerX, (int)timerY), survivalTimerColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

                if (player.CurrentHealth > 0 && currentSurvivalTimeLeft > 0)
                {
                    string noAttackMsg = "공격 불가!";
                    SizeF noAttackMsgSize = g.MeasureString(noAttackMsg, uiFont);
                    g.DrawString(noAttackMsg, uiFont, Brushes.Gold, ClientSize.Width / 2f - noAttackMsgSize.Width / 2f, ClientSize.Height * 0.9f);
                }
            }

            DrawMinimap(g);

            if (player != null && player.CurrentHealth <= 0)
            {
                Font gameOverFont = new Font("Impact", Math.Max(20f, (float)ClientSize.Height / 7f), FontStyle.Bold);
                string gameOverText = "GAME OVER";
                SizeF gameOverSize = g.MeasureString(gameOverText, gameOverFont);

                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), 0, 0, ClientSize.Width, ClientSize.Height);
                g.DrawString(gameOverText, gameOverFont, Brushes.Black, ClientSize.Width / 2f - gameOverSize.Width / 2f + 5, ClientSize.Height / 2f - gameOverSize.Height / 2f + 5);
                g.DrawString(gameOverText, gameOverFont, Brushes.Firebrick, ClientSize.Width / 2f - gameOverSize.Width / 2f, ClientSize.Height / 2f - gameOverSize.Height / 2f);
            }
        }

        private void DrawMinimap(Graphics g)
        {
            if (currentLevel == null || currentLevel.Rooms.Count == 0 || currentLevel.CurrentRoom == null) return;

            int minimapMargin = 15;
            int roomCellSize = (int)(Math.Min(ClientSize.Width, ClientSize.Height) / 55.0);
            roomCellSize = Math.Max(5, Math.Min(12, roomCellSize));
            int roomCellPadding = 2;

            HashSet<Room> roomsToDisplay = new HashSet<Room>();
            List<Room> discoveredRooms = currentLevel.Rooms.Values.Where(r => r.IsDiscovered).ToList();

            if (currentLevel.CurrentRoom.IsDiscovered) roomsToDisplay.Add(currentLevel.CurrentRoom);

            foreach (var discRoom in discoveredRooms)
            {
                roomsToDisplay.Add(discRoom);
                Point currentGridPos = discRoom.GridPosition;
                Point[] neighborOffsets = { new Point(0, -1), new Point(0, 1), new Point(-1, 0), new Point(1, 0) };
                bool[] doorsExist = { discRoom.HasTopDoor, discRoom.HasBottomDoor, discRoom.HasLeftDoor, discRoom.HasRightDoor };

                for (int i = 0; i < neighborOffsets.Length; i++)
                {
                    if (doorsExist[i])
                    {
                        Point adjGridPos = new Point(currentGridPos.X + neighborOffsets[i].X, currentGridPos.Y + neighborOffsets[i].Y);
                        if (currentLevel.Rooms.TryGetValue(adjGridPos, out Room adjacentRoom))
                        {
                            roomsToDisplay.Add(adjacentRoom);
                        }
                    }
                }
            }

            if (!roomsToDisplay.Any()) return;

            int minX = roomsToDisplay.Min(r => r.GridPosition.X);
            int maxX = roomsToDisplay.Max(r => r.GridPosition.X);
            int minY = roomsToDisplay.Min(r => r.GridPosition.Y);
            int maxY = roomsToDisplay.Max(r => r.GridPosition.Y);

            int mapGridWidth = (maxX - minX + 1);
            int mapGridHeight = (maxY - minY + 1);

            int mapPixelWidth = mapGridWidth * (roomCellSize + roomCellPadding) - roomCellPadding;
            int mapPixelHeight = mapGridHeight * (roomCellSize + roomCellPadding) - roomCellPadding;
            mapPixelWidth = Math.Max(mapPixelWidth, roomCellSize);
            mapPixelHeight = Math.Max(mapPixelHeight, roomCellSize);

            float mapAreaX = ClientSize.Width - mapPixelWidth - minimapMargin;
            float mapAreaY = minimapMargin;

            g.FillRectangle(new SolidBrush(Color.FromArgb(160, 10, 15, 35)), mapAreaX - roomCellPadding, mapAreaY - roomCellPadding, mapPixelWidth + roomCellPadding * 2, mapPixelHeight + roomCellPadding * 2);

            foreach (var room in roomsToDisplay)
            {
                float cellX = mapAreaX + (room.GridPosition.X - minX) * (roomCellSize + roomCellPadding);
                float cellY = mapAreaY + (room.GridPosition.Y - minY) * (roomCellSize + roomCellPadding);
                Brush roomBrush;
                Pen borderPen = Pens.DarkSlateGray;

                if (room.IsDiscovered)
                {
                    if (room.GridPosition == currentLevel.CurrentRoom.GridPosition)
                    {
                        roomBrush = Brushes.LimeGreen;
                        borderPen = Pens.WhiteSmoke;
                    }
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
                        case RoomType.Boss:
                            roomBrush = new SolidBrush(Color.FromArgb(100, Color.IndianRed));
                            break;
                        case RoomType.Treasure:
                            roomBrush = new SolidBrush(Color.FromArgb(100, Color.Gold));
                            break;
                        case RoomType.Shop:
                            roomBrush = new SolidBrush(Color.FromArgb(100, Color.MediumPurple));
                            break;
                        case RoomType.Secret:
                            roomBrush = new SolidBrush(Color.FromArgb(50, Color.DarkSlateGray));
                            break;
                        case RoomType.MiniBoss:
                            roomBrush = new SolidBrush(Color.FromArgb(100, Color.OrangeRed));
                            break;
                        case RoomType.Puzzle:
                            roomBrush = new SolidBrush(Color.FromArgb(100, Color.Turquoise));
                            break;
                        case RoomType.Survival:
                            roomBrush = new SolidBrush(Color.FromArgb(100, Color.Coral));
                            break;
                        default:
                            roomBrush = new SolidBrush(Color.FromArgb(100, Color.DimGray));
                            break;
                    }
                }
                g.FillRectangle(roomBrush, cellX, cellY, roomCellSize, roomCellSize);
                g.DrawRectangle(borderPen, cellX, cellY, roomCellSize, roomCellSize);
            }
        }

        private void SafeApplicationExit()
        {
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Dispose();
                gameTimer = null;
            }
            if (gameStopwatch != null)
            {
                gameStopwatch.Stop();
                gameStopwatch = null;
            }

            if (this.IsHandleCreated && !this.IsDisposed)
            {
                if (Application.MessageLoop)
                {
                    Application.Exit();
                }
                else
                {
                    Environment.Exit(1);
                }
            }
            else
            {
                Environment.Exit(1);
            }
        }

        private PointF Normalize(PointF vec, float magnitude)
        {
            float length = (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
            if (length > 0)
            {
                return new PointF((vec.X / length) * magnitude, (vec.Y / length) * magnitude);
            }
            return PointF.Empty;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void SetMoveKey(Direction dir, Keys key)
        {
            switch (dir)
            {
                case Direction.Up: moveUpKey = key; break;
                case Direction.Down: moveDownKey = key; break;
                case Direction.Left: moveLeftKey = key; break;
                case Direction.Right: moveRightKey = key; break;
            }
        }

        public void SetShootKey(Direction dir, Keys key)
        {
            switch (dir)
            {
                case Direction.Up: shootUpKey = key; break;
                case Direction.Down: shootDownKey = key; break;
                case Direction.Left: shootLeftKey = key; break;
                case Direction.Right: shootRightKey = key; break;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Enter:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }
    }
}