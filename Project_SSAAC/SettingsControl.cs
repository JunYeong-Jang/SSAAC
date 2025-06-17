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
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Project_SSAAC
{
    public partial class SettingsControl : UserControl
    {

        private Form1 _main;
        private Player _player;

        private Label lblMoveKey, lblAttackKey;
        private Button btnMoveKey1, btnMoveKey2, btnMoveKey3, btnMoveKey4;
        private Button btnAttackKey1, btnAttackKey2, btnAttackKey3, btnAttackKey4;

        private Label lblMusic, lblSFX;
        private TrackBar tbMusic, tbSFX;
        private CheckBox cbFullscreen, cbCRT;

        private Button activeKeyButton = null;

        private void button1_Click(object sender, EventArgs e)
        {
            _main.LoadControl(new HomeControl(_main, _player));
        }

       

        public SettingsControl(Form1 main, Player player)

        {
            InitializeComponent();
            _main = main;
            _player = player;
            this.BackColor = Color.Gray;
            this.addButtons();
            this.Resize += resize_Controls;
            resize_Controls(null, null);

            var bindButtons = new[] { btnMoveKey1, btnMoveKey2, btnMoveKey3, btnMoveKey4, btnAttackKey1, btnAttackKey2, btnAttackKey3, btnAttackKey4 };
          
            foreach (var btn in bindButtons)
            {
                btn.Click += KeyBindButton_Click;
            }


        }

        private void KeyBindButton_Click(object sender, EventArgs e)
        {
            activeKeyButton = sender as Button;
            activeKeyButton.Text = "대기중";

            var form = this.FindForm();

            if (form != null)
            {
                form.KeyPreview = true;
                form.KeyDown -= Form_KeyDown_Handler; // 중복 방지
                form.KeyDown += Form_KeyDown_Handler;
            }
        }

        private void Form_KeyDown_Handler(object sender, KeyEventArgs e)
        {
            if (activeKeyButton == null) return;

            string key = e.KeyCode.ToString();


            if (activeKeyButton == btnMoveKey1)
            {
                _main.SetMoveKey(Direction.Up, e.KeyCode);
            }
            else if (activeKeyButton == btnMoveKey2)
            {
                _main.SetMoveKey(Direction.Left, e.KeyCode);
            }
            else if (activeKeyButton == btnMoveKey3)
            {
                _main.SetMoveKey(Direction.Down, e.KeyCode);
            }
            else if (activeKeyButton == btnMoveKey4)
            {
                _main.SetMoveKey(Direction.Right, e.KeyCode);
            }
            else if (activeKeyButton == btnAttackKey1)
            {
                _main.SetShootKey(Direction.Up, e.KeyCode);
            }
            else if (activeKeyButton == btnAttackKey2)
            {
                _main.SetShootKey(Direction.Left, e.KeyCode);
            }
            else if (activeKeyButton == btnAttackKey3)
            {
                _main.SetShootKey(Direction.Down, e.KeyCode);
            }
            else if (activeKeyButton == btnAttackKey4)
            {
                _main.SetShootKey(Direction.Right, e.KeyCode);
            }




            activeKeyButton.Text = key;
            activeKeyButton = null;

            var form = sender as Form;
            
            if (form != null)
            {
                form.KeyDown -= Form_KeyDown_Handler;
            }
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
            btnAttackKey1 = new Button { Text = "↑", Location = new Point(200, 90), Size = new Size(60, 30), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White };
            btnAttackKey2 = new Button { Text = "←", Location = new Point(220, 90), Size = new Size(60, 30), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White };
            btnAttackKey3 = new Button { Text = "↓", Location = new Point(240, 90), Size = new Size(60, 30), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White };
            btnAttackKey4 = new Button { Text = "→", Location = new Point(260, 90), Size = new Size(60, 30), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White };

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
            this.Controls.Add(btnAttackKey1);
            this.Controls.Add(btnAttackKey2);
            this.Controls.Add(btnAttackKey3);
            this.Controls.Add(btnAttackKey4);
            this.Controls.Add(lblMusic);
            this.Controls.Add(tbMusic);
            this.Controls.Add(lblSFX);
            this.Controls.Add(tbSFX);
            this.Controls.Add(cbFullscreen);
            this.Controls.Add(cbCRT);


            //  컨트롤들을 맨앞으로 보이게 함
            lblMoveKey.BringToFront();
            btnMoveKey1.BringToFront();
            btnMoveKey2.BringToFront();
            btnMoveKey3.BringToFront();
            btnMoveKey4.BringToFront();
            lblAttackKey.BringToFront();
            btnAttackKey1.BringToFront();
            btnAttackKey2.BringToFront();
            btnAttackKey3.BringToFront();
            btnAttackKey4.BringToFront();
            lblMusic.BringToFront();
            tbMusic.BringToFront();
            lblSFX.BringToFront();
            tbSFX.BringToFront();
            cbFullscreen.BringToFront();
            cbCRT.BringToFront();


            
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
            btnAttackKey1.Location = new Point(marginX + labelWidth, currentY - 10);
            btnAttackKey2.Location = new Point(btnAttackKey1.Right + 10, btnAttackKey1.Top);
            btnAttackKey3.Location = new Point(btnAttackKey2.Right + 10, btnAttackKey1.Top);
            btnAttackKey4.Location = new Point(btnAttackKey3.Right + 10, btnAttackKey1.Top);
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
