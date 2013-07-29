using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Globalization;

namespace CarSim
{
    
    /// <summary>
    /// Simulation class, contains all information and processes simulation steps.
    /// Call its public functions to initiate and proceed simulation. Returns images of current state.
    /// </summary>
    class Simulation
    {
        public const int WIDTH = 10; //number of blocks on width
        public const int HEIGHT = 8; //number of blocks on height
        public const int TILESIZE = 64; //size of a square in pixels
        public const int FRAMERATE = 60; //default framerate

        public static CoOrds[] dirs = new CoOrds[4] {new CoOrds(1,0),
                                                           new CoOrds(0,1),
                                                           new CoOrds(-1,0),
                                                           new CoOrds(0,-1)};
        
        private char[,] map = new char[WIDTH,HEIGHT];
        private MapItem[,] objmap = new MapItem[WIDTH,HEIGHT]; //only stores crossroads and depots, for quick access
        private Sign[,,] signmap = new Sign[WIDTH, HEIGHT, 4]; //stores info about signs

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
        
        /// <summary>
        /// Draws background, based on the objects in the simulation.
        /// </summary>
        /// <returns>A bitmap containing the background.</returns>
        public Bitmap DrawBackground(){
            drawer = new Drawer(map);
            return drawer.DrawBackground();
        }

        /// <summary>
        /// Draws signs and depots, meant to be used as an overlay to cars.
        /// </summary>
        /// <returns>Bitmap containing signs and depots.</returns>
        public Bitmap DrawSignsAndDepots(){
            return drawer.DrawSignsAndDepots(depots,signmap);
        }

