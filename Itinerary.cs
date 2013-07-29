using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    /// <summary>
    /// Class containing a queue of temporary goals for the car to reach.
    /// </summary>
    class Itinerary{
        public Queue<ItinPart> route;
        public Itinerary(Queue<ItinPart> route){
            this.route = route;
        }
        public Itinerary Clone(){
            Queue<ItinPart> newRoute = new Queue<ItinPart>();
            foreach(ItinPart ip in route){
                newRoute.Enqueue(ip.Clone());
            }
            return new Itinerary(newRoute);
        }
    }

    /// <summary>
    /// A temporary goal description. Describes the type and target of the goal.
    /// </summary>
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
        public ItinPart Clone(){
            return new ItinPart(type, dest);
        }
    }

    /// <summary>
    /// List of possible short term goals for the car.
    /// </summary>
    public enum ItinType{
        GoTo, TurnLeftTo, TurnRightTo, AskCrossroad, EnterCrossroad, SpeedLimit, NoPassing
    }
}
