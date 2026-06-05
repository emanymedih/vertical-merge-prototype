Shader "VerticalMerge/NebulaBackdrop"
{
    Properties
    {
        _Tint ("Tint", Color) = (0.18, 0.34, 0.78, 0.34)
        _Accent ("Accent", Color) = (0.84, 0.26, 1, 0.26)
        _Intensity ("Intensity", Range(0, 2)) = 0.7
        _Vignette ("Vignette", Range(0, 1.5)) = 0.74
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Tint;
            fixed4 _Accent;
            float _Intensity;
            float _Vignette;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centered = i.uv - 0.5;
                float t = _Time.y * 0.028;
                float n1 = noise(i.uv * 3.1 + float2(t, -t * 0.6));
                float n2 = noise(i.uv * 7.4 + float2(-t * 1.7, t));
                float nebula = smoothstep(0.42, 0.92, n1 * 0.72 + n2 * 0.38);
                float verticalMist = smoothstep(0.12, 0.9, 1.0 - abs(centered.x * 2.1)) * smoothstep(0.02, 0.95, 1.0 - abs(centered.y * 0.85));
                float vignette = saturate(1.0 - dot(centered, centered) * _Vignette * 2.2);
                fixed4 color = lerp(_Tint, _Accent, n2) * (nebula * 0.78 + verticalMist * 0.42) * _Intensity;
                color.a *= vignette * i.color.a;
                return color;
            }
            ENDCG
        }
    }
}
