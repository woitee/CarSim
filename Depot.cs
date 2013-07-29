using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    /// <summary>
    /// An item cars go from, and end at. Extends MapItem.
    /// </summary>
    class Depot : MapItem
    {

        public Depot(CoOrds coords, int index):base(coords,index){
        }

        public override bool CanGo(Car car, int dirFrom, int dirTo){
            //throw new NotImplementedException();
            return true;
        }
    }
}
