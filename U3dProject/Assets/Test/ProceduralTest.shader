Shader "Unlit/ProceduralTest"
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
            struct appdata { uint vertexID : SV_VertexID; };
            struct v2f { float4 vertex : SV_POSITION; };
            v2f vert(appdata v)
            {
                v2f o;
                float2 pos[6] = {
                    float2(-0.5,-0.5), float2(0.5,-0.5), float2(-0.5,0.5),
                    float2(0.5,-0.5), float2(0.5,0.5), float2(-0.5,0.5)
                };
                o.vertex = UnityObjectToClipPos(float4(pos[v.vertexID],0,1));
                return o;
            }
            fixed4 frag(v2f i) : SV_Target { return fixed4(1,0,0,1); }
            ENDCG
        }
    }
}