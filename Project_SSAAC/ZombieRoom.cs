using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Project_SSAAC.World;
using Project_SSAAC.GameObjects;
using Project_SSAAC.Managers;
using Project_SSAAC.Properties;
using Project_SSAAC.UI;

namespace Project_SSAAC
{
    public partial class ZombieRoom : UserControl
    {
        private Form1 _main;
        public ZombieRoom(Form1 main)
        {
            InitializeComponent();
            _main = main;

            //  좀비룸 세팅
            Room zombieRoom = new Room(new Point(10, 10), RoomType.Survival, new SizeF(1024, 576));
            zombieRoom.ClearRoom();

            //zombieRoom.AddEnemySpawn()


        }
    }
}
