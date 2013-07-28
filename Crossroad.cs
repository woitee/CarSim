using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    /// <summary>
    /// An item the cars will pass through to get to their goal.
    /// Lists incoming cars and judges whether they can pass the crossroad.
    /// </summary>
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

        public SignType[] priorities = new SignType[4] {SignType.Undefined,SignType.Undefined,SignType.Undefined,SignType.Undefined};

        private int priority (SignType st){
            switch (st){
                case SignType.Stop:
                case SignType.Giveway:
                    return 0;
                case SignType.Undefined:
                    return 1;
                case SignType.Mainway:
                    return 2;
            }
            return -1;
        }
        private bool hasPriority(int dir1, int dir2){ //returns true if sign at dir1 has higher priority than dir2
            return priority(priorities[dir1]) > priority(priorities[dir2]);
        }

        /// <summary>
        /// Asks whether a car can pass the crossroad.
        /// </summary>
        /// <param name="car">The car, that will pass.</param>
        /// <param name="dirFrom">Direction the car is coming from.</param>
        /// <param name="dirTo">Direction the car is going to.</param>
        /// <returns>Boolean value, whether the car can pass.</returns>
        public override bool CanGo(Car car, int dirFrom, int dirTo){
            if (priorities[dirFrom] == SignType.Stop && (car.speed > 0.03 || car.coords.Distance(dispCoords) > 15)) {return false;} 
            if (nearestCar(dirFrom) != car) {return false;}

            int toLeft = CoOrds.toLeftDir(dirFrom);
            int oppDir = CoOrds.oppDir(dirFrom);
            int toRight = CoOrds.toRightDir(dirFrom);

            
            bool[] crossesPaths = new bool[3] {false,false,false};

            #region logic branches of who to let go
            if (getLastPassedDist() > 11){
                if(dirTo == toRight){ //going right
                    if(
                        (hasPriority(toLeft, dirFrom) && nearestCarDist(toLeft) < 80 && nearestCar(toLeft).willTurn() == 1) ||
                        (hasPriority(oppDir, dirFrom) && nearestCarDist(oppDir) < 80 && nearestCar(oppDir).willTurn() == 0)
                      ){
                        return false;
                    } else {
                        return true;
                    }
                } else if (dirTo == dirFrom){ //going straight
                    if(
                        (!hasPriority(dirFrom, toRight) && nearestCarDist(toRight) < 80) ||
                        (hasPriority(toLeft, dirFrom) && nearestCarDist(toLeft) < 80 && nearestCar(toLeft).willTurn() != 2) ||
                        (hasPriority(oppDir, dirFrom) && nearestCarDist(oppDir) < 80 && nearestCar(oppDir).willTurn() == 0)
                      ){
                        return false;
                    } else {
                        return true;
                    }
                } else if(dirTo == toLeft){ //going left
                    if(
                        (!hasPriority(dirFrom, toRight) && nearestCarDist(toRight) < 80) ||
                        (!hasPriority(dirFrom, oppDir) && nearestCarDist(oppDir) < 80 && nearestCar(oppDir).willTurn() != 0) ||
                        (hasPriority(toLeft, dirFrom) && nearestCarDist(toLeft) < 80 && nearestCar(toLeft).willTurn() != 2)
                      ){
                        return false;
                    } else {
                        return true;
                    }
                }
            }
            #endregion
            return false;
        }

        /// <summary>
        /// Resets the crossroad, needs to be done when loading a new simulation.
        /// </summary>
        public override void Reset(){
            base.Reset();
            lastPasses = new List<Car>();
        }
    }
}
