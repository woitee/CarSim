using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Itinerary{
        public ItinPart[] list;
    }

    public struct ItinPart{
        ItinType type;
        double speed;
        public ItinPart(ItinType type, double speed){
            this.type = type;
            this.speed = speed;
        }
    }

    public enum ItinType{
        GoTo, SpeedUp, SlowDown
    }
}
