using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace DvD_Dev
{
    class Octree
    {
        public int maxLevel;
        public Vector3 corner;
        public float size;
        public float cellSize
        {
            get { return size / (1 << maxLevel); }
        }
        public OctreeNode root;

        public static int[,] dir = { { 1, 0, 0 }, { -1, 0, 0 }, { 0, 1, 0 }, { 0, -1, 0 }, { 0, 0, 1 }, { 0, 0, -1 } };
        public static int[,] cornerDir = { { 0, 0, 0 }, { 1, 0, 0 }, { 1, 1, 0 }, { 0, 1, 0 }, { 0, 0, 1 }, { 1, 0, 1 }, { 1, 1, 1 }, { 0, 1, 1 } };
        public static int[,] edgeDir = { { 0, 1, 1 }, { 0, 1, -1 }, { 0, -1, 1 }, { 0, -1, -1 }, { 1, 0, 1 }, { -1, 0, 1 }, { 1, 0, -1 }, { -1, 0, -1 }, { 1, 1, 0 }, { 1, -1, 0 }, { -1, 1, 0 }, { -1, -1, 0 } };

        static SimpleMarkerSymbol blockedNodeSymbol = new SimpleMarkerSymbol()
        {
            Color = Color.Red,
            Size = 10,
            Style = SimpleMarkerSymbolStyle.Triangle
        };

        static SimpleMarkerSymbol nodeSymbol = new SimpleMarkerSymbol()
        {
            Color = Color.LightBlue,
            Size = 10,
            Style = SimpleMarkerSymbolStyle.Triangle
        };

        private Octree() { }
        public Octree(float _size, Vector3 _corner, int _maxLevel)
        {
            size = _size;
            corner = _corner;
            maxLevel = _maxLevel;
            root = new OctreeNode(0, new int[] { 0, 0, 0 }, null, this);
        }

        public void BuildFromMeshes(List<Mesh> meshes, float normalExpansion = 0)
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                int[] triangles = meshes[i].triangles;
                Vector3[] verts = meshes[i].vertices;
                Vector3[] vertsNormal = meshes[i].normals;
                for (int j = 0; j < triangles.Length / 3; j++)
                {
                    DivideTriangle(
                        verts[triangles[3 * j]] + vertsNormal[triangles[3 * j]] * normalExpansion,
                        verts[triangles[3 * j + 1]] + vertsNormal[triangles[3 * j + 1]] * normalExpansion,
                        verts[triangles[3 * j + 2]] + vertsNormal[triangles[3 * j + 2]] * normalExpansion, true);
                }
            }
        }

        public int[] PositionToIndex(Vector3 p)
        {
            p -= corner;
            float d = cellSize;
            return new int[] { (int)MathF.Floor(p.X / d), (int)MathF.Floor(p.Y / d), (int)MathF.Floor(p.Z / d) };
        }
        public Vector3 IndexToPosition(int[] gridIndex)
        {
            float d = cellSize;
            return new Vector3(gridIndex[0] * d, gridIndex[1] * d, gridIndex[2] * d) + corner;
        }

        public OctreeNode Find(int[] gridIndex)
        {
            return Find(gridIndex, maxLevel);
        }

        public bool CheckWithinBounds(Vector3 v)
        {
            v -= root.center;
            float r = size / 2;
            if (v.X >= r || v.X < -r || v.Y >= r || v.Y < -r || v.Z >= r || v.Z < -r) return false;
            else return true;
        }

        public bool CheckWithinBounds(Vector3 v, int units)
        {
            v -= root.center;
            float r = units / 2;
            if (v.X >= r || v.X < -r || v.Y >= r || v.Y < -r || v.Z >= r || v.Z < -r) return false;
            else return true;
        }

        public OctreeNode Find(int[] gridIndex, int level)
        {
            int xi = gridIndex[0];
            int yi = gridIndex[1];
            int zi = gridIndex[2];
            int t = 1 << level;
            if (xi >= t || xi < 0 || yi >= t || yi < 0 || zi >= t || zi < 0) return null;

            OctreeNode current = root;
            for (int l = 0; l < level; l++)
            {
                t >>= 1;
                if (current.children == null) return current;
                current = current.children[xi / t, yi / t, zi / t];
                xi %= t;
                yi %= t;
                zi %= t;
            }
            return current;
        }
        public OctreeNode Find(Vector3 p)
        {
            return Find(PositionToIndex(p));
        }
        public bool IsBlocked(int[] gridIndex, bool outsideIsBlocked = false, bool doublePrecision = false)
        {
            int xi = gridIndex[0];
            int yi = gridIndex[1];
            int zi = gridIndex[2];
            if (doublePrecision)
            {
                xi /= 2;
                yi /= 2;
                zi /= 2;
            }
            int t = 1 << maxLevel;
            if (xi >= t || xi < 0 || yi >= t || yi < 0 || zi >= t || zi < 0) return outsideIsBlocked;
            OctreeNode current = root;
            for (int l = 0; l < maxLevel; l++)
            {
                t >>= 1;
                if (!current.containsBlocked) return false;
                if (current.children == null) return current.blocked;
                current = current.children[xi / t, yi / t, zi / t];
                xi %= t;
                yi %= t;
                zi %= t;
            }
            return current.blocked;
        }

        public void Divide(Vector3 p, bool markAsBlocked = false)
        {
            int[] gridIndex = PositionToIndex(p);
            int xi = gridIndex[0];
            int yi = gridIndex[1];
            int zi = gridIndex[2];
            int t = 1 << maxLevel;
            if (xi >= t || xi < 0 || yi >= t || yi < 0 || zi >= t || zi < 0) return;
            OctreeNode current = root;
            for (int l = 0; l < maxLevel; l++)
            {
                t >>= 1;
                current.containsBlocked = current.containsBlocked || markAsBlocked;
                if (current.children == null) current.CreateChildren();
                current = current.children[xi / t, yi / t, zi / t];
                xi %= t;
                yi %= t;
                zi %= t;
            }
            current.blocked = current.blocked || markAsBlocked;
            current.containsBlocked = current.blocked;
        }

        public void DivideTriangle(Vector3 p1, Vector3 p2, Vector3 p3, bool markAsBlocked = false)
        {
            root.DivideTriangleUntilLevel(p1, p2, p3, maxLevel, markAsBlocked);
        }

        public bool LineOfSight(Vector3 p1, Vector3 p2, bool outsideIsBlocked = false, bool doublePrecision = false)
        {
            if (p1.Z < 0 || p2.Z < 0) return false;
            Vector3 p1g = (p1 - corner) / cellSize;
            Vector3 p2g = (p2 - corner) / cellSize;
            if (doublePrecision)
            {
                p1g *= 2;
                p2g *= 2;
            }
            int[,] p = new int[2, 3];
            int[] d = new int[3];
            int[] sign = new int[3];
            int[] f = new int[2];

            for (int i = 0; i < 3; i++)
            {
                p[0, i] = (int)MathF.Round(p1g.Get(i));
                p[1, i] = (int)MathF.Round(p2g.Get(i));
                d[i] = p[1, i] - p[0, i];
                if (d[i] < 0)
                {
                    d[i] = -d[i];
                    sign[i] = -1;
                }
                else
                {
                    sign[i] = 1;
                }
            }
            int[] pBlock = { p[0, 0] + (sign[0] - 1) / 2, p[0, 1] + (sign[1] - 1) / 2, p[0, 2] + (sign[2] - 1) / 2 };

            int longAxis;
            if (d[0] >= d[1] && d[0] >= d[2]) longAxis = 0;
            else if (d[1] >= d[2]) longAxis = 1;
            else longAxis = 2;
            if (d[longAxis] == 0) return true;  //Returns here
            int axis0 = (longAxis + 1) % 3;
            int axis1 = (longAxis + 2) % 3;

            while (p[0, longAxis] != p[1, longAxis])
            {
                f[0] += d[axis0];
                f[1] += d[axis1];
                if (f[0] >= d[longAxis] && f[1] < d[longAxis])
                {
                    f[0] -= d[longAxis];
                    if (d[axis1] != 0)
                    {
                        if (IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) return false;
                    }
                    else
                    {
                        bool sight = false;
                        pBlock[axis1] -= 1;
                        if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                        pBlock[axis1] += 1;
                        if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                        if (!sight) return false;
                    }
                    p[0, axis0] += sign[axis0];
                    pBlock[axis0] += sign[axis0];
                }
                else if (f[1] >= d[longAxis] && f[0] < d[longAxis])
                {
                    f[1] -= d[longAxis];
                    if (d[axis0] != 0)
                    {
                        if (IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) return false;
                    }
                    else
                    {
                        bool sight = false;
                        pBlock[axis0] -= 1;
                        if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                        pBlock[axis0] += 1;
                        if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                        if (!sight) return false;
                    }
                    p[0, axis1] += sign[axis1];
                    pBlock[axis1] += sign[axis1];
                }
                else if (f[0] >= d[longAxis] && f[1] >= d[longAxis])
                {
                    f[0] -= d[longAxis];
                    f[1] -= d[longAxis];
                    if (IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) return false;
                    int det = f[0] * d[axis1] - f[1] * d[axis0];
                    if (det > 0)
                    {
                        pBlock[axis0] += sign[axis0];
                        if (IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) return false;
                        pBlock[axis1] += sign[axis1];
                    }
                    else if (det < 0)
                    {
                        pBlock[axis1] += sign[axis1];
                        if (IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) return false;
                        pBlock[axis0] += sign[axis0];
                    }
                    else
                    {
                        pBlock[axis0] += sign[axis0];
                        pBlock[axis1] += sign[axis1];
                    }
                    p[0, axis0] += sign[axis0];
                    p[0, axis1] += sign[axis1];
                }

                if (f[0] != 0 && f[1] != 0 && IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) return false;
                if (d[axis0] == 0 && d[axis1] != 0)
                {
                    bool sight = false;
                    pBlock[axis0] -= 1;
                    if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                    pBlock[axis0] += 1;
                    if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                    if (!sight) return false;
                }
                else if (d[axis0] != 0 && d[axis1] == 0)
                {
                    bool sight = false;
                    pBlock[axis1] -= 1;
                    if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                    pBlock[axis1] += 1;
                    if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                    if (!sight) return false;
                }
                else if (d[axis0] == 0 && d[axis1] == 0)
                {
                    bool sight = false;
                    pBlock[axis0] -= 1;
                    if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                    pBlock[axis1] -= 1;
                    if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                    pBlock[axis0] += 1;
                    if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                    pBlock[axis1] += 1;
                    if (!IsBlocked(pBlock, outsideIsBlocked, doublePrecision)) sight = true;
                    if (!sight) return false;
                }
                p[0, longAxis] += sign[longAxis];
                pBlock[longAxis] += sign[longAxis];
            }
            return true;
        }

        private bool FloorToIntSnap(float n, out int i, float epsilon = 0.001f)
        {
            i = (int)MathF.Floor(n + epsilon);
            return MathF.Abs(n - i) <= epsilon;
        }

        public Graph centerGraph;
        public Dictionary<OctreeNode, Node> centerGraphDictionary;
        public Graph ToCenterGraph()
        {
            List<OctreeNode> leaves = root.Leaves();
            Dictionary<OctreeNode, Node> dict = new Dictionary<OctreeNode, Node>();
            int count = 0;
            List<Node> nodes = new List<Node>();
            foreach (OctreeNode q in leaves)
            {
                if (!q.blocked)
                {
                    Node n = new Node(q.center, count);
                    dict.Add(q, n);
                    nodes.Add(n);
                    count++;
                }
            }
            foreach (OctreeNode q in leaves)
            {
                if (!q.blocked)
                {
                    if (q.level == 0) continue;
                    Node n = dict[q];
                    for (int i = 0; i < 6; i++)
                    {
                        OctreeNode found = Find(new int[] { q.index[0] + dir[i, 0], q.index[1] + dir[i, 1], q.index[2] + dir[i, 2] }, q.level);
                        if (found == null || found.blocked) continue;
                        if (found.level < q.level)
                        {
                            Node nFound = dict[found];
                            n.arcs.Add(new Arc(n, nFound));
                            nFound.arcs.Add(new Arc(nFound, n));
                        }
                        else if (found.children == null)
                        {
                            Node nFound = dict[found];
                            n.arcs.Add(new Arc(n, nFound));
                        }
                    }
                }
            }
            Graph g = new Graph();
            g.nodes = nodes;
            g.CalculateConnectivity();
            g.type = Graph.GraphType.CENTER;
            centerGraph = g;
            centerGraphDictionary = dict;
            return g;
        }

        public Graph cornerGraph;
        public Dictionary<long, Node> cornerGraphDictionary;
        public Graph ToCornerGraph()
        {
            List<OctreeNode> leaves = root.Leaves();
            Dictionary<long, Node> dict = new Dictionary<long, Node>();
            Dictionary<long, bool> arcAdded = new Dictionary<long, bool>();
            List<Node> nodes = new List<Node>();

            foreach (OctreeNode o in leaves)
            {
                for (int i = 0; i < 12; i++)
                {
                    int[][] threeNeighborDir = ThreeNeighborDir(new int[] { edgeDir[i, 0], edgeDir[i, 1], edgeDir[i, 2] });
                    bool draw;
                    if (o.blocked)
                    {
                        draw = false;
                        for (int j = 0; j < 3; j++)
                        {
                            draw = !IsBlocked(new int[] { o.index[0] + threeNeighborDir[j][0], o.index[1] + threeNeighborDir[j][1], o.index[2] + threeNeighborDir[j][2] });
                            if (draw) break;
                        }
                    }
                    else
                    {
                        draw = true;
                        for (int j = 0; j < 3; j++)
                        {
                            OctreeNode found = Find(new int[] { o.index[0] + threeNeighborDir[j][0], o.index[1] + threeNeighborDir[j][1], o.index[2] + threeNeighborDir[j][2] }, o.level);
                            if (found != null && found.level == o.level && found.children != null)
                            {
                                draw = false;
                                break;
                            }
                        }
                    }

                    if (draw)
                    {
                        int[][] arcVertexCoord = ArcVertexDir(new int[] { edgeDir[i, 0], edgeDir[i, 1], edgeDir[i, 2] });
                        for (int j = 0; j < 2; j++)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                arcVertexCoord[j][k] = (o.index[k] * 2 + 1 + arcVertexCoord[j][k]) / 2 * (1 << (maxLevel - o.level));
                            }
                        }
                        long arcKey = GetArcKey(arcVertexCoord[0], arcVertexCoord[1]);
                        bool temp;
                        if (!arcAdded.TryGetValue(arcKey, out temp))
                        {
                            arcAdded[arcKey] = true;
                            Node n1 = GetNodeFromDict(arcVertexCoord[0], dict, nodes);
                            Node n2 = GetNodeFromDict(arcVertexCoord[1], dict, nodes);
                            n1.arcs.Add(new Arc(n1, n2));
                            n2.arcs.Add(new Arc(n2, n1));
                        }
                    }
                }
            }
            Graph g = new Graph();
            g.nodes = nodes;
            g.CalculateConnectivity();
            g.type = Graph.GraphType.CORNER;
            cornerGraph = g;
            cornerGraphDictionary = dict;

            return g;
        }

        public Graph crossedGraph;
        public Dictionary<long, Node> crossedGraphDictionary;
        public Dictionary<OctreeNode, HashSet<Node>> crossedGraphBoundingNodesDictionary;
        public Graph ToCrossedGraph()
        {
            List<OctreeNode> leaves = root.Leaves();
            Dictionary<long, Node> dict = new Dictionary<long, Node>();
            Dictionary<OctreeNode, HashSet<Node>> dictNodes = new Dictionary<OctreeNode, HashSet<Node>>();
            Dictionary<string, bool> arcAdded = new Dictionary<string, bool>();
            List<Node> nodes = new List<Node>();
            List<int[]> coords = new List<int[]>();

            foreach (OctreeNode o in leaves)
            {
                if (!o.blocked)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        int[] index = o.cornerIndex(i);
                        long key = GetNodeKey(index);
                        if (!dict.ContainsKey(key))
                        {
                            Node node = GetNodeFromDict(index, dict, nodes);
                            coords.Add(index);
                            for (int j = 0; j < 8; j++)
                            {
                                int[] gridIndex = new int[] { index[0] - 1 + cornerDir[j, 0], index[1] - 1 + cornerDir[j, 1], index[2] - 1 + cornerDir[j, 2] };
                                OctreeNode voxel = Find(gridIndex);
                                if (voxel != null && !voxel.blocked)
                                {
                                    if (!dictNodes.ContainsKey(voxel))
                                    {
                                        dictNodes.Add(voxel, new HashSet<Node> { node });
                                    }
                                    else
                                    {
                                        dictNodes[voxel].Add(node);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (OctreeNode voxel in dictNodes.Keys)
            {
                List<Node> enclosingNodes = new List<Node>(dictNodes[voxel]);
                for (int i = 0; i < enclosingNodes.Count - 1; i++)
                {
                    for (int j = i + 1; j < enclosingNodes.Count; j++)
                    {
                        Node n1 = enclosingNodes[i];
                        Node n2 = enclosingNodes[j];
                        int[] coord1 = coords[n1.index];
                        int[] coord2 = coords[n2.index];
                        if ((coord1[0] == coord2[0] || coord1[1] == coord2[1] || coord1[2] == coord2[2]) && (coord1[0] - coord2[0]) % 2 == 0 && (coord1[1] - coord2[1]) % 2 == 0 && (coord1[2] - coord2[2]) % 2 == 0)
                        {
                            int[] coordM = new int[] { (coord1[0] + coord2[0]) / 2, (coord1[1] + coord2[1]) / 2, (coord1[2] + coord2[2]) / 2 };
                            if (dict.ContainsKey(GetNodeKey(coordM)))
                            {
                                continue;
                            }
                        }
                        string arcKey = GetArcKeyS(coord1, coord2);
                        bool temp;
                        if (!arcAdded.TryGetValue(arcKey, out temp))
                        {
                            arcAdded[arcKey] = true;
                            n1.arcs.Add(new Arc(n1, n2));
                            n2.arcs.Add(new Arc(n2, n1));
                        }
                    }
                }
            }
            Graph g = new Graph();
            g.nodes = nodes;
            g.CalculateConnectivity();
            g.type = Graph.GraphType.CROSSED;
            crossedGraph = g;
            crossedGraphDictionary = dict;
            crossedGraphBoundingNodesDictionary = dictNodes;
            return g;
        }

        private Node GetNodeFromDict(int[] index, Dictionary<long, Node> dict, List<Node> nodes = null)
        {
            long key = GetNodeKey(index);
            Node result = null;
            if (!dict.TryGetValue(key, out result) && nodes != null)
            {
                result = new Node(IndexToPosition(index), nodes.Count);
                dict.Add(key, result);
                nodes.Add(result);
            }
            return result;
        }
        private long GetNodeKey(int[] index)
        {
            long rowCount = 1 << maxLevel + 1;
            return (index[0] * rowCount + index[1]) * rowCount + index[2];
        }
        private long GetArcKey(int[] index1, int[] index2)
        {
            long rowCount = 1 << (maxLevel + 1) + 1;
            return ((index1[0] + index2[0]) * rowCount + index1[1] + index2[1]) * rowCount + index1[2] + index2[2];
        }
        private string GetArcKeyS(int[] index1, int[] index2)
        {
            return "" + (char)index1[0] + (char)index1[1] + (char)index1[2] + (char)index2[0] + (char)index2[1] + (char)index2[2];
        }
        private int[][] ThreeNeighborDir(int[] edgeDir)
        {
            int zeroIndex = -1;
            for (int i = 0; i < 3; i++)
            {
                if (edgeDir[i] == 0)
                {
                    zeroIndex = i;
                    break;
                }
            }
            int[][] result = new int[3][];
            for (int i = 0; i < 3; i++)
            {
                result[i] = new int[3];
                for (int j = 0; j < 3; j++)
                {
                    result[i][j] = edgeDir[j];
                }
            }
            result[0][(zeroIndex + 1) % 3] = 0;
            result[2][(zeroIndex + 2) % 3] = 0;
            return result;
        }
        private int[][] ArcVertexDir(int[] edgeDir)
        {
            int zeroIndex = -1;
            for (int i = 0; i < 3; i++)
            {
                if (edgeDir[i] == 0)
                {
                    zeroIndex = i;
                    break;
                }
            }
            int[][] result = new int[2][];
            for (int i = 0; i < 2; i++)
            {
                result[i] = new int[3];
                for (int j = 0; j < 3; j++)
                {
                    result[i][j] = edgeDir[j];
                }
            }
            result[0][zeroIndex] = 1;
            result[1][zeroIndex] = -1;
            return result;
        }

        public List<Node> FindCorrespondingCenterGraphNode(Vector3 position)
        {
            List<Node> result = new List<Node>();
            OctreeNode node = Find(position);
            if (node != null && centerGraphDictionary.ContainsKey(node))
            {
                result.Add(centerGraphDictionary[node]);
            }
            return result;
        }

        public List<Node> FindBoundingCornerGraphNodes(Vector3 position)
        {
            List<Node> result = new List<Node>();
            OctreeNode node = Find(position);
            if (node != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    int[] cornerIndex = new int[3];
                    for (int j = 0; j < 3; j++)
                    {
                        cornerIndex[j] = (node.index[j] + cornerDir[i, j]) << (maxLevel - node.level);
                    }
                    result.Add(GetNodeFromDict(cornerIndex, cornerGraphDictionary));
                }
            }
            else throw new Exception("The source node is null.");
            return result;
        }

        //public List<Vector3> OctreeBfs(Vector3 position)
        //{
        //    System.Diagnostics.Debug.WriteLine("Original pos is " + position);
        //    Vector3[] arr = new Vector3[] {new Vector3(10, 0, 0), new Vector3(-10, 0, 0),
        //                                   new Vector3(0, 10, 0), new Vector3(0, -10, 0)};
        //    List<Vector3> res = new List<Vector3>();
        //    HashSet<string> set = new HashSet<string>();
        //    Queue<Vector3> q = new Queue<Vector3>();

        //    q.Enqueue(position);
        //    set.Add(position.ToString());

        //    test
        //    int i = 300;

        //    while (q.Count > 0)
        //    {
        //        Vector3 curr = q.Dequeue();
        //        res.Add(curr);

        //        System.Diagnostics.Debug.WriteLine("take out from q: " + curr.ToString());
        //        foreach (Vector3 v in arr)
        //        {
        //            Vector3 neighbor = new Vector3(curr.X, curr.Y, curr.Z);
        //            neighbor += v;
        //            Vector3 centredNeighbor = neighbor - root.center;
        //            float r = root.size / 2; // - root.tolerance; //todo set tolerance
        //            string neighborStr = neighbor.ToString();

        //            System.Diagnostics.Debug.WriteLine("neighbor " + neighbor + " Str " + neighborStr + " set contains? " + set.Contains(neighborStr));
        //            if (centredNeighbor.X >= r || centredNeighbor.X < -r || centredNeighbor.Y >= r || centredNeighbor.Y < -r) continue;
        //            if (!set.Contains(neighborStr)
        //                && LineOfSight(curr, neighbor, false, false)) //todo
        //            {
        //                System.Diagnostics.Debug.WriteLine("neighbor added, neighbor " + neighbor);
        //                q.Enqueue(neighbor);
        //                set.Add(neighborStr);
        //            }
        //        }

        //        i--;
        //        if (i < 0) break;

        //    }
        //    foreach (Vector3 v in res)
        //        System.Diagnostics.Debug.WriteLine("res: " + v.ToString());
        //    System.Diagnostics.Debug.WriteLine("#vectors in result: " + res.Count);

        //    foreach (Vector3 v in res)
        //    {

        //    }
        //    return res;
        //}

        public void SnapToNearestCell(Vector3 localDest, out int row, out int col, out Vector3 pos)
        {
           localDest -= root.center;
            row = (int) Math.Ceiling(localDest.Y);
            if (row <= 0) row = 49 + Math.Abs(row);
            else if (row > 0) row = 49 - row;;

            col = (int) Math.Ceiling(localDest.X);
            if (col >= 0) col += 49;
            else if (col < 0) col = 49 + col;
           
            pos = Vector3.Zero;
            if (row >= 49) pos.Y = (row - 49) * -1 + root.center.Y;
            else if (row < 49) pos.Y = (49 - row) + root.center.Y;
            else pos.Y = 0;

            if (col < 49) pos.X = (49 - col) * -1 + root.center.X;
            else if (col >= 49) pos.X = (col - 49) + root.center.X;
            else pos.X = 0;

            pos.Z = localDest.Z;

        
        }

        public void ConvertMapPointToCell(MapPoint p, out int row, out int col)
        {
            Vector3 local = new Vector3((float) p.X / 10, (float)p.Y / 10f, (float)p.Z / 10f);
            Vector3 res;
            SnapToNearestCell(local,out row, out col, out res);
        }
        public List<Vector3> SearchInSpiral2(Vector3 dest)
        {
            int[,] cost = new int[100, 100];
            Vector3[,] position = new Vector3[100, 100];
            for (int i = 0; i < 100; i++)
                for (int j = 0; j < 100; j++)
                    position[i, j] = Vector3.Zero;
            List<Vector3> res = new List<Vector3>();

           // res.Add(dest);
            Vector3 pos;
            int row, col;
            SnapToNearestCell(dest, out row, out col, out pos);
            System.Diagnostics.Debug.WriteLine("Snap to nearest Cell, dest " + dest + " row " + row + " col " + col + " pos " + pos);
            bool isFound = false;
            bool testing = true;
            while (true)
            {
                //System.Diagnostics.Debug.WriteLine("row " + row + " col " + col);
                cost[row, col] = -1;
                if (position[row, col].Equals(Vector3.Zero)) position[row, col] = pos;

                //if (testing) 
               //System.Diagnostics.Debug.WriteLine("pos " + position[row, col] + " row " + row + " col " + col);
               // if (testing)
             res.Add(pos);
             //   testing = false;

                isFound = false;
                int stepCost = int.MaxValue;
                for(int r = -1; r < 2; r++)
                {
                    for(int c = -1; c < 2; c++)
                    {
                        if (row + r >= 100 || row + r < 0 || col + c >= 100 || col + c < 0) continue;

                        if (position[row + r, col + c].Equals(Vector3.Zero))
                        {
                            position[row + r, col + c] = new Vector3(pos.X + c, pos.Y + r, pos.Z);
                           // System.Diagnostics.Debug.WriteLine("Updated pos: " + position[row + r, col + c]);
                        }

                        int currCost = 0;
                        if (cost[row + r, col + c] == 0)
                        {
                            if (LineOfSight(pos, position[row + r, col + c], false, false)) currCost = 10;
                            else currCost = int.MaxValue;

                            cost[row + r, col + c] = currCost;
                        }
                        else if (cost[row + r, col + c] == -1) continue;

                        if(stepCost > cost[row + r, col + c])
                        {

                            stepCost = currCost;
                            row = row + r;
                            col = col + c;
                        }

                        if (stepCost == 10)
                        {
                            isFound = true;
                            break;
                        }
                    }
                    if (isFound) break;
                }
                if (!isFound)
                {
                   // System.Diagnostics.Debug.WriteLine("neighbor is NOT FOUND");
                    break;
                }
                pos = position[row, col];
            }
            //foreach (Vector3 r in res)
            //    System.Diagnostics.Debug.WriteLine("res vectors: " + r);
            return res;
        }

       

            //    foreach (Vector3 v in path)
            //        System.Diagnostics.Debug.WriteLine("Path points are " + v);
            //    return path;
            //}

            //public List<Node> Find2DBoundingCornerGraphNodes(Vector3 position)
            //{
            //    List<Node> result = new List<Node>();
            //    OctreeNode node = Find(position);
            //    System.Diagnostics.Debug.WriteLine("The found octree node: " + node.center.ToString());

            //    if (node != null)
            //    {
            //        for (int i = 0; i < 8; i++)
            //        {
            //            int[] cornerIndex = new int[3];
            //            for (int j = 0; j < 3; j++)
            //            {
            //                cornerIndex[j] = (node.index[j] + cornerDir[i, j]) << (maxLevel - node.level);
            //                System.Diagnostics.Debug.WriteLine("cornerIndex[" + j + "] = (" + node.index[j] + " + " + cornerDir[i,j] + " << " + maxLevel + " - " + node.level + ") = " + cornerIndex[j]);
            //            }
            //            Node n = GetNodeFromDict(cornerIndex, cornerGraphDictionary);
            //            System.Diagnostics.Debug.WriteLine("The bounding graph node: " + n.center.ToString());
            //            result.Add(n);
            //        }
            //    }
            //    else throw new Exception("The source node is null.");
            //    return result;
            //}

            public List<Node> FindBoundingCrossedGraphNodes(Vector3 position)
        {
            List<Node> result = new List<Node>();
            OctreeNode node = Find(position);
            if (node != null && crossedGraphBoundingNodesDictionary.ContainsKey(node))
            {
                return new List<Node>(crossedGraphBoundingNodesDictionary[node]);
            }
            return result;
        }

        public List<OctreeNode> DisplayOctreeBlockedNodes(ref GraphicsOverlay overlay)
        {
            int printNodes = 100000000;
            HashSet<OctreeNode> res = new HashSet<OctreeNode>();
            Queue<OctreeNode> q = new Queue<OctreeNode>();
            q.Enqueue(root);

            while (q.Count > 0)
            {
                OctreeNode curr = q.Dequeue();
                res.Add(curr);
                if (printNodes >= 0 && curr.blocked)
                {
                    Vector3 center = curr.center;
                    MapPoint point = new MapPoint(center.X * 10, center.Y * 10, center.Z * 10, PathFinder.spatialRef);
                    Graphic graphicWithSymbol = new Graphic(point, blockedNodeSymbol);
                    overlay.Graphics.Add(graphicWithSymbol);

                    printNodes--;
                }

                if (curr.children != null)
                    foreach (OctreeNode child in curr.children) q.Enqueue(child);
            }
            return res.ToList();
        }

        public List<OctreeNode> DisplayOctreeNodes(ref GraphicsOverlay overlay)
        {
            int printNodes = 60000;
            HashSet<OctreeNode> res = new HashSet<OctreeNode>();
            Queue<OctreeNode> q = new Queue<OctreeNode>();
            q.Enqueue(root);

            while (q.Count > 0)
            {
                OctreeNode curr = q.Dequeue();
                res.Add(curr);
                if (printNodes >= 0)
                {
                    Vector3 center = curr.center;
                    MapPoint point = new MapPoint(center.X * 10, center.Y * 10, center.Z * 10, PathFinder.spatialRef);
                    Graphic graphicWithSymbol;
                    if (curr.blocked)
                        graphicWithSymbol = new Graphic(point, blockedNodeSymbol);
                    else
                        graphicWithSymbol = new Graphic(point, nodeSymbol);
                    overlay.Graphics.Add(graphicWithSymbol);

                    printNodes--;
                }

                if (curr.children != null)
                    foreach (OctreeNode child in curr.children) q.Enqueue(child);
            }
            return res.ToList();
        }

    }
    class OctreeNode
    {
        public Octree tree;
        public int level;
        public int[] index;
        public OctreeNode parent;
        public OctreeNode[,,] children;
        public bool visited = false;
        public bool blocked = false;
        public bool containsBlocked = false;

        private OctreeNode() { }
        public float size
        {
            get { return tree.size / (1 << level); }
        }
        public Vector3 center
        {
            get { return corners(0) + (size / 2) * Vector3.One; }
        }
        public Vector3 corners(int n)
        {
            return tree.corner + size * (new Vector3(index[0] + Octree.cornerDir[n, 0], index[1] + Octree.cornerDir[n, 1], index[2] + Octree.cornerDir[n, 2]));
        }
        public int[] cornerIndex(int n)
        {
            int s = 1 << (tree.maxLevel - level);
            return new int[] { (index[0] + Octree.cornerDir[n, 0]) * s, (index[1] + Octree.cornerDir[n, 1]) * s, (index[2] + Octree.cornerDir[n, 2]) * s };
        }

        public OctreeNode(int _level, int[] _index, OctreeNode _parent, Octree _tree)
        {
            level = _level;
            index = _index;
            parent = _parent;
            tree = _tree;
        }

        public virtual void CreateChildren()
        {
            if (children == null)
            {
                children = new OctreeNode[2, 2, 2];
                for (int xi = 0; xi < 2; xi++)
                    for (int yi = 0; yi < 2; yi++)
                        for (int zi = 0; zi < 2; zi++)
                        {
                            int[] newIndex = { index[0] * 2 + xi, index[1] * 2 + yi, index[2] * 2 + zi };
                            children[xi, yi, zi] = new OctreeNode(level + 1, newIndex, this, tree);
                        }
            }
        }

        public bool Contains(Vector3 p)
        {
            Vector3 pp = p - center;
            float r = tree.size / (1 << (level + 1));
            return MathF.Abs(pp.X) < r && MathF.Abs(pp.Y) < r && MathF.Abs(pp.Z) < r;
        }

        public bool IntersectLine(Vector3 p1, Vector3 p2, float tolerance = 0)
        {
            Vector3 c = center;
            float r = size / 2 - tolerance;
            p1 -= c;
            p2 -= c;

            float xm, xp, ym, yp, zm, zp;
            xm = MathF.Min(p1.X, p2.X);
            xp = MathF.Max(p1.X, p2.X);
            ym = MathF.Min(p1.Y, p2.Y);
            yp = MathF.Max(p1.Y, p2.Y);
            zm = MathF.Min(p1.Z, p2.Z);
            zp = MathF.Max(p1.Z, p2.Z);
            if (xm >= r || xp < -r || ym >= r || yp < -r || zm >= r || zp < -r) return false;

            for (int i = 0; i < 3; i++)
            {
                Vector3 a = Vector3.Zero;
                a.Set(i, 1);
                a = Vector3.Cross(a, p2 - p1);
                float d = MathF.Abs(Vector3.Dot(p1, a));
                float rr = r * (MathF.Abs(a.Get((i + 1) % 3)) + MathF.Abs(a.Get((i + 2) % 3)));
                if (d > rr) return false;
            }

            return true;
        }

        public bool IntersectTriangle(Vector3 p1, Vector3 p2, Vector3 p3, float tolerance = 0)
        {
            Vector3 c = center;
            //System.Diagnostics.Debug.WriteLine("INTERSECTTRIANGLE'S CENTER: " + c.ToString());
            float r = size / 2 - tolerance;
            //System.Diagnostics.Debug.WriteLine("radius r: " + r);
            p1 -= c;
            p2 -= c;
            p3 -= c;
            double xm, xp, ym, yp, zm, zp;
            xm = Math.Min(p1.X, Math.Min(p2.X, p3.X));
            xp = Math.Max(p1.X, Math.Max(p2.X, p3.X));
            ym = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
            yp = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
            zm = Math.Min(p1.Z, Math.Min(p2.Z, p3.Z));
            zp = Math.Max(p1.Z, Math.Max(p2.Z, p3.Z));
            if (xm >= r || xp < -r || ym >= r || yp < -r || zm >= r || zp < -r)
            {
                return false;
            }

            Vector3 n = Vector3.Cross(p2 - p1, p3 - p1);
            double d = Math.Abs(Vector3.Dot(p1, n));
            if (d > r * (Math.Abs(n.X) + Math.Abs(n.Y) + Math.Abs(n.Z))) return false;

            Vector3[] p = { p1, p2, p3 };
            Vector3[] f = { p3 - p2, p1 - p3, p2 - p1 };
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector3 a = Vector3.Zero;
                    a.Set(i, 1);
                    a = Vector3.Cross(a, f[j]);
                    double d1 = Vector3.Dot(p[j], a);
                    double d2 = Vector3.Dot(p[(j + 1) % 3], a);
                    double rr = r * (Math.Abs(a.Get((i + 1) % 3)) + Math.Abs(a.Get((i + 2) % 3)));
                    if (Math.Min(d1, d2) > rr || Math.Max(d1, d2) < -rr) return false;
                }
            }
            return true;
        }

        public bool IntersectSphere(Vector3 sphereCenter, float radius)
        {
            Vector3 c1 = corners(0);
            Vector3 c2 = corners(7);
            float r2 = radius * radius;
            for (int i = 0; i < 3; i++)
            {
                if (sphereCenter.Get(i) < c1.Get(i)) r2 -= (sphereCenter.Get(i) - c1.Get(i)) * (sphereCenter.Get(i) - c1.Get(i));
                else if (sphereCenter.Get(i) > c2.Get(i)) r2 -= (sphereCenter.Get(i) - c2.Get(i)) * (sphereCenter.Get(i) - c2.Get(i));
            }
            return r2 > 0;
        }

        public void DivideUntilLevel(Vector3 p, int maxLevel, bool markAsBlocked = false)
        {
            if (Contains(p))
            {
                containsBlocked = containsBlocked || markAsBlocked;
                if (level < maxLevel)
                {
                    CreateChildren();
                    Vector3 corner = corners(0);
                    int xi = (int)MathF.Floor((p.X - corner.X) * 2 / size);
                    int yi = (int)MathF.Floor((p.Y - corner.Y) * 2 / size);
                    int zi = (int)MathF.Floor((p.Z - corner.Z) * 2 / size);
                    children[xi, yi, zi].DivideUntilLevel(p, maxLevel, markAsBlocked);
                }
                else
                {
                    blocked = blocked || markAsBlocked;
                }
            }
        }

        public void DivideTriangleUntilLevel(Vector3 p1, Vector3 p2, Vector3 p3, int maxLevel, bool markAsBlocked = false)
        {
            if (IntersectTriangle(p1, p2, p3))
            {
                containsBlocked = containsBlocked || markAsBlocked;
                if (level < maxLevel)
                {
                    CreateChildren();

                    for (int xi = 0; xi < 2; xi++)
                        for (int yi = 0; yi < 2; yi++)
                            for (int zi = 0; zi < 2; zi++)
                            {
                                children[xi, yi, zi].DivideTriangleUntilLevel(p1, p2, p3, maxLevel, markAsBlocked);
                            }
                }
                else
                {
                    blocked = blocked || markAsBlocked;
                }
            }
        }

        public void DivideSphereUntilLevel(Vector3 sphereCenter, float radius, int maxLevel, bool markAsBlocked = false)
        {
            if (IntersectSphere(sphereCenter, radius))
            {
                containsBlocked = containsBlocked || markAsBlocked;
                if (level < maxLevel)
                {
                    CreateChildren();
                    for (int xi = 0; xi < 2; xi++)
                        for (int yi = 0; yi < 2; yi++)
                            for (int zi = 0; zi < 2; zi++)
                                children[xi, yi, zi].DivideSphereUntilLevel(sphereCenter, radius, maxLevel, markAsBlocked);
                }
                else
                {
                    blocked = blocked || markAsBlocked;
                }
            }
        }

        private void Leaves(List<OctreeNode> result)
        {
            if (children != null)
            {
                for (int xi = 0; xi < 2; xi++)
                {
                    for (int yi = 0; yi < 2; yi++)
                    {
                        for (int zi = 0; zi < 2; zi++)
                        {
                            children[xi, yi, zi].Leaves(result);
                        }
                    }
                }
            }
            else
            {
                result.Add(this);
            }
        }

        public List<OctreeNode> Leaves()
        {
            List<OctreeNode> result = new List<OctreeNode>();
            Leaves(result);
            return result;
        }
    }
}