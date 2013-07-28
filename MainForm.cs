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
            //TEMP
            sim.Load("save.txt");
            pbBackground.Image = sim.DrawBackground();
            pbOverlay.Image = sim.DrawSignsAndDepots();
        }


        private void mainTimer_Tick(object sender, EventArgs e)
        {
            Bitmap bmp; int ticks;
            if (sim.Tick(out bmp, out ticks)){
                mainTimer.Stop();
                System.Windows.Forms.MessageBox.Show("Simulation finished after "+ticks+
                                                     " ticks ("+Math.Round( (double)ticks/65,1 )+" seconds at 65 fps).");

                stopToolStripMenuItem.Enabled = false;
            }
            pbCars.Image = bmp;

            FPSlabel.Text = "FPS: "+FPSCounter.Tick();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e){
            OpenFileDialog ofDialog = new OpenFileDialog();
            ofDialog.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            ofDialog.FilterIndex = 1;

            if (ofDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK){
                sim.Load(ofDialog.FileName);
                pbBackground.Image = sim.DrawBackground();
                pbOverlay.Image = sim.DrawSignsAndDepots();
                stopToolStripMenuItem.Enabled = false;
                paused = false;
                pbCars.Image = new Bitmap(pbBackground.Width, pbBackground.Height);
                mainTimer.Stop();
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e){
            sim.Start();
            mainTimer.Start();
            stopToolStripMenuItem.Enabled = true;
            paused = false;
        }

        private bool _paused = false;
        private bool paused{
            set{
                if(value){
                    stopToolStripMenuItem.Text = "Unpause";
                } else {
                    stopToolStripMenuItem.Text = "Stop";
                }
                _paused = value;
            }
            get{
                return _paused;
            }
        }
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(paused){
                mainTimer.Start();
                paused = false;
            } else {
                mainTimer.Stop();
                paused = true;
            }
        }
    }
}
