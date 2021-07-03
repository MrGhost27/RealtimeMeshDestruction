using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCreator2 : MonoBehaviour
{
    // Start is called before the first frame update
    public Mesh mesh;

    public Vector3[] vertices;
    public int[] triangles;

    public int height = 1;
    public int width = 1;
    List<Vector3> VertCoords;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;

        // Origin
        int numVerts = height * width * 4;
        int numTriangles = height * width * 6;

        vertices = new Vector3[numVerts];
        triangles = new int[numTriangles];

        int vertIndex = 0;
        int triIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                vertices[vertIndex++] = (new Vector3(x, y, 0));             // Bottom Left;
                vertices[vertIndex++] = (new Vector3(x, y+1, 0));             // Top Left;
                vertices[vertIndex++] = (new Vector3(x+1, y, 0));             // Bottom Right;
                vertices[vertIndex++] = (new Vector3(x+1, y+1, 0));             // Top Left;

                triangles[triIndex++] = (y * 4*width) + x*4;
                triangles[triIndex++] = (y * 4*width) + x*4+1;
                triangles[triIndex++] = (y * 4*width) + x*4 + 2;
                triangles[triIndex++] = (y * 4*width) + x*4 + 3;
                triangles[triIndex++] = (y * 4*width) + x*4 + 2;
                triangles[triIndex++] = (y * 4*width) + x*4 + 1;
            }
        }


        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
}
