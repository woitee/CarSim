using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    /// <summary>
    /// An abstract class that Depots and Crossroads derive from.
    /// </summary>
    abstract class MapItem
    {
        protected CoOrds _coords;
        protected int _index;
        public MapItem[] connObjs = new MapItem[4];
        public Path[] fromPaths = new Path[4];
        public List<Car>[] incomCars = new List<Car>[4] {new List<Car>(),new List<Car>(),new List<Car>(),new List<Car>()};
        protected CoOrds dispCoords; //actual pixel-on-screen coordinates

        public MapItem(CoOrds coords, int index){
            this._coords = coords;
            this._index = index;
            this.dispCoords = coords.Multiply(Simulation.TILESIZE).Add(new CoOrds(27,27));
        }

        public CoOrds coords{
            get {
                return _coords;
            }
        }
        public int index{
            get {
                return _index;
            }
        }

        /// <summary>
        /// Returns distance from nearest car in a specific direction.
        /// </summary>
        /// <param name="dirFrom">Direction the car is coming from.</param>
        /// <returns>Real distance from the car.</returns>
        protected double nearestCarDist(int dirFrom){
            if(incomCars[dirFrom].Count == 0){
                return double.PositiveInfinity;
            }
            return incomCars[dirFrom].First().coords.Distance(dispCoords);
        }
        /// <summary>
        /// Returns distance from the nearest car in a specified direction.
        /// </summary>
        /// <param name="dirFrom">Direction the car is coming from.</param>
        /// <returns>Car object.</returns>
        protected Car nearestCar(int dirFrom){
            if(incomCars[dirFrom].Count == 0){
                return null;
            }
            return incomCars[dirFrom].First();
        }
        /// <summary>
        /// Returns direction of a MapItem a road leads directly to.
        /// </summary>
        /// <param name="mapItem">The requested MapItem.</param>
        /// <returns>0-3 integer value of right, down, left, up; respectively.</returns>
        public int getDirOf(MapItem mapItem){
            for (int i = 0; i < 4; i++){
                if(connObjs[i] == mapItem) {return i;}
            }
            return -1; //counts as errorCode
        }
        /// <summary>
        /// Return ending direction of outgoing path.
        /// </summary>
        /// <param name="dir">Direction of the outgoing path.</param>
        /// <returns>Direction, in which the part ends.</returns>
        public int getEndDir(int dir){
            return fromPaths[dir].route.Last().direction;
        }
        public virtual void Reset(){
            //resets car queues
            incomCars = new List<Car>[4] {new List<Car>(),new List<Car>(),new List<Car>(),new List<Car>()};
        }
        /// <summary>
        /// Places one car in front of another in internal queues. Useful when passing.
        /// </summary>
        /// <param name="dirFrom">Direction the cars are coming from.</param>
        /// <param name="car1">Car to move forward..</param>
        /// <param name="car2">Car the first will be put in front of.</param>
        public void putCarInFrontOf(int dirFrom, Car car1, Car car2){
            //This could be done better, but probably not with this interface with List.
            incomCars[dirFrom].Remove(car1);
            int i = incomCars[dirFrom].IndexOf(car2);
            incomCars[dirFrom].Insert(i,car1);
        }

        /// <summary>
        /// Returns car, thats behind another car, that are driving towards this MapItem.
        /// </summary>
        /// <param name="dirFrom">Direction both cars are coming from.</param>
        /// <param name="car">The car in front.</param>
        /// <returns></returns>
        public Car getCarBehind(int dirFrom, Car car){
             int i = incomCars[dirFrom].IndexOf(car);
             if (incomCars[dirFrom].Count > i+1 ){
                 return incomCars[dirFrom][i+1];
             }
             return null;
         }


        public abstract bool CanGo(Car car, int dirFrom, int dirTo);
    }
}
