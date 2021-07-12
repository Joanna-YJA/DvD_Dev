using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Devices.Geolocation;
using System.Net.Http;


using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Symbology;
using Colors = System.Drawing.Color;
using System.Collections.Generic;
using System.Numerics;
using MIConvexHull;
using NetTopologySuite.Mathematics;
using System.Linq;
using Esri.ArcGISRuntime.Mapping.Popups;
using Windows.UI.Xaml.Input;
using Esri.ArcGISRuntime.Mapping.Labeling;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DvD_Dev
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        PathFinder pathFinder;

        bool isEvenClick = true;
        MapPoint[] sourceDest = new MapPoint[2];

        //test
        static GraphicsOverlay overlay;
        static GraphicsOverlay absOverlay;
        static SpatialReference spatialRef;
        MapView MyMapView;
        public MapPage()
        {
            this.InitializeComponent();
            MySceneView.GeoViewTapped += SceneView_Tapped;
            //MyMapView.GeoViewDoubleTapped += MapView_DoubleTapped;
            MapPoint mapCenterPoint = new MapPoint(103.8198, 1.3521, SpatialReferences.Wgs84);
           // MyMapView.SetViewpoint(new Viewpoint(mapCenterPoint, 100000));

            Scene myScene = new Scene(Basemap.CreateTopographic());
            Camera camera = new Camera(locationPoint: mapCenterPoint,
                        heading: 322,
                        pitch: 73,
                        roll: 0);

            // Assign the Scene to the SceneView.
            MySceneView.Scene = myScene;

            // Set view point of scene view using camera.
            MySceneView.SetViewpointCameraAsync(camera);

            absOverlay = new GraphicsOverlay();
            absOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            MySceneView.GraphicsOverlays.Add(absOverlay);

            pathFinder = new PathFinder(ref MyMapView, ref MySceneView);

            overlay = new GraphicsOverlay();
           // MyMapView.GraphicsOverlays.Add(overlay);

        }

        //When map is loaded(navigated to), set the starting location of the map
        override
        protected async void OnNavigatedTo(NavigationEventArgs e)
        {
            await pathFinder.InitPathFinder();

            spatialRef = PathFinder.spatialRef;


            //List<List<MapPoint>> triVertices = pathFinder.triVertices;
            //List<List<MapPoint>> vertices = pathFinder.vertices;
            //SimpleLineSymbol triLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.White, 0.3);
            ////Display triangulated vertices
            //int i = 0;
            //while (i < triVertices.Count && triVertices[i] != null)
            //{
            //    List<MapPoint> triVert = triVertices[i];
            //    // foreach (MapPoint p in featureVert) System.Diagnostics.Debug.WriteLine("Feature vertice: " + p.ToString());
            //    Polygon tri = new Polygon(triVert);
            //    Graphic triGraphic = new Graphic(tri, triLineSymbol);
            //    absOverlay.Graphics.Add(triGraphic);
            //    i++;
            //}


            //SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Orange, 0.5);
            //SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Orange, lineSymbol);
            //List<DelaunayTriangulation<Vertex, Cell>> dmeshes = pathFinder.dmeshes;
            //foreach(DelaunayTriangulation<Vertex, Cell> mesh in dmeshes)
            //{
            //    foreach (Cell tri in mesh.Cells)
            //    {
            //        List<MapPoint> triPoints = new List<MapPoint>();
            //        foreach (Vertex v in tri.Vertices) {
            //            Vector3D vCenter = v.Center;
            //            triPoints.Add(new MapPoint(vCenter.X, vCenter.Y, vCenter.Z, pathFinder.spatialRef));
            //            Polygon triPoly = new Polygon(triPoints);
            //           Graphic triGraphic = new Graphic(triPoly, triLineSymbol);
            //            overlay.Graphics.Add(triGraphic);
            //        }

            //    }
            //}

            //List<MapPoint> featureVert = vertices[0];
            //Polygon feature = new Polygon(featureVert);
            //Graphic featureGraphic = new Graphic(feature, lineSymbol);
            //overlay.Graphics.Add(featureGraphic);

            //Print all nodes of an octree
            // System.Diagnostics.Debug.WriteLine("Octree has " + pathFinder.shipWorld.space.maxLevel + " levels");
            SimpleMarkerSymbol simpleYellowSymbol = new SimpleMarkerSymbol()
            {
                Color = Colors.Yellow,
                Size = 10,
                Style = SimpleMarkerSymbolStyle.Triangle
            };

             List<OctreeNode> nodes = pathFinder.shipWorld.space.GetAllNodes();
            //Vector3 center = nodes[0].center;
            //MapPoint point = new MapPoint(center.X, center.Y, center.Z, pathFinder.spatialRef);
            //// System.Diagnostics.Debug.WriteLine("Octree node: " + point.ToString());
            //Graphic graphicWithSymbol = new Graphic(point, simpleYellowSymbol);
            //Geometry combinedNodes = graphicWithSymbol.Geometry;

            //foreach (OctreeNode node in nodes)
            //{
            //    center = node.center;
            //    point = new MapPoint(center.X, center.Y, center.Z, pathFinder.spatialRef);
            //    //System.Diagnostics.Debug.WriteLine("Octree node: " + point.ToString());
            //    graphicWithSymbol = new Graphic(point, simpleYellowSymbol);
            //    GeometryEngine.Union(combinedNodes, graphicWithSymbol.Geometry);

            //}
            //overlay.Graphics.Add(new Graphic(combinedNodes));
        }

        public static void ShowTriangle(Vector3 center)
        {
            SimpleMarkerSymbol simpleYellowSymbol = new SimpleMarkerSymbol()
            {
                Color = Colors.Yellow,
                Size = 10,
                Style = SimpleMarkerSymbolStyle.Triangle
            };
            MapPoint point = new MapPoint(center.X * 10, center.Y * 10, center.Z * 10, spatialRef);
            Graphic graphicWithSymbol = new Graphic(point, simpleYellowSymbol);
            overlay.Graphics.Add(graphicWithSymbol);
        }

        public static void ShowBlockedTriangle(Vector3 center)
        {
            SimpleMarkerSymbol simpleRedSymbol = new SimpleMarkerSymbol()
            {
                Color = Colors.Red,
                Size = 10,
                Style = SimpleMarkerSymbolStyle.Triangle
            };
            MapPoint point = new MapPoint(center.X * 10, center.Y * 10, center.Z * 10, spatialRef);
            Graphic graphicWithSymbol = new Graphic(point, simpleRedSymbol);
            absOverlay.Graphics.Add(graphicWithSymbol);
        }

        public static void ShowPath(List<MapPoint> linePoints)
        {
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Black, 5.0);
            SimpleMarkerSymbol simpleOrangeSymbol = new SimpleMarkerSymbol()
            {
                Color = Colors.Orange,
                Size = 10,
                Style = SimpleMarkerSymbolStyle.Circle
            };

            Polyline line = new Polyline(linePoints);
            Graphic graphicWithSymbol = new Graphic(line, lineSymbol);
            absOverlay.Graphics.Add(graphicWithSymbol);



            foreach (MapPoint point in linePoints)
            {
                Graphic pointSym = new Graphic(point, simpleOrangeSymbol);
                absOverlay.Graphics.Add(pointSym);
                System.Diagnostics.Debug.WriteLine("Path point: <" + point.X + ", " + point.Y + ", " + point.Z + ">" );
            }
        }

        public static void ShowPath(List<Vector3> lineVect)
        {
            List<MapPoint> linePoints = new List<MapPoint>();
            foreach (Vector3 v in lineVect)
                linePoints.Add(new MapPoint(v.X * 10, v.Y * 10, v.Z * 10, PathFinder.spatialRef));
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Yellow, 1.0);
            Polygon line = new Polygon(linePoints);
            Graphic graphicWithSymbol = new Graphic(line, lineSymbol);
            overlay.Graphics.Add(graphicWithSymbol);
        }

        private void SceneView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            SimpleMarkerSymbol simpleRedSymbol = new SimpleMarkerSymbol()
            {
                Color = Colors.Red,
                Size = 10,
                Style = SimpleMarkerSymbolStyle.Circle
            };

            SimpleMarkerSymbol simpleGreenSymbol = new SimpleMarkerSymbol()
            {
                Color = Colors.Green,
                Size = 10,
                Style = SimpleMarkerSymbolStyle.Circle
            };

            MapPoint tappedPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(e.Location);
            tappedPoint = new MapPoint(tappedPoint.X, tappedPoint.Y, 25, tappedPoint.SpatialReference);
           // System.Diagnostics.Debug.WriteLine("tappedPoint before projection: " + tappedPoint.ToString());
            MapPoint projectedPoint = (MapPoint)GeometryEngine.Project(tappedPoint, PathFinder.spatialRef);
            //System.Diagnostics.Debug.WriteLine("tappedPoint after projection: " + projectedPoint.ToString());
            Graphic graphicWithSymbol;
            if (isEvenClick)
            {
                graphicWithSymbol = new Graphic(tappedPoint, simpleRedSymbol);
                sourceDest[0] = projectedPoint;
            }
            else
            {
                graphicWithSymbol = new Graphic(tappedPoint, simpleGreenSymbol);
                sourceDest[1] = projectedPoint;
                pathFinder.MoveToCoords(sourceDest);
            }

            isEvenClick = !isEvenClick;
            absOverlay.Graphics.Add(graphicWithSymbol);


            //test
           // System.Diagnostics.Debug.WriteLine("projected tapped point: " + projectedPoint.X + " " + projectedPoint.Y);
        }
    }
}
