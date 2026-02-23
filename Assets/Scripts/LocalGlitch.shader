Shader "Custom/LocalGlitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlitchAmount ("Glitch Amount", Range(0, 1)) = 0.0
        _JitterSpeed ("Jitter Speed", Range(0, 100)) = 15.0
        _ChromaOffset ("Chromatic Offset", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _GlitchAmount;
            float _JitterSpeed;
            float _ChromaOffset;

            float rand(float n) { return frac(sin(n) * 43758.5453123); }

            v2f vert (appdata v)
            {
                v2f o;
                float time = _Time.y * _JitterSpeed;
                
                // Stronger vertex jittering
                if (_GlitchAmount > 0.01) {
                    float noise = rand(floor(v.vertex.y * 5.0 + time));
                    if (noise > 0.7) {
                        v.vertex.xyz += (rand(time) - 0.5) * _GlitchAmount * 0.3;
                    }
                }

                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _JitterSpeed;
                float2 uv = i.uv;

                // Blocky UV glitch
                if (_GlitchAmount > 0.2) {
                    if (rand(floor(uv.y * 10.0) + time) > 0.8) {
                        uv.x += (rand(time) - 0.5) * _GlitchAmount * 0.1;
                    }
                }

                // Intense Chromatic Aberration
                float offset = _ChromaOffset * _GlitchAmount;
                float4 colR = tex2D(_MainTex, uv + float2(offset, 0));
                float4 colG = tex2D(_MainTex, uv);
                float4 colB = tex2D(_MainTex, uv - float2(offset, 0));

                float4 finalCol = float4(colR.r, colG.g, colB.b, colG.a);
                
                // Add a digital tint
                finalCol += float4(0, _GlitchAmount * 0.2, _GlitchAmount * 0.1, 0);

                return finalCol * _Color;
            }
            ENDHLSL
        }
    }
}
