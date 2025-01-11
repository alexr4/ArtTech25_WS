Shader "Hidden/BoneTracer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius("Radius", Float) = 0.025
        _FadeOutFactor("Fade out factor", Float) = 0.95
        _Aspect("Fade out factor", Float) = 1.7
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Packages/com.bonjour-lab.utils/Runtime/Shaders/SDF/sdf2DShapes.hlsl"
            #include "Packages/com.bonjour-lab.utils/Runtime/Shaders/SDF/sdfStrokeFill.hlsl"

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

            sampler2D _MainTex;
            uniform float4 bone;
            uniform float4 prevbone;
            uniform float _Radius;
            uniform float _FadeOutFactor;
            uniform float _Aspect;

            float2 ratio(in float2 st, in float2 s) {
                return lerp(float2((st.x * s.x / s.y) - (s.x * .5 - s.y * .5) / s.y, st.y),
                float2(st.x, st.y * (s.y / s.x) - (s.y * .5 - s.x * .5) / s.x),
                step(s.x, s.y));
            }

            fixed4 frag(v2f i) : SV_Target
            {

                
                float2 uv = ratio(i.uv.xy, float2(_Aspect, 1.0));
                float2 bonestart = ratio(prevbone.xy, float2(_Aspect, 1.0));
                float2 boneend = ratio(bone.xy, float2(_Aspect, 1.0));

                float sdf = sdSegment(uv, bonestart, boneend);
                float circ = fill(sdf, _Radius, 0.01);

                float4 rbga = tex2D(_MainTex, i.uv);

                return rbga * _FadeOutFactor + float4(circ, circ, circ, circ);
            }
            ENDCG
        }
    }
}
