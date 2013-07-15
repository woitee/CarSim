using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Car
    {
        private const int TILESIZE = Simulation.TILESIZE;
        private CoOrds _coords;
        public CoOrds coords{
            get{ return _coords; }
        }
        private double maxSpeed;
        private double speed;
        private double accel = 0.02; //acceleration per tick
        private double decel = 0.02; //deccelartion per tick
        private double X;
        private double Y;

        private Path basicPath; //basic, doesn't use scaling
        private Itinerary itinerary;

        public Path path{
            get{return basicPath;}
            set{
                basicPath = value;
                _coords = value.route[0].from.Multiply(TILESIZE);
                X=_coords.x; Y=_coords.y;
                MakeItinerary();
            }
        }

        public Car(double speed){
            this.maxSpeed = speed;
        }

        public Car(double speed, double X, double Y, CoOrds coords, Path path, Itinerary itinerary){
            this.maxSpeed = speed;
            this.X = X;
            this.Y = Y;
            this.basicPath = path;
            this._coords = coords;
            this.itinerary = itinerary;
        }

        public void MakeItinerary(){
            //ToDo: make it real
            Queue<ItinPart> qu = new Queue<ItinPart>();
            for (int i = 0; i < path.route.Length; i++){
                if(path.route[i].type == PathPart.Type.Straight){
                    qu.Enqueue(new ItinPart(ItinType.GoTo, path.route[i].to.Multiply(TILESIZE), maxSpeed));
                } else {
                    qu.Enqueue(new ItinPart(ItinType.GoTo, path.route[i].to.Multiply(TILESIZE), maxSpeed));
                }
            }
            itinerary = new Itinerary(qu);
        }

        public bool Tick(){ //returns if car has finished moving
            //follow path
            double distToTravel = maxSpeed;
            CoOrds goal = itinerary.route.First().dest;
            //Manhattan distance, as being the simplest, but the points differ only in x or y
            double distX = goal.x-X; double distY = goal.y-Y;
            while(distToTravel > Math.Abs(distX)+Math.Abs(distY)){
                if(itinerary.route.Count <= 1){
                    return true;
                }
                X = goal.x; Y = goal.y;
                distToTravel -= Math.Abs(distX)+Math.Abs(distY);
                itinerary.route.Dequeue();
                goal = itinerary.route.First().dest;
                distX = goal.x-X; distY = goal.y-Y;
            }
            X += distToTravel*Math.Sign(distX);
            Y += distToTravel*Math.Sign(distY);
            _coords = new CoOrds((int)X,(int)Y);
            return false;
        }

        public Car Clone(){
            return new Car(maxSpeed,X,Y,coords,basicPath,itinerary.Clone());
        }
    }
}
