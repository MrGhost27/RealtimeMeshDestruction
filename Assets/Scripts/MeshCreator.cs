using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCreator : MonoBehaviour
{
    // Start is called before the first frame update
    public Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        // 4 Vertices
        vertices = new Vector3[]
        {
             new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,0,0),
             new Vector3(1,1,0)
        };

        triangles = new int[]
        {
            0, 1, 2, 3, 2, 1
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
}
