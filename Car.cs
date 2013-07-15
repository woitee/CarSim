using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Car
    {
        private CoOrds _coords;
        public CoOrds coords{
            get{ return _coords; }
        }
        private double maxSpeed;
        private double speed;
        private double accel = 0.02; //acceleration per tick
        private Path basicPath; //basic, doesn't use scaling
        private Itinerary itinerary;
        public Path path{
            get{return basicPath;}
            set{
                basicPath = value;
                _coords = value.route[0].from;
            }
        }

        public Car(CoOrds coords, float speed){
            this._coords = coords;
            this.maxSpeed = speed;
        }

        public void MakeItinerary(){
            //ToDo:
            
        }

        public void Tick(){
            //follow path
            //_coords = _coords.Add(CoOrds.fromDir(path.route[0].direction));
        }
    }
}
