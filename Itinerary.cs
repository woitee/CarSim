using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Itinerary{
        public Queue<ItinPart> route;
        public Itinerary(Queue<ItinPart> route){
            this.route = route;
        }
        public Itinerary Clone(){
            Queue<ItinPart> newRoute = new Queue<ItinPart>(route);
            return new Itinerary(newRoute);
        }
    }

    public class ItinPart{
        public ItinType type;
        public CoOrds dest;

        public ItinPart(){
            this.type = ItinType.GoTo;
            //rest is null
        }
        public ItinPart(ItinType type, CoOrds dest){
            this.type = type;
            this.dest = dest;
        }
        public bool isTurn(){
            return ((this.type == ItinType.TurnLeftTo) || (this.type == ItinType.TurnRightTo));
        }
    }

    public enum ItinType{
        GoTo, TurnLeftTo, TurnRightTo, AskCrossroad, EnterCrossroad, SpeedLimit
    }
}
