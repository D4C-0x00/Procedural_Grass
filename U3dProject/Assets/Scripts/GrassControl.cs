using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public struct GrassComputeConfig
{
    public int grassDensity;
    public float grassSpacing;
    public float terrainSizeX;
    public float terrainSizeZ;
    public float terrainSizeY;
}

public class GrassControl : MonoBehaviour
{
    [Header("准备物料")]
    public Terrain Terrain;
    public ComputeShader GrassCompute;
    public Material GrassMaterial;
    public Mesh GrassMesh;
    public Material VoronoiMat;


    ComputeBuffer _grassComputeOutPutBuffer;

    ComputeBuffer _grassComputeInputConfigBuffer;

    ComputeBuffer _grassPatchConfigs;

    ComputeBuffer _argsBuffer;



    [Header("草地基础渲染设置")]
    public GrassRenderSettings GrassSettings;


    [Header("草丛渲染设置")]
    public GrassPatchConfig[] GrassPatchConfigs;

    private Bounds _grassInstanceBounds;

    [SerializeField]
    RenderTexture _voronoiRenderMap;


    void Start()
    {
        GetBound();
        InitDrawGrass();
        InitGrassComputeConfig();
        StartGrassCompute();
    }

    private void GetBound()
    {
        Vector3 terrainSize = Terrain.terrainData.size;
        Vector3 terrainPos = Terrain.GetPosition();
        Vector3 center = terrainPos + terrainSize * 0.5f; // 地形中心
        Vector3 size = terrainSize * 10;                // 放大包围盒方便观察调试
        _grassInstanceBounds = new Bounds(center, size);
    }

    private void InitGrassComputeConfig()
    {
        GrassComputeConfig config = new GrassComputeConfig();


        config.grassDensity = GrassSettings.GrassDensity;

        float grassSpacing = GrassSettings.GrassDensity / Terrain.terrainData.size.x;
        config.grassSpacing = grassSpacing;


        config.terrainSizeX = Terrain.terrainData.size.x;
        config.terrainSizeZ = Terrain.terrainData.size.z;
        config.terrainSizeY= Terrain.terrainData.size.y;

        _grassComputeInputConfigBuffer = new ComputeBuffer(1, System.Runtime.InteropServices.Marshal.SizeOf(typeof(GrassComputeConfig)));
        _grassComputeInputConfigBuffer.SetData(new GrassComputeConfig[] { config });
        GrassCompute.SetBuffer(0, "_config", _grassComputeInputConfigBuffer);


        _grassPatchConfigs = new ComputeBuffer(GrassPatchConfigs.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(GrassPatchConfig)));

        _grassPatchConfigs.SetData(GrassPatchConfigs);


        GrassCompute.SetBuffer(0, "_grassPatchConfigs", _grassPatchConfigs);
    }



    private void InitDrawGrass()
    {
        _grassComputeOutPutBuffer = new ComputeBuffer(GrassSettings.GrassDensity * GrassSettings.GrassDensity, sizeof(float)*7, ComputeBufferType.Append);

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

        GrassCompute.SetTexture(0, "_TerrainHeightMap", terrainheightMap);

        _argsBuffer = new ComputeBuffer(1, sizeof(int) * 4, ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(new int[] { GrassMesh.triangles.Length, 0, 0, 0 });

        Texture2D null2d = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
        _voronoiRenderMap = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        Graphics.Blit(null2d, _voronoiRenderMap, VoronoiMat);

        RenderTexture.active = _voronoiRenderMap;


        Texture2D voronoiTextureMap = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
        voronoiTextureMap.ReadPixels(new Rect(0,0, _voronoiRenderMap.width, _voronoiRenderMap.height),0,0);

        voronoiTextureMap.filterMode = FilterMode.Point;
        voronoiTextureMap.Apply();
        RenderTexture.active = null;

        GrassCompute.SetTexture(0, "_VoronoiTextureMap", voronoiTextureMap);


        ComputeBuffer grassMeshUvs = new ComputeBuffer(GrassMesh.uv.Length,sizeof(float)*2);

        grassMeshUvs.SetData(GrassMesh.uv);

        GrassMaterial.SetBuffer("_Uvs", grassMeshUvs);

        ComputeBuffer triangles = new ComputeBuffer(GrassMesh.triangles.Length, sizeof(int));

        triangles.SetData(GrassMesh.triangles);

        GrassMaterial.SetBuffer("_Triangles", triangles);
    }

    private void StartGrassCompute()
    {
        GrassCompute.SetBuffer(0, "_GrassComputeOutputs", _grassComputeOutPutBuffer);
    }


    private void DrawGrass()
    {

        _grassComputeOutPutBuffer.SetCounterValue(0);
        GrassMaterial.SetBuffer("_GrassComputeOutputs", _grassComputeOutPutBuffer);

        int groups = Mathf.CeilToInt(GrassSettings.GrassDensity / 8f);
        GrassCompute.Dispatch(0, groups, groups, 1);

        ComputeBuffer.CopyCount(_grassComputeOutPutBuffer, _argsBuffer, sizeof(int));

        GrassMaterial.SetBuffer("_GrassComputeOutputs", _grassComputeOutPutBuffer);

        int[] debug=new int[4];

        //_argsBuffer.GetData(debug);
        float[] debugData = new float[sizeof(float)*7]; // 10个float4

        _grassComputeOutPutBuffer.GetData(debugData);

        
        string va = "";
        try
        {
            for (int i = 0; i < debugData.Length; i += 5)
            {
                va += $"x:{debugData[i]},y:{debugData[i + 2]}\n";
            }
        }
        catch 
        {

        }


        Graphics.DrawProceduralIndirect(GrassMaterial, _grassInstanceBounds, MeshTopology.Triangles, _argsBuffer,
            0, null, null, ShadowCastingMode.Off, true, gameObject.layer);
    }


    void Update()
    {
        DrawGrass();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_grassInstanceBounds.center, _grassInstanceBounds.size);
    }
}
