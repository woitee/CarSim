using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarSim
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Bitmap bmp = new Bitmap(640,480);
            pictureBox.Image=bmp;
            for (int i = 0; i < 20; i++){
                for (int j = 0; j < 15; j++){
                    Graphics g = Graphics.FromImage(bmp);
                    g.DrawImage(Properties.Resources.Road,i*32,j*32);
                }
            }
        }
    }
}
