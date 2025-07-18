Shader "Custom/DrawGrass"
{
    Properties { }
    SubShader
    {
        Pass
        {

            Cull off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct GrassPropertiesStruct
            {
                float3 position;
                float height;
                float width;
                float tilt;
                float bend;
                // 其它属性
            };

           StructuredBuffer<GrassPropertiesStruct> GrassProperties;
          
           StructuredBuffer<float3> MeshVertices;

           StructuredBuffer<int> MeshTriangles;

            struct appdata
            {
                uint vertexID : SV_VertexID;
                uint instanceID : SV_InstanceID;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float3 cubicBezier(float3 p0, float3 p1, float3 p2, float3 p3, float t )
            {
                float3 a = lerp(p0, p1, t);
                float3 b = lerp(p2, p3, t);
                float3 c = lerp(p1, p2, t);
                float3 d = lerp(a, c, t);
                float3 e = lerp(c, b, t);
                return lerp(d,e,t); 
            }


            v2f vert(appdata v)
            {

                int triangIndex=MeshTriangles[v.vertexID];
                float3 localPos=MeshVertices[triangIndex];


                GrassPropertiesStruct grass=GrassProperties[v.instanceID];

                float3 worldPos=grass.position+localPos;

                //worldPos.z=grass.width;

                float3 p0=float3(0,0,0);


                float height = grass.height;

                float tilt = grass.tilt;
                float bend = grass.bend;

                float p3y =  tilt*height;
                float p3x = sqrt(height*height - p3y*p3y);

                float3 p3 = float3(-p3x,p3y,0);

                float3 bladeDir = normalize(p3);
                float3 bezCtrlOffsetDir = normalize(cross(bladeDir, float3(0,0,1)));

                float3 p1 = 0.33* p3;
                float3 p2 = 0.66 * p3;

                p1 += bezCtrlOffsetDir * bend ;
                p2 += bezCtrlOffsetDir * bend ;

                float t=v.uv.y;

                float3 bezierVertex=cubicBezier(p0,p1,p2,p3,t);

                ///计算草宽度
                float side=1;
                side=(side*2)-1;

                float width=grass.width*(1-t*0.975);
                bezierVertex.z += side * width;


                v2f o;
                o.vertex = UnityObjectToClipPos(bezierVertex);



                return o;
             
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(0, 1, 0, 1); // 绿色
            }
            ENDCG
        }
    }
}