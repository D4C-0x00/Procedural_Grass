using System;
using UnityEngine;
[Serializable]
public class GrassRenderSettings 
{
    /// <summary>
    /// 草的密度
    /// </summary>
    [Header("草的密度")]
    public int GrassDensity;

}
[Serializable]
public struct GrassPatchConfig
{
    public float Width;
    public float Height;
    public float Tilt;
    public float Bend;

}