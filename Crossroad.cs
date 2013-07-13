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

        public Crossroad(CoOrds coords):base(coords){
            //ToDo
        }
    }
}
