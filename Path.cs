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
    }

    public class PathPart
    {
        public enum Type{Straight,TurnL,TurnR};
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
            switch (type){
                case Type.Straight:
                    CoOrds co = CoOrds.fromDir(direction);
                    return new PathPart(type,(direction+2)%4,to.Subtract(co),from.Subtract(co),crossroad);
                case Type.TurnL:
                    int newDir = (direction+3)%4;
                    co = CoOrds.fromDir(newDir);
                    return new PathPart(PathPart.Type.TurnR,newDir,from,from.Add(co),crossroad);
                case Type.TurnR:
                    newDir = (direction+1)%4;
                    co = CoOrds.fromDir(newDir);
                    return new PathPart(PathPart.Type.TurnL,newDir,from,from.Add(co),crossroad);
                default: //should never be reached
                    return null;
            }
        }
    }
}
