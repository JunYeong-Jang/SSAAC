namespace Project_SSAAC
{
    partial class Form1
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

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450); // 기본값, Form1 생성자에서 변경됨
            this.Text = "Form1"; // 기본값, Form1 생성자에서 변경됨
            // Form1_Paint 이벤트 핸들러는 Form1.cs의 InitializeGame에서 연결되거나,
            // 여기서 직접 this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint); 로 추가.
            // 현재 제공된 코드에서는 Designer.cs에서 Paint 이벤트를 명시적으로 추가하고 있지 않으나,
            // WinForms 디자이너 사용 시 자동으로 추가되는 경우가 많음.
            // 명시적으로 추가하려면 아래 줄의 주석을 해제합니다.
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
        }

        #endregion
    }
}