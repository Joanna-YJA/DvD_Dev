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
        public float referencePointLat;
        public float referencePointLon;

       public SpaceUnit racingDrone;

        World shipWorld;

        Commanding command;
        //BoundingBox targetDroneBB;

        Vector3 originalRefPos;
        Vector3 centerOfMap = new Vector3(-18, -30, 82);
        int octreeLevel = 8; // the center point in space of the 3D cube to construct the octree from
        float shipSize = .5f; // this is used to calculate ext, which is the extension of buffer space around buildings to reduce collisions
        int dimensions = 200; // in Unity units of the volume to be constructed in octree


        public async void InitPathFinder()
        {
            //targetDroneBB = racingDrone.GetComponent<BoundingBox>();
            //LinkShip();
            // racingDrone.GetComponent<SpaceUnit>().enabled = true;
            //submitCoordsButton.GetComponent<Button>().onClick.AddListener(() => MoveToCoords());
            // MoveToCoords();
            //test
            //Get the Window's HWND
            //var hwnd = this.As<IWindowNative>().WindowHandle;
             await DeserializeWorld();
            //LoadScene(0, centerOfMap, octreeLevel, shipSize, dimensions);
        }

        public async Task DeserializeWorld()
        {
            Octree space = null;
            Graph spaceGraph = null;
            System.Diagnostics.Debug.WriteLine("Starting to deserialize World properties...");
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

            StorageFile file2 = await picker.PickSingleFileAsync();
            if (file2 != null)
            {
                using (var inputStream = await file2.OpenReadAsync())
                using (var classicStream = inputStream.AsStreamForRead())
                using (var streamReader = new StreamReader(classicStream))
                { 
                    spaceGraph = (Graph)serializer.Deserialize(streamReader, typeof(Graph));
                }
            }
            else
            {
                throw new FileNotFoundException("Could not locate .json file of Octree to deserialize");
            }
            System.Diagnostics.Debug.WriteLine("FINISHED deserializing world properties!");

            shipWorld = new World(space, spaceGraph);

            //test
            await SerializeWorld();
        }

        public async Task SerializeWorld()
        {
            System.Diagnostics.Debug.WriteLine("Starting to serialize spaceGraph...");
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            System.Diagnostics.Debug.WriteLine("Done picking folder...");


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
                    System.Diagnostics.Debug.WriteLine("shipWorld.space.root.tree: " + shipWorld.space.root.tree);
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
                System.Diagnostics.Debug.WriteLine("Could not locate folder to store serialized .json file");
            }



            System.Diagnostics.Debug.WriteLine("FINISHED reserializing spaceGraph!");
        }
        // Construct an Octree from the loaded scene
        public void LoadScene(int sceneIndex, Vector3 centerCoords, int octreeLevels, float shipSize, int dimensions)
        {
            float ext = MathF.Max(shipSize - 16f / (1 << 8) * MathF.Sqrt(3) / 2, 0);
           // racingDrone.GetComponent<SpaceUnit>().ext = ext;

           // shipWorld = new World(scenes[sceneIndex], dimensions, centerCoords, octreeLevels, ext, true, Graph.GraphType.CORNER);

            command = new Commanding(shipWorld);
            //command.ext = ext;
            //Settings.showShipTrajectory = true;

            // scale world by x10
            //originalRefPos = referencePoint.transform.position;
            //scenes[sceneIndex].transform.parent.localScale = new Vector3(10, 10, 10);

        }


        //public void PathfindingModeOn()
        //{
        //    racingDrone.GetComponent<SpaceUnit>().enabled = true;
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
        //public void MoveToCoords()
        //{
        //    InputField inputFieldX = coordInputFieldX.GetComponent<InputField>();
        //    InputField inputFieldY = coordInputFieldY.GetComponent<InputField>();
        //    InputField inputFieldZ = coordInputFieldZ.GetComponent<InputField>();
        //    float lat, lon, height;
        //    if (inputFieldX.text == "") lat = 1.28258f;
        //    else lat = float.Parse(inputFieldX.text);
        //    if (inputFieldY.text == "") lon = 103.85140f;
        //    else lon = float.Parse(inputFieldY.text);
        //    if (inputFieldZ.text == "") height = 10;
        //    else height = float.Parse(inputFieldZ.text) / 10;

        //    float x = ConvertLatLonToLocal(lon, lat)[0];
        //    float z = ConvertLatLonToLocal(lon, lat)[1];
        //    racingDrone.GetComponent<SpaceUnit>().landed = false;
        //    //racingDrone.GetComponent<SpaceUnit>().position = racingDrone.transform.position;
        //    //racingDrone.GetComponent<SpaceUnit>().standPoint = racingDrone.transform.position;
        //    PathfindingModeOn();
        //    command.MoveOrder(new Vector3(x, height, z));
        //}
        /*
         * Has to be corrected if to be used, is wrong now
        public List<float> ConvertLocalToLatLon(float x, float z)
        {
            // lat is Z coordinates
            //float baseOfFlyerLat = 1.28936f;
            //float baseOfFlyerLon = 103.86317f;
            List<float> latLon = new List<float>();
            float z_dist = (z - referencePoint.transform.position.Z) * 10;
            float x_dist = (x - referencePoint.transform.position.X) * 10;
            latLon.Add(referencePointLat + z_dist / 30.6f * 0.00027778f);
            latLon.Add(referencePointLon + x_dist / 30.6f * 0.00027778f);
            return latLon;
        }
        */
        //public List<float> ConvertLatLonToLocal(float lon, float lat)
        //{
        //    List<float> local = new List<float>();
        //    float lon_dist = lon - referencePointLon;
        //    float lat_dist = lat - referencePointLat;
        //    local.Add(originalRefPos.X + lon_dist * 30.6f / 0.00027778f / 10);
        //    local.Add(originalRefPos.Z + lat_dist * 30.6f / 0.00027778f / 10);
        //    return local;
        //}

        // Allow the ship to be commanded
        // If multiple ships are added to command, they can all pathfind to the same location together
        //public void LinkShip()
        //{
        //    SpaceUnit newShip = racingDrone.GetComponent<SpaceUnit>();
        //    newShip.space = shipWorld.space;
        //    newShip.spaceGraph = shipWorld.spaceGraph;
        //    command.activeUnits.Add(newShip);
        //}
    }
}
