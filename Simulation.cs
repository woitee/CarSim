using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace CarSim
{
    class Simulation
    {
        private Car[] cars;
        private char[,] arr = new char[20,15];

        public Bitmap DrawBackground(){
            Bitmap bmp = new Bitmap(640,480);
            for (int i = 0; i < 20; i++){
                for (int j = 0; j < 15; j++){
                    Graphics g = Graphics.FromImage(bmp);
                    g.DrawImage(Properties.Resources.Road,i*32,j*32);
                }
            }
            return bmp;
        }

        public void Load(string path = "save.txt"){
            //save map
            StreamReader sr = new StreamReader(path);
            for (int i = 0; i < 15; i++){
                string line = sr.ReadLine();
                for (int j = 0; j < 20; j++){
                    arr[j,i]=line[j];
                }
            }
            sr.Close();
        }

        public void Save(string path = "save.txt"){
            //save map
            StreamWriter sw = new StreamWriter(path);
            for (int i = 0; i < 15; i++){
                StringBuilder sb = new StringBuilder("********************");
                for (int j = 0; j < 20; j++){
                    sb[j]=arr[j,i];
                }
                sw.WriteLine(sb.ToString());
            }
            sw.Close();
        }
    }
}