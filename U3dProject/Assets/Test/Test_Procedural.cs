using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Test_Procedural : MonoBehaviour
{
    public Material Mat;
    public Mesh Mesh;
    ComputeBuffer _argsBuffer;

    Bounds bounds;
    void Start()
    {

        bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        _argsBuffer = new ComputeBuffer(1, sizeof(int) * 4,ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(new int[] { Mesh.triangles.Length, 1, 0, 0 }); // 2¸öÈý½ÇÐÎ

        ComputeBuffer Uvs = new ComputeBuffer(Mesh.uv.Length,sizeof(float)*2);
        Uvs.SetData(Mesh.uv);
        Mat.SetBuffer("_Uvs", Uvs);

        ComputeBuffer Triangles = new ComputeBuffer(Mesh.triangles.Length, sizeof(int));
        Triangles.SetData(Mesh.triangles);
        Mat.SetBuffer("_Triangles", Triangles);

    }
    void Update()
    {
        Graphics.DrawProceduralIndirect(Mat, bounds, MeshTopology.Triangles, _argsBuffer);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}