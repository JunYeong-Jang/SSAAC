using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Project_SSAAC.GameObjects; // 네임스페이스 확인!
using System.Diagnostics;

namespace Project_SSAAC // 네임스페이스 확인!
{
    public partial class Form1 : Form
    {
        // 게임 객체 및 상태 변수
        private Player player;
        private List<Enemy> enemies = new List<Enemy>();
        private List<Projectile> projectiles = new List<Projectile>();
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();

        // 게임 루프 및 시간 관련 변수
        private Timer gameTimer;
        private Stopwatch gameStopwatch;
        private float lastFrameTime = 0f;

        // 발사 쿨다운 변수
        private float shootCooldownTimer = 0f;
        private const float SHOOT_COOLDOWN = 0.3f;

        public Form1()
        {
            InitializeComponent();
            LoadControl(new HomeControl(this));
            this.DoubleBuffered = true; // 필수
            InitializeGame();
            InitializeTimer();
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
            //this.ClientSize = new Size(800, 600);
            this.Text = "Project SSAAC"; // 폼 제목 변경

            player = new Player(new PointF(this.ClientSize.Width / 2f - 16f, this.ClientSize.Height / 2f - 16f));
            enemies.Add(new BasicEnemy(new PointF(100, 100)));
            enemies.Add(new BasicEnemy(new PointF(600, 400)));

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.Paint += Form1_Paint;
        }

        private void InitializeTimer()
        {
            gameTimer = new Timer { Interval = 16 };
            gameTimer.Tick += GameTimer_Tick;
            gameStopwatch = Stopwatch.StartNew();
            lastFrameTime = (float)gameStopwatch.Elapsed.TotalSeconds;
            gameTimer.Start();
        }

        // --- 게임 루프 ---
        private void GameTimer_Tick(object sender, EventArgs e)
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

        // --- 입력 처리 ---
        private void HandleInput(float deltaTime)
        {
            if (player == null) return;
            PointF moveDirection = PointF.Empty;
            if (pressedKeys.Contains(Keys.W)) moveDirection.Y -= 1;
            if (pressedKeys.Contains(Keys.S)) moveDirection.Y += 1;
            if (pressedKeys.Contains(Keys.A)) moveDirection.X -= 1;
            if (pressedKeys.Contains(Keys.D)) moveDirection.X += 1;
            player.Velocity = moveDirection.IsEmpty ? PointF.Empty : Normalize(moveDirection, player.Speed);

            PointF shootDirection = PointF.Empty;
            if (pressedKeys.Contains(Keys.Up)) shootDirection.Y -= 1;
            if (pressedKeys.Contains(Keys.Down)) shootDirection.Y += 1;
            if (pressedKeys.Contains(Keys.Left)) shootDirection.X -= 1;
            if (pressedKeys.Contains(Keys.Right)) shootDirection.X += 1;

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

        // --- 상태 업데이트 ---
        private void UpdateGameObjects(float deltaTime)
        {
            player?.Update(deltaTime);
            // List<Enemy> 사용 시 foreach 루프 중 컬렉션 수정 오류 방지를 위해 역방향 순회 또는 복사본 사용 고려 필요
            // 여기서는 간단하게 foreach 사용 (CleanupObjects에서 제거하므로 일반적으로는 괜찮음)
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive) // 살아있는 적만 업데이트
                {
                    enemy.UpdateEnemy(deltaTime, player.Position);
                }
            }
            // Projectile 업데이트 (역방향 순회)
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update(deltaTime);
            }
        }

        // --- 충돌 감지 ---
        private void CheckCollisions()
        {
            if (player == null) return;
            // 플레이어 탄환 vs 적 (역방향 순회하며 제거 고려)
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var p = projectiles[i];
                if (!p.IsPlayerProjectile) continue; // 플레이어 탄환만
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    var e = enemies[j];
                    if (!e.IsAlive) continue; // 살아있는 적만
                    if (p.Bounds.IntersectsWith(e.Bounds))
                    {
                        e.TakeDamage(p.Damage);
                        // 발사체 제거 (Update 루프 밖에서 제거하는 것이 더 안전할 수 있음 -> CleanupObjects 활용)
                        // projectiles.RemoveAt(i); // 여기서 바로 제거하면 인덱스 문제 발생 가능성 있음
                        // 대신 Projectile에 IsMarkedForRemoval 플래그 추가 고려
                        // 임시방편: 발사체 수명을 매우 짧게 하여 CleanupObjects에서 제거되도록 함
                        // 또는 아래 CleanupObjects에서 제거하므로 여기서는 TakeDamage만 호출
                        // projectiles.RemoveAt(i); // 이 줄은 주석 처리하거나 다른 방식으로 처리
                        break; // 한 발은 한 명만
                    }
                }
                // 발사체 제거는 CleanupObjects에서 하므로 여기서 break 후 다음 발사체로 넘어감
            }

            // 적 vs 플레이어
            foreach (var e in enemies)
            {
                if (!e.IsAlive) continue;
                if (e.Bounds.IntersectsWith(player.Bounds))
                {
                    player.TakeDamage(e.CollisionDamage);
                    // (넉백 등 추가 로직)
                }
            }
        }

        // --- 객체 정리 ---
        private void CleanupObjects()
        {
            // 죽은 적 제거 (IsAlive가 false인 적 제거)
            enemies.RemoveAll(e => !e.IsAlive);
            // 제거 필요한 발사체 제거 (수명 다하거나 화면 밖)
            projectiles.RemoveAll(p => p.ShouldBeRemoved(this.ClientRectangle));

            // 플레이어 탄환 vs 적 충돌 시 발사체 제거 로직 (CheckCollisions에서 제거하지 않은 경우)
            projectiles.RemoveAll(p =>
            {
                if (!p.IsPlayerProjectile) return false; // 플레이어 탄환만
                foreach (var e in enemies) // 이미 죽은 적은 위에서 제거됨
                {
                    // 여기서는 IsAlive 체크 불필요 (살아있는 적만 enemies 리스트에 있음)
                    if (p.Bounds.IntersectsWith(e.Bounds))
                    {
                        return true; // 충돌했으면 제거 대상으로 표시
                    }
                }
                return false;
            });
        }


        // --- 화면 그리기 ---
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.FromArgb(40, 40, 40));
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var enemy in enemies) enemy.Draw(g); // 살아있는 적만 그려짐 (죽으면 IsAlive false)
            foreach (var projectile in projectiles) projectile.Draw(g);
            player?.Draw(g);

            DrawUI(g);
        }

        // --- UI 그리기 ---
        private void DrawUI(Graphics g)
        {
            if (player != null)
            {
                string healthText = $"Health: {player.CurrentHealth} / {player.MaxHealth}";
                g.DrawString(healthText, this.Font, Brushes.White, 10, 10);
            }
        }

        // --- 키보드 이벤트 핸들러 ---
        private void Form1_KeyDown(object sender, KeyEventArgs e) { pressedKeys.Add(e.KeyCode); }
        private void Form1_KeyUp(object sender, KeyEventArgs e) { pressedKeys.Remove(e.KeyCode); }

        // --- 헬퍼 함수 ---
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
    }
}