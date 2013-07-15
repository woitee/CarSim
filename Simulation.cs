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
        public const int WIDTH = 10; //number of blocks on width
        public const int HEIGHT = 8; //number of blocks on height
        public const int TILESIZE = 64; //size of a square in pixels

        public static CoOrds[] dirs = new CoOrds[4] {new CoOrds(1,0),
                                                           new CoOrds(0,1),
                                                           new CoOrds(-1,0),
                                                           new CoOrds(0,-1)};
        
        private char[,] map = new char[WIDTH,HEIGHT];
        private MapItem[,] objmap = new MapItem[WIDTH,HEIGHT]; //only stores crossroads and depots, for quick access

        private List<Car> cars = new List<Car>();
        private Depot[] depots;
        private Crossroad[] crossroads;
        private Planner planner;
        private Drawer drawer;

        public Bitmap DrawBackground(){
            drawer = new Drawer(map);
            return drawer.DrawBackground();
        }

        public void Start(){
            //ToDo
        }

        public void Tick(out Bitmap carsBitmap){
            //ToDo
            cars.RemoveAll(car => car.Tick());
            /*foreach(Car car in cars){ 
                if (car.Tick()) {}
            }*/
            carsBitmap = drawer.DrawCars(cars);
        }

        private void ProcessMap(){
            //creates Depos, Crossroads, and Paths between them
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
                planner.FindConnections(dpt);
            }
            //create Crossroads
            crossroads = new Crossroad[crossList.Count];
            for (int i = 0; i < crossroads.Length; i++){
                Crossroad crd = crossList.Dequeue();
                crossroads[i] = crd;
                planner.FindConnections(crd);
            }
        }

        public void Load(string path = "save.txt"){
            //loads map from file
            string line;
            StreamReader sr = new StreamReader(path);
            for (int i = 0; i < HEIGHT; i++){
                line = sr.ReadLine();
                for (int j = 0; j < WIDTH; j++){
                    map[j,i]=line[j];
                }
            }
            ProcessMap();
            line = sr.ReadLine();
            while(line != "===CARS==="){
                //ToDo: load signs
                line = sr.ReadLine();
            }
            while(!sr.EndOfStream){
                //ToDo: load cars 
                line = sr.ReadLine();
            }
            sr.Close();
            //TEMP
            Car car = new Car(new CoOrds(20,20),5);
            car.path = planner.FindPath(car,depots[0],depots[1]);
            cars.Add(car);
        }

        public void Save(string path = "save.txt"){
            //saves map to file
            StreamWriter sw = new StreamWriter(path);
            for (int i = 0; i < HEIGHT; i++){
                StringBuilder sb = new StringBuilder(WIDTH);
                for (int j = 0; j < WIDTH; j++){
                    sb[j]=map[j,i];
                }
                sw.WriteLine(sb.ToString());
            }
            sw.WriteLine("===SIGNS===");
            //ToDo: Write Signs
            sw.WriteLine("===CARS===");
            //ToDo: Write Cars
            sw.Close();
        }
    }
}