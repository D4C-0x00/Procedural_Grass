Shader "Unlit/BezierTest"
{
    Properties
    {
            _Width("Width", Range (0, 1)) = 1
            _TaperAmount("TaperAmount",Range(0,1))=0.1
            _WavePower("WavePower",float)=1
            _WaveSpeed("WaveSpeed",float)=1
            _WaveAmplitude("WaveAmplitude",float)=1
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
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color:COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float3 p0;
            float3 p1;
            float3 p2;
            float3 p3;

            float _Width;
            float _TaperAmount;
            float _WavePower;
            float _WaveSpeed;
            float _WaveAmplitude;
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

                float3 bladeDir = normalize(p3-float3(0,0,0));

                float3 bezCtrlOffsetDir = -normalize(cross(bladeDir, float3(0,1,0)));

                float t=v.uv.y;
                

                 ///增加扰动
                float p1Weight=0.33;
                float p2Weight=0.66;
                float p3Weight=1;

                float p1ffset = pow(p1Weight,_WavePower)* (_WaveAmplitude/100) * sin(_Time*_WaveSpeed +p1Weight*2*3.1415); 
                float p2ffset = pow(p2Weight,_WavePower)* (_WaveAmplitude/100) * sin(_Time*_WaveSpeed +p2Weight*2*3.1415); 
                float p3ffset = pow(p3Weight,_WavePower)* (_WaveAmplitude/100) * sin(_Time*_WaveSpeed +p3Weight*2*3.1415); 

                p1 += bezCtrlOffsetDir*  p1ffset;
                p2 += bezCtrlOffsetDir*  p2ffset;
                p3 += bezCtrlOffsetDir*  p3ffset;


                float3 bezierVertex=cubicBezier(p0,p1,p2,p3,t);


                ///计算草宽度
                float side=v.color.g;
                side=(side*2)-1;
                float width=_Width*(1-t*_TaperAmount);
                bezierVertex.z += side * width;


               

       


           
                o.vertex = UnityObjectToClipPos(bezierVertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                
                return fixed4(1,i.uv.y,1,1);
            }
            ENDCG
        }
    }
}
