﻿using System;
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
            for (int i = 0; i < 4; i++){
                if(mapitem.connObjs[i] == null) { //if this direction not already set
                    CoOrds c = mapitem.coords.Add(CoOrds.fromDir(i));
                    if(c.isValid() && (map[c.x,c.y] == 'D' || map[c.x,c.y] == '+')) {
                        mapitem.fromPaths[i] = getPath(c, mapitem.coords, out mapitem.connObjs[i]);
                    }
                    MapItem other = mapitem.connObjs[i];
                    if(other != null){ //this should pass or dead end
                        Path pth = mapitem.fromPaths[i].reverse();
                        int dir = pth.route[0].direction;
                        other.connObjs[dir] = mapitem;
                        other.fromPaths[dir] = pth;
                    }
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
            workPath.Enqueue(new PathPart(PathPart.Type.Straight, cur.Subtract(last).toDir(), cur, cur, false)); //length 0
            if(objmap[cur.x,cur.y] != null){
                int dir = cur.Subtract(last).toDir();
                Path pth = new Path();
                pth.route = new PathPart[1];
                pth.route[0] = new PathPart(PathPart.Type.Straight, dir, cur, cur, false);
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
                                workPath.Enqueue(new PathPart(PathPart.Type.Straight, i, cur, c, false));
                            } else {
                                PathPart part = workPath.Last();
                                if(part.direction == i){ //if the road is continuing the same direction as before
                                    part.to = c;
                                } else {
                                    if (((i-part.direction) == 1) || ((i-part.direction) == -3)) {
                                        workPath.Enqueue(new PathPart(PathPart.Type.TurnR, i, cur, c, false)); //1 square long
                                    } else {
                                        workPath.Enqueue(new PathPart(PathPart.Type.TurnL, i, cur, c, false)); //1 square long
                                    }
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
                            dist[next.coords.x, next.coords.y] = min + objmap[minCo.x, minCo.y].fromPaths[i].Length;
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
                    pth2.route = new PathPart[1] {new PathPart(type, i, crd.coords, crd.coords.Add(CoOrds.fromDir(i)), true)};
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