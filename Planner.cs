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

        public void FindConnections(MapItem mapitem){
            Depot dpt = mapitem as Depot;
            if (dpt != null){
                if(dpt.connObj != null) {return;} //if already set

                CoOrds co = dpt.coords; //only remains null if no path
                for (int i = 0; i < 4; i++){
                    co = dpt.coords.Add(CoOrds.fromDir(i));
                    char ch = map[co.x,co.y];
                    if(ch == 'D' || ch == '+'){
                        break;
                    }
                }
                dpt.fromPath = getPath(co, dpt.coords, out dpt.connObj); //impossible value -777 for debug

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
            workPath.Enqueue(new PathPart(PathPart.Type.Straight, cur.Subtract(last).toDir(), cur, cur, false)); //length 0
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
            do {
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


        public Path FindPath(Car car, Depot dptFrom, Depot dptTo){
            //simple BFS for finding paths between depots
            //ToDo: make it better, actually finding the shortest paths
            //ToDo: Remember found roads, so similiar cars can get their Path immediately
            int[,] dist = new int[map.GetLength(0),map.GetLength(1)];
            bool[,] been = new bool[map.GetLength(0),map.GetLength(1)];
            Queue<MapItem> qu = new Queue<MapItem>();
            if(dptFrom.connObj == null) {return null;}
            qu.Enqueue(dptFrom.connObj); been[dptFrom.coords.x,dptFrom.coords.y] = true;
            been[dptFrom.connObj.coords.x,dptFrom.connObj.coords.y] = true;
            dist[dptFrom.connObj.coords.x,dptFrom.connObj.coords.y] = 1;
            while(qu.Count > 0){
                MapItem cur = qu.Dequeue();
                if(cur == dptTo){ break; }
                else if (cur as Crossroad != null){
                    Crossroad crd = cur as Crossroad;
                    for (int i = 0; i < 4; i++){
                        MapItem other = crd.connObjs[i];
                        if (!(other == null) && !been[other.coords.x, other.coords.y]){
                            qu.Enqueue(other);
                            dist[other.coords.x, other.coords.y] = 1 + dist[cur.coords.x,cur.coords.y];
                            been[other.coords.x, other.coords.y] = true;
                        }
                    }
                }
            }
            if(dist[dptTo.coords.x,dptTo.coords.y] == 0){return null;}
            //path exists
            Stack<MapItem> st = new Stack<MapItem>();
            MapItem mi = dptTo.connObj;
            st.Push(dptTo);
            st.Push(mi);
            int a = dist[mi.coords.x,mi.coords.y];
            while(a != 1){
                a--;
                Crossroad crd = (Crossroad)mi;
                for (int i = 0; i < 4; i++){
                    if (!(crd.connObjs[i] == null) && (dist[crd.connObjs[i].coords.x,crd.connObjs[i].coords.y] == a)){
                        mi = crd.connObjs[i];
                        st.Push(mi);
                    }
                }
            }
            Path pth = dptFrom.fromPath;
            pth.route[0].from = dptFrom.coords;
            while(st.Count > 1){
                Crossroad crd = (Crossroad)st.Pop();
                int i;
                for (i = 0; i < 4; i++){
                    if( crd.connObjs[i] == st.Peek() ) {break;}
                }
                int srcDir = pth.route.Last().direction;
                int destDir = i;
                Path pth2 = new Path();
                PathPart.Type type = ((Math.Abs(srcDir-i) & 1) == 1) ? PathPart.Type.Turn : PathPart.Type.Straight;
                pth2.route = new PathPart[1] {new PathPart(type, i, crd.coords, crd.coords.Add(CoOrds.fromDir(i)), true)};
                pth = pth.Merge(pth2);
                pth = pth.Merge(crd.fromPaths[i]);
            }
            pth.route.Last().to = dptTo.coords;
            return pth;
        }
    }
}