        /// <summary>
        /// Resets and starts the simulation.
        /// </summary>
        public void Start(){
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

        /// <summary>
        /// Processes everything needed to advance one tick.
        /// </summary>
        /// <param name="carsBitmap">Outputs the bitmap of cars.</param>
        /// <param name="totalTicks">Outputs number of ticks elapsed since start.</param>
        /// <returns>Boolean value, whether the simulation has finished.</returns>
        public bool Tick(out Bitmap carsBitmap, out long totalTicks){
            activeCars.RemoveAll(car => car.Tick()); //main simulation step hidden here
            while ((carStarts.Length > nextCar) && (carStarts[nextCar] <= time)){ //insert new cars
                
                Car car = cars[nextCar++].Clone();
                MapItem srcDepot = objmap[car.from.x, car.from.y];
                MapItem mi = srcDepot.connObjs[car.direction/3];
                car.inFront = mi.incomCars[ mi.getDirOf(srcDepot) ].LastOrDefault();
                car.setCross(mi, mi.getDirOf(srcDepot) );
                mi.incomCars[ mi.getDirOf(srcDepot) ].Add(car);
                
                activeCars.Add(car);
            }
            carsBitmap = drawer.DrawCars(activeCars);
            time++;
            
            totalTicks = time;
            return (activeCars.Count == 0 && nextCar == cars.Length);
        }

        /// <summary>
        /// Creates Depots and Crossroad objects, and sets Paths between them.
        /// Takes into account signs, make sure signmap is set.
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
                                for (int k = 0; k < 4; k++){
                                    if(signmap[j,i,k] != null){
                                        switch(signmap[j,i,k].type){
                                            case SignType.Mainway:
                                            case SignType.Giveway:
                                            case SignType.Stop:
                                                int newk = CoOrds.oppDir(k);
                                                crd.priorities[newk] = signmap[j,i,k].type;
                                                break;
                                        }
                                    }
                                }
                                objmap[j,i]=crd;
                            }
                            break;
                    }
                }
            }
            planner = new Planner(map, objmap, signmap);
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

        /// <summary>
        /// Loads map from file. Processes everything needed in advance, start can be called immediately after.
        /// Traces out parse errors.
        /// </summary>
        /// <param name="path">Path to a file to be loaded.</param>
        /// <returns>Boolean value, if the file has been loaded sucessfully.</returns>
        public bool Load(string path = "save.txt"){
            //loads map from file
            //initialize
            activeCars = new List<Car>();
            map = new char[WIDTH,HEIGHT];
            objmap = new MapItem[WIDTH,HEIGHT];
            //process
            string line;
            StreamReader sr;
            try{
                sr = new StreamReader(path);
            } catch(Exception){
                tracer.Trace("Loading error: File invalid. Does the file exist?");
                return false;
            }
            for (int i = 0; i < HEIGHT; i++){
                line = sr.ReadLine();
                if(line == null || line.Length != WIDTH){
                    tracer.Trace("Loading error: Incorrect map dimensions. Make sure the map is "+WIDTH+"x"+HEIGHT+" characters.");
                    return false;
                }
                for (int j = 0; j < WIDTH; j++){
                    if("+D.".IndexOf(line[j]) == -1) {
                        tracer.Trace("Loading error: Invalid map character: "+line[j]+". The map must consist of only \"+D.\" characters.");
                        return false;
                    }
                    map[j,i]=line[j];
                }
            }
            if (sr.ReadLine() != "===SIGNS==="){
                tracer.Trace("Loading error: Line \"===SIGNS===\" must immediately follow map description.");
                return false;
            }
            line = sr.ReadLine();
            signmap = new Sign[WIDTH, HEIGHT, 4];
            while(line != "===CARS==="){
                if(sr.EndOfStream){
                    tracer.Trace("Loading error: Line \"===CARS===\" expected before end of file.");
                    return false;
                }
                try{
                    string[] arr = line.Split(' ');
                    //Type X Y direction
                    Sign sign = new Sign(Sign.typeString(arr[0]));
                    if(sign.type == SignType.Undefined){
                        tracer.Trace("Loading error: Invalid sign keyword: "+arr[0]);
                        return false;
                    }
                    int signX = int.Parse(arr[1]);
                    int signY = int.Parse(arr[2]);
                    int signDir = CoOrds.dirFromString(arr[3]);
                    signmap[signX,signY,signDir] = sign;
                    line = sr.ReadLine();
                } catch(Exception){
                    tracer.Trace("Loading error: Invalid description of sign: \""+line+"\"");
                    return false;
                }
            }
            ProcessMap();
            List<Car> stockCars = new List<Car>();
            List<int> starts = new List<int>();
            while(!sr.EndOfStream){
                try{
                    line = sr.ReadLine();
                    string[] arr = line.Split(' ');
                    //From(DepotIndex) To(DepotIndex) Speed(double) TimeStart
                    Car car = new Car( double.Parse(arr[2], CultureInfo.InvariantCulture)/145 );
                    int arg1 = int.Parse(arr[0]);
                    int arg2 = int.Parse(arr[1]);
                    if(arg1 == arg2){
                        tracer.Trace("Loading Error: Destination depot has to be different from source depot.");
                        return false;
                    }
                    Path pth = planner.FindPath(car,depots[arg1],depots[arg2]);
                    if (pth == null){
                        tracer.Trace("Loading Error: Couldn't find valid path between depot "+arg1+" and "+arg2);
                        return false;
                    }
                    car.path = pth;
                    stockCars.Add(car);
                    starts.Add( (int)Math.Round( double.Parse(arr[3],CultureInfo.InvariantCulture)*FRAMERATE ));
                } catch (Exception) {
                    tracer.Trace("Loading error: Invalid description of car: \""+line+"\"");
                    return false;
                }
            }
            cars = stockCars.ToArray();
            carStarts = starts.ToArray();
            sr.Close();
            tracer.Trace("Map loaded.");
            return true;
        }

        //Saving not implemented, modifying map at runtime is not yet allowed
        //Function not finished
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
                string ind3 = (cars[i].maxSpeed*145).ToString().Replace(',','.');
                double ind4 = ((double)carStarts[i])/FRAMERATE;
                sw.WriteLine("{0} {1} {2} {3}",ind1,ind2,ind3,carStarts[i]/FRAMERATE);
            }
            sw.Close();
            tracer.Trace("Map saved.");
        }
    }
}