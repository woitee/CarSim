using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace CarSim
{
    
    class Drawer
    {
        private char[,] map;
        private const int TILESIZE = Simulation.TILESIZE;
        private const int WIDTH = Simulation.WIDTH;
        private const int HEIGHT = Simulation.HEIGHT;
        private static int[] powsOf2 = {1,2,4,8};

        public Drawer(char[,] map){
            this.map = map;
        }

        public Bitmap DrawBackground(){
            Bitmap bmp = new Bitmap(WIDTH*TILESIZE,HEIGHT*TILESIZE);
            Graphics g = Graphics.FromImage(bmp);
            for (int i = 0; i < WIDTH; i++){
                for (int j = 0; j < HEIGHT; j++){
                    switch (map[i,j]) {
                        case '+':
                            //count roads around, generate number
                            int code = 0;
                            for (int k = 0; k < 4; k++){
                                CoOrds co = new CoOrds(i,j).Add(Simulation.dirs[k]);
                                if(map[co.x,co.y] == '+' || map[co.x,co.y] == 'D'){ code += powsOf2[k]; }
                            }
                            g.DrawImage(Properties.Resources.Roads,i*TILESIZE,j*TILESIZE,
                                        new Rectangle(64*code-1,0,TILESIZE,TILESIZE),GraphicsUnit.Pixel); //ToDo: Figure the -1
                            break;
                        case '.':
                            g.DrawImage(Properties.Resources.Roads,i*TILESIZE,j*TILESIZE,
                                        new Rectangle(1024-1,0,TILESIZE,TILESIZE),GraphicsUnit.Pixel); //ToDo: Figure the -1
                            break;
                    }
                    
                }
            }
            return bmp;
        }

        public Bitmap DrawCars(Car[] cars){
            Bitmap bmp = new Bitmap(WIDTH*TILESIZE,HEIGHT*TILESIZE);
            Graphics g = Graphics.FromImage(bmp);
            for (int i = 0; i < cars.Length; i++){
                g.DrawImage(Properties.Resources.Car,cars[i].coords.x,cars[i].coords.y);
            }
            return bmp;
        }
    }
}
