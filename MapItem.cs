using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    abstract class MapItem
    {
        protected CoOrds _coords;
        public MapItem(CoOrds coords){
            this._coords = coords;
        }

        public CoOrds coords{
            get {
                return _coords;
            }
        }
    }
}
