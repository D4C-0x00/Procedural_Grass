Shader "Unlit/TestShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                uint vertexID : SV_VertexID;
            };

            struct v2f
            {
               float2 uv : TEXCOORD0;
               float4 vertex : SV_POSITION;
            };
            StructuredBuffer<float2> _Uvs;

            StructuredBuffer<int> _Triangles;

            float3 cubicBezier(float3 p0, float3 p1, float3 p2, float3 p3, float t )
            {
                float3 a = lerp(p0, p1, t);
                float3 b = lerp(p2, p3, t);
                float3 c = lerp(p1, p2, t);
                float3 d = lerp(a, c, t);
                float3 e = lerp(c, b, t);
                return lerp(d,e,t); 
            }

            v2f vert (appdata v)
            {
                v2f o;

                 int vertexId=_Triangles[v.vertexID];
                 float2 uv=_Uvs[vertexId];


                float3 p0 = float3(0, 0, 0);
                float3 p1 = float3(0, 1 * 0.33, 0);
                float3 p2 = float3(0, 1 * 0.66, 0);
                float3 p3 = float3(0, 1, 0);
                
                float3 center = cubicBezier(p0, p1, p2, p3, uv.y);
                float3 pos = center + float3((uv.x - 0.5) * 0.5, 0, 0);
                o.vertex = UnityObjectToClipPos(pos);
                o.uv = uv;

                return o;
            }

           fixed4 frag(v2f i, fixed facing : VFACE) : SV_Target
           {
                if (facing > 0)
                 return fixed4(1,0,0,1); // 正面红色
                else
                 return fixed4(0,0,1,1); // 背面蓝色
           }
            ENDCG
        }
    }
}
