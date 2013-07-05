using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarSim
{
    public partial class MainForm : Form
    {
        Simulation sim = new Simulation();
        public MainForm()
        {
            InitializeComponent();
            pictureBox.Image = sim.DrawBackground();
            sim.Load();
            sim.Save("save2.txt");
        }
    }
}
