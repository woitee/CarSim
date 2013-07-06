using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    abstract class MapItem
    {
        protected CoOrds coords;
        public MapItem(CoOrds coords){
            this.coords = coords;
        }
    }
}
