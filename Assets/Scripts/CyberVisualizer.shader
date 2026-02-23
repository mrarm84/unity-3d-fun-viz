Shader "Custom/CyberVisualizer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlitchAmount ("Glitch Amount", Range(0, 1)) = 0.0
        _Flicker ("Flicker", Range(0, 1)) = 0.0
        _ScanlineSpeed ("Scanline Speed", Range(0, 20)) = 5.0
        _NeonColor ("Neon Color", Color) = (0, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float3 normal : NORMAL; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; float3 worldPos : TEXCOORD1; float3 worldNormal : NORMAL; };

            sampler2D _MainTex;
            float4 _MainTex_ST, _Color, _NeonColor;
            float _GlitchAmount, _Flicker, _ScanlineSpeed;

            float rand(float n) { return frac(sin(n) * 43758.5453123); }

            v2f vert (appdata v)
            {
                v2f o;
                float3 pos = v.vertex.xyz;
                
                // Horizontal Slicing Glitch
                if (_GlitchAmount > 0.1) {
                    float slice = floor(v.vertex.y * 10.0 + _Time.y * 10.0);
                    if (rand(slice) > 0.8) {
                        pos.x += (rand(slice + _Time.y) - 0.5) * _GlitchAmount;
                    }
                }
                
                o.vertex = TransformObjectToHClip(pos);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // 1. Base Texture with RGB Split
                float r = tex2D(_MainTex, uv + float2(_GlitchAmount * 0.05, 0)).r;
                float g = tex2D(_MainTex, uv).g;
                float b = tex2D(_MainTex, uv - float2(_GlitchAmount * 0.05, 0)).b;
                float4 col = float4(r, g, b, 1.0);

                // 2. Moving Scanlines
                float scanline = sin(i.worldPos.y * 50.0 - _Time.y * _ScanlineSpeed) * 0.5 + 0.5;
                col *= (0.8 + 0.2 * scanline);

                // 3. Fresnel Glow (Cyber Outline)
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = 1.0 - saturate(dot(normalize(i.worldNormal), viewDir));
                float4 neon = _NeonColor * pow(fresnel, 3.0) * 2.0;
                
                // 4. Digital Flicker
                float flicker = rand(floor(_Time.y * 30.0)) > 0.9 ? 0.5 : 1.0;
                
                float4 final = (col + neon) * _Color;
                final.a = (0.4 + 0.6 * fresnel) * flicker;

                // Mode B: Invert
                if (_Flicker > 0.5) final.rgb = 1.0 - final.rgb;

                return final;
            }
            ENDHLSL
        }
    }
}
