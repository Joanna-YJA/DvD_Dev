using System.Collections;
using System.IO;

namespace DvD_Dev
{
    class World
    {

        public Octree space;
        public Graph spaceGraph;

        //public World(GameObject scene, float size, Vector3 center, int maxLevel, float normalExtension, bool progressive = true, Graph.GraphType type = Graph.GraphType.CENTER)
        //{
        //    space = progressive ? new ProgressiveOctree(size, center - Vector3.One * size / 2, maxLevel) : new Octree(size, center - Vector3.One * size / 2, maxLevel);
        //    space.BuildFromGameObject(scene, normalExtension);
        //    spaceGraph =
        //        type == Graph.GraphType.CENTER ? space.ToCenterGraph() :
        //        type == Graph.GraphType.CORNER ? space.ToCornerGraph() : space.ToCrossedGraph();
        //}

        public World(Octree space, Graph.GraphType type = Graph.GraphType.CENTER)
        {
            this.space = space;
            spaceGraph =
                type == Graph.GraphType.CENTER ? space.ToCenterGraph() :
                type == Graph.GraphType.CORNER ? space.ToCornerGraph() : space.ToCrossedGraph();
        }

        public World(Octree space, Graph spaceGraph)
        {
            this.space = space;
            this.spaceGraph = spaceGraph;
        }

        //public void DisplayVoxels() {
        //    space.DisplayVoxels();
        //}
    }
}