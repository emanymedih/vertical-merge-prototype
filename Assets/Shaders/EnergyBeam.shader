Shader "VerticalMerge/EnergyBeam"
{
    Properties
    {
        _Tint ("Tint", Color) = (0.4, 0.9, 1, 0.8)
        _Intensity ("Intensity", Range(0, 3)) = 1
        _PulseSpeed ("Pulse Speed", Range(0, 8)) = 2.2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Tint;
            float _Intensity;
            float _PulseSpeed;

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
                float center = saturate(1.0 - abs(i.uv.y - 0.5) * 2.0);
                float streak = smoothstep(0.08, 1.0, center);
                float flow = 0.78 + sin((i.uv.x * 18.0) - _Time.y * _PulseSpeed * 4.0) * 0.22;
                fixed4 color = _Tint * i.color * _Intensity * flow;
                color.a *= streak;
                return color;
            }
            ENDCG
        }
    }
}
