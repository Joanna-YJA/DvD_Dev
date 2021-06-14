using DJI.WindowsSDK;
using DJI.WindowsSDK.Components;
using DJI.WindowsSDK.Mission.Waypoint;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls.Maps;
using UnityEngine.;

namespace DvD_Dev
{
    class MapController
    {
        static Color pathColor = Color.FromArgb(255, 225, 173, 1); //Pink
        static Color testFootprintColor = Color.FromArgb(50, 225, 225, 0); //Grey
        static Color coverageColor = Color.FromArgb(50, 112, 128, 141); //Green
        static Color coverageStrokeColor = Color.FromArgb(155, 112, 128, 141); //Green
        static Color oneSideColor = Color.FromArgb(155, 255, 0, 0); //Red
        static Color otherSideColor = Color.FromArgb(155, 0, 0, 255); //Blue

        uint ProductIndex = 0, ComponentIndex = 0;
        Mission mission;
        static LocationCoordinate2D startCoord = new LocationCoordinate2D { latitude = 1.290270, longitude = 103.851959 };

        public MapControl Map;
        List<MapElement> Landmarks;
        MapElementsLayer LandmarksLayer;

        FootprintCalculator fpCalc;
  
        public MapController(MapControl map)
        {
            fpCalc = new FootprintCalculator(100, 100, 100);

            Map = map;
            Map.StyleSheet = MapStyleSheet.RoadDark();
            NewMapLayer();

            mission = new Mission(ProductIndex, ComponentIndex, startCoord);
        }

        private void NewMapLayer()
        {
            Landmarks = new List<MapElement>();
            LandmarksLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = Landmarks
            };

            Map.Layers.Add(LandmarksLayer);
        }

 

        public void RenewMap()
        {
            RefreshMapLayer();
            SetMapLoc();
        }

        private void RefreshMapLayer()
        {
            Map.Layers.Remove(LandmarksLayer);
            NewMapLayer();
        }

        private async void SetMapLoc()
        {
            Map.ZoomLevel = 12;
            Map.LandmarksVisible = true;
            //If Location Service is enabled
            if (await Geolocator.RequestAccessAsync() == GeolocationAccessStatus.Allowed)
            {
                // Get the current location.  
                Geolocator geolocator = new Geolocator();
                Geoposition pos = await geolocator.GetGeopositionAsync();
                Geopoint myLocation = pos.Coordinate.Point;
                // Set the map location.  
                Map.Center = myLocation;
            }
            else
            {  //Else set starting location to centre of Singapore
               // Set the map location.
                BasicGeoposition startPos = new BasicGeoposition() { Latitude = startCoord.latitude, Longitude = startCoord.longitude };
                Geopoint startLocation = new Geopoint(startPos);
                Map.Center = startLocation;
            }
        }

        public void HandleMapTap(BasicGeoposition tappedPos)
        {
            DropPin(tappedPos, "tapped");

            List<BasicGeoposition> pathList = mission.ExecuteMission(fpCalc, tappedPos);

            //test
            //List<BasicGeoposition> oneSideList = fpCalc.FindFootprintInnerSide(coordList);
            //DrawLine(oneSideList, oneSideColor);
            //DropPin(oneSideList[0], "First Point");

            //Stack<BasicGeoposition> stack = fpCalc.FindFootprintOuterSide(coordList);
            //List<BasicGeoposition> otherSideList = new List<BasicGeoposition>(stack);
            //DrawLine(otherSideList, otherSideColor);

            List<BasicGeoposition> coverageList = fpCalc.FindFootprintCoverage(pathList);
            DrawCoveragePolygon(coverageList);

            DrawSeperateFootprint(pathList);
            DrawLine(pathList, pathColor);

            //testing
            PathFinder pathFinder = new PathFinder();
            pathFinder.InitPathFinder();
        }

        private void DropPin(BasicGeoposition pos, String title)
        {
            Geopoint tappedPoint = new Geopoint(pos);

            var spaceNeedleIcon = new MapIcon
            {
                Location = tappedPoint,
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                ZIndex = 0,
                Title = title
                
            };

            AddMapElement(spaceNeedleIcon);
        }

        private void AddMapElement(MapElement e)
        {
            Landmarks.Add(e);
            Map.Layers.Remove(LandmarksLayer);
            LandmarksLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = Landmarks
            };
            Map.Layers.Add(LandmarksLayer);
        }

        private void DrawLine(List<BasicGeoposition> posList, Color color)
        {
            var route = new MapPolyline
            {
                Path = new Geopath(posList),
                ZIndex = 0,
                StrokeColor = color,
            };

            AddMapElement(route);
        }

        private void DrawPolygon(List<BasicGeoposition> posList, Color color)
        {
                var polygon = new MapPolygon
                {
                    Path = new Geopath(posList),
                    ZIndex = 0,
                    FillColor = color
                };


                AddMapElement(polygon);
        }

        private void DrawCoveragePolygon(List<BasicGeoposition> posList)
        {
            //List<BasicGeoposition> subOneSide, subOtherSide;
            //for (int i = 0; i < oneSideList.Count; i += 4)
            //{
            //subOneSide = oneSideList.GetRange(i, 4);
            //subOtherSide = otherSideList.GetRange(i, 4);
            var polygon = new MapPolygon
            {
                Path = new Geopath(posList),//.GetRange(i, 4)),
                FillColor = coverageColor,
                StrokeColor = coverageStrokeColor,
                ZIndex = 0
            };


                AddMapElement(polygon);
            //}
        }

        private void DrawSeperateFootprint(List<BasicGeoposition> coordList)
        {
            List<List<BasicGeoposition>> rectFootprints = fpCalc.FindSeperateFootprint(coordList);
            foreach (List<BasicGeoposition> rect in rectFootprints)
                DrawPolygon(rect, testFootprintColor);
        }

        private void printLocation(Object sender, LocationCoordinate2D? location)
        {
            string status = "Location changed to " + location.ToString();
            //System.Diagnostics.Debug.WriteLine(status);
        }
    }
}
