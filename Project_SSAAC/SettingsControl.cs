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
    public partial class SettingsControl : UserControl
    {

        private Form1 _main;

        private Label lblMoveKey, lblAttackKey;
        private Button btnMoveKey1, btnMoveKey2, btnMoveKey3, btnMoveKey4, btnAttackKey;
        private Label lblMusic, lblSFX;
        private TrackBar tbMusic, tbSFX;
        private CheckBox cbFullscreen, cbCRT;

        private void button1_Click(object sender, EventArgs e)
        {
            _main.LoadControl(new HomeControl(_main));
        }

       

        public SettingsControl(Form1 main)

        {
            InitializeComponent();
            _main = main;
            this.BackColor = Color.Gray;
            this.addButtons();
            this.Resize += resize_Controls;
            resize_Controls(null, null);


        }

        private void addButtons()
        {
            // 키 설정
            lblMoveKey = new Label { Text = "이동 키:", ForeColor = Color.White, Location = new Point(100, 50), AutoSize = true };
            btnMoveKey1 = new Button { Text = "W", Location = new Point(200, 40), Size = new Size(60, 30), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White };
            btnMoveKey2 = new Button { Text = "A", Location = new Point(220, 40), Size = new Size(60, 30), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White };
            btnMoveKey3 = new Button { Text = "S", Location = new Point(240, 40), Size = new Size(60, 30), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White };
            btnMoveKey4 = new Button { Text = "D", Location = new Point(260, 40), Size = new Size(60, 30), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White };

            lblAttackKey = new Label { Text = "공격 키:", ForeColor = Color.White, Location = new Point(100, 100), AutoSize = true };
            btnAttackKey = new Button { Text = "↑", Location = new Point(200, 90), Size = new Size(60, 30), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White };

            // 사운드 설정
            lblMusic = new Label { Text = "배경음악:", ForeColor = Color.White, Location = new Point(100, 160), AutoSize = true };
            tbMusic = new TrackBar { Location = new Point(200, 150), Width = 300, Minimum = 0, Maximum = 100, Value = 50 };

            lblSFX = new Label { Text = "효과음:", ForeColor = Color.White, Location = new Point(100, 210), AutoSize = true };
            tbSFX = new TrackBar { Location = new Point(200, 200), Width = 300, Minimum = 0, Maximum = 100, Value = 70 };

            // 체크박스 옵션
            cbFullscreen = new CheckBox { Text = "전체화면", ForeColor = Color.White, Location = new Point(100, 270), AutoSize = true };
            cbCRT = new CheckBox { Text = "CRT 필터", ForeColor = Color.White, Location = new Point(250, 270), AutoSize = true };

            // 컨트롤 추가
            this.Controls.Add(lblMoveKey);
            this.Controls.Add(btnMoveKey1);
            this.Controls.Add(btnMoveKey2);
            this.Controls.Add(btnMoveKey3);
            this.Controls.Add(btnMoveKey4);
            this.Controls.Add(lblAttackKey);
            this.Controls.Add(btnAttackKey);
            this.Controls.Add(lblMusic);
            this.Controls.Add(tbMusic);
            this.Controls.Add(lblSFX);
            this.Controls.Add(tbSFX);
            this.Controls.Add(cbFullscreen);
            this.Controls.Add(cbCRT);

            lblMoveKey.BringToFront();


            
        }

        private void resize_Controls(object sender, EventArgs e)
        {
            //  창 리사이즈시 마다 버튼 컨트롤도 같이 리사이징

            int parentWidth = this.Width;
            int parentHeight = this.Height;
            int marginX = parentWidth / 20;   // 좌우 여백
            int spacingY = parentHeight / 20; // 수직 간격
            int labelWidth = 100;
            int buttonWidth = 60;
            int buttonHeight = 30;

            int currentY = spacingY;

            // 이동 키
            lblMoveKey.Location = new Point(marginX, currentY);
            btnMoveKey1.Location = new Point(marginX + labelWidth, currentY - 10);
            btnMoveKey2.Location = new Point(btnMoveKey1.Right + 10, btnMoveKey1.Top );
            btnMoveKey3.Location = new Point(btnMoveKey2.Right + 10, btnMoveKey1.Top);
            btnMoveKey4.Location = new Point(btnMoveKey3.Right + 10, btnMoveKey1.Top);
            currentY += buttonHeight + spacingY;

            // 공격 키
            lblAttackKey.Location = new Point(marginX, currentY);
            btnAttackKey.Location = new Point(marginX + labelWidth, currentY - 10);
            currentY += buttonHeight + spacingY;

            // 배경음악
            lblMusic.Location = new Point(marginX, currentY + 5);
            tbMusic.Location = new Point(marginX + labelWidth, currentY);
            tbMusic.Width = parentWidth / 2;
            currentY += tbMusic.Height + spacingY;

            // 효과음
            lblSFX.Location = new Point(marginX, currentY + 5);
            tbSFX.Location = new Point(marginX + labelWidth, currentY);
            tbSFX.Width = parentWidth / 2;
            currentY += tbSFX.Height + spacingY;

            // 체크박스
            cbFullscreen.Location = new Point(marginX, currentY);
            cbCRT.Location = new Point(cbFullscreen.Right + 30, currentY);


            //  체크박스 핸들러 등록

            
            cbFullscreen.CheckedChanged += cbFullscreen_CheckedChanged;


            // 홈 아이콘 왼쪽 아래
            int pb2Width = (int)(parentWidth * 0.06);   
            int pb2Height = pb2Width;          // 정사각형으로 가정

            int pb2X = (int)(parentWidth * 0.02); // 오른쪽 여백 약 2%
            int pb2Y = parentHeight - pb2Height - (int)(parentHeight * 0.02); // 아래쪽 여백 약 2%

            btnToHome.SetBounds(pb2X, pb2Y, pb2Width, pb2Height);
            btnToHome.BackColor = Color.Gray;
            btnToHome.BringToFront();
        }

        private void cbFullscreen_CheckedChanged(object sender, EventArgs e)
        {
            //  전체화면 체크박스 기능
            if (cbFullscreen.Checked)
            {
                _main.FormBorderStyle = FormBorderStyle.None;
                _main.WindowState = FormWindowState.Maximized;
                _main.TopMost = true;
                this.resize_Controls(null, null);
            }
            else
            {
                _main.FormBorderStyle = FormBorderStyle.Sizable;
                _main.WindowState = FormWindowState.Normal;
                _main.TopMost = false;
            }
        }
    }
}
