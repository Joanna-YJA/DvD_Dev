
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace DvD_Dev
{
    public class U
    {

        public static float Det(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        public static List<T> InverseList<T>(List<T> list)
        {
            if (list == null) return null;
            List<T> result = new List<T>();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                result.Add(list[i]);
            }
            return result;
        }

        public static float Sq(float n)
        {
            return n * n;
        }

        public static float limitedDeltaTime
        {
            //get { return MathF.Min(Time.deltaTime, 0.1f); }
            get { return 0.1f; }
        }
    }


}