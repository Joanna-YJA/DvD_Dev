using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using C5;
using Newtonsoft.Json;

namespace DvD_Dev
{
    class Node
    {
        public int index;
        public int connectIndex = 0;
        public List<Arc> arcs;
        public Vector3 center;
        public Node(Vector3 _center, int _index)
        {
            center = _center;
            index = _index;
            arcs = new List<Arc>();
        }
    }
    class Arc
    {
        public Node from, to;
        public float distance;

        private Arc() { }
        public Arc(Node _from, Node _to)
        {
            from = _from;
            to = _to;
            distance = (from.center - to.center).Length();
        }
        public Arc(Node _from, Node _to, float _distance)
        {
            from = _from;
            to = _to;
            distance = _distance;
        }
    }
    class NodeInfo : Node
    {
        public float f, g, h;
        public int indexTemp;
        public NodeInfo parent;
        public bool open = false;
        public bool closed = false;
        public IPriorityQueueHandle<NodeInfo> handle;
        public NodeInfo(Node node) : base(node.center, node.index)
        {
            arcs = node.arcs;
        }
    }

    class NodeFComparer : IComparer<NodeInfo>
    {
        public int Compare(NodeInfo x, NodeInfo y)
        {
            return x.f - y.f > 0 ? 1 : (x.f - y.f < 0 ? -1 : 0);
        }
    }
    class Graph
    {
        public List<Node> nodes;
        public List<Node> temporaryNodes;
        public enum GraphType
        {
            CENTER,
            CORNER,
            CROSSED,
            OTHER
        }
        public GraphType type = GraphType.OTHER;

        public Graph()
        {
            nodes = new List<Node>();
            temporaryNodes = new List<Node>();
        }
        public int AddNode(Vector3 center)
        {
            nodes.Add(new Node(center, nodes.Count));
            return nodes.Count - 1;
        }

        public void AddArc(int fromIndex, int toIndex)
        {
            nodes[fromIndex].arcs.Add(new Arc(nodes[fromIndex], nodes[toIndex]));
        }

        public void CalculateConnectivity()
        {
            foreach (Node node in nodes)
            {
                node.connectIndex = 0;
            }
            int current = 1;
            foreach (Node node in nodes)
            {
                if (node.connectIndex == 0)
                {
                    node.connectIndex = current;
                    Queue<Node> toSet = new Queue<Node>();
                    toSet.Enqueue(node);
                    while (toSet.Count > 0)
                    {
                        Node next = toSet.Dequeue();
                        foreach (Arc arc in next.arcs)
                        {
                            if (arc.to.connectIndex != current)
                            {
                                arc.to.connectIndex = current;
                                toSet.Enqueue(arc.to);
                            }
                        }
                    }
                    current++;
                }
            }
        }

        public Node AddTemporaryNode(Vector3 position, List<Node> neighbors)
        {
            Node newNode = new Node(position, -1 - temporaryNodes.Count);
            temporaryNodes.Add(newNode);
            foreach (Node neighbor in neighbors)
            {
                float minDist2 = float.MaxValue;
                if (neighbor != null)
                {
                    newNode.arcs.Add(new Arc(newNode, neighbor));
                    neighbor.arcs.Add(new Arc(neighbor, newNode));
                    float d2 = (neighbor.center - newNode.center).LengthSquared();
                    if (d2 < minDist2)
                    {
                        newNode.connectIndex = neighbor.connectIndex;
                        minDist2 = d2;
                    }
                }
            }
            return newNode;
        }

        public void RemoveTemporaryNodes()
        {
            foreach (Node node in temporaryNodes)
            {
                foreach (Arc arc in node.arcs)
                {
                    List<Arc> originalArcs = new List<Arc>();
                    foreach (Arc neighborArc in arc.to.arcs)
                    {
                        if (neighborArc.to != node)
                        {
                            originalArcs.Add(neighborArc);
                        }
                    }
                    arc.to.arcs = originalArcs;
                }
                node.arcs.Clear();
            }
            temporaryNodes.Clear();
        }

        public delegate float H(Node from, Node to);
        public float estimatedCost(Node from, Node to)
        {
            return (from.center - to.center).Length();
        }

