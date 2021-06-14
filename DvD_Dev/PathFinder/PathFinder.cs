using UnityEngine.GameObject;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
//using Newtonsoft.Json.UnityConverters;
//using Newtonsoft.Json.UnityConverters.Math;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace DvD_Dev
{
    class PathFinder
    {

        public GameObject[] scenes;
        public static float defaultWaypointSize = 0.2f;

        public GameObject coordInputFieldX;
        public GameObject coordInputFieldY;
        public GameObject coordInputFieldZ;
        public GameObject submitCoordsButton;

        public GameObject referencePoint;
        public float referencePointLat;
        public float referencePointLon;

        public GameObject racingDrone;

        World shipWorld;

        Commanding command;
        //BoundingBox targetDroneBB;

        [SerializeField]
        Vector3 originalRefPos;
        [SerializeField]
        Vector3 centerOfMap = new Vector3(-18, -30, 82);
        int octreeLevel = 8; // the center point in space of the 3D cube to construct the octree from
        float shipSize = .5f; // this is used to calculate ext, which is the extension of buffer space around buildings to reduce collisions
        int dimensions = 200; // in Unity units of the volume to be constructed in octree


        public async void InitPathFinder()
        {
            //targetDroneBB = racingDrone.GetComponent<BoundingBox>();
            LoadScene(0, centerOfMap, octreeLevel, shipSize, dimensions);
            //LinkShip();
            racingDrone.GetComponent<SpaceUnit>().enabled = true;
            //submitCoordsButton.GetComponent<Button>().onClick.AddListener(() => MoveToCoords());
            // MoveToCoords();
            //test
            Graph spaceGraph = null;
            System.Diagnostics.Debug.WriteLine("Starting to deserialize spaceGraph...");
            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add(".json");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using (var inputStream = await file.OpenReadAsync())
                using (var classicStream = inputStream.AsStreamForRead())
                using (var streamReader = new StreamReader(classicStream))
                {
                    JsonSerializer serializer = JsonSerializer.CreateDefault();
                    spaceGraph = (Graph)serializer.Deserialize(streamReader, typeof(Graph));
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Could not locate .json file to deserialize");
            }

            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageFile newFile = await folder.CreateFileAsync("ReserializedSpaceGraph.json", CreationCollisionOption.ReplaceExisting);

                using (var inputStream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
                using (var classicStream = inputStream.AsStreamForWrite())
                using (var streamWriter = new StreamWriter(classicStream))
                {
                    JsonSerializer serializer = JsonSerializer.CreateDefault();
                    serializer.Serialize(streamWriter, spaceGraph);

                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Could not locate folder to store serialized .json file");
            }



            System.Diagnostics.Debug.WriteLine("Finished deserializing spaceGraph!");
        }

        // Construct an Octree from the loaded scene
        public void LoadScene(int sceneIndex, Vector3 centerCoords, int octreeLevels, float shipSize, int dimensions)
        {
            float ext = Mathf.Max(shipSize - 16f / (1 << 8) * Mathf.Sqrt(3) / 2, 0);
            racingDrone.GetComponent<SpaceUnit>().ext = ext;

           // shipWorld = new World(scenes[sceneIndex], dimensions, centerCoords, octreeLevels, ext, true, Graph.GraphType.CORNER);

            command = new Commanding(shipWorld);
            //command.ext = ext;
            //Settings.showShipTrajectory = true;

            // scale world by x10
            originalRefPos = referencePoint.transform.position;
            scenes[sceneIndex].transform.parent.localScale = new Vector3(10, 10, 10);

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
            float z_dist = (z - referencePoint.transform.position.z) * 10;
            float x_dist = (x - referencePoint.transform.position.x) * 10;
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
        //    local.Add(originalRefPos.x + lon_dist * 30.6f / 0.00027778f / 10);
        //    local.Add(originalRefPos.z + lat_dist * 30.6f / 0.00027778f / 10);
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
