using System;
using UnityEngine;
[Serializable]
public class GrassRenderSettings 
{
    /// <summary>
    /// �ݵ��ܶ�
    /// </summary>
    [Header("�ݵ��ܶ�")]
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