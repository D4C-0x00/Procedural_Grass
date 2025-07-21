Shader "Unlit/GrassShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            struct GrassComputeOutPut
            {
                float3 position;
            }

            StructuredBuffer<GrassComputeOutPut> _GrassComputeOutputs;


            v2f vert (appdata v)
            {
                v2f o;

                float3 position;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
               return fixed4(0, 1, 0, 1); // бли╚
            }
            ENDCG
        }
    }
}
