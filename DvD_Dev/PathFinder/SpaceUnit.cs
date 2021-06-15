using System.Collections;
using System.Collections.Generic;
using System.Numerics;

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

        // Move it to the next position
        private void MovementUpdate()
        {
           // position = transform.position;

            // Find next waypoint
            Vector3 next = Vector3.Zero;
            Vector3 nextSpot = Vector3.Zero;
            if (wayPoints != null && wayPoints.Count > 0)
            {
                next = wayPoints.Peek();
                nextSpot = next + ((lastWayPoint == Vector3.Zero || wayPoints.Count == 1) ? Vector3.Zero : Vector3.Normalize(next - lastWayPoint) * wayPointRange);

                while (wayPoints.Count > 0 && (nextSpot - position).LengthSquared() < wayPointRange * wayPointRange)
                {
                    lastWayPoint = wayPoints.Dequeue();
                    if (wayPoints.Count > 0)
                    {
                        next = wayPoints.Peek();
                        nextSpot = next + ((lastWayPoint == Vector3.Zero || wayPoints.Count == 1) ? Vector3.Zero : Vector3.Normalize(next - lastWayPoint) * wayPointRange);
                    }
                    else if (state == UnitState.MOVING)
                    {
                        state = UnitState.IDLE;
                        standPoint = lastWayPoint;
                    }
                }
            }
            if (wayPoints == null || wayPoints.Count == 0)
            {
                lastWayPoint = Vector3.Zero;
                velocity = Vector3.Zero;
               // PathfindingModeOff();

            }


            // Check line of sight to the nextnext way point
            // If LOS is found, we can skip the immediate next waypoint
            pathFindingRecheckTimer -= U.limitedDeltaTime;
            if (pathFindingRecheckTimer <= 0)
            {
                bool jumped = false;
                if (wayPoints != null && wayPoints.Count > 1)
                {
                    Queue<Vector3>.Enumerator e = wayPoints.GetEnumerator();
                    e.MoveNext(); e.MoveNext();
                    Vector3 nextnext = e.Current;
                    //if (LineOfSightRaycast(position, nextnext))
                    //{
                    //    lastWayPoint = wayPoints.Dequeue();
                    //    next = wayPoints.Peek();
                    //    nextSpot = next + (wayPoints.Count == 1 ? Vector3.Zero : Vector3.Normalize(next - lastWayPoint) * wayPointRange);
                    //    jumped = true;
                    //    velocity = Vector3.Zero;
                    //}
                }

                //if (!jumped && wayPoints != null && wayPoints.Count > 0 && !LineOfSightRaycast(position, next))
                //{
                //    List<Node> tempPath = spaceGraph.FindPath(spaceGraph.LazyThetaStar, position, next, space);
                //    if (tempPath != null)
                //    {
                //        Queue<Vector3> newPath = new Queue<Vector3>();
                //        foreach (Node node in tempPath)
                //        {
                //            newPath.Enqueue(node.center);
                //        }
                //        wayPoints.Dequeue();
                //        while (wayPoints.Count > 0) newPath.Enqueue(wayPoints.Dequeue());
                //        wayPoints = newPath;
                //    }
                //}
                pathFindingRecheckTimer += pathFindingRecheckInterval;
            }

            // Decelerate to zero velocity when it reaches waypoint
            // Based on the current velocity, calculate how much distance it would take for spaceship to decelerate to zero velocity
            float stoppingDistance = (float)System.Math.Pow(velocity.Length(), 2) / (2 * acceleration);

            // If it is less than or equal a multiple of stopping distance, start decelerating
            bool decelerating = false;
            if ((nextSpot - position).Length() <= stoppingDistance * 2f)
            {
                decelerating = true;
            }

            Vector3 targetVelocity = Vector3.Zero;

            // If it is very near the waypoint, lower the speed to 1m/s
            // This should not be necessary if deceleration works, but keep this as fallback
            if ((nextSpot - position).Length() < 1f)
            {
                velocity = Vector3.Normalize(nextSpot - position) * 1f;
            }
            // Gradually decelerate to 1m/s as it approaches waypoint
            else if (decelerating)
            {
                if (wayPoints != null && wayPoints.Count > 0)
                {
                    targetVelocity = Vector3.Normalize(nextSpot - position) * 1f;
                }
                if ((targetVelocity - velocity).LengthSquared() < U.Sq(acceleration * U.limitedDeltaTime))
                {
                    velocity = targetVelocity;
                }
                else
                {

                    velocity += Vector3.Normalize(targetVelocity - velocity) * acceleration * U.limitedDeltaTime;
                }
            }
            else
            {
                // Accelerate to max velocity

                if (wayPoints != null && wayPoints.Count > 0)
                {
                    targetVelocity = Vector3.Normalize(nextSpot - position) * maxVelocity;
                }
                if ((targetVelocity - velocity).LengthSquared() < U.Sq(acceleration * U.limitedDeltaTime))
                {
                    velocity = targetVelocity;
                }
                else
                {
                    velocity += Vector3.Normalize(targetVelocity - velocity) * acceleration * U.limitedDeltaTime;
                }
            }

            // Repulsive force from SpaceUnits
            //Collider[] touch = Physics.OverlapSphere(position, radius + repulsiveRadius);
            //foreach (Collider col in touch)
            //{
            //    SpaceUnit ship = col.GetComponent<SpaceUnit>();
            //    if (ship != null)
            //    {
            //        float d = (ship.position - position).Length() - radius - ship.radius;
            //        Vector3 acc = (position - ship.position).normalized * repulsiveCoeff * MathF.Pow(1 - MathF.Clamp01(d / repulsiveRadius), repulsivePow);
            //        //Vector3 acc = (position - ship.position).normalized * repulsiveCoeff / (d + radius) / (d + radius);
            //        velocity += acc * U.limitedDeltaTime;
            //    }
            //}

            //// Repulsive force from walls
            //for (int i = 0; i < 16; i++)
            //{
            //    Ray ray = new Ray(position, Random.onUnitSphere);
            //    RaycastHit res;
            //    if (Physics.Raycast(ray, out res, radius + repulsiveRadius) && res.collider.GetComponent<SpaceUnit>() == null)
            //    {
            //        float d = (res.point - position).Length() - radius;
            //        Vector3 acc = (position - res.point).normalized * repulsiveCoeff * MathF.Pow(1 - MathF.Clamp01(d / repulsiveRadius), repulsivePow);
            //        velocity += acc * U.limitedDeltaTime / 16 * 8;
            //    }
            //}

            //position += velocity * U.limitedDeltaTime;

            //// Update position
            //transform.position = position;

            //if (targetVelocity.LengthSquared() > 0.0001f)
            //{
            //    transform.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(transform.forward, targetVelocity), U.limitedDeltaTime * 5) * transform.rotation;
            //}
            //Rigidbody body = GetComponent<Rigidbody>();
            //body.velocity = Vector3.Zero;
            //body.angularVelocity = Vector3.Zero;
            //if (Settings.showShipTrajectory)
            //{
            //    DrawTrajectory();
            //}
            //else if (line != null)
            //{
            //    ClearTrajectory();
            //}


            // Level the SpaceUnit with the horizon at all times
            // Except when it is facing close to straight up to prevent spinning phenomenom
            //Vector3 eulerRotation = transform.rotation.eulerAngles;
            //if (Vector3.Angle(gameObject.transform.forward, Vector3.up) > 25) { transform.rotation = Quaternion.Euler(eulerRotation.X, eulerRotation.Y, 0); }

        }

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
            foreach (Node node in wp)
            {
                wayPoints.Enqueue(node.center);
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