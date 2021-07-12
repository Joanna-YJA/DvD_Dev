using System;
using System.IO;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Numerics;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using NetTopologySuite.IO.Streams;
using AttributesTable = NetTopologySuite.Features.AttributesTable;
using Feature = NetTopologySuite.Features.Feature;
using Geometry = NetTopologySuite.Geometries.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Colors = System.Drawing.Color;
using Esri.ArcGISRuntime.UI;
using NetTopologySuite.Mathematics;
using NetTopologySuite.IO.KML;
using NetTopologySuite.IO;
using g3;
using MIConvexHull;
using System.Linq;

namespace DvD_Dev
{
    class PathFinder
    {

        // public GameObject[] scenes;
        public static float defaultWaypointSize = 0.2f;

        //public GameObject coordInputFieldX;
        //public GameObject coordInputFieldY;
        //public GameObject coordInputFieldZ;
        //public GameObject submitCoordsButton;

        //public GameObject referencePoint;
        public static float referencePointLat = 1.290167f;
        public static float referencePointLon = 103.8623f;

        public SpaceUnit racingDrone;

        //test
        public World shipWorld;
        MapController mapController;

        Commanding command;
        //BoundingBox targetDroneBB;

        Vector3 originalRefPos = new Vector3(57.49f, 1.5f, 142.57f);
        Vector3 centerOfMap = new Vector3(29590.69f, 31892.40f, 10);
        int octreeLevel = 8; // the center point in space of the 3D cube to construct the octree from
        float shipSize = .5f; // this is used to calculate ext, which is the extension of buffer space around buildings to reduce collisions
        float dimensions = 200; // Should not be changed. Dimensions in Unity units of the volume to be constructed in octree

        //test
        public List<Mesh> meshes = new List<Mesh>();
        List<Feature> features;
        //test
        public List<List<MapPoint>> vertices;
        public List<List<MapPoint>> triVertices;
        MapView mapView;
        SceneView sceneView;
        //test
        public static SpatialReference spatialRef;
        public static Dictionary<Coordinate, Vector3> vertexNormals;


        public PathFinder(ref MapView mapView, ref SceneView sceneView)
        {
            this.mapView = mapView;
            this.sceneView = sceneView;
        }
        public async Task InitPathFinder()
        {
            await ReadInput();
          LoadScene(meshes, centerOfMap, octreeLevel, shipSize, dimensions);
          LinkShip();
        }

        public async Task ReadInput()
        {
            FolderPicker picker = new FolderPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add("*");
            picker.FileTypeFilter.Add(".dbf");

            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                StorageFile shpFile = null, dbfFile = null, shxFile = null;
                foreach (StorageFile file in files)
                {

                    if (file.FileType.Equals(".shp")) shpFile = file;
                    else if (file.FileType.Equals(".dbf")) dbfFile = file;
                    else if (file.FileType.Equals(".shx")) shxFile = file;
                   // else if (file.FileType.Equals(".prj")) System.Diagnostics.Debug.WriteLine("Found prj file");

                    StorageFile localTemp = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(file.Name, CreationCollisionOption.ReplaceExisting);
                    await file.CopyAndReplaceAsync(localTemp);
                }

                if (dbfFile != null && shpFile != null && dbfFile != null)
                {

                    StorageFile localShp = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(shpFile.Name, CreationCollisionOption.ReplaceExisting);
                    StorageFile localDbf = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(dbfFile.Name, CreationCollisionOption.ReplaceExisting);
                    StorageFile localShx = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(shxFile.Name, CreationCollisionOption.ReplaceExisting);

                    await shpFile.CopyAndReplaceAsync(localShp);
                    await dbfFile.CopyAndReplaceAsync(localDbf);
                    await shxFile.CopyAndReplaceAsync(localShx);

                    ShapefileFeatureTable table = await ShapefileFeatureTable.OpenAsync(localShp.Path);
                   
                    spatialRef = table.SpatialReference;
                    // System.Diagnostics.Debug.WriteLine("ShapefileFeatureTable's spatial reference is " + spatialRef +  " vs " + table.SpatialReference);

                   // FeatureLayer layer = new FeatureLayer(table){
                   //     RenderingMode = FeatureRenderingMode.Dynamic
                   // };

                   // SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.White, 1.0);
                   // SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Blue, lineSymbol);
                   // SimpleRenderer renderer = new SimpleRenderer(fillSymbol);

                   // RendererSceneProperties sceneProperties = renderer.SceneProperties;
                   // sceneProperties.ExtrusionMode = ExtrusionMode.AbsoluteHeight;
                   // sceneProperties.ExtrusionExpression = "[HEIGHT]";

                   // layer.Renderer = renderer;
                   ////mapView.Map.OperationalLayers.Add(layer);
                   // sceneView.Scene.OperationalLayers.Add(layer);

                    features = new List<Feature>();
                    await GenerateFeatures(shpFile, dbfFile, features);
                   // GetAllVertices();
                   // Triangulate(features);
                  //  Extrude();
                }
                else
                    throw new FileNotFoundException("Could not locate .shp file or .dbf file given in input");
            } else {
                System.Diagnostics.Debug.WriteLine("Could not locate input file");
                throw new FileNotFoundException("Could not locate input file");
            }
        }

