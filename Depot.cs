using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Depot : MapItem
    {
        public MapItem connObj;
        public Path fromPath;

        public Depot(CoOrds coords, int index):base(coords,index){
            //ToDo
        }

        public int direction{
            get{
                return fromPath.route[0].direction;
            }
        }
    }
}
