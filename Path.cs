using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    /// <summary>
    /// The path consists of parts of either going straight forward, or turning
    /// we chose a representation considering this
    /// PathParts have a location "from" and "to", "from" is inclusive and "to" exclusive.
    /// PathParts of length 0 have "from" the same as "to".
    /// </summary>
    class Path
    {
        public PathPart[] route;
        public Path Merge(Path other){
            Path pth = new Path();
            pth.route = new PathPart[this.route.Length+other.route.Length];
            for (int i = 0; i < this.route.Length; i++){
                pth.route[i] = this.route[i];
            }
            for (int i = 0; i < other.route.Length; i++){
                pth.route[i+this.route.Length] = other.route[i];
            }
            return pth;
        }
        public int Length{
            get{
                int sum = 0;
                for (int i = 0; i < route.Length; i++){
                    sum += route[i].Length;
                }
                return sum;
            }
        }
        public Path Clone(){
            Path pth = new Path();
            pth.route = new PathPart[route.Length];
            for (int i = 0; i < route.Length; i++){
                pth.route[i] = this.route[i].Clone();
            }
            return pth;
        }
    }

    /// <summary>
    /// One part of the path. Can contain various modifiers, usually picked by signs to the side of the road.
    /// </summary>
    public class PathPart
    {
        public enum Type{Straight,TurnL,TurnR};
        public Type type;
        public int direction; //RDLU
        public CoOrds from;
        public CoOrds to;
        public bool crossroad;
        public PathPartMod mod = PathPartMod.none;
        public int modArg = -1;
        
        public PathPart(Type type, int direction, CoOrds from, CoOrds to, bool isCrossroad,
            PathPartMod mod, int modArg){
            this.type = type;
            this.direction = direction;
            this.from = from;
            this.to = to;
            crossroad = isCrossroad;
            this.mod = mod;
            this.modArg = modArg;
        }
        public int Length{
            get{
                //if(this.to.Equals(this.from)){return 0;}
                return Math.Abs(this.to.x - this.from.x) + Math.Abs(this.to.y - this.from.y);
            }
        }
        public PathPart Clone(){
            return new PathPart(type,direction,from,to,crossroad,mod,modArg);
        }
    }
    public enum PathPartMod{speed, nopass, noway, none}
}
