Shader "Custom/Jnc_CircleBrush"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _BrushPosition ("Brush Position", Vector) = (0.5, 0.5, 0, 0)
        _BrushRadius ("Brush Radius", Float) = 0.1
        _BrushColor ("Brush Color", Color) = (1, 0, 0, 1)
        _BrushSoftness ("Brush Softness", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _BrushPosition;
            float _BrushRadius;
            float4 _BrushColor;
            float _BrushSoftness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 baseColor = tex2D(_MainTex, i.uv);

                float dist = distance(i.uv, _BrushPosition.xy);

                if (dist < _BrushRadius)
                {
                    float factor = smoothstep(_BrushRadius * (1.0 - _BrushSoftness), _BrushRadius, dist);
                    return lerp(_BrushColor, baseColor, factor);
                }

                return baseColor;
            }
            ENDCG
        }
    }
}