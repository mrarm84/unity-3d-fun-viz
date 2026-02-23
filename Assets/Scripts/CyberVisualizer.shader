Shader "Custom/CyberVisualizer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlitchAmount ("Glitch Amount", Range(0, 1)) = 0.0
        _BlurAmount ("Blur Amount", Range(0, 1)) = 0.0
        _RGBOffset ("RGB Offset", Range(0, 1)) = 0.0
        _Invert ("Invert", Range(0, 1)) = 0.0
        _Neon ("Neon Mode", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float3 normal : NORMAL; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; float3 worldNormal : TEXCOORD1; };

            sampler2D _MainTex;
            float4 _MainTex_ST, _Color;
            float _GlitchAmount, _BlurAmount, _RGBOffset, _Invert, _Neon;

            float rand(float n) { return frac(sin(n) * 43758.5453123); }

            v2f vert (appdata v)
            {
                v2f o;
                float t = _Time.y * 20.0;
                float3 pos = v.vertex.xyz;
                
                if (_GlitchAmount > 0.1) {
                    pos.x += (rand(t + v.vertex.y) - 0.5) * _GlitchAmount * 0.5;
                }
                
                o.vertex = TransformObjectToHClip(pos);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 col = tex2D(_MainTex, uv);

                // Aggressive 9-tap Blur
                if (_BlurAmount > 0.01) {
                    float s = _BlurAmount * 0.05;
                    col += tex2D(_MainTex, uv + float2(s, s));
                    col += tex2D(_MainTex, uv + float2(-s, s));
                    col += tex2D(_MainTex, uv + float2(s, -s));
                    col += tex2D(_MainTex, uv + float2(-s, -s));
                    col += tex2D(_MainTex, uv + float2(s, 0));
                    col += tex2D(_MainTex, uv + float2(0, s));
                    col /= 7.0;
                }

                // RGB Split
                float offset = _RGBOffset * 0.1;
                float r = tex2D(_MainTex, uv + float2(offset, 0)).r;
                float b = tex2D(_MainTex, uv - float2(offset, 0)).b;
                col = float4(r, col.g, b, col.a);

                // Neon Wireframe Effect (N Key action)
                if (_Neon > 0.5) {
                    float fresnel = 1.0 - saturate(dot(normalize(i.worldNormal), float3(0,0,1)));
                    col = float4(0, 1, 1, 1) * pow(fresnel, 3.0) * 2.0;
                }

                // Inversion (B Key action)
                if (_Invert > 0.5) col.rgb = 1.0 - col.rgb;

                col *= _Color;
                if (_GlitchAmount > 0.5) col.a = 0.5; // Transparency on heavy glitch

                return col;
            }
            ENDHLSL
        }
    }
}