        public List<Node> Backtrack(NodeInfo node)
        {
            List<Node> temp = new List<Node>();
            temp.Add(node);
            while (node.parent != null)
            {
                //System.Diagnostics.Debug.WriteLine("For node " + node.center + " ,Adding node's parent " + node.parent.center + " to List<Node>");
                temp.Add(node.parent);
                node = node.parent;
            }
            int n = temp.Count;
            List<Node> result = new List<Node>();
            for (int i = 0; i < n; i++)
            {
                result.Add(temp[n - 1 - i]);
            }
            return result;
        }

        public delegate List<List<Node>> PathFindingMethod(Node source, List<Node> destinations, Octree space, H h = null);

        public List<Node> FindPath(PathFindingMethod method, Node source, Node destination, Octree space, H h = null)
        {
            return FindPath(method, source, new List<Node>() { destination }, space, h)[0];
        }
        public List<List<Node>> FindPath(PathFindingMethod method, Node source, List<Node> destinations, Octree space, H h = null)
        {
            return method(source, destinations, space, h);
        }
        public List<Node> FindPath(PathFindingMethod method, Vector3 source, Vector3 destination, Octree space, H h = null)
        {
            return FindPath(method, source, new List<Vector3>() { destination }, space, h)[0];
        }
        public List<List<Node>> FindPath(PathFindingMethod method, Vector3 source, List<Vector3> destinations, Octree space, H h = null)
        {
            //foreach (Node n in this.nodes)
            //{
            //    if (n.center.Equals(new Vector3(-58.625f, 10.625f, 141.375f)))
            //    {
            //        foreach (Arc a in n.arcs)
            //            System.Diagnostics.Debug.WriteLine(n.index + ")In Graph find path, Target node n is connected to " + a.to.center);
            //    }
            //}
            //System.Diagnostics.Debug.WriteLine("When finding path from " + source + " to [" + String.Join(". ", destinations) + "]");
            List<Node> sourceNeighbors = null;
            //if (space.Find(source).blocked) return new List<List<Node>>();
            if (type == GraphType.CENTER)
            {
                sourceNeighbors = space.FindCorrespondingCenterGraphNode(source);
            }
            else if (type == GraphType.CORNER)
            {
                sourceNeighbors = space.FindBoundingCornerGraphNodes(source);
            }
            else if (type == GraphType.CROSSED)
            {
                sourceNeighbors = space.FindBoundingCrossedGraphNodes(source);
            }
            //System.Diagnostics.Debug.Write("sourceNeighbors: [");
            //foreach (Node sn in sourceNeighbors)
            //    System.Diagnostics.Debug.Write(sn.center + ", ");
            //System.Diagnostics.Debug.Write("]\n");



            Node tempSourceNode = AddTemporaryNode(source, sourceNeighbors);
            List<Node> tempDestinationNodes = new List<Node>();
            foreach (Vector3 destination in destinations)
            {
                List<Node> destinationNeighbors = null;

                if (type == GraphType.CENTER)
                {
                    destinationNeighbors = space.FindCorrespondingCenterGraphNode(destination);
                }
                else if (type == GraphType.CORNER)
                {
                    destinationNeighbors = space.FindBoundingCornerGraphNodes(destination);
                }
                else if (type == GraphType.CROSSED)
                {
                    destinationNeighbors = space.FindBoundingCrossedGraphNodes(destination);
                }

                //System.Diagnostics.Debug.WriteLine("type: " + type);
                //System.Diagnostics.Debug.Write("destinationNeighbors: [");
                //foreach (Node dn in destinationNeighbors)
                //{
                //    if (dn != null) System.Diagnostics.Debug.Write(dn.center + ", ");
                //}
                //System.Diagnostics.Debug.Write("]\n");

                tempDestinationNodes.Add(AddTemporaryNode(destination, destinationNeighbors));
            }
            //System.Diagnostics.Debug.Write("sourceNeighbors: [");
            //foreach (Node sn in sourceNeighbors)
            //    System.Diagnostics.Debug.Write(sn.center + ", ");
            //System.Diagnostics.Debug.Write("]\n");

            List <List<Node>> result = FindPath(method, tempSourceNode, tempDestinationNodes, space, h);
            RemoveTemporaryNodes();

            return result;
        }


