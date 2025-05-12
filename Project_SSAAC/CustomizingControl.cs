using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Project_SSAAC.GameObjects;
using Project_SSAAC.UI;

namespace Project_SSAAC
{
    public partial class CustomizingControl : UserControl
    {
        //  테스트용 

        private Form1 _main;

        // 캐릭터 이미지의 프레임 인덱스
        int currentFrame = 0;

        // 캐릭터 이미지 불러오기
        Bitmap blueSheet = Properties.Resources.blue_Idle;
        Bitmap greenSheet = Properties.Resources.green_Idle;
        Bitmap pinkSheet = Properties.Resources.pink_Idle;
        Bitmap maskedSheet = Properties.Resources.masked_Idle;
        // 유저가 고른 캐릭터의 이미지 sheet를 저장할 bitmap
        Bitmap currentSheet;

        public CustomizingControl(Form1 main)
        {
            InitializeComponent();
            _main = main;

            // 기본 캐릭터는 blue
            Bitmap currentSheet = blueSheet;

            // 캐릭터 애니메이션을 활성화할 타이머
            Timer animationTimer = new Timer();
            animationTimer.Interval = 100; // 0.1초 간격 (10fps 정도)
            animationTimer.Tick += AnimationTimer_Tick; // 틱마다 자세를 변경하는 함수
            animationTimer.Start();

            // 현재 컨트롤의 크기 지정
            this.Width = _main.ClientSize.Width;
            this.Height = _main.ClientSize.Height;
            this.Dock = DockStyle.Fill;

            // 유저컨트롤 배경 이미지
            this.BackgroundImage = Properties.Resources.background_customizing;

            // 캐릭터 외형 이미지
            picBoxCharacter.BackColor = Color.Transparent;
            picBoxCharacter.Size = new Size(50, 50);
            picBoxCharacter.SizeMode = PictureBoxSizeMode.StretchImage;
            
            // 외형선택 메뉴 전체 패널
            panel_menu.BackgroundImage = Properties.Resources.panel_menu;
            panel_menu.BackColor = Color.Transparent;
            panel_menu.Size = new Size(600, 350);
            
            // 외형선택 메뉴 안쪽 패널
            panel_selectMenu.BackgroundImage = Properties.Resources.frame_grey;
            panel_selectMenu.Size = new Size(216, 88);
            
            // 선택버튼 이미지
            picBox_btnPrev.Image = Properties.Resources.button_grey_prev_idle;
            picBox_btnPrev.Size = new Size(28, 28);
            picBox_btnNext.Image = Properties.Resources.button_grey_next_idle;
            picBox_btnNext.Size = new Size(28, 28);
            // 인덱스 이미지
            picBox_index.Image = Properties.Resources.text_1;
            picBox_index.Size = new Size(30, 30);

            this.Resize += CustomizingControl_Resize;   // 창 크기변화에 따라 재정렬하는 함수
            // 컨트롤 최초 정렬
            CustomizingControl_Resize(null, null);

            // 캐릭터 이미지 띄우기
            UpdateCharacterImage();

        }

        // 캐릭터 sheet에서 frameIndex에서 
        public Bitmap GetCharacterFrame(int frameIndex)
        {
            int frameWidth = 32;
            int frameHeight = 32;

            // 이미지가 그려질 50 * 50 크기의 bitmap 정의
            Bitmap frame = new Bitmap(50, 50);

            // 잘라올 위치 (가로 방향으로 index번째)
            Rectangle srcRect = new Rectangle(frameIndex * frameWidth, 0, frameWidth, frameHeight);

            // 도화지에 그릴 위치 (0,0부터 시작, 50*50 크기)
            Rectangle destRect = new Rectangle(0, 0, 50, 50);

            // frame에 이미지 그리기
            Graphics g = Graphics.FromImage(frame);
            g.DrawImage(currentSheet, destRect, srcRect, GraphicsUnit.Pixel);
            g.Dispose();

            return frame;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            //// 상태변경은 나중에
            //if (currentState == "idle")
            //{
            //}
            //else if (currentState == "run")
            //{
            //}
            picBoxCharacter.Image = GetCharacterFrame(currentFrame);
            currentFrame = (currentFrame + 1) % 11; // 다음 프레임으로 순환
        }

        //// 상태변경 함수
        //private void SetState(string newState)
        //{
        //    if (currentState != newState)
        //    {
        //        currentState = newState;
        //        currentFrame = 0; // 상태 바뀔 때 첫 프레임부터 다시
        //    }
        //}

