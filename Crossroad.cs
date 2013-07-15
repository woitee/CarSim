using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Crossroad : MapItem
    {
        public MapItem[] connObjs = new MapItem[4];
        public Path[] fromPaths = new Path[4];
        public Queue<Car>[] incomCars = new Queue<Car>[4] {new Queue<Car>(),new Queue<Car>(),new Queue<Car>(),new Queue<Car>()};
        public const int visibleRange = 3; //# of squares you can see an incoming car from

        public Crossroad(CoOrds coords, int index):base(coords,index){
            //ToDo
        }

        public bool CanGo(int dirFrom, int dirTo){
            return true; //TEMP
        }
    }
}
