using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DvD_Dev
{
    class FootprintCalculator
    {
        private double xsensorMm = 36;
        private double ysensorMm = 24;
        private double altitudeM = 30;

        public double focalLenMm = 50;

        private static double yawRad = 0;
        private static double pitchRad = 0;
        private static double HFVRad;
        private static double VFVRad;

        public float top, bott, left, right;

        private static SimpleLineSymbol rectOutline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(48, 0, 255, 40), 1.0);
        private static SimpleFillSymbol seperateRectSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(48, 0, 255, 40), rectOutline);

        private static SimpleLineSymbol pathOutline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(255, 211, 205, 23), 1.0);
        private static SimpleFillSymbol pathSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(54, 255, 247, 0), pathOutline);

        public FootprintCalculator(double altitudeM)
        {
            this.altitudeM = altitudeM;
            yawRad = LimitToRange(yawRad, -80, 80);
            pitchRad = LimitToRange(pitchRad, -80, 80);

            HFVRad = FindHFV();
            VFVRad = FindVFV();

            FindFootprintDimensions();
        }

        private static double LimitToRange(double value, double incMin, double incMax)
        {
            if (value < incMin) return incMin;
            if (value > incMax) return incMax;
            return value;
        }

        private double FindHFV() //Horizontal Field-Of-View
        {
            double angle = xsensorMm / (2 * focalLenMm);
            return 2 * Math.Atan(angle);
        }

        private double FindVFV() //Vertical Field-Of-View
        {
            double angle = ysensorMm / (2 * focalLenMm);
            return 2 * Math.Atan(angle);
        }

        public void FindFootprintDimensions()
        {
            double angleR = yawRad + 0.5 * HFVRad;
            double angleL = yawRad - 0.5 * HFVRad;
            right = (float)(altitudeM * Math.Tan(angleR));
            right = Math.Abs(right);
            left = (float)(altitudeM * Math.Tan(angleL));
            left = Math.Abs(left);

            double angleT = pitchRad + 0.5 * VFVRad;
            double angleB = pitchRad - 0.5 * VFVRad;
            top = (float)(altitudeM * Math.Tan(angleT));
            top = Math.Abs(top);
            bott = (float)(altitudeM * Math.Tan(angleB));
            bott = Math.Abs(bott);
        }

        public List<MapPoint> FindFootprintInnerSide(List<MapPoint> points)
        {
            List<MapPoint> oneSideList = new List<MapPoint>();
            MapPoint p, innerSide;

            int j = points.Count - 1;
            while(points[j] != null)
            {
                p = points[j];
                innerSide = new MapPoint(p.X + right, p.Y - bott, 0.01, PathFinder.spatialRef);
                oneSideList.Add(innerSide);
                j--;
            }

            int signX = 1, signY = -1;
            float xChange = right, yChange = right;
            bool nextToChangeX = true;
            for(int i = j - 1; i >= 0; i--)
            {
                p = points[i];
                if(p == null)
                {
                    if (nextToChangeX) signX *= -1;
                    else signY *= -1;

                    nextToChangeX = !nextToChangeX;
                } else
                {
                    innerSide = new MapPoint(p.X + (signX * xChange), p.Y + (signY * yChange), 0.01, PathFinder.spatialRef);                                                                                                              //test
                    oneSideList.Add(innerSide);
                }
            }
            return oneSideList;
        }

        public Stack<MapPoint> FindFootprintOuterSide(List<MapPoint> points)
        {
            Stack<MapPoint> otherSideStack = new Stack<MapPoint>();
            MapPoint p, outerSide;

            int j = points.Count - 1;
            while (points[j] != null)
            {
                p = points[j];
                outerSide = new MapPoint(p.X - left, p.Y - bott, 0.01, PathFinder.spatialRef);
                otherSideStack.Push(outerSide);
                j--;
            }

            int signX = -1, signY = 1;
            float xChange = left, yChange = left;
            bool nextToChangeX = true;
            for(int i = j - 1; i >= 0; i--)
            {
                p = points[i];
                if(p == null)
                {
                    if (nextToChangeX) signX *= -1;
                    else signY *= -1;
                    nextToChangeX = !nextToChangeX;

                } else
                {
                    outerSide = new MapPoint(p.X + (signX * xChange), p.Y + (signY * yChange), 0.01, PathFinder.spatialRef);
                    otherSideStack.Push(outerSide);
                }
            }
            return otherSideStack;
        }

        public void ShowFootprintCoverage(List<MapPoint> points, GraphicsOverlay overlay)
        {
            List<MapPoint> OneSideList = FindFootprintInnerSide(points);  
            Stack<MapPoint> OtherSideStack = FindFootprintOuterSide(points);  

            Polyline lineOne = new Polyline(OneSideList);
            Graphic lineOneGraphic = new Graphic(lineOne, new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 1.0));
            overlay.Graphics.Add(lineOneGraphic);

            Graphic g = new Graphic(points[points.Count - 3], new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, Color.HotPink, 10));
            overlay.Graphics.Add(g);

            Polyline lineTwo = new Polyline(OtherSideStack.ToList());
            Graphic lineTwoGraphic = new Graphic(lineTwo, new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Green, 1.0));
           overlay.Graphics.Add(lineTwoGraphic);

            while (OtherSideStack.Count > 0)
                OneSideList.Add(OtherSideStack.Pop());

            OneSideList.Add(OneSideList.First()); //Add back the first point to form a complete polygon

            Geometry coverage = new Polygon(OneSideList);
            Graphic coverageGraphic = new Graphic(coverage, pathSymbol);
            overlay.Graphics.Add(coverageGraphic);

            //Calculate footprint coverage
            coverage = GeometryEngine.Clip(coverage, PathFinder.env);
            

            double fieldArea = PathFinder.fieldDimM * PathFinder.fieldDimM;
            foreach(Geometry obs in PathFinder.flatGeometries)
            {
                //Minus the area of each building to find the actual area of field
                Geometry clippedObs = GeometryEngine.Clip(obs, PathFinder.env);
                double obsArea = GeometryEngine.Area(clippedObs);
                fieldArea -= obsArea;

                //Minus the area of coverage that goes above/under the building
                //As we are not considering coverage of buildings
                coverage = GeometryEngine.Difference(coverage, clippedObs);
            }

            double coverageArea = Math.Abs(GeometryEngine.Area(coverage));
            double percentCovered = coverageArea / fieldArea * 100;
            percentCovered = Math.Min(100, percentCovered);
            System.Diagnostics.Debug.WriteLine("% Coverage: " + percentCovered);
        }


        public void ShowSeperateFootprint(IEnumerable<MapPoint> points, GraphicsOverlay overlay)
        {
            //Direction[] fourDir = new Direction[] {}
            //    bool isFirst = false;
            //    double yVal, xVal;
            //    Direction dir  
            //    MapPoint prev = null;

            //    foreach (MapPoint p in points)
            //    {
            //        if (p == null)
            //        {
            //            isOrigOrient = !isOrigOrient;
            //            isFirst = true;
            //        }
            //        else
            //        {
            //            prev = p;
            //            List<MapPoint> fourPoints = new List<MapPoint>();

            //            if (isOrigOrient)
            //            {

            //                fourPoints.Add(new MapPoint(p.X - xVal, p.Y - yVal, 0, PathFinder.spatialRef)); //TODO footprint height change to z = 0
            //                fourPoints.Add(new MapPoint(p.X + xVal, p.Y - yVal, 0, PathFinder.spatialRef));
            //                fourPoints.Add(new MapPoint(p.X + xVal, p.Y + yVal, 0, PathFinder.spatialRef));
            //                fourPoints.Add(new MapPoint(p.X - xVal, p.Y + yVal, 0, PathFinder.spatialRef));
            //            }
            //            else
            //            {

            //                fourPoints.Add(new MapPoint(p.X - xVal, p.Y - yVal, 0, PathFinder.spatialRef)); //TODO footprint height change to z = 0
            //                fourPoints.Add(new MapPoint(p.X + xVal, p.Y - yVal, 0, PathFinder.spatialRef));
            //                fourPoints.Add(new MapPoint(p.X + xVal, p.Y + yVal, 0, PathFinder.spatialRef));
            //                fourPoints.Add(new MapPoint(p.X - xVal, p.Y + yVal, 0, PathFinder.spatialRef));
            //            }

            //            fourPoints.Add(fourPoints.First()); //Add back the first point to form a complete polygon

            //            Polygon coverage = new Polygon(fourPoints);
            //            Graphic graphic = new Graphic(coverage, seperateRectSymbol);
            //            overlay.Graphics.Add(graphic);

            //            if (isFirst)
            //            {
            //                bool opp = !isOrigOrient;
            //                if (opp)
            //                {
            //                    yVal = halfHeight;
            //                    xVal = halfBase;
            //                }
            //                else
            //                {
            //                    yVal = halfBase;
            //                    xVal = halfHeight;
            //                }

            //                List<MapPoint> oppPoints = new List<MapPoint>();
            //                oppPoints.Add(new MapPoint(prev.X - xVal, prev.Y - yVal, 0, PathFinder.spatialRef)); //TODO footprint height change to z = 0
            //                oppPoints.Add(new MapPoint(prev.X + xVal, prev.Y - yVal, 0, PathFinder.spatialRef));
            //                oppPoints.Add(new MapPoint(prev.X + xVal, prev.Y + yVal, 0, PathFinder.spatialRef));
            //                oppPoints.Add(new MapPoint(prev.X - xVal, prev.Y + yVal, 0, PathFinder.spatialRef));
            //                oppPoints.Add(oppPoints.First()); //Add back the first point to form a complete polygon

            //                Polygon oppCoverage = new Polygon(oppPoints);
            //                Graphic oppGraphic = new Graphic(oppCoverage, seperateRectSymbol);
            //                overlay.Graphics.Add(oppGraphic);
            //                isFirst = false;
            //            }
            //        }
            //    }
            }
        }
}
