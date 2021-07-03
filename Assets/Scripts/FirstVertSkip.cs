using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Linq;
using UnityEngine.XR;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FirstVertSkip : MonoBehaviour
{
    // Start is called before the first frame update
    public float minimumDistance = 0.2f;   // At what distance does a single vert get removed?
    public bool updateMeshColliderToo = false;
    public bool permanentMeshDestruction = false;
    public GameObject destroyer;
    public Transform destroyerObjectT; // Used to track the position of the destroyer
    public MeshFilter filter;
    public MeshCollider myCollider;
    Mesh mesh;
    Vector3[] vertices;
    Vector3[] normals;
    Vector2[] uvs;
    int[] triangles;
    bool[] trianglesDisabled;
    List <int>[] trisWithVertex;

    Vector3[] origvertices;
    Vector3[] orignormals;
    Vector2[] origuvs;
    int[] origtriangles;

    BoxCollider Box;

    public bool ColliderOptimiserOff = false;
    public float CollisionBoxScaler = 2f;
    public float CollisionBoxBuffer = 0.5f;
    
    public InputDevice Controller;
    public float ClickTimer = 1f;
    public float ClickMax = 1f;

    void Start()
    {
        Stopwatch S = new Stopwatch();
        S.Start();
        mesh = new Mesh();

        filter = GetComponent<MeshFilter>();
        myCollider = GetComponent<MeshCollider>();
        destroyer = GameObject.FindGameObjectWithTag("Destroyer");
        destroyerObjectT = destroyer.transform;

        origvertices = filter.mesh.vertices;
        orignormals = filter.mesh.normals;
        origuvs = filter.mesh.uv;
        origtriangles = filter.mesh.triangles;

        vertices = new Vector3[origvertices.Length];
        normals = new Vector3[orignormals.Length];
        uvs = new Vector2[origuvs.Length];

        triangles = new int[origtriangles.Length];
        trianglesDisabled = new bool[origtriangles.Length];

        origvertices.CopyTo(vertices, 0);
        orignormals.CopyTo(normals, 0);
        origuvs.CopyTo(uvs, 0);
        origtriangles.CopyTo(triangles, 0);

        trisWithVertex = new List<int>[origvertices.Length];

        for (int i = 0; i < origvertices.Length; ++i)
        {
            trisWithVertex[i] = origtriangles.IndexOf(i);
        }

        filter.mesh = GenerateMeshWithHoles();

        if (!ColliderOptimiserOff)
        {
            Box = gameObject.AddComponent<BoxCollider>();           // Instantiate a Box Collider component;

            Vector3 BoxSize = Box.size;
            BoxSize *= CollisionBoxScaler;
            BoxSize.x += CollisionBoxBuffer;
            BoxSize.y += CollisionBoxBuffer;
            BoxSize.z += CollisionBoxBuffer;
            Box.size = BoxSize;
        }

        List<InputDevice> devices = new List<InputDevice>();

        InputDeviceCharacteristics characteristics = InputDeviceCharacteristics.Right;

        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

        if (devices.Count > 0)
        Controller = devices[0];
        else
        {
            UnityEngine.Debug.Log("No Controller");
            UnityEngine.Debug.Break();
        }

        S.Stop();
        UnityEngine.Debug.Log(string.Format("VertSkip Setup on {0} took {1} ms to complete", gameObject.name, S.ElapsedMilliseconds));
    }

    // Update is called once per frame
    void Update()
    {
        ClickTimer += Time.deltaTime;
        if (ColliderOptimiserOff)
        {
            Stopwatch S = new Stopwatch();
            S.Start();
            filter.mesh = GenerateMeshWithHoles();
            S.Stop();
            UnityEngine.Debug.Log(string.Format("VertSkip Destruction took {0} ms to complete", S.ElapsedMilliseconds));
        }

        if (Controller.TryGetFeatureValue(CommonUsages.primaryButton,out bool primaryButtonValue))
        {
            if (primaryButtonValue && ClickTimer > ClickMax) 
            {
                permanentMeshDestruction = !permanentMeshDestruction;
                ClickTimer = 0f;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Destroyer")
        {
            Stopwatch S = new Stopwatch();
            S.Start();
            filter.mesh = GenerateMeshWithHoles();
            S.Stop();
            UnityEngine.Debug.Log(string.Format("VertSkip Destruction took {0} ms to complete", S.ElapsedMilliseconds));
        }
    }

    Mesh GenerateMeshWithHoles()
    {
        Vector3 trackPos = destroyerObjectT.position;
        for (int i = 0; i <origvertices.Length; ++i)
        {
            Vector3 v = transform.TransformPoint(origvertices[i]);
            if ((v - trackPos).magnitude < minimumDistance)
            {
                for (int j = 0; j <trisWithVertex[i].Count; ++j)
                {
                    int value = trisWithVertex[i][j];
                    int remainder = value % 3;
                    trianglesDisabled[value - remainder] = true;
                    trianglesDisabled[value - remainder + 1] = true;
                    trianglesDisabled[value - remainder + 2] = true;
                }
            }
        }
        triangles = origtriangles;
        triangles = triangles.RemoveAllSpecifiedIndicesFromArray(trianglesDisabled).ToArray();

        mesh.SetVertices(vertices.ToList<Vector3>());
        mesh.SetNormals(normals.ToList());
        mesh.SetUVs(0, uvs.ToList());
        mesh.SetTriangles(triangles, 0);
        
        // This is where the disabled triangles are reenabled.
        // We may not want permanent destruction.
        // By default, destruction is only temporary.
        if (!permanentMeshDestruction)
        for (int i = 0; i <trianglesDisabled.Length; ++i)
            trianglesDisabled[i] = false;

        // This is where the shared mesh used in physics is disabled.
        if (updateMeshColliderToo)
        myCollider.sharedMesh = mesh;
        
        return mesh;
    }
}
