using DJI.WindowsSDK;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using static DvD_Dev.SpatialMath;

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

        private float halfHeight;
        private float halfBase;

        private static SimpleLineSymbol rectOutline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Green, 1.0);
        private static SimpleFillSymbol seperateRectSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Green, rectOutline);

        private static SimpleLineSymbol pathOutline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.LightGreen, 1.0);
        private static SimpleFillSymbol pathSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.LightGreen, pathOutline);


        public FootprintCalculator()
        {
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
            float fpBase = (float)( altitudeM * (Math.Tan(angleR) - Math.Tan(angleL)) );
            halfBase = fpBase / 2;

            double angleU = pitchRad + 0.5 * VFVRad;
            double angleD = pitchRad - 0.5 * VFVRad;
            float fpHeight = (float)(altitudeM * (Math.Tan(angleU) - Math.Tan(angleD)));
            halfHeight = fpHeight / 2;
        }

        public List<MapPoint> FindFootprintInnerSide(ref List<MapPoint> points)
        {
            List<MapPoint> OneSideList = new List<MapPoint>();

            MapPoint curr = points[0];
            MapPoint innerSide = new MapPoint(curr.X - halfBase, curr.Y - halfHeight, 0, PathFinder.spatialRef);
            OneSideList.Add(innerSide);

            int signY = -1, signX = 1;
            bool nextToChangeY = false;
            foreach (MapPoint p in points.GetRange(1, points.Count - 1))
            {
                innerSide = new MapPoint(p.X + (signX * halfBase), p.Y + (signY * halfBase), p.Z, PathFinder.spatialRef); //TODO
                OneSideList.Add(innerSide);

                if (nextToChangeY) signY *= -1;
                else signX *= -1;
                nextToChangeY = !nextToChangeY;
            }

            //In the inner side of the coverage area, the last point is special case
            MapPoint last = OneSideList.Last();
            last = new MapPoint(last.X -  halfBase * 2, last.Y, last.Z, last.SpatialReference);
            OneSideList.Add(last);

            return OneSideList;
        }

        public Stack<MapPoint> FindFootprintOuterSide(ref List<MapPoint> points)
        {
            Stack<MapPoint> OtherSideStack = new Stack<MapPoint>();

            int signY = 1, signX = -1;
            bool nextToChangeY = false;

            MapPoint curr = points[0];
            MapPoint outerSide = new MapPoint(curr.X - halfBase, curr.Y - halfHeight, curr.Z, PathFinder.spatialRef); //Todo
            OtherSideStack.Push(outerSide);

            foreach (MapPoint p in points.GetRange(1, points.Count - 1))
            {
                outerSide = new MapPoint(p.X + (signX * halfBase), p.Y + (signY * halfBase), p.Z, PathFinder.spatialRef); //TODO change coverage height
                OtherSideStack.Push(outerSide);

                if (nextToChangeY) signY *= -1;
                else signX *= -1;
                nextToChangeY = !nextToChangeY;
            }

            // DrawFigure(new List<Vector3>(OtherSideStack), Color.FromArgb(255, 0, 0, 255));
            return OtherSideStack;
        }

        public void ShowFootprintCoverage(ref List<MapPoint> points, GraphicsOverlay overlay)
        {
            List<MapPoint> OneSideList = FindFootprintInnerSide(ref points); //new List<Vector3>();  
            Stack<MapPoint> OtherSideStack = FindFootprintOuterSide(ref points);  //new Stack<Vector3>(); 

            // OneSideList.Concat(OtherSideStack);
            while (OtherSideStack.Count > 0)
                OneSideList.Add(OtherSideStack.Pop());

            OneSideList.Add(OneSideList.First()); //Add back the first point to form a complete polygon

            Polygon coverage = new Polygon(OneSideList);
            Graphic coverageGraphic = new Graphic(coverage, pathSymbol);
            overlay.Graphics.Add(coverageGraphic);
        }


        public void ShowSeperateFootprint(ref List<MapPoint> points, GraphicsOverlay overlay)
        {
            Boolean isOrigOrient = true;
            double yVal, xVal;

            foreach (MapPoint p in points)
            {
                if (isOrigOrient)
                {
                    yVal = halfHeight;
                    xVal = halfBase;
                }
                else
                {
                    yVal = halfBase;
                    xVal = halfHeight;
                }
                List<MapPoint> fourPoints = new List<MapPoint>();
                fourPoints.Add(new MapPoint(p.X - xVal, p.Y - yVal, p.Z, PathFinder.spatialRef)); //TODO footprint height change to z = 0
                fourPoints.Add(new MapPoint(p.X + xVal, p.Y - yVal, p.Z, PathFinder.spatialRef));
                fourPoints.Add(new MapPoint(p.X + xVal, p.Y + yVal, p.Z, PathFinder.spatialRef));
                fourPoints.Add(new MapPoint(p.X - xVal, p.Y + yVal, p.Z, PathFinder.spatialRef));
                fourPoints.Add(fourPoints.First()); //Add back the first point to form a complete polygon

                Polygon rect = new Polygon(fourPoints);
                Graphic rectGraphic = new Graphic(rect, seperateRectSymbol);
                overlay.Graphics.Add(rectGraphic);
                isOrigOrient = !isOrigOrient;
            }
        }

        public float getHalfBase()
        {
            return halfBase;
        }

        public float getHalfHeight()
        {
            return halfHeight;
        }


    }
}