        public List<List<Node>> AStar(Node source, List<Node> destinations, Octree space, H h = null)
        {
            if (h == null)
                h = estimatedCost;
            List<List<Node>> result = new List<List<Node>>();

            Dictionary<int, NodeInfo> infoTable = new Dictionary<int, NodeInfo>();
            NodeInfo sourceInfo = new NodeInfo(source);
            infoTable[sourceInfo.index] = sourceInfo;

            IntervalHeap<NodeInfo> open = new IntervalHeap<NodeInfo>(new NodeFComparer());
            sourceInfo.open = true;
            sourceInfo.g = 0;

            for (int i = 0; i < destinations.Count; i++)
            {
                Node destination = destinations[i];
                if (i == 0)
                {
                    sourceInfo.f = h(source, destination);
                    open.Add(ref sourceInfo.handle, sourceInfo);
                }
                else
                {
                    NodeInfo destInfo;
                    if (infoTable.TryGetValue(destination.index, out destInfo) && destInfo.closed)
                    {
                        result.Add(Backtrack(destInfo));
                        continue;
                    }
                }
                if (source.connectIndex != destination.connectIndex)
                {
                    result.Add(null);
                    continue;
                }

                if (i > 0)
                {
                    IntervalHeap<NodeInfo> newOpen = new IntervalHeap<NodeInfo>(new NodeFComparer());
                    foreach (NodeInfo n in open)
                    {
                        n.f = n.g + h(n, destination);
                        n.handle = null;
                        newOpen.Add(ref n.handle, n);
                    }
                    open = newOpen;
                }
                NodeInfo current = null;
                while (open.Count > 0)
                {
                    current = open.DeleteMin();
                    current.open = false;
                    current.closed = true;
                    if (current.index == destination.index) break;
                    foreach (Arc a in current.arcs)
                    {
                        NodeInfo successor;
                        if (!infoTable.TryGetValue(a.to.index, out successor))
                        {
                            successor = new NodeInfo(a.to);
                            successor.g = float.MaxValue;
                            successor.h = h(successor, destination);
                            infoTable[a.to.index] = successor;
                        }
                        if (!successor.closed)
                        {
                            float g_old = successor.g;
                            // ComputeCost
                            if (successor.g > current.g + a.distance)
                            {
                                successor.parent = current;
                                successor.g = current.g + a.distance;
                                successor.f = successor.g + successor.h;
                            } //
                            if (successor.g < g_old)
                            {
                                if (successor.open)
                                    open.Delete(successor.handle);
                                open.Add(ref successor.handle, successor);
                                successor.open = true;
                            }
                        }
                    }
                }
                if (current == null || current.index != destination.index)
                {
                    result.Add(null);
                    continue;
                }
                result.Add(Backtrack(current));
                open.Add(ref current.handle, current);
            }
            return result;
        }


