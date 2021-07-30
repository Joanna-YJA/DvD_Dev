using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Labeling;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Streams;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using ArcGisGeometry = Esri.ArcGISRuntime.Geometry.Geometry;
using AttributesTable = NetTopologySuite.Features.AttributesTable;
using Feature = NetTopologySuite.Features.Feature;
using Geometry = NetTopologySuite.Geometries.Geometry;

namespace DvD_Dev
{
    class PathFinder
    {
        StorageFile[] targetFiles;

        //test
        public static Esri.ArcGISRuntime.Geometry.Envelope env;

        Octree space;
        public World shipWorld;
        Commanding command;
        SpaceUnit racingDrone;
        FootprintCalculator footprintCalc;
        public static SpatialReference spatialRef;

        SceneView sceneView;
        public static GraphicsOverlay pathOverlay;
        GraphicsOverlay meshOverlay, octreeOverlay, graphOverlay;

        static ClassBreaksRenderer renderer;
        static LabelDefinition heightLabelDef;

        public static float defaultWaypointSize = 0.2f;
        public static float referencePointLat = 1.290167f;
        public static float referencePointLon = 103.8623f;

        int octreeLevel = 8; // the center point in space of the 3D cube to construct the octree from
        float shipSize = .5f; // this is used to calculate ext, which is the extension of buffer space around buildings to reduce collisions
        int octreeDimM = 200;

        public List<Mesh> meshes = new List<Mesh>();
        public static List<ArcGisGeometry> flatGeometries = new List<ArcGisGeometry>();
        List<MapPoint> points;

        bool showMesh, showMeshNormals, showBoundingBox, showOctree, showOctreeBlockedNodes, showGraph, showPath, showFootprint;
        public static int fieldDimM = 200;

