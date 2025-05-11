using System;
using System.Windows.Forms;
using System.Diagnostics; // Debug 클래스 사용을 위해 추가

namespace Project_SSAAC
{
    /// <summary>
    /// 애플리케이션의 주 진입점 클래스입니다.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Debug.WriteLine("[Program.cs] Main - Running Form1..."); // 디버그 로그 추가
            try
            {
                Application.Run(new Form1()); // 메인 폼 실행
            }
            catch (Exception ex)
            {
                // 최상위 수준에서 예외 처리 (애플리케이션 실행 자체의 오류)
                MessageBox.Show($"애플리케이션 실행 중 치명적인 오류 발생: {ex.Message}\n\n{ex.StackTrace}",
                                "애플리케이션 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"[Program.cs] Main - APPLICATION CRITICAL ERROR: {ex.ToString()}");
            }
            Debug.WriteLine("[Program.cs] Main - Application.Run finished or an unhandled exception occurred before this point.");
        }
    }
}