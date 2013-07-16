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
        
        private int _direction = 0; //direction 0-12
        private CoOrds _coords;
        private double _maxSpeed;
        private double speed;
        private double accel = 0.02; //acceleration per tick
        private double decel = 0.02; //deccelartion per tick
        private double X;
        private double Y;

        private Path basicPath; //basic, doesn't use scaling
        private Itinerary itinerary;

        //for drawing turns
        private CoOrds turnFrom;
        private double turnProgress;

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
            //follow path
            double distToTravel = _maxSpeed;
            ItinPart cur = itinerary.route.First();
            double distX, distY;
            dist(cur, out distX, out distY);
            while(distToTravel > Math.Abs(distX)+Math.Abs(distY)){
                if(itinerary.route.Count <= 1){
                    return true;
                }
                X = cur.dest.x; Y = cur.dest.y;
                distToTravel -= Math.Abs(distX)+Math.Abs(distY);
                itinerary.route.Dequeue();
                cur = itinerary.route.First();
                if((cur.type == ItinType.TurnLeftTo) || (cur.type == ItinType.TurnRightTo)){
                    turnFrom = new CoOrds((int)Math.Round(X), (int)Math.Round(Y));
                    turnProgress = 0;
                    dist(cur, out distX, out distY);
                } else {
                    dist(cur, out distX, out distY);
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
            return false;
        }

        private void dist(ItinPart itPart, out double distX, out double distY){
            switch (itPart.type)
            {
                case ItinType.GoTo:
                    distX = itPart.dest.x-X; distY = itPart.dest.y-Y;
                    break;
                case ItinType.TurnLeftTo:
                    distX = (Math.PI/2)*Math.Abs(itPart.dest.x-X)*(1-turnProgress); distY = 0;
                    break;
                case ItinType.TurnRightTo:
                    distX = (Math.PI/2)*Math.Abs(itPart.dest.x-X)*(1-turnProgress); distY = 0;
                    break;
                case ItinType.AskCrossroad:
                    distX = 0; distY = 0;
                    return;
                default: //just to feed compiler
                    distX = 0; distY = 0;
                    return;
            }
        }

        public Car Clone(){
            return new Car(_maxSpeed,X,Y,coords,direction,basicPath,itinerary.Clone());
        }
    }
}
