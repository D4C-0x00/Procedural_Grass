using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Test_2 : MonoBehaviour
{
    public Material Mat;

    public Mesh Mesh;

    Bounds Bounds;

    ComputeBuffer _argsBuffer;

    void Start()
    {
        GetBound();
        //string value = "";
        //
        //for (int i = 0; i < Mesh.triangles.Length; i++) 
        //{
        //    value += $"tri[{i}]={Mesh.triangles[i]}, uv={Mesh.uv[Mesh.triangles[i]]}" + "\n";
        //
        //
        //}
        //Debug.Log(value);
        //
        //
        //
        //_argsBuffer = new ComputeBuffer(1, sizeof(int) * 4);
        //
        //_argsBuffer.SetData(new int[] {Mesh.triangles.Length, 1,0,0 });
        //
        //
        //ComputeBuffer grassMeshUvs = new ComputeBuffer(Mesh.uv.Length, sizeof(float) * 2);
        //
        //grassMeshUvs.SetData(Mesh.uv);
        //
        //Mat.SetBuffer("_Uvs", grassMeshUvs);
        //ComputeBuffer triangles = new ComputeBuffer(Mesh.triangles.Length,sizeof(int));
        //triangles.SetData(Mesh.triangles);
        //Mat.SetBuffer("_Triangles", triangles);

        _argsBuffer = new ComputeBuffer(1, sizeof(int) * 4);
        _argsBuffer.SetData(new int[] { 6, 1, 0, 0 }); // 2个三角形

    }
    private void GetBound()
    {

        Vector3 terrainSize = new Vector3(300,300,300);
        Vector3 terrainPos = Vector3.zero;
        Vector3 center = terrainPos + terrainSize * 0.5f; // 地形中心
        Vector3 size = terrainSize * 10;                // 放大包围盒方便观察调试
        Bounds = new Bounds(center, size);
    }
    // Update is called once per frame
    void Update()
    {
        Graphics.DrawProceduralIndirect(Mat, Bounds, MeshTopology.Triangles, _argsBuffer,
           0, null, null, ShadowCastingMode.Off, true, gameObject.layer);

        
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }
}
