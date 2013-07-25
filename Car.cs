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
            set{ _maxSpeed = value; _actualMaxSpeed = value; }
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
        private void addToCoords(CoOrds co){
            _coords.x += co.x;
            _coords.y += co.y;
            X += co.x;
            Y += co.y; 
        }
        private double _maxSpeed;
        private double _actualMaxSpeed;
        private double speed;
        private double X;
        private double Y;

        //some statistics
        private int timeAlive = 0;
        private double totalDistTravelled = 0;

        private Path basicPath; //basic, doesn't use scaling
        private Itinerary itinerary;

        //for drawing cars in turns
        private CoOrds turnFrom;
        private double turnProgress;

        //for handling crossroads
        private bool waitingForAllow = false;
        private MapItem _cross;
        public MapItem cross{
            get{ return _cross; }
        }
        public void setCross(MapItem cross, int crossComingFrom){
            _cross = cross; this.crossComingFrom = crossComingFrom;
        }
        private int crossComingFrom; //the road we are coming to cross

        //for handling cars in front, back and eventually passing them
        private Car _inFront;
        public Car inFront{
            set{
                if(inFront != null){
                    inFront.inBack = this;
                }
                _inFront = value;
            }
            get{
                return _inFront;
            }
        }
        private Car inBack;
        private const int safeDistanceMult = 40;
        //passing
        private bool passingAllowed = true;
        private bool passing;
        private int passingPhase;
        public bool beingPassed;
        private Car passCar;

        public Path path{
            get{return basicPath;}
            set{
                basicPath = value;
                itinerary = MakeItinerary();
                X=_coords.x; Y=_coords.y;
            }
        }

        //CONSTRUCTORS
        public Car(){
            this.maxSpeed = 0;
            this.X = -777;
            this.Y = -777;
            this._coords = new CoOrds(-777,-777);
        }
        public Car(double maxSpeed){ 
            this.maxSpeed = maxSpeed;
        }
        public Car(double speed, double X, double Y, CoOrds coords, int direction, Path path, Itinerary itinerary){
            //Basically just used for cloning
            this.maxSpeed = speed;
            this.X = X;
            this.Y = Y;
            this.basicPath = path;
            this._coords = coords;
            this._direction = direction;
            this.itinerary = itinerary;
        }

        public Itinerary MakeItinerary(){
            _coords = path.route[0].from.Multiply(TILESIZE).Add(directionOffset(path.route[0].direction,true));
            _direction = path.route[0].direction*3;
            Queue<ItinPart> qu = new Queue<ItinPart>();
            for (int i = 0; i < path.route.Length; i++){
                ItinPart itPart;
                switch (path.route[i].mod){
		            case PathPartMod.speed:
                        qu.Enqueue(new ItinPart(ItinType.SpeedLimit, new CoOrds(path.route[i].modArg,0)));
                        break;
                    case PathPartMod.nopass:
                        qu.Enqueue(new ItinPart(ItinType.NoPassing, new CoOrds(0,0)));
                        break;
                    case PathPartMod.none:
                    default:
                        break;
	            }   
                if(path.route[i].type == PathPart.Type.Straight){
                    itPart = new ItinPart(ItinType.GoTo,
                                          path.route[i].to.Multiply(TILESIZE).Add(directionOffset(path.route[i].direction,false))
                                         );
                } else if (path.route[i].type == PathPart.Type.TurnL){
                    itPart = new ItinPart(ItinType.TurnLeftTo,
                                          path.route[i].from.Multiply(TILESIZE).Add(directionOffset(path.route[i].direction,true))
                                         );
                } else { //turnR
                    itPart = new ItinPart(ItinType.TurnRightTo,
                                          path.route[i].from.Multiply(TILESIZE).Add(directionOffset(path.route[i].direction,true))
                                         );
                }
                if(path.route[i].crossroad){
                    //we are now on a road thats one square long
                    ItinPart toSplit = qu.Last(); //the one we put there last time
                    if (toSplit.type != ItinType.GoTo) {
                       throw new Exception("Pathfinding failed."); //just after crossroad, put connecting road
                    }
                    CoOrds splitPoint = toSplit.dest.Subtract( CoOrds.fromDir(path.route[i-1].direction).Multiply(50) );
                    CoOrds endPoint = toSplit.dest;
                    toSplit.dest = splitPoint;
                    qu.Enqueue(new ItinPart(ItinType.AskCrossroad, path.route[i].from));
                    qu.Enqueue(new ItinPart(ItinType.GoTo, endPoint));
                    qu.Enqueue(new ItinPart(ItinType.EnterCrossroad, new CoOrds(
                                                                                CoOrds.oppDir(path.route[i-1].direction),
                                                                                path.route[i].direction
                                                                               )));
                    if (path.route[i].type == PathPart.Type.Straight){ continue;}
                }
                qu.Enqueue(itPart);
            }
            //process start
            double distX; double distY; double dist;
            getDist(qu.First(), out distX, out distY, out dist);
            while (dist == 0){
                ProcessSpecItin( qu.First() );
                qu.Dequeue();
                getDist(qu.First(), out distX, out distY, out dist);
            }
            return new Itinerary(qu);
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
            next = next.type == ItinType.EnterCrossroad ? itinerary.route.ElementAt(2) : next;
            double distX, distY, dist;
            getDist(cur, out distX, out distY, out dist);
            #region figure out what speed
            #region maybe ask crossroad if i should go
            if (waitingForAllow){
                int curDir = crossComingFrom;
                int toDir = -1; //errorCode
                switch (next.type){
                    case ItinType.GoTo:
                        toDir = curDir;
                        break;
                    case ItinType.TurnLeftTo:
                        toDir = CoOrds.toLeftDir(curDir);
                        break;
                    case ItinType.TurnRightTo:
                        toDir = CoOrds.toRightDir(curDir);
                        break;
                    case ItinType.AskCrossroad:
                    case ItinType.EnterCrossroad:
                    default:
                        break;
                }
                waitingForAllow = !cross.CanGo(this,curDir,toDir);
            }
            #endregion
            #region if passing
            if (passing){
                if(passingPhase < 16){
                    //First part of four.
                    if(passingPhase % 4 == 0){
                        CoOrds dir = CoOrds.fromDir( CoOrds.toRightDir(direction/3) );
                        addToCoords(dir);
                        cur.dest = cur.dest.Add(dir);
                    }
                    passingPhase++;
                } else if (passingPhase == 16){
                    //Second part of four.
                    if(inBack != null){ 
                        inBack._inFront = inFront;
                    }
                    inFront.inBack = inBack;
                    passingPhase++;
                } else if (passingPhase == 17){
                    //Third part of four.
                    int inFrontNeeded = (int)Math.Round(passCar.speed*safeDistanceMult+10);
                    int diff;
                    switch (direction/3){
                        case 0:
                            diff = passCar.coords.x - coords.x; break;
                        case 1:
                            diff = passCar.coords.y - coords.y; break;
                        case 2:
                            diff = coords.x - passCar.coords.x; break;
                        case 3:
                            diff = coords.y - passCar.coords.y; break;
                        default:
                            diff = 7777; break;
                    }
                    if(diff < -inFrontNeeded){
                        passingPhase++;
                    } else {
                        speed = speed + accel;
                        speed = speed > maxSpeed ? maxSpeed : speed;
                    }
                } else if (passingPhase < 34){
                    //Fourth part of four.
                    if(passingPhase % 4 == 0){
                        CoOrds dir = CoOrds.fromDir( CoOrds.toLeftDir(direction/3) );
                        addToCoords(dir);
                        cur.dest = cur.dest.Add(dir);
                    }
                    passingPhase++;
                } else {
                    //end passing
                    passing = false;
                    Car tempCar = inFront;
                    inFront = passCar.inFront;
                    passCar._inFront = this;
                    while(tempCar != passCar){
                        tempCar.beingPassed = false;
                        tempCar = tempCar.inFront;
                    }
                    passCar.beingPassed = false;
                }
                //drive accordingly
                if(passingPhase <= 16){
                    speed += accel;
                } else {
                    Car frontFront = passCar.inFront;
                    if ((frontFront != null) && (coords.Distance(frontFront.coords) < 10+safeDistanceMult*speed)){
                        speed -= 5*accel;
                        speed = speed < 0 ? 0 : speed;
                    }
                }
                speed = speed > maxSpeed ? maxSpeed : speed;
                X += speed*Math.Sign(distX);
                Y += speed*Math.Sign(distY);
            #endregion
            #region if not passing
            } else { //dont convert this to an else if
                if (beingPassed) {
                    //actually, do nothing
                } else if (waitingForAllow && (dist < 50*speed || dist < 1)){
                    //waiting for crossroad to clear way, stop at current destination
                    double t=2*dist/speed;
                    t = t > 1 ? t : 1; //cant be other way, because of handling NaN
                    speed -= speed/t;
                } else if (next.isTurn() && dist < 50*speed) {
                    //close to a turn, slow down to turning speed at current destination
                    double maxTSpeed = next.type == ItinType.TurnLeftTo ? maxLTurnSpeed : maxRTurnSpeed;
                    if (speed > maxTSpeed){
                        double t=2*dist/(maxTSpeed+speed);
                        t = t > 1 ? t : 1; //cant be other way, because of handling NaN
                        speed -= (speed-maxTSpeed)/t;
                    } else {
                        speed = speed + accel;
                        speed = speed > maxTSpeed ? maxTSpeed : speed;
                    }
                } else if (next.type == ItinType.SpeedLimit && dist < 50*speed){
                    //close to a slowdown sign, slow down to its speed
                    double nextMaxSpeed = ((double)next.dest.x)/145;
                    if (speed > nextMaxSpeed){
                        double t=2*dist/(nextMaxSpeed+speed);
                        t = t > 1 ? t : 1; //cant be other way, because of handling NaN
                        speed -= (speed-nextMaxSpeed)/t;
                    } else {
                        speed = speed + accel;
                        speed = speed > nextMaxSpeed ? nextMaxSpeed : speed;
                    }
                } else if ((itinerary.route.Count <= 1) && dist < 50*speed){ //should be 1
                    //close to a final depot, stop at current destination
                    double t=2*dist/speed;
                    t = t > 1 ? t : 1; //cant be other way, because of handling NaN
                    speed -= speed/t;
                    speed = speed < 0.01 ? 0.01 : speed; //keep at least minimum speed to actually reach destination
                } else {
                    //just driving :]
                    speed = speed + accel;
                }
                speed = speed > maxSpeed ? maxSpeed : speed;
                if((inFront != null) && (coords.Distance(inFront.coords) < 10+safeDistanceMult*speed)){
                    if(canPass()){
                        //initiate passing
                        Car tempCar = inFront;
                        while(tempCar != passCar){
                            tempCar.beingPassed = true;
                            tempCar = tempCar.inFront;
                        }
                        passCar.beingPassed = true;
                        passing = true;
                        passingPhase = 0;
                        Car behind = cross.getCarBehind( crossComingFrom, this );
                        if(behind != null){
                            behind.inFront = passCar;
                        }
                        //swap these even in crossroad lists
                        cross.putCarInFrontOf(crossComingFrom, this, passCar);
                    } else {
                        speed -= 5*accel;
                        speed = speed < 0 ? 0 : speed;
                    }
                }
                #endregion
                #endregion
                #region follow path
                double distToTravel = speed;
                while(distToTravel > dist){
                    //all encountering roads code here
                    if(itinerary.route.Count <= 1){
                        List<Car> qu = cross.incomCars[CoOrds.oppDir(direction/3)];
                        qu.Remove(this);
                        if(qu.Count > 0) {
                            Car first = qu.First();
                            while(first.passing){
                                first = cross.getCarBehind(crossComingFrom, first);
                            }
                            first.inFront = null;
                        }
                        return true; //end of path
                    }
                    distToTravel -= dist;
                    ProcessSpecItin(cur);
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
                #endregion
            }
            _coords = new CoOrds((int)X,(int)Y);
            timeAlive++; totalDistTravelled += speed;
            return false;
        }
        
        private void ProcessSpecItin(ItinPart cur){
            switch (cur.type){ //all special itin parts, deploy here
                        //these trigger when ItinPart is reached
                        case ItinType.AskCrossroad:
                            waitingForAllow = true;
                            break;
                        case ItinType.EnterCrossroad:
                            //Dequeue from crosses queue and enqueue into next one.
                            cross.incomCars[cur.dest.x].Remove(this);
                            ((Crossroad)cross).lastPassed = this;
                            MapItem newCross = cross.connObjs[cur.dest.y];
                            inFront = newCross.incomCars[newCross.getDirOf(cross)].LastOrDefault();
                            newCross.incomCars[newCross.getDirOf(cross)].Add(this);
                            setCross(newCross, newCross.getDirOf(cross));
                            maxSpeed = _actualMaxSpeed; passingAllowed = true; //cancel whats forbidden
                            break;
                        case ItinType.SpeedLimit:
                            _maxSpeed = ((double)cur.dest.x)/145;
                            break;
                        case ItinType.NoPassing:
                            passingAllowed = false;
                            break;
                        default:
                            X = cur.dest.x; Y = cur.dest.y;
                            break;
                    }
        }

        private static double getDist(ItinPart itPart, double X, double Y){
            double devNull; double totalDist;
            getDist(itPart, X, Y, 0, out devNull, out devNull, out totalDist);
            return totalDist;
        }
        private void getDist(ItinPart itPart, out double distX, out double distY, out double totalDist){
            getDist(itPart, this.X, this.Y, this.turnProgress, out distX, out distY, out totalDist);
        }
        private static void getDist(ItinPart itPart, double X, double Y, double turnProgress, out double distX, out double distY, out double totalDist){
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
                case ItinType.EnterCrossroad:
                case ItinType.SpeedLimit:
                default:
                    distX = 0; distY = 0; totalDist = 0;
                    return;
            }
        }

        /// <summary>
        /// Function counting if the car/cars in front of this car are passable. Sets variable passCar in progress.
        /// </summary>
        /// <returns>True if passing can be initiated.</returns>
        /*private bool canPass(bool boo = true){ //FOR DEBUGGING
            if(boo && canPass(false)){
                int abc = 5; //PUT BREAKPOINT HERE, WILL BREAK ONLY IF RESULT IS TRUE
            }
        */
        private bool canPass(){
            passCar = inFront;
            if(passing || beingPassed || (totalDistTravelled < 32) || (itinerary.route.First().type != ItinType.GoTo) || !passingAllowed){
                return false;
            } else {
                #region figure out how many cars to pass and how long of a stretch we need
                bool goOn = false;
                do{
                    if ( !passCar.itinerary.route.First().dest.Equals( itinerary.route.First().dest) ||
                        passCar.beingPassed || passCar.passing || (maxSpeed < passCar.maxSpeed) ) { return false; }
                    if ((passCar.inFront != null) &&
                        (passCar.coords.Distance(passCar.inFront.coords) < passCar.speed*safeDistanceMult+30)){
                        passCar = passCar.inFront;
                        goOn = true;
                    } else {
                        goOn = false;
                    }
                } while (goOn);
                while((passCar.inFront != null) &&
                    (passCar.coords.Distance(passCar.inFront.coords) < passCar.speed*safeDistanceMult+30)){
                    if (!passCar.inFront.itinerary.route.First().dest.Equals( itinerary.route.First().dest ) || 
                        passCar.beingPassed) {return false;}
                    passCar = passCar.inFront;
                }
                int inFrontNeeded = (int)Math.Round(passCar.speed*safeDistanceMult+10);
                //relative distance and speed between me and where i am supposed to be after passing
                double relDist = passCar.coords.Add(CoOrds.fromDir(passCar.direction/3).Multiply(inFrontNeeded)).Distance(coords);
                double relSpeed = speed - passCar.speed;
                double relMaxSpeed = maxSpeed - passCar.speed;

                double tToSpeed = (maxSpeed-speed)/accel;
                double reldToSpeed = (relSpeed+(maxSpeed-speed)/2)*tToSpeed;
                double dToSpeed = (maxSpeed+speed)/2*tToSpeed;

                if(reldToSpeed <= relDist){
                    //all cool, calculate rest of the speeds
                    tToSpeed = (relDist-reldToSpeed)/relMaxSpeed;
                    dToSpeed += maxSpeed*tToSpeed;
                    dToSpeed += maxSpeed*20; //it takes 16 ticks to get back to own lane
                } else {
                    //troublesome, need to solve a quadratic equation
                    double devNull;
                    CustomMath.solveQuadraticEquation(accel, speed, -relDist, out tToSpeed, out devNull);
                    dToSpeed = (speed+maxSpeed)/2*tToSpeed;
                    dToSpeed += (speed + accel*tToSpeed)*20; // reserve for further slowing down
                }

                if ((itinerary.route.Count >= 2) && (itinerary.route.ElementAt(1).type == ItinType.EnterCrossroad || itinerary.route.ElementAt(1).type == ItinType.AskCrossroad)){
                    //if the following is a crossroad, dont be as hasty
                    //ToDo: LowPriority: search for the crossroad further in itinerary
                    dToSpeed *= 1.5;
                }
                
                #endregion
                //we have set values: dToSpeed - distance in pixels needed to pass car
                if(itinerary.route.First().dest.Distance(this.coords) < dToSpeed){
                    return false;
                }
                
                //So far all conditions have been met
                #region Consider cars driving towards you
                //int i = itinerary.route.Count-1; //should be at least 0
                //ItinPart ip = itinerary.route.ElementAt(i--);
                MapItem behindCross = cross.connObjs[ crossComingFrom ];
                List<Car> list = behindCross.incomCars[ behindCross.getDirOf(cross) ];
                int dir = direction/3;
                //myCoords are coordinates switched to the other lane
                CoOrds myCoords = coords. Subtract ( directionOffset(dir,true) ). Add ( directionOffset( CoOrds.oppDir(dir),false ));

                foreach(Car car in list){
                    //This loops through cars from first to last
                    double distFrom = 0;
                    CoOrds coBefore = car.coords;
                    bool broke = false;

                    foreach(ItinPart itOther in car.itinerary.route){
                        //Add distance of ItinParts, till you get to the same ItinPart as this
                        if(itOther.type == ItinType.EnterCrossroad){
                            break; //the cars cant meet ever
                        }
                        CoOrds vec1 = coBefore.Subtract(myCoords);
                        CoOrds vec2 = myCoords.Subtract(itOther.dest);
                        if( vec1.Normalize() .Equals (vec2.Normalize()) ){
                            broke = true; break; //i am between it and its destination
                        }
                        distFrom += getDist(itOther, coBefore.x, coBefore.y);
                        coBefore = itOther.dest;
                    }
                    if(broke){
                        distFrom += coBefore.Distance( myCoords );
                        //two rough cuts... accelerating all the way
                        double willTravel = (accel*tToSpeed/2 + speed) * tToSpeed;
                        if(distFrom < willTravel+dToSpeed){
                            return false;
                        }
                    }
                }
                #endregion
                return true;
            }
        }

        public Car Clone(){
            return new Car(_maxSpeed,X,Y,coords,direction,basicPath.Clone(),itinerary.Clone() );
        }
    }
}