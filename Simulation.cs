using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Globalization;

namespace CarSim
{
    
    class Simulation
    {
        public const int WIDTH = 10; //number of blocks on width
        public const int HEIGHT = 8; //number of blocks on height
        public const int TILESIZE = 64; //size of a square in pixels
        public const int FRAMERATE = 60; //default framerate, if changing, change mainTimer

        public static CoOrds[] dirs = new CoOrds[4] {new CoOrds(1,0),
                                                           new CoOrds(0,1),
                                                           new CoOrds(-1,0),
                                                           new CoOrds(0,-1)};
        
        private char[,] map = new char[WIDTH,HEIGHT];
        private MapItem[,] objmap = new MapItem[WIDTH,HEIGHT]; //only stores crossroads and depots, for quick access

        private List<Car> activeCars = new List<Car>();
        private Car[] cars;
        private int nextCar;
        private int[] carStarts;

        private Depot[] depots;
        private Crossroad[] crossroads;
        private Planner planner;
        private Drawer drawer;
        public Tracer tracer = new Tracer();

        private static int time;
        public static int Time{
            get {return time;}
        }

        public Bitmap DrawBackground(){
            drawer = new Drawer(map);
            return drawer.DrawBackground();
        }

        public void Start(){
            //ToDo
            time = 0; nextCar = 0;
            activeCars = new List<Car>();
            foreach (MapItem mi in depots){
                mi.Reset();
            }
            foreach (MapItem mi in crossroads){
                mi.Reset();
            }
            tracer.Trace("Simulation started.");
        }

        public void Tick(out Bitmap carsBitmap){
            activeCars.RemoveAll(car => car.Tick()); //main simulation step hidden here
            while ((carStarts.Length > nextCar) && (carStarts[nextCar] <= time)){ //insert new cars
                Car car = cars[nextCar++].Clone();
                MapItem srcDepot = objmap[car.from.x, car.from.y];
                MapItem mi = srcDepot.connObjs[car.direction/3];
                car.cross = mi; mi.incomCars[ mi.getDirOf(srcDepot) ].Add(car);
                
                activeCars.Add(car);
            }
            carsBitmap = drawer.DrawCars(activeCars);
            time++;
        }

        /// <summary>
        /// Creates Depots and Crossroad objects, and sets Paths between them.
        /// </summary>
        private void ProcessMap(){
            Queue<Depot> depotList=new Queue<Depot>();
            Queue<Crossroad> crossList=new Queue<Crossroad>();
            for (int i = 0; i < HEIGHT; i++){
                for (int j = 0; j < WIDTH; j++){
                    switch (map[j,i]){
                        case 'D':
                            Depot dpt = new Depot(new CoOrds(j,i),depotList.Count);
                            depotList.Enqueue(dpt);
                            objmap[j,i]=dpt;
                            break;
                        case '+':
                            int count = 0;
                            for (int k = 0; k < dirs.Length; k++){
                                CoOrds co = new CoOrds(j+dirs[k].x, i+dirs[k].y);
                                if( co.isValid() ){
                                    char c = map[j+dirs[k].x,i+dirs[k].y];
                                    if((c == '+') || (c == 'D')) {count++;}
                                }
                            }
                            if(count>=3){
                                Crossroad crd = new Crossroad(new CoOrds(j,i),crossList.Count);
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
            sr.ReadLine(); line = sr.ReadLine();
            while(line != "===CARS==="){
                //ToDo: load signs
                line = sr.ReadLine();
            }
            List<Car> stockCars = new List<Car>();
            List<int> starts = new List<int>();
            do {
                line = sr.ReadLine();
                string[] arr = line.Split(' ');
                //From(DepotIndex) To(DepotIndex) Speed(double) TimeStart
                Car car = new Car( double.Parse(arr[2], CultureInfo.InvariantCulture) );
                car.path = planner.FindPath(car,depots[int.Parse(arr[0])],depots[int.Parse(arr[1])]);
                stockCars.Add(car);
                starts.Add(int.Parse(arr[3])*FRAMERATE);
            } while(!sr.EndOfStream);
            cars = stockCars.ToArray();
            carStarts = starts.ToArray();
            sr.Close();
            tracer.Trace("Map loaded.");
        }

        public void Save(string path = "save.txt"){
            //saves map to file
            StreamWriter sw = new StreamWriter(path);
            for (int i = 0; i < HEIGHT; i++){
                StringBuilder sb = new StringBuilder(WIDTH);
                for (int j = 0; j < WIDTH; j++){
                    sb.Append(map[j,i]);
                }
                sw.WriteLine(sb.ToString());
            }
            sw.WriteLine("===SIGNS===");
            //ToDo: Write Signs
            sw.WriteLine("===CARS===");
            for (int i = 0; i < cars.Length; i++){
                int ind1 = objmap[cars[i].from.x,cars[i].from.y].index;
                int ind2 = objmap[cars[i].to.x,cars[i].to.y].index;
                sw.WriteLine("{0} {1} {2} {3}",ind1,ind2,cars[i].maxSpeed,carStarts[i]/FRAMERATE);
            }
            sw.Close();
            tracer.Trace("Map saved.");
        }
    }
}