using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    public struct CoOrds{public int x, y;
            public CoOrds (int x, int y) {this.x = x; this.y = y;}
            public bool Equals(CoOrds other){
                return((this.x == other.x) && (this.y == other.y));
            }
            public CoOrds Add(CoOrds other){
                return (new CoOrds (this.x + other.x, this.y + other.y));
            }
            public CoOrds Add(int x, int y){
                return (new CoOrds (this.x + x, this.y + y));
            }
            public CoOrds Subtract(CoOrds other){
                return (new CoOrds (this.x - other.x, this.y - other.y));
            }
            public CoOrds Multiply(int n){
                return (new CoOrds (this.x * n, this.y * n));
            }
            public double Distance(){
                return Math.Sqrt(x*x+y*y);
            }
            public double Distance(CoOrds other){
                return other.Subtract(this).Distance();
            }
            public bool isValid(){
                return ( (x>=0) && (x<Simulation.WIDTH) && (y>=0) && (y<Simulation.HEIGHT) );
            }
            public int toDir(){
                for (int i = 0; i < Simulation.dirs.Length; i++){
                    if (this.Equals(Simulation.dirs[i])){
                        return i;
                    }
                }
                return -1; //counts as ErrorCode
            }
            public static CoOrds fromDir(int dir){
                return (Simulation.dirs[dir]);
            }

            public static int oppDir(int dir){
                return (dir+2)%4;
            }
            public static int toRightDir(int dir){
                return (dir+3)%4;
            }
            public static int toLeftDir(int dir){
                return (dir+1)%4;
            }
    }
}
