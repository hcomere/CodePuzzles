using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingGame_engine_MarsLander
{
    class Vector2
    {
        ///////////////////////////////////////////////////////////////////////
        ////////////////////////// Instance Fields ////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public double X { get; private set; }
        public double Y { get; private set; }

        ///////////////////////////////////////////////////////////////////////
        ////////////////////////// Constructors ///////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public Vector2(double a_x, double a_y)
        {
            X = a_x;
            Y = a_y;
        }

        ///////////////////////////////////////////////////////////////////////
        ////////////////////////// Static Methods /////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public static double Distance2(Vector2 a_v1, Vector2 a_v2)
        {
            return (a_v2.X - a_v1.X) * (a_v2.X - a_v1.X) + (a_v2.Y - a_v1.Y) * (a_v2.Y - a_v1.Y);
        }

        ///////////////////////////////////////////////////////////////////////
        ////////////////////////////// Methods ////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public void Copy(Vector2 a_toCopy)
        {
            X = a_toCopy.X;
            Y = a_toCopy.Y;
        }
        
        public void Set(double a_x, double a_y)
        {
            X = a_x;
            Y = a_y;
        }

        public double Length2()
        {
            return X * X + Y * Y;
        }

        public double Length()
        {
            return Math.Sqrt(Length2());
        }

        public void Normalize()
        {
            double length = Length();
            X /= length;
            Y /= length;
        }

        ///////////////////////////////////////////////////////////////////////
        ////////////////// Object Virtual Methods Override ////////////////////
        ///////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }
    }
}
