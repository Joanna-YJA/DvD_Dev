using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvD_Dev
{
    class CoordinateTransformationFilter : ICoordinateSequenceFilter
    {
        private double height;

        ///<summary>
        /// Yes, always true.
        /// </summary>
        public bool Done => true;

        /// <summary>
        /// Automatic call IGeometry.GeometryChanged() method after tranformation.
        /// </summary>
        public bool GeometryChanged => true;
        public CoordinateTransformationFilter(double height)
        {
            this.height = height;
        }
        public void Filter(CoordinateSequence seq, int i)
        {
           var (x, y) = (seq.GetX(i), seq.GetY(i));
            seq.SetX(i, x);
            seq.SetY(i, y);
            seq.SetZ(i, height);
           // seq.Set
        }
    }
}
