using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarSim
{
    /// <summary>
    /// Static Framerate Counter.
    /// </summary>
    class FPSCounter
    {
        /// <summary>
        /// Send after frame has been processed.
        /// </summary>
        /// <returns>Frames processed in the last second.</returns>
        public static int Tick(){
            if (System.Environment.TickCount - lastTick >= 1000)
            {
                lastFrameRate = frameRate;
                frameRate = 0;
                lastTick = System.Environment.TickCount;
            }
            frameRate++;
            return lastFrameRate;
        }

        private static int lastTick;
        private static int lastFrameRate;
        private static int frameRate;
    }
}
