using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Planner
    {
        private char[,] map;
        private MapItem[,] objmap;

        public Planner(char[,] map, MapItem[,] objmap){
            this.map = map;
            this.objmap = objmap;
        }

        public void findConnections(MapItem mapitem){
            Depot dpt = mapitem as Depot;
            if (dpt != null){
                //ToDo: Find connObj and path
                CoOrds co = dpt.coords;
                dpt.fromPath = getPath(dpt.coords, new CoOrds(-777,-777), out dpt.connObj); //impossible value -777 for debug
                return;
            }
            Crossroad crd = mapitem as Crossroad;
            if (crd != null){ //this should pass
                //ToDo: Find connObjs[] and paths[]
                return;
            }
        }

        private Path getPath(CoOrds cur, CoOrds last, out MapItem endObj){
            //ToDo: implement finding paths here
            Queue<PathPart> workPath = new Queue<PathPart>();
            bool goOn = false;
            do { //ToDo: Add Crossroad Support and Reverse Path to not count twice
                //find next
                goOn = false;
                for (int i = 0; i < Simulation.dirs.Length; i++){
                    CoOrds c = new CoOrds(cur.x+Simulation.dirs[i].x, cur.y+Simulation.dirs[i].y);
                    if (!last.Equals(c)){
                        char ch = map[c.x,c.y];
                        if (ch == 'D' || ch == '+'){
                            //c is next square
                            //check direction
                            int dir = c.Subtract(cur).toDir();
                            //build path
                            if (workPath.Count == 0){
                                workPath.Enqueue(new PathPart(PathPart.Type.Straight, dir, cur, c, false));
                            } else {
                                PathPart part = workPath.Last();
                                if(part.direction == dir){ //if the road is continuing the same direction as before
                                    if (part.type == PathPart.Type.Straight){
                                        part.to = c;
                                    } else { //after a turn
                                        part = new PathPart(PathPart.Type.Straight, dir, cur, c, false);
                                        workPath.Enqueue(part);
                                    }
                                } else {
                                    part.to = part.to.Subtract(CoOrds.fromDir(part.direction)); //take one square away
                                    workPath.Enqueue(new PathPart(PathPart.Type.Turn, dir, cur, cur, false)); //1 square long
                                }
                            }
                            if (objmap[c.x,c.y] == null){
                                //continue building
                                last = cur;
                                cur = c;
                                goOn = true; break;
                            } else {
                                //finish here
                                if(objmap[c.x, c.y] as Crossroad != null){
                                    PathPart part = workPath.Peek();
                                    part.to = part.to.Subtract(CoOrds.fromDir(part.direction)); //take one square away   
                                }
                                endObj = objmap[c.x,c.y];
                                Path path = new Path();
                                path.route = workPath.ToArray();
                                return path;
                            }
                            //if the road is a dead end, connObj will be left null
                        }
                    }
                }
            } while (goOn);
            endObj = null; return null;
        }
    }
}
