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
    }

    public struct ItinPart{
        public ItinType type;
        public CoOrds dest;
        public double speed;
        public ItinPart(ItinType type, CoOrds dest, double speed){
            this.type = type;
            this.dest = dest;
            this.speed = speed;
        }
    }

    public enum ItinType{
        GoTo, TurnTo, AskCrossroad
    }
}
