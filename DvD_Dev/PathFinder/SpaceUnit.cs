using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Windows.Devices.Geolocation;
using Windows.UI;

namespace DvD_Dev
{
    class SpaceUnit
    {

        public Octree space;
        public Graph spaceGraph;


        public float radius = 0.1f;

        public bool movable = true;
        public float maxVelocity = 15f; // Bebop max speed is 15m/s
        public float acceleration = 3f;
        public float defaultWayPointRange = 0.2f;
        public float pathFindingRecheckInterval = 1f;
        float repulsiveRadius = 0.1f;
        float repulsiveCoeff = 5f;
        float repulsivePow = 1.5f;
        [System.NonSerialized]
        public float ext;

        public enum UnitState
        {
            IDLE,
            MOVING,
            ATTACKING
        }
        public UnitState state = UnitState.IDLE;

        public Vector3 velocity;
        public Vector3 position;
        public Vector3 standPoint;
        Queue<Vector3> wayPoints;
        float wayPointRange;
        Vector3 lastWayPoint;

        public bool landed = true;

        private float pathFindingRecheckTimer = 1f;

        //BoundingBox targetDroneBB;

        void Start()
        {
           // position = transform.position;
          // standPoint = position;
         //   targetDroneBB = gameObject.GetComponent<BoundingBox>();
        }

        //void Update()
        //{

        //    if (landed)
        //    {
        //        // Correct components to be activated if drone is landed
        //        gameObject.GetComponent<AeroplanePropellerAnimator>().enabled = false;
        //        gameObject.GetComponent<StabilisedAIController>().enabled = true;
        //        gameObject.GetComponent<Rigidbody>().useGravity = true;
        //        gameObject.GetComponent<SpaceUnit>().enabled = false;
        //    }
        //    else if (movable)
        //    {
        //        // Correct components to be activated if drone is moving
        //        gameObject.GetComponent<AeroplanePropellerAnimator>().enabled = true;
        //        MovementUpdate();
        //    }

        //    // If BB found, stop pathfinding and let AI take over
        //    if (targetDroneBB.DrawBoundingBox(out Vector2 _))
        //    {
        //        PathfindingModeOff();
        //        ClearTrajectory();
        //    }
        //}

        // Turn off pathfinding to allow AI to take over
        //private void PathfindingModeOff()
        //{
        //    gameObject.GetComponent<StabilisedAIController>().enabled = true;
        //    gameObject.GetComponent<StabilisedAIController>().resetYawRef();
        //    gameObject.GetComponent<Rigidbody>().useGravity = true;
        //    gameObject.GetComponent<SpaceUnit>().enabled = false;
        //    targetDroneBB.maxVisibleDistance += 5f;
        //}

        //private LineRenderer line = null;
        //private LineRenderer thickLine = null;


        // Draw two lines, one thinner one for FPV view, and one thicker one for main camera
        //public void DrawTrajectory()
        //{
        //    if (line == null)
        //    {
        //        GameObject trajectoryGO = new GameObject("Trajectory");
        //        trajectoryGO.layer = 6; // This layer is called trajectory
        //        line = trajectoryGO.AddComponent<LineRenderer>();

        //        line.startWidth = 0.01f;
        //        line.endWidth = 0.01f;
        //        line.material = GameObject.Find("LineMaterial").GetComponent<MeshRenderer>().sharedMaterial;

        //        GameObject thickTrajectoryGO = new GameObject("ThickTrajectory");
        //        thickTrajectoryGO.layer = 7; // This layer is called trajectory
        //        thickLine = thickTrajectoryGO.AddComponent<LineRenderer>();

        //        thickLine.startWidth = 1f;
        //        thickLine.endWidth = 1f;
        //        thickLine.material = GameObject.Find("LineMaterial").GetComponent<MeshRenderer>().sharedMaterial;
        //    }
        //    if (wayPoints != null && wayPoints.Count > 0)
        //    {
        //        line.positionCount = wayPoints.Count + 1;
        //        line.SetPosition(0, position);
        //        int t = 1;
        //        foreach (Vector3 pos in wayPoints)
        //        {
        //            line.SetPosition(t++, pos);
        //        }

        //        thickLine.positionCount = wayPoints.Count + 1;
        //        thickLine.SetPosition(0, position);
        //        t = 1;
        //        foreach (Vector3 pos in wayPoints)
        //        {
        //            thickLine.SetPosition(t++, pos);
        //        }
        //    }
        //    else
        //    {
        //        line.positionCount = 0;
        //        thickLine.positionCount = 0;
        //    }
        //}

        // Delete the lines
        //public void ClearTrajectory()
        //{
        //    if (line != null)
        //    {
        //        Destroy(line.gameObject);
        //        Destroy(thickLine.gameObject);
        //    }
        //}

        public void MoveOrder(Vector3 targetPoint, float range)
        {
            MoveOrder(spaceGraph.FindPath(spaceGraph.LazyThetaStar, position, targetPoint, space), range);
        }

        public void MoveOrder(List<Node> wp, float range)
        {
            if (wp == null || !movable) return;
            wayPoints = new Queue<Vector3>();
            List<BasicGeoposition> posList = new List<BasicGeoposition>();
            foreach (Node node in wp)
            {
                wayPoints.Enqueue(node.center);
                float x = PathFinder.ConvertLocalToLatLon(node.center.X, node.center.Z)[0];
                float z = PathFinder.ConvertLocalToLatLon(node.center.X, node.center.Z)[1];
                posList.Add(new BasicGeoposition { Latitude = x, Longitude = z, Altitude = node.center.Y });
            }
            wayPointRange = range;
            state = UnitState.MOVING;
        }

        public void MoveOrder(List<Vector3> wp, float range)
        { 
            if (wp == null || !movable) return;
            wayPoints = new Queue<Vector3>();
            foreach (Vector3 vec in wp)
            {
                wayPoints.Enqueue(vec);
            }
            wayPointRange = range;
            state = UnitState.MOVING;
        }

        // Check LOS (if collides with collider) from p1 to p2
        //public bool LineOfSightRaycast(Vector3 p1, Vector3 p2)
        //{
        //    Vector3 dir = p2 - p1;
        //    Ray ray = new Ray(p1, dir);
        //    // raycast first then spherecast
        //    // because SphereCast will not detect colliders for which the sphere overlaps the collider.
        //    // from Docs
        //    if (Physics.Raycast(ray, dir.Length()))
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        if (Physics.SphereCast(ray, ext, dir.Length()))
        //        {
        //            return false;
        //        }
        //        else { return true; }
        //    }

        //}
    }
}