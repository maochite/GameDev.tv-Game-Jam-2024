using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof(MeshFilter))]
public class VoxelUV : MonoBehaviour
{
    [Range(0, 1024)]
    public int index = 0;

    void OnValidate()
    {
        List<Vector3> uvs = new();
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        mesh.GetUVs(0, uvs);
        for(int i = 0; i < uvs.Count; i++)
        {
            uvs[i] = new Vector3(uvs[i].x, uvs[i].y, index);
        }

        mesh.SetUVs(0, uvs);

    }
}
