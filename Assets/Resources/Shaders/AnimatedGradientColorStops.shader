Shader "UI/AnimatedGradientColorStops"
{
    Properties
    {
        _MainTex         ("Mask Sprite",     2D)    = "white" {}
        _Speed           ("Scroll Speed",    Float) = 0.2
        _Repeat          ("Repeat Count",    Float) = 1.0
        _ColorA          ("Left Color",      Color) = (1,0,0,1)
        _ColorB          ("Mid Color",       Color) = (0,1,0,1)
        _ColorC          ("Right Color",     Color) = (0,0,1,1)
        _GlowColor       ("Glow Color",      Color) = (1,1,1,1)
        _GlowRange       ("Glow Range",      Range(0,0.5)) = 0.1
        _GlowIntensity   ("Glow Intensity",  Range(0,2  )) = 1.0
        _FillAmount      ("Fill Amount",     Range(0,1  )) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off  
        ZWrite Off  
        ZTest Always  
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Mask sprite (alpha only)
            sampler2D _MainTex;
            float4   _MainTex_ST;     // (offset.x,offset.y,scale.x,scale.y)

            // Animation
            float    _Speed;
            float    _Repeat;

            // Procedural colors
            float4 _ColorA;
            float4 _ColorB;
            float4 _ColorC;

            // Glow
            float4 _GlowColor;
            float  _GlowRange;
            float  _GlowIntensity;

            // Fill fraction (0~1)
            float _FillAmount;

            struct appdata 
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;  // mesh UV (0→1)
                float4 color  : COLOR;
            };

            struct v2f    
            {
                float4 pos    : SV_POSITION;
                float2 uvMain : TEXCOORD0;  // sprite UV in atlas
                float2 uv     : TEXCOORD1;  // mesh UV
                float4 color  : COLOR;
            };

            v2f vert(appdata IN)
            {
                v2f OUT;
                OUT.pos    = UnityObjectToClipPos(IN.vertex);
                OUT.uvMain = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.uv     = IN.uv;
                OUT.color  = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // --- 1) Mask alpha & early out ---
                fixed4 src = tex2D(_MainTex, IN.uvMain) * IN.color;
                if (src.a < 0.01) discard;

                // --- 2) Compute normalized fill UV range ---
                // offsetX = _MainTex_ST.x, scaleX = _MainTex_ST.z
                float offsetX = _MainTex_ST.x;
                float scaleX  = _MainTex_ST.z;
                float fillEnd = offsetX + scaleX * _FillAmount;
                if (IN.uvMain.x > fillEnd) discard;

                // norm = 0 at left of fill, 1 at right
                float norm = saturate((IN.uvMain.x - offsetX) / (scaleX * _FillAmount));

                // --- 3) Animated repeat coordinate ---
                float u = frac(norm * _Repeat + _Time.y * _Speed);

                // --- 4) Procedural 3-stop gradient ---
                fixed4 col;
                if (u < 0.5)
                    col = lerp(_ColorA, _ColorB, u * 2);
                else
                    col = lerp(_ColorB, _ColorC, (u - 0.5) * 2);

                // --- 5) Glow at right edge ---
                float glowT = saturate((norm - (1 - _GlowRange)) / _GlowRange);
                col.rgb = lerp(col.rgb, _GlowColor.rgb, glowT * _GlowIntensity);

                // --- 6) Preserve original alpha ---
                col.a = src.a;

                return col;
            }
            ENDCG
        }
    }
}
