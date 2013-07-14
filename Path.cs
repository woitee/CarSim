using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    //the path consists of parts of either going straight forward, or turning
    //we chose a representation considering this
    //PathParts have a location "from" and "to", "from" is inclusive and "to" exclusive
    class Path
    {
        public PathPart[] route;
        public Path reverse(){
            Path pth = new Path();
            pth.route = new PathPart[route.Length];
            for (int i = 0; i < route.Length; i++){
                pth.route[i]=route[route.Length-i-1].reverse();
            }
            return pth;
        }
        /*public int Length{
            get {
                int val = 0;
                for (int i = 0; i < route.Length; i++){
                    val += route[i].Length;
                }
                return val;
            }
        }*/
    }

    public class PathPart
    {
        public enum Type{Straight,Turn};
        public Type type;
        public int direction; //RDLU
        public CoOrds from;
        public CoOrds to;
        bool crossroad;
        
        public PathPart(Type type, int direction, CoOrds from, CoOrds to, bool isCrossroad){
            this.type = type;
            this.direction = direction;
            this.from = from;
            this.to = to;
            crossroad = isCrossroad;
        }
        public PathPart reverse(){
            if (from.Equals(to)){ //length 0
                return new PathPart(type,(direction+2)%4,from,to,crossroad);
            } else {
                CoOrds co = CoOrds.fromDir(direction);
                return new PathPart(type,(direction+2)%4,to.Subtract(co),from.Subtract(co),crossroad);
            }
        }
        /*public int Length{
            get {//ToDo: Temporary?
                return(Math.Abs(from.x-to.x)+Math.Abs(from.y-to.y));
            }
        }*/
    }
}
