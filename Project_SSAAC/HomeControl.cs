// 파일 위치: Project_SSAAC/HomeControl.cs
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

        public HomeControl(Form1 main)
        {
            InitializeComponent();
            _main = main;
            this.Resize += resize_Controls;
            resize_Controls(null, null);
        }

        private void btnCm_Click(object sender, EventArgs e)
        {
            _main.LoadControl(new CustomizingControl(_main));
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            _main.Close();
        }

        private void resize_Controls(object sender, EventArgs e)
        {
            int pw = pictureBox1.ClientSize.Width;
            int ph = pictureBox1.ClientSize.Height;

            int btnWidth = (int)(pw * 0.293);
            int btnHeight = (int)(ph * 0.104);
            int spacing = (int)(ph * 0.02);

            int totalHeight = btnHeight * 3 + spacing * 2;
            int startY = (ph - totalHeight) / 2 + (int)(ph * 0.1);
            int startX = (pw - btnWidth) / 2;

            btnStart.SetBounds(startX, startY, btnWidth, btnHeight);
            btnCm.SetBounds(startX, startY + btnHeight + spacing, btnWidth, btnHeight);
            btnExit.SetBounds(startX, startY + (btnHeight + spacing) * 2, btnWidth, btnHeight);

            btnStart.BringToFront();
            btnCm.BringToFront();
            btnExit.BringToFront();

            int pb2Width = (int)(pw * 0.06);
            int pb2Height = pb2Width;

            int pb2X = pw - pb2Width - (int)(pw * 0.02);
            int pb2Y = ph - pb2Height - (int)(ph * 0.02);

            btnSettings.SetBounds(pb2X, pb2Y, pb2Width, pb2Height);
            btnSettings.BackColor = Color.Gray;

            btnSettings.BringToFront();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Form1에 있는 StartGame 메서드를 호출하여 게임 시작을 알립니다.
            _main.StartGame();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            _main.LoadControl(new SettingsControl(_main));
        }
    }
}