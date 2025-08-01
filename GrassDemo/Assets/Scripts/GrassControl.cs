using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
[Serializable]
public class GrassConfig
{
    [Header("草密度")]
    public int GrassDensity;
}


public class GrassControl : MonoBehaviour
{
    public Terrain Terrain;
    public ComputeShader GrassComputeShader;
    public Material BladeMaterial;
    public Mesh BladMesh;

    public Material ClumpingMaterial;


    public GrassConfig GrassConfig;


    public ClumpingConfigStruct[] ClumpingConfig;


    private ComputeBuffer _argsBuffer;

    /// <summary>
    /// GPU计算出的草数据缓冲区
    /// </summary>
    private ComputeBuffer _grassComputeAppendBuffer;

    private Bounds _grassInstanceBounds;

    private void Start()
    {

        Vector3 terrainSize = Terrain.terrainData.size;
        Vector3 terrainPos = Terrain.GetPosition();
        Vector3 center = terrainPos + terrainSize * 0.5f; // 地形中心
        Vector3 size = terrainSize * 10;                // 放大包围盒方便观察调试
        _grassInstanceBounds = new Bounds(center, size);

        InitDrawGrass();
    }

    private void InitDrawGrass() 
    {
        _grassComputeAppendBuffer = new ComputeBuffer(GrassConfig.GrassDensity* GrassConfig.GrassDensity,sizeof(float)*8,ComputeBufferType.Append);


        GrassComputeShader.SetFloat("_GrassDensity", GrassConfig.GrassDensity);


        GrassComputeShader.SetBuffer(0,"_GrassBlades", _grassComputeAppendBuffer);
        Vector3 terrainSize = Terrain.terrainData.size;
        GrassComputeShader.SetVector("_TerrainSize", terrainSize);




        int width = Terrain.terrainData.heightmapResolution - 1;
        int height = Terrain.terrainData.heightmapResolution - 1;
        var heightMaps = Terrain.terrainData.GetHeights(0, 0, width, height);
        Texture2D terrainheightMap = new Texture2D(width, height, TextureFormat.RFloat, mipChain: false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float h = heightMaps[y, x]; // 0~1
                terrainheightMap.SetPixel(x, y, new Color(h, 0, 0, 0));
            }
        }
        terrainheightMap.Apply();
        GrassComputeShader.SetTexture(0, "_TerrainHeightMap", terrainheightMap);

        _argsBuffer = new ComputeBuffer(1, sizeof(int) * 4, ComputeBufferType.IndirectArguments);

        int[] _argsData = new int[]{BladMesh.triangles.Length, 0,0,0 };
        _argsBuffer.SetData(_argsData);


        ComputeBuffer meshUv = new ComputeBuffer(BladMesh.uv.Length,sizeof(float)*2);
        meshUv.SetData(BladMesh.uv);
        BladeMaterial.SetBuffer("_Uvs", meshUv);


        ComputeBuffer meshTriangles = new ComputeBuffer(BladMesh.triangles.Length,sizeof(int));
        meshTriangles.SetData(BladMesh.triangles);
        BladeMaterial.SetBuffer("_Triangles", meshTriangles);




        Texture2D start2D = new Texture2D(width,height,TextureFormat.RGBAFloat,false,true);
        RenderTexture voronoiTexture = new RenderTexture(width, height,0,RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        voronoiTexture.Create();
        Graphics.Blit(start2D, voronoiTexture, ClumpingMaterial);
        GrassComputeShader.SetTexture(kernelIndex: 0,"_ClumpVoronoiMap", voronoiTexture);

        ComputeBuffer clumpingConfigBuffer = new ComputeBuffer(ClumpingConfig.Length, Marshal.SizeOf(typeof(ClumpingConfigStruct)));
        clumpingConfigBuffer.SetData(ClumpingConfig);

        GrassComputeShader.SetBuffer(0, "_ClumpingConfig", clumpingConfigBuffer);

    }


    private void DrawGrass() 
    {

        _grassComputeAppendBuffer.SetCounterValue(0);

        int threadGroup = Mathf.CeilToInt(GrassConfig.GrassDensity/8f);

        GrassComputeShader.Dispatch(0, threadGroup, threadGroup,1);

        ComputeBuffer.CopyCount(_grassComputeAppendBuffer, _argsBuffer, sizeof(int));

        float[] debugData = new float[_grassComputeAppendBuffer.count*7]; 

        _grassComputeAppendBuffer.GetData(debugData);



        BladeMaterial.SetBuffer("_GrassBlades", _grassComputeAppendBuffer);

        Graphics.DrawProceduralIndirect(BladeMaterial, _grassInstanceBounds,MeshTopology.Triangles, _argsBuffer,0,null,properties: null, ShadowCastingMode.Off, receiveShadows: true,gameObject.layer);
    }


    private void Update()
    {
        DrawGrass();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_grassInstanceBounds.center, _grassInstanceBounds.size);
    }

}
