using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using ArcGisGeometry = Esri.ArcGISRuntime.Geometry.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Labeling;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Streams;
using NetTopologySuite.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using AttributesTable = NetTopologySuite.Features.AttributesTable;
using Colors = System.Drawing.Color;
using Feature = NetTopologySuite.Features.Feature;
using Geometry = NetTopologySuite.Geometries.Geometry;
using Esri.ArcGISRuntime.Geometry;

namespace DvD_Dev
{
    class PathFinder
    {
        World shipWorld;
        Commanding command;
        SpaceUnit racingDrone;
        FootprintCalculator footprintCalc;
        public static SpatialReference spatialRef;

        SceneView sceneView;
        GraphicsOverlay pathOverlay, meshOverlay, octreeOverlay, graphOverlay;
        static ClassBreaksRenderer renderer;
        static LabelDefinition heightLabelDef;

        public static float defaultWaypointSize = 0.2f;
        public static float referencePointLat = 1.290167f;
        public static float referencePointLon = 103.8623f;

        int octreeLevel = 8; // the center point in space of the 3D cube to construct the octree from
        float shipSize = .5f; // this is used to calculate ext, which is the extension of buffer space around buildings to reduce collisions
        float dimensions = 200; // Should not be changed. Dimensions in Unity units of the volume to be constructed in octree

        Vector3 originalRefPos = new Vector3(57.49f, 1.5f, 142.57f);
        Vector3 centerOfMap = new Vector3(29590.69f, 31892.40f, 10);

        public List<Mesh> meshes = new List<Mesh>();
        List<MapPoint> points;

        bool isDisplayPath = false, isDisplayCoverage = false;

        static PathFinder()
        {
            InitializeRenderer();
            InitializeLabelDef();
        }

        private static void InitializeRenderer()
        {
            // Define the colors that will be used by the unique value renderer.
            Color gray = Color.FromArgb(255, 153, 153, 153);
            Color blue1 = Color.FromArgb(255, 227, 235, 207);
            Color blue2 = Color.FromArgb(255, 150, 194, 191);
            Color blue3 = Color.FromArgb(255, 97, 166, 181);
            Color blue4 = Color.FromArgb(255, 69, 125, 150);
            Color blue5 = Color.FromArgb(255, 41, 84, 120);

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
                new ClassBreak("Buildings with height below 40m", "0 - 40", 0, 40, simpleFileSymbol1),
                new ClassBreak("Buildings with height from 41m to 80m", "40 - 80", 40, 80, simpleFileSymbol2),
                new ClassBreak("Buildings with height from 81m to 120m", "81 - 120", 80, 120, simpleFileSymbol3),
                new ClassBreak("Buildings with height above 121m", ">120", 120, Int32.MaxValue, simpleFileSymbol4),
               // new ClassBreak("> 86,100 to 10,110,975", "> 86,100 to 10,110,975", 86100, 10110975, simpleFileSymbol5)
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
            footprintCalc = new FootprintCalculator();

            this.sceneView = sceneView;
            pathOverlay = new GraphicsOverlay();
            pathOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            sceneView.GraphicsOverlays.Add(pathOverlay);
        }
        public async Task InitPathFinder()
        {
            await ReadInput();
            LoadScene(meshes, centerOfMap, octreeLevel, shipSize);
            LinkShip();
        }

        public async Task ReadInput()
        {
            FolderPicker picker = new FolderPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add("*");

            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                StorageFile[] targetFiles = new StorageFile[4];
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

                await GenerateMeshes(targetFiles[0], targetFiles[1]);
            }
            else throw new FileNotFoundException("Folder not found.");
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

                        Mesh mesh = new Mesh(geometry, h, spatialRef);
                        meshes.Add(mesh);
                    }

                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public static Vector3 CoordToVect(Coordinate c)
        {
            return new Vector3(Convert.ToSingle(c.X), Convert.ToSingle(c.Y), Convert.ToSingle(c.Z));
        }

        // Construct an Octree from the loaded scene
        public void LoadScene(List<Mesh> meshes, Vector3 centerCoords, int octreeLevels, float shipSize)
        {
            racingDrone = new SpaceUnit();
            float ext = MathF.Max(shipSize - 16f / (1 << 8) * MathF.Sqrt(3) / 2, 0);
            racingDrone.ext = ext;

            Coordinate testCoord = new CoordinateZ(11557028.5197362 / 10, 150832.223844404 / 10, 0);
            Vector3 testCenter = new Vector3(Convert.ToSingle(testCoord.X), Convert.ToSingle(testCoord.Y), Convert.ToSingle(testCoord.Z));
            shipWorld = new World(ref sceneView, meshes, dimensions, testCenter, octreeLevels, 0.5f, true, Graph.GraphType.CORNER);

            command = new Commanding(ref sceneView, shipWorld);
        }

