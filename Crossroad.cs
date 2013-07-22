using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Crossroad : MapItem
    {

        public Crossroad(CoOrds coords, int index):base(coords,index){
        }

        private Car lastPassed = new Car();
        public bool passBooked = false;

        public override bool CanGo(Car car, int dirFrom, int dirTo){
            int toRight = CoOrds.toRightDir(dirFrom);
            int toLeft = CoOrds.toLeftDir(dirFrom);
            int oppDir = CoOrds.oppDir(dirFrom);

            if(!passBooked && (lastPassed.coords.Distance(this.dispCoords) > 11)){
                if(toRight == dirTo){
                    lastPassed = car;
                    passBooked = true; return true;
                }
                if(nearestCar(toRight) > 80){
                    if(toLeft != dirTo || nearestCar(oppDir) > 80){
                        lastPassed = car;
                        passBooked = true; return true;   
                    }
                }
            }
            return false;
        }

        public override void Reset(){
            base.Reset();
            lastPassed = new Car();
        }
    }
}
