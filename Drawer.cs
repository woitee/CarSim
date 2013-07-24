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
                        case 'D': case '+':
                            //count roads around, generate number
                            int code = 0;
                            for (int k = 0; k < 4; k++){
                                CoOrds co = new CoOrds(i,j).Add(Simulation.dirs[k]);
                                if(co.isValid() && ((map[co.x,co.y] == '+') || (map[co.x,co.y] == 'D'))){ code += powsOf2[k]; }
                            }
                            g.DrawImage(Properties.Resources.Roads,i*TILESIZE,j*TILESIZE,
                                        new Rectangle(64*code,0,TILESIZE,TILESIZE),GraphicsUnit.Pixel);
                            break;
                        case '.':
                            g.DrawImage(Properties.Resources.Roads,i*TILESIZE,j*TILESIZE,
                                        new Rectangle(1024,0,TILESIZE,TILESIZE),GraphicsUnit.Pixel);
                            break;
                    }
                    
                }
            }
            return bmp;
        }

        public Bitmap DrawCars(List<Car> cars){
            Bitmap bmp = new Bitmap(WIDTH*TILESIZE,HEIGHT*TILESIZE);
            Graphics g = Graphics.FromImage(bmp);
            foreach(Car car in cars){
                g.DrawImage(Properties.Resources.CarS,car.coords.x,car.coords.y,
                    new Rectangle(car.direction*10,0,10,10),GraphicsUnit.Pixel);
            }
            return bmp;
        }
    
        private CoOrds signOffset(int dir){
            switch(dir){
                case 0:
                    return new CoOrds(0, 36);
                case 1:
                    return new CoOrds(0, 0);
                case 2:
                    return new CoOrds(36, 0);
                case 3:
                    return new CoOrds(36, 36);
                default:
                    return new CoOrds(-777,-777);
            }
        }

        public Bitmap DrawSignsAndDepots(Depot[] depots, Sign[,,] signmap){
            Bitmap bmp = new Bitmap(WIDTH*TILESIZE,HEIGHT*TILESIZE);
            Graphics g = Graphics.FromImage(bmp);
            for (int i = 0; i < depots.Length; i++){
                g.DrawImage(Properties.Resources.Depot, depots[i].coords.x*TILESIZE+16, depots[i].coords.y*TILESIZE+16);
            }
            for (int i = 0; i < WIDTH; i++){
                for (int j = 0; j < HEIGHT; j++){
                    for (int k = 0; k < 4; k++){
                        if(signmap[i,j,k] != null){
                            CoOrds toDraw = new CoOrds(i*TILESIZE,j*TILESIZE). Add (signOffset(k));
                            //ToDo: rotate image
                            Bitmap b = new Bitmap(28,28);
                            Graphics.FromImage(b).DrawImage(Properties.Resources.Signs,0,0,
                                        new Rectangle((int)signmap[i,j,k].type*28,0,28,28),GraphicsUnit.Pixel);
                            
                            switch (k){
                                case 0: //right
                                    b.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                    g.DrawImage(b, i*TILESIZE, j*TILESIZE+36);
                                    break;
                                case 1: //down
                                    b.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                    g.DrawImage(b, i*TILESIZE, j*TILESIZE);
                                    break;
                                case 2: //left
                                    b.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                    g.DrawImage(b, i*TILESIZE+36, j*TILESIZE);
                                    break;
                                case 3: //up
                                    g.DrawImage(b, i*TILESIZE+36, j*TILESIZE+36);
                                    break;
                            }
                        }
                    }
                }
            }
            return bmp;
        }
    }
}
