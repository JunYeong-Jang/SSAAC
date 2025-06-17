using Project_SSAAC.GameObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_SSAAC
{
    public partial class HomeControl : UserControl
    {

        private Form1 _main;
        private Player _player;

        public HomeControl(Form1 main, Player player)
        {
            InitializeComponent();
            _main = main;
            _player = player;
            this.Resize += resize_Controls;
            resize_Controls(null, null);


        }

        private void btnCm_Click(object sender, EventArgs e)
        {
            //  커스터마이징 버튼 클릭시
            
            _main.LoadControl(new CustomizingControl(_main, _player));
            //_main.LoadControl(new Control_CharacterCustom(_main));
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            //  창 종료

            _main.Close();
        }

        private void resize_Controls(object sender, EventArgs e)
        {
            //  창 리사이즈시 마다 버튼 컨트롤도 같이 리사이징

            int pw = pictureBox1.ClientSize.Width;
            int ph = pictureBox1.ClientSize.Height;

            int btnWidth = (int)(pw * 0.293);     // 300 / 1024
            int btnHeight = (int)(ph * 0.104);    // 60 / 576
            int spacing = (int)(ph * 0.02);       // 약 11.5~20px (기존 유지)

            int totalHeight = btnHeight * 3 + spacing * 2;
            int startY = (ph - totalHeight) / 2 + (int)(ph * 0.1);
            int startX = (pw - btnWidth) / 2;

            btnStart.SetBounds(startX, startY, btnWidth, btnHeight);
            btnCm.SetBounds(startX, startY + btnHeight + spacing, btnWidth, btnHeight);
            btnExit.SetBounds(startX, startY + (btnHeight + spacing) * 2, btnWidth, btnHeight);

            // 앞으로 보내기
            btnStart.BringToFront();
            btnCm.BringToFront();
            btnExit.BringToFront();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _main.LoadControl(new testroom(_main));
        }
    }
}
