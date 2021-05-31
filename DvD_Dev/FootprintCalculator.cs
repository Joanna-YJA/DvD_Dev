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
        public static double xsensorMm = 36;
        public static double ysensorMm = 24;

        public static double focalLenMm = 50;
        public static double altitudeM = 100;

        public static double xgimbalRad = DegreesToRadians(0);
        public static double ygimbalRad = DegreesToRadians(0);

        public static double HFVRad = FindHFV();
        public static double VFVRad = FindVFV();

        public static double FindHFV()
        {
            double angle = xsensorMm / (2 * focalLenMm);
            return 2 * Math.Atan(angle);
        }

        public static double FindVFV()
        {
            double angle = ysensorMm / (2 * focalLenMm);
            return 2 * Math.Atan(angle);
        }

        public static double FindDroneToPictureEdge(String type)
        {

            switch (type)
            {
                case "right":
                    double angleR = xgimbalRad + 0.5 * HFVRad;
                    return altitudeM * Math.Tan(angleR);
                case "left":
                    double angleL = xgimbalRad - 0.5 * HFVRad;
                    return altitudeM * Math.Tan(angleL);

                case "up":
                    double angleU = ygimbalRad + 0.5 * VFVRad;
                    return altitudeM * Math.Tan(angleU);
                case "down":
                    double angleD = ygimbalRad - 0.5 * VFVRad;
                    return altitudeM * Math.Tan(angleD);

                default:
                    return -1;
            }
            
        }

        public static double[] FindPictureEdges()
        {
            double[] lenEdgeFromDrone = new double[4];

            lenEdgeFromDrone[0] = FindDroneToPictureEdge("up");
            lenEdgeFromDrone[1] = FindDroneToPictureEdge("right");
            lenEdgeFromDrone[2] = FindDroneToPictureEdge("down");
            lenEdgeFromDrone[3] = FindDroneToPictureEdge("left");

            return lenEdgeFromDrone;
        }

        public static List<BasicGeoposition> FindFootprintCorners(BasicGeoposition camPos)
        {
            double[] cornerLatLon = FindPictureEdges();
            //test
            PrintArr(cornerLatLon);

            ConvertToLatLon(cornerLatLon, camPos);

            //test
            PrintArr(cornerLatLon);

            BasicGeoposition topLeft = new BasicGeoposition
            {
                Latitude = cornerLatLon[0],
                Longitude = cornerLatLon[3]
            };

            BasicGeoposition topRight = new BasicGeoposition
            {
                Latitude = cornerLatLon[0],
                Longitude = cornerLatLon[1]
            };

            BasicGeoposition bottRight = new BasicGeoposition
            {
                Latitude = cornerLatLon[2],
                Longitude = cornerLatLon[1]
            };

            BasicGeoposition bottLeft = new BasicGeoposition
            {
                Latitude = cornerLatLon[2],
                Longitude = cornerLatLon[3]
            };

            return new List<BasicGeoposition> { topLeft, topRight, bottRight, bottLeft, topLeft };
        }

        private static void ConvertToLatLon(double[] cornerLatLon, BasicGeoposition camPos)
        {
            Waypoint camWp = new Waypoint
            {
                location = new LocationCoordinate2D
                {
                    latitude = camPos.Latitude,
                    longitude = camPos.Longitude
                }
            };
            Waypoint midTop = FindPointAtDistanceFrom(camWp, RadiansToDegrees(ygimbalRad), cornerLatLon[0]/1000);
            cornerLatLon[0] = midTop.location.latitude;

            Waypoint midRight = FindPointAtDistanceFrom(camWp, RadiansToDegrees(xgimbalRad), cornerLatLon[1]/1000);
            cornerLatLon[1] = midRight.location.longitude;

            Waypoint midBott = FindPointAtDistanceFrom(camWp, 180+ RadiansToDegrees(ygimbalRad), cornerLatLon[0]/1000);
            cornerLatLon[2] = midBott.location.latitude;

            Waypoint midLeft = FindPointAtDistanceFrom(camWp, 180 + RadiansToDegrees(xgimbalRad), cornerLatLon[1]/1000);
            cornerLatLon[3] = midLeft.location.longitude;

        }

        private static void PrintArr(double[] arr)
        {
            System.Diagnostics.Debug.WriteLine("Printing array... ");
            foreach (double val in arr)
                System.Diagnostics.Debug.WriteLine(val + " ");
        }

    }
}
