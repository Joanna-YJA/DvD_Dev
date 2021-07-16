using Esri.ArcGISRuntime.UI.Controls;
using g3;
using NetTopologySuite.Geometries;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace DvD_Dev
{
    class World
    {

        public Octree space;
        public Graph spaceGraph;
        public List<Arc> arcList;

        //public World(GameObject scene, float size, Vector3 center, int maxLevel, float normalExtension, bool progressive = true, Graph.GraphType type = Graph.GraphType.CENTER)
        //{
        //    space = progressive ? new ProgressiveOctree(size, center - Vector3.One * size / 2, maxLevel) : new Octree(size, center - Vector3.One * size / 2, maxLevel);
        //    space.BuildFromGameObject(scene, normalExtension) ;
        //    spaceGraph =
        //        type == Graph.GraphType.CENTER ? space.ToCenterGraph() :
        //        type == Graph.GraphType.CORNER ? space.ToCornerGraph() : space.ToCrossedGraph();
        //}

        private World() { }

        public World(ref SceneView sceneView, List<Mesh> meshes, float size, Vector3 center, int maxLevel, float normalExtension, bool progressive = true, Graph.GraphType type = Graph.GraphType.CENTER)
        {
            space = progressive ? new ProgressiveOctree(size, center - Vector3.One * (size / 2), maxLevel) : new Octree(size, center - Vector3.One * size / 2, maxLevel);
            space.BuildFromMeshes(meshes, normalExtension);
            spaceGraph =
                type == Graph.GraphType.CENTER ? space.ToCenterGraph() :
                type == Graph.GraphType.CORNER ? space.ToCornerGraph() : space.ToCrossedGraph();
        }

        public World(Octree space, Graph.GraphType type = Graph.GraphType.CENTER)
        {
            this.space = space;
            spaceGraph =
                type == Graph.GraphType.CENTER ? space.ToCenterGraph() :
                type == Graph.GraphType.CORNER ? space.ToCornerGraph() : space.ToCrossedGraph();
            initialiseArcs();
        }

        public World(Octree space, Graph spaceGraph)
        {
            this.space = space;
            this.spaceGraph = spaceGraph;
            initialiseArcs();
        }

        private void initialiseArcs()
        {
            arcList = new List<Arc>();
            foreach (Node node in spaceGraph.nodes)
            {
                foreach (Arc arc in node.arcs)
                {
                    arcList.Add(arc);
                }
            }
        }

        public World(Octree space, Graph spaceGraph, List<Arc> arcList)
        {
            this.space = space;
            this.spaceGraph = spaceGraph;
            this.arcList = arcList;
            allocateArcs();
        }

        private void allocateArcs()
        {
            int arcInd = 0, nodeInd = 0;
            Arc currArc = arcList[arcInd];
            Node currNode = spaceGraph.nodes[nodeInd];
            while(arcInd < arcList.Count && nodeInd < spaceGraph.nodes.Count)
            {
                if (currArc.from.center.Equals(currNode.center))
                {
                    ////System.Diagnostics.Debug.WriteLine("This arc belongs to this node: " + currNode.center);
                    currNode.arcs.Add(currArc);
                    if (arcInd >= arcList.Count - 1) break;
                    currArc = arcList[++arcInd];
                } else
                {
                    ////System.Diagnostics.Debug.WriteLine("Not equals");
                    currNode = spaceGraph.nodes[++nodeInd];
                }
            }
        }

        //public void DisplayVoxels() {
        //    space.DisplayVoxels();
        //}
    }
}