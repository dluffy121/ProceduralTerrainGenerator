using System;
using UnityEngine;

namespace TG
{
    public class MeshGenerator
    {
        public static MeshData GenerateMesh(float[,] heightMap, AnimationCurve heightCurve, int levelOfDetail, float heightMultiplier, bool connectable = false)
        {
            // NOTE : Clone is created as animation curves Evaluate incorrectly in other threads
            AnimationCurve l_heightCurve = new AnimationCurve(heightCurve.keys);

            int pseudoWidth = heightMap.GetLength(0);
            int pseudoHeight = heightMap.GetLength(1);

            int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

            int meshWidth = pseudoWidth - (connectable ? 2 : 0) * meshSimplificationIncrement;
            int meshHeight = pseudoHeight - (connectable ? 2 : 0) * meshSimplificationIncrement;
            int meshWidthUnsimplified = pseudoWidth - (connectable ? 2 : 0);
            int meshHeightUnsimplified = pseudoHeight - (connectable ? 2 : 0);

            float topLeftX = (meshWidthUnsimplified - 1) / -2f;
            float topLeftZ = (meshHeightUnsimplified - 1) / 2f;

            int verticesPerLineHorizontal = (meshWidth - 1) / meshSimplificationIncrement + 1;
            int verticesPerLineVertical = (meshHeight - 1) / meshSimplificationIncrement + 1;

            MeshData l_meshData = new MeshData(verticesPerLineHorizontal, verticesPerLineVertical);

            int[,] indices = new int[pseudoWidth, pseudoHeight];
            int trueMeshIndex = 0;
            int pseudoMeshIndex = -1;

            bool isEdgeVertex(int x, int y)
            {
                return x == 0
                        || y == 0
                        || x == pseudoWidth - 1
                        || y == pseudoHeight - 1;
            }

            for (int y = 0; y < pseudoHeight; y += meshSimplificationIncrement)
            {
                for (int x = 0; x < pseudoWidth; x += meshSimplificationIncrement)
                {
                    if (connectable
                        && isEdgeVertex(x, y))
                        indices[x, y] = pseudoMeshIndex--;
                    else
                        indices[x, y] = trueMeshIndex++;
                }
            }


            for (int y = 0; y < pseudoHeight; y += meshSimplificationIncrement)
            {
                for (int x = 0; x < pseudoWidth; x += meshSimplificationIncrement)
                {
                    int index = indices[x, y];
                    // NOTE : Since UVs dont need to be mapped on pseudo triangles we use meshWidth and meshHeight and disregard the pseudo edge vertices by subtracting the meshSimplificationIncrement
                    Vector2 uv = new Vector2((x - meshSimplificationIncrement) / (float)meshWidth,
                                             (y - meshSimplificationIncrement) / (float)meshHeight);
                    // NOTE : 
                    // Also position of true vertices should still remain same, making pseudo vertices to be creating around them 
                    // i.e. if first mesh vertex starts at 0, 0 pseudo vertex should respect this and start from -1, -1
                    // this can be achieved by using the uv values
                    Vector3 position = new Vector3(topLeftX + uv.x * meshWidthUnsimplified,
                                                   l_heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier,
                                                   topLeftZ - uv.y * meshHeightUnsimplified);

                    l_meshData.AddVertex(index, position, uv);

                    if (x < pseudoWidth - 1 && y < pseudoHeight - 1)
                    {
                        //   A - B
                        //  / \ /
                        // C - D
                        int A = indices[x, y];
                        int B = indices[x + meshSimplificationIncrement, y];
                        int C = indices[x, y + meshSimplificationIncrement];
                        int D = indices[x + meshSimplificationIncrement, y + meshSimplificationIncrement];

                        l_meshData.AddTriangle(A, D, C);
                        l_meshData.AddTriangle(D, A, B);
                    }

                    index++;
                }
            }

            l_meshData.BakeNormals();

            return l_meshData;
        }
    }
}

public class MeshData
{
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;
    int triangleIndex;

