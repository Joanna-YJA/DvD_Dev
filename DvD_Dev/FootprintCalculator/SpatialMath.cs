using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DJI.WindowsSDK;
using Windows.Devices.Geolocation;

namespace DvD_Dev
{
    /// <summary>
    /// Contains methods for calculating points on map,
    /// and routes between points.
    /// </summary>
    class SpatialMath
    {
        int defaultSegments = 32; //The route between 2 points are calculated by dividing it into smaller line segments

        /// <summary>
        /// Given a start point, initial bearing (direction), and distance, 
        /// this function computes the destination point travelling along a the shortest path on an ellipsoid 
        /// (geomatric shapre of Earth). 
        /// </summary>
        /// <returns>
        /// the destination point in latitude and longtitude.
        /// </returns>
        public static Waypoint FindPointAtDistanceFrom(BasicGeoposition startPoint, double initialBearingDegs, double distanceKilometres)
        {
            const double radiusEarthKilometres = 6371.01;

            initialBearingDegs = distanceKilometres < 0 ? (initialBearingDegs + 180) % 360 : initialBearingDegs;
            var distRatio = distanceKilometres / radiusEarthKilometres;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);

            var startLatRad = DegreesToRadians(startPoint.Latitude);
            var startLonRad = DegreesToRadians(startPoint.Longitude);

            var startLatCos = Math.Cos(startLatRad);
            var startLatSin = Math.Sin(startLatRad);

            var initialBearingRadians = DegreesToRadians(initialBearingDegs);
            var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(initialBearingRadians)));

            var endLonRads = startLonRad
                + Math.Atan2(
                    Math.Sin(initialBearingRadians) * distRatioSine * startLatCos,
                    distRatioCosine - startLatSin * Math.Sin(endLatRads));

            return new Waypoint
            {
                location = new LocationCoordinate2D
                {
                    latitude = RadiansToDegrees(endLatRads),
                    longitude = RadiansToDegrees(endLonRads)
                }
            };
        }

        public static double DegreesToRadians(double degrees)
        {
            const double degToRadFactor = Math.PI / 180;
            return degrees * degToRadFactor;
        }

        public static double RadiansToDegrees(double radians)
        {
            const double radToDegFactor = 180 / Math.PI;
            return radians * radToDegFactor;
        }

        /// <summary>
        /// Get the geodesic approximation of a line or poloygon.
        /// Geodesic curves allows user to visualise the shortest distance between 2 points
        /// on a map.
        /// </summary>
        /// <returns>
        /// List of waypoints that represents geodesic curve.
        /// </returns>
        public static void ToGeodesicFigure(List<Waypoint> points, int numSegments)
        {
            var locs = new List<Waypoint>();
            for (var i = 0; i < points.Count - 1; i++)
            {
                // Convert coordinates from degrees to Radians
                var loc1 = points[i].location;
                var lat1 = DegreesToRadians(loc1.latitude);
                var lon1 = DegreesToRadians(loc1.longitude);

                var loc2 = points[i + 1].location;
                var lat2 = DegreesToRadians(loc2.latitude);
                var lon2 = DegreesToRadians(loc2.longitude);
                // Calculate the distance of the route between these 2 points
                var d = 2 * Math.Asin(
                                Math.Sqrt(
                                    Math.Pow((Math.Sin((lat1 - lat2) / 2)), 2)
                                        + Math.Cos(lat1) * Math.Cos(lat2)
                                            * Math.Pow((Math.Sin((lon1 - lon2) / 2)), 2)));
                // Calculate  positions of intermeidate points
                // at fixed intervals along the route
                for (var k = 0; k <= numSegments; k++)
                {
                    var f = (k / numSegments);
                    var A = Math.Sin((1 - f) * d) / Math.Sin(d);
                    var B = Math.Sin(f * d) / Math.Sin(d);

                    // Obtain 3D Cartesian coordinates of each point
                    var x = A * Math.Cos(lat1) * Math.Cos(lon1) + B * Math.Cos(lat2) * Math.Cos(lon2);
                    var y = A * Math.Cos(lat1) * Math.Sin(lon1) + B * Math.Cos(lat2) * Math.Sin(lon2);
                    var z = A * Math.Sin(lat1) + B * Math.Sin(lat2);

                    // Convert these to latitude/longitude
                    var lat = Math.Atan2(z, Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)));
                    var lon = Math.Atan2(y, x);

                    // Create a Location (remember to convert back to degrees)
                    var p = new Waypoint
                    {
                        location = new LocationCoordinate2D
                        {
                            latitude = RadiansToDegrees(lat),
                            longitude = RadiansToDegrees(lon)
                        }
                    };


                    // Add this to the array
                    locs.Add(p);
                }
            }
            points = locs;

        }
    }
}
