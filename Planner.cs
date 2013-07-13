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
                if(dpt.connObj != null) {return;} //if already set

                CoOrds co = dpt.coords; //only remains null if no path
                for (int i = 0; i < 4; i++){
                    co = dpt.coords.Add(CoOrds.fromDir(i));
                    char ch = map[co.x,co.y];
                    if(ch == 'D' || ch == '+'){
                        break;
                    }
                }
                dpt.fromPath = getPath(co, new CoOrds(-777,-777), out dpt.connObj); //impossible value -777 for debug

                Depot dptOther = dpt.connObj as Depot;
                if(dptOther != null){
                    dptOther.connObj = dpt;
                    dptOther.fromPath = dpt.fromPath.reverse();
                }
                Crossroad crdOther = dpt.connObj as Crossroad;
                if(crdOther != null){ //this should pass or dead end
                    Path pth = dpt.fromPath.reverse();
                    int dir = pth.route[0].direction;
                    crdOther.connObjs[dir] = dpt;
                    crdOther.fromPaths[dir] = pth;
                }
                return;
            }
            Crossroad crd = mapitem as Crossroad;
            if (crd != null){ //this should pass
                for (int i = 0; i < 4; i++){
                    if(crd.connObjs[i] != null) {return;} //if this direction already set
                    CoOrds c = crd.coords.Add(CoOrds.fromDir(i));
                    
                    if(map[c.x,c.y] == 'D' || map[c.x,c.y] == '+') {
                        crd.fromPaths[i] = getPath(c, crd.coords, out crd.connObjs[i]);
                    }
                    Depot dptOther = crd.connObjs[i] as Depot;
                    if(dptOther != null){ //we shouldn't really get here, as Depots are processed before Crossroads
                        dptOther.connObj = crd;
                        dptOther.fromPath = crd.fromPaths[i].reverse();
                    }
                    Crossroad crdOther = crd.connObjs[i] as Crossroad;
                    if(crdOther != null){ //this should pass or dead end
                        Path pth = crd.fromPaths[i].reverse();
                        int dir = pth.route[0].direction;
                        crdOther.connObjs[dir] = crd;
                        crdOther.fromPaths[dir] = pth;
                    }
                }
                return;
            }
        }

        /// <summary>
        /// Returns Path to the nearest MapItem, according to map.
        /// </summary>
        /// <param name="cur">Starting location.</param>
        /// <param name="last">Set to a neighbouring location, search won't continue in that direction.</param>
        /// <param name="endObj">MapItem found at the end of searched road.</param>
        /// <returns></returns>
        private Path getPath(CoOrds cur, CoOrds last, out MapItem endObj){
            Queue<PathPart> workPath = new Queue<PathPart>();
            if(objmap[cur.x,cur.y] != null){
                int dir = cur.Subtract(last).toDir();
                Path pth = new Path();
                pth.route = new PathPart[1];
                pth.route[0] = new PathPart(PathPart.Type.Straight, dir, cur, cur, //length 0
                                            objmap[cur.x,cur.y] as Crossroad != null);
                endObj = objmap[cur.x, cur.y];
                return pth;
            }
            bool goOn = false;
            do { //ToDo: Add Crossroad Support
                //find next
                goOn = false;
                for (int i = 0; i < Simulation.dirs.Length; i++){
                    CoOrds c = new CoOrds(cur.x+Simulation.dirs[i].x, cur.y+Simulation.dirs[i].y);
                    if (!last.Equals(c)){
                        char ch = map[c.x,c.y];
                        if (ch == 'D' || ch == '+'){
                            //c is next square
                            //build path
                            if (workPath.Count == 0){
                                workPath.Enqueue(new PathPart(PathPart.Type.Straight, i, cur, c, false));
                            } else {
                                PathPart part = workPath.Last();
                                if(part.direction == i){ //if the road is continuing the same direction as before
                                    part.to = c;
                                } else {
                                    workPath.Enqueue(new PathPart(PathPart.Type.Turn, i, cur, c, false)); //1 square long
                                    workPath.Enqueue(new PathPart(PathPart.Type.Straight, i, c, c, false)); //0 squares long
                                }
                            }
                            if (objmap[c.x,c.y] == null){
                                //continue building
                                last = cur;
                                cur = c;
                                goOn = true; break;
                            } else {
                                //finish here
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
