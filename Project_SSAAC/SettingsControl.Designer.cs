namespace Project_SSAAC
{
    partial class SettingsControl
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
            this.btnToHome = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnToHome
            // 
            this.btnToHome.Image = global::Project_SSAAC.Properties.Resources.home_icon_60x60;
            this.btnToHome.Location = new System.Drawing.Point(31, 470);
            this.btnToHome.Name = "btnToHome";
            this.btnToHome.Size = new System.Drawing.Size(60, 60);
            this.btnToHome.TabIndex = 0;
            this.btnToHome.UseVisualStyleBackColor = true;
            this.btnToHome.Click += new System.EventHandler(this.button1_Click);
            // 
            // SettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnToHome);
            this.Name = "SettingsControl";
            this.Size = new System.Drawing.Size(1024, 576);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnToHome;
    }
}
