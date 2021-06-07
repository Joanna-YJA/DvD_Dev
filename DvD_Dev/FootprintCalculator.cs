using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private static double yawRad = Math.Min(DegreesToRadians(45), DegreesToRadians(80));
        private static double pitchRad = Math.Min(DegreesToRadians(0), DegreesToRadians(80));
        private static double HFVRad;
        private static double VFVRad;

        private double fpBase;
        private double fpHeight;
        private double[] fpDimensions;

        private double halfBaseLon;
        private double halfBaseLat;

        private double halfHeightLat;
        private double halfHeightLon;

        private double FindHFV()
        {
            double angle = xsensorMm / (2 * focalLenMm);
            return 2 * Math.Atan(angle);
        }

        private double FindVFV()
        {
            double angle = ysensorMm / (2 * focalLenMm);
            return 2 * Math.Atan(angle);
        }

        public FootprintCalculator() {
            HFVRad = FindHFV();
            VFVRad = FindVFV();
        }

        public FootprintCalculator(double altitudeM, double focalLenMm, double xsensorMMm, double ysensorMm)
        {
            altitudeM = altitudeM;
            focalLenMm = focalLenMm;
            xsensorMm = xsensorMm;
            ysensorMm = ysensorMm;
            HFVRad = FindHFV();
            VFVRad = FindVFV();
        }
         
        public FootprintCalculator(double altitudeM, double HFVDeg, double VFVDeg)
        {
            altitudeM = altitudeM;
            HFVRad = DegreesToRadians(HFVDeg);
            VFVRad = DegreesToRadians(VFVDeg);
        }

        public void FindFootprintDimensions(BasicGeoposition initCoord)
        {
            double angleR = yawRad + 0.5 * HFVRad;
            double angleL = yawRad - 0.5 * VFVRad;
            fpBase = altitudeM * (Math.Tan(angleR) - Math.Tan(angleL));
            fpBase = fpBase / 1000 * 2;

            fpDimension 

            double angleU = pitchRad + 0.5 * VFVRad;
            double angleD = pitchRad - 0.5 * VFVRad;
            fpHeight = altitudeM * (Math.Tan(angleU) - Math.Tan(angleD));
            fpHeight = fpHeight / 1000 * 2;

            System.Diagnostics.Debug.WriteLine("fpBase: " + fpBase + " fpHeight: " + fpHeight);

            ConvertLatLonDimensions(initCoord);
        }

        public void ConvertLatLonDimensions(BasicGeoposition initCoord)
        {
            Waypoint init = new Waypoint
            {
                location = new LocationCoordinate2D { latitude = 0, longitude = 0 }
            };
            Waypoint midTop = FindPointAtDistanceFrom(initCoord, 0, fpHeight/2);
            halfHeightLat = midTop.location.latitude - initCoord.Latitude;

            Waypoint midHorizRight = FindPointAtDistanceFrom(initCoord, 90, fpHeight / 2);
            halfHeightLon = midHorizRight.location.longitude - initCoord.Longitude;

            Waypoint midRight = FindPointAtDistanceFrom(initCoord, 90, fpBase/2);
            halfBaseLon = midRight.location.longitude - initCoord.Longitude;

            Waypoint midHorizTop = FindPointAtDistanceFrom(initCoord, 0, fpBase / 2);
            halfBaseLat = midHorizTop.location.latitude - initCoord.Latitude;
        } 
     
        private static void PrintArr(double[] arr)
        {
            System.Diagnostics.Debug.WriteLine("Printing array... ");
            foreach (double val in arr)
                System.Diagnostics.Debug.WriteLine(val + " ");
        }

        public List<BasicGeoposition> FindFootprintInnerSide(List<BasicGeoposition> coordList)
        {
            List<BasicGeoposition> OneSideList = new List<BasicGeoposition>();

            BasicGeoposition curr, innerSide;
             
            curr = coordList[0];
            innerSide = new BasicGeoposition { Latitude = curr.Latitude - halfHeightLat, Longitude = curr.Longitude + halfBaseLon};
            OneSideList.Add(innerSide);

            int latSign = -1, lonSign = 1;
            Boolean nextToChangeLat = false;
            foreach(BasicGeoposition coord in coordList.GetRange(1, coordList.Count - 1))
            {
                innerSide = new BasicGeoposition { Latitude = coord.Latitude + (latSign * halfBaseLat), Longitude = coord.Longitude + (lonSign * halfBaseLon)};
                OneSideList.Add(innerSide);

                if (nextToChangeLat)latSign *= -1;
                else lonSign *= -1;
                nextToChangeLat = !nextToChangeLat;
            }

            //In the inner side of the coverage area, the last point is special case
            curr = OneSideList.Last();
            curr.Longitude -= halfBaseLon * 2;
            OneSideList.Add(curr);

            return OneSideList;
        }

        public Stack<BasicGeoposition> FindFootprintOuterSide(List<BasicGeoposition> coordList)
        {
            Stack<BasicGeoposition> OtherSideStack = new Stack<BasicGeoposition>();

            BasicGeoposition curr, outerSide;
            int latSign = 1, lonSign = -1;
            Boolean nextToChangeLat = false;

            curr = coordList[0];
            outerSide = new BasicGeoposition { Latitude = curr.Latitude - halfHeightLat, Longitude = curr.Longitude - halfBaseLon };
            OtherSideStack.Push(outerSide);

            foreach (BasicGeoposition coord in coordList.GetRange(1, coordList.Count - 1))
            {
                outerSide = new BasicGeoposition { Latitude = coord.Latitude + (latSign * halfBaseLat), Longitude = coord.Longitude + (lonSign * halfBaseLon) };
                OtherSideStack.Push(outerSide);

                if (nextToChangeLat) latSign *= -1;
                else lonSign *= -1;
                nextToChangeLat = !nextToChangeLat;
            }

            // DrawFigure(new List<BasicGeoposition>(OtherSideStack), Color.FromArgb(255, 0, 0, 255));
            return OtherSideStack;
        }

        public List<BasicGeoposition> FindFootprintCoverage(List<BasicGeoposition> coordList)
        {
            List<BasicGeoposition> OneSideList = FindFootprintInnerSide(coordList); //new List<BasicGeoposition>();  
            Stack<BasicGeoposition> OtherSideStack = FindFootprintOuterSide(coordList);  //new Stack<BasicGeoposition>(); 

            // OneSideList.Concat(OtherSideStack);
            while (OtherSideStack.Count > 0)
                OneSideList.Add(OtherSideStack.Pop());

          OneSideList.Add(OneSideList.First()); //Add back the first point to form a complete polygon
            return OneSideList;
        }


        public List<List<BasicGeoposition>> FindSeperateFootprint(List<BasicGeoposition> coordList)
        {
            List<List<BasicGeoposition>> rectFootprints = new List<List<BasicGeoposition>>();
            Boolean isOrigOrient = true;
            double latVal, lonVal;

            foreach (BasicGeoposition coord in coordList)
            {
                if (isOrigOrient)
                {
                    latVal = halfHeightLat;
                    lonVal = halfBaseLon;
                } else
                {
                    latVal = halfBaseLat;
                    lonVal = halfHeightLon;
                }
                List<BasicGeoposition> fourPoints = new List<BasicGeoposition>();
                fourPoints.Add(new BasicGeoposition { Latitude = coord.Latitude -latVal, Longitude = coord.Longitude - lonVal});
                fourPoints.Add(new BasicGeoposition { Latitude = coord.Latitude - latVal, Longitude = coord.Longitude + lonVal });
                fourPoints.Add(new BasicGeoposition { Latitude = coord.Latitude + latVal, Longitude = coord.Longitude + lonVal });
                fourPoints.Add(new BasicGeoposition { Latitude = coord.Latitude + latVal, Longitude = coord.Longitude - lonVal});
                fourPoints.Add(fourPoints.First()); //Add back the first point to form a complete polygon

                rectFootprints.Add(fourPoints);
                isOrigOrient = !isOrigOrient;
            }
            return rectFootprints;
        }

        public double getHalfBaseLon()
        {
            return halfBaseLon; 
        }

        public double getHalfHeightLon()
        {
            return halfHeightLon; 
        }

        public double getHalfBaseLat()
        {
            return halfBaseLat; 
        }

        public double getHalfHeightLat()
        {
            return halfHeightLat; 
        }
    }
}
