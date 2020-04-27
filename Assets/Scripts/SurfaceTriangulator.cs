using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;



public static class SurfaceTriangulator
{
    /// <summary>
    /// Triangular decomposition of the building surface, which is spanned by the vertices in the form of a polygon. All vertices must be in one plane. 
    /// The returned array contains indexes for the assignment of the vertices of the initial polygon involved in the formation of a triangle. Each three
    /// steps in the returned array vertices are assigned to a new triangle. 
    /// </summary>
    /// <param name="unitySurfaceCoordinates">Vertices that span a planar surface in 3D space.</param>
    /// <returns>Array with indexes referencing the vertices of the area passed as parameter.</returns>
    public static int[] GetTriangles(Vector3[] unitySurfaceCoordinates, Surface surface)
    {
        // Check for sufficient vertices
        if (unitySurfaceCoordinates.Length < 3)
        {


            // TODO: hier fehlten Daten, die im CityGML-Datensatz richtig eingelesen wurden aber nicht in der Datenbank sind!!!!!!


            return null;
            //throw new MissingComponentException("A surface must consist of at least three vertices. Surface CityGML ID: " + surface.CityGMLID);
        }

        Vector3 surfaceNormal = surface.GetSurfaceNormal();

        /// Calculate the Quaternion
        Quaternion rotation = Quaternion.identity;

        rotation = Quaternion.FromToRotation(surfaceNormal, Vector3.down);
     
        /// Create a new Vector2 list, after the rotation was performed and it was ensured that all vertices have the same z-value.
        List<Vector2> surface2DCoordinates = new List<Vector2>();
        foreach (Vector3 vertex in unitySurfaceCoordinates)
        {
            Vector3 currentVertex = vertex;

            currentVertex = rotation * currentVertex;

            surface2DCoordinates.Add(new Vector2(currentVertex.x, currentVertex.z)); ;
            //Debug.Log("\nRotated Vertex: x: " + currentVertex.x + ", y: " + currentVertex.y + ", z: " + currentVertex.z);
        }
        

/*
        List<Vector2> surface2DCoordinates = GetPlaneCoordinates(vertices);

        foreach (Vector2 surface2DCoordinate in surface2DCoordinates)
        {
            Debug.Log(surface2DCoordinate.ToString() + "\n");
        }
*/
        return Triangulator.Triangulate(surface2DCoordinates);
    }



    public static Mesh GetInvertedMesh(Mesh mesh)
    {
        int[] indices = mesh.triangles;
        int triangleCount = indices.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            var tmp = indices[i * 3];
            indices[i * 3] = indices[i * 3 + 1];
            indices[i * 3 + 1] = tmp;
        }
        mesh.triangles = indices;
        // additionally flip the vertex normals to get the correct lighting
        var normals = mesh.normals;
        for (var n = 0; n < normals.Length; n++)
        {
            normals[n] = -normals[n];
        }
        mesh.normals = normals;

        return mesh;
    }



    public static List<Vector2> GetPlaneCoordinates(Vector3[] vertices3D)
    {

        // TODO: korriegieren
        /* For a dimensional reduction from 3D to 2D space a transformation is necessary:
         * Define new basis vectors to perform the intended projection
         * b0 = x-axis by determining the directional vector from the first so the second point of the polygon
         * b1 = y-axis by interating over the polygon points until a directional vector from the first point to the current point is found, which is linearly independent from b0
         * b2 = polygon's normal vector treated as the z-axis (with all z-components = 0); by determing the cross product of b0 and b1
         */

        Vector3 e1 = new Vector3(1,0,0);
        Vector3 surfaceNormal = Vector3.zero;
        Vector3 u;
        Vector3 v;

        //List<Vector3> surfacePoints = vertices3D;

        // dertermination of the surface normal
        Vector3 directionalVectorsP1ToP2;
        directionalVectorsP1ToP2 = vertices3D[1] - vertices3D[0];
        directionalVectorsP1ToP2 = math.normalizesafe(directionalVectorsP1ToP2);

        foreach (Vector3 polygonPoint in vertices3D)
        {
            Vector3 potentialSurfaceNormal;
            potentialSurfaceNormal = math.cross(directionalVectorsP1ToP2, polygonPoint);
            potentialSurfaceNormal = math.normalizesafe(potentialSurfaceNormal);

            if (potentialSurfaceNormal.Equals(directionalVectorsP1ToP2))
            {
                if (Array.IndexOf(vertices3D, polygonPoint) == vertices3D.Length - 1)
                {
                    throw new ArgumentException("Surface is not a plane polygon.");
                }
                continue;
            }
            else
            {
                // first directional vector detected which is linearly independent from b0
                surfaceNormal = potentialSurfaceNormal;
                break;
            }
        }

        //directionalVectors[2] = math.cross(directionalVectors[0], directionalVectors[1]);
        //directionalVectors[2] = math.normalizesafe(directionalVectors[2]); ;

        Debug.Log("SurfaceNormal: " + surfaceNormal);

        // TODO: 
        // determin dot product (Skalarprodukt)

        // determin scalar product, if ==1 => 
        float sp = math.dot(surfaceNormal, e1);
        if (sp == 1)
        {
            u = new Vector3(0, 0, 1);
        }
        else
        {
            u = e1 - (sp * surfaceNormal);
            u = math.normalize(u);
        }

        v = math.cross(surfaceNormal, u);



        // proceed projection for each surface point

        List<Vector2> projected2DPoints = new List<Vector2>();

        foreach (Vector3 surfacePoint in vertices3D)
        {
            Vector2 projected2DPoint = new Vector2();
            projected2DPoint.x = Vector3.Dot(surfacePoint, u);
            projected2DPoint.y = math.dot(surfacePoint, v);

            projected2DPoints.Add(projected2DPoint);
        }

        return projected2DPoints;
    }



    /// <summary>
    /// Class for triangular decomposition of a polygon.
    /// Modified Unity Trinagulator, available on http://wiki.unity3d.com/index.php/Triangulator
    /// </summary>
    private static class Triangulator
    {
        /// <summary>
        /// Performs a triangle decomposition of the given polygon and returns the indexes for referencing the initial points spanning the triangles in an interger array. 
        /// This must be a planar polygon in 2D space. A new triangle starts every three steps in the array. 
        /// </summary>
        /// <param name="vertices">Vertices spanning the considered polygon</param>
        /// <returns>Indexes to describe the triangles</returns>
        public static int[] Triangulate(List<Vector2> vertices)
        {
            List<int> indices = new List<int>();

            int n = vertices.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area(vertices) > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(vertices, u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private static float Area(List<Vector2> vertices)
        {
            int n = vertices.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = vertices[p];
                Vector2 qval = vertices[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private static bool Snip(List<Vector2> vertices, int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = vertices[V[u]];
            Vector2 B = vertices[V[v]];
            Vector2 C = vertices[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = vertices[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private static bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}