    Vector3[] pseudoVertices;
    int[] pseudoTriangles;
    int pseudoTriangleIndex;

    Vector3[] normals;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];

        pseudoVertices = new Vector3[(meshWidth * 2) + (meshHeight * 2) + 4];
        // pseudoTriangles = new int[(((meshWidth - 1) * 2) + ((meshHeight - 1) * 2) + 4) * 6];
        // pseudoTriangles = new int[((meshWidth - 1) * 12) + ((meshHeight - 1) * 12) + 24];
        pseudoTriangles = new int[12 * (meshWidth + meshHeight)];
    }

    public void AddVertex(int index, Vector3 position, Vector2 uv)
    {
        if (index < 0)
        {
            pseudoVertices[-index - 1] = position;
            return;
        }

        vertices[index] = position;
        uvs[index] = uv;
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            pseudoTriangles[pseudoTriangleIndex++] = a;
            pseudoTriangles[pseudoTriangleIndex++] = b;
            pseudoTriangles[pseudoTriangleIndex++] = c;
        }
        else
        {
            triangles[triangleIndex++] = a;
            triangles[triangleIndex++] = b;
            triangles[triangleIndex++] = c;
        }
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        // NOTE : In any case if meshes are placed adjacent, the normals on the edges wont match
        // mesh.RecalculateNormals();   
        // NOTE : 
        // Also only calculating edge normals isn't sufficient as the mesh only calculates normals based on the triangles it has 
        // so the vertex normals on the edge are calculated without consider the adjacent mesh's edge triangles and thus may not create a seamless connection
        // This can be solved be expanding the mesh by 1 vertex on each side and creating pseudo normals by calculating the triangles generated by these extra vertices
        // Optimization can also be done if the adjacent mesh is a continuing mesh by not calculating again for the connecting side of the mesh.
        mesh.normals = normals;
        return mesh;
    }

    private Vector3[] CalculateNormals()
    {
        Vector3[] l_normals = new Vector3[vertices.Length];

        int trianglesCount = triangles.Length / 3;
        for (int i = 0; i < trianglesCount; i++)
        {
            int normalIndex = i * 3;

            int indexA = triangles[normalIndex];
            int indexB = triangles[normalIndex + 1];
            int indexC = triangles[normalIndex + 2];

            Vector3 surfaceNormal = CalculateTriangleSurfaceNormal(indexA,
                                                                   indexB,
                                                                   indexC);

            l_normals[indexA] += surfaceNormal;
            l_normals[indexB] += surfaceNormal;
            l_normals[indexC] += surfaceNormal;
        }

        trianglesCount = pseudoTriangles.Length / 3;
        for (int i = 0; i < trianglesCount; i++)
        {
            int normalIndex = i * 3;

            int indexA = pseudoTriangles[normalIndex];
            int indexB = pseudoTriangles[normalIndex + 1];
            int indexC = pseudoTriangles[normalIndex + 2];

            Vector3 surfaceNormal = CalculateTriangleSurfaceNormal(indexA,
                                                                   indexB,
                                                                   indexC);


            if (indexA >= 0)
                l_normals[indexA] += surfaceNormal;
            if (indexB >= 0)
                l_normals[indexB] += surfaceNormal;
            if (indexC >= 0)
                l_normals[indexC] += surfaceNormal;
        }

        for (int i = 0; i < l_normals.Length; i++)
        {
            l_normals[i].Normalize();
        }

        return l_normals;
    }

    private Vector3 CalculateTriangleSurfaceNormal(int indexA, int indexB, int indexC)
    {
        Vector3 A = indexA < 0 ? pseudoVertices[-indexA - 1] : vertices[indexA];
        Vector3 B = indexB < 0 ? pseudoVertices[-indexB - 1] : vertices[indexB];
        Vector3 C = indexC < 0 ? pseudoVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = B - A;
        Vector3 sideAC = C - A;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    internal void BakeNormals()
    {
        normals = CalculateNormals();
    }
}