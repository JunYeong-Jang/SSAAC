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
    public partial class HomeControl : UserControl
    {

        private Form1 _main;

        public HomeControl(Form1 main)
        {
            InitializeComponent();
            _main = main;
        }

        private void btnCm_Click(object sender, EventArgs e)
        {
            _main.LoadControl(new CustomizingControl(_main));
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            _main.Close();
        }
    }
}
