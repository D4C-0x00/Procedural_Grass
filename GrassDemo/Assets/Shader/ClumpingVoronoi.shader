Shader "Unlit/ClumpingVoronoi"
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
    

            #include "UnityCG.cginc"

            struct appdata
            {
               float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

         float2 N22(float2 p){
            
                float3 a = frac(p.xyx*float3(123.34,234.34,345.65));
                a += dot(a, a+34.45);
                return frac(float2(a.x*a.y,a.y*a.z));
            
            }


            fixed4 frag (v2f i) : SV_Target
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
