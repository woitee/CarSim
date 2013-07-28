using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    class Sign
    {
        public SignType type;
        
        public Sign(SignType type){
            this.type = type;
        }
        public static SignType typeString(string name){
            switch(name){
                case "noway":
                    return SignType.Noway;
                case "oneway":
                    return SignType.Oneway;
                case "notthisway":
                    return SignType.Notthisway;
                case "nopass":
                    return SignType.Nopass;
                case "max30":
                    return SignType.Max30;
                case "max60":
                    return SignType.Max60;
                case "max90":
                    return SignType.Max90;
                case "stop":
                    return SignType.Stop;
                case "mainway":
                    return SignType.Mainway;
                case "giveway":
                    return SignType.Giveway;
                default:
                    return SignType.Undefined;
            }
        }
        public override string ToString()
        {
            string line = base.ToString();
            return line.ToLowerInvariant();
        }
        public bool isSpeedSign(){
            return ((type == SignType.Max90) || (type == SignType.Max60) || (type == SignType.Max30));
        }
    }
    public enum SignType
    {
        Notthisway, Noway, Nopass, Max30, Max60, Max90, Stop, Mainway, Giveway, Oneway, Undefined
    }
}
