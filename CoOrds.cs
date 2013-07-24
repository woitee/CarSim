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
            public CoOrds Normalize(){
                int xx, yy;
                if (x>0){
                    xx = 1;
                } else if(x == 0){
                    xx = 0;
                } else {
                    xx = -1;
                }
                if (y>0){
                    yy = 1;
                } else if(y == 0){
                    yy = 0;
                } else {
                    yy = -1;
                }
                return new CoOrds(xx,yy);
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
            
            public static int dirFromString(string s){
                switch (s[0]){
                    case 'R': case 'r':
                        return 0;
                    case 'D': case 'd':
                        return 1;
                    case 'L': case 'l':
                        return 2;
                    case 'U': case 'u':
                        return 3;
                    default:
                        return -1;
                }
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
