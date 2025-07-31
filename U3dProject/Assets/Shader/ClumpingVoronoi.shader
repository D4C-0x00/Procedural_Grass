Shader "Custom/ClumpingVoronoi"
{
    Properties
    {
    }
  SubShader
  {
      Pass
      {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"


            struct appdata
            {
              float4 vertex:POSITION;
              float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float2 N22(float2 p)
            {
                float n = sin(dot(p, float2(12.9898, 78.233))) * 43758.5453;
                return frac(float2(n, n * 1.2154));
            }

            v2f vert(appdata v)
            {

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                 float minDist = 100000;
                 float id = 0;
                 float2 Centre=float2(0,0);
                 for(int j=0;j<40;j++)
                 {
                     float2 jvalue=float2(j,j);
                     float2 mix=N22(jvalue);

                     float dis=distance(mix,i.uv);

                     if(dis<minDist)
                     {
                         minDist=dis;
                         id=fmod(j,4);
                         Centre=mix;
                     }
                 }

                 float3 col = float3(id,Centre);

                 return fixed4(col, 1);

            }

            ENDCG
      }
  }
}
