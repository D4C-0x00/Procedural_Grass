Shader "Unlit/GrassShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
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
                uint vertexID : SV_VertexID;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                int instanceID : TEXCOORD1;
            };

            struct GrassComputeOutPut
            {
                 float3 position;
                 float height;
                 float width;
                 float tilt;
                 float bend;
            };
    
            StructuredBuffer<GrassComputeOutPut> _GrassComputeOutputs;
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

                GrassComputeOutPut output=_GrassComputeOutputs[v.instanceID];

                v2f o;

               int vertexId=_Triangles[v.vertexID];

                float2 uv =_Uvs[vertexId];

                float3 position=output.position;
                

                 float3 p0 = float3(0, 0, 0);


                float height=output.height;

                float p3y=height*output.tilt;

                float p3x=sqrt(height*height-p3y*p3y);

                float3 p3=float3(p3x,p3y,0);

                float3 p1 =p3*0.33;
                float3 p2 = p3*0.66;
            

                float3 bladeDir = normalize(p3);
                float3 bezCtrlOffsetDir = normalize(cross(bladeDir, float3(0,0,1)));

                float p2ffset=output.bend*0.3;
                float p3ffset=output.bend*0.6;

                p2 += bezCtrlOffsetDir * p2ffset;
                p3 += bezCtrlOffsetDir *p3ffset;



                
                float3 center = cubicBezier(p0, p1, p2, p3, uv.y);

                 float width =0.5;


                 float side = (uv.x * 2) - 1; // -1µ½1

                float3 pos = center + float3(0, 0, side * width);


                // center.z+=width;

              //  float3 pos = center+ float3((uv.x - 0.5) * 0.5, 0, 0);

                // float3 pos = center;
                // pos.z=+width*uv.x;




                pos=pos+position;
                o.vertex = UnityObjectToClipPos(pos);
                o.uv = uv;
                o.instanceID= v.instanceID;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
               
                return fixed4(i.uv.x, 0,0, 1);
            }
            ENDCG
        }
    }
}
