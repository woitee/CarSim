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
        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            sim.Tick();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            sim.Start();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            sim.Load();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            sim.Save();
        }
    }
}
