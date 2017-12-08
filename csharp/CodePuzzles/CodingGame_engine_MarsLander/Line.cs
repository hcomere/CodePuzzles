using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingGame_engine_MarsLander
{
    class Line
    {
        ///////////////////////////////////////////////////////////////////////
        ////////////////////////// Instance Fields ////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public double StartX { get; private set; }
        public double EndX { get; private set; }
        public double StartY { get; private set; }
        public double EndY { get; private set; }
        public double MiddleX { get; private set; }
        public double MiddleY { get; private set; }

        private double A { get; set; }
        private double B { get; set; }

        ///////////////////////////////////////////////////////////////////////
        ////////////////////////// Constructors ///////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public Line(double a_startX, double a_startY, double a_endX, double a_endY)
        {
            StartX = a_startX;
            StartY = a_startY;
            EndX = a_endX;
            EndY = a_endY;

            MiddleX = (a_startX + a_endX) * 0.5;
            MiddleY = (a_startY + a_endY) * 0.5;

            A = (EndY - StartY) / (EndX - StartX);
            B = StartY - StartX * A;
        }

        ///////////////////////////////////////////////////////////////////////
        ////////////////////////// Static Methods /////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public static bool Collide(Line l1, Line l2)
        {
            if (Math.Max(l1.StartX, l1.EndX) < Math.Min(l2.StartX, l2.EndX))
                return false;
            
            if (l1.A == l2.A)
                return false;

            double x = (l2.B - l1.B) / (l1.A - l2.A);
            double y = l1.A * x + l1.B;

            double xmin = Math.Max(Math.Min(l1.StartX, l1.EndX), Math.Min(l2.StartX, l2.EndX));
            double xmax = Math.Min(Math.Max(l1.StartX, l1.EndX), Math.Max(l2.EndX, l2.EndX));

            return x >= xmin && x <= xmax;
        }

        ///////////////////////////////////////////////////////////////////////
        ////////////////////////////// Methods ////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public bool IsHorizontal()
        {
            return StartY == EndY;
        }

        ///////////////////////////////////////////////////////////////////////
        ////////////////// Object Virtual Methods Override ////////////////////
        ///////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return "Line (" + StartX + "," + StartY + ") to (" + EndX + "," + EndY + ")";
        }
    }
}
