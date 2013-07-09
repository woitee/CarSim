using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    //the path consists of parts of either going straight forward, or turning
    //we chose a representation considering this
    //straight roads start at the first straight piece and end at the last straight piece
    class Path
    {
        public PathPart[] route;
    }

    public class PathPart
    {
        public enum Type{Straight,Turn};
        public Type type;
        public int direction; //RDLU
        CoOrds from;
        public CoOrds to;
        bool crossroad;
        
        public PathPart(Type type, int direction, CoOrds from, CoOrds to, bool isCrossroad){
            this.type = type;
            this.direction = direction;
            this.from = from;
            this.to = to;
            crossroad = isCrossroad;
        }
    }
}
