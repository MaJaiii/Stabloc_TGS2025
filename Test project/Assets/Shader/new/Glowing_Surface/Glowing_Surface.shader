Shader "Custom/FinalGlowExp"
{
    Properties
    {
        _MainTex ("Pattern Mask (White lines, Black background)", 2D) = "white" {}
        _LineColor ("Line Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _BaseColor ("Base Color (Glow Area)", Color) = (0,0,0,1)
        _GlowFalloff ("Glow Falloff (Spread)", Range(1.0, 20.0)) = 5.0
        _GlowPower ("Max Glow Power", Range(0.1, 5.0)) = 1.0
        _BlinkMinPower ("Min Glow Power", Range(0.1, 2.0)) = 0.1
        _GlowStartDistance ("Glow Start Distance", Range(0.0, 0.5)) = 0.0
        _GlowIntensity ("Glow Intensity", Range(0, 120)) = 5.0
        _Opacity ("Overall Opacity", Range(0, 1)) = 1.0
        _BlinkSpeed ("Blink Speed", Float) = 1.0
        _BlinkOffset ("Blink Offset", Float) = 0.0 // C#から設定するオフセット
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _LineColor;
            fixed4 _GlowColor;
            fixed4 _BaseColor;
            float _GlowFalloff;
            float _GlowPower;
            float _BlinkMinPower;
            float _GlowStartDistance;
            float _GlowIntensity;
            float _Opacity;
            float _BlinkSpeed;
            float _BlinkOffset; // シェーダーで使う変数

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // 点滅の最小値と最大値の差分を計算
                float blinkRange = _GlowPower - _BlinkMinPower;
                
                // C#スクリプトから受け取ったオフセット値を使用
                float blinkFactor = (sin((_Time.y * _BlinkSpeed) + _BlinkOffset) + 1.0) / 2.0;
                
                // 点滅の最小値から最大値までの間で強度を変化させる
                float dynamicGlowPower = _BlinkMinPower + blinkFactor * blinkRange;
                
                // 1. 発光ファクターの計算
                float2 centerOffset = i.uv - float2(0.5, 0.5);
                float distFromCenter = length(centerOffset);
                
                float glowFactor;
                // 発光のフォールオフを指数関数で計算
                glowFactor = exp(-distFromCenter * _GlowFalloff) * dynamicGlowPower;
                
                // 2. 最終色の決定 (テクスチャの輝度でロジックを分岐)
                fixed4 finalOutputColor;
                
                float brightness = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
                
                if (brightness > 0.5)
                {
                    finalOutputColor.rgb = _LineColor.rgb;
                    finalOutputColor.a = _Opacity;
                }
                else
                {
                    fixed3 baseColor = _BaseColor.rgb;
                    fixed3 emission = _GlowColor.rgb * glowFactor * _GlowIntensity;

                    finalOutputColor.rgb = baseColor + emission;
                    
                    finalOutputColor.a = _Opacity;
                }
                
                UNITY_APPLY_FOG(i.fogCoord, finalOutputColor);
                return finalOutputColor;
            }
            ENDCG
        }
    }
}