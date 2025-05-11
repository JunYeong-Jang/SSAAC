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
            this.picBoxCharacter = new System.Windows.Forms.PictureBox();
            this.panel_menu = new System.Windows.Forms.Panel();
            this.panel_selectMenu = new System.Windows.Forms.Panel();
            this.picBox_btnPrev = new System.Windows.Forms.PictureBox();
            this.picBox_btnNext = new System.Windows.Forms.PictureBox();
            this.picBox_index = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxCharacter)).BeginInit();
            this.panel_menu.SuspendLayout();
            this.panel_selectMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_btnPrev)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_btnNext)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_index)).BeginInit();
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
            this.panel_menu.Controls.Add(this.panel_selectMenu);
            this.panel_menu.Controls.Add(this.picBoxCharacter);
            this.panel_menu.Location = new System.Drawing.Point(147, 108);
            this.panel_menu.Name = "panel_menu";
            this.panel_menu.Size = new System.Drawing.Size(705, 360);
            this.panel_menu.TabIndex = 5;
            // 
            // panel_selectMenu
            // 
            this.panel_selectMenu.Controls.Add(this.picBox_index);
            this.panel_selectMenu.Controls.Add(this.picBox_btnNext);
            this.panel_selectMenu.Controls.Add(this.picBox_btnPrev);
            this.panel_selectMenu.Location = new System.Drawing.Point(327, 143);
            this.panel_selectMenu.Name = "panel_selectMenu";
            this.panel_selectMenu.Size = new System.Drawing.Size(216, 88);
            this.panel_selectMenu.TabIndex = 5;
            // 
            // picBox_btnPrev
            // 
            this.picBox_btnPrev.Location = new System.Drawing.Point(27, 37);
            this.picBox_btnPrev.Name = "picBox_btnPrev";
            this.picBox_btnPrev.Size = new System.Drawing.Size(28, 28);
            this.picBox_btnPrev.TabIndex = 0;
            this.picBox_btnPrev.TabStop = false;
            this.picBox_btnPrev.Click += new System.EventHandler(this.picBox_btnPrev_Click);
            this.picBox_btnPrev.MouseEnter += new System.EventHandler(this.picBox_btnPrev_MouseEnter);
            this.picBox_btnPrev.MouseLeave += new System.EventHandler(this.picBox_btnPrev_MouseLeave);
            // 
            // picBox_btnNext
            // 
            this.picBox_btnNext.Location = new System.Drawing.Point(163, 37);
            this.picBox_btnNext.Name = "picBox_btnNext";
            this.picBox_btnNext.Size = new System.Drawing.Size(28, 28);
            this.picBox_btnNext.TabIndex = 1;
            this.picBox_btnNext.TabStop = false;
            this.picBox_btnNext.Click += new System.EventHandler(this.picBox_btnNext_Click);
            this.picBox_btnNext.MouseEnter += new System.EventHandler(this.picBox_btnNext_MouseEnter);
            this.picBox_btnNext.MouseLeave += new System.EventHandler(this.picBox_btnNext_MouseLeave);
            // 
            // picBox_index
            // 
            this.picBox_index.Location = new System.Drawing.Point(97, 37);
            this.picBox_index.Name = "picBox_index";
            this.picBox_index.Size = new System.Drawing.Size(30, 30);
            this.picBox_index.TabIndex = 2;
            this.picBox_index.TabStop = false;
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
            this.panel_selectMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picBox_btnPrev)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_btnNext)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_index)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnBackToHome;
        private System.Windows.Forms.PictureBox picBoxCharacter;
        private System.Windows.Forms.Panel panel_menu;
        private System.Windows.Forms.Panel panel_selectMenu;
        private System.Windows.Forms.PictureBox picBox_btnPrev;
        private System.Windows.Forms.PictureBox picBox_index;
        private System.Windows.Forms.PictureBox picBox_btnNext;
    }
}
