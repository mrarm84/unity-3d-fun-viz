Shader "Custom/LocalGlitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlitchAmount ("Glitch Amount", Range(0, 1)) = 0.0
        _JitterSpeed ("Jitter Speed", Range(0, 100)) = 10.0
        _ChromaOffset ("Chromatic Offset", Range(0, 0.1)) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            float rand(float n){return frac(sin(n) * 43758.5453123);}

            v2f vert (appdata v)
            {
                v2f o;
                
                // Jitter vertex position based on time and glitch amount
                float time = _Time.y * _JitterSpeed;
                if (_GlitchAmount > 0.1) {
                    float noise = rand(floor(v.vertex.y * 10.0 + time));
                    v.vertex.x += (noise - 0.5) * _GlitchAmount * 0.5;
                }

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Chromatic Aberration: Sample R, G, B with offsets
                float2 uvR = i.uv + float2(_ChromaOffset * _GlitchAmount, 0);
                float2 uvB = i.uv - float2(_ChromaOffset * _GlitchAmount, 0);

                fixed4 colR = tex2D(_MainTex, uvR);
                fixed4 colG = tex2D(_MainTex, i.uv);
                fixed4 colB = tex2D(_MainTex, uvB);

                fixed4 finalCol = fixed4(colR.r, colG.g, colB.b, colG.a);
                return finalCol * _Color;
            }
            ENDCG
        }
    }
}
