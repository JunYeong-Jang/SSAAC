namespace Project_SSAAC
{
    partial class CustomizingControl
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnBackToHome = new System.Windows.Forms.Button();
            this.btnBodyPrev = new System.Windows.Forms.Button();
            this.btnBodyNext = new System.Windows.Forms.Button();
            this.lblBody = new System.Windows.Forms.Label();
            this.picBoxCharacter = new System.Windows.Forms.PictureBox();
            this.panel_menu = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxCharacter)).BeginInit();
            this.panel_menu.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnBackToHome
            // 
            this.btnBackToHome.Location = new System.Drawing.Point(3, 536);
            this.btnBackToHome.Name = "btnBackToHome";
            this.btnBackToHome.Size = new System.Drawing.Size(140, 37);
            this.btnBackToHome.TabIndex = 0;
            this.btnBackToHome.Text = "홈으로 돌아가기";
            this.btnBackToHome.UseVisualStyleBackColor = true;
            this.btnBackToHome.Click += new System.EventHandler(this.btnBackToHome_Click);
            // 
            // btnBodyPrev
            // 
            this.btnBodyPrev.BackColor = System.Drawing.Color.Transparent;
            this.btnBodyPrev.Location = new System.Drawing.Point(16, 59);
            this.btnBodyPrev.Name = "btnBodyPrev";
            this.btnBodyPrev.Size = new System.Drawing.Size(75, 23);
            this.btnBodyPrev.TabIndex = 2;
            this.btnBodyPrev.Text = "Prev";
            this.btnBodyPrev.UseVisualStyleBackColor = false;
            this.btnBodyPrev.Click += new System.EventHandler(this.btnBodyPrev_Click);
            // 
            // btnBodyNext
            // 
            this.btnBodyNext.Location = new System.Drawing.Point(216, 59);
            this.btnBodyNext.Name = "btnBodyNext";
            this.btnBodyNext.Size = new System.Drawing.Size(75, 23);
            this.btnBodyNext.TabIndex = 1;
            this.btnBodyNext.Text = "Next";
            this.btnBodyNext.UseVisualStyleBackColor = true;
            this.btnBodyNext.Click += new System.EventHandler(this.btnBodyNext_Click);
            // 
            // lblBody
            // 
            this.lblBody.AutoSize = true;
            this.lblBody.Location = new System.Drawing.Point(151, 63);
            this.lblBody.Name = "lblBody";
            this.lblBody.Size = new System.Drawing.Size(15, 15);
            this.lblBody.TabIndex = 0;
            this.lblBody.Text = "1";
            // 
            // picBoxCharacter
            // 
            this.picBoxCharacter.Location = new System.Drawing.Point(115, 158);
            this.picBoxCharacter.Name = "picBoxCharacter";
            this.picBoxCharacter.Size = new System.Drawing.Size(50, 50);
            this.picBoxCharacter.TabIndex = 4;
            this.picBoxCharacter.TabStop = false;
            // 
            // panel_menu
            // 
            this.panel_menu.Controls.Add(this.panel1);
            this.panel_menu.Controls.Add(this.picBoxCharacter);
            this.panel_menu.Location = new System.Drawing.Point(147, 108);
            this.panel_menu.Name = "panel_menu";
            this.panel_menu.Size = new System.Drawing.Size(705, 360);
            this.panel_menu.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnBodyPrev);
            this.panel1.Controls.Add(this.btnBodyNext);
            this.panel1.Controls.Add(this.lblBody);
            this.panel1.Location = new System.Drawing.Point(302, 122);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(310, 137);
            this.panel1.TabIndex = 5;
            // 
            // CustomizingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel_menu);
            this.Controls.Add(this.btnBackToHome);
            this.Name = "CustomizingControl";
            this.Size = new System.Drawing.Size(1024, 576);

            this.Load += new System.EventHandler(this.CustomizingControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picBoxCharacter)).EndInit();
            this.panel_menu.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();

            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnBackToHome;
        private System.Windows.Forms.Button btnBodyPrev;
        private System.Windows.Forms.Button btnBodyNext;
        private System.Windows.Forms.Label lblBody;
        private System.Windows.Forms.PictureBox picBoxCharacter;
        private System.Windows.Forms.Panel panel_menu;
        private System.Windows.Forms.Panel panel1;
    }
}
