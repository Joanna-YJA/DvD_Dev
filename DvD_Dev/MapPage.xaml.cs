using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DJI.WindowsSDK;
using Windows.UI.Xaml.Media.Imaging;
using DJIVideoParser;
using System.Threading.Tasks;
using MUXC = Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using System.Threading;
using Windows.Devices.Geolocation;
using DJI.WindowsSDK.Components;
using DJI.WindowsSDK.Mission.Waypoint;
using Windows.Services.Maps;
using static DvD_Dev.SpatialMath;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DvD_Dev
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        List<MapElement> Landmarks;
        MapElementsLayer LandmarksLayer;
        double heading;

        
        uint ProductIndex = 0, ComponentIndex = 0;
        static double startLat = 1.290270, startLon = 103.851959;
        int numSpiral = 10;

        FlightControllerHandler fcHandler;
        WaypointMissionHandler wpHandler;
        public MapPage()
        {
            this.InitializeComponent();
            initializeMap();
        }

        private async void initializeMap()
        {
            Landmarks = new List<MapElement>();
            LandmarksLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = Landmarks
            };

            Map.Layers.Add(LandmarksLayer);

            //Handler that executes everytime drone location changes
            fcHandler = DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(ProductIndex, ComponentIndex);
            fcHandler.AircraftLocationChanged += printLocation;

            wpHandler = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(ProductIndex);

            //TODO: set home location flexibly
            LocationCoordinate2D startCoord;
            startCoord.latitude = (startLat / 1E6);
            startCoord.longitude = (startLon / 1E6);
            await fcHandler.SetHomeLocationAsync(startCoord);


        }

        private void printLocation(Object sender, LocationCoordinate2D? location)
        {
            string status = "Location changed to " + location.ToString();
            System.Diagnostics.Debug.WriteLine(status);
        }
        //When map is loaded(navigated to), set the starting location of the map
        override
        protected async void OnNavigatedTo(NavigationEventArgs e)
        {
            //If Location Service is enabled
            if (await Geolocator.RequestAccessAsync() == GeolocationAccessStatus.Allowed)
            {
                // Get the current location.  
                Geolocator geolocator = new Geolocator();
                Geoposition pos = await geolocator.GetGeopositionAsync();
                Geopoint myLocation = pos.Coordinate.Point;
                // Set the map location.  
                Map.Center = myLocation;
                Map.ZoomLevel = 12;
                Map.LandmarksVisible = true;
            }
            else
            {  //Else set starting location to centre of Singapore
               // Set the map location.
                BasicGeoposition pos = new BasicGeoposition() { Latitude = startLat, Longitude = startLon };
                Geopoint startLocation = new Geopoint(pos);
                Map.Center = startLocation;
                Map.ZoomLevel = 12;
                Map.LandmarksVisible = true;
            }
        }


        /// <summary>
        /// <see cref="windows.UI.Xaml.Controls.Maps.MapControl.HeadingChanged"/>
        /// </summary>
        private void Heading_HeadingChanged(MapControl sender, Object obj)
        {
            heading = Map.Heading;
            System.Diagnostics.Debug.WriteLine("Heading changed to " + heading);
        }
        private void Map_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            BasicGeoposition tappedPos = args.Location.Position;
            DropPin(tappedPos);
            CreateMission(tappedPos);
        }

        private void DropPin(BasicGeoposition pos)
        {
            Geopoint tappedPoint = new Geopoint(pos);

            var spaceNeedleIcon = new MapIcon
            {
                Location = tappedPoint,
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                ZIndex = 0,
                Title = "Space Needle"


            };

            Landmarks.Add(spaceNeedleIcon);
            Map.Layers.Remove(LandmarksLayer);
            LandmarksLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = Landmarks
            };
            Map.Layers.Add(LandmarksLayer);

            //test
            string status = "MapTapped at \nLatitude:" + pos.Latitude + "\nLongitude: " + pos.Longitude;
            System.Diagnostics.Debug.WriteLine(status);
            long count = LandmarksLayer.MapElements.Count();
            System.Diagnostics.Debug.WriteLine("Landmarks layer has " + count + " map element");
            System.Diagnostics.Debug.WriteLine("Landmarks layer has " + count + " map element");
            MapElementsLayer l = (MapElementsLayer)Map.Layers.ElementAt(0);
            System.Diagnostics.Debug.WriteLine("Inside the map, there is really " + l.MapElements.Count());
        }

        private void CreateMission(BasicGeoposition startPos)
        {
            var state = wpHandler.GetCurrentState();
            //testing (remove condition)
            //  if (state.Equals(WaypointMissionState.READY_TO_UPLOAD))
            //   {
            WaypointMission wpMission = SimpleTraversal(startPos);

            //Load Mission into aircraft
            wpHandler.LoadMission(wpMission);

            state = wpHandler.GetCurrentState();
            //test remove condition
            if (state.Equals(WaypointMissionState.READY_TO_EXECUTE))
                System.Diagnostics.Debug.WriteLine("Mission ready to execute");
            else
                System.Diagnostics.Debug.WriteLine("MISSION UPLOAD FAILURE");

            //      }




        }

        private WaypointMission SimpleTraversal(BasicGeoposition startPos)
        {
            var wpList = new List<Waypoint>();

            LocationCoordinate2D startCoord = new LocationCoordinate2D { latitude = startPos.Latitude, longitude = startPos.Longitude };
            Waypoint startWP = new Waypoint { location = startCoord };
            wpList.Add(startWP);

            GenerateSpiralPoints(wpList, startWP);

            ToGeodesic(wpList, 32);

            //Draw on Map
            List<BasicGeoposition> posList = new List<BasicGeoposition>();
            foreach (Waypoint wp in wpList)
            {
                var loc = wp.location;
                posList.Add(new BasicGeoposition
                {
                    Longitude = loc.longitude,
                    Latitude = loc.latitude
                });
            }

            var route = new MapPolyline
            {
                Path = new Geopath(posList),
                ZIndex = 0
            };

            Landmarks.Add(route);
            Map.Layers.Remove(LandmarksLayer);
            LandmarksLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = Landmarks
            };
            Map.Layers.Add(LandmarksLayer);

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

        private void GenerateSpiralPoints(List<Waypoint> wpList, Waypoint startWP)
        {
            Waypoint prevWP = startWP;
            double len = 0.1, incr = 0.1;

            //Initial upWP to travsere in forward direction
            Waypoint upWP = FindPointAtDistanceFrom(prevWP, Map.Heading, len);
            wpList.Add(upWP);
            prevWP = upWP;

            for(int i = 0; i < numSpiral; i++)
            {
                //test
                System.Diagnostics.Debug.WriteLine("Heading: " + Map.Heading);

                Waypoint rightWP = FindPointAtDistanceFrom(prevWP,  Map.Heading + 90, len);
                wpList.Add(rightWP);
                prevWP = rightWP;

                len += incr;
                Waypoint bottRightWP = FindPointAtDistanceFrom(rightWP, Map.Heading + 180, len);
                wpList.Add(bottRightWP);
                prevWP = bottRightWP;

                Waypoint bottLeftWP = FindPointAtDistanceFrom(bottRightWP, Map.Heading + 270, len);
                wpList.Add(bottLeftWP);
                prevWP = bottLeftWP;

                len += incr;
                Waypoint leftWP = FindPointAtDistanceFrom(bottLeftWP, Map.Heading, len);
                wpList.Add(leftWP);
                prevWP = leftWP;
            }
        }


    }
}