        public async Task GenerateFeatures(StorageFile shpFile, StorageFile dbfFile, List<Feature> features)
        {
            bool isFirst = true;
            GeometryFactory geomFact = new GeometryFactory();
            GeometryFactory threeDimGeomFact = new GeometryFactory();

            //test
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Brown, 1.0);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Gray, lineSymbol);
            GraphicsOverlay testOverlay = new GraphicsOverlay();
            testOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            sceneView.GraphicsOverlays.Add(testOverlay);


            using (var shpRandStream = await shpFile.OpenReadAsync())
            using (var shpStream = shpRandStream.AsStreamForRead())
            {
                ByteStreamProvider shpProvider = new ByteStreamProvider(StreamTypes.Shape, shpStream);

                using (var dbfRandStream = await dbfFile.OpenReadAsync())
                using (var dbfStream = dbfRandStream.AsStreamForRead())
                {
                    ByteStreamProvider dbfProvider = new ByteStreamProvider(StreamTypes.Data, dbfStream);
                    IStreamProviderRegistry providers = new ShapefileStreamProviderRegistry(shpProvider, dbfProvider, false, false);
                    ShapefileDataReader reader = new ShapefileDataReader(providers, geomFact);

                    DbaseFileHeader header = reader.DbaseHeader;

                    while (reader.Read())
                    {
                        Feature feature = new Feature();
                        Geometry geometry = reader.Geometry;
                        

                        AttributesTable attributesTable = new AttributesTable();
                        string[] keys = new string[header.NumFields];

                        for (int i = 0; i < header.NumFields; i++)
                        {
                            String keyName = header.Fields[i].Name;
                            keys[i] = keyName;
                            attributesTable.Add(keyName, reader.GetValue(i + 1));
                        }

                        //Convert 2D Geometry to 3D Geoemetry
                        double h = (double) attributesTable["HEIGHT"];

                        Mesh mesh = new Mesh(geometry, h, spatialRef);
                        meshes.Add(mesh);
                        if(isFirst) mesh.DisplayMeshGraphic(sceneView);
                        isFirst = false;
                       // testOverlay.Graphics.Add(graphic);

                        //List<Coordinate> topFace = new List<Coordinate>(), bottFace = new List<Coordinate>();

                        //Coordinate top, bott;
                        //foreach (Coordinate c in geometry.Coordinates)
                        //{
                        //    bott = new CoordinateZ(c.X / 10, c.Y / 10, 0);
                        //    top = new CoordinateZ(c.X / 10, c.Y / 10, h / 10);
                            //for(int i = 1; i < (h/10); i ++)
                            //{
                            //    topFace.Add(new CoordinateZ(c.X / 10, c.Y / 10, i));
                            //}
                        //    topFace.Add(top);
                        //    bottFace.Add(bott);
                        //}

                        //topFace.AddRange(bottFace);

                        //foreach (Coordinate c in topFace) System.Diagnostics.Debug.WriteLine("plotted vertice: " + c.ToString());

                        //for (int i = 0; i < bottFace.Count - 1; i++)
                        //{
                        //    topFace.Add(topFace[i].Copy());
                        //    topFace.Add(topFace[i + 1].Copy());
                        //    topFace.Add(bottFace[i + 1].Copy());
                        //    topFace.Add(bottFace[i].Copy());
                        //    topFace.Add(topFace[i].Copy());

                        //    //threeDimCoord.Add(bottFace[i]);
                        //    //threeDimCoord.Add(bottFace[i + 1]);
                        //    //threeDimCoord.Add(topFace[i + 1]);
                        //}

                        //topFace.Add(topFace[0].Copy());
            
                        // topFace.Add(topFace.Last().Copy());
                        //topFace.Add(topFace[0].Copy());
                        //topFace.Add(topFace[0]);

                        //List<Coordinate> twoFace = new List<int>().Concat(new List<int>()).ToList();
                        //Geometry threeDimGeom = threeDimGeomFact.CreateLineString(topFace.ToArray());



                        //List<Vertex> vertices = new List<Vertex>();
                        //foreach (Coordinate c in topFace)
                        //    vertices.Add(new Vertex(c.X, c.Y, c.Z));

                        //threeDimGeom = threeDimGeom.ConvexHull();
                        //ConvexHullCreationResult<Vertex, Face> convexHull = ConvexHull.Create<Vertex, Face>(vertices);
                        //List<Vertex> convexHullVertices = convexHull.Result.Points.ToList();
                        //List<Face> faces = convexHull.Result.Faces.ToList();

                        //foreach(Face f in faces)
                        //{
                        //    List<MapPoint> triPoints = new List<MapPoint>();
                        //    foreach(Vertex v in f.Vertices)
                        //    {
                        //        double[] pos = v.Position;
                        //        triPoints.Add(new MapPoint(pos[0] * 10, pos[1] * 10, pos[2] * 10, spatialRef));
                        //    }
                        //    Esri.ArcGISRuntime.Geometry.Polygon tri = new Esri.ArcGISRuntime.Geometry.Polygon(triPoints);
                        //    testOverlay.Graphics.Add(new Graphic(tri, fillSymbol));

                        //}                        
                        //List<Coordinate> geomCoord = new List<Coordinate>();
                        //// foreach (Face face in faces) {
                        ////Face face = faces[0];
                        //    foreach (Vertex v in convexHullVertices)
                        //    {
                        //        double[] pos = v.Position;
                        //        CoordinateZ c = new CoordinateZ(pos[0], pos[1], pos[2]);
                        //        geomCoord.Add(c);
                        //        //System.Diagnostics.Debug.WriteLine("Convex hull vertice: " + c);
                        //    }

                        // }
                        //Geometry geom = threeDimGeomFact.CreateLineString(geomCoord.ToArray());
                        // string kmlExtrudeStr = KMLWriter.WriteGeometry(threeDimGeom, h, 32, true, "absolute");

                        //System.Diagnostics.Debug.WriteLine("KML STRING: " + kmlExtrudeStr); 
                        //     KMLReader kmlReader = new KMLReader();
                        //     threeDimGeom = kmlReader.Read(kmlExtrudeStr);


                        //List<Coordinate> threeDimCoord = new List<Coordinate>(topFace);
                        //for (int i = 0; i < topFace.Count - 1; i++)
                        //{
                        //    threeDimCoord.Add(topFace[i]);
                        //    threeDimCoord.Add(topFace[i + 1]);
                        //    threeDimCoord.Add(bottFace[i + 1]);
                        //    threeDimCoord.Add(bottFace[i]);
                        //    threeDimCoord.Add(topFace[i]);

                        //    //threeDimCoord.Add(bottFace[i]);
                        //    //threeDimCoord.Add(bottFace[i + 1]);
                        //    //threeDimCoord.Add(topFace[i + 1]);
                        //}
                        //threeDimCoord.AddRange(bottFace);


                        //foreach (Coordinate c in threeDimGeom.Coordinates)
                        //    System.Diagnostics.Debug.WriteLine("threeDimGeom coords: " + c);
                        //threeDimGeom = threeDimGeom.ConvexHull();

                        //CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();
                        //CoordinateSystemFactory csFact = new CoordinateSystemFactory();

                        //CoordinateSystem wgs84 = csFact.CreateGeocentricCoordinateSystem();
                        //CoordinateSystem local = csFact.CreateProjectedCoordinateSystem("Local Coordinate System", gcs, projection, ProjNet.CoordinateSystems.LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North))

                        //CoordinateTransformation transformToLocal = ctFact.CreateFromCoordinateSystems(wgs84, local);

                        //Geometry transformedGeom = GeometryTransform.TransformGeometry(transGeomFact, geometry, transformToLocal);


                        //feature.Geometry = geom;
                        //feature.Attributes = attributesTable;
                        //features.Add(feature);
                    }

                    reader.Close();
                    reader.Dispose();
                  // PrintAllGeneratedFeature();
                }
            }
        }

        public async void PrintAllGeneratedFeature()
        {
            GraphicsOverlay absOverlay = new GraphicsOverlay();
            absOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            sceneView.GraphicsOverlays.Add(absOverlay);

            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Purple, 1.0);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Blue, lineSymbol);

            foreach (Feature f in features)
            {
                List<MapPoint> points = new List<MapPoint>();
                foreach (Coordinate c in f.Geometry.Coordinates)
                {
                    MapPoint p = new MapPoint(c.X * 10, c.Y * 10, c.Z * 10, spatialRef);
                    points.Add(p);
                    //System.Diagnostics.Debug.WriteLine("feature vertice: " + p.ToString());
                }
                Esri.ArcGISRuntime.Geometry.Polyline poly = new Esri.ArcGISRuntime.Geometry.Polyline(points);
                absOverlay.Graphics.Add(new Graphic(poly, fillSymbol));
            }
            
        }
        public void GetAllVertices()
        {

            vertices = new List<List<MapPoint>>();
            foreach (Feature f in features)
            {
                List<MapPoint> featurePoints = new List<MapPoint>();
                foreach (Coordinate c in f.Geometry.Coordinates)
                {
                    featurePoints.Add(new MapPoint(c.X / 10, c.Y / 10, c.Z / 10, spatialRef));
                }
                vertices.Add(featurePoints);
            }

        }

        public void Triangulate(List<Feature> features)
        {
            List<GeometryCollection> meshes = new List<GeometryCollection>();
            foreach (Feature feature in features)
            {
                Coordinate[] coordinates = feature.Geometry.Coordinates;
                var builder = new ConformingDelaunayTriangulationBuilder();
                builder.SetSites(feature.Geometry);
                GeometryCollection collection = builder.GetTriangles(new GeometryFactory());
                meshes.Add(collection);
            }
            FindVertexNormals();
           // PrintAllTriangulatedVertices();
        }

        public void Extrude()
        {
            //foreach(GeometryCollection mesh in meshes)
            //{
            //    List<Vector3d> vertices = new List<Vector3d>();
            //    List<Vector3d> normals = new List<Vector3d>();
            //    List<int> triangles = new List<int>();
            //    foreach (Geometry tri in mesh)
            //    {
            //        foreach (Coordinate c in tri.Coordinates)
            //        {
            //            vertices.Add(new Vector3d(c.X, c.Y, c.Z));
            //            Vector3 normal = vertexNormals[c];
            //            normals.Add(new Vector3d(normal.X, normal.Y, normal.Z));
            //            triangles.Add(triangles.Count);
            //        }
            //    }
            //    DMesh3 dmesh = DMesh3Builder.Build(vertices, triangles, normals);
            //   MeshExtrudeMesh extrusion = new MeshExtrudeMesh(dmesh);
            //}
        }

        public static Vector3 CoordToVect(Coordinate c)
        {
            return new Vector3(Convert.ToSingle(c.X), Convert.ToSingle(c.Y), Convert.ToSingle(c.Z));
        }
        public void FindVertexNormals()
        {
            List<GeometryCollection> meshes = new List<GeometryCollection>();
            vertexNormals = new Dictionary<Coordinate, Vector3>();
            foreach (GeometryCollection mesh in meshes)
            {
                foreach (Geometry tri in mesh)
                {
                    
                    Coordinate[] triVerts = tri.Coordinates;

                    Coordinate triNorm;
                    if (Math.Abs(triVerts[0].Z - 0) < 0.001 && Math.Abs(triVerts[1].Z - 0) < 0.001 && Math.Abs(triVerts[2].Z - 0) < 0.01)
                    {
                        triNorm = VectorMath.NormalToTriangle(triVerts[2], triVerts[1], triVerts[0]);
                       // System.Diagnostics.Debug.WriteLine("Normal points down, triNorm: " + triNorm.ToString());
                    }
                    else if ((Math.Abs(triVerts[0].X - triVerts[1].X) < 0.001 && Math.Abs(triVerts[1].X - triVerts[2].X) < 0.001)
                        || (Math.Abs(triVerts[0].Y - triVerts[1].Y) < 0.001 && Math.Abs(triVerts[1].Y - triVerts[2].Y) < 0.001))
                    {
                        triNorm = VectorMath.NormalToTriangle(triVerts[0], triVerts[1], triVerts[2]);
                       // System.Diagnostics.Debug.WriteLine("Normal points sideways, triNorm: " + triNorm.ToString());
                    }
                    else
                    {
                        triNorm = VectorMath.NormalToTriangle(triVerts[0], triVerts[1], triVerts[2]);
                       // System.Diagnostics.Debug.WriteLine("Normal points up, mormalized triNorm: " + Vector3.Normalize(CoordToVect(triNorm)).ToString());
                    }
                    Vector3 triNormVect = CoordToVect(triNorm);
                    //System.Diagnostics.Debug.WriteLine("Triangle normal vector: " + triangleNormal.ToString());
                    for (int i = 0; i < 3; i++)
                    {
                        if (!vertexNormals.ContainsKey(triVerts[i])) vertexNormals[triVerts[i]] = Vector3.Zero;
                        vertexNormals[triVerts[i]] += triNormVect;
                       // System.Diagnostics.Debug.WriteLine("New normal for that coordinate: " + vertexNormals[triangleVertices[i]].ToString());
                    }
                }
            }

            List<Coordinate> keys = new List<Coordinate>(vertexNormals.Keys);
            foreach(Coordinate vertex in keys)
                vertexNormals[vertex] = Vector3.Normalize(vertexNormals[vertex]);

            //foreach (Vector3D normal in vertexNormals.Values)
            //    System.Diagnostics.Debug.WriteLine("Vertex Normal: " + normal.ToString());
        } 
        public void PrintAllTriangulatedVertices()
        {
            //triVertices = new List<List<MapPoint>>();
            //foreach (GeometryCollection mesh in meshes)
            //{
            //    foreach (Geometry tri in mesh)
            //    {
            //        List<MapPoint> triPoints = new List<MapPoint>();
            //        foreach (Coordinate c in tri.Coordinates)
            //        {
            //            triPoints.Add(new MapPoint(c.X * 10, c.Y * 10, 32, spatialRef));
            //            // System.Diagnostics.Debug.WriteLine("TRIANGLE VERTICE: " + c.ToString());
            //        }
            //        triVertices.Add(triPoints);
            //    }
            //    triVertices.Add(null);
            //}
            GeometryFactory geomFact = new GeometryFactory();
            List<List<Coordinate>> triCoord = new List<List<Coordinate>>();
            List<GeometryCollection> meshes = new List<GeometryCollection>();
            foreach (GeometryCollection mesh in meshes)
            {
                List<Coordinate> meshCoord = new List<Coordinate>();
                foreach (Geometry tri in mesh)
                {
                    meshCoord.AddRange(tri.Coordinates);
                }  
                Geometry meshGeom = geomFact.CreateLineString(meshCoord.ToArray());
               meshGeom = meshGeom.ConvexHull();
                meshCoord = new List<Coordinate>(meshGeom.Coordinates);
                triCoord.Add(meshCoord);
            }

            GraphicsOverlay absOverlay = new GraphicsOverlay();
            absOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            sceneView.GraphicsOverlays.Add(absOverlay);

            SimpleLineSymbol meshLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Magenta, 1.0);
            SimpleFillSymbol meshFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Pink, meshLineSymbol);
            //Display triangulated vertices
            foreach(List<Coordinate> meshCoord in triCoord)
            {
                List<MapPoint> meshPoints = new List<MapPoint>();
                foreach(Coordinate c in meshCoord){
                    MapPoint p = new MapPoint(c.X * 10, c.Y * 10, 0, spatialRef);
                    meshPoints.Add(p);
                    //System.Diagnostics.Debug.WriteLine("meshPoint: " + p);
                }

                Esri.ArcGISRuntime.Geometry.Polygon mesh = new Esri.ArcGISRuntime.Geometry.Polygon(meshPoints);
                Graphic meshGraphic = new Graphic(mesh, meshFillSymbol);
                absOverlay.Graphics.Add(meshGraphic);
            }
        }

        //public async Task DeserializeWorld()
        //{
        //Octree space = null;
        //Graph spaceGraph = null;
        //List<Arc> arcList = null;

        //////System.Diagnostics.Debug.WriteLine("Starting to deserialize World properties...");
        //FileOpenPicker picker = new FileOpenPicker();
        //picker.ViewMode = PickerViewMode.List;
        //picker.FileTypeFilter.Add(".json");

        //JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
        //{
        //    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        //    Formatting = Formatting.Indented,
        //    TypeNameHandling = TypeNameHandling.Auto,
        //    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        //    ObjectCreationHandling = ObjectCreationHandling.Auto
        //});

        //StorageFile file1 = await picker.PickSingleFileAsync();
        //if (file1 != null)
        //{
        //    using (var inputStream = await file1.OpenReadAsync())
        //    using (var classicStream = inputStream.AsStreamForRead())
        //    {
        //        result = reader.Read(classicStream, "obj", ReadOptions.Defaults);
        //    }
        //}
        //else
        //{
        //    throw new FileNotFoundException("Could not locate .json file of Octree to deserialize");
        //}

        //MeshFormatReader mfr = new OBJFormatReader();
        //List<String> exts = mfr.SupportedExtensions;
        //foreach (String ext in exts)
        //    System.Diagnostics.Debug.WriteLine("Supported format: " + ext);
        //if (result.code == IOCode.Ok)
        //    meshes = builder.Meshes.ToArray();
        //else
        //    throw new ExecutionEngineException("The file stream is not read properly, reader.Read error code: " + result.code);

        //System.Diagnostics.Debug.WriteLine("meshes: " + meshes);

        //System.Diagnostics.Debug.WriteLine("FINISHED deserializing octree!");
        //StorageFile file2 = await picker.PickSingleFileAsync();
        //if (file2 != null)
        //{
        //    using (var inputStream = await file2.OpenReadAsync())
        //    using (var classicStream = inputStream.AsStreamForRead())
        //    using (var streamReader = new StreamReader(classicStream))
        //    { 
        //        spaceGraph = (Graph)serializer.Deserialize(streamReader, typeof(Graph));
        //    }
        //}
        //else
        //{
        //    throw new FileNotFoundException("Could not locate .json file of Graph to deserialize");
        //}

        ////System.Diagnostics.Debug.WriteLine("FINISHED deserializing graph!");

        //StorageFile file3 = await picker.PickSingleFileAsync();
        //if (file3 != null)
        //{
        //    using (var inputStream = await file3.OpenReadAsync())
        //    using (var classicStream = inputStream.AsStreamForRead())
        //    using (var streamReader = new StreamReader(classicStream))
        //    {
        //        arcList = ((World)serializer.Deserialize(streamReader, typeof(World))).arcList;
        //    }
        //}
        //else
        //{
        //    throw new FileNotFoundException("Could not locate .json file of Octree to deserialize");
        //}

        //System.Diagnostics.Debug.WriteLine("FINISHED deserializing arcList!");
        //System.Diagnostics.Debug.WriteLine("FINISHED deserializing world properties!");

        //System.Diagnostics.Debug.WriteLine("space: " + space
        // + " spaceGraph: " + spaceGraph
        //+ " arcList: " +    arcList);
        // shipWorld = new World(space, Graph.GraphType.CORNER);
        //test
        //await SerializeWorld();


        // }

        // Construct an Octree from the loaded scene
        public void LoadScene(List<Mesh> meshes, Vector3 centerCoords, int octreeLevels, float shipSize, float dimension)
        {
            racingDrone = new SpaceUnit();
            float ext = MathF.Max(shipSize - 16f / (1 << 8) * MathF.Sqrt(3) / 2, 0);
            racingDrone.ext = ext;

            Coordinate testCoord = new CoordinateZ(11557028.5197362/10, 150832.223844404/10, 0);
            Vector3 testCenter = new Vector3(Convert.ToSingle(testCoord.X), Convert.ToSingle(testCoord.Y), Convert.ToSingle(testCoord.Z));
            shipWorld = new World(meshes, dimensions, testCenter, octreeLevels, ext, true, Graph.GraphType.CORNER);

            command = new Commanding(shipWorld);
            //command.ext = ext;
            //Settings.showShipTrajectory = true;

            // scale world by x10
            //originalRefPos = referencePoint.transform.position;
            //scenes[sceneIndex].transform.parent.localScale = new Vector3(10, 10, 10);

        }


        //public void PathfindingModeOn()
        //{
        //    racingDrone.GetComponent<SpaceUnit>().enabled = true; //Space Unit is visible
        //    racingDrone.GetComponent<StabilisedAIController>().enabled = false;
        //    racingDrone.GetComponent<Rigidbody>().useGravity = false;
        //}

        //public void Update()
        //{
        //    if (targetDroneBB.DrawBoundingBox(out Vector2 _))
        //    {
        //        if (Vector3.Distance(racingDrone.transform.position, targetDroneBB.targetObject.transform.position) < targetDroneBB.maxVisibleDistance - 5)
        //        {
        //            if (targetDroneBB.targetObject.GetComponent<MovingBehavior>() != null)
        //                targetDroneBB.targetObject.GetComponent<MovingBehavior>().enabled = true;
        //        }
        //    }
        //}

        // Read data from the input fields and move to provided coords
        public void MoveToCoords(MapPoint[] sourceDest)
        {
            ////Set initial location of drone
            ////racingDrone.landed = false;
            //float lonSrc = Convert.ToSingle(sourceDest[0].X),
            //      latSrc = Convert.ToSingle(sourceDest[0].Y),
            //      heightSrc = 10;
            //racingDrone.position = new Vector3(lonSrc, heightSrc, latSrc);
            //racingDrone.standPoint = new Vector3(lonSrc, heightSrc, latSrc); //Hover position
            ////PathfindingModeOn();

            //float lonDest = Convert.ToSingle(sourceDest[1].X),
            //      latDest = Convert.ToSingle(sourceDest[1].Y),
            //      heightDest = 10;
            //System.Diagnostics.Debug.WriteLine("Finding path from " + lonSrc + ", " + heightSrc + ", " + latSrc + " to " + lonDest + ", " + heightDest + ", " + latDest);
            // command.MoveOrder(new Vector3(lonDest, heightDest, latDest), mapController);
            float srcX = Convert.ToSingle(sourceDest[0].X) / 10,
                  srcY = Convert.ToSingle(sourceDest[0].Y) / 10,
                  //srcZ = Convert.ToSingle(sourceDest[0].Z) / 10;
                  srcZ = 25f / 10;
            Vector3 src = new Vector3(srcX, srcY, srcZ);
            racingDrone.position = racingDrone.standPoint = src;

            float destX = Convert.ToSingle(sourceDest[1].X) / 10,
                  destY = Convert.ToSingle(sourceDest[1].Y) / 10,
                   // destZ = Convert.ToSingle(sourceDest[1].Z) / 10;
                   destZ = 25f / 10;
            Vector3 dest = new Vector3(destX, destY, destZ);
            System.Diagnostics.Debug.WriteLine("Finding path from src " + src.ToString() + " to dest " + dest.ToString());
            command.MoveOrder(dest, mapController);
        }

        // ORIGINAL: Read data from the input fields and move to provided coords
        //public void MoveToCoords(MapPoint[] sourceDest)
        //{
        //    //Set initial location of drone
        //    //racingDrone.landed = false;
        //    float lonSrc = Convert.ToSingle(sourceDest[0].X), 
        //          latSrc = Convert.ToSingle(sourceDest[0].Y), 
        //          heightSrc = 10;
        //    float xSrc = ConvertLatLonToLocal(lonSrc, latSrc)[0];
        //    float zSrc = ConvertLatLonToLocal(lonSrc, latSrc)[1];
        //    racingDrone.position = new Vector3(xSrc, heightSrc, zSrc);
        //    racingDrone.standPoint = new Vector3(xSrc, heightSrc, zSrc); //Hover position
        //    //PathfindingModeOn();

        //    float lonDest = Convert.ToSingle(sourceDest[1].X),
        //          latDest = Convert.ToSingle(sourceDest[1].Y), 
        //          heightDest = 10;

        //    float xDest = ConvertLatLonToLocal(lonDest, latDest)[0];
        //    float zDest = ConvertLatLonToLocal(lonDest, latDest)[1];

            
        //    System.Diagnostics.Debug.WriteLine("Finding path from " + new Vector3(xSrc, heightSrc, zSrc) + " to " + new Vector3(xDest, heightDest, zDest));
        //    command.MoveOrder(new Vector3(xDest, heightDest, zDest), mapController);
        //}
        // Has to be corrected if to be used, is wrong now
        public static List<float> ConvertLocalToLatLon(float x, float z)
        {
            // lat is Z coordinates
            //float baseOfFlyerLat = 1.28936f;
            //float baseOfFlyerLon = 103.86317f;
            List<float> latLon = new List<float>();
            //float z_dist = (z - referencePoint.transform.position.Z)
            //;
            //float x_dist = (x - referencePoint.transform.position.X) * 10;
            float z_dist = z * 10;
            float x_dist = x * 10;
            //Convert distance in metres to lat/lon
            latLon.Add(referencePointLat + z_dist / 30.6f * 0.00027778f);
            latLon.Add(referencePointLon + x_dist / 30.6f * 0.00027778f);
            return latLon;
        }

        /// <summary>
        /// Converts lat/lon entered into distance(metres) 
        /// from the lat/lon of the reference point.
        /// This conversion only works in the context of Singapore.
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <returns></returns>
        public List<float> ConvertLatLonToLocal(float lon, float lat)
        {
            List<float> local = new List<float>();
            float lon_dist = lon - referencePointLon;
            float lat_dist = lat - referencePointLat;
            local.Add(originalRefPos.X + lon_dist * 30.6f / 0.00027778f / 10);
            local.Add(originalRefPos.Z + lat_dist * 30.6f / 0.00027778f / 10);

            //local.Add(lon_dist * 30.6f / 0.00027778f / 10);
            //local.Add(lat_dist * 30.6f / 0.00027778f / 10);
            return local;
        }

        // Allow the ship to be commanded
        // If multiple ships are added to command, they can all pathfind to the same location together
        public void LinkShip()
        {
            //link racing drone to surrounding space
            racingDrone.space = shipWorld.space;
            racingDrone.spaceGraph = shipWorld.spaceGraph;
            command.activeUnits.Add(racingDrone);
        }

        public void OverlayShapefile()
        {
            //PathGeometry collection = new PathGeometry();
            //foreach(Feature feature in features)
            //{
            //    PathFigure figure = new PathFigure();
            //    new LineSegment();
            //    feature.Geometry.Coordinates
            //}
        }
    }
}
