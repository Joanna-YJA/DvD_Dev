using NetTopologySuite.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DvD_Dev
{
    public static class Vector3DExtensions
    {
        public static double Get(this Vector3D vector, int i)
        {
            if (i == 0) return vector.X;
            else if (i == 1) return vector.Y;
            else if (i == 2) return vector.Z;
            else throw new ArgumentException("Index i is not in range");
        }

        public static void Set(this Vector3D vector, int i, float val)
        {
            if (i == 0) vector = new Vector3D(val, vector.Y, vector.Z);
            else if (i == 1) vector = new Vector3D(vector.X, val, vector.Z);
            else if (i == 2) vector = new Vector3D(vector.X, vector.Y, val);
            else throw new ArgumentException("Index i is not in range");
        }
    }
}
