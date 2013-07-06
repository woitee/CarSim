using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    //the path consists of parts of either going straight forward, or turning
    //we chose a representation considering this
    class Path
    {
        public PathPart[] route;
    }

    struct PathPart
    {
        enum Type {StraightH,StraightV,Turn}
        Type type;
        bool crossroad;
        
        public PathPart(Type type, bool isCrossroad){
            this.type = type;
            crossroad = isCrossroad;
        }
    }
}
