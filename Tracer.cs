using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarSim
{
    class Tracer
    {
        private Label label = new Label(); //to not cause errors if not set
        private Queue<string> cache = new Queue<string>();
        private Timer timer = new Timer();

        public string output {
            set{
                label.Text = value;
            }
        }

        public Tracer(){}
        public Tracer(Label output){
            this.label = output;
            output.Text = "";
            timer.Interval = 750;
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, EventArgs e){
            if (cache.Count == 0){
                timer.Stop();
            } else {
                output = cache.Dequeue();
            }
        }

        public void Trace(string text){
            if(timer.Enabled){
                cache.Enqueue(text);
            } else {
                output = text;
                timer.Start();
            }
        }
    }
}