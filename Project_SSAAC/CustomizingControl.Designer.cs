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
            // CustomizingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnBackToHome);
            this.Name = "CustomizingControl";
            this.Size = new System.Drawing.Size(1024, 576);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnBackToHome;
    }
}
