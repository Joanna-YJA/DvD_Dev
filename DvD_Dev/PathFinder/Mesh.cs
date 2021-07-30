using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using MapPoint = Esri.ArcGISRuntime.Geometry.MapPoint;
using Polyline = Esri.ArcGISRuntime.Geometry.Polyline;
using SpatialReference = Esri.ArcGISRuntime.Geometry.SpatialReference;

namespace DvD_Dev
{
    class Mesh
    {
        static SimpleLineSymbol normalLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 1.0);
        static SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.DarkGreen, 1.0);
        static SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.DarkOrchid, lineSymbol);
        static TextSymbol textSymbol = new TextSymbol("", Color.Red, 20, HorizontalAlignment.Center, VerticalAlignment.Middle);

        SpatialReference spatialRef;

        public Vector3[] vertices;
        public int[] triangles;
        public Dictionary<Vector3, Vector3> normalz;
        public Vector3[] normals;

        public Mesh(Geometry surface, double height, SpatialReference spatialRef)
        {
            // convert polygon to triangles
            this.spatialRef = spatialRef;
            Triangulate(surface);
            ExtrudeMesh(height);
        }

        public void Triangulate(Geometry surface)
        {
            Dictionary<Coordinate, int> indDict = new Dictionary<Coordinate, int>();
            vertices = new Vector3[surface.NumPoints];
            for (int i = 0; i < vertices.Length; i++)
            {
                Coordinate c = surface.Coordinates[i];
                indDict[c] = i;
                vertices[i] = new Vector3((float) c.X / 10, (float) c.Y / 10, (float) c.Z / 10);
            }
   
            var builder = new ConformingDelaunayTriangulationBuilder();
            builder.SetSites(surface);
            GeometryCollection collection = builder.GetTriangles(new GeometryFactory());

            List<int> triIndices = new List<int>();
            foreach(Geometry tri in collection){
                triIndices.Add(indDict[tri.Coordinates[0]]);
                triIndices.Add(indDict[tri.Coordinates[1]]);
                triIndices.Add(indDict[tri.Coordinates[2]]);
                }
            triangles = triIndices.ToArray();
         }

        public void CalculateNormals()
        {
            normalz = new Dictionary<Vector3, Vector3>();
            normals = new Vector3[vertices.Length];
            for (int q = 0; q < normals.Length; q++) normals[q] = Vector3.Zero;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3[] triVerts = new Vector3[3];
                triVerts[0] = vertices[triangles[i]];
                triVerts[1] = vertices[triangles[i + 1]];
                triVerts[2] = vertices[triangles[i + 2]];

                Vector3 normal = Vector3.Cross(triVerts[2] - triVerts[0],
                                               triVerts[1] - triVerts[0]);

                for(int j = 0; j < 3; j++)
                {
                    if (!normalz.ContainsKey(triVerts[j])) normalz[triVerts[j]] = Vector3.Zero;
                    normalz[triVerts[j]] += normal;
                    normals[triangles[i + j]] += normal;
 
                }
            }

            List<Vector3> keys = new List<Vector3>(normalz.Keys);
            foreach (Vector3 key in keys)
                normalz[key] = Vector3.Normalize(normalz[key]);
            for(int k = 0; k < normals.Length; k++)
            {
                normals[k] = normalz[vertices[k]];
            }


        }

        public void ExtrudeMesh(double height)
        {
            Vector3[] newVertices = new Vector3[vertices.Length * 2];

            for (int i = 0; i < vertices.Length; i++)
            {
                newVertices[i].X = vertices[i].X;
                newVertices[i].Y = vertices[i].Y;
                newVertices[i].Z = 0; // front vertex
                newVertices[i + vertices.Length].X = vertices[i].X;
                newVertices[i + vertices.Length].Y = vertices[i].Y;
                newVertices[i + vertices.Length].Z = (float) height / 10;  // back vertex    
            }
            int[] newTriangles = new int[triangles.Length * 2 + vertices.Length * 6];
            int count_triangles = 0;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                newTriangles[i] = triangles[i];
                newTriangles[i + 1] = triangles[i + 1];
                newTriangles[i + 2] = triangles[i + 2];
            } // front vertices
            count_triangles += triangles.Length;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                newTriangles[count_triangles + i] = triangles[i + 2] + vertices.Length;
                newTriangles[count_triangles + i + 1] = triangles[i + 1] + vertices.Length;
                newTriangles[count_triangles + i + 2] = triangles[i] + vertices.Length;
            } // back vertices
            count_triangles += triangles.Length;
            for (int i = 0; i < vertices.Length; i++)
            {
                // triangles around the perimeter of the object
                int n = (i + 1) % vertices.Length;
                newTriangles[count_triangles] = i;
                newTriangles[count_triangles + 1] = n;
                newTriangles[count_triangles + 2] = i + vertices.Length;
                newTriangles[count_triangles + 3] = n;
                newTriangles[count_triangles + 4] = n + vertices.Length;
                newTriangles[count_triangles + 5] = i + vertices.Length;
                count_triangles += 6;
            }
            this.vertices = newVertices;
            this.triangles = newTriangles;
            this.CalculateNormals();
        }

        public void DisplayMesh(ref GraphicsOverlay overlay)
        { 
            
            for (int i = 0; i < triangles.Length; i += 3)
            {
                List<MapPoint> points = new List<MapPoint>();
                float[] zvalues = new float[3];
                for (int j = i; j <= i + 2; j++)
                {
                    Vector3 v = vertices[triangles[j]];
                    MapPoint p = new MapPoint(v.X * 10, v.Y * 10, v.Z * 10, spatialRef);
                    points.Add(p);

                    zvalues[j - i] = v.Z;
                }

                Graphic graphic;
                MapPoint first = points[0];
                points.Add(new MapPoint(first.X, first.Y, first.Z, spatialRef));
                

                    Polyline line = new Polyline(points);
                    graphic = new Graphic(line, fillSymbol);
                overlay.Graphics.Add(graphic);       
            }
        }

        public void DisplayMeshNormals(ref GraphicsOverlay overlay)
        {
            for (int i = 0; i < normals.Length; i ++)
            {           
                Vector3 v = vertices[i];
 
                Vector3 n = normals[i];
                n = v + n;
                List<MapPoint> normalLine = new List<MapPoint>();
                normalLine.Add(new MapPoint(v.X * 10, v.Y * 10, v.Z * 10, spatialRef));
                normalLine.Add(new MapPoint(n.X * 10, n.Y * 10, n.Z * 10, spatialRef));
                Polyline line = new Polyline(normalLine);
                Graphic lineGraphic = new Graphic(line, normalLineSymbol);
                overlay.Graphics.Add(lineGraphic);            
            }
        }

    }
}
