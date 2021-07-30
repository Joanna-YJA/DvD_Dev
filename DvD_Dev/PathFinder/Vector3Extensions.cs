using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DvD_Dev
{
    public static class Vector3Extensions
    {
        public static float Get(this Vector3 vector, int i)
        {
            if (i == 0) return vector.X;
            else if (i == 1) return vector.Y;
            else if (i == 2) return vector.Z;
            else throw new ArgumentException("Index i is not in range");
        }

        public static void Set(ref this Vector3 vector, int i, float val)
        {
            if (i == 0) vector.X = val;
            else if (i == 1) vector.Y = val;
            else if (i == 2) vector.Z = val;
            else throw new ArgumentException("Index i is not in range");
        }
    }
}
