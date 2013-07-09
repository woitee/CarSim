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
                CoOrds last = new CoOrds(-777,-777); //value 
                bool goOn = false;
                Queue<PathPart> pth = new Queue<PathPart>();
                do { //ToDo: Add Crossroad Support and Reverse Path to not count twice
                    //find next
                    goOn = false;
                    for (int i = 0; i < Simulation.dirs.Length; i++){
                        CoOrds c = new CoOrds(co.x+Simulation.dirs[i].x , co.y+Simulation.dirs[i].y);
                        if (!last.Equals(c)){
                            char ch = map[c.x,c.y];
                            if (ch == 'D' || ch == '+'){
                                //c is next square
                                //check direction
                                int dir = c.Subtract(co).toDir();
                                //build path
                                if (pth.Count == 0){
                                    pth.Enqueue(new PathPart(PathPart.Type.Straight, dir, co, c, false));
                                } else {
                                    PathPart part = pth.Last();
                                    if(part.direction == dir){ //if the road is continuing the same direction as before
                                        if (part.type == PathPart.Type.Straight){
                                            part.to = c;
                                        } else { //after a turn
                                            part = new PathPart(PathPart.Type.Straight, dir, c, c, false);
                                            pth.Enqueue(part);
                                        }
                                    } else {
                                        part.to = part.to.Subtract(CoOrds.fromDir(part.direction)); //take one square away
                                        pth.Enqueue(new PathPart(PathPart.Type.Turn, dir, co, co, false)); //1 square long
                                        //pth.Enqueue(new PathPart(PathPart.Type.Straight, dir, c, c, false)); //1 square long
                                    }
                                }
                                if (objmap[c.x,c.y] == null){
                                    //continue building
                                    last = co;
                                    co = c;
                                    goOn = true;
                                } else {
                                    //finish here
                                    if(objmap[c.x, c.y] as Crossroad != null){
                                        PathPart part = pth.Peek();
                                        part.to = part.to.Subtract(CoOrds.fromDir(part.direction)); //take one square away   
                                    }
                                    dpt.connObj = objmap[c.x,c.y];
                                    Path path = new Path();
                                    path.route = pth.ToArray();
                                    dpt.fromPath = path;
                                }
                                //if the road is a dead end, connObj will be left null
                                break; //no need to check other directions
                            }
                        }
                    }
                } while (goOn); 
                return;
            }
            Crossroad crd = mapitem as Crossroad;
            if (crd != null){ //this should pass
                //ToDo: Find connObjs[] and paths[]
                return;
            }
        }

        private Path getPath(){
            //ToDo: implement finding paths here
            return null;
        }
    }
}