        public List<List<Node>> ThetaStar(Node source, List<Node> destinations, Octree space, H h = null)
        {
            //float t = Time.realtimeSinceStartup;
            int nodeCount = 0;
            int newNodeCount = 0;

            if (h == null)
                h = estimatedCost;
            List<List<Node>> result = new List<List<Node>>();

            Dictionary<int, NodeInfo> infoTable = new Dictionary<int, NodeInfo>();
            NodeInfo sourceInfo = new NodeInfo(source);
            infoTable[sourceInfo.index] = sourceInfo;

            IntervalHeap<NodeInfo> open = new IntervalHeap<NodeInfo>(new NodeFComparer());
            sourceInfo.open = true;
            sourceInfo.g = 0;

            for (int i = 0; i < destinations.Count; i++)
            {
                Node destination = destinations[i];
                if (i == 0)
                {
                    sourceInfo.f = h(source, destination);
                    open.Add(ref sourceInfo.handle, sourceInfo);
                }
                else
                {
                    NodeInfo destInfo;
                    if (infoTable.TryGetValue(destination.index, out destInfo) && destInfo.closed)
                    {
                        result.Add(Backtrack(destInfo));
                        continue;
                    }
                }
                if (source.connectIndex != destination.connectIndex)
                {
                    result.Add(null);
                    continue;
                }

                if (i > 0)
                {
                    IntervalHeap<NodeInfo> newOpen = new IntervalHeap<NodeInfo>(new NodeFComparer());
                    foreach (NodeInfo n in open)
                    {
                        n.f = n.g + h(n, destination);
                        n.handle = null;
                        newOpen.Add(ref n.handle, n);
                    }
                    open = newOpen;
                }

                NodeInfo current = null;
                while (open.Count > 0)
                {
                    nodeCount++;
                    current = open.DeleteMin();
                    current.open = false;
                    current.closed = true;
                    if (current.index == destination.index) break;
                    foreach (Arc a in current.arcs)
                    {
                        NodeInfo successor;
                        if (!infoTable.TryGetValue(a.to.index, out successor))
                        {
                            newNodeCount++;
                            successor = new NodeInfo(a.to);
                            successor.g = float.MaxValue;
                            successor.h = h(successor, destination);
                            infoTable[a.to.index] = successor;
                        }
                        if (!successor.closed)
                        {
                            float g_old = successor.g;
                            // ComputeCost
                            NodeInfo parent = current;
                            if (parent.parent != null && space.LineOfSight(parent.parent.center, successor.center, false, type == GraphType.CENTER))
                            {
                                parent = parent.parent;
                            }
                            float gNew = parent.g + (successor.center - parent.center).Length();
                            if (successor.g > gNew)
                            {
                                successor.parent = parent;
                                successor.g = gNew;
                                successor.f = successor.g + successor.h;
                            } //
                            if (successor.g < g_old)
                            {
                                if (successor.open)
                                    open.Delete(successor.handle);
                                open.Add(ref successor.handle, successor);
                                successor.open = true;
                            }
                        }
                    }
                }

                if (current == null || current.index != destination.index)
                {
                    result.Add(null);
                    continue;
                }
                NodeInfo check = current;
                while (check.parent != null)
                {
                    while (check.parent.parent != null && space.LineOfSight(check.parent.parent.center, check.center, false, type == GraphType.CENTER))
                    {
                        check.parent = check.parent.parent;
                    }
                    check = check.parent;
                }
                result.Add(Backtrack(current));
                open.Add(ref current.handle, current);
            }
            //System.Diagnostics.Debug.WriteLine("time: " + (Time.realtimeSinceStartup - t) + " NodeCount: " + nodeCount + " NewNodeCount: " + newNodeCount);
            return result;
        }


