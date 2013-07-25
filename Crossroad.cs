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

        private List<Car> lastPasses = new List<Car>(); //list of all passed car with distance from Crossroad of less than 11
        public Car lastPassed{
            set{
                for (int i = 0; i < lastPasses.Count; i++){
                    Car car = lastPasses[i];
                    if(car.coords.Distance(this.dispCoords) > 11) { lastPasses.Remove(car); }
                }
                lastPasses.Add(value);
            }
        }
        private double getLastPassedDist(){
            double tR = double.PositiveInfinity;
            foreach(Car car in lastPasses){
                double dist = car.coords.Distance(this.dispCoords);
                tR = dist < tR ? dist : tR;
            }
            return tR;
        }
        public bool passBooked = false;

        public override bool CanGo(Car car, int dirFrom, int dirTo){
            int toRight = CoOrds.toRightDir(dirFrom);
            int toLeft = CoOrds.toLeftDir(dirFrom);
            int oppDir = CoOrds.oppDir(dirFrom);

            if(getLastPassedDist() > 11){
                if(toRight == dirTo){
                    //lastPassed = car;
                    return true;
                }
                if(nearestCar(toRight) > 80){
                    if(toLeft != dirTo || nearestCar(oppDir) > 80){
                        //lastPassed = car;
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
