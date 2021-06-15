using System.Collections;
using System.Collections.Generic;
using System;
using System.Numerics;

namespace DvD_Dev
{
    class Commanding
    {

        public List<SpaceUnit> activeUnits;
        public Octree space;
        public Graph spaceGraph;

        float losCheckInterval = 0.001f;
        [System.NonSerialized]
        public float ext;

        public Commanding(World world)
        {
            activeUnits = new List<SpaceUnit>();
            this.space = world.space;
            this.spaceGraph = world.spaceGraph;
        }
        public Commanding(Octree space, Graph spaceGraph)
        {
            activeUnits = new List<SpaceUnit>();
            this.space = space;
            this.spaceGraph = spaceGraph;
        }

        public void MoveOrder(Vector3 target)
        {
            List<Vector3> pathFindingDest = new List<Vector3>();
            foreach (SpaceUnit unit in activeUnits)
            {
                //pathFindingDest.Add(unit.position);
            }
            if (pathFindingDest.Count > 0)
            {
                List<List<Node>> allWayPoints = spaceGraph.FindPath(spaceGraph.LazyThetaStar, target, pathFindingDest, space);
                //List<List<Vector3>> allWayPointsAfterLOSCheck = new List<List<Vector3>>();
                if (allWayPoints[0] != null)
                {
                    foreach (List<Node> waypointList in allWayPoints)
                    {
                        foreach (Node node in waypointList)
                        {
                            // convert the node to the proper real world scale location
                            // since this calculated node center is in the 0.1 scale
                            node.center *= 10;
                        }

                        /* This is the attempt to calculate the new LOS-checked waypoints are start
                         * 
                        // Do the LOS from each node to the nextnext node
                        List<Vector3> waypointListAfterLOSCheck = new List<Vector3>();
                        if (waypointList.Count > 2)
                        {
                            //Vector3 currPos = activeUnits[0].gameObject.transform.position;
                            Vector3 currPos = waypointList[0].center;
                            waypointListAfterLOSCheck.Add(currPos);
                            for (int i = 0; i < waypointList.Count - 2; i++)
                            {
                                float distFromCurrPos = 0;
                                Vector3 next = waypointList[i + 1].center;
                                Vector3 dirToNext = Vector3.Normalize(next - currPos);
                                float distToNext = (next - currPos).Length();

                                Vector3 nextnext = waypointList[i + 2].center;
                                while (distFromCurrPos < distToNext)
                                {
                                    if (LineOfSightRaycast(currPos + distFromCurrPos * dirToNext, nextnext))
                                    {
                                        currPos = currPos + distFromCurrPos * dirToNext;
                                        waypointListAfterLOSCheck.Add(currPos);
                                        break;
                                    }
                                    distFromCurrPos += losCheckInterval;
                                }
                                waypointListAfterLOSCheck.Add(next);
                                currPos = next;
                            }
                            // Add in position of the final two nodes
                            waypointListAfterLOSCheck.Add(waypointList[waypointList.Count - 2].center);
                            waypointListAfterLOSCheck.Add(waypointList[waypointList.Count - 1].center);
                        }
                        else
                        {
                            for (int i= 0; i < waypointList.Count; i++)
                            {
                                waypointListAfterLOSCheck.Add(waypointList[i].center);
                            }
                        }
                        allWayPointsAfterLOSCheck.Add(waypointListAfterLOSCheck);
                        */
                    }

                }


                for (int i = 0; i < activeUnits.Count; i++)
                {
                    activeUnits[i].MoveOrder(U.InverseList(allWayPoints[i]), PathFinder.defaultWaypointSize * MathF.Pow(activeUnits.Count, 0.333f));
                    //activeUnits[i].MoveOrder(U.InverseList(allWayPointsAfterLOSCheck[i]), Main.defaultWaypointSize * MathF.Pow(activeUnits.Count, 0.333f));
                }
            }
        }

        /*
         * For the LOS check attempt above
        // Check LOS (if does not collide with collider) from p1 to p2
        // Return true if has LOS
        public bool LineOfSightRaycast(Vector3 p1, Vector3 p2)
        {
            Vector3 dir = p2 - p1;
            Ray ray = new Ray(p1, dir);
            // raycast first then spherecast
            // because SphereCast will not detect colliders for which the sphere overlaps the collider.
            // from Docs
            if (Physics.Raycast(ray, dir.Length()))
            {
                return false;
            }
            else
            {
                if (Physics.SphereCast(ray, ext, dir.Length()))
                {
                    return false;
                }
                else { return true; }

            }

        }
        */
    }
}