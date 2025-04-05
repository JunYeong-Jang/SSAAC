using System;
using System.Windows.Forms;

namespace Project_SSAAC // 네임스페이스 확인!
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); // Form1 실행
        }
    }
}