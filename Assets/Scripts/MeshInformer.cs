using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshInformer : MonoBehaviour
{
    Mesh M;
    public bool bTellUVs;
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter MF = GetComponent<MeshFilter>();
        if (MF == null)
        {
            Debug.Log("Mesh Filter is Not Found");
        }
        else{
            M = MF.mesh;

            if (M == null) Debug.Log("No Mesh Found");
            else{
                Debug.Log("VertexCount:" +  M.vertexCount);
                Debug.Log("SubmeshCount:" +  M.subMeshCount);
                Debug.Log("TrianglesCount:" +  M.triangles.Length);
                if(bTellUVs) TellUVs();
            }
        }
    }

    void TellUVs()
    {
        if (M)
        {
            Debug.Log("vert size:" + M.vertices.Length);
            Debug.Log("UV size:" + M.uv.Length);
            for (int i = 0; i < M.uv.Length; i++)
            {
                Debug.Log("uv: " + M.uv[i]);
            }
        }
    }
}
