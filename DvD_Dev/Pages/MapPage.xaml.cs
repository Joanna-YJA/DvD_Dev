using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Colors = System.Drawing.Color;

namespace DvD_Dev
{
    public sealed partial class MapPage : Page
    {
        SimpleMarkerSymbol sourceSymbol = new SimpleMarkerSymbol()
        {
            Color = Colors.Red,
            Size = 10,
            Style = SimpleMarkerSymbolStyle.Circle
        };

        SimpleMarkerSymbol destinationSymbol = new SimpleMarkerSymbol()
        {
            Color = Colors.Green,
            Size = 10,
            Style = SimpleMarkerSymbolStyle.Circle
        };

        GraphicsOverlay overlay;
        PathFinder pathFinder;
        bool isEvenClick = true;
        MapPoint[] sourceDest = new MapPoint[2];

        public MapPage()
        {
            this.InitializeComponent();
            InitializeSceneView();

            pathFinder = new PathFinder(ref MySceneView);
        }

        private void InitializeSceneView()
        {
            Scene myScene = new Scene(Basemap.CreateTopographic());
            MapPoint mapCenterPoint = new MapPoint(103.8198, 1.3521, SpatialReferences.Wgs84);
            Camera camera = new Camera(locationPoint: mapCenterPoint,
                                       heading: 322,
                                       pitch: 73,
                                       roll: 0);
            MySceneView.SetViewpointCameraAsync(camera);
            MySceneView.Scene = myScene;
            MySceneView.GeoViewTapped += SceneView_Tapped;
            MySceneView.GeoViewDoubleTapped += SceneView_DoubleTapped;

            overlay = new GraphicsOverlay();
            overlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            MySceneView.GraphicsOverlays.Add(overlay);
        }

        //When map is loaded(navigated to), set the starting location of the map
        override
        protected async void OnNavigatedTo(NavigationEventArgs e)
        {
            pathFinder.ReadInput();

            

        }

        private void SceneView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            if (pathFinder.shipWorld == null) return;
            MapPoint tappedPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(e.Location);
            tappedPoint = new MapPoint(tappedPoint.X, tappedPoint.Y, 20, tappedPoint.SpatialReference);
            MapPoint projectedPoint = (MapPoint)GeometryEngine.Project(tappedPoint, PathFinder.spatialRef);
            Graphic graphicWithSymbol;

            if (isEvenClick)
            {
                graphicWithSymbol = new Graphic(tappedPoint, sourceSymbol);
                sourceDest[0] = projectedPoint;
            }
            else
            {
                graphicWithSymbol = new Graphic(tappedPoint, destinationSymbol);
                sourceDest[1] = projectedPoint;
                pathFinder.TravelAndSearch(sourceDest);
            }

            isEvenClick = !isEvenClick;
            overlay.Graphics.Add(graphicWithSymbol);
        }

        private async void SceneView_DoubleTapped(object sender, GeoViewInputEventArgs e)
        {
            MapPoint tappedPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(e.Location);
            tappedPoint = new MapPoint(tappedPoint.X, tappedPoint.Y, 0, tappedPoint.SpatialReference);
            MapPoint projectedPoint = (MapPoint)GeometryEngine.Project(tappedPoint, PathFinder.spatialRef);

            MySceneView.Camera.MoveTo(tappedPoint);


            System.Diagnostics.Debug.WriteLine("Projected point: <" + projectedPoint.X + ", " + projectedPoint.Y + ", " + projectedPoint.Z + "> with spatial ref " + PathFinder.spatialRef.WkText);
            await pathFinder.InitPathFinder(projectedPoint);

            //Show different types of intermediate data structures (for testing/debugging)
            //Uncomment as neccessary
            //pathFinder.DisplayMesh();
            //pathFinder.DisplayMeshNormals();
            //pathFinder.DisplayOctreeNodes();
            //pathFinder.DisplayOctreeBlockedNodes();
           pathFinder.DisplayGraphNodes();
           // pathFinder.DisplayPath();
            //pathFinder.DisplayFootprintCoverage();
            pathFinder.DisplayOctreeBoundingBox();
            pathFinder.DisplayDimensionBoundingBox(200);
        }
    }
}
