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
        PictureBox pbCars = new PictureBox();
        PictureBox pbOverlay = new PictureBox();

        public Bitmap CarsImage {
            get {return (Bitmap)pbCars.Image;} //to draw on
        }
        public MainForm()
        {
            InitializeComponent();
            
            //Creating Car layer
            pbCars.Width = pbBackground.Width; pbCars.Height = pbBackground.Height;
            pbCars.BackColor = Color.Transparent;
            pbCars.Parent = pbBackground;
            //Creating Overlay layer
            pbOverlay.Width = pbBackground.Width; pbOverlay.Height = pbBackground.Height;
            pbOverlay.BackColor = Color.Transparent;
            pbOverlay.Parent = pbCars;

            sim.tracer = new Tracer(infoLabel);
            sim.Load();
            pbBackground.Image = sim.DrawBackground();
            pbOverlay.Image = sim.DrawSignsAndDepots();
        }


        private void mainTimer_Tick(object sender, EventArgs e)
        {
            Bitmap bmp;
            sim.Tick(out bmp);
            pbCars.Image = bmp;

            FPSlabel.Text = "FPS: "+FPSCounter.Tick();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            sim.Start();
            mainTimer.Start();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            sim.Load();
            pbBackground.Image = sim.DrawBackground();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            sim.Save();
        }
    }
}
