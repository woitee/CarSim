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

        public Depot(CoOrds coords):base(coords){
            //ToDo
        }

        public int direction{
            get{
                return fromPath.route[0].direction;
            }
        }
    }
}
