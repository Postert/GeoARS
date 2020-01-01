using System.Collections.Generic;
using UnityEngine;

public static class SurfaceTriangulator
{
    /// <summary>
    /// Triangular decomposition of the building surface, which is spanned by the vertices in the form of a polygon. All vertices must be in one plane. 
    /// The returned array contains indexes for the assignment of the vertices of the initial polygon involved in the formation of a triangle. Each three
    /// steps in the returned array vertices are assigned to a new triangle. 
    /// </summary>
    /// <param name="vertices">Vertices that span a planar surface in 3D space.</param>
    /// <returns>Array with indexes referencing the vertices of the area passed as parameter.</returns>
    public static int[] GetTriangles(Vector3[] vertices)
    {
        /// Calculate the surface normal of the planar polygon to determine its orientation in 3D space.
        Vector3 surfaceNormal = Vector3.Cross(vertices[1] - vertices[0], vertices[vertices.Length-1] - vertices[0]);
        surfaceNormal.Normalize();

        /// Calculate the Quaternion
        Quaternion rotation;

        if (surfaceNormal == Vector3.up)
        {
            /// Surface in x-y-plane, surface normal shows upwards in the direction of standard base
            rotation = Quaternion.identity;
        }
        else if (surfaceNormal == Vector3.down)
        {
            /// Surface in x-y-plane, surface normal shows downwards in the opposite direction of standard base
            rotation = Quaternion.identity;
        }
        else
        {
            /// Surface normal does not point in direction of standard base, rotation of surface required before triangular decomposition
			rotation = Quaternion.FromToRotation(surfaceNormal, Vector3.forward);
        }


        /// Create a new Vector2 list, after the rotation was performed and it was ensured that all vertices have the same z-value.
        List<Vector2> triangulationVertices = new List<Vector2>();
        foreach (Vector3 vertex in vertices)
        {
            Vector3 currentVertex =  vertex;
            // Debug.Log("\nRotated Vertex: x: " + currentVertex.x + ", y: " + currentVertex.y + ", z: " + currentVertex.z);
            triangulationVertices.Add(new Vector2(currentVertex.x, currentVertex.z)); ;
        }


        return Triangulator.Triangulate(triangulationVertices);
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