        public List<List<Node>> LazyThetaStar(Node source, List<Node> destinations, Octree space, H h = null)
        {
            Node testFocus = null;
            foreach (Node n in this.nodes)
            {
                //if (n.center.Equals(new Vector3(-57.06298f, 10f, 142.9338f)))
                if(n.index == -1)
                {
                    testFocus = n;
                    //foreach (Arc a in n.arcs)
                    //    System.Diagnostics.Debug.WriteLine(n.index + ")In Lazy Theta Star, Target node n is connected to " + a.to.center);
                }
            }
            //float t = Time.realtimeSinceStartup;
            int nodeCount = 0;
            int newNodeCount = 0;

            if (h == null)
                h = estimatedCost;
            List<List<Node>> result = new List<List<Node>>();

            Dictionary<int, NodeInfo> infoTable = new Dictionary<int, NodeInfo>();
            NodeInfo sourceInfo = new NodeInfo(source);
            infoTable[sourceInfo.index] = sourceInfo;

            IntervalHeap<NodeInfo> open = new IntervalHeap<NodeInfo>(new NodeFComparer());
            sourceInfo.open = true;
            sourceInfo.g = 0;

            for (int i = 0; i < destinations.Count; i++)
            {
                Node destination = destinations[i];
                if (i == 0)
                {
                    sourceInfo.f = h(source, destination);
                    open.Add(ref sourceInfo.handle, sourceInfo);
                }
                else
                {
                    NodeInfo destInfo;
                    if (infoTable.TryGetValue(destination.index, out destInfo) && destInfo.closed)
                    {
                        result.Add(Backtrack(destInfo));
                        continue;
                    }
                }
                if (source.connectIndex != destination.connectIndex)
                {
                    result.Add(null);
                    continue;
                }

                if (i > 0)
                {
                    IntervalHeap<NodeInfo> newOpen = new IntervalHeap<NodeInfo>(new NodeFComparer());
                    foreach (NodeInfo n in open)
                    {
                        n.f = n.g + h(n, destination);
                        n.handle = null;
                        newOpen.Add(ref n.handle, n);
                    }
                    open = newOpen;
                }

                NodeInfo current = null;
                while (open.Count > 0)
                {
                    nodeCount++;
                    current = open.DeleteMin();
                    current.open = false;
                    current.closed = true;
                    // SetVertex
                    if (current.parent != null && !space.LineOfSight(current.parent.center, current.center, false, type == GraphType.CENTER))
                    {
                        NodeInfo realParent = null;
                        float realg = float.MaxValue;
                        foreach (Arc a in current.arcs)
                        {
                            NodeInfo tempParent;
                            float tempg;
                            if (infoTable.TryGetValue(a.to.index, out tempParent) && tempParent.closed)
                            {
                                tempg = tempParent.g + (current.center - tempParent.center).Length();
                                if (tempg < realg)
                                {
                                    realParent = tempParent;
                                    realg = tempg;
                                }
                            }
                        }
                        current.parent = realParent;
                        current.g = realg;
                    } //
                    if (current.index == destination.index) break;
                    //System.Diagnostics.Debug.WriteLine("current.index " + current.index + " Arcs connnected to current node " + current.center);
                    //System.Diagnostics.Debug.WriteLine("testFocus == null? " + (testFocus == null) + " testFocus == current? " + (testFocus == current) + " testFocus.center " + testFocus.center);
                    //foreach (Arc ac in current.arcs)
                    //    System.Diagnostics.Debug.WriteLine(ac.to.center + ", ");
                    //System.Diagnostics.Debug.WriteLine("]\n");

                    foreach (Arc a in current.arcs)
                    {
                        NodeInfo successor;
                        if (!infoTable.TryGetValue(a.to.index, out successor))
                        {
                            newNodeCount++;
                            successor = new NodeInfo(a.to);
                            //System.Diagnostics.Debug.WriteLine("Does a.t/succesor " + successor.center + " has any arcs? arc count: " + a.to.arcs.Count);
                            //System.Diagnostics.Debug.WriteLine("Index of successor node: " + successor.index);
                            successor.g = float.MaxValue;
                            successor.h = h(successor, destination);
                            infoTable[a.to.index] = successor;
                        }
                        if (!successor.closed)
                        {
                            float g_old = successor.g;
                            // ComputeCost
                            NodeInfo parent = current.parent == null ? current : current.parent;
                            float gNew = parent.g + (successor.center - parent.center).Length();
                            if (successor.g > gNew)
                            {
                                //System.Diagnostics.Debug.WriteLine("successor.g > gNew for successor " + successor.center + " and parent " + parent.center);
                                successor.parent = parent;
                                successor.g = gNew;
                                successor.f = successor.g + successor.h;
                            } //
                            if (successor.g < g_old)
                            {
                                if (successor.open)
                                    open.Delete(successor.handle);
                               //System.Diagnostics.Debug.WriteLine("Node's successor " + successor.center + " is added back to heap");
                                open.Add(ref successor.handle, successor);
                                successor.open = true;
                            }
                        }
                    }
                }
                if (current == null || current.index != destination.index)
                {
                    result.Add(null);
                    continue;
                }
                NodeInfo check = current;
                while (check.parent != null)
                {
                    while (check.parent.parent != null && space.LineOfSight(check.parent.parent.center, check.center, false, type == GraphType.CENTER))
                    {
                        check.parent = check.parent.parent;
                    }
                    check = check.parent;
                }
                result.Add(Backtrack(current));
                open.Add(ref current.handle, current);
            }
            //System.Diagnostics.Debug.WriteLine("time: " + (Time.realtimeSinceStartup - t) + " NodeCount: " + nodeCount + " NewNodeCount: " + newNodeCount);
            //System.Diagnostics.Debug.WriteLine("Result of Lazy Theta Star: [");
            //foreach (List<Node> lr in result)
            //{
            //    if (lr != null)
            //        foreach (Node r in lr)
            //        {
            //            if (r != null) System.Diagnostics.Debug.WriteLine(r.center + ", ");
            //        }
            //}
            //System.Diagnostics.Debug.WriteLine("]\n");
            return result;
        }
    }
}


