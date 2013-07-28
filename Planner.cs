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
        private Sign[,,] signmap;

        public Planner(char[,] map, MapItem[,] objmap, Sign[,,] signmap){
            this.map = map;
            this.objmap = objmap;
            this.signmap = signmap;
        }

        public void FindConnections(MapItem mapitem){
            for (int i = 0; i < 4; i++){
                CoOrds c = mapitem.coords.Add(CoOrds.fromDir(i));
                if(c.isValid() && (map[c.x,c.y] == 'D' || map[c.x,c.y] == '+')) {
                    mapitem.fromPaths[i] = getPath(c, mapitem.coords, out mapitem.connObjs[i]);;
                }
            }
            return;
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
            PathPartMod a; int b;
            int dir = cur.Subtract(last).toDir();
            a = getPathPartMod(cur, dir, out b);

            if (a == PathPartMod.noway) {endObj = null; return null;}
            PathPart pp = new PathPart(PathPart.Type.Straight, dir, cur, cur, false, a, b);
            workPath.Enqueue(pp); //length 0
            
            if(objmap[cur.x,cur.y] != null){
                Path pth = new Path();
                pth.route = new PathPart[1];
                pth.route[0] = workPath.Dequeue();
                endObj = objmap[cur.x, cur.y];
                return pth;
            }
            bool goOn = false;
            do {
                //find next
                goOn = false;
                for (int i = 0; i < Simulation.dirs.Length; i++){
                    CoOrds c = new CoOrds(cur.x+Simulation.dirs[i].x, cur.y+Simulation.dirs[i].y);
                    if (!last.Equals(c) && c.isValid()){
                        char ch = map[c.x,c.y];
                        if (ch == 'D' || ch == '+'){
                            //c is next square
                            //build path
                            if (workPath.Count == 0){
                                a = getPathPartMod(cur,i, out b);
                                if (a == PathPartMod.noway) {endObj = null; return null;}
                                pp = new PathPart(PathPart.Type.Straight, i, cur, c, false,a,b);
                                workPath.Enqueue(pp);
                            } else {
                                PathPart part = workPath.Last();
                                if(part.direction == i){ //if the road is continuing the same direction as before
                                    if (a == PathPartMod.noway) {endObj = null; return null;}
                                    a = getPathPartMod(cur,i, out b);
                                    switch (a){
                                        case PathPartMod.nopass:
                                        case PathPartMod.speed:
                                            pp = new PathPart(PathPart.Type.Straight, i, cur, c, false, a ,b);
                                            workPath.Enqueue(pp);
                                            break;
                                        case PathPartMod.none:
                                            //nothing strange
                                            part.to = c;
                                            break;
                                    }
                                } else {
                                    if (((i-part.direction) == 1) || ((i-part.direction) == -3)) {
                                        a = getPathPartMod(cur,part.direction, out b);
                                        if (a == PathPartMod.noway) {endObj = null; return null;}
                                        workPath.Enqueue(new PathPart(PathPart.Type.TurnR, i, cur, c, false, a, b)); //1 square long
                                    } else {
                                        a = getPathPartMod(cur, part.direction, out b);
                                        if (a == PathPartMod.noway) {endObj = null; return null;}
                                        workPath.Enqueue(new PathPart(PathPart.Type.TurnL, i, cur, c, false, a, b)); //1 square long
                                    }
                                    workPath.Enqueue(new PathPart(PathPart.Type.Straight, i, c, c, false, PathPartMod.none, -1)); //0 squares long
                                }
                            }
                            if (objmap[c.x,c.y] == null){
                                //continue building
                                last = cur;
                                cur = c;
                                goOn = true; break;
                            } else {
                                //finish here
                                if(getPathPartMod(c, i, out b) == PathPartMod.noway) { endObj = null; return null; }
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

        private PathPartMod getPathPartMod(CoOrds co, int direction, out int arg){
            if(signmap[ co.x, co.y, direction ] == null) {arg = -1; return PathPartMod.none;}
            switch (signmap[ co.x, co.y, direction ].type){
                case SignType.Max30:
                    arg = 30; return PathPartMod.speed;
                case SignType.Max60:
                    arg = 60; return PathPartMod.speed;
                case SignType.Max90:
                    arg = 90; return PathPartMod.speed;
                case SignType.Nopass:
                    arg = -1; return PathPartMod.nopass;
                case SignType.Notthisway:
                    arg = -1; return PathPartMod.noway;
                case SignType.Noway:
                    arg = -1; return PathPartMod.noway;
                default:
                    arg = -1; return PathPartMod.none;
            }
        }

        public Path FindPath(Car car, Depot dptFrom, Depot dptTo){
            //simple implementation of Dijkstra's algorithm for finding paths between depots
            List<CoOrds> found = new List<CoOrds>();
            bool[,] been = new bool[map.GetLength(0), map.GetLength(1)];
            int[,] dist = new int[map.GetLength(0),map.GetLength(1)];
            CoOrds[,] prev = new CoOrds[map.GetLength(0), map.GetLength(1)];
            for (int i = 0; i < map.GetLength(0); i++){
                for (int j = 0; j < map.GetLength(1); j++){
                    dist[i,j] = int.MaxValue;
                }
            }
            found.Add(dptFrom.coords); dist[dptFrom.coords.x, dptFrom.coords.y] = 0;
            //initialized
            while(true){
                //pick minimum
                int min = int.MaxValue;
                CoOrds minCo = new CoOrds();
                foreach (CoOrds co in found){
                    if( dist[co.x, co.y] < min ){
                        min = dist[co.x, co.y];
                        minCo = co;
                    }
                }
                if (min == int.MaxValue){
                    //the road does not exist
                    return null;
                }
                if (objmap[minCo.x, minCo.y] == dptTo){
                    //found target
                    break;
                }
                found.Remove(minCo);
                been[minCo.x, minCo.y] = true;
                for (int i = 0; i < 4; i++){
                    MapItem next = objmap[minCo.x, minCo.y].connObjs[i];
                    if (next != dptTo) { next = next as Crossroad; }
                    if(next != null){
                        if(been[next.coords.x, next.coords.y]) {continue;}

                        if(dist[next.coords.x, next.coords.y] > min + objmap[minCo.x, minCo.y].fromPaths[i].Length){
                            if(dist[next.coords.x, next.coords.y] == int.MaxValue) {found.Add(next.coords);}
                            dist[next.coords.x, next.coords.y] = min + objmap[minCo.x, minCo.y].fromPaths[i].Length + 1;
                            prev[next.coords.x, next.coords.y] = minCo;
                        }
                    }                   
                }
            }
            //use a Stack to reverse
            Stack<MapItem> st = new Stack<MapItem>();
            CoOrds coo = dptTo.coords;
            st.Push(dptTo);
            do{
                coo = prev[coo.x, coo.y];
                st.Push(objmap[coo.x, coo.y]);
            } while (!coo.Equals(dptFrom.coords));
            //make into whole path
            Path pth = null;
            while(st.Count > 1){
                MapItem crd = st.Pop();
                int i = -1,j; int min = int.MaxValue;
                for (j = 0; j < 4; j++){
                    if(( crd.connObjs[j] == st.Peek() ) && (crd.fromPaths[j].Length < min)) {i=j; min=crd.fromPaths[j].Length;}
                }
                if(pth == null){
                    pth = crd.fromPaths[i];
                } else {
                    int srcDir = pth.route.Last().direction;
                    //NOTE: destination Dir = i
                    Path pth2 = new Path();
                    PathPart.Type type;
                    //if(((i-srcDir) == 2) || ((i-srcDir) == -2)){ //this means turning back
                    if(i == srcDir){
                        type = PathPart.Type.Straight;
                    } else if (((i-srcDir) == 1) || ((i-srcDir) == -3)) {
                        type = PathPart.Type.TurnR;
                    } else {
                        type = PathPart.Type.TurnL;
                    }
                    pth2.route = new PathPart[1] {new PathPart(type, i, crd.coords, crd.coords.Add(CoOrds.fromDir(i)), true, PathPartMod.none, -1)};
                    pth = pth.Merge(pth2);
                    pth = pth.Merge(crd.fromPaths[i]);
                }
            }
            pth.route.First().from = dptFrom.coords;
            pth.route.Last().to = dptTo.coords;
            return pth;
        }
    }
}