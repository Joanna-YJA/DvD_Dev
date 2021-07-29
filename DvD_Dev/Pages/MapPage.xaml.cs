using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

            ClickDefaultButtons();
        }

        public void ClickDefaultButtons()
        {
            show_bounding_box.IsChecked = true;
            ShowBoundingBox(new object(), new RoutedEventArgs());

            show_drone_path.IsChecked = true;
            ShowPath(new object(), new RoutedEventArgs());

            show_camera_footprint.IsChecked = true;
            ShowFootprint(new object(), new RoutedEventArgs());
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

        public async void UploadShapefile(object sender, RoutedEventArgs r)
        {
            await pathFinder.ReadShapefile();
        }

        public async void DeserializeOctree(object sender, RoutedEventArgs r)
        {
            await pathFinder.DeserializeWorld();
        }

        public async void SerializeOctree(object sender, RoutedEventArgs r)
        {
            await pathFinder.SerializeWorld();
        }

        public async void ShowMesh(object sender, RoutedEventArgs r)
        {
            pathFinder.ShowMesh();
        }

        public async void ShowMeshNormals(object sender, RoutedEventArgs r)
        {
            pathFinder.ShowMeshNormals();
        }

        public async void ShowBoundingBox(object sender, RoutedEventArgs r)
        {
            pathFinder.ShowBoundingBox();
        }

        public async void ShowPath(object sender, RoutedEventArgs r)
        {
            pathFinder.ShowPath();
        }

        public async void ShowFootprint(object sender, RoutedEventArgs r)
        {
            pathFinder.ShowFootprint();
        }

        public async void ShowOctreeNodes(object sender, RoutedEventArgs r)
        {
            pathFinder.ShowOctreeNodes();
        }

        public async void ShowGraph(object sender, RoutedEventArgs r)
        {
            pathFinder.ShowGraph();
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

            int fieldDimM = int.Parse(fieldDimInput.Text);
            await pathFinder.GenerateWorld(projectedPoint, fieldDimM);
            pathFinder.ShowBoundingBox();
        }
    }
}
