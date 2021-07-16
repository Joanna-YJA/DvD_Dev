using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        static SimpleLineSymbol pathSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 5.0);
        static SimpleMarkerSymbol pathVertexSymbol = new SimpleMarkerSymbol()
        {
            Color = Color.Orange,
            Size = 10,
            Style = SimpleMarkerSymbolStyle.Circle
        };

        public Commanding(ref SceneView sceneView, World world)
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

        public void MoveOrder(Vector3 target, ref List<MapPoint> points)
        {
            List<Vector3> pathFindingDest = new List<Vector3>();
            foreach (SpaceUnit unit in activeUnits)
            {
                pathFindingDest.Add(unit.position);
            }
            if (pathFindingDest.Count > 0)
            {
                List<List<Node>> allWayPoints = spaceGraph.FindPath(spaceGraph.LazyThetaStar, target, pathFindingDest, space);
                AddMapPoints(ref points, allWayPoints[0]);
            }
        }

        public void AddMapPoints(ref List<MapPoint> points, List<Node> sectionPoints)
        {
            //if(points.Count > 0) System.Diagnostics.Debug.WriteLine("the pos of drone: " + points[points.Count - 1].ToString());
            System.Diagnostics.Debug.Write("Move to coords is called, path points: ");
            for(int i = sectionPoints.Count - 1; i >= 0; i--)
            {
                Vector3 c = sectionPoints[i].center * 10;
                MapPoint p = new MapPoint(c.X, c.Y, c.Z, PathFinder.spatialRef);
                points.Add(p);
                // System.Diagnostics.Debug.WriteLine("each of the map point p in move order: " + p);
                System.Diagnostics.Debug.Write(p.ToString() + " ");
            }
            System.Diagnostics.Debug.Write("\n");
        }

        public void ShowPath(ref List<MapPoint> points, GraphicsOverlay overlay)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                MapPoint p = points[i];
                Graphic pointSym = new Graphic(p, pathVertexSymbol);
                overlay.Graphics.Add(pointSym);
            }
            Polyline line = new Polyline(points);
            Graphic graphicWithSymbol = new Graphic(line, pathSymbol);
            overlay.Graphics.Add(graphicWithSymbol);
        }
    }
}