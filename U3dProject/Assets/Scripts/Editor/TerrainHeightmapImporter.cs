using UnityEngine;
using UnityEditor;

public class TerrainHeightmapImporter : EditorWindow
{
    Texture2D heightmapTexture;
    Terrain targetTerrain;
    float heightScale = 0.3f; // 控制地形起伏强度，越小越平坦
    int smoothKernelSize = 5; // 平滑核大小，越大越圆滑

    [MenuItem("Tools/导入高度贴图到Terrain（支持平滑）")]
    public static void ShowWindow()
    {
        GetWindow<TerrainHeightmapImporter>("高度贴图导入Terrain");
    }

    void OnGUI()
    {
        GUILayout.Label("选择高度贴图和目标Terrain", EditorStyles.boldLabel);

        heightmapTexture = (Texture2D)EditorGUILayout.ObjectField("高度贴图", heightmapTexture, typeof(Texture2D), false);
        targetTerrain = (Terrain)EditorGUILayout.ObjectField("目标Terrain", targetTerrain, typeof(Terrain), true);

        heightScale = EditorGUILayout.Slider("高度缩放(scale)", heightScale, 0.01f, 1f);
        smoothKernelSize = EditorGUILayout.IntSlider("平滑核大小", smoothKernelSize, 1, 15);

        if (GUILayout.Button("导入并应用"))
        {
            if (heightmapTexture == null || targetTerrain == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择高度贴图和Terrain！", "确定");
                return;
            }
            ImportHeightmapTextureToTerrain(heightmapTexture, targetTerrain, heightScale, smoothKernelSize);
        }
    }

    void ImportHeightmapTextureToTerrain(Texture2D tex, Terrain terrain, float scale, int kernelSize)
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        // 1. 缩放高度贴图到地形分辨率
        Texture2D scaledTex = tex;
        if (tex.width != width || tex.height != height)
        {
            scaledTex = new Texture2D(width, height, TextureFormat.RGB24, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / (width - 1);
                    float v = (float)y / (height - 1);
                    Color c = tex.GetPixelBilinear(u, v);
                    scaledTex.SetPixel(x, y, c);
                }
            }
            scaledTex.Apply();
        }

        // 2. 读取灰度值并写入高度
        float[,] heights = new float[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = scaledTex.GetPixel(x, y);
                heights[y, x] = pixel.grayscale * scale;
            }
        }

        // 3. 平滑处理
        heights = SmoothHeights(heights, kernelSize);

        // 4. 写入地形
        terrainData.SetHeights(0, 0, heights);

        // 5. 保存
        EditorUtility.SetDirty(terrainData);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("完成", "高度图已成功导入并应用到Terrain！", "确定");
    }

    // 简单均值滤波平滑
    float[,] SmoothHeights(float[,] src, int kernelSize = 3)
    {
        int h = src.GetLength(0);
        int w = src.GetLength(1);
        float[,] dst = new float[h, w];
        int half = kernelSize / 2;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float sum = 0f;
                int count = 0;
                for (int ky = -half; ky <= half; ky++)
                {
                    for (int kx = -half; kx <= half; kx++)
                    {
                        int ny = Mathf.Clamp(y + ky, 0, h - 1);
                        int nx = Mathf.Clamp(x + kx, 0, w - 1);
                        sum += src[ny, nx];
                        count++;
                    }
                }
                dst[y, x] = sum / count;
            }
        }
        return dst;
    }
}