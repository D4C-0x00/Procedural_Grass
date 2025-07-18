using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public struct GrassParametersStruct
{
    public float baseHeight;
    public float baseWidth;
    public float tilt;
    public float bend;

};


public class DrawGrass : MonoBehaviour
{
    #region 基础属性
    public int GrassResolution = 2000;

    #endregion

    private ComputeBuffer _grassPropertiesBuffer;


    private ComputeBuffer _argsBuffer;


    public ComputeShader GrassComputeShader;

    public Material GrassMaterial;
    
    public Bounds _grassInstanceBounds;

    public Mesh GrassMesh;


    private Mesh _cloneMesh;

    private Terrain terrain;

    #region ComputeShader 字段ID映射
    int resolutionId = Shader.PropertyToID("_Resolution");
    int GrassSpacingId= Shader.PropertyToID("_GrassSpacing");
   
    #endregion


    #region 渲染草用的buff
    ComputeBuffer _meshTriangles;
    ComputeBuffer _meshvertices;
    ComputeBuffer _meshnormals;
    #endregion

    #region Clumping 部分
    [Header("Clumping")]
    public Material ClumpingVoronoiMat;

    #endregion

    public RenderTexture VoronoiOutPut;


    [Header("草丛属性")]
    public List<GrassParametersStruct> GrassParameters;
    GrassParametersStruct[] _GrassParameters;
    ComputeBuffer GrassParametersBuff;


    void Start()
    {
        InitBounds();
        InitGrassComputeShader();
    }

    private void InitBounds()
    {
         terrain = GetComponent<Terrain>();

  
        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 terrainPos = terrain.GetPosition();
        Vector3 center = terrainPos + terrainSize * 0.5f; // 地形中心
        Vector3 size = terrainSize * 1.2f;                // 稍微放大
        _grassInstanceBounds = new Bounds(center, size);

    }

    private void InitGrassComputeShader()
    {

        _argsBuffer = new ComputeBuffer(1, sizeof(int) * 4, ComputeBufferType.IndirectArguments);

       

        _grassPropertiesBuffer = new ComputeBuffer(GrassResolution* GrassResolution, 32, ComputeBufferType.Append);

        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.heightmapResolution-1;
        int height = terrainData.heightmapResolution-1;
        float[,] heights = terrainData.GetHeights(0, 0, width, height);
        // 创建一张灰度贴图
        Texture2D heightmapTex = new Texture2D(width, height, TextureFormat.RFloat, false, true);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float h = heights[y, x]; // 0~1
                heightmapTex.SetPixel(x, y, new Color(h, h, h, 1));
            }
        }
        heightmapTex.Apply();

        GrassComputeShader.SetTexture(0, "hightMap", heightmapTex);

        GrassComputeShader.SetInt(resolutionId, GrassResolution);


        float grassSpacing = terrain.terrainData.size.x / GrassResolution;

        GrassComputeShader.SetFloat(GrassSpacingId, grassSpacing);


        Vector3 terrainPos = terrain.GetPosition();
        Vector3 terrainSize = terrain.terrainData.size;

        Debug.Log($"terrainPos:{terrainPos}");
        Debug.Log($"terrainSize:{terrainSize}");

        GrassComputeShader.SetVector("_TerrainPos", new Vector4(terrainPos.x, terrainPos.z, 0, 0));
        GrassComputeShader.SetVector("_TerrainSize", new Vector4(terrainSize.x, terrainSize.z, 0, 0));

        GrassComputeShader.SetFloat("_TerrainHeight", terrain.terrainData.size.y);

        _cloneMesh =new Mesh();
        _cloneMesh.name = "clone";
        _cloneMesh.vertices = GrassMesh.vertices;
        _cloneMesh.triangles = GrassMesh.triangles;
        _cloneMesh.normals = GrassMesh.normals;
        _cloneMesh.uv = GrassMesh.uv;

        Color[] newColors = new Color[_cloneMesh.colors.Length];

        for (int i = 0; i < _cloneMesh.colors.Length; i++)
        {
            Color col = _cloneMesh.colors[i];
          
            newColors[i] = col;
        } 

        _cloneMesh.colors = newColors;

        int[] triangles = _cloneMesh.triangles;

        _meshTriangles = new ComputeBuffer(triangles.Length,sizeof(int));
        _meshTriangles.SetData(triangles);

        Vector3[] positions = _cloneMesh.vertices;

        _meshvertices = new ComputeBuffer(positions.Length,sizeof(float)*3);
        _meshvertices.SetData(positions);

        _argsBuffer.SetData(new int[] { _cloneMesh.triangles.Length,0,0,0 });

      
        ///传入顶点坐标
        GrassMaterial.SetBuffer("MeshVertices", _meshvertices);
        ///传入顶点索引
        GrassMaterial.SetBuffer("MeshTriangles", _meshTriangles);


        Texture2D null2d = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);

        VoronoiOutPut = new RenderTexture(width, height,0,RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        Graphics.Blit(null2d, VoronoiOutPut, ClumpingVoronoiMat);

        GrassComputeShader.SetTexture(0, "_VoronoiOutPut", VoronoiOutPut);

        _GrassParameters = new GrassParametersStruct[GrassParameters.Count];
        for (int i = 0; i < GrassParameters.Count; i++) 
        {
            _GrassParameters[i] = GrassParameters[i];
        }

        GrassParametersBuff = new ComputeBuffer(_GrassParameters.Length,sizeof(float)*4);
        GrassParametersBuff.SetData(_GrassParameters);

        GrassComputeShader.SetBuffer(0, "GrassParameters", GrassParametersBuff);
    }


    private void Draw()
    {
        _grassPropertiesBuffer.SetCounterValue(0);

        GrassComputeShader.SetBuffer(0, "GrassProperties", _grassPropertiesBuffer);

        float[] debugData = new float[100 * 4]; // 10个float4

     

        GrassMaterial.SetBuffer("GrassProperties", _grassPropertiesBuffer);
        int groups = Mathf.CeilToInt(GrassResolution / 8f);

        GrassComputeShader.Dispatch(0, groups, groups, 1);
        _grassPropertiesBuffer.GetData(debugData);


        ComputeBuffer.CopyCount(_grassPropertiesBuffer, _argsBuffer, sizeof(int));

        Graphics.DrawProceduralIndirect(GrassMaterial, _grassInstanceBounds, MeshTopology.Triangles, _argsBuffer,
            0, null, null, ShadowCastingMode.Off, true, gameObject.layer);
    }

    void OnDestroy()
    {

    }
    // Update is called once per frame
    void Update()
    {
        Draw();
    }
}
