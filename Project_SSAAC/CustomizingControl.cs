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

            this.BackgroundImage = Properties.Resources.background_customizing;
            picBoxCharacter.BackColor = Color.Transparent;
            picBoxCharacter.Size = new Size(50, 50);
            picBoxCharacter.SizeMode = PictureBoxSizeMode.StretchImage;
            panel_menu.BackgroundImage = Properties.Resources.panel_menu;
            panel_menu.BackColor = Color.Transparent;
            
            panel_menu.Size = new Size(600, 350);
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
                    break;
                case "green":
                    picBoxCharacter.Image = Properties.Resources.playerGreen;
                    break;
                case "mask":
                    picBoxCharacter.Image = Properties.Resources.playerMasked;
                    break;
                case "pink":
                    picBoxCharacter.Image = Properties.Resources.playerPink;
                    break;
            }
            
        }

        private void btnBodyNext_Click(object sender, EventArgs e)
        {
            currentAppearanceIndex = (currentAppearanceIndex + 1) % appearanceOptions.Count;
            lblBody.Text = (currentAppearanceIndex + 1).ToString();
            UpdateCharacterImage();
        }

        private void btnBodyPrev_Click(object sender, EventArgs e)
        {
            if(currentAppearanceIndex == 0)
            {
                currentAppearanceIndex = appearanceOptions.Count;
                return;
            }

            currentAppearanceIndex = (currentAppearanceIndex - 1);
            lblBody.Text = (currentAppearanceIndex + 1).ToString();
            UpdateCharacterImage();
        }

        private void CustomizingControl_Load(object sender, EventArgs e)
        {
            panel_menu.Location = new Point(_main.Width / 2 - panel_menu.Width / 2, this.Height / 2 - panel_menu.Height/2);
        }
    }
}
