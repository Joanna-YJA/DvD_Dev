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

            overlay = new GraphicsOverlay();
            overlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            MySceneView.GraphicsOverlays.Add(overlay);
        }

        //When map is loaded(navigated to), set the starting location of the map
        override
        protected async void OnNavigatedTo(NavigationEventArgs e)
        {
            await pathFinder.InitPathFinder();

            //Show different types of intermediate data structures (for testing/debugging)
            //Uncomment as neccessary
            //pathFinder.DisplayMesh();
            //pathFinder.DisplayMeshNormals();
            //pathFinder.DisplayOctreeBlockedNodes();
            //pathFinder.DisplayGraphNodes();
            pathFinder.DisplayPath();
            //pathFinder.DisplayFootprintCoverage();
        }

        private void SceneView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            MapPoint tappedPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(e.Location);
            tappedPoint = new MapPoint(tappedPoint.X, tappedPoint.Y, 25, tappedPoint.SpatialReference);
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
                pathFinder.SpiralSearch(sourceDest);
            }

            isEvenClick = !isEvenClick;
            //overlay.Graphics.Add(graphicWithSymbol);
        }
    }
}
