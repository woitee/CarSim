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
            //ToDo
        }

        private Car lastPassed = new Car();

        public override bool CanGo(Car car, int dirFrom, int dirTo){
            int toRight = CoOrds.toRightDir(dirFrom);
            int toLeft = CoOrds.toLeftDir(dirFrom);

            if(lastPassed.coords.Distance(this.dispCoords) > 11){
                if(toRight == dirTo){
                    lastPassed = car;
                    return true;
                }
                if(nearestCar(toRight) > 80){
                    if(toLeft != dirTo || nearestCar(toLeft) > 80){
                        lastPassed = car;
                        return true;   
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
