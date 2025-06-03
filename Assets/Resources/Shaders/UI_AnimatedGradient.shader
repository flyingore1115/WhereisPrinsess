Shader "UI/AnimatedGradientSimpleMasked"
{
    Properties
    {
        _MainTex      ("Sprite (Mask)",  2D)   = "white" {}
        _GradientTex  ("Gradient Tex",   2D)   = "gray"  {}
        _Speed        ("Scroll Speed",   Float)= 0.2
        _Repeat       ("Repeat Count",   Float)= 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4   _MainTex_ST;    // sprite UV 오프셋·스케일
            sampler2D _GradientTex;
            float    _Speed;
            float    _Repeat;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;  // 메쉬 UV (0~1)
                float4 color  : COLOR;
            };
            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;   // 메쉬 UV
                float4 color : COLOR;
            };

            v2f vert(appdata IN)
            {
                v2f OUT;
                OUT.pos   = UnityObjectToClipPos(IN.vertex);
                OUT.uv    = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 0) 원본 스프라이트 알파 샘플 — 이걸로 마스크 처리
                float srcAlpha = tex2D(_MainTex, TRANSFORM_TEX(IN.uv, _MainTex)).a;
                if (srcAlpha < 0.01) discard;  // 투명한 영역 밖은 절대 그리지 않음

                // 1) 그라데이션 UV 계산 (메쉬 UV.x 기준)
                float u = frac(IN.uv.x * _Repeat + _Time.y * _Speed);

                // 2) 그라데이션 텍스처에서 색상 샘플
                fixed4 grad = tex2D(_GradientTex, float2(u, 0.5));

                // 3) 알파는 원본 스프라이트 알파로
                grad.a = srcAlpha * IN.color.a;

                return grad;
            }
            ENDCG
        }
    }
}
