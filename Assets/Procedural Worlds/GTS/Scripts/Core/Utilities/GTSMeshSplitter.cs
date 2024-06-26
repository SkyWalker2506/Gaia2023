using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.GTS
{
    public class GTSMeshSplitter
    {
        public enum TranslatePivotType
        {
            None = 0,
            MinimumXZ = 1,
            CenterXZ = 2
        }

        #region Member Variables

        private readonly Vector3[] vertices;
        private readonly Vector3[] normals;
        private readonly Vector4[] tangents;
        private readonly Vector2[] uvs;
        private readonly int[] indices;
        private Bounds bounds;

        #endregion Member Variables

        #region Constructors

        public GTSMeshSplitter(Mesh mesh)
        {
            if (mesh == null)
                throw new Exception("Mesh is null.");
            if (mesh.vertexCount < 3)
                throw new Exception("Vertex count < 3.");
            vertices = mesh.vertices;
            normals = mesh.normals;
            if (normals == null || normals.Length == 0)
            {
                mesh.RecalculateNormals();
                normals = mesh.normals;
            }

            if (normals.Length != vertices.Length)
                throw new Exception("Normals count != vertex count.");
            tangents = mesh.tangents;
            if (tangents == null || tangents.Length == 0)
            {
                mesh.RecalculateTangents();
                tangents = mesh.tangents;
            }

            if (tangents.Length != vertices.Length)
                throw new Exception("Tangents count != vertex count.");
            uvs = mesh.uv;
            if (uvs.Length != vertices.Length)
                throw new Exception("UV count != vertex count.");
            indices = mesh.GetIndices(0);
            bounds = mesh.bounds;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Splits a mesh and creates GameObjects of the new split meshes.
        /// </summary>
        /// <param name="numberOfXSplits"></param>
        /// <param name="numberOfZSplits"></param>
        /// <param name="translatePivot"></param>
        /// <param name="addMeshCollider"></param>
        /// <returns></returns>
        public List<GameObject> SplitAndCreateGameObjects(int numberOfXSplits, int numberOfZSplits, TranslatePivotType translatePivot, bool addMeshCollider = true)
        {
            List<Mesh> splitMeshes = Split(numberOfXSplits, numberOfZSplits, translatePivot);

            List<GameObject> gameObjects = new List<GameObject>();
            int numRows = numberOfZSplits + 1;
            int numCols = numberOfXSplits + 1;

            for (int i = 0; i < splitMeshes.Count; i++)
            {
                GameObject go = new GameObject($"Terrain Mesh_{i}", typeof(MeshFilter), typeof(MeshRenderer));
                gameObjects.Add(go);
                go.transform.GetComponent<MeshFilter>().sharedMesh = splitMeshes[i];
                if (addMeshCollider)
                {
                    go.AddComponent<MeshCollider>().sharedMesh = splitMeshes[i];
                }

                if (translatePivot == TranslatePivotType.None)
                    continue;
                int row = i / numCols;
                int col = i % numCols;
                TranslateToPivot(go, row, col, translatePivot, numRows, numCols);
            }

            return gameObjects;
        }

        /// <summary>
        /// Splits the mesh into meshes[row][column].
        /// </summary>
        /// <param name="numberOfSplits">The number of splits to make. The number of rows and columns is numberOfSplits + 1.</param>
        /// <param name="translatePivot"></param>
        /// <returns>returns meshes split into splitMeshes[numRows][numColumns] meshes.</returns>
        public List<Mesh> Split(int numberOfSplits, TranslatePivotType translatePivot = TranslatePivotType.None)
        {
            return Split(numberOfSplits, numberOfSplits, translatePivot);
        }

        /// <summary>
        /// Splits the mesh into meshes[row][column].
        /// </summary>
        /// <param name="numberOfXSplits">The number of vertical splits to make. The number of columns is numberOfXSplits + 1.</param>
        /// <param name="numberOfZSplits">The number of horizontal splits to make. The number of rows is numberOfZSplits + 1.</param>
        /// <param name="translatePivot"></param>
        /// <returns>returns meshes split into splitMeshes[numRows][numColumns] meshes.</returns>
        public List<Mesh> Split(int numberOfXSplits, int numberOfZSplits, TranslatePivotType translatePivot = TranslatePivotType.None)
        {
            int numXCells = numberOfXSplits + 1;
            int numZCells = numberOfZSplits + 1;
            Vector2 cellSize = new Vector2(bounds.size.x / numXCells, bounds.size.z / numZCells);
            Vector3 lowerLeft = bounds.center - bounds.extents;

            // Prepare the split planes.
            List<SplitPlane> verticalSplitPlanes = new List<SplitPlane>();
            for (int i = 1; i <= numberOfXSplits; i++)
            {
                float x = lowerLeft.x + i * cellSize.x;
                Vector3 pos3d = new Vector3(x, 0.0f, 0.0f);
                verticalSplitPlanes.Add(new SplitPlane(pos3d, Vector3.right));
            }

            List<SplitPlane> horizontalSplitPlanes = new List<SplitPlane>();
            for (int i = 1; i <= numberOfZSplits; i++)
            {
                float z = lowerLeft.z + i * cellSize.y;
                Vector3 pos3d = new Vector3(0.0f, 0.0f, z);
                horizontalSplitPlanes.Add(new SplitPlane(pos3d, Vector3.forward));
            }

            return Split(verticalSplitPlanes, horizontalSplitPlanes, translatePivot);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Splits a mesh by vertical and horizontal planes (in XZ).
        /// The vertical planes must be in left to right order along X with normal facing right (1,0,0).
        /// The horizontal planes must in near to far order along Z with the normal facing far (0,0,1).
        /// </summary>
        /// <param name="vPlanes">vertical planes in left to right order along X with normal facing right (1,0,0).</param>
        /// <param name="hPlanes">horizontal planes in near to far order along Z with the normal facing far (0,0,1).</param>
        /// <param name="translatePivot"></param>
        /// <returns></returns>
        private List<Mesh> Split(IReadOnlyList<SplitPlane> vPlanes, IReadOnlyList<SplitPlane> hPlanes, TranslatePivotType translatePivot)
        {
            int totalCells = (hPlanes.Count + 1) * (vPlanes.Count + 1);
            int trisPerCell = indices.Length / totalCells;
            trisPerCell += (int)(trisPerCell * 0.1f);
            int numRows = hPlanes.Count + 1;
            int numCols = vPlanes.Count + 1;
            int numBuckets = numRows * numCols;
            MeshBuilder[] buckets = new MeshBuilder[numBuckets];
            for (int i = 0; i < numBuckets; i++)
            {
                buckets[i] = new MeshBuilder(trisPerCell);
            }

            // Now process each triangle

            SplittableTriangle splitTri = new SplittableTriangle(this);
            // Foreach triangle in the mesh
            for (int i = 0; i < indices.Length; i += 3)
            {
                splitTri.StartNew(i);
                splitTri.SplitWithColumnPlanes(vPlanes);
                splitTri.SplitWithRowPlanes(hPlanes);

                // Add all resultant (possibly) split triangles to their buckets
                for (int j = 0; j < splitTri.numTriangles; j++)
                {
                    Triangle tri = splitTri.triangles[j];
                    if (tri == null || !tri.isValid)
                        continue;
                    int index = tri.bucketRowIndex * numCols + tri.bucketColumnIndex;
                    buckets[index].AddTriangle(tri.p, tri.n, tri.t, tri.uv);
                }
            }

            List<Mesh> meshes = new List<Mesh>();
            foreach (MeshBuilder bucket in buckets)
            {
                if (translatePivot != TranslatePivotType.None)
                {
                    TranslateMesh(bucket, translatePivot);
                }

                meshes.Add(bucket.ToUnityMesh(false));
            }

            return meshes;
        }

        private static void TranslateMesh(MeshBuilder mesh, TranslatePivotType translatePivot)
        {
            Bounds bounds = mesh.GetBounds();
            switch (translatePivot)
            {
                case TranslatePivotType.MinimumXZ:
                {
                    Vector3 min = bounds.min;
                    min.y = 0.0f;
                    mesh.TranslateAllVertices(-min);
                    break;
                }
                case TranslatePivotType.CenterXZ:
                {
                    Vector3 center = bounds.center;
                    center.y = 0.0f;
                    mesh.TranslateAllVertices(-center);
                    break;
                }
                case TranslatePivotType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(translatePivot), translatePivot, null);
            }
        }

        private void TranslateToPivot(GameObject go, int row, int col, TranslatePivotType translatePivot, int numRows, int numCols)
        {
            float cellWidth = bounds.size.x / numCols;
            float cellDepth = bounds.size.z / numRows;
            switch (translatePivot)
            {
                case TranslatePivotType.MinimumXZ:
                {
                    Vector3 position = Vector3.zero;
                    position.x = bounds.min.x + col * cellWidth;
                    position.z = bounds.min.z + row * cellDepth;
                    go.transform.position = position;
                    break;
                }
                case TranslatePivotType.CenterXZ:
                {
                    Vector3 position = Vector3.zero;
                    position.x = bounds.min.x + col * cellWidth + cellWidth * 0.5f;
                    position.z = bounds.min.z + row * cellDepth + cellDepth * 0.5f;
                    go.transform.position = position;
                    break;
                }
                case TranslatePivotType.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(translatePivot), translatePivot, null);
            }
        }

        #endregion Private Methods

        #region Private Utility Classes

        private struct SplitPlane
        {
            public Vector3 PointOnPlane { get; set; }
            public Vector3 PlaneNormal { get; set; }
            public PlaneABCD ThisSide { get; set; }
            public PlaneABCD ThatSide { get; set; }

            public SplitPlane(Vector3 pointOnPlane, Vector3 planeNormal)
            {
                PointOnPlane = pointOnPlane;
                PlaneNormal = planeNormal;
                ThisSide = new PlaneABCD(pointOnPlane, planeNormal);
                ThatSide = new PlaneABCD(pointOnPlane, -planeNormal);
            }
        }

        private class MeshBuilder
        {
            #region Variables

            [Obfuscation(Exclude = false)] private readonly List<Vector3> m_vertices;
            [Obfuscation(Exclude = false)] private readonly List<Vector3> m_normals;
            [Obfuscation(Exclude = false)] private readonly List<Vector4> m_tangents;
            [Obfuscation(Exclude = false)] private readonly List<Vector2> m_uvs;
            [Obfuscation(Exclude = false)] private readonly List<int> m_indices;
            [Obfuscation(Exclude = false)] private readonly Dictionary<Vector3, int> m_vertexHashTable = new Dictionary<Vector3, int>();

            #endregion Variables
            
            #region Constructors

            public MeshBuilder(IReadOnlyCollection<Vector3> vertices, IEnumerable<int> indices)
            {
                m_vertices = new List<Vector3>(vertices);
                m_indices = new List<int>(indices);
                m_uvs = new List<Vector2>(vertices.Count);
                m_normals = new List<Vector3>(vertices.Count);
                m_tangents = new List<Vector4>(vertices.Count);
            }

            public MeshBuilder(IReadOnlyCollection<Vector3> vertices, IEnumerable<Vector2> uvs, IEnumerable<int> indices)
            {
                m_vertices = new List<Vector3>(vertices);
                m_indices = new List<int>(indices);
                m_uvs = new List<Vector2>(uvs);
                m_normals = new List<Vector3>(vertices.Count);
                m_tangents = new List<Vector4>(vertices.Count);
            }

            public MeshBuilder(Mesh mesh)
            {
                m_vertices = new List<Vector3>(mesh.vertices);
                m_indices = new List<int>(mesh.triangles);
                m_uvs = mesh.uv.Length == m_vertices.Count ? new List<Vector2>(mesh.uv) : new List<Vector2>();
                m_normals = mesh.normals.Length == m_vertices.Count ? new List<Vector3>(mesh.normals) : new List<Vector3>();
                m_tangents = mesh.tangents.Length == m_vertices.Count ? new List<Vector4>(mesh.tangents) : new List<Vector4>();
            }

            public MeshBuilder(int capacity)
            {
                m_vertices = new List<Vector3>(capacity);
                m_indices = new List<int>(capacity * 2);
                m_uvs = new List<Vector2>(capacity);
                m_normals = new List<Vector3>(capacity);
                m_tangents = new List<Vector4>(capacity);
            }

            #endregion

            #region Public Methods

            public Bounds GetBounds()
            {
                if (m_vertices.Count < 1)
                    return new Bounds();
                Bounds bounds = new Bounds(m_vertices[0], Vector3.zero);
                foreach (Vector3 vertex in m_vertices)
                    bounds.Encapsulate(vertex);
                return bounds;
            }

            /// <summary>
            /// Translates all vertices by amount.
            /// </summary>
            /// <param name="amount">The amount to translate the vertices.</param>
            public void TranslateAllVertices(Vector3 amount)
            {
                for (int i = 0; i < m_vertices.Count; i++)
                {
                    m_vertices[i] += amount;
                }
            }

            public Mesh ToUnityMesh(bool recalculateTangents = true)
            {
                if (m_vertices.Count < 3)
                    return null;
                Mesh mesh = new Mesh
                {
                    vertices = m_vertices.ToArray()
                };
                if (m_vertices.Count > 65500)
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.SetIndices(m_indices.ToArray(), MeshTopology.Triangles, 0);
                if (m_uvs.Count != m_vertices.Count)
                {
                    PlanarProjectXZ(1.0f);
                }

                mesh.uv = m_uvs.ToArray();
                if (m_normals.Count == m_vertices.Count)
                    mesh.normals = m_normals.ToArray();
                else
                    mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                if (recalculateTangents || m_tangents.Count != m_vertices.Count)
                    mesh.RecalculateTangents();
                else
                    mesh.tangents = m_tangents.ToArray();
                return mesh;
            }

            public void AddTriangle(Vector3[] triVerts)
            {
                m_indices.Add(AddVertex(triVerts[0]));
                m_indices.Add(AddVertex(triVerts[1]));
                m_indices.Add(AddVertex(triVerts[2]));
            }

            public void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
            {
                m_indices.Add(AddVertex(v0));
                m_indices.Add(AddVertex(v1));
                m_indices.Add(AddVertex(v2));
            }

            public void AddTriangle(Vector3[] triVerts, Vector2[] uvs)
            {
                m_indices.Add(AddVertex(triVerts[0], uvs[0]));
                m_indices.Add(AddVertex(triVerts[1], uvs[1]));
                m_indices.Add(AddVertex(triVerts[2], uvs[2]));
            }

            public void AddTriangle(Vector3[] triVerts, Vector3[] normals, Vector2[] uvs)
            {
                m_indices.Add(AddVertex(triVerts[0], normals[0], uvs[0]));
                m_indices.Add(AddVertex(triVerts[1], normals[1], uvs[1]));
                m_indices.Add(AddVertex(triVerts[2], normals[2], uvs[2]));
            }

            public void AddTriangle(Vector3[] triVerts, Vector3[] normals, Vector4[] tangents, Vector2[] uvs)
            {
                m_indices.Add(AddVertex(triVerts[0], normals[0], tangents[0], uvs[0]));
                m_indices.Add(AddVertex(triVerts[1], normals[1], tangents[1], uvs[1]));
                m_indices.Add(AddVertex(triVerts[2], normals[2], tangents[2], uvs[2]));
            }

            public int AddVertex(Vector3 vert)
            {
                if (m_vertexHashTable.TryGetValue(vert, out int index))
                {
                    return index;
                }

                index = m_vertices.Count;
                m_vertices.Add(vert);
                m_vertexHashTable.Add(vert, index);
                return index;
            }

            public int AddVertex(Vector3 vert, Vector2 uv)
            {
                if (m_vertexHashTable.TryGetValue(vert, out int index))
                {
                    return index;
                }

                index = m_vertices.Count;
                m_vertices.Add(vert);
                m_uvs.Add(uv);
                m_vertexHashTable.Add(vert, index);
                return index;
            }

            public int AddVertex(Vector3 vert, Vector2 uv, bool replaceUV)
            {
                if (m_vertexHashTable.TryGetValue(vert, out int index))
                {
                    if (replaceUV)
                        m_uvs[index] = uv;
                    return index;
                }

                index = m_vertices.Count;
                m_vertices.Add(vert);
                m_uvs.Add(uv);
                m_vertexHashTable.Add(vert, index);
                return index;
            }

            public int AddVertex(Vector3 vert, Vector3 norm, Vector2 uv)
            {
                if (m_vertexHashTable.TryGetValue(vert, out int index))
                {
                    return index;
                }

                index = m_vertices.Count;
                m_vertices.Add(vert);
                m_normals.Add(norm);
                m_uvs.Add(uv);
                m_vertexHashTable.Add(vert, index);
                return index;
            }

            public int AddVertex(Vector3 vert, Vector3 norm, Vector4 tangent, Vector2 uv)
            {
                if (m_vertexHashTable.TryGetValue(vert, out int index))
                {
                    return index;
                }

                index = m_vertices.Count;
                m_vertices.Add(vert);
                m_normals.Add(norm);
                m_tangents.Add(tangent);
                m_uvs.Add(uv);
                m_vertexHashTable.Add(vert, index);
                return index;
            }

            /// <summary>
            /// Create UV's planar projected in the XZ plane in world space.
            /// </summary>
            /// <param name="scale"></param>
            public void PlanarProjectXZ(float scale)
            {
                if (m_vertices.Count < 2)
                    return;
                m_uvs.Clear(); // We'll be starting over
                Vector2 uvScale = new Vector2(scale, scale);
                for (int i = 0; i < m_vertices.Count; i++)
                {
                    Vector2 uv = new Vector2(m_vertices[i].x, m_vertices[i].z);
                    uv.Scale(uvScale);
                    m_uvs.Add(uv);
                }
            }

            #endregion Public Methods
        }

        private class Triangle
        {
            private readonly GTSMeshSplitter outer = null;
            public int triIndex;
            public bool isValid = false;
            public int bucketColumnIndex;
            public int bucketRowIndex;
            public readonly Vector3[] p;
            public readonly Vector3[] n;
            public readonly Vector4[] t;
            public readonly Vector2[] uv;

            public Triangle(GTSMeshSplitter parent)
            {
                outer = parent;
                triIndex = -1;
                bucketColumnIndex = bucketRowIndex = -1;
                p = new Vector3[4];
                n = new Vector3[4];
                t = new Vector4[4];
                uv = new Vector2[4];
            }

            public bool CompletelyLeftOf(SplitPlane plane)
            {
                float px = plane.PointOnPlane.x;
                return p[0].x <= px && p[1].x <= px && p[2].x <= px;
            }

            public bool CompletelyBehindOf(SplitPlane plane)
            {
                float pz = plane.PointOnPlane.z;
                return p[0].z <= pz && p[1].z <= pz && p[2].z <= pz;
            }

            public void CopyOtherHalf(Triangle other)
            {
                triIndex = other.triIndex;
                bucketColumnIndex = other.bucketColumnIndex;
                bucketRowIndex = other.bucketRowIndex;
                isValid = true;
                for (int i = 0; i < 3; i++)
                {
                    // copy 0, 2, 3
                    int srcIndex = i;
                    if (i > 0) srcIndex++;
                    p[i] = other.p[srcIndex];
                    n[i] = other.n[srcIndex];
                    t[i] = other.t[srcIndex];
                    uv[i] = other.uv[srcIndex];
                }
            }

            public void Copy(Triangle other)
            {
                triIndex = other.triIndex;
                bucketColumnIndex = other.bucketColumnIndex;
                bucketRowIndex = other.bucketRowIndex;
                isValid = true;
                for (int i = 0; i < 3; i++)
                {
                    p[i] = other.p[i];
                    n[i] = other.n[i];
                    t[i] = other.t[i];
                    uv[i] = other.uv[i];
                }
            }

            public void CopyVerts(int index)
            {
                triIndex = index;
                bucketColumnIndex = bucketRowIndex = -1;
                for (int i = 0; i < 3; i++)
                {
                    int ndx = outer.indices[index + i];
                    p[i] = outer.vertices[ndx];
                    n[i] = outer.normals[ndx];
                    t[i] = outer.tangents[ndx];
                    uv[i] = outer.uvs[ndx];
                }

                isValid = true;
            }

            public float[] EvaluateForPlane(PlaneABCD plane)
            {
                return EvaluatePointsForPlane(plane, p);
            }

            /// <summary>
            /// Clips this triangle to a plane.
            /// </summary>
            /// <param name="plane"></param>
            /// <returns>0, 3, or 4 depending on the number of vertices in result.</returns>
            public int Clip(PlaneABCD plane)
            {
                ClipTriangle(plane, p, n, t, uv, out int count);
                // Nothing left.
                isValid = count != 0;
                // 1 or 2 triangles (3 or 4 verts).
                return count;
            }
        }

        private class SplittableTriangle
        {
            public int numTriangles = 0;
            public readonly Triangle[] triangles = new Triangle[16];

            public SplittableTriangle(GTSMeshSplitter parent)
            {
                for (int i = 0; i < triangles.Length; i++)
                {
                    triangles[i] = new Triangle(parent);
                }
            }

            public void StartNew(int triIndex)
            {
                numTriangles = 1;
                triangles[0].CopyVerts(triIndex);
            }

            public void SplitWithColumnPlanes(IReadOnlyList<SplitPlane> planes)
            {
                int splitTriCount = numTriangles;
                for (int i = 0; i < splitTriCount; i++)
                {
                    Triangle tri = triangles[i];
                    for (int j = 0; j < planes.Count; j++)
                    {
                        SplitPlane plane = planes[j];
                        if (tri.CompletelyLeftOf(plane))
                        {
                            tri.bucketColumnIndex = j;
                            break;
                        }

                        float[] side = tri.EvaluateForPlane(plane.ThisSide);
                        if (side[0] <= 0 && side[1] <= 0 && side[2] <= 0)
                        {
                            // left of plane.
                            // This means we have the column bucket index for it.
                            tri.bucketColumnIndex = j;
                            break;
                        }

                        if (side[0] >= 0 && side[1] >= 0 && side[2] >= 0)
                        {
                            // right side
                            // so, nothing to do here.
                        }
                        else
                        {
                            // Split this triangle
                            Triangle otherTri = triangles[numTriangles];
                            otherTri.Copy(tri);
                            numTriangles++;

                            tri.bucketColumnIndex = j;
                            int numVerts = tri.Clip(plane.ThisSide);
                            if (numVerts > 3)
                            {
                                triangles[numTriangles].CopyOtherHalf(tri);
                                numTriangles++;
                            }

                            otherTri.bucketColumnIndex = j + 1;
                            numVerts = otherTri.Clip(plane.ThatSide);
                            if (numVerts > 3)
                            {
                                triangles[numTriangles].CopyOtherHalf(otherTri);
                                numTriangles++;
                            }

                            break;
                        }
                    }

                    // If this triangle is still un split and unassigned
                    // then we assign it to the rightmost bucket column.
                    if (tri.bucketColumnIndex < 0)
                        tri.bucketColumnIndex = planes.Count;
                }
            }

            public void SplitWithRowPlanes(IReadOnlyList<SplitPlane> planes)
            {
                int splitTriCount = numTriangles;
                for (int i = 0; i < splitTriCount; i++)
                {
                    Triangle tri = triangles[i];
                    for (int j = 0; j < planes.Count; j++)
                    {
                        SplitPlane plane = planes[j];
                        if (tri.CompletelyBehindOf(plane))
                        {
                            tri.bucketRowIndex = j;
                            break;
                        }

                        float[] side = tri.EvaluateForPlane(plane.ThisSide);
                        if (side[0] <= 0 && side[1] <= 0 && side[2] <= 0)
                        {
                            // left of plane.
                            // This means we have the column bucket index for it.
                            tri.bucketRowIndex = j;
                            break;
                        }

                        if (side[0] >= 0 && side[1] >= 0 && side[2] >= 0)
                        {
                            // right side
                            // so, nothing to do here.
                        }
                        else
                        {
                            // Split this triangle
                            Triangle otherTri = triangles[numTriangles];
                            otherTri.Copy(tri);
                            otherTri.bucketRowIndex = j + 1;
                            numTriangles++;

                            tri.bucketRowIndex = j;
                            int numVerts = tri.Clip(plane.ThisSide);
                            if (numVerts > 3)
                            {
                                triangles[numTriangles].CopyOtherHalf(tri);
                                triangles[numTriangles].bucketRowIndex = j;
                                numTriangles++;
                            }

                            numVerts = otherTri.Clip(plane.ThatSide);
                            if (numVerts > 3)
                            {
                                triangles[numTriangles].CopyOtherHalf(otherTri);
                                numTriangles++;
                            }

                            break;
                        }
                    }

                    // If this triangle is still un split and unassigned
                    // then we assign it to the far most bucket column.
                    if (tri.bucketRowIndex < 0)
                        tri.bucketRowIndex = planes.Count;
                }
            }
        }

        private struct PlaneABCD
        {
            public readonly float A, B, C, D;

            // Determine the equation of the plane as
            // Ax + By + Cz + D = 0
            public PlaneABCD(Vector3 point, Vector3 normal)
            {
                A = normal.x;
                B = normal.y;
                C = normal.z;
                D = -(normal.x * point.x + normal.y * point.y + normal.z * point.z);
            }
        }

        /// <summary>
        /// Evaluate the side that each of 3 points is on of a plane.
        /// </summary>
        /// <param name="plane">Precomputed plane coefficients.</param>
        /// <param name="p">3 or more points</param>
        /// <returns>3 float values describing the side that each of 3 points lies compared to a 3d plane.</returns>
        private static float[] EvaluatePointsForPlane(PlaneABCD plane, Vector3[] p)
        {
            float[] side = new float[3];

            // Evaluate the equation of the plane for each vertex.
            // If side < 0 then it is on the side to be retained
            // else it is to be clipped.
            side[0] = plane.A * p[0].x + plane.B * p[0].y + plane.C * p[0].z + plane.D;
            side[1] = plane.A * p[1].x + plane.B * p[1].y + plane.C * p[1].z + plane.D;
            side[2] = plane.A * p[2].x + plane.B * p[2].y + plane.C * p[2].z + plane.D;

            return side;
        }

        private static bool ClipTriangle(PlaneABCD plane, Vector3[] p, Vector3[] norm, Vector4[] tang, Vector2[] uv, out int numVerts)
        {
            float[] side = EvaluatePointsForPlane(plane, p);
            return ClipTriangle(side, p, norm, tang, uv, out numVerts);
        }

        private static bool ClipTriangle(IReadOnlyList<float> side, Vector3[] p, Vector3[] norm, Vector4[] tang, Vector2[] uv, out int numVerts)
        {
            Vector3 q;
            Vector2 r;
            Vector3 s;
            Vector4 t;

            // Are all the vertices are on the clipped side?
            if (side[0] >= 0 && side[1] >= 0 && side[2] >= 0)
            {
                numVerts = 0;
                return true;
            }

            // Are all the vertices on the not-clipped side?
            if (side[0] <= 0 && side[1] <= 0 && side[2] <= 0)
            {
                numVerts = 3;
                return false;
            }

            // Is p0 the only point on the clipped side?
            if (side[0] > 0 && side[1] <= 0 && side[2] <= 0)
            {
                q.x = p[0].x - side[0] * (p[2].x - p[0].x) / (side[2] - side[0]);
                q.y = p[0].y - side[0] * (p[2].y - p[0].y) / (side[2] - side[0]);
                q.z = p[0].z - side[0] * (p[2].z - p[0].z) / (side[2] - side[0]);
                p[3] = q;
                r.x = uv[0].x - side[0] * (uv[2].x - uv[0].x) / (side[2] - side[0]);
                r.y = uv[0].y - side[0] * (uv[2].y - uv[0].y) / (side[2] - side[0]);
                uv[3] = r;
                s.x = norm[0].x - side[0] * (norm[2].x - norm[0].x) / (side[2] - side[0]);
                s.y = norm[0].y - side[0] * (norm[2].y - norm[0].y) / (side[2] - side[0]);
                s.z = norm[0].z - side[0] * (norm[2].z - norm[0].z) / (side[2] - side[0]);
                norm[3] = s;
                t.x = tang[0].x - side[0] * (tang[2].x - tang[0].x) / (side[2] - side[0]);
                t.y = tang[0].y - side[0] * (tang[2].y - tang[0].y) / (side[2] - side[0]);
                t.z = tang[0].z - side[0] * (tang[2].z - tang[0].z) / (side[2] - side[0]);
                t.w = tang[0].w;
                tang[3] = t;

                q.x = p[0].x - side[0] * (p[1].x - p[0].x) / (side[1] - side[0]);
                q.y = p[0].y - side[0] * (p[1].y - p[0].y) / (side[1] - side[0]);
                q.z = p[0].z - side[0] * (p[1].z - p[0].z) / (side[1] - side[0]);
                p[0] = q;
                r.x = uv[0].x - side[0] * (uv[1].x - uv[0].x) / (side[1] - side[0]);
                r.y = uv[0].y - side[0] * (uv[1].y - uv[0].y) / (side[1] - side[0]);
                uv[0] = r;
                s.x = norm[0].x - side[0] * (norm[1].x - norm[0].x) / (side[1] - side[0]);
                s.y = norm[0].y - side[0] * (norm[1].y - norm[0].y) / (side[1] - side[0]);
                s.z = norm[0].z - side[0] * (norm[1].z - norm[0].z) / (side[1] - side[0]);
                norm[0] = s;
                t.x = tang[0].x - side[0] * (tang[1].x - tang[0].x) / (side[1] - side[0]);
                t.y = tang[0].y - side[0] * (tang[1].y - tang[0].y) / (side[1] - side[0]);
                t.z = tang[0].z - side[0] * (tang[1].z - tang[0].z) / (side[1] - side[0]);
                t.w = tang[0].w;
                tang[0] = t;

                numVerts = 4;
                return true;
            }

            // Is p1 the only point on the clipped side?
            if (side[1] > 0 && side[0] <= 0 && side[2] <= 0)
            {
                p[3] = p[2];
                norm[3] = norm[2];
                tang[3] = tang[2];
                uv[3] = uv[2];

                q.x = p[1].x - side[1] * (p[2].x - p[1].x) / (side[2] - side[1]);
                q.y = p[1].y - side[1] * (p[2].y - p[1].y) / (side[2] - side[1]);
                q.z = p[1].z - side[1] * (p[2].z - p[1].z) / (side[2] - side[1]);
                p[2] = q;
                r.x = uv[1].x - side[1] * (uv[2].x - uv[1].x) / (side[2] - side[1]);
                r.y = uv[1].y - side[1] * (uv[2].y - uv[1].y) / (side[2] - side[1]);
                uv[2] = r;
                s.x = norm[1].x - side[1] * (norm[2].x - norm[1].x) / (side[2] - side[1]);
                s.y = norm[1].y - side[1] * (norm[2].y - norm[1].y) / (side[2] - side[1]);
                s.z = norm[1].z - side[1] * (norm[2].z - norm[1].z) / (side[2] - side[1]);
                norm[2] = s;
                t.x = tang[1].x - side[1] * (tang[2].x - tang[1].x) / (side[2] - side[1]);
                t.y = tang[1].y - side[1] * (tang[2].y - tang[1].y) / (side[2] - side[1]);
                t.z = tang[1].z - side[1] * (tang[2].z - tang[1].z) / (side[2] - side[1]);
                t.w = tang[1].w;
                tang[2] = t;

                q.x = p[1].x - side[1] * (p[0].x - p[1].x) / (side[0] - side[1]);
                q.y = p[1].y - side[1] * (p[0].y - p[1].y) / (side[0] - side[1]);
                q.z = p[1].z - side[1] * (p[0].z - p[1].z) / (side[0] - side[1]);
                p[1] = q;
                r.x = uv[1].x - side[1] * (uv[0].x - uv[1].x) / (side[0] - side[1]);
                r.y = uv[1].y - side[1] * (uv[0].y - uv[1].y) / (side[0] - side[1]);
                uv[1] = r;
                s.x = norm[1].x - side[1] * (norm[0].x - norm[1].x) / (side[0] - side[1]);
                s.y = norm[1].y - side[1] * (norm[0].y - norm[1].y) / (side[0] - side[1]);
                s.z = norm[1].z - side[1] * (norm[0].z - norm[1].z) / (side[0] - side[1]);
                norm[1] = s;
                t.x = tang[1].x - side[1] * (tang[0].x - tang[1].x) / (side[0] - side[1]);
                t.y = tang[1].y - side[1] * (tang[0].y - tang[1].y) / (side[0] - side[1]);
                t.z = tang[1].z - side[1] * (tang[0].z - tang[1].z) / (side[0] - side[1]);
                t.w = tang[1].w;
                tang[1] = t;

                numVerts = 4;
                return true;
            }

            // Is p2 the only point on the clipped side?
            if (side[2] > 0 && side[0] <= 0 && side[1] <= 0)
            {
                q.x = p[2].x - side[2] * (p[0].x - p[2].x) / (side[0] - side[2]);
                q.y = p[2].y - side[2] * (p[0].y - p[2].y) / (side[0] - side[2]);
                q.z = p[2].z - side[2] * (p[0].z - p[2].z) / (side[0] - side[2]);
                p[3] = q;
                r.x = uv[2].x - side[2] * (uv[0].x - uv[2].x) / (side[0] - side[2]);
                r.y = uv[2].y - side[2] * (uv[0].y - uv[2].y) / (side[0] - side[2]);
                uv[3] = r;
                s.x = norm[2].x - side[2] * (norm[0].x - norm[2].x) / (side[0] - side[2]);
                s.y = norm[2].y - side[2] * (norm[0].y - norm[2].y) / (side[0] - side[2]);
                s.z = norm[2].z - side[2] * (norm[0].z - norm[2].z) / (side[0] - side[2]);
                norm[3] = s;
                t.x = tang[2].x - side[2] * (tang[0].x - tang[2].x) / (side[0] - side[2]);
                t.y = tang[2].y - side[2] * (tang[0].y - tang[2].y) / (side[0] - side[2]);
                t.z = tang[2].z - side[2] * (tang[0].z - tang[2].z) / (side[0] - side[2]);
                t.w = tang[2].w;
                tang[3] = t;

                q.x = p[2].x - side[2] * (p[1].x - p[2].x) / (side[1] - side[2]);
                q.y = p[2].y - side[2] * (p[1].y - p[2].y) / (side[1] - side[2]);
                q.z = p[2].z - side[2] * (p[1].z - p[2].z) / (side[1] - side[2]);
                p[2] = q;
                r.x = uv[2].x - side[2] * (uv[1].x - uv[2].x) / (side[1] - side[2]);
                r.y = uv[2].y - side[2] * (uv[1].y - uv[2].y) / (side[1] - side[2]);
                uv[2] = r;
                s.x = norm[2].x - side[2] * (norm[1].x - norm[2].x) / (side[1] - side[2]);
                s.y = norm[2].y - side[2] * (norm[1].y - norm[2].y) / (side[1] - side[2]);
                s.z = norm[2].z - side[2] * (norm[1].z - norm[2].z) / (side[1] - side[2]);
                norm[2] = s;
                t.x = tang[2].x - side[2] * (tang[1].x - tang[2].x) / (side[1] - side[2]);
                t.y = tang[2].y - side[2] * (tang[1].y - tang[2].y) / (side[1] - side[2]);
                t.z = tang[2].z - side[2] * (tang[1].z - tang[2].z) / (side[1] - side[2]);
                t.w = tang[2].w;
                tang[2] = t;

                numVerts = 4;
                return true;
            }

            // Is p0 the only point on the not-clipped side?
            if (side[0] <= 0 && side[1] > 0 && side[2] > 0)
            {
                q.x = p[0].x - side[0] * (p[1].x - p[0].x) / (side[1] - side[0]);
                q.y = p[0].y - side[0] * (p[1].y - p[0].y) / (side[1] - side[0]);
                q.z = p[0].z - side[0] * (p[1].z - p[0].z) / (side[1] - side[0]);
                p[1] = q;
                r.x = uv[0].x - side[0] * (uv[1].x - uv[0].x) / (side[1] - side[0]);
                r.y = uv[0].y - side[0] * (uv[1].y - uv[0].y) / (side[1] - side[0]);
                uv[1] = r;
                s.x = norm[0].x - side[0] * (norm[1].x - norm[0].x) / (side[1] - side[0]);
                s.y = norm[0].y - side[0] * (norm[1].y - norm[0].y) / (side[1] - side[0]);
                s.z = norm[0].z - side[0] * (norm[1].z - norm[0].z) / (side[1] - side[0]);
                norm[1] = s;
                t.x = tang[0].x - side[0] * (tang[1].x - tang[0].x) / (side[1] - side[0]);
                t.y = tang[0].y - side[0] * (tang[1].y - tang[0].y) / (side[1] - side[0]);
                t.z = tang[0].z - side[0] * (tang[1].z - tang[0].z) / (side[1] - side[0]);
                t.w = tang[0].w;
                tang[1] = t;

                q.x = p[0].x - side[0] * (p[2].x - p[0].x) / (side[2] - side[0]);
                q.y = p[0].y - side[0] * (p[2].y - p[0].y) / (side[2] - side[0]);
                q.z = p[0].z - side[0] * (p[2].z - p[0].z) / (side[2] - side[0]);
                p[2] = q;
                r.x = uv[0].x - side[0] * (uv[2].x - uv[0].x) / (side[2] - side[0]);
                r.y = uv[0].y - side[0] * (uv[2].y - uv[0].y) / (side[2] - side[0]);
                uv[2] = r;
                s.x = norm[0].x - side[0] * (norm[2].x - norm[0].x) / (side[2] - side[0]);
                s.y = norm[0].y - side[0] * (norm[2].y - norm[0].y) / (side[2] - side[0]);
                s.z = norm[0].z - side[0] * (norm[2].z - norm[0].z) / (side[2] - side[0]);
                norm[2] = s;
                t.x = tang[0].x - side[0] * (tang[2].x - tang[0].x) / (side[2] - side[0]);
                t.y = tang[0].y - side[0] * (tang[2].y - tang[0].y) / (side[2] - side[0]);
                t.z = tang[0].z - side[0] * (tang[2].z - tang[0].z) / (side[2] - side[0]);
                t.w = tang[0].w;
                tang[2] = t;

                numVerts = 3;
                return true;
            }

            // Is p1 the only point on the not-clipped side?
            if (side[1] <= 0 && side[0] > 0 && side[2] > 0)
            {
                q.x = p[1].x - side[1] * (p[0].x - p[1].x) / (side[0] - side[1]);
                q.y = p[1].y - side[1] * (p[0].y - p[1].y) / (side[0] - side[1]);
                q.z = p[1].z - side[1] * (p[0].z - p[1].z) / (side[0] - side[1]);
                p[0] = q;
                r.x = uv[1].x - side[1] * (uv[0].x - uv[1].x) / (side[0] - side[1]);
                r.y = uv[1].y - side[1] * (uv[0].y - uv[1].y) / (side[0] - side[1]);
                uv[0] = r;
                s.x = norm[1].x - side[1] * (norm[0].x - norm[1].x) / (side[0] - side[1]);
                s.y = norm[1].y - side[1] * (norm[0].y - norm[1].y) / (side[0] - side[1]);
                s.z = norm[1].z - side[1] * (norm[0].z - norm[1].z) / (side[0] - side[1]);
                norm[0] = s;
                t.x = tang[1].x - side[1] * (tang[0].x - tang[1].x) / (side[0] - side[1]);
                t.y = tang[1].y - side[1] * (tang[0].y - tang[1].y) / (side[0] - side[1]);
                t.z = tang[1].z - side[1] * (tang[0].z - tang[1].z) / (side[0] - side[1]);
                t.w = tang[1].w;
                tang[0] = t;

                q.x = p[1].x - side[1] * (p[2].x - p[1].x) / (side[2] - side[1]);
                q.y = p[1].y - side[1] * (p[2].y - p[1].y) / (side[2] - side[1]);
                q.z = p[1].z - side[1] * (p[2].z - p[1].z) / (side[2] - side[1]);
                p[2] = q;
                r.x = uv[1].x - side[1] * (uv[2].x - uv[1].x) / (side[2] - side[1]);
                r.y = uv[1].y - side[1] * (uv[2].y - uv[1].y) / (side[2] - side[1]);
                uv[2] = r;
                s.x = norm[1].x - side[1] * (norm[2].x - norm[1].x) / (side[2] - side[1]);
                s.y = norm[1].y - side[1] * (norm[2].y - norm[1].y) / (side[2] - side[1]);
                s.z = norm[1].z - side[1] * (norm[2].z - norm[1].z) / (side[2] - side[1]);
                norm[2] = s;
                t.x = tang[1].x - side[1] * (tang[2].x - tang[1].x) / (side[2] - side[1]);
                t.y = tang[1].y - side[1] * (tang[2].y - tang[1].y) / (side[2] - side[1]);
                t.z = tang[1].z - side[1] * (tang[2].z - tang[1].z) / (side[2] - side[1]);
                t.w = tang[1].w;
                tang[2] = t;

                numVerts = 3;
                return true;
            }

            // Is p2 the only point on the not-clipped side?
            if (side[2] <= 0 && side[0] > 0 && side[1] > 0)
            {
                q.x = p[2].x - side[2] * (p[1].x - p[2].x) / (side[1] - side[2]);
                q.y = p[2].y - side[2] * (p[1].y - p[2].y) / (side[1] - side[2]);
                q.z = p[2].z - side[2] * (p[1].z - p[2].z) / (side[1] - side[2]);
                p[1] = q;
                r.x = uv[2].x - side[2] * (uv[1].x - uv[2].x) / (side[1] - side[2]);
                r.y = uv[2].y - side[2] * (uv[1].y - uv[2].y) / (side[1] - side[2]);
                uv[1] = r;
                s.x = norm[2].x - side[2] * (norm[1].x - norm[2].x) / (side[1] - side[2]);
                s.y = norm[2].y - side[2] * (norm[1].y - norm[2].y) / (side[1] - side[2]);
                s.z = norm[2].z - side[2] * (norm[1].z - norm[2].z) / (side[1] - side[2]);
                norm[1] = s;
                t.x = tang[2].x - side[2] * (tang[1].x - tang[2].x) / (side[1] - side[2]);
                t.y = tang[2].y - side[2] * (tang[1].y - tang[2].y) / (side[1] - side[2]);
                t.z = tang[2].z - side[2] * (tang[1].z - tang[2].z) / (side[1] - side[2]);
                t.w = tang[2].w;
                tang[1] = t;

                q.x = p[2].x - side[2] * (p[0].x - p[2].x) / (side[0] - side[2]);
                q.y = p[2].y - side[2] * (p[0].y - p[2].y) / (side[0] - side[2]);
                q.z = p[2].z - side[2] * (p[0].z - p[2].z) / (side[0] - side[2]);
                p[0] = q;
                r.x = uv[2].x - side[2] * (uv[0].x - uv[2].x) / (side[0] - side[2]);
                r.y = uv[2].y - side[2] * (uv[0].y - uv[2].y) / (side[0] - side[2]);
                uv[0] = r;
                s.x = norm[2].x - side[2] * (norm[0].x - norm[2].x) / (side[0] - side[2]);
                s.y = norm[2].y - side[2] * (norm[0].y - norm[2].y) / (side[0] - side[2]);
                s.z = norm[2].z - side[2] * (norm[0].z - norm[2].z) / (side[0] - side[2]);
                norm[0] = s;
                t.x = tang[2].x - side[2] * (tang[0].x - tang[2].x) / (side[0] - side[2]);
                t.y = tang[2].y - side[2] * (tang[0].y - tang[2].y) / (side[0] - side[2]);
                t.z = tang[2].z - side[2] * (tang[0].z - tang[2].z) / (side[0] - side[2]);
                t.w = tang[2].w;
                tang[0] = t;

                numVerts = 3;
                return true;
            }

            // Shouldn't get here (unless degenerate?)
            // Debug.LogError($"ERROR: ClipTriangle failed side = {side[0]}, {side[1]}, {side[2]}");
            numVerts = 0;
            return true;
        }

        #endregion Private Utility Classes
    }
}