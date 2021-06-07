using DJI.WindowsSDK;
using DJI.WindowsSDK.Components;
using DJI.WindowsSDK.Mission.Waypoint;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;

namespace DvD_Dev
{
    class Mission
    {
        FlightControllerHandler fcHandler;
        WaypointMissionHandler wpHandler;

        static int numSpiral = 10;

        public Mission(uint ProductIndex, uint ComponentIndex, LocationCoordinate2D startCoord)
        {
            //Handler that executes everytime drone location changes
            fcHandler = DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(ProductIndex, ComponentIndex);

            wpHandler = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(ProductIndex);

            fcHandler.SetHomeLocationAsync(startCoord);
        }


        public List<BasicGeoposition> ExecuteMission(FootprintCalculator fpCalc, BasicGeoposition startPos)
        {
            var state = wpHandler.GetCurrentState();
            //testing (remove condition)
            //  if (state.Equals(WaypointMissionState.READY_TO_UPLOAD))
            //   {
            List<BasicGeoposition> pathList = CreateSimpleTraversalPath(fpCalc, startPos);
            WaypointMission wpMission = ConvertToWaypointMission(pathList);

            //Load Mission into aircraft
            wpHandler.LoadMission(wpMission);

            state = wpHandler.GetCurrentState();
            //test remove condition
            if (state.Equals(WaypointMissionState.READY_TO_EXECUTE))
                System.Diagnostics.Debug.WriteLine("Mission ready to execute");
            else
                System.Diagnostics.Debug.WriteLine("MISSION UPLOAD FAILURE");

            //      }

            return pathList;
        }

        private List<BasicGeoposition> CreateSimpleTraversalPath(FootprintCalculator fpCalc, BasicGeoposition startCoord)
        {
            List<BasicGeoposition> coordList = new List<BasicGeoposition>();
            coordList.Add(startCoord);

            fpCalc.FindFootprintDimensions(startCoord); //Initialize Base and Height attribute of footprint calculator object
            GenerateSpiralPoints(fpCalc, coordList);

            return coordList;
        }

        private WaypointMission ConvertToWaypointMission(List<BasicGeoposition> coordList)
        {
            List<Waypoint> wpList = ConvertToWaypoints(coordList);

            //Create Mission for drone to execute
            WaypointMission wpMission = new WaypointMission
            {
                waypointCount = 1,
                autoFlightSpeed = 2.5,
                finishedAction = WaypointMissionFinishedAction.NO_ACTION,
                headingMode = WaypointMissionHeadingMode.USING_WAYPOINT_HEADING,
                flightPathMode = WaypointMissionFlightPathMode.CURVED,
                repeatTimes = 0,
                waypoints = wpList,
                missionID = 1
            };

            return wpMission;
        }

        private void GenerateSpiralPoints(FootprintCalculator fpCalc, List<BasicGeoposition> coordList)
        {
            BasicGeoposition prevCoord = coordList.First();
            double halfHeightLat = fpCalc.getHalfHeightLat(), halfBaseLat = fpCalc.getHalfBaseLat();
            double halfHeightLon = fpCalc.getHalfHeightLon(), halfBaseLon = fpCalc.getHalfBaseLon();
            double baseHeightDiff = halfBaseLat - halfHeightLat;
            double incrLat = halfHeightLat + halfBaseLat, incrLon = halfBaseLon + halfBaseLon;

            BasicGeoposition forwardCoord, sideCoord;
            int sign = 1;

            for (int i = 0; i < numSpiral * 2; i++)
            {
                forwardCoord = new BasicGeoposition { Latitude = prevCoord.Latitude + (sign * incrLat), Longitude = prevCoord.Longitude };
                coordList.Add(forwardCoord);
                prevCoord = forwardCoord;
                incrLat += halfBaseLat;
                if (i > 0) incrLat += halfBaseLat;
                else incrLat += halfHeightLat;

                sideCoord = new BasicGeoposition { Latitude = prevCoord.Latitude, Longitude = prevCoord.Longitude + (sign * incrLon) };
                coordList.Add(sideCoord);
                prevCoord = sideCoord;
                incrLon += halfBaseLon + halfBaseLon;

                sign *= -1;
            }
        }

        private List<Waypoint> ConvertToWaypoints(List<BasicGeoposition> coordList)
        {
            List<Waypoint> res = new List<Waypoint>();
            foreach (BasicGeoposition coord in coordList)
                res.Add(new Waypoint
                {
                    location = new LocationCoordinate2D { latitude = coord.Latitude, longitude = coord.Longitude }
                });
            return res;
        }
    }
}
