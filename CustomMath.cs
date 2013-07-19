using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSim
{
    //static class for some neccesary math functions
    static class CustomMath
    {
        /// <summary>
        /// Calculates result of a quadratic equation of type a*x^2 + b*x + c = 0.
        /// </summary>
        /// <param name="a">Quadratic coefficient of the equation.</param>
        /// <param name="b">Linear coefficient of the equation.</param>
        /// <param name="c">Constant coefficient of the equation.</param>
        /// <param name="x1">Outputs the first and bigger solution, or NaN if none exists.</param>
        /// <param name="x2">Outputs the second and smaller solution, or NaN if none exists.</param>
        public static void solveQuadraticEquation(double a, double b, double c, out double x1, out double x2){
            double D = b*b - 4*a*c;
            if(D<0){
                x1 = double.NaN;
                x2 = double.NaN;
                return;
            } else {
                x1 = (-b + Math.Sqrt(D))/(2*a);
                x2 = (-b - Math.Sqrt(D))/(2*a);
                return;
            }
        }
    }
}