        private void CustomizingControl_Resize(object sender, EventArgs e)
        {
            panel_menu.Location = new Point(
    (this.Width - panel_menu.Width) / 2, (this.Height - panel_menu.Height) / 2);

            picBoxCharacter.Location = new Point(
                (panel_menu.Width - picBoxCharacter.Width) / 2, panel_menu.Height / 4);

            panel_selectMenu.Location = new Point(
                (panel_menu.Width - panel_selectMenu.Width) / 2,
                (panel_menu.Height - panel_selectMenu.Height) * 3 / 4);

            picBox_btnPrev.Location = new Point(20, (panel_selectMenu.Height - picBox_btnPrev.Height) / 2);
            picBox_btnNext.Location = new Point(panel_selectMenu.Width - picBox_btnNext.Width - 20, (panel_selectMenu.Height - picBox_btnNext.Height) / 2);
            picBox_index.Location = new Point((panel_selectMenu.Width - picBox_index.Width) / 2, (panel_selectMenu.Height - picBox_index.Height) / 2);
            btnBackToHome.Location = new Point(10, this.Bottom - btnBackToHome.Height - 10);
        }

        private void btnBackToHome_Click(object sender, EventArgs e)
        {
            //  홈으로 돌아가기 버튼을 누르면 선택된 픽쳐박스 데이터 저장됨
            PictureBox pic = new PictureBox();
            pic.Image = picBoxCharacter.Image;

            _main.RegisterCharacter(pic);
            _main.LoadControl(new HomeControl(_main));
        }

        // 요청
        public Player customizedPlayer { get; private set; } = new Player(new PointF(10,10)); // (PointF(10,10))임시 생성 --> Player() 기본생성자 필요
        private List<string> appearanceOptions = new List<string> { "blue", "green", "mask", "pink" };
        private int currentAppearanceIndex = 0;
        private void UpdateCharacterImage()
        {
            string appearance = appearanceOptions[currentAppearanceIndex];
            
            // 요청
            // Player 클래스에 유저가 선택한 외형을 저장할 public string 타입의 SelectedAppearance 변수 필요
            // customizedPlayer.SelectedAppearance = appearance;


            switch (appearance)
            {
                case "blue":
                    //picBoxCharacter.Image = Properties.Resources.playerBlue;
                    currentSheet = blueSheet;
                    picBox_index.Image = Properties.Resources.text_1;
                    break;
                case "green":
                    currentSheet = greenSheet;
                    //picBoxCharacter.Image = Properties.Resources.playerGreen;
                    picBox_index.Image= Properties.Resources.text_2;
                    break;
                case "mask":
                    currentSheet = maskedSheet;
                    //picBoxCharacter.Image = Properties.Resources.playerMasked;
                    picBox_index.Image = Properties.Resources.text_3;
                    break;
                case "pink":
                    currentSheet = pinkSheet;
                    //picBoxCharacter.Image = Properties.Resources.playerPink;
                    picBox_index.Image = Properties.Resources.text_4;
                    break;
            }
            
        }


        private void CustomizingControl_Load(object sender, EventArgs e)
        {
            // resize로 인한 불필요한 호출이 되었음
            // 오버헤드 발생
            //panel_menu.Location = new Point(
            //    (this.Width - panel_menu.Width) / 2, (this.Height - panel_menu.Height) / 2);

            //picBoxCharacter.Location = new Point(
            //    (panel_menu.Width - picBoxCharacter.Width) / 2, panel_menu.Height / 4);

            //panel_selectMenu.Location = new Point(
            //    (panel_menu.Width - panel_selectMenu.Width) / 2,
            //    (panel_menu.Height - panel_selectMenu.Height) * 3 / 4);
            
            //picBox_btnPrev.Location = new Point(20, (panel_selectMenu.Height - picBox_btnPrev.Height) / 2);
            //picBox_btnNext.Location = new Point(panel_selectMenu.Width - picBox_btnNext.Width - 20, (panel_selectMenu.Height - picBox_btnNext.Height) / 2);
            //picBox_index.Location = new Point((panel_selectMenu.Width - picBox_index.Width) / 2, (panel_selectMenu.Height - picBox_index.Height) / 2);

            //btnBackToHome.Location = new Point(10, this.Bottom - btnBackToHome.Height - 10);
        }

        private void picBox_btnPrev_MouseEnter(object sender, EventArgs e)
        {
            picBox_btnPrev.Image = Properties.Resources.button_grey_prev_focused;
        }

        private void picBox_btnPrev_MouseLeave(object sender, EventArgs e)
        {
            picBox_btnPrev.Image = Properties.Resources.button_grey_prev_idle;
        }
        private void picBox_btnNext_MouseEnter(object sender, EventArgs e)
        {
            picBox_btnNext.Image = Properties.Resources.button_grey_next_focused;
        }
        private void picBox_btnNext_MouseLeave(object sender, EventArgs e)
        {
            picBox_btnNext.Image = Properties.Resources.button_grey_next_idle;
        }

        private void picBox_btnPrev_Click(object sender, EventArgs e)
        {
            if (currentAppearanceIndex == 0)
            {
                currentAppearanceIndex = appearanceOptions.Count - 1;
                UpdateCharacterImage();
                return;
            }

            currentAppearanceIndex = (currentAppearanceIndex - 1);
            UpdateCharacterImage();
        }

        private void picBox_btnNext_Click(object sender, EventArgs e)
        {
            currentAppearanceIndex = (currentAppearanceIndex + 1) % appearanceOptions.Count;
            UpdateCharacterImage();
        }
    }
}
