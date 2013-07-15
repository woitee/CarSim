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
        protected int _index;
        public MapItem(CoOrds coords, int index){
            this._coords = coords;
            this._index = index;
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
    }
}
