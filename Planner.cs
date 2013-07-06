using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Planner
    {
        private char[,] map;
        private MapItem[,] objmap;

        public Planner(char[,] map, MapItem[,] objmap){
            this.map = map;
            this.objmap = objmap;
        }

        public void findConnections(MapItem mapitem){
            Depot dpt = mapitem as Depot;
            if (dpt != null){
                //ToDo: Find connObj and path
                return;
            }
            Crossroad crd = mapitem as Crossroad;
            if (crd != null){ //this should pass
                //ToDo: Find connObjs[] and paths[]
                return;
            }
        }
    }
}
