using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Car
    {
        private const int TILESIZE = Simulation.TILESIZE;
        
        public CoOrds coords{
            get{ return _coords; }
        }
        public double maxSpeed{
            get{ return _maxSpeed; }
        }
        public CoOrds from{
            get{ return basicPath.route[0].from; }
        }
        public CoOrds to{
            get{ return basicPath.route[basicPath.route.Length-1].to; }
        }
        public int direction {
        get{ return _direction; }
        }
        
        private const double accel = 0.01; //acceleration per tick
        private const double maxLTurnSpeed = 0.5; //maximum speed when turning left  (looser turn)
        private const double maxRTurnSpeed = 0.2; //maximum speed when turning right (sharper turn)

        private int _direction = 0; //direction 0-12
        private CoOrds _coords;
        private double _maxSpeed;
        private double speed;
        private double X;
        private double Y;

        private Path basicPath; //basic, doesn't use scaling
        private Itinerary itinerary;

        //for drawing turns
        private CoOrds turnFrom;
        private double turnProgress;

        //for handling crossroads
        private bool waitingForAllow = false;
        private Crossroad cross;

        public Path path{
            get{return basicPath;}
            set{
                basicPath = value;
                MakeItinerary();
                X=_coords.x; Y=_coords.y;
            }
        }

        public Car(double maxSpeed){
            this._maxSpeed = maxSpeed;
        }

        public Car(double speed, double X, double Y, CoOrds coords, int direction, Path path, Itinerary itinerary){
            //basically just for cloning
            this._maxSpeed = speed;
            this.X = X;
            this.Y = Y;
            this.basicPath = path;
            this._coords = coords;
            this._direction = direction;
            this.itinerary = itinerary;
        }

        public void MakeItinerary(){
            //ToDo: make it real
            _coords = path.route[0].from.Multiply(TILESIZE).Add(directionOffset(path.route[0].direction,true));
            _direction = path.route[0].direction*3;
            Queue<ItinPart> qu = new Queue<ItinPart>();
            for (int i = 0; i < path.route.Length; i++){
                if(path.route[i].type == PathPart.Type.Straight){
                    qu.Enqueue(new ItinPart(ItinType.GoTo,
                                            path.route[i].to.Multiply(TILESIZE).Add(directionOffset(path.route[i].direction,false)),
                                            _maxSpeed));
                } else if (path.route[i].type == PathPart.Type.TurnL){
                    qu.Enqueue(new ItinPart(ItinType.TurnLeftTo,
                                            path.route[i].from.Multiply(TILESIZE).Add(directionOffset(path.route[i].direction,true)),
                                            _maxSpeed));
                } else { //turnR
                    qu.Enqueue(new ItinPart(ItinType.TurnRightTo,
                                            path.route[i].from.Multiply(TILESIZE).Add(directionOffset(path.route[i].direction,true)),
                                            _maxSpeed));
                }
            }
            itinerary = new Itinerary(qu);
        }

        private CoOrds directionOffset(int dir, bool afterCrossroad){
            if(afterCrossroad){//this is where they ride from
                switch(dir){
                    case 0:
                        return new CoOrds(35,29);
                    case 1:
                        return new CoOrds(25,35);
                    case 2:
                        return new CoOrds(19,24);
                    case 3:
                        return new CoOrds(29,19);
                    default:
                        return new CoOrds(); //to feed compiler
                }
            } else {
                switch(dir){//this is where they ride to
                    case 0:
                        return new CoOrds(19,29);
                    case 1:
                        return new CoOrds(25,19);
                    case 2:
                        return new CoOrds(35,24);
                    case 3:
                        return new CoOrds(29,35);
                    default:
                        return new CoOrds(); //to feed compiler
                }
            }
        }

        public bool Tick(){ //returns true if car has reached its destination
            //calculate distances
            ItinPart cur = itinerary.route.First();
            ItinPart next = itinerary.route.Count > 1 ? itinerary.route.ElementAt(1) : new ItinPart();
            double distX, distY, dist;
            getDist(cur, out distX, out distY, out dist);
            #region figure out what speed
            if (waitingForAllow){
                //waiting for crossroad to clear way, stop at current destination
                double t=2*dist/speed;
                speed -= speed/t;
            } else if (next.isTurn() && dist < 50) {
                //close to a turn, slow down to turning speed at current destination
                double maxTSpeed = next.type == ItinType.TurnLeftTo ? maxLTurnSpeed : maxRTurnSpeed;
                double t=2*dist/(maxTSpeed+speed);
                speed -= (speed-maxTSpeed)/t;
            } else if ((itinerary.route.Count <= 1) && dist < 50){ //should be 1
                //close to a final depot, stop at current destination
                double t=2*dist/speed;
                speed -= speed/t;
                speed = speed < 0.01 ? 0.01 : speed; //keep at least minimum speed to actually reach destination
            } else {
                speed = speed + accel;
                speed = speed > maxSpeed ? maxSpeed : speed;
            }
            #endregion
            #region follow path
            double distToTravel = speed;
            while(distToTravel > dist){
                if(itinerary.route.Count <= 1){
                    return true;
                }
                X = cur.dest.x; Y = cur.dest.y;
                distToTravel -= dist;
                itinerary.route.Dequeue();
                cur = itinerary.route.First();
                if( cur.isTurn() ){
                    turnFrom = new CoOrds((int)Math.Round(X), (int)Math.Round(Y));
                    turnProgress = 0;
                    getDist(cur, out distX, out distY, out dist);
                } else {
                    getDist(cur, out distX, out distY, out dist);
                    _direction = new CoOrds(Math.Sign(distX),Math.Sign(distY)).toDir()*3;
                }
                
            }
            if(cur.type == ItinType.GoTo){
                X += distToTravel*Math.Sign(distX);
                Y += distToTravel*Math.Sign(distY);
            } else if (cur.type == ItinType.TurnLeftTo || cur.type == ItinType.TurnRightTo){
                #region turning
                //dont forget - change direction
                //quarter turn starts at turnFrom, ends at cur.dest, is in way of cur.type
                double rad = Math.Abs(cur.dest.x-turnFrom.x);
                double turnSize = (Math.PI/2)*rad; //ToDo: 15 on Left????
                //what part are we travelling
                turnProgress += distToTravel/turnSize; //should still be <1
                int frameTurn = (int)Math.Round(turnProgress*3); //which step of animation are we in
                if (cur.type == ItinType.TurnLeftTo){
                    double angle = turnProgress/2*Math.PI;
                    if((turnFrom.x < cur.dest.x) && (turnFrom.y < cur.dest.y)){ //RD
                        X = cur.dest.x - (Math.Cos(angle) * rad);
                        Y = turnFrom.y + (Math.Sin(angle) * rad);
                        _direction = 3 - frameTurn;
                    } else if ((turnFrom.x > cur.dest.x) && (turnFrom.y < cur.dest.y)){ //LD
                        X = turnFrom.x - (Math.Sin(angle) * rad);
                        Y = cur.dest.y - (Math.Cos(angle) * rad);
                        _direction = 6 - frameTurn;
                    } else if ((turnFrom.x < cur.dest.x) && (turnFrom.y > cur.dest.y)){ //RU
                        X = turnFrom.x + (Math.Sin(angle) * rad);
                        Y = cur.dest.y + (Math.Cos(angle) * rad);
                        _direction = 12 - frameTurn;
                    } else { //LU
                        X = cur.dest.x + (Math.Cos(angle) * rad);
                        Y = turnFrom.y - (Math.Sin(angle) * rad);
                        _direction = 9 - frameTurn;
                    }
                } else {
                    //right turns
                    double angle = turnProgress/2*Math.PI;
                    if((turnFrom.x < cur.dest.x) && (turnFrom.y < cur.dest.y)){ //RD
                        X = turnFrom.x + (Math.Sin(angle) * rad);
                        Y = cur.dest.y - (Math.Cos(angle) * rad);
                        _direction = 0 + frameTurn;
                    } else if ((turnFrom.x > cur.dest.x) && (turnFrom.y < cur.dest.y)){ //LD
                        X = cur.dest.x + (Math.Cos(angle) * rad);
                        Y = turnFrom.y + (Math.Sin(angle) * rad);
                        _direction = 3 + frameTurn;
                    } else if ((turnFrom.x < cur.dest.x) && (turnFrom.y > cur.dest.y)){ //RU
                        X = cur.dest.x - (Math.Cos(angle) * rad);
                        Y = turnFrom.y - (Math.Sin(angle) * rad);
                        _direction = 9 + frameTurn;
                    } else { //LU
                        X = turnFrom.x - (Math.Sin(angle) * rad);
                        Y = cur.dest.y + (Math.Cos(angle) * rad);
                        _direction = 6 + frameTurn;
                    }
                }
                #endregion
            }
            _coords = new CoOrds((int)X,(int)Y);
            #endregion
            return false;
        }

        private void getDist(ItinPart itPart, out double distX, out double distY, out double totalDist){
            switch (itPart.type)
            {
                case ItinType.GoTo:
                    distX = itPart.dest.x-X; distY = itPart.dest.y-Y;
                    totalDist = Math.Abs(distX)+Math.Abs(distY);
                    break;
                case ItinType.TurnLeftTo:
                case ItinType.TurnRightTo:
                    distX = 0; distY = 0;
                    totalDist = (Math.PI/2)*Math.Abs(itPart.dest.x-X)*(1-turnProgress);
                    break;
                case ItinType.AskCrossroad:
                default:
                    distX = 0; distY = 0; totalDist = 0;
                    return;
            }
        }

        public Car Clone(){
            return new Car(_maxSpeed,X,Y,coords,direction,basicPath,itinerary.Clone());
        }
    }
}
