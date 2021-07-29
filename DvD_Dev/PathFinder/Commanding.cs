using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using NetTopologySuite.Mathematics;
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
        [NonSerialized]
        public float ext;

        static TextSymbol textSymbol = new TextSymbol("", Color.Black, 15, HorizontalAlignment.Center, VerticalAlignment.Top);
        static SimpleLineSymbol pathSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 5.0);
        static SimpleMarkerSymbol pathVertexSymbol1 = new SimpleMarkerSymbol()
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

        public void BfsSearch(Vector3 target, ref List<MapPoint> points)
        {

            List<Vector3> wayPoints = space.SearchInSpiral2(target);
            AddMapPoints(ref points, wayPoints);

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
                AddFrontMapPoints(ref points, allWayPoints[0]);
            }
        }

        public void SearchInSpiral(ref List<MapPoint> points, Vector3 dest, FootprintCalculator fpCalc)
        {
            List<Vector3> path = new List<Vector3>();


            Vector3 prev = dest;
            path.Add(prev);
            path.Add(Vector3.Zero);
            float origHeight = prev.Z;

            float localFieldDim = PathFinder.fieldDimM / 10;
            float top = fpCalc.top / 10, left = fpCalc.left / 10, right = fpCalc.right / 10;
            float maxSide = Math.Max(fpCalc.right + fpCalc.left, fpCalc.top + fpCalc.bott)/ 10;
            float incrY = top + right, incrX = right + left;

            Vector3 backFront, side;
            int sign = 1;

            for (int i = 0; i < 200; i++)
            {
                backFront = new Vector3(prev.X, prev.Y + (sign * incrY), origHeight);
                if (!space.CheckWithinBounds(backFront, localFieldDim + maxSide * 2f))
                {
                    AddMapPoints(ref points, path);
                    return;
                }

                if (!space.LineOfSight(prev, backFront, false, false))
                {
                    float height = space.FindMinUnBlockedHeight(backFront);//3.90625f; 
                    backFront.Z = height;
                    List<Node> sectionPath = spaceGraph.FindPath(spaceGraph.LazyThetaStar, prev, backFront, space);

                    foreach (Node n in sectionPath.GetRange(1, sectionPath.Count - 1))
                        path.Add(n.center);
                    path.Add(Vector3.Zero);
                }
                else
                {
                    path.Add(backFront);
                    path.Add(Vector3.Zero);
                }

                prev = backFront;

                incrY += right + left;

                side = new Vector3(prev.X + (sign * incrX), prev.Y, origHeight);
                if (!space.CheckWithinBounds(side, localFieldDim + maxSide * 2f))
                {
                    AddMapPoints(ref points, path);
                    return;
                }

                if (!space.LineOfSight(prev, side, false, false))
                {
                    float height = space.FindMinUnBlockedHeight(side);

                    side.Z = height;
                    List<Node> sectionPath = spaceGraph.FindPath(spaceGraph.LazyThetaStar, prev, side, space);

                    foreach (Node n in sectionPath.GetRange(1, sectionPath.Count - 1))
                        path.Add(n.center);
                    path.Add(Vector3.Zero);
                }
                else
                {
                    path.Add(side);
                    path.Add(Vector3.Zero);
                }

                prev = side;

                incrX += right + left;
                sign *= -1;
            }
            spaceGraph.RemoveTemporaryNodes();
            AddMapPoints(ref points, path);
        }

        public void AddMapPoints(ref List<MapPoint> points, List<Vector3> sectionPoints)
        {
            for (int i = sectionPoints.Count - 1; i >= 0; i--)
            {
                Vector3 c = sectionPoints[i] * 10;
                MapPoint p = new MapPoint(c.X, c.Y, c.Z, PathFinder.spatialRef);
                //points.Add(p);

                //if (Math.Abs(c.X) < 0.001 && Math.Abs(c.Y) < 0.001 && Math.Abs(c.Z) < 0.001)
                if(c.Equals(Vector3.Zero))
                {
                    points.Add(null);
                    continue;
                }
                else
                    points.Add(p);
            }
        }

        public void AddMapPoints(ref List<MapPoint> points, List<Node> sectionPoints)
        {
            for (int i = sectionPoints.Count - 1; i >= 0; i--)
            {
                Vector3 c = sectionPoints[i].center * 10;
                MapPoint p = new MapPoint(c.X, c.Y, c.Z, PathFinder.spatialRef);
                points.Add(p);
            }
        }

        public void AddFrontMapPoints(ref List<MapPoint> points, List<Node> sectionPoints)
        {
            List<MapPoint> front = new List<MapPoint>();
            for (int i = sectionPoints.Count - 1; i > 0; i--)
            {
                Node n = sectionPoints[i];
                Vector3 c = n.center * 10;
                MapPoint p = new MapPoint(c.X, c.Y, c.Z, PathFinder.spatialRef);
                front.Add(p);
            }
            front.AddRange(points);
            points = front;
        }

        public void ShowPath(ref List<MapPoint> points, GraphicsOverlay overlay)
        {
            for (int i = 0; i < points.Count; i++)
            {
               
                if (points[i] == null) continue;
                MapPoint p = points[i];
                //test
                Graphic pointGraphic = new Graphic(new MapPoint(p.X, p.Y, p.Z, PathFinder.spatialRef), pathVertexSymbol1);
                overlay.Graphics.Add(pointGraphic);

                TextSymbol labelSymbol = (TextSymbol)textSymbol.Clone();
                labelSymbol.Text = i.ToString();

                Graphic labelGraphic = new Graphic(p, labelSymbol);
                overlay.Graphics.Add(labelGraphic);

            }

            List<MapPoint> nonNullPoints = new List<MapPoint>();
            foreach (MapPoint p in points)
            {
                if (p != null)
                {
                    nonNullPoints.Add(p);
                }
            }

            Polyline line = new Polyline(nonNullPoints);
            Graphic graphicWithSymbol = new Graphic(line, pathSymbol);
            overlay.Graphics.Add(graphicWithSymbol);
        }
    }
}