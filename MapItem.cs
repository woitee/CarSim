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

        protected double nearestCar(int dirFrom){
            if(incomCars[dirFrom].Count == 0){
                return double.PositiveInfinity;
            }
            return incomCars[dirFrom].Last().coords.Distance(dispCoords);
        }
        public int getDirOf(MapItem mapItem){
            for (int i = 0; i < 4; i++){
                if(connObjs[i] == mapItem) {return i;}
            }
            return -1; //counts as errorCode
        }
        public virtual void Reset(){
            //resets car queues
            incomCars = new List<Car>[4] {new List<Car>(),new List<Car>(),new List<Car>(),new List<Car>()};
        }
        public void putCarInFrontOf(int dirFrom, Car car1, Car car2){
            //This could be done better, but probably not with this interface with List.
            incomCars[dirFrom].Remove(car1);
            int i = incomCars[dirFrom].IndexOf(car2);
            incomCars[dirFrom].Insert(i,car1);
        }
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
