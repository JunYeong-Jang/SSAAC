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
                currentLevel = levelGenerator.GenerateLevel(numberOfRooms: 7); // 예시로 7개 방 생성
                Debug.WriteLine($"[Form1] InitializeGame - Level generated. Rooms: {currentLevel?.Rooms?.Count}, CurrentRoom: {currentLevel?.CurrentRoom?.GridPosition}");


                if (currentLevel == null || currentLevel.Rooms.Count == 0 || currentLevel.CurrentRoom == null)
                {
                    string errorDetail = $"Level: {(currentLevel == null ? "null" : "OK")}, Rooms.Count: {(currentLevel?.Rooms == null ? "N/A" : currentLevel.Rooms.Count.ToString())}, CurrentRoom: {(currentLevel?.CurrentRoom == null ? "null" : "OK")}";
                    MessageBox.Show($"레벨 생성 실패 또는 유효하지 않은 레벨 상태.\n{errorDetail}\n게임을 종료합니다.", "초기화 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SafeApplicationExit(); return;
                }

                // 플레이어 생성 및 시작 위치 설정 (현재 방의 중앙)
                SizeF playerSize = new SizeF(32, 32); // Player.PlayerDefaultSize와 일치
                player = new Player(new PointF(
                    this.ClientSize.Width / 2f - playerSize.Width / 2f,
                    this.ClientSize.Height / 2f - playerSize.Height / 2f
                ));
                Debug.WriteLine("[Form1] InitializeGame - Player created");

                LoadCurrentRoomObjects(); // 현재 방의 적 로드 및 특수방 로직 초기화
            }
            catch (Exception ex)
            {
                MessageBox.Show($"게임 초기화 중 예외 발생: {ex.Message}\n{ex.StackTrace}\n게임을 종료합니다.", "초기화 심각한 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SafeApplicationExit(); return;
            }

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            // this.Paint += Form1_Paint; // Designer.cs에서 이미 연결됨
            Debug.WriteLine("[Form1] InitializeGame - End");
        }

        private void InitializeTimer()
        {
            Debug.WriteLine("[Form1] InitializeTimer - Start");
            gameTimer = new Timer { Interval = 16 }; // 약 62.5 FPS
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

            // 이전 방이 특수 방이었다면 관련 상태 초기화
            if (activePuzzleRoom != null && activePuzzleRoom.IsPuzzleActive && activePuzzleRoom != newCurrentRoom)
            {
                activePuzzleRoom.IsPuzzleActive = false; // 이전 퍼즐방 비활성화
                Debug.WriteLine($"[Form1] Deactivated puzzle in previous room {activePuzzleRoom.GridPosition} upon leaving.");
            }
            if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive && activeSurvivalRoom != newCurrentRoom)
            {
                activeSurvivalRoom.IsSurvivalActive = false; // 이전 생존방 비활성화
                Debug.WriteLine($"[Form1] Deactivated survival in previous room {activeSurvivalRoom.GridPosition} upon leaving.");
            }

            // 현재 활성 특수방 상태 초기화
            activePuzzleRoom = null;
            activeSurvivalRoom = null;
            isInPuzzleInputMode = false; // 기본적으로 퍼즐 입력 모드 해제

            enemies.Clear();    // 이전 방의 적들 제거
            projectiles.Clear(); // 이전 방의 투사체들 제거
            currentPlayerPuzzleInput = ""; // 퍼즐 입력 초기화

            // 새 방이 퍼즐방이고 아직 해결되지 않았다면 퍼즐 활성화
            if (newCurrentRoom.Type == RoomType.Puzzle && !newCurrentRoom.IsPuzzleSolved)
            {
                newCurrentRoom.IsPuzzleActive = true;
                isInPuzzleInputMode = true;
                currentPuzzleTimeLeft = newCurrentRoom.PuzzleTimeLimit;
                activePuzzleRoom = newCurrentRoom;
                Debug.WriteLine($"[Form1] PuzzleRoom {newCurrentRoom.GridPosition} - Puzzle Activated. Time: {currentPuzzleTimeLeft}s. Q: {newCurrentRoom.PuzzleQuestion}");
            }
            // 새 방이 생존방이고 아직 완료/클리어되지 않았다면 생존 모드 활성화
            else if (newCurrentRoom.Type == RoomType.Survival && !newCurrentRoom.IsSurvivalCompleted && !newCurrentRoom.IsCleared)
            {
                newCurrentRoom.IsSurvivalActive = true;
                currentSurvivalTimeLeft = newCurrentRoom.SurvivalTimeDuration;
                activeSurvivalRoom = newCurrentRoom;
                Debug.WriteLine($"[Form1] SurvivalRoom {newCurrentRoom.GridPosition} - Survival Activated. Duration: {currentSurvivalTimeLeft}s");
            }

            // 방이 클리어되지 않았고, 특수 조건(퍼즐 해결, 생존 완료)에 해당하지 않으면 적 스폰
            if (!newCurrentRoom.IsCleared)
            {
                bool shouldSpawnEnemies = true;
                // 퍼즐방이지만 이미 퍼즐이 해결된 경우 적 스폰 X
                if (newCurrentRoom.Type == RoomType.Puzzle && newCurrentRoom.IsPuzzleSolved) shouldSpawnEnemies = false;
                // 생존방이지만 이미 생존이 완료된 경우 적 스폰 X
                if (newCurrentRoom.Type == RoomType.Survival && newCurrentRoom.IsSurvivalCompleted) shouldSpawnEnemies = false;

                if (shouldSpawnEnemies)
                {
                    for (int i = 0; i < newCurrentRoom.EnemyTypesToSpawn.Count; i++)
                    {
                        Type enemyType = newCurrentRoom.EnemyTypesToSpawn[i];
                        PointF spawnPos = newCurrentRoom.EnemySpawnPositions[i];

                        // 적 타입에 따라 다른 적 객체 생성 (현재는 BasicEnemy만)
                        if (enemyType == typeof(BasicEnemy)) enemies.Add(new BasicEnemy(spawnPos));
                        // else if (enemyType == typeof(AnotherEnemyType)) enemies.Add(new AnotherEnemyType(spawnPos));
                    }
                }
            }
            Debug.WriteLine($"[Form1] LoadCurrentRoomObjects - Finished. Enemies in room: {enemies.Count}. PuzzleInputMode: {isInPuzzleInputMode}. ActivePuzzleRoom: {activePuzzleRoom?.GridPosition}. ActiveSurvivalRoom: {activeSurvivalRoom?.GridPosition}");
        }


        private void GameTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                float currentTime = (float)gameStopwatch.Elapsed.TotalSeconds;
                float deltaTime = currentTime - lastFrameTime;
                lastFrameTime = currentTime;

                // deltaTime이 비정상적으로 커지는 것을 방지 (예: 디버깅 중단 후 재개 시)
                deltaTime = Math.Min(deltaTime, 0.1f); // 최대 deltaTime을 0.1초로 제한

                if (shootCooldownTimer > 0f) shootCooldownTimer -= deltaTime;

                HandleInput(deltaTime);         // 입력 처리
                UpdateGameObjects(deltaTime);   // 게임 객체 상태 업데이트
                CheckPlayerRoomTransition();    // 방 이동 확인 (UpdateGameObjects 이후에 호출하여 플레이어 위치 확정 후 처리)
                CheckCollisions();              // 충돌 감지 및 처리
                CleanupObjects();               // 불필요한 객체 제거

                this.Invalidate(); // 화면 다시 그리기를 요청 -> Form1_Paint 호출
            }
            catch (Exception ex)
            {
                gameTimer?.Stop(); // 예외 발생 시 타이머 정지
                Debug.WriteLine($"[Form1] GameTimer_Tick - CRITICAL EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"게임 루프 중 심각한 오류 발생: {ex.Message}\n게임을 종료합니다.", "런타임 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SafeApplicationExit();
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // 퍼즐 입력 모드 처리
            if (isInPuzzleInputMode && activePuzzleRoom != null && activePuzzleRoom.IsPuzzleActive &&
                !activePuzzleRoom.IsPuzzleSolved && player != null && player.CurrentHealth > 0)
            {
                bool inputProcessedThisKey = true; // 이 키 입력이 퍼즐 처리용이었는지 여부
                if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) // 숫자키 0-9
                {
                    currentPlayerPuzzleInput += (e.KeyCode - Keys.D0).ToString();
                }
                else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9) // 넘패드 0-9
                {
                    currentPlayerPuzzleInput += (e.KeyCode - Keys.NumPad0).ToString();
                }
                else if (e.KeyCode == Keys.Back && currentPlayerPuzzleInput.Length > 0) // 백스페이스
                {
                    currentPlayerPuzzleInput = currentPlayerPuzzleInput.Substring(0, currentPlayerPuzzleInput.Length - 1);
                }
                else if (e.KeyCode == Keys.Enter) // 엔터키 (정답 제출)
                {
                    Debug.WriteLine($"[Form1] Puzzle Answer Submitted: '{currentPlayerPuzzleInput}' for Q: '{activePuzzleRoom.PuzzleQuestion}' (Expected: '{activePuzzleRoom.PuzzleAnswer}')");
                    if (currentPlayerPuzzleInput == activePuzzleRoom.PuzzleAnswer) // 정답
                    {
                        activePuzzleRoom.IsPuzzleSolved = true; // 퍼즐 해결됨
                        activePuzzleRoom.IsPuzzleActive = false; // 퍼즐 비활성화
                        isInPuzzleInputMode = false;          // 입력 모드 해제
                        Debug.WriteLine($"[Form1] Puzzle SOLVED for room {activePuzzleRoom.GridPosition}!");

                        // 퍼즐 풀리면 해당 방의 적들 모두 제거 (선택적 기능)
                        if (enemies.Any())
                        {
                            Debug.WriteLine($"[Form1] Killing all {enemies.Count} enemies due to puzzle solve.");
                            for (int i = enemies.Count - 1; i >= 0; i--) enemies.RemoveAt(i); // 뒤에서부터 제거
                        }
                        currentLevel.CurrentRoom.ClearRoom(); // 방 클리어 처리 (IsCleared = true)
                        activePuzzleRoom = null; // 더 이상 활성 퍼즐방 아님
                    }
                    else // 오답
                    {
                        Debug.WriteLine($"[Form1] Puzzle WRONG! Player takes 1 damage.");
                        player?.TakeDamage(1); // 오답 시 1 데미지
                        currentPlayerPuzzleInput = ""; // 입력 초기화
                    }
                }
                else
                {
                    inputProcessedThisKey = false; // 퍼즐 관련 키가 아니었음
                }

                if (inputProcessedThisKey)
                {
                    this.Invalidate(); // UI 갱신 (입력된 내용, 타이머 등)
                    e.Handled = true; // 다른 컨트롤로 이벤트가 전달되지 않도록 함
                    e.SuppressKeyPress = true; // 추가적인 KeyPress 이벤트 방지 (특히 숫자 외 문자 입력 방지)
                    return; // 퍼즐 입력 처리했으므로 이후 로직(플레이어 이동 등) 실행 안 함
                }
            }

            // 일반 키 입력 처리 (퍼즐 입력 모드가 아닐 때)
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
            if (player == null || player.CurrentHealth <= 0) // 플레이어가 없거나 죽었으면 입력 처리 안함
            {
                if (player != null) player.Velocity = PointF.Empty; // 죽었으면 속도 0으로
                return;
            }

            if (isInPuzzleInputMode) // 퍼즐 입력 중에는 이동/공격 불가
            {
                player.Velocity = PointF.Empty; // 이동 정지
                return;
            }

            // 플레이어 이동 처리 (WASD)
            PointF moveDirection = PointF.Empty;
            if (pressedKeys.Contains(moveUpKey)) moveDirection.Y -= 1;
            if (pressedKeys.Contains(moveDownKey)) moveDirection.Y += 1;
            if (pressedKeys.Contains(moveLeftKey)) moveDirection.X -= 1;
            if (pressedKeys.Contains(moveRightKey)) moveDirection.X += 1;

            // Normalize된 속도 적용 또는 정지
            player.Velocity = !moveDirection.IsEmpty ? Normalize(moveDirection, player.Speed) : PointF.Empty;


            // 플레이어 공격 처리 (방향키)
            bool canAttack = true;
            // 생존방에서는 공격 불가
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
                        shootCooldownTimer = SHOOT_COOLDOWN; // 공격 쿨타임 초기화
                    }
                }
            }
        }

        private void UpdateGameObjects(float deltaTime)
        {
            player?.Update(deltaTime); // 플레이어 업데이트 (위치, 무적타이머 등)

            // 적 업데이트 (살아있는 적만)
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i].IsAlive)
                {
                    enemies[i].UpdateEnemy(deltaTime, player.Position); // AI 및 이동
                }
                else // 죽은 적은 리스트에서 제거
                {
                    enemies.RemoveAt(i);
                }
            }

            // 투사체 업데이트
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update(deltaTime);
            }


            // 퍼즐방 시간 제한 로직
            if (activePuzzleRoom != null &&
                activePuzzleRoom.IsPuzzleActive &&
                !activePuzzleRoom.IsPuzzleSolved &&
                player != null && player.CurrentHealth > 0)
            {
                currentPuzzleTimeLeft -= deltaTime; // 남은 시간 감소

                if (currentPuzzleTimeLeft <= 0)
                {
                    currentPuzzleTimeLeft = 0;
                    Debug.WriteLine($"[Form1] Puzzle TIMEOUT for room {activePuzzleRoom.GridPosition}! Player instant death.");

                    player?.InstantKill(); // 시간 초과 시 플레이어 즉사

                    activePuzzleRoom.IsPuzzleActive = false; // 퍼즐 비활성화
                    isInPuzzleInputMode = false;          // 입력 모드 해제
                    // activePuzzleRoom = null; // 여기서 null로 하면 UI가 바로 안사라질 수 있음. 다음 방 로드시 처리.
                }
            }

            // 생존방 시간 경과 및 완료 처리
            if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive
                && !activeSurvivalRoom.IsSurvivalCompleted && player != null && player.CurrentHealth > 0)
            {
                currentSurvivalTimeLeft -= deltaTime; // 남은 생존 시간 감소
                if (currentSurvivalTimeLeft <= 0)
                {
                    currentSurvivalTimeLeft = 0;
                    Debug.WriteLine($"[Form1] Survival COMPLETED for room {activeSurvivalRoom.GridPosition}!" +
                        $" All enemies (if any) die.");

                    activeSurvivalRoom.IsSurvivalCompleted = true; // 생존 완료
                    activeSurvivalRoom.IsSurvivalActive = false;   // 생존 모드 비활성화

                    // 생존 완료 시 해당 방의 적들 모두 제거
                    if (enemies.Any())
                    {
                        Debug.WriteLine($"[Form1] Killing all {enemies.Count} enemies after survival.");
                        for (int i = enemies.Count - 1; i >= 0; i--) enemies.RemoveAt(i);
                    }
                    activeSurvivalRoom.ClearRoom(); // 방 클리어 처리 (IsCleared = true)
                    activeSurvivalRoom = null; // 더 이상 활성 생존방 아님
                }
            }


            // 현재 방 클리어 조건 확인 (일반 전투방 등)
            if (currentLevel?.CurrentRoom != null && !currentLevel.CurrentRoom.IsCleared && player?.CurrentHealth > 0)
            {
                // 전투가 필요한 방 타입(Normal, Boss, MiniBoss)이고, 스폰할 적이 있었는데 현재 적이 없으면 클리어
                bool combatRoomNeedsClearing = currentLevel.CurrentRoom.Type == RoomType.Normal ||
                                               currentLevel.CurrentRoom.Type == RoomType.Boss ||
                                               currentLevel.CurrentRoom.Type == RoomType.MiniBoss;

                if (combatRoomNeedsClearing &&
                    currentLevel.CurrentRoom.EnemyTypesToSpawn.Count > 0 && // 스폰될 적 목록이 있어야 함 (즉, 원래 적이 있던 방)
                    enemies.Count == 0) // 현재 방에 적이 없음
                {
                    currentLevel.CurrentRoom.ClearRoom(); // 방 클리어
                    Debug.WriteLine($"[Form1] Combat Room {currentLevel.CurrentRoom.GridPosition} ({currentLevel.CurrentRoom.Type}) cleared by defeating all enemies.");
                }
            }
        }

        private void CheckPlayerRoomTransition()
        {
            if (player == null || currentLevel == null || currentLevel.CurrentRoom == null || player.CurrentHealth <= 0)
            {
                return; // 플레이어가 없거나, 레벨/현재방 정보가 없거나, 플레이어가 죽었으면 이동 불가
            }

            // 특정 상태에서는 방 이동 불가
            if (isInPuzzleInputMode) return; // 퍼즐 입력 중 이동 X
            if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive && !activeSurvivalRoom.IsSurvivalCompleted) return; // 생존 미완료 시 이동 X


            RectangleF playerBounds = player.Bounds;
            Room currentRoomLogic = currentLevel.CurrentRoom;
            bool moved = false;

            // 문의 시각적 크기와 충돌 감지 영역은 다를 수 있음. 여기서는 플레이어 위치 기준으로.
            // 플레이어가 화면 가장자리에 도달하고, 해당 방향에 문이 있으면 이동 시도
            float doorInteractionThreshold = player.Size.Width / 2f; // 플레이어가 문에 얼마나 들어가야 반응할지 (플레이어 크기의 절반 정도)
                                                                     // 또는 고정값 사용: float doorInteractionThreshold = 5; (픽셀)

            string attemptedDirection = "NONE"; // 디버깅용

            // 위쪽 문 (Y < threshold)
            if (playerBounds.Top < doorInteractionThreshold)
            {
                attemptedDirection = "TOP";
                if (currentRoomLogic.HasTopDoor && currentLevel.TryMoveToRoom(currentRoomLogic.GridPosition, 0, -1, player)) moved = true;
            }
            // 아래쪽 문 (Y > ClientHeight - threshold - playerHeight)
            else if (playerBounds.Bottom > ClientRectangle.Height - doorInteractionThreshold)
            {
                attemptedDirection = "BOTTOM";
                if (currentRoomLogic.HasBottomDoor && currentLevel.TryMoveToRoom(currentRoomLogic.GridPosition, 0, 1, player)) moved = true;
            }
            // 왼쪽 문 (X < threshold)
            else if (playerBounds.Left < doorInteractionThreshold)
            {
                attemptedDirection = "LEFT";
                if (currentRoomLogic.HasLeftDoor && currentLevel.TryMoveToRoom(currentRoomLogic.GridPosition, -1, 0, player)) moved = true;
            }
            // 오른쪽 문 (X > ClientWidth - threshold - playerWidth)
            else if (playerBounds.Right > ClientRectangle.Width - doorInteractionThreshold)
            {
                attemptedDirection = "RIGHT";
                if (currentRoomLogic.HasRightDoor && currentLevel.TryMoveToRoom(currentRoomLogic.GridPosition, 1, 0, player)) moved = true;
            }


            if (moved)
            {
                Debug.WriteLine($"[Form1] Successfully moved via {attemptedDirection} door from {currentRoomLogic.GridPosition}. Loading new room objects.");
                LoadCurrentRoomObjects(); // 새 방의 객체 로드
            }
            else // 이동하지 못했고, 벽에 부딪혔다면 위치 보정 (벽 뚫기 방지)
            {
                // 방 경계 밖으로 나가지 않도록 위치 보정 (문 통과 실패 시에도 적용될 수 있음)
                PointF currentPosition = player.Position;
                float newX = Math.Max(0, Math.Min(currentPosition.X, ClientRectangle.Width - player.Size.Width));
                float newY = Math.Max(0, Math.Min(currentPosition.Y, ClientRectangle.Height - player.Size.Height));

                // 문 근처에서 이동 실패 시, 문턱에서 약간 밀려나도록 처리할 수도 있음.
                // 예를 들어, 위쪽 문으로 이동 시도 실패 시: newY = doorInteractionThreshold;
                if (attemptedDirection != "NONE" && !moved) // 문으로 이동 시도했으나 실패한 경우
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
                    // player.Velocity = PointF.Empty; // 벽에 부딪히면 속도 0으로 (선택적)
                }
            }
        }


        private void CheckCollisions()
        {
            if (player == null || player.CurrentHealth <= 0 || currentLevel == null || currentLevel.CurrentRoom == null) return;

            // 플레이어 투사체 vs 적 충돌
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var p = projectiles[i];
                if (!p.IsPlayerProjectile) continue; // 플레이어 투사체만 검사

                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    var e = enemies[j];
                    if (!e.IsAlive) continue; // 살아있는 적만 검사

                    if (p.Bounds.IntersectsWith(e.Bounds))
                    {
                        e.TakeDamage(p.Damage); // 적에게 데미지
                        projectiles.RemoveAt(i);  // 투사체 제거
                        break; // 이 투사체는 하나의 적만 맞추므로 루프 종료
                    }
                }
            }

            // 적 vs 플레이어 충돌 (플레이어 무적 상태 고려됨 - Player.TakeDamage 내부에서)
            foreach (var e in enemies)
            {
                if (e.IsAlive && e.Bounds.IntersectsWith(player.Bounds))
                {
                    player.TakeDamage(e.CollisionDamage); // 플레이어에게 데미지
                    // player.TakeDamage 내부에서 무적 처리하므로, 연속 히트 방지는 Player 클래스에서 담당
                }
            }
        }

        private void CleanupObjects()
        {
            // 화면 밖으로 나가거나 수명이 다한 투사체 제거
            projectiles.RemoveAll(p => p.ShouldBeRemoved(this.ClientRectangle));

            // 죽은 적들은 UpdateGameObjects에서 이미 제거됨
        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.FromArgb(30, 30, 30)); // 기본 배경색 어둡게
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // 부드럽게 그리기 (선택적)

            try
            {
                currentLevel?.DrawCurrentRoom(g, this.ClientRectangle); // 현재 방 그리기 (배경, 문)

                foreach (var enemy in enemies) enemy.Draw(g);       // 적 그리기
                foreach (var projectile in projectiles) projectile.Draw(g); // 투사체 그리기
                player?.Draw(g);                                    // 플레이어 그리기

                DrawUI(g); // UI 그리기 (체력, 미니맵, 특수방 UI, 게임오버 등)
            }
            catch (Exception ex)
            {
                // 그리기 중 예외 발생 시 로그 (게임이 불안정해질 수 있음)
                Debug.WriteLine($"[Form1] Form1_Paint - EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                // 간단한 오류 메시지를 화면에 표시할 수도 있음
                // g.DrawString($"Paint Error: {ex.Message}", new Font("Arial", 12), Brushes.Red, 10, ClientSize.Height - 30);
            }
        }

        /// <summary>
        /// 게임의 사용자 인터페이스(UI) 요소들을 그립니다.
        /// </summary>
        private void DrawUI(Graphics g)
        {
            Font uiFont = new Font("Arial", Math.Max(8f, ClientSize.Height / 50f), FontStyle.Bold); // 화면 크기 비례 폰트, 최소 8pt

            // 플레이어 정보 (체력)
            if (player != null)
            {
                g.DrawString($"Health: {player.CurrentHealth} / {player.MaxHealth}", uiFont, Brushes.White, 10, 10);

                // 디버깅용 현재 방 정보
                if (currentLevel?.CurrentRoom != null)
                {
                    string roomDebugInfo = $"Room: {currentLevel.CurrentRoom.GridPosition} ({currentLevel.CurrentRoom.Type})";
                    if (currentLevel.CurrentRoom.IsCleared) roomDebugInfo += " [C]";
                    if (currentLevel.CurrentRoom.Type == RoomType.Puzzle) roomDebugInfo += $" (PuzS: {currentLevel.CurrentRoom.IsPuzzleSolved}, Active: {currentLevel.CurrentRoom.IsPuzzleActive})";
                    if (currentLevel.CurrentRoom.Type == RoomType.Survival) roomDebugInfo += $" (SurvC: {currentLevel.CurrentRoom.IsSurvivalCompleted}, Active: {currentLevel.CurrentRoom.IsSurvivalActive})";
                    if (player.IsInvincible) roomDebugInfo += " [INV]";


                    g.DrawString(roomDebugInfo, uiFont, Brushes.LightYellow, 10, ClientSize.Height - uiFont.GetHeight() * 2 - 10);
                    // g.DrawString($"PlayerPos: {player.Position.X:F1},{player.Position.Y:F1}", uiFont, Brushes.LightCyan, 10, ClientSize.Height - uiFont.GetHeight() - 5);
                }
            }

            // 퍼즐방 UI
            if (isInPuzzleInputMode && activePuzzleRoom != null && !activePuzzleRoom.IsPuzzleSolved && player != null && player.CurrentHealth > 0)
            {
                Font puzzleFont = new Font("Consolas", Math.Max(10f, ClientSize.Height / 30f), FontStyle.Bold);
                Font inputFont = new Font("Consolas", Math.Max(10f, ClientSize.Height / 28f), FontStyle.Bold);
                Font timerFont = new Font("Arial", Math.Max(9f, ClientSize.Height / 35f), FontStyle.Italic | FontStyle.Bold);

                string question = activePuzzleRoom.PuzzleQuestion;
                string cursor = (gameStopwatch.ElapsedMilliseconds / 400 % 2 == 0) ? "_" : " "; // 깜빡이는 커서
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
                float boxY = ClientSize.Height / 3.5f - boxHeight / 2f; // 화면 중앙보다 약간 위

                g.FillRectangle(new SolidBrush(Color.FromArgb(220, 10, 25, 55)), boxX, boxY, boxWidth, boxHeight); // UI 박스 배경
                g.DrawRectangle(Pens.LightSkyBlue, boxX, boxY, boxWidth, boxHeight); // UI 박스 테두리

                float currentYDraw = boxY + boxPadding;
                g.DrawString(question, puzzleFont, Brushes.White, boxX + boxPadding, currentYDraw);
                currentYDraw += qSize.Height + textVMargin;
                g.DrawString(currentAnswerDisplay, inputFont, Brushes.LimeGreen, boxX + boxPadding, currentYDraw);
                currentYDraw += iSize.Height + textVMargin;

                Brush timerBrush = (currentPuzzleTimeLeft <= 0) ? Brushes.DarkRed : (currentPuzzleTimeLeft < 10.0f && (int)(gameStopwatch.Elapsed.TotalSeconds * 2) % 2 == 0) ? Brushes.Red : Brushes.OrangeRed;
                g.DrawString(timerDisplay, timerFont, timerBrush, boxX + boxPadding, currentYDraw);
            }
            // 생존방 UI
            else if (activeSurvivalRoom != null && activeSurvivalRoom.IsSurvivalActive && !activeSurvivalRoom.IsSurvivalCompleted && player != null && player.CurrentHealth > 0)
            {
                Font survivalFont = new Font("Impact", Math.Max(12f, ClientSize.Height / 15f), FontStyle.Regular);
                Font survivalTimerFont = new Font("Consolas", Math.Max(11f, ClientSize.Height / 20f), FontStyle.Bold);

                string surviveText = "SURVIVE!";
                string survivalTimerDisplay = $"{Math.Max(0, currentSurvivalTimeLeft):F1}";

                SizeF surviveTextSize = g.MeasureString(surviveText, survivalFont);
                //SizeF survivalTimerDisplaySize = g.MeasureString(survivalTimerDisplay, survivalTimerFont);

                float surviveX = ClientSize.Width / 2f - surviveTextSize.Width / 2f;
                float surviveY = ClientSize.Height * 0.1f;

                float timerX = ClientSize.Width / 2f; // TextRenderer 중앙 정렬 사용 예정
                float timerY = surviveY + surviveTextSize.Height + ClientSize.Height * 0.01f;

                // "SURVIVE!" 텍스트 (외곽선 효과)
                TextRenderer.DrawText(g, surviveText, survivalFont, new Point((int)surviveX + 2, (int)surviveY + 2), Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, surviveText, survivalFont, new Point((int)surviveX, (int)surviveY), Color.OrangeRed, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);


                // 생존 타이머 텍스트 (외곽선 효과 및 시간 부족 시 깜빡임)
                Color survivalTimerColor;
                if (currentSurvivalTimeLeft <= 0) survivalTimerColor = Color.Yellow; // 완료 직전
                else if (currentSurvivalTimeLeft < 5.0f && (int)(gameStopwatch.Elapsed.TotalSeconds * 2.5) % 2 == 0) survivalTimerColor = Color.Red; // 5초 미만 깜빡임
                else survivalTimerColor = Color.White;

                TextRenderer.DrawText(g, survivalTimerDisplay, survivalTimerFont, new Point((int)timerX + 2, (int)timerY + 2), Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, survivalTimerDisplay, survivalTimerFont, new Point((int)timerX, (int)timerY), survivalTimerColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);


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
                Font gameOverFont = new Font("Impact", Math.Max(20f, (float)ClientSize.Height / 7f), FontStyle.Bold);
                string gameOverText = "GAME OVER";
                SizeF gameOverSize = g.MeasureString(gameOverText, gameOverFont);

                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), 0, 0, ClientSize.Width, ClientSize.Height); // 반투명 검은 배경
                // 그림자 효과
                g.DrawString(gameOverText, gameOverFont, Brushes.Black, ClientSize.Width / 2f - gameOverSize.Width / 2f + 5, ClientSize.Height / 2f - gameOverSize.Height / 2f + 5);
                g.DrawString(gameOverText, gameOverFont, Brushes.Firebrick, ClientSize.Width / 2f - gameOverSize.Width / 2f, ClientSize.Height / 2f - gameOverSize.Height / 2f); // 메인 텍스트
            }
        }


        private void DrawMinimap(Graphics g)
        {
            if (currentLevel == null || currentLevel.Rooms.Count == 0 || currentLevel.CurrentRoom == null) return;

            int minimapMargin = 15; // 화면 가장자리로부터의 여백
            int roomCellSize = (int)(Math.Min(ClientSize.Width, ClientSize.Height) / 55.0); // 셀 크기 동적 계산
            roomCellSize = Math.Max(5, Math.Min(12, roomCellSize)); // 최소 5, 최대 12
            int roomCellPadding = 2; // 셀 간 간격

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
                else // 발견되지 않은 방 (윤곽선만 표시)
                {
                    borderPen = Pens.SlateGray;
                    // Color.FromArgb의 두 번째 인자로 Color 구조체 사용
                    switch (room.Type)
                    {
                        // 수정된 부분 시작
                        case RoomType.Boss:
                            roomBrush = new SolidBrush(Color.FromArgb(100, Color.IndianRed)); // Brushes.IndianRed.Color -> Color.IndianRed
                            break;
                        case RoomType.Treasure:
                            roomBrush = new SolidBrush(Color.FromArgb(100, Color.Gold));      // Brushes.Gold.Color -> Color.Gold
                            break;
                        case RoomType.Shop:
                            roomBrush = new SolidBrush(Color.FromArgb(100, Color.MediumPurple));
                            break;
                        case RoomType.Secret:
                            // 비밀방은 발견 전에는 거의 안보이게 하거나 다른 어두운 색으로
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
                        // 수정된 부분 끝
                        default:
                            roomBrush = new SolidBrush(Color.FromArgb(100, Color.DimGray)); // Brushes.DimGray -> Color.DimGray
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

            // 애플리케이션 종료 (메시지 루프가 있는 경우와 없는 경우 모두 고려)
            if (this.IsHandleCreated && !this.IsDisposed) // 폼 핸들이 아직 유효한 경우
            {
                if (Application.MessageLoop) // 메시지 루프가 실행 중이면 정상 종료
                {
                    Application.Exit();
                }
                else // 메시지 루프가 없다면 (예: 초기화 실패로 바로 종료 시) 강제 종료
                {
                    Environment.Exit(1); // 오류 코드와 함께 종료
                }
            }
            else // 핸들이 이미 없거나 Dispose된 경우
            {
                Environment.Exit(1); // 오류 코드와 함께 종료
            }
        }

        // 벡터 정규화 유틸리티 메서드 (플레이어 이동 및 투사체 발사 시 사용)
        private PointF Normalize(PointF vec, float magnitude)
        {
            float length = (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
            if (length > 0)
            {
                return new PointF((vec.X / length) * magnitude, (vec.Y / length) * magnitude);
            }
            return PointF.Empty; // 길이가 0이면 빈 벡터 반환
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
    }
}