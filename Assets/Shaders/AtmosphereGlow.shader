Shader "VerticalMerge/AtmosphereGlow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (0.4, 0.9, 1, 0.5)
        _Intensity ("Intensity", Range(0, 3)) = 1
        _PulseSpeed ("Pulse Speed", Range(0, 8)) = 1.4
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
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
                fixed4 tex = tex2D(_MainTex, i.uv);
                float2 centered = i.uv - 0.5;
                float radial = saturate(1.0 - length(centered) * 2.0);
                float rim = smoothstep(0.05, 0.78, 1.0 - abs(length(centered) * 2.0 - 0.86));
                float pulse = 0.82 + sin(_Time.y * _PulseSpeed) * 0.18;
                fixed4 color = _Tint * i.color * _Intensity * pulse;
                color.a *= tex.a * saturate(radial * 0.5 + rim * 0.8);
                return color;
            }
            ENDCG
        }
    }
}
