using System.Collections.Generic;
using System.Numerics;
using Windows.Devices.Geolocation;

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
    }
}