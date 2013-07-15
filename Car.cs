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

        public Car(CoOrds coords, float speed){
            this._coords = coords;
            this.maxSpeed = speed;
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
                X = goal.x; Y = goal.y;
                itinerary.route.Dequeue();
                if(itinerary.route.Count <= 0){
                    return true;
                }
                goal = itinerary.route.First().dest;
                distX = goal.x-X; distY = goal.y-Y;
            }
            X += maxSpeed*Math.Sign(distX);
            Y += maxSpeed*Math.Sign(distY);
            _coords = new CoOrds((int)X,(int)Y);
            return false;
        }
    }
}
