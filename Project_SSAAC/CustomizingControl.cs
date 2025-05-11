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

        public CustomizingControl(Form1 main)
        {
            InitializeComponent();
            _main = main;

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

            UpdateCharacterImage();
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
                    picBoxCharacter.Image = Properties.Resources.playerBlue;
                    picBox_index.Image = Properties.Resources.text_1;
                    break;
                case "green":
                    picBoxCharacter.Image = Properties.Resources.playerGreen;
                    picBox_index.Image= Properties.Resources.text_2;
                    break;
                case "mask":
                    picBoxCharacter.Image = Properties.Resources.playerMasked;
                    picBox_index.Image = Properties.Resources.text_3;
                    break;
                case "pink":
                    picBoxCharacter.Image = Properties.Resources.playerPink;
                    picBox_index.Image = Properties.Resources.text_4;
                    break;
            }
            
        }


        private void CustomizingControl_Load(object sender, EventArgs e)
        {
            panel_menu.Location = new Point(_main.Width / 2 - panel_menu.Width / 2, this.Height / 2 - panel_menu.Height/2);
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
