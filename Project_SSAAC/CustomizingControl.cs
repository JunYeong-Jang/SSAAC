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
    public partial class CustomizingControl : UserControl
    {
        //  테스트용 

        private Form1 _main;

        public CustomizingControl(Form1 main)
        {
            InitializeComponent();
            _main = main;
        }

        private void btnBackToHome_Click(object sender, EventArgs e)
        {
            _main.LoadControl(new HomeControl(_main));
        }
    }
}
