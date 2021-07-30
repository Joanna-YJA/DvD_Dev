using Esri.ArcGISRuntime.Geometry;
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
        public List<Arc> arcs;

        //public World(GameObject scene, float size, Vector3 center, int maxLevel, float normalExtension, bool progressive = true, Graph.GraphType type = Graph.GraphType.CENTER)
        //{
        //    space = progressive ? new ProgressiveOctree(size, center - Vector3.One * size / 2, maxLevel) : new Octree(size, center - Vector3.One * size / 2, maxLevel);
        //    space.BuildFromGameObject(scene, normalExtension) ;
        //    spaceGraph =
        //        type == Graph.GraphType.CENTER ? space.ToCenterGraph() :
        //        type == Graph.GraphType.CORNER ? space.ToCornerGraph() : space.ToCrossedGraph();
        //}

        private World() 
        {

        }

        public World(ref SceneView sceneView, Octree space, List<Arc> arcs, Graph.GraphType type = Graph.GraphType.CENTER)
        {
            this.space = space;
            this.spaceGraph = space.cornerGraph;
            this.arcs = arcs;
            allocateArcs();
        }
        public World(ref SceneView sceneView, SpatialReference spatialRef, List<Mesh> meshes, float size, Vector3 center, int maxLevel, float normalExtension, bool progressive = true, Graph.GraphType type = Graph.GraphType.CENTER)
        {

            this.space = progressive ? new ProgressiveOctree(size, center - Vector3.One * (size / 2), maxLevel) : new Octree(size, center - Vector3.One * size / 2, maxLevel);
            this.space.BuildFromMeshes(meshes, normalExtension);
            this.space.SetSpatialRef(spatialRef);

            this.spaceGraph =
                type == Graph.GraphType.CENTER ? this.space.ToCenterGraph() :
                type == Graph.GraphType.CORNER ? this.space.ToCornerGraph() : this.space.ToCrossedGraph();
            this.spaceGraph.SetSpatialRef(spatialRef);
            initialiseArcs();

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
            arcs = new List<Arc>();
            foreach (Node node in spaceGraph.nodes)
            {
                foreach (Arc arc in node.arcs)
                {
                    arcs.Add(arc);
                }
            }
        }

        public World(Octree space, Graph spaceGraph, List<Arc> arcs)
        {
            this.space = space;
            this.spaceGraph = spaceGraph;
            this.arcs = arcs;
            allocateArcs();
        }

        public void allocateArcs()
        {
            int arcInd = 0, nodeInd = 0;
            Arc currArc = arcs[arcInd];
            Node currNode = spaceGraph.nodes[nodeInd];
            while (arcInd < arcs.Count && nodeInd < spaceGraph.nodes.Count)
            {
                if (currArc.from.center.Equals(currNode.center))
                {
                    ////System.Diagnostics.Debug.WriteLine("This arc belongs to this node: " + currNode.center);
                    currNode.arcs.Add(currArc);
                    if (arcInd >= arcs.Count - 1) break;
                    currArc = arcs[++arcInd];
                }
                else
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