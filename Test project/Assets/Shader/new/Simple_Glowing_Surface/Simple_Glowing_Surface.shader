Shader "Custom/Simple_Glowing_Surface"
{
    Properties
    {
        _MainTex ("Base (RGB) / Mask (A)", 2D) = "white" {}
        _LineColor ("_LineColor (White areas)", Color) = (1,1,1,1)
        _BaseColor ("_BaseColor", Color) = (0,0,0,1)
        _GlowColor ("Glow Color (Black areas)", Color) = (1,0,0,1) // 黒い部分の発光色
        _LineGlowIntensity ("Line Glow Intensity", Range(0.0, 10.0)) = 1.0 // ラインの発光の強さ
        _BaseGlowIntensity ("Base Glow Intensity", Range(0.0, 10.0)) = 1.0 // 面の発光の強さ
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5 // テクスチャの閾値（白黒の境界）
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
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _LineColor;
            fixed4 _BaseColor;
            fixed4 _GlowColor;
            float _LineGlowIntensity;
            float _BaseGlowIntensity;
            float _Cutoff;

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
                // テクスチャから色をサンプリング
                fixed4 tex = tex2D(_MainTex, i.uv);

                // テクスチャの輝度（グレースケール値）を使用
                // 白い部分と黒い部分の境界を_Cutoffで調整
                float mask = step(_Cutoff, tex.r); // tex.r が _Cutoff より大きい（明るい）なら1.0、小さい（暗い）なら0.0

                fixed4 finalColor;
                fixed3 emission = 0; // 発光成分を初期化

                if (mask > 0.5) // 白い部分 (tex.r > _Cutoff)
                {
                    finalColor.rgb = _LineColor.rgb;
                    finalColor.a = _LineColor.a;

                    // ラインの発光強度を個別のプロパティで制御
                    emission = _LineColor.rgb * _LineGlowIntensity;
                }
                else // 黒い部分 (tex.r <= _Cutoff)
                {
                    finalColor.rgb = _BaseColor.rgb;
                    finalColor.a = _BaseColor.a;

                    // 黒い部分にのみ発光を適用
                    emission = _GlowColor.rgb * _BaseGlowIntensity;
                }
                
                finalColor.rgb += emission; // 最終色に発光を加算

                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
}