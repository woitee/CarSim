using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Car
    {
        private CoOrds coords;
        private double maxSpeed;
        private double speed;
        private double accel = 0.02; //acceleration per tick
        public Path path;

        public Car(CoOrds coords, float speed){
            this.coords = coords;
            this.maxSpeed = speed;
        }
    }
}
