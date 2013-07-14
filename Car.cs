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
        public Path basicPath; //basic, doesn't use scaling

        public Car(CoOrds coords, float speed, Path path){
            this._coords = coords;
            this.maxSpeed = speed;
            this.basicPath = path;
        }

        public void Tick(){
            //follow path
            //_coords = _coords.Add(CoOrds.fromDir(path.route[0].direction));
        }
    }
}
