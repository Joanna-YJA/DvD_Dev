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
        [System.NonSerialized]
        public float ext;

        static TextSymbol textSymbol = new TextSymbol("", Color.Black, 15, HorizontalAlignment.Center, VerticalAlignment.Top);
        static SimpleLineSymbol pathSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 5.0);
        static SimpleMarkerSymbol pathVertexSymbol1 = new SimpleMarkerSymbol()
        {
            Color = Color.Orange,
            Size = 10,
            Style = SimpleMarkerSymbolStyle.Circle
        };
        static SimpleMarkerSymbol pathVertexSymbol2 = new SimpleMarkerSymbol()
        {
            Color = Color.Purple,
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
                //allWayPoints.Add(null);
                AddMapPoints(ref points, allWayPoints[0]);

                //foreach (Node n in allWayPoints[0])
                //    System.Diagnostics.Debug.WriteLine("VECTOR3 in path from source to dest: " + n.center);

                //foreach (MapPoint p in points)
                //    System.Diagnostics.Debug.WriteLine("path from source to dest: <" + p.X + ", " + p.Y + ", " + p.Z + ">");
            }
        }

        public void ConvertToAvailPoints(ref Vector3 localSrc, ref Vector3 localDest)
        {
            Vector3 dir = localDest - localSrc;
            dir = Vector3.Normalize(dir);
            dir = Vector3.Multiply(-0.1f, dir);

            while (!space.LineOfSight(localSrc, localDest, false, false))
            {
                System.Diagnostics.Debug.Write("Dest is moved a little bit from " + localDest);
                localDest += dir;
                System.Diagnostics.Debug.Write(" to " + localDest + "\n");
            }
            System.Diagnostics.Debug.WriteLine("Last line of convert to avail points dest " + localDest);

            //while (Math.Abs(src.X - dest.X) > 0.001 || Math.Abs(src.Y - dest.Y) > 0.001 || Math.Abs(src.Z - dest.Z) > 0.001)
            //{
            //    List<Node> tempPath = spaceGraph.FindPath(spaceGraph.LazyThetaStar, src, dest, space);

            //    System.Diagnostics.Debug.WriteLine("There is a LOS between source " + src + " dest " + dest + "? " + space.LineOfSight(src, dest, false, false));
            //    Vector3 test1 = new Vector3(1155708, 15064.18f, 2.5f), test2 = new Vector3(155727, 15068.38f, 3.125f);
            //    System.Diagnostics.Debug.WriteLine("There is a LOS between 2 points test1 " + test1 + " test2 " + test2 + "? " + space.LineOfSight(test1, test2, false, false));
            //    bool isValidPath = true;
            //    Node prevNode = null;
            //    foreach (Node n in tempPath)
            //    {
            //       if(prevNode != null) System.Diagnostics.Debug.WriteLine("comparing prevNode: " + prevNode.center + " with current: " + n.center);
            //        if (prevNode != null && !space.LineOfSight(prevNode.center, n.center, false, false))
            //        {
            //            System.Diagnostics.Debug.WriteLine("Is valid path is faslse");
            //            isValidPath = false;
            //            break;
            //        }
            //        prevNode = n;
            //    }

            //    if (isValidPath)
            //    {
            //        System.Diagnostics.Debug.WriteLine("Returned bcos a valid path is found");
            //        sourceDestLocal[1] = dest;
            //        return;
            //    }
            //    System.Diagnostics.Debug.WriteLine("dest is moved along line a little bit");
            //    dest += dir;
            //}

           // sourceDestLocal[1] = dest;
        }

        public void SearchInSpiral(ref List<MapPoint> points, Vector3 dest)
        {
            FootprintCalculator footprintCalc = new FootprintCalculator();
            List<Vector3> path = new List<Vector3>();

            // path.Add(dest);
            Vector3 prev = dest;

            float localhalfHeight = footprintCalc.getHalfHeight() / 10, localHalfBase = footprintCalc.getHalfBase() / 10;
            float incrY = localhalfHeight + localHalfBase, incrX = localHalfBase * 2;

            Vector3 backFront, side;
            int sign = 1;

            for (int i = 0; i < 200; i++)
            {
                backFront = new Vector3(prev.X, prev.Y + (sign * incrY), prev.Z);
                if (!space.CheckWithinBounds(backFront))
                {
                    System.Diagnostics.Debug.WriteLine("Check within bounds of backfront fails, backfront " + backFront);
                    AddMapPoints(ref points, path);
                    return;
                }
                int dirChange = 0;
                bool isFirst = true;
                while (!space.LineOfSight(prev, backFront, false, false))
                {
                    System.Diagnostics.Debug.WriteLine("there is no LOS between prev and backFront");
                    try
                    {
                        if (!space.CheckWithinBounds(backFront))
                        {
                            System.Diagnostics.Debug.WriteLine("Check within bounds of backfront fails, backfront " + backFront);
                            AddMapPoints(ref points, path);
                            return;
                        }
                        List<Node> tempPath = spaceGraph.FindPath(spaceGraph.LazyThetaStar, backFront, prev, space);

                        bool isValidPath = true;
                        Node prevNode = null;
                        float maxZ = 0;
                        foreach (Node n in tempPath)
                        {
                            if (isValidPath && prevNode != null && !space.LineOfSight(prevNode.center, n.center, false, false))
                            {
                                isValidPath = false;
                                System.Diagnostics.Debug.WriteLine("3d path is not valid as 2 of the points has no LOS");
                                //break;
                            }
                            prevNode = n;
                            maxZ = Math.Max(n.center.Z, maxZ);
                        }
                        System.Diagnostics.Debug.Write("backFront.Z changed from " + backFront.Z + " to ");
                        backFront.Z = maxZ;
                        System.Diagnostics.Debug.Write(backFront.Z + "\n");

                        if (isValidPath)
                        {
                            tempPath = spaceGraph.FindPath(spaceGraph.LazyThetaStar, backFront, prev, space);
                            for (int k = tempPath.Count - 2; k > 0; k--)
                                path.Add(tempPath[k].center);
                            //foreach (Node n in tempPath)
                            //    path.Add(n.center);
                            break;
                        }

                        if(!isFirst) backFront.Y += (sign * 0.5f);
                        isFirst = false;
                        System.Diagnostics.Debug.WriteLine("Y changed by 0.5");
                    }
                    catch (Exception e)
                    {
                        //sign *= -1;
                        //dirChange++;
                        //if (dirChange > 1)
                        //{
                        AddMapPoints(ref points, path);
                        System.Diagnostics.Debug.WriteLine("Returned because of Y incr/decr");
                        return;
                        //}
                    }
                }
                path.Add(backFront);
                path.Add(Vector3.Zero);

                prev = backFront;
                incrY += localHalfBase;
                if (i > 0) incrY += localHalfBase;
                else incrY += localhalfHeight;

                if(true)
                {
                    side = new Vector3(prev.X + (sign * incrX), prev.Y, prev.Z);
                    if (!space.CheckWithinBounds(side))
                    {
                        System.Diagnostics.Debug.WriteLine("Check within bounds of side fails, side " + side);
                        AddMapPoints(ref points, path);
                        return;
                    }
                    dirChange = 0;
                    isFirst = true;
                    while (!space.LineOfSight(prev, side, false, false))
                    {
                        System.Diagnostics.Debug.WriteLine("there is no LOS between prev and side");
                        try
                        {
                            if (!space.CheckWithinBounds(side))
                            {
                                System.Diagnostics.Debug.WriteLine("Check within bounds of side fails, side " + side);
                                AddMapPoints(ref points, path);
                                return;
                            }
                            List<Node> tempPath = spaceGraph.FindPath(spaceGraph.LazyThetaStar, side, prev, space);

                            bool isValidPath = true;
                            Node prevNode = null;
                            float maxZ = 0;
                            foreach (Node n in tempPath)
                            {
                                if (isValidPath && prevNode != null && !space.LineOfSight(prevNode.center, n.center, false, false))
                                {
                                    isValidPath = false;
                                    System.Diagnostics.Debug.WriteLine("3d path is not valid as 2 of the points has no LOS");
                                    //break;
                                }
                                prevNode = n;
                                maxZ = Math.Max(n.center.Z, maxZ);
                            }
                            System.Diagnostics.Debug.Write("side.Z changed from " + side.Z + " to ");
                            if(!isFirst) side.Z = maxZ;
                            isFirst = false;

                            System.Diagnostics.Debug.Write(side.Z + "\n");

                            if (isValidPath)
                            {
                                tempPath = spaceGraph.FindPath(spaceGraph.LazyThetaStar, side, prev, space);
                                for (int k = tempPath.Count - 2; k > 0; k--)
                                    path.Add(tempPath[k].center);
                                //foreach (Node n in tempPath)
                                //    path.Add(n.center);
                                break;
                            }              
                            side.X += (sign * 0.5f);
                            System.Diagnostics.Debug.WriteLine("X changed by 0.5");
                        }
                        catch (Exception e)
                        {
                            //sign *= -1;
                            //dirChange++;
                            //if (dirChange > 1)
                            //{
                            AddMapPoints(ref points, path);
                            System.Diagnostics.Debug.WriteLine("Returned because of X incr/decr");
                            return;
                            //}

                        }
                    }
                    path.Add(side);
                    path.Add(Vector3.Zero);

                    prev = side;
                    incrX += localHalfBase + localHalfBase;
                    sign *= -1;
                }
            }
            AddMapPoints(ref points, path);
        }

        public void AddMapPoints(ref List<MapPoint> points, List<Vector3> sectionPoints)
        {
            //if(points.Count > 0) System.Diagnostics.Debug.WriteLine("the pos of drone: " + points[points.Count - 1].ToString());
            //System.Diagnostics.Debug.Write("Move to coords is called, path points: ");
            foreach (Vector3 v in sectionPoints)
            {
                Vector3 c = v * 10;
                MapPoint p = new MapPoint(c.X, c.Y, c.Z, PathFinder.spatialRef);
                //points.Add(p);
               
                if (Math.Abs(c.X) < 0.001 && Math.Abs(c.Y) < 0.001 && Math.Abs(c.Z) < 0.001)
                {
                    points.Add(null);
                    continue;
                }
                else points.Add(p);
                // System.Diagnostics.Debug.WriteLine("each of the map point p in move order: " + p);
                //System.Diagnostics.Debug.WriteLine(p.ToString() + " ");
            }
            // System.Diagnostics.Debug.Write("\n");
        }

        public void AddMapPoints(ref List<MapPoint> points, List<Node> sectionPoints)
        {
            //if(points.Count > 0) System.Diagnostics.Debug.WriteLine("the pos of drone: " + points[points.Count - 1].ToString());
            //System.Diagnostics.Debug.Write("Move to coords is called, path points: ");
            for (int i = sectionPoints.Count - 1; i >= 0; i--)
            {
                if(sectionPoints[i] == null)
                {
                    points.Add(null);
                    continue;
                }
                Vector3 c = sectionPoints[i].center * 10;
                MapPoint p = new MapPoint(c.X, c.Y, c.Z, PathFinder.spatialRef);
               // System.Diagnostics.Debug.WriteLine("c: " + c);
                 points.Add(p);
                // System.Diagnostics.Debug.WriteLine("each of the map point p in move order: " + p);
                //System.Diagnostics.Debug.WriteLine(p.ToString() + " ");
            }
            // System.Diagnostics.Debug.Write("\n");
        }

        public void ShowPath(ref List<MapPoint> points, GraphicsOverlay overlay)
        {
            MapPoint prev = null;
            bool useSecond = false;
            for (int i = 0; i < points.Count; i++)
            {
                //test
                if (points[i] == null)
                {
                    useSecond = !useSecond;
                    continue;
                }

                MapPoint p = points[i];
                //test
                Graphic pointGraphic;
                if(useSecond) pointGraphic = new Graphic(p, pathVertexSymbol2);
                else pointGraphic = new Graphic(p, pathVertexSymbol1);
                overlay.Graphics.Add(pointGraphic);

                TextSymbol labelSymbol = (TextSymbol)textSymbol.Clone();
                labelSymbol.Text = i.ToString(); //+ " <" + p.X + ", " + p.Y + ", " + p.Z + ">";

                //test
                //int row, col;
                //space.ConvertMapPointToCell(p, out row, out col);
                //if (prev != null) labelSymbol.Text += " cell: " + row + ", " + col;
                Graphic labelGraphic = new Graphic(p, labelSymbol);
                //System.Diagnostics.Debug.WriteLine("text symbol added..." + labelSymbol.Text);
                overlay.Graphics.Add(labelGraphic);
                prev = p;
            }

            //test
            //foreach (MapPoint p in points)
            //    System.Diagnostics.Debug.WriteLine("Polyline vectors: " + new Vector3((float) p.X / 10, (float)p.Y / 10, (float)p.Z / 10));
            //test
            List<MapPoint> nonNullPoints = new List<MapPoint>();
            foreach (MapPoint p in points) {
                if (p != null)
                {
                    nonNullPoints.Add(p);
                   // System.Diagnostics.Debug.WriteLine("line points: <" + p.X + ", " + p.Y + ", " + p.Z + ">");
                }
            }

            Polyline line = new Polyline(nonNullPoints);
            Graphic graphicWithSymbol = new Graphic(line, pathSymbol);
            overlay.Graphics.Add(graphicWithSymbol);
        }
    }
}