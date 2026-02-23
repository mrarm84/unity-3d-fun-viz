Shader "Custom/CyberVisualizer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlitchAmount ("Glitch Amount", Range(0, 1)) = 0.0
        _BlurAmount ("Blur Amount", Range(0, 0.1)) = 0.0
        _RGBOffset ("RGB Offset", Range(0, 0.5)) = 0.0
        _Invert ("Invert Colors", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _GlitchAmount, _BlurAmount, _RGBOffset, _Invert;

            float rand(float n) { return frac(sin(n) * 43758.5453123); }

            v2f vert (appdata v)
            {
                v2f o;
                float t = _Time.y * 10.0;
                if (_GlitchAmount > 0.1) {
                    v.vertex.xyz += (rand(t + v.vertex.y) - 0.5) * _GlitchAmount * 0.2;
                }
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // Simple 4-tap blur
                float4 col = tex2D(_MainTex, uv);
                if (_BlurAmount > 0) {
                    col += tex2D(_MainTex, uv + float2(_BlurAmount, _BlurAmount));
                    col += tex2D(_MainTex, uv + float2(-_BlurAmount, -_BlurAmount));
                    col /= 3.0;
                }

                // RGB Aberration
                float4 r = tex2D(_MainTex, uv + float2(_RGBOffset, 0));
                float4 b = tex2D(_MainTex, uv - float2(_RGBOffset, 0));
                float4 final = float4(r.r, col.g, b.b, col.a);

                if (_Invert > 0.5) final.rgb = 1.0 - final.rgb;

                return final * _Color;
            }
            ENDHLSL
        }
    }
}
