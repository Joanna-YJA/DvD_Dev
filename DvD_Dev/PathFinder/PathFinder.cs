using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
//using Newtonsoft.Json.UnityConverters;
//using Newtonsoft.Json.UnityConverters.Math;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Numerics;
using System.Threading.Tasks;

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

        World shipWorld;
        MapController mapController;

        Commanding command;
        //BoundingBox targetDroneBB;

        Vector3 originalRefPos = new Vector3(57.49f, 1.5f, 142.57f);
        Vector3 centerOfMap = new Vector3(-18, -30, 82);
        int octreeLevel = 8; // the center point in space of the 3D cube to construct the octree from
        float shipSize = .5f; // this is used to calculate ext, which is the extension of buffer space around buildings to reduce collisions
        int dimensions = 200; // in Unity units of the volume to be constructed in octree


        public async void InitPathFinder(MapController mapController)
        {
            //targetDroneBB = racingDrone.GetComponent<BoundingBox>();
            //submitCoordsButton.GetComponent<Button>().onClick.AddListener(() => MoveToCoords());
           this.mapController = mapController;
            await DeserializeWorld();
            LoadScene();
            LinkShip();
            MoveToCoords();
            //LoadScene(0, centerOfMap, octreeLevel, shipSize, dimensions);
        }

        public async Task DeserializeWorld()
        {
            Octree space = null;
            Graph spaceGraph = null;
            List<Arc> arcList = null;
            ////System.Diagnostics.Debug.WriteLine("Starting to deserialize World properties...");
            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add(".json");

            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ObjectCreationHandling = ObjectCreationHandling.Auto
            });

            StorageFile file1 = await picker.PickSingleFileAsync();
            if (file1 != null)
            {
                using (var inputStream = await file1.OpenReadAsync())
                using (var classicStream = inputStream.AsStreamForRead())
                using (var streamReader = new StreamReader(classicStream))
                {
                   space = (Octree)serializer.Deserialize(streamReader, typeof(Octree));
                }
            }
            else
            {
                throw new FileNotFoundException("Could not locate .json file of Octree to deserialize");
            }

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
            shipWorld = new World(space, Graph.GraphType.CORNER);
            //test
            //await SerializeWorld();
        }

        public async Task SerializeWorld()
        {
            //System.Diagnostics.Debug.WriteLine("Starting to serialize spaceGraph...");
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            //System.Diagnostics.Debug.WriteLine("Done picking folder...");


            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings {
                ContractResolver = CustomVector3ContractResolver.Instance,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            }
            );

            if (folder != null)
            {
                StorageFile octreeFile = await folder.CreateFileAsync("ReserializedSpaceOctree.json", CreationCollisionOption.ReplaceExisting);

                using (var inputStream = await octreeFile.OpenAsync(FileAccessMode.ReadWrite))
                using (var classicStream = inputStream.AsStreamForWrite())
                using (var streamWriter = new StreamWriter(classicStream))
                {
                    //System.Diagnostics.Debug.WriteLine("shipWorld.space.root.tree: " + shipWorld.space.root.tree);
                    serializer.Serialize(streamWriter, shipWorld.space);

                }

                StorageFile graphFile = await folder.CreateFileAsync("ReserializedSpaceGraph.json", CreationCollisionOption.ReplaceExisting);

                using (var inputStream = await graphFile.OpenAsync(FileAccessMode.ReadWrite))
                using (var classicStream = inputStream.AsStreamForWrite())
                using (var streamWriter = new StreamWriter(classicStream))
                {
                    serializer.Serialize(streamWriter, shipWorld.spaceGraph);

                }
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("Could not locate folder to store serialized .json file");
            }



            //System.Diagnostics.Debug.WriteLine("FINISHED reserializing spaceGraph!");
        }
        // Construct an Octree from the loaded scene
        public void LoadScene()
        {
            racingDrone = new SpaceUnit();
            float ext = MathF.Max(shipSize - 16f / (1 << 8) * MathF.Sqrt(3) / 2, 0);
            racingDrone.ext = ext;

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
        public void MoveToCoords()
        { 
            float lat, lon, height;
            lat = 1.2902f;
            lon = 103.8519f;
            height = 10;

            float x = ConvertLatLonToLocal(lon, lat)[0];
            float z = ConvertLatLonToLocal(lon, lat)[1];

            //Set initial location of drone
            //racingDrone.landed = false;
            //float latp = 1.290270f, lonp = 103.851959f;
            //float xp = ConvertLatLonToLocal(lonp, latp)[0];
            //float zp = ConvertLatLonToLocal(lonp, latp)[1];
            racingDrone.position = new Vector3(-90.36761f, 1.41f, 31.52639f);
            racingDrone.standPoint = new Vector3(-90.36761f, 1.41f, 31.52639f); //Hover position
            //PathfindingModeOn();
            System.Diagnostics.Debug.WriteLine("Target position: " + new Vector3(x, height, z));
            foreach (Node n in shipWorld.spaceGraph.nodes)
            {
                if (n.center.Equals(new Vector3(-58.625f, 10.625f, 141.375f)))
                {
                    foreach (Arc a in n.arcs)
                        System.Diagnostics.Debug.WriteLine(n.index + ")Before move to order, target node n is connected to " + a.to.center);
                }
            }
            command.MoveOrder(new Vector3(x, height, z), mapController);
        }
        // Has to be corrected if to be used, is wrong now
        public static List<float> ConvertLocalToLatLon(float x, float z)
        {
            // lat is Z coordinates
            //float baseOfFlyerLat = 1.28936f;
            //float baseOfFlyerLon = 103.86317f;
            List<float> latLon = new List<float>();
            //float z_dist = (z - referencePoint.transform.position.Z) * 10;
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
    }
}
