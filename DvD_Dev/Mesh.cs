using MapPoint = Esri.ArcGISRuntime.Geometry.MapPoint;
using Polyline = Esri.ArcGISRuntime.Geometry.Polyline;
using Polygon  = Esri.ArcGISRuntime.Geometry.Polygon;
using SpatialReference = Esri.ArcGISRuntime.Geometry.SpatialReference;
using GeometryEngine = Esri.ArcGISRuntime.Geometry.GeometryEngine;
using LinearUnits = Esri.ArcGISRuntime.Geometry.LinearUnits;
using GeodeticCurveType = Esri.ArcGISRuntime.Geometry.GeodeticCurveType;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Mapping;
using System.Linq;

namespace DvD_Dev
{
    class Mesh
    {
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
                    //if (triangles[i + j] == 7) 
                    //    System.Diagnostics.Debug.WriteLine("BEFORE, 3 vertices of tri " + triVerts[0] + " " + triVerts[1] + " " + triVerts[2]
                    //       + " has normal " + normal + " cumulative normal at 7: " + normals[triangles[i + j]]);

                    normals[triangles[i + j]] += normal;
                    //if (triangles[i + j] == 7)
                    //    System.Diagnostics.Debug.WriteLine("AFTER, cumulative normal at 7: " + normals[triangles[i + j]]);
                }
            }

            List<Vector3> keys = new List<Vector3>(normalz.Keys);
            foreach (Vector3 key in keys)
                normalz[key] = Vector3.Normalize(normalz[key]);
            for(int k = 0; k < normals.Length; k++)
            {
                //normals[k] = Vector3.Normalize(normals[k]);
                //System.Diagnostics.Debug.WriteLine("normal: " + normals[v]);
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

        public void DisplayMeshGraphic(SceneView sceneView)
        {
            GraphicsOverlay overlay = new GraphicsOverlay();
            overlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            sceneView.GraphicsOverlays.Add(overlay);

            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.DarkGreen, 1.0);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.DarkOrchid, lineSymbol);
            SimpleLineSymbol normalLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 1.0);
            int numTri = triangles.Length / 3;
            TextSymbol textSymbol = new TextSymbol(numTri.ToString(), Color.Red, 20, HorizontalAlignment.Center, VerticalAlignment.Middle);
            
            for (int i = 0; i < triangles.Length; i += 3)
            {
                List<MapPoint> points = new List<MapPoint>();
                float[] zvalues = new float[3];
                for (int j = i; j <= i + 2; j++)
                {
                    Vector3 v = vertices[triangles[j]];
                    //System.Diagnostics.Debug.Write("Vertice " + v.ToString());
                    MapPoint p = new MapPoint(v.X * 10, v.Y * 10, v.Z * 10, spatialRef);
                    points.Add(p);

                    Vector3 n = normals[triangles[j]];
                   // System.Diagnostics.Debug.Write(" At Pos " + triangles[j] + " Normal " + n.ToString() + "\n");
                    n = v + n;
                    List<MapPoint> normalLine = new List<MapPoint>();
                    normalLine.Add(new MapPoint(p.X, p.Y, p.Z, spatialRef));
                    normalLine.Add(new MapPoint(n.X * 10, n.Y * 10, n.Z * 10, spatialRef));
                    Polyline line = new Polyline(normalLine);
                    Graphic lineGraphic = new Graphic(line, normalLineSymbol);
                    overlay.Graphics.Add(lineGraphic);
                    //System.Diagnostics.Debug.WriteLine("Length of normal: " + GeometryEngine.LengthGeodetic(line, LinearUnits.Meters, GeodeticCurveType.Geodesic)
                    //                                   + " normal from: " + normalLine[0].ToString() + " to: " + normalLine[1].ToString());

                    zvalues[j - i] = v.Z;
                }

                Graphic graphic;
                MapPoint first = points[0];
                points.Add(new MapPoint(first.X, first.Y, first.Z, spatialRef));
                
                if (Math.Abs(zvalues[0] - zvalues[1]) < 0.001 && Math.Abs(zvalues[1] - zvalues[2]) < 0.001) {
                    Polygon poly = new Polygon(points);
                    graphic = new Graphic(poly, fillSymbol);
                }
                else
                {
                    Polyline line = new Polyline(points);
                    graphic = new Graphic(line, fillSymbol);
                }
                overlay.Graphics.Add(graphic);
                // points.Add(new MapPoint(points[0].X, points[0].Y, points[0].Z, spatialRef));           
            }

            Graphic text = new Graphic();
            text.Symbol = textSymbol;
            text.Geometry = new MapPoint(vertices[0].X * 10, vertices[0].Y * 10, 200, spatialRef);
            //overlay.Graphics.Add(text);
;
        }

    }
}