        static SimpleLineSymbol boxOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Gray, 2);
        static SimpleMarkerSymbol centerPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.Gray, 10);

        static PathFinder()
        {
            InitializeRenderer();
            InitializeLabelDef();
        }

        private static void InitializeRenderer()
        {
            // Define the colors that will be used by the unique value renderer.
            Color gray = Color.FromArgb(255, 153, 153, 153);
            Color blue1 = Color.FromArgb(255, 183, 240, 233);
            Color blue2 = Color.FromArgb(255, 142, 233, 221);
            Color blue3 = Color.FromArgb(255, 101, 232, 215);
            Color blue4 = Color.FromArgb(255, 59, 217, 196);
            Color blue5 = Color.FromArgb(255, 45, 202, 182);

            // Create a gray outline and five fill symbols with different shades of blue.
            SimpleLineSymbol outlineSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, gray, 1);
            SimpleFillSymbol simpleFileSymbol1 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue1, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol2 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue2, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol3 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue3, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol4 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue4, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol5 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue5, outlineSimpleLineSymbol);

            // Create a list of five class breaks for different population ranges.
            List<ClassBreak> listClassBreaks = new List<ClassBreak>
            {
                new ClassBreak("Buildings with height below 10m", "0 - 10", 0, 10, simpleFileSymbol1),
                new ClassBreak("Buildings with height from 10m to 20m", "10 - 20", 10, 20, simpleFileSymbol2),
                new ClassBreak("Buildings with height from 20m to 30m", "20 - 30", 20, 30, simpleFileSymbol3),
                new ClassBreak("Buildings with height from 30m to 40m", "30 - 40", 30, 40, simpleFileSymbol4),
                new ClassBreak("Buildings with height above 40m", "> 40", 40, int.MaxValue, simpleFileSymbol5)
            };

            // Create and return the a class break renderer for use with the POP2007 field in the counties sub-layer.
            renderer = new ClassBreaksRenderer("HEIGHT", listClassBreaks);

            RendererSceneProperties sceneProps = renderer.SceneProperties;
            sceneProps.ExtrusionMode = ExtrusionMode.AbsoluteHeight;
            sceneProps.ExtrusionExpression = "[HEIGHT]";
        }

        private static void InitializeLabelDef()
        {
            TextSymbol textSymbol = new TextSymbol
            {
                Size = 24,
                Color = Color.Black,
                HaloColor = Color.White,
                HaloWidth = 2,
            };

            LabelExpression arcadeLabelExpression = new ArcadeLabelExpression("$feature.HEIGHT");

            heightLabelDef = new LabelDefinition(arcadeLabelExpression, textSymbol)
            {
                Placement = Esri.ArcGISRuntime.ArcGISServices.LabelingPlacement.PolygonAlwaysHorizontal
            };
        }
        public PathFinder(ref SceneView sceneView)
        {
            this.sceneView = sceneView;
            pathOverlay = new GraphicsOverlay();
            pathOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            sceneView.GraphicsOverlays.Add(pathOverlay);
        }


        public async Task ReadShapefile()
        {
            FolderPicker picker = new FolderPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add("*");

            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                this.targetFiles = new StorageFile[4];
                foreach (StorageFile file in files)
                {

                    if (file.FileType.Equals(".shp")) targetFiles[0] = file;
                    else if (file.FileType.Equals(".dbf")) targetFiles[1] = file;
                    else if (file.FileType.Equals(".shx")) targetFiles[2] = file;
                    else if (file.FileType.Equals(".prj")) targetFiles[3] = file;
                }

                string shpFilePath = "";
                for (int i = 0; i < 4; i++)
                {
                    if (targetFiles[i] != null)
                    {
                        StorageFile newFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(targetFiles[i].Name, CreationCollisionOption.ReplaceExisting);
                        await targetFiles[i].CopyAndReplaceAsync(newFile);

                        targetFiles[i] = newFile;
                        if (i == 0) shpFilePath = newFile.Path;
                    }
                    else throw new Exception("Some files in the shapefile are missing.");
                }

                ShapefileFeatureTable table = await ShapefileFeatureTable.OpenAsync(shpFilePath);
                spatialRef = table.SpatialReference;

                FeatureLayer layer = new FeatureLayer(table)
                {
                    RenderingMode = FeatureRenderingMode.Dynamic
                };

                layer.Renderer = renderer;
                layer.LabelDefinitions.Add(heightLabelDef);

                layer.LabelsEnabled = true;

                sceneView.Scene.OperationalLayers.Add(layer);



            }
            else throw new FileNotFoundException("Folder not found.");
        }

        public async Task DeserializeWorld()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add(".json");

            SpatialReferenceConverter conv = new SpatialReferenceConverter();
            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
            });
            serializer.Converters.Add(conv);

            StorageFile worldFile = await picker.PickSingleFileAsync();
            if (worldFile != null)
            {
                using (var inputStream = await worldFile.OpenReadAsync())
                using (var classicStream = inputStream.AsStreamForRead())
                using (var streamReader = new StreamReader(classicStream))
                {
                    shipWorld = (World)serializer.Deserialize(streamReader, typeof(World));
                }

                //shipWorld.allocateArcs();
                if (showOctree)
                    shipWorld.space.DisplayOctreeNodes(ref octreeOverlay);
            }
            else
            {
                throw new FileNotFoundException("Could not locate .json file of Octree to deserialize");
            }
        }

        public async Task SerializeWorld()
        {
            FolderPicker picker = new FolderPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add("*");

            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = CustomVector3ContractResolver.Instance,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            }
);

            StorageFolder folder = await picker.PickSingleFolderAsync();



            if (folder != null)
            {
                StorageFile octreeFile = await folder.CreateFileAsync("World.json", CreationCollisionOption.ReplaceExisting);

                using (var inputStream = await octreeFile.OpenAsync(FileAccessMode.ReadWrite))
                using (var classicStream = inputStream.AsStreamForWrite())
                using (var streamWriter = new StreamWriter(classicStream))
                {
                    serializer.Serialize(streamWriter, shipWorld);

                }
            }
            else
            {
                throw new FileNotFoundException("Could not locate .json file of Octree to deserialize");
            }
        }

        public async Task GenerateWorld(MapPoint center, int fieldDimM)
        {
            PathFinder.fieldDimM = fieldDimM;

            if (space == null && targetFiles == null)
                throw new ArgumentException("Either the Shapefile or Octree Json must be uploaded");
            else if (space == null)
                await GenerateMeshes(targetFiles[0], targetFiles[1]);

            LoadScene(center, octreeLevel, shipSize);
            space = shipWorld.space;
            LinkShip();

            if (showOctree)
                shipWorld.space.DisplayOctreeNodes(ref octreeOverlay);
            else if (showOctreeBlockedNodes)
                shipWorld.space.DisplayOctreeBlockedNodes(ref octreeOverlay);

            if (showGraph)
                shipWorld.spaceGraph.DisplayGraphNodes(ref graphOverlay, space);
            if (showBoundingBox)
            {
                DisplayBothBoundingBox();
            }

        }

        public async Task GenerateMeshes(StorageFile shpFile, StorageFile dbfFile)
        {
            GeometryFactory geomFact = new GeometryFactory();

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

                        double h = (double)attributesTable["HEIGHT"];


                        //Create ArcGis Geometry
                        List<MapPoint> geomVertices = new List<MapPoint>();
                        foreach (Coordinate geomCoord in geometry.Coordinates)
                        {
                            geomVertices.Add(new MapPoint(geomCoord.X, geomCoord.Y, geomCoord.Z, spatialRef));
                        }
                        flatGeometries.Add(new Esri.ArcGISRuntime.Geometry.Polygon(geomVertices));

                        Mesh mesh = new Mesh(geometry, h, spatialRef);
                        meshes.Add(mesh);
                    }

                    reader.Close();
                    reader.Dispose();
                }
            }

            if (showMesh)
            {
                foreach (Mesh mesh in meshes)
                    mesh.DisplayMesh(ref meshOverlay);
            }

            if (showMeshNormals)
            {
                foreach (Mesh mesh in meshes)
                    mesh.DisplayMeshNormals(ref meshOverlay);
            }
        }

        public static Vector3 CoordToVect(Coordinate c)
        {
            return new Vector3(Convert.ToSingle(c.X), Convert.ToSingle(c.Y), Convert.ToSingle(c.Z));
        }

        // Construct an Octree from the loaded scene
        public void LoadScene(MapPoint center, int octreeLevels, float shipSize)
        {
            racingDrone = new SpaceUnit();
            float ext = MathF.Max(shipSize - 16f / (1 << 8) * MathF.Sqrt(3) / 2, 0);
            racingDrone.ext = ext;

            Vector3 centerMap = new Vector3((float)center.X / 10, (float)center.Y / 10, (float)center.Z / 10);

            shipWorld = new World(ref sceneView, spatialRef, meshes, octreeDimM, centerMap, octreeLevels, 0.5f, true, Graph.GraphType.CORNER);

            command = new Commanding(ref sceneView, shipWorld);
        }

        public void TravelAndSearch(MapPoint[] sourceDest)
        {
            Vector3 localSrc = Vector3.Zero, localDest = Vector3.Zero;
            ConvertToLocal(sourceDest, ref localSrc, ref localDest);

            double timeLimitMin = 30, avrSpeedMph = 31;
            double targetDistM = (timeLimitMin / 60) * (avrSpeedMph * 1609.34);
            double distanceM = double.MaxValue, altitudeM = 20;

            int numSpiralPoints = 0;


            while (distanceM > targetDistM)
            {
                footprintCalc = new FootprintCalculator(++altitudeM);
                points = new List<MapPoint>();
                command.SearchInSpiral(ref points, localDest, footprintCalc);
                numSpiralPoints = points.Count;

                Vector3 localStart = new Vector3((float)points[1].X / 10, (float)points[1].Y / 10f, (float)points[1].Z / 10f);
                MoveToCoords(localSrc, localStart);
                //test
                List<MapPoint> nonNullPoints = new List<MapPoint>();
                foreach (MapPoint p in points)
                    if (p != null)
                        nonNullPoints.Add(p);
                Polyline path = new Polyline(nonNullPoints);
               // ArcGisGeometry densifiedPath = GeometryEngine.DensifyGeodetic(path, 1, LinearUnits.Meters, GeodeticCurveType.Geodesic);
                distanceM = GeometryEngine.LengthGeodetic(path, LinearUnits.Meters, GeodeticCurveType.Geodesic);
            }

            if (showPath) command.ShowPath(ref points, pathOverlay);
            if (showFootprint)
            {
                int firstSpiralPt = points.Count - numSpiralPoints;
                footprintCalc.ShowFootprintCoverage(points.GetRange(firstSpiralPt, numSpiralPoints), pathOverlay);
                //footprintCalc.ShowSeperateFootprint(points.Skip(points.Count - numSpiralPoints), pathOverlay);
            }
           // WriteToKml();
        }

        public async void WriteToKml()
        {
            KmlDocument kmlDoc = new KmlDocument() { Name = "KML Drone Path" };
            KmlFolder kmlFolder = new KmlFolder();
            foreach (MapPoint p in points)
            {
                if (p == null) continue;
                ArcGisGeometry geom = (ArcGisGeometry)p;
                ArcGisGeometry projectedGeom = GeometryEngine.Project(geom, SpatialReferences.Wgs84);
                KmlGeometry kmlGeom = new KmlGeometry(projectedGeom, KmlAltitudeMode.RelativeToGround);
                KmlPlacemark placemark = new KmlPlacemark(kmlGeom);

                kmlFolder.ChildNodes.Add(placemark);
            }

            kmlDoc.ChildNodes.Add(kmlFolder);

            List<MapPoint> nonNullPoints = new List<MapPoint>();
            foreach (MapPoint p in points)
                if (p != null) nonNullPoints.Add(p);

            for (int i = 0; i < nonNullPoints.Count; i += 99)
            {
                ArcGisGeometry line = (ArcGisGeometry)new Polyline(
                                        nonNullPoints.GetRange(i, Math.Min(nonNullPoints.Count - i, 99)));
                ArcGisGeometry projectedLine = GeometryEngine.Project(line, SpatialReferences.Wgs84);
                KmlGeometry kmlLine = new KmlGeometry(projectedLine, KmlAltitudeMode.RelativeToGround);
                KmlPlacemark placemarkLine = new KmlPlacemark(kmlLine);
                kmlDoc.ChildNodes.Add(placemarkLine);

            }

            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
            savePicker.FileTypeChoices.Add("KMZ file", new List<string>() { ".kmz" });
            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                try
                {
                    using (Stream stream = await file.OpenStreamForWriteAsync())
                    {
                        // Write the KML document to the stream of the file.
                        await kmlDoc.WriteToAsync(stream);
                    }
                }
                catch
                {
                   throw new FileNotFoundException("File not saved.");
                }
            }
        }

        public void ConvertToLocal(MapPoint[] sourceDest, ref Vector3 localSrc, ref Vector3 localDest)
        {
            float srcX = Convert.ToSingle(sourceDest[0].X) / 10,
                 srcY = Convert.ToSingle(sourceDest[0].Y) / 10,
                 srcZ = 20f / 10;
            localSrc = new Vector3(srcX, srcY, srcZ);

            float destX = Convert.ToSingle(sourceDest[1].X) / 10,
                destY = Convert.ToSingle(sourceDest[1].Y) / 10,
                 destZ = 20f / 10;
            localDest = new Vector3(destX, destY, destZ);
        }

        // Read data from the input fields and move to provided coords
        public void MoveToCoords(Vector3 localSrc, Vector3 localDest)
        {
            racingDrone.position = racingDrone.standPoint = localSrc;
            command.MoveOrder(localDest, ref points);
        }

        public static List<float> ConvertLocalToLatLon(float x, float z)
        {
            List<float> latLon = new List<float>();
            float z_dist = z * 10;
            float x_dist = x * 10;
            //Convert distance in metres to lat/lon
            latLon.Add(referencePointLat + z_dist / 30.6f * 0.00027778f);
            latLon.Add(referencePointLon + x_dist / 30.6f * 0.00027778f);
            return latLon;
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

        public void ShowMesh()
        {
            if (meshOverlay == null)
            {
                meshOverlay = new GraphicsOverlay();
                meshOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(meshOverlay);
            }

            showMesh = true;
        }

        public void ShowMeshNormals()
        {
            if (meshOverlay == null)
            {
                meshOverlay = new GraphicsOverlay();
                meshOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(meshOverlay);
            }

            showMeshNormals = true;
        }

        public void ShowPath()
        {
            if (pathOverlay == null)
            {
                pathOverlay = new GraphicsOverlay();
                pathOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(pathOverlay);
            }

            showPath = true;
        }

        public void ShowOctreeBlockedNodes()
        {
            if (octreeOverlay == null)
            {
                octreeOverlay = new GraphicsOverlay();
                octreeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(octreeOverlay);
            }

            showOctreeBlockedNodes = true;
        }

        public void ShowOctreeNodes()
        {
            if (octreeOverlay == null)
            {
                octreeOverlay = new GraphicsOverlay();
                octreeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(octreeOverlay);
            }

            showOctree = true;
        }


        public void ShowGraph()
        {
            if (graphOverlay == null)
            {
                graphOverlay = new GraphicsOverlay();
                graphOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(graphOverlay);
            }

            showGraph = true;
        }

        public void ShowBoundingBox()
        {
            showBoundingBox = true;
        }

        public void DisplayBothBoundingBox()
        {
            DisplayOctreeBoundingBox();
            DisplayDimensionBoundingBox();
        }
        public void DisplayOctreeBoundingBox()
        {
            List<MapPoint> corners = new List<MapPoint>();
            Vector3 center = shipWorld.space.root.center;
            float r = shipWorld.space.size / 2;

            MapPoint centerPoint = new MapPoint(center.X * 10f, center.Y * 10f, center.Z * 10f, spatialRef);
            Graphic centerGraphic = new Graphic(centerPoint, centerPointSymbol);

            for (int i = 0; i < 4; i++)
            {
                int signY = i < 2 ? 1 : -1;
                int signX = (i == 1 || i == 2) ? -1 : 1;
                corners.Add(new MapPoint((center.X + (r * signX)) * 10f,
                                          (center.Y + (r * signY)) * 10f,
                                          center.Z * 10f, spatialRef));
            }
            corners.Add(corners[0]);

            ArcGisGeometry box = new Polyline(corners);
            Graphic boxGraphic = new Graphic(box, boxOutlineSymbol);

            if (octreeOverlay == null)
            {
                octreeOverlay = new GraphicsOverlay();
                octreeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(octreeOverlay);
            }

            octreeOverlay.Graphics.Add(boxGraphic);
            octreeOverlay.Graphics.Add(centerGraphic);
        }

        public void DisplayDimensionBoundingBox()
        {
            List<MapPoint> corners = new List<MapPoint>();
            Vector3 center = shipWorld.space.root.center;
            float origR = shipWorld.space.size / 2;
            float r = (fieldDimM / 10) / 2; //trav for user error
            if ((r - origR) > 0.001)
                throw new ArgumentOutOfRangeException("The dimensions of the field cannot be greater than the size of octree");

            MapPoint centerPoint = new MapPoint(center.X * 10f, center.Y * 10f, center.Z * 10f, spatialRef);
            Graphic centerGraphic = new Graphic(centerPoint, centerPointSymbol);

            for (int i = 0; i < 4; i++)
            {
                int signY = i < 2 ? 1 : -1;
                int signX = (i == 1 || i == 2) ? -1 : 1;
                corners.Add(new MapPoint((center.X + (r * signX)) * 10f,
                                          (center.Y + (r * signY)) * 10f,
                                          center.Z * 10f, spatialRef));
            }
            corners.Add(corners[0]);

            ArcGisGeometry box = new Polyline(corners);
            Graphic boxGraphic = new Graphic(box, boxOutlineSymbol);

            ArcGisGeometry geom = new Esri.ArcGISRuntime.Geometry.Polygon(corners);
            env = geom.Extent;
       
            if (octreeOverlay == null)
            {
                octreeOverlay = new GraphicsOverlay();
                octreeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(octreeOverlay);
            }

            octreeOverlay.Graphics.Add(boxGraphic);
            octreeOverlay.Graphics.Add(centerGraphic);

            //test
            //octreeOverlay.Graphics.Add(envGraphic);
        }

        public void ShowFootprint()
        {
            showFootprint = true;
        }

    }
}
