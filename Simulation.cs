using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace CarSim
{
    public struct CoOrds{public int x, y;
            public CoOrds (int x, int y) {this.x = x; this.y = y;}
            public bool Equals(CoOrds other){
                return((this.x == other.x) && (this.y == other.y));
            }
            public CoOrds Add(CoOrds other){
                return (new CoOrds (this.x + other.x, this.y + other.y));
            }
            public CoOrds Subtract(CoOrds other){
                return (new CoOrds (this.x - other.x, this.y - other.y));
            }
            public int toDir(){
                for (int i = 0; i < Simulation.dirs.Length; i++){
                    if (this.Equals(Simulation.dirs[i])){
                        return i;
                    }
                }
                return -1; //counts as ErrorCode
            }
            public static CoOrds fromDir(int dir){
                return (Simulation.dirs[dir]);
            }
    }

    class Simulation
    {
        private const int WIDTH = 20; //number of blocks to go on width
        private const int HEIGHT = 15; //number of blockt to go on height
        private const int TILESIZE = 32; //size of a square in pixels
        public static CoOrds[] dirs = new CoOrds[4] {new CoOrds(1,0),
                                                           new CoOrds(0,1),
                                                           new CoOrds(-1,0),
                                                           new CoOrds(0,-1)};
        
        private char[,] map = new char[WIDTH,HEIGHT];
        private MapItem[,] objmap = new MapItem[WIDTH,HEIGHT]; //only stores crossroads and depots, for quick access

        private Car[] cars;
        private Depot[] depots;
        private Crossroad[] crossroads;
        private Planner planner;
        

        public Bitmap DrawBackground(){
            //ToDo: vyrobit vykreslovani podle mapy
            Bitmap bmp = new Bitmap(WIDTH*TILESIZE,HEIGHT*TILESIZE);
            for (int i = 0; i < WIDTH; i++){
                for (int j = 0; j < HEIGHT; j++){
                    Graphics g = Graphics.FromImage(bmp);
                    g.DrawImage(Properties.Resources.Road,i*TILESIZE,j*TILESIZE);
                }
            }
            return bmp;
        }

        public void Start(){
            //ToDo
            ProcessMap();
        }

        public void Tick(){
            //ToDo
        }

        private void ProcessMap(){
            //creates Depos, Crossroads, and Paths between them
            //ToDo:CrossRoads
            Queue<Depot> depotList=new Queue<Depot>();
            Queue<Crossroad> crossList=new Queue<Crossroad>();
            for (int i = 0; i < HEIGHT; i++){
                for (int j = 0; j < WIDTH; j++){
                    switch (map[j,i]){
                        case 'D':
                            Depot dpt = new Depot(new CoOrds(j,i));
                            depotList.Enqueue(dpt);
                            objmap[j,i]=dpt;
                            break;
                        case '+':
                            int count = 0;
                            for (int k = 0; k < dirs.Length; k++){
                                char c = map[j+dirs[k].x,i+dirs[k].y];
                                if((c == '+') || (c == 'D')) {count++;}
                            }
                            if(count>=3){
                                Crossroad crd = new Crossroad(new CoOrds(j,i));
                                crossList.Enqueue(crd);
                                objmap[j,i]=crd;
                            }
                            break;
                    }
                }
            }
            planner = new Planner(map, objmap);
            //create Depots
            depots = new Depot[depotList.Count];
            for (int i = 0; i < depots.Length; i++){
                Depot dpt = depotList.Dequeue();
                depots[i] = dpt;
                planner.findConnections(dpt);
            }
            //create Crossroads
            crossroads = new Crossroad[crossList.Count];
            for (int i = 0; i < crossroads.Length; i++){
                Crossroad crd = crossList.Dequeue();
                crossroads[i] = crd;
                planner.findConnections(crd);
            }
        }

        public void Load(string path = "save.txt"){
            //loads map from file
            StreamReader sr = new StreamReader(path);
            for (int i = 0; i < HEIGHT; i++){
                string line = sr.ReadLine();
                for (int j = 0; j < WIDTH; j++){
                    map[j,i]=line[j];
                }
            }
            sr.Close();
        }

        public void Save(string path = "save.txt"){
            //saves map to file
            StreamWriter sw = new StreamWriter(path);
            for (int i = 0; i < HEIGHT; i++){
                StringBuilder sb = new StringBuilder("********************");
                for (int j = 0; j < WIDTH; j++){
                    sb[j]=map[j,i];
                }
                sw.WriteLine(sb.ToString());
            }
            sw.Close();
        }
    }
}