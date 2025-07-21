using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public struct GrassComputeConfig
{
    public int grassDensity;
    public float grassSpacing;
    public float width;
    public float height;
}

public class GrassControl : MonoBehaviour
{

    public Terrain Terrain;
    public ComputeShader GrassCompute;
    public Material GrassMaterial;


    ComputeBuffer _grassComputeOutPutBuffer;

    ComputeBuffer _grassComputeInputConfigBuffer;

    ComputeBuffer _argsBuffer;


    [Header("草地渲染设置")]
    public GrassRenderSettings GrassSettings;


    public Bounds _grassInstanceBounds;


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


        config.width = Terrain.terrainData.size.x;
        config.height = Terrain.terrainData.size.y;



        _grassComputeInputConfigBuffer = new ComputeBuffer(1, System.Runtime.InteropServices.Marshal.SizeOf(typeof(GrassComputeConfig)));
        _grassComputeInputConfigBuffer.SetData(new GrassComputeConfig[] { config });
        GrassCompute.SetBuffer(0, "_config", _grassComputeInputConfigBuffer);
    }



    private void InitDrawGrass()
    {
        _grassComputeOutPutBuffer = new ComputeBuffer(GrassSettings.GrassDensity * GrassSettings.GrassDensity, 32, ComputeBufferType.Append);


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

        GrassCompute.SetTexture(0, "_TerrainHeightMap", terrainheightMap);

        _argsBuffer = new ComputeBuffer(1, 4, ComputeBufferType.IndirectArguments);



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

        ComputeBuffer.CopyCount(_grassComputeOutPutBuffer,_argsBuffer,sizeof(int));

        GrassMaterial.SetBuffer("_GrassComputeOutputs", _grassComputeOutPutBuffer);


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
