using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarSlicer : MonoBehaviour
{
    [SerializeField]
    private bool edgeSet = false;
    [SerializeField] private Vector3 edgeVertex = Vector3.zero;
    [SerializeField] private Vector2 edgeUV = Vector2.zero;
    [SerializeField] private Plane edgePlane = new Plane();
    [SerializeField] private Transform trackedObject; // Which object are you checking for collision
    [SerializeField] private float minDistance;       // How close do you want to instigate an effect
    [SerializeField] private float bufferDistance = 0;       // How close do you want to instigate an effec
    [SerializeField] Matrix4x4 localToWorld;


    //public int CutCascades = 1;
    public float ExplodeForce = 0;

    // Start is called before the first frame update
    void Start()
    {
        localToWorld = transform.localToWorldMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space Pressed");
            DestroyMesh();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DestroyMesh();
        Debug.Log("Mesh Collided and Smashed");
    }

    private void DestroyMesh()
    {
        var originalMesh = GetComponent<MeshFilter>().mesh; // Capture Mesh state pre slice.
        originalMesh.RecalculateBounds();                   // Mesh Bounds are relevant later... They're used for defining a cut's position.  We should be defining the plane, not random bounds.
        /// Actually, we're not using a Plane to cut at all anymore?
        var parts = new List<PartMesh>();                   // A List of Part Meshses
        var subParts = new List<PartMesh>();                // A second list of Part Meshes

        var mainPart = new PartMesh()                                   // Begin by collecting the Pre Destroy data.
        {
            UV = originalMesh.uv,
            Vertices = originalMesh.vertices,
            Normals = originalMesh.normals,
            Triangles = new int[originalMesh.subMeshCount][],
            Bounds = originalMesh.bounds
        };

        for (int i = 0; i < originalMesh.subMeshCount; i++)             // Cycle through all submeshes...
            mainPart.Triangles[i] = originalMesh.GetTriangles(i);       // Collect all mesh triangles (Presumably only achieved by including sub-meshes).

        parts.Add(mainPart);                                            // After collecting all triangles We have the PRE CUT STATE!

        // CASCADE CUTS (We don't want cascade cuts, we'll instead want to repeat the whole processin real time... So we'll introduce more lag. This is expected.


        for (var i = 0; i < parts.Count; i++)                       // For each Submesh
        {
            //var bounds = parts[i].Bounds;                           // Check the bounds of the Sub Mesh.  (Begs the question... Each Sub Mesh has it's own random Cut. We need test models with Submeshes?)
            //bounds.Expand(0.5f);                                    // Half the bounds to limit the random plane effects... This shouldn't be used because we'll be defining the plane

            //var plane = new Plane(UnityEngine.Random.onUnitSphere, new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                                                                               //UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                                                                               //UnityEngine.Random.Range(bounds.min.z, bounds.max.z)));

            // We're fundamentally using 2 opposing systems.
            // We need to implement the Schwerpunkt.  Spear Point tip, and 3 Planes.
            // Also, the Plane cut is an infinite plane...
            // We don't want to infinitely cut.
            // We don't want to cut at all, do we?  We want to add new Vert information. We're rebuilding around the sword, as long as it's inside the mesh?

            // We'll see how this method is used to generate meshes along the cut face.

            // Original Project intends to Bisect the mesh into 2 parts.
            // We actually want to annihilate the mesh inside the space?
            // It's possible I've kept the wrong side, in which case, swap them round...
            // But we'll try and get it working first, shall we...

            subParts.Add(GenerateMesh(parts[i], true));
            //subParts.Add(GenerateMesh(parts[i], plane, false));
        }
        parts = new List<PartMesh>(subParts);
        subParts.Clear();

        for (var i = 0; i < parts.Count; i++)
        {
            parts[i].MakeGameobject(this, trackedObject, minDistance);
            //parts[i].GameObject.GetComponent<Rigidbody>().AddForceAtPosition(parts[i].Bounds.center * ExplodeForce, transform.position);
        }

        Destroy(gameObject);
    }

    private PartMesh GenerateMesh(PartMesh original, bool left)
    {
        var partMesh = new PartMesh() { };
        //var ray1 = new Ray();
        //var ray2 = new Ray();

        Vector3 trackPos = trackedObject.position;      // World space Position of the Destroyer Object
        //trackPos = trackedObject.TransformPoint(trackPos);

        //  for the 2 directions of the Plane? There is no plane.


        for (var i = 0; i < original.Triangles.Length; i++)
        {
            var triangles = original.Triangles[i];      // For each Pre Cut Triangle.
            edgeSet = false;                            // ... We haven't yet set an edge. What is an Edge?

            for (var j = 0; j < triangles.Length; j = j + 3)    // Increment in 3s, 
            {
                // WHAT EXACT PARAMETER ARE WE PASSING HERE?  THIS IS WHERE WE WANT TO MAKE OUR OWN CHECKS.

                // Is the Vert of the triangle too close to the Slicing Object?
                // FIRST CHANGE OF THIS WILL BE DISTANCE TO A POINT.
                // THEN WE CAN TRANSITION TO DISTANCE TO A LINE, AND THEN DISTANCE INSIDE BLADE LENGTH?

                // Get a vertex and put it in the world.  Mesh Coords to World.

                Vector3 v1 = transform.TransformPoint(original.Vertices[triangles[j]]);
                Vector3 v2 = transform.TransformPoint(original.Vertices[triangles[j+1]]);
                Vector3 v3 = transform.TransformPoint(original.Vertices[triangles[j+2]]);

                //Vector3 v1 = original.Vertices[triangles[j]];
                //Vector3 v2 = original.Vertices[triangles[j+1]];
                //Vector3 v3 = original.Vertices[triangles[j+2]];

                // if ((v + transform.position - trackPos).magnitude < minDistance)

                // If it's inside the minimum distance, it's too close... So it needs cutting...?
                // Get a Vector from Cutter to World Vertex position.
                Vector3 CutterToV1 = (v1 - trackPos);
                Vector3 CutterToV2 = (v2 - trackPos);
                Vector3 CutterToV3 = (v3 - trackPos);

                // Check if Cutter to World Vertex is too lose.
                bool sideA = CutterToV1.magnitude < minDistance;  // True == cut
                bool sideB = CutterToV2.magnitude < minDistance;
                bool sideC = CutterToV3.magnitude < minDistance;

                // Vert A to Vert B line is to be kept? (Called Left)?
                // + Vert B to Vert C line is on the left?
                // + Vert C to Vert A line is on the left?
                var sideCount = (sideA ? 1 : 0) +
                                        (sideB ? 1 : 0) +
                                        (sideC ? 1 : 0);
                // If ALL the verts are onthe right.  Ignore it.  Don't Add it to the new LEFT OF CUT Mesh.
                // We're now saying, If ALL 3 VERTS ARE TOO CLOSE, DON'T ADD THEM TO THE NEW LIST OF EXISTING VERTS. IT'S INVERTED LOGIC.

                if (sideCount == 0) // If ALL the verts are "on the left"... To be kept.
                {
                    // Copy the Verts, Normals and texture UVs. I.E. Keep the data.
                    partMesh.AddTriangle(i,
                                         original.Vertices[triangles[j]], original.Vertices[triangles[j + 1]], original.Vertices[triangles[j + 2]],
                                         original.Normals[triangles[j]], original.Normals[triangles[j + 1]], original.Normals[triangles[j + 2]],
                                         original.UV[triangles[j]], original.UV[triangles[j + 1]], original.UV[triangles[j + 2]]);
                    continue;
                }
                if (sideCount == 3)
                {
                    // This will create a big hole in the mesh.  Entire Triangles will be removed.
                    continue;
                }

                //Debug.Log("Should Cut a Mesh Triangle in half.  This means new Vert information, etc.");
                //cut points

                bool make2Triangles = false;

                // If we only cut 1 point, we make a quad.
                if (sideCount==1)
                {
                    make2Triangles = true;
                    Debug.Log("Cutting 1 Vert, so 2 new triangles");
                }

                List<Vector3> NewVerts = new List<Vector3>();

                //Vector3 PrintVertex1 = original.Vertices[triangles[j]];
                //Vector3 PrintVertex2 = original.Vertices[triangles[j+1]];
                //Vector3 PrintVertex3 = original.Vertices[triangles[j+2]];

                // ONly needed if make2Triangles is positive, we'll need 4 verts to make a quad.

                bool Cut1 = false;
                bool Cut2 = false;
                bool Cut3 = false;

                int sideAVerts = 0;
                int sideBVerts = 0;
                int sideCVerts = 0;

                // Get where the position of the World Vertex should be.
                if (sideA)
                {
                    //Debug.Log("Cutter Magnitude start: " + CutterToV1.magnitude);
                    //CutterToV1.Normalize();
                    //Debug.Log("Cutter Normalized Magnitude: " + CutterToV1.magnitude);
                    //CutterToV1 *= (minDistance + bufferDistance);
                    //Debug.Log("Cutter Scaled to Min Distance Magnitude: " + CutterToV1.magnitude);
                    ////CutterToV1 = CutterToV1.normalized * (minDistance + bufferDistance);
                    ////PrintVertex1 += v1 - CutterToV1;
                    //PrintVertex1 = transform.InverseTransformPoint(trackPos + CutterToV1);
                    ////PrintVertex1 = CutterToV1;
                    //Debug.Log(String.Format("VertexBefore: {0}, VertexAfter: {1}", original.Vertices[triangles[j]], PrintVertex1));

                    // Check whether B or C is also being culled.  If so, DON'T Provide a new vertex on that side.

                    // If Vert B is kept... Move towards it.
                    if (!sideB)
                    {
                        NewVerts.Add(transform.InverseTransformPoint(DetermineNewDistancePosition(v2, v1, trackPos)));
                        ++sideAVerts;
                    }
                    if (!sideC)    // If it's not being kept
                    {
                        NewVerts.Add(transform.InverseTransformPoint(DetermineNewDistancePosition(v3, v1, trackPos)));
                        ++sideAVerts;
                    }

                    Cut1 = true;
                }
                else
                {
                    //NewVerts.Add(transform.InverseTransformPoint(v1));
                    NewVerts.Add(original.Vertices[triangles[j]]);
                    //Debug.Log("Kept Vert 1");
                }


                if (sideB)
                {
                    if (!sideA)
                    {
                        NewVerts.Add(transform.InverseTransformPoint(DetermineNewDistancePosition(v1, v2, trackPos)));
                        ++sideBVerts;
                    }
                    if (!sideC)    // If it's not being kept
                    {
                        NewVerts.Add(transform.InverseTransformPoint(DetermineNewDistancePosition(v3, v2, trackPos)));
                        ++sideBVerts;
                    }

                    Cut2 = true;

                    //CutterToV2.Normalize();
                    //CutterToV2 *= (minDistance + bufferDistance);
                    //CutterToV2 = CutterToV2.normalized * (minDistance + bufferDistance);
                    //PrintVertex2 += v2 - CutterToV2;
                    //PrintVertex2 = transform.InverseTransformPoint(CutterToV2);
                    //PrintVertex2 = transform.InverseTransformPoint(trackPos + CutterToV2);
                    //PrintVertex2 = CutterToV2;
                }
                else
                {
                    //NewVerts.Add(transform.InverseTransformPoint(v2));
                    NewVerts.Add(original.Vertices[triangles[j+1]]);
                    //Debug.Log("Kept Vert 2");
                }

                if (sideC)
                {
                    if (!sideB)
                    {
                        NewVerts.Add(transform.InverseTransformPoint(DetermineNewDistancePosition(v2, v3, trackPos)));
                        ++sideCVerts;
                    }
                    if (!sideA)    // If it's not being kept
                    {
                        NewVerts.Add(transform.InverseTransformPoint(DetermineNewDistancePosition(v1, v3, trackPos)));
                        ++sideCVerts;
                    }
                    Cut3 = true;
                    //CutterToV3.Normalize();
                    //CutterToV3 *= (minDistance + bufferDistance);
                    //PrintVertex3 += v3 - CutterToV3;
                    //PrintVertex3 = transform.InverseTransformPoint(CutterToV3);
                    //PrintVertex3 = transform.InverseTransformPoint(trackPos + CutterToV3);
                    //PrintVertex3 = CutterToV3;
                }
                else
                {
                    //NewVerts.Add(transform.InverseTransformPoint(v3));
                    NewVerts.Add(original.Vertices[triangles[j + 2]]);
                    //Debug.Log("Kept Vert 3");
                }

                string DebugString = "";
                DebugString += "NumberOfNewVerts: " + NewVerts.Count;
                DebugString += String.Format("\nCut1: {0}, Cut2: {1}, Cut3: {2}", Cut1, Cut2, Cut3);
                DebugString += "\n" + (String.Format("\nVerts1: {0}, Verts2: {1}, Verts3: {2}", sideAVerts, sideBVerts, sideCVerts));
                Debug.Log(DebugString);

                if (!make2Triangles)
                {
                    partMesh.AddTriangle(i,
                                         NewVerts[0], NewVerts[1], NewVerts[2],
                                         original.Normals[triangles[j]], original.Normals[triangles[j + 1]], original.Normals[triangles[j + 2]],
                                         original.UV[triangles[j]], original.UV[triangles[j + 1]], original.UV[triangles[j + 2]]);

                }
                else
                {
                    Debug.Log("HOWMANYVERTSINLIST: " + NewVerts.Count);
                    partMesh.AddTriangle(i,
                                         NewVerts[0], NewVerts[1], NewVerts[2],
                                         original.Normals[triangles[j]], original.Normals[triangles[j + 1]], original.Normals[triangles[j + 2]],
                                         original.UV[triangles[j]], original.UV[triangles[j + 1]], original.UV[triangles[j + 2]]);

                    partMesh.AddTriangle(i,
                                         NewVerts[0], NewVerts[2], NewVerts[3],
                                         original.Normals[triangles[j+1]], original.Normals[triangles[j + 2]], original.Normals[triangles[j]],
                                         original.UV[triangles[j+1]], original.UV[triangles[j + 2]], original.UV[triangles[j]]);
                }

                NewVerts.Clear();
                // Convert World Vertex positions to Mesh Coords.




                // Use of the CONTINUE Keyword ensures this algorithm is only reached when the triangles are bisected by the planar cut.
                // 
                //  THIS IS THE CUTTING FACE WE NEED TO DEAL WITH. Hussah.
                //
                // If both Side B and Side C are on the same side... index is 0. Dealing with Vert 1 of 3 of the Triangle. The Slice potentially cuts A off from the others.
                // If they're NOT on the same side... is side A and side C on the same side? IF SO, Index = 1. Then the slice cuts B off from the others.
                // But if neither A nor B are on the same side as C, then the slice cuts C off from the others.
                //var singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;

                /// We need to determind what is to be cut. Single vert 1, 2 or 3.  Or Double verts 1.2, 1.3, 2.3
                /// We need 2 points regardless.  Any way you cut a triangle results in 2 new points? Which side has the new points

                /// Index 0 = Verts 2 and 3 are the same state (Keep or Cull).
                /// Index 1 = Verts 1 and 3 are the same state
                /// Index 2 = Verts 1 and 2 are the same state

                //var newVec2 = (v1 + v2) * 0.5f;
                //var newVec3 = (v1 + v3) * 0.5f;
                //var newVert2 = (original.Vertices[triangles[j]] + original.Vertices[triangles[j + 1]]) * 0.5f;
                //var newVert3 = (original.Vertices[triangles[j]] + original.Vertices[triangles[j + 2]]) * 0.5f;

                //ray1.origin = original.Vertices[triangles[j + singleIndex]];
                // Begin the Ray at Triangle that will be removed.
                //var dir1 = original.Vertices[triangles[j + ((singleIndex + 1) % 3)]] - original.Vertices[triangles[j + singleIndex]];
                // Point the ray at the next index (Remembering to loop from +2 to +0).

                //ray1.direction = dir1;      // Set Destination - starting point = direction.
                //plane.Raycast(ray1, out var enter1);        // Declaration of a new local variable "Enter1". Which is the distance between the Vert we're cutting off and the Plane used to bisect the mesh.
                /*
                 WE CAN'T USE THIS FUNCTION.  WE HAVE 2 POINTS IN SPACE, AND WE WANT TO DETERMINE AT WHAT THIRD POINT IN SPACE DOES IT GET TOO CLOSE TO OUR POINT?
                 */
                //var lerp1 = enter1 / dir1.magnitude;        //  The float Distance divided by the Vector of the Ray. So much in the X axis, so much in the Y, and Z.  So how far in each Axis to the plane.

                // We'll need to repeat the process again to get the distance between the First Vertex and the THIRD Vertex. Until it reaches the plane that is.

                //ray2.origin = original.Vertices[triangles[j + singleIndex]];    // Same starting point.  The Isolated Vertex.
                //var dir2 = original.Vertices[triangles[j + ((singleIndex + 2) % 3)]] - original.Vertices[triangles[j + singleIndex]];   // Same principle as above, but with THIRD vertex. (Remember Modulus to loop)
                //ray2.direction = dir2;
                //plane.Raycast(ray2, out var enter2);
                //var lerp2 = enter2 / dir2.magnitude;

                // AT THIS POINT, WE HAVE THE DISTANCE BETWEEN THE ISOLATED VERTEX AND THE PLANE, IN HANDY Vec3 FORM, FOR BOTH OTHER VERTS OF THE TRIANGLE.  
                // ^^ ISH.  The local variables lerp1 and lerp2 are a measure of how far along the original triangle they go.  So we can LERP the UV Texture values.  Handy that.
                //  ...SO WE CAN IN THEORY CREATE A NEW TRIANGLE THAT WILL STOP AT THE PLANE, REPEAT FOR ALL TRIs IN THE MESH.

                //first vertex = ancor
                // Instead of adding the original triangle, as happens above...

                //int IsolatedVertIndex1 = singleIndex;   // Kept or Removed? Not determined yet.
                //int KeptVert2 = (singleIndex + 1) % 3;
                //int KeptVert3 = (singleIndex + 2) % 3;

                //Vector3 IVPosition1 = transform.TransformPoint(original.Vertices[triangles[IsolatedVert1]]);
                //Vector3 IVPosition2 = transform.TransformPoint(original.Vertices[triangles[IsolatedVert2]]);
                //Vector3 IVPosition3 = transform.TransformPoint(original.Vertices[triangles[IsolatedVert3]]);

                // Not sure this is what was meant.
                //Vector3 IVPosition1 = original.Vertices[triangles[IsolatedVertIndex1]];
                //Vector3 IVPosition2 = original.Vertices[triangles[KeptVert2]];
                //Vector3 IVPosition3 = original.Vertices[triangles[KeptVert3]];

                // At this point. We don't want real world positions do we? What does Add Edge do?
                //Vector3 RealWorldIVPosition1 = localToWorld.MultiplyPoint3x4(IVPosition1);
                //Vector3 RealWorldIVPosition2 = localToWorld.MultiplyPoint3x4(IVPosition2);
                //Vector3 RealWorldIVPosition3 = localToWorld.MultiplyPoint3x4(IVPosition3);

                // What 

                //AddExtraEdge();

                //AddEdge(i,
                //        partMesh,
                //        left ? plane.normal * -1f : plane.normal,
                //        //Vector2.Lerp(original.Vertices[triangles[j + singleIndex]], original.Vertices[triangles[j + ((singleIndex + 1) % 3)]], 0.5f),
                //        //Vector2.Lerp(original.Vertices[triangles[j + singleIndex]], original.Vertices[triangles[j + ((singleIndex + 2) % 3)]], 0.5f),
                //        Vector3.Lerp(IVPosition1, IVPosition2, 0.5f),
                //        Vector3.Lerp(IVPosition1, IVPosition3, 0.5f),
                //        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], 0.5f),
                //        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], 0.5f));

                //  Invert the Normal of the triangle if it's on the Left face.  I'm going to assume that works, because it seems to...
                // Adding and Edge... This is the newly created edge that runs between Vert 2 and Vert 3 (Relative, remember we looped using Modulus%)
                //  
                //  Vertex Position is the position we worked out above. (//A) and (//B)
                //  Even the Texture information is captured by Lerping the UV vales.

                // Either The verts are ALL left, or None Left. In which case there's no cut, and we never reach this far.
                // Or 1 Vert is left, or 2 verts are left of Bisecting Plane. See Below.

                // IT'S WORTH NOTING, THIS WILL BE DONE ON BOTH SIDES OF THE PLANE??

                // Add one triangle.
                //if (sideCount == 2)
                //{
                //    partMesh.AddTriangle(i,
                //                        original.Vertices[triangles[j + singleIndex]],
                //                        Vector3.Lerp(original.Vertices[triangles[j + singleIndex]], original.Vertices[triangles[j + ((singleIndex + 1) % 3)]], 0.5f),
                //                        Vector3.Lerp(original.Vertices[triangles[j + singleIndex]], original.Vertices[triangles[j + ((singleIndex + 2) % 3)]], 0.5f),
                //                        //newVec2,
                //                        //newVec3,
                //                        original.Normals[triangles[j + singleIndex]],
                //                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], 0.5f),
                //                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % 3)]], 0.5f),
                //                        original.UV[triangles[j + singleIndex]],
                //                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], 0.5f),
                //                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], 0.5f));

                //    continue;
                //}
                ////  Add Two Triangles.
                //if (sideCount == 1)
                //{
                //    partMesh.AddTriangle(i,
                //                        Vector3.Lerp(original.Vertices[triangles[j + singleIndex]], original.Vertices[triangles[j + ((singleIndex + 1) % 3)]], 0.5f),
                //                        original.Vertices[triangles[j + ((singleIndex + 1) % 3)]],
                //                        original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                //                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], 0.5f),
                //                        original.Normals[triangles[j + ((singleIndex + 1) % 3)]],
                //                        original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                //                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], 0.5f),
                //                        original.UV[triangles[j + ((singleIndex + 1) % 3)]],
                //                        original.UV[triangles[j + ((singleIndex + 2) % 3)]]);
                //    partMesh.AddTriangle(i,
                //                        Vector3.Lerp(original.Vertices[triangles[j + singleIndex]], original.Vertices[triangles[j + ((singleIndex + 1) % 3)]], 0.5f),
                //                        original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                //                        Vector3.Lerp(original.Vertices[triangles[j + singleIndex]], original.Vertices[triangles[j + ((singleIndex + 2) % 3)]], 0.5f),
                //                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], 0.5f),
                //                        original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                //                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % 3)]], 0.5f),
                //                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], 0.5f),
                //                        original.UV[triangles[j + ((singleIndex + 2) % 3)]],
                //                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], 0.5f));
                //    continue;
                //}


            }
        }


        // This is great for an Infinitely Scaled, Planar Bisection.  But we actually don't want this...
        // This algorithm works out how to fill the extra space.
        // But We don't want to work out distance to a Plane.
        // We need distance to a Line?  Which isn't a million miles away, is it?

        partMesh.FillArrays();

        return partMesh;
    }

    private Vector3 DetermineNewDistancePosition(Vector3 worldKeptVert, Vector3 worldCulledVert, Vector3 worldPointForDistance)
    {
        // Somewhere along this vector is the position we want. 
        // If we know what ratio of Outside to Inside we have, we can determine how far along this vector to look.
        Vector3 vecBetweenVerts = worldKeptVert - worldCulledVert;

        // Vector between Kept vert and Cutter  Kept ------> Cutter
        Vector3 VectorToCutterofKept = worldPointForDistance - worldKeptVert;
        // Vector between Culled vert and cutter Culled ...>
        Vector3 VectorToCutterofCUlled = worldPointForDistance - worldKeptVert;

        // How far outside the threshold is Kept vert?
        float distanceOutsideMinDistanceOfKeptVert = VectorToCutterofKept.magnitude - (minDistance + bufferDistance);
        // How far inside the threshold is Culled vert? Because we know it's culled, it's inside minDistance to answer will be negative
        float distanceInsideMinDistanceOfCulledVert = VectorToCutterofCUlled.magnitude - minDistance;

        // Distance Outside + Distance inside gives the total distance covered.
        float totalRangeCovered = distanceOutsideMinDistanceOfKeptVert + Mathf.Abs(distanceInsideMinDistanceOfCulledVert);

        // Determine the whole distance Covered, and
        float ratioOfVectorWeWant = totalRangeCovered / distanceOutsideMinDistanceOfKeptVert;
        Debug.Log("ratio: " + ratioOfVectorWeWant);
        vecBetweenVerts *= ratioOfVectorWeWant;

        // The new position is the keptVert position + the ratio 
        return worldKeptVert += vecBetweenVerts;
    }

    private void AddEdge(int subMesh, PartMesh partMesh, Vector3 normal, Vector3 vertex1, Vector3 vertex2, Vector2 uv1, Vector2 uv2)
    {
        if (!edgeSet)
        {
            edgeSet = true;
            edgeVertex = vertex1;   // Only set once per object. Used repeatedly to draw with all the other vertices passed to this function?
            edgeUV = uv1;           // Can this method still be used if we're not Plane slicing?
        }
        else
        {
            edgePlane.Set3Points(edgeVertex, vertex1, vertex2);

            partMesh.AddTriangle(subMesh,
                                edgeVertex,
                                edgePlane.GetSide(edgeVertex + normal) ? vertex1 : vertex2,
                                edgePlane.GetSide(edgeVertex + normal) ? vertex2 : vertex1,
                                normal,
                                normal,
                                normal,
                                edgeUV,
                                uv1,
                                uv2);
        }
    }

    public class PartMesh
    {
        private List<Vector3> _Verticies = new List<Vector3>();
        private List<Vector3> _Normals = new List<Vector3>();
        private List<List<int>> _Triangles = new List<List<int>>();
        private List<Vector2> _UVs = new List<Vector2>();
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public int[][] Triangles;
        public Vector2[] UV;
        public GameObject GameObject;
        public Bounds Bounds = new Bounds();

        public PartMesh()
        {

        }

        public void AddTriangle(int submesh, Vector3 vert1, Vector3 vert2, Vector3 vert3, Vector3 normal1, Vector3 normal2, Vector3 normal3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            if (_Triangles.Count - 1 < submesh)
                _Triangles.Add(new List<int>());

            _Triangles[submesh].Add(_Verticies.Count);
            _Verticies.Add(vert1);
            _Triangles[submesh].Add(_Verticies.Count);
            _Verticies.Add(vert2);
            _Triangles[submesh].Add(_Verticies.Count);
            _Verticies.Add(vert3);
            _Normals.Add(normal1);
            _Normals.Add(normal2);
            _Normals.Add(normal3);
            _UVs.Add(uv1);
            _UVs.Add(uv2);
            _UVs.Add(uv3);

            // Establishing the smallest and largest values used by all 3 verts.  Should define an Axially Aligned cube.
            Bounds.min = Vector3.Min(Bounds.min, vert1);
            Bounds.min = Vector3.Min(Bounds.min, vert2);
            Bounds.min = Vector3.Min(Bounds.min, vert3);
            Bounds.max = Vector3.Min(Bounds.max, vert1);
            Bounds.max = Vector3.Min(Bounds.max, vert2);
            Bounds.max = Vector3.Min(Bounds.max, vert3);
        }

        public void FillArrays()
        {
            Vertices = _Verticies.ToArray();
            Normals = _Normals.ToArray();
            UV = _UVs.ToArray();
            Triangles = new int[_Triangles.Count][];
            for (var i = 0; i < _Triangles.Count; i++)
                Triangles[i] = _Triangles[i].ToArray();
        }

        public void MakeGameobject(PlanarSlicer original, Transform TRef, float minD)
        {
            GameObject = new GameObject(original.name);
            GameObject.transform.position = original.transform.position;
            GameObject.transform.rotation = original.transform.rotation;
            GameObject.transform.localScale = original.transform.localScale;

            var mesh = new Mesh();
            mesh.name = original.GetComponent<MeshFilter>().mesh.name;

            mesh.vertices = Vertices;
            mesh.normals = Normals;
            mesh.uv = UV;
            for (var i = 0; i < Triangles.Length; i++)
                mesh.SetTriangles(Triangles[i], i, true);
            Bounds = mesh.bounds;

            var renderer = GameObject.AddComponent<MeshRenderer>();
            renderer.materials = original.GetComponent<MeshRenderer>().materials;

            var filter = GameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            var collider = GameObject.AddComponent<MeshCollider>();
            //collider.convex = true;

            //var rigidbody = GameObject.AddComponent<Rigidbody>();
            var meshDestroy = GameObject.AddComponent<PlanarSlicer>();
            meshDestroy.ExplodeForce = original.ExplodeForce;
            meshDestroy.trackedObject = TRef;
            meshDestroy.minDistance = minD;
        }

    }
}
