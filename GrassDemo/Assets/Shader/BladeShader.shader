Shader "Unlit/BladeShader"
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
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
             float4 vertex : SV_POSITION;
             float3 worldNormal : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct GrassBlade
            {
                float3 position;
                float BaseHeight;
                float BaseWidth;
                float BaseTilt;
                float BaseBend;
                float angle;
            };
            StructuredBuffer<GrassBlade> _GrassBlades;

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
             float3x3 AngleAxis3x3(float angle, float3 axis)
	        {
		        float c, s;
		        sincos(angle, s, c);

		        float t = 1 - c;
		        float x = axis.x;
		        float y = axis.y;
		        float z = axis.z;

		        return float3x3(
			        t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			        t * x * z - s * y, t * y * z + s * x, t * z * z + c
			        );
	        }

            v2f vert (appdata v)
            {
                v2f o;

                int _Triang=_Triangles[v.vertexID];


                float2 uv=_Uvs[_Triang];

                GrassBlade blade=_GrassBlades[v.instanceID];

                float3 p0=float3(0,0,0);

                //float3 p3=float3(0,blade.BaseHeight,0);

                float p3y=blade.BaseHeight*blade.BaseTilt;

                float p3x=sqrt(blade.BaseHeight*blade.BaseHeight-p3y*p3y);


                float3 p3=float3(p3x,p3y,0);


                float3 p1=p3*0.33;
                float3 p2=p3*0.66;


                
                float3 mainDir=normalize (p3);

                float3 pxOffsetDir=normalize(cross(float3(0,0,1),mainDir)); /* normalize(cross(mainDir,float3(0,0,1))); */

                p1+=pxOffsetDir*blade.BaseBend;

                p2+=pxOffsetDir*blade.BaseBend;


                float3 center =cubicBezier(p0,p1,p2,p3,uv.y);

                float side = (uv.x*2)-1;

                float3 pos=center+float3(0,0,blade.BaseWidth *side);


                float3x3 rotMat = AngleAxis3x3(blade.angle, float3(0,1,0));

                pos = mul(rotMat,pos);

                pos=pos+blade.position;

                o.vertex= UnityObjectToClipPos(pos);

               // o.vertex = UnityObjectToClipPos(v.vertex);
               // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
              
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                return fixed4(0,1,0,1);
            }
            ENDCG
        }
    }
}