        public void TravelAndSearch(MapPoint[] sourceDest)
        {
            points = new List<MapPoint>();
            Vector3 localSrc = Vector3.Zero, localDest = Vector3.Zero;
            ConvertToLocal(sourceDest, ref localSrc, ref localDest);
            command.ConvertToAvailPoints(ref localSrc, ref localDest);
            System.Diagnostics.Debug.WriteLine("Final changed dest " + localDest);
            MoveToCoords(localSrc, localDest);
            SearchInSpiral(localDest);

            if (isDisplayPath) command.ShowPath(ref points, pathOverlay);
            if (isDisplayCoverage)
            {
                footprintCalc.ShowFootprintCoverage(ref points, pathOverlay);
                footprintCalc.ShowSeperateFootprint(ref points, pathOverlay);
            }
            WriteToKml();
        }

        public async void WriteToKml()
        {
            KmlDocument kmlDoc = new KmlDocument() { Name = "KML Drone Path" };
            foreach (MapPoint p in points)
            {
                if (p == null) continue;
                ArcGisGeometry geom = (ArcGisGeometry)p;
                ArcGisGeometry projectedGeom = GeometryEngine.Project(geom, SpatialReferences.Wgs84);
                KmlGeometry kmlGeom = new KmlGeometry(projectedGeom, KmlAltitudeMode.ClampToGround);
                KmlPlacemark placemark = new KmlPlacemark(kmlGeom);

                kmlDoc.ChildNodes.Add(placemark);
            }

            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
            savePicker.FileTypeChoices.Add("KMZ file", new List<string>() { ".kmz" });
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                try
                {
                    using (Stream stream = await file.OpenStreamForWriteAsync())
                    {
                        // Write the KML document to the stream of the file.
                        await kmlDoc.WriteToAsync(stream);
                    }
                    System.Diagnostics.Debug.WriteLine("Item saved.");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("File not saved.");
                }
            }
        }

        public void ConvertToLocal(MapPoint[] sourceDest, ref Vector3 localSrc, ref Vector3 localDest)
        {
            float srcX = Convert.ToSingle(sourceDest[0].X) / 10,
                 srcY = Convert.ToSingle(sourceDest[0].Y) / 10,
                 //srcZ = Convert.ToSingle(sourceDest[0].Z) / 10;
                 srcZ = 25f / 10;
            localSrc = new Vector3(srcX, srcY, srcZ);

            float destX = Convert.ToSingle(sourceDest[1].X) / 10,
                destY = Convert.ToSingle(sourceDest[1].Y) / 10,
                 // destZ = Convert.ToSingle(sourceDest[1].Z) / 10;
                 destZ = 25f / 10;
            localDest = new Vector3(destX, destY, destZ);
        }

        // Read data from the input fields and move to provided coords
        public void MoveToCoords(Vector3 localSrc, Vector3 localDest)
        {
            racingDrone.position = racingDrone.standPoint = localSrc;
            command.MoveOrder(localDest, ref points);
        }

        public void SearchInSpiral(Vector3 localDest)
        {
            command.SearchInSpiral(ref points, localDest);
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

        public void DisplayMesh()
        {
            if (meshOverlay == null)
            {
                meshOverlay = new GraphicsOverlay();
                meshOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(meshOverlay);
            }

            foreach (Mesh mesh in meshes)
                mesh.DisplayMesh(ref meshOverlay);
        }

        public void DisplayMeshNormals()
        {
            if (meshOverlay == null)
            {
                meshOverlay = new GraphicsOverlay();
                meshOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(meshOverlay);
            }

            foreach (Mesh mesh in meshes)
                mesh.DisplayMeshNormals(ref meshOverlay);
        }

        public void DisplayOctreeBlockedNodes()
        {
            if (octreeOverlay == null)
            {
                octreeOverlay = new GraphicsOverlay();
                octreeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(octreeOverlay);
            }

            shipWorld.space.DisplayOctreeBlockedNodes(ref octreeOverlay);
        }

        public void DisplayOctreeNodes()
        {
            if (octreeOverlay == null)
            {
                octreeOverlay = new GraphicsOverlay();
                octreeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(octreeOverlay);
            }

            shipWorld.space.DisplayOctreeNodes(ref octreeOverlay);
        }


        public void DisplayGraphNodes()
        {
            if (graphOverlay == null)
            {
                graphOverlay = new GraphicsOverlay();
                graphOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                sceneView.GraphicsOverlays.Add(graphOverlay);
            }

            shipWorld.spaceGraph.DisplayGraphNodes(ref graphOverlay, shipWorld.space);
        }

        public void DisplayPath()
        {
            isDisplayPath = true;
        }

        public void DisplayFootprintCoverage()
        {
            isDisplayCoverage = true;
        }

    }
}
