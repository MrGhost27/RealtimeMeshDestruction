using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCreator3 : MonoBehaviour
{
    // Start is called before the first frame update
    public Mesh mesh;
    public MeshFilter filter;
    //public Texture texture;

    public Vector3[] vertices;
    public Vector3[] normals;
    public Vector2[] UVs;
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
        normals = new Vector3[numVerts];
        UVs = new Vector2[numVerts];
        triangles = new int[numTriangles];

        // Textures
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        //float TexWidthIncrement = 1 / width * renderer.material.mainTexture.width;
        float dividend = 1;
        float TexWidthIncrement = dividend / width;
        //float TexHeightIncrement = 1 / height * renderer.material.mainTexture.height;
        float TexHeightIncrement = dividend / height;


        int vertIndex = 0;
        int triIndex = 0;
        int UVIndex = 0;
        int NormalIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                vertices[vertIndex++] = new Vector3(x, y, 0);             // Bottom Left;
                vertices[vertIndex++] = new Vector3(x, y + 1, 0);         // Top Left;
                vertices[vertIndex++] = new Vector3(x + 1, y, 0);         // Bottom Right;
                vertices[vertIndex++] = new Vector3(x + 1, y + 1, 0);     // Top Left;

                triangles[triIndex++] = (y * 4 * width) + x * 4;
                triangles[triIndex++] = (y * 4 * width) + x * 4 + 1;
                triangles[triIndex++] = (y * 4 * width) + x * 4 + 2;
                triangles[triIndex++] = (y * 4 * width) + x * 4 + 3;
                triangles[triIndex++] = (y * 4 * width) + x * 4 + 2;
                triangles[triIndex++] = (y * 4 * width) + x * 4 + 1;

                // Texture UVs
                UVs[UVIndex++] = new Vector2(
                    TexWidthIncrement * x,
                    TexHeightIncrement * y
                    );
                // Top Left
                UVs[UVIndex++] = new Vector2(
                    TexWidthIncrement * x,
                    TexHeightIncrement * (y+1)
                    );
                // Bottom Right
                UVs[UVIndex++] = new Vector2(
                    TexWidthIncrement * (x + 1),
                    TexHeightIncrement * y
                    );
                // Top Right
                UVs[UVIndex++] = new Vector2(
                    TexWidthIncrement * (x + 1),
                    TexHeightIncrement * (y + 1)
                    );

                //Normals
                normals[NormalIndex++] = new Vector3(0, 0, -1);
                normals[NormalIndex++] = new Vector3(0, 0, -1);
                normals[NormalIndex++] = new Vector3(0, 0, -1);
                normals[NormalIndex++] = new Vector3(0, 0, -1);
            }
        }


        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = UVs;
    }
}

