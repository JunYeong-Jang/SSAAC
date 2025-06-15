using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Project_SSAAC.GameObjects;
using Project_SSAAC.UI;

namespace Project_SSAAC
{
    public partial class testroom : UserControl
    {
        private Form1 _main;
        public testroom(Form1 main)
        {
            InitializeComponent();
            _main = main;

            // 임시 주석
            //_main.LoadCharacterTo(this);
        }
    }
}
