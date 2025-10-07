Shader "Custom/ghostbloc"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1) // 全体の色
        _MonochromeColor ("Monochrome Color", Color) = (0,0,0,1) // モノクロの濃さに応じて適用する色
        _MonochromePower ("Monochrome Power", Range(0, 5)) = 1.0 // モノクロの色付けの強さ
        _Opacity ("Overall Opacity", Range(0, 1)) = 1.0 // 全体の不透明度
    }
    SubShader
    {
        // 透明度を有効にするためにRenderTypeをTransparentに変更
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        // アルファブレンドを有効にし、ZWriteをオフにする
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
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
            fixed4 _Color; // 全体の色
            fixed4 _MonochromeColor; // モノクロの濃さに応じて適用する色
            float _MonochromePower; // モノクロの色付けの強さ
            float _Opacity; // 全体の不透明度

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
                // テクスチャをサンプリング
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // テクスチャのモノクロ強度を計算 (輝度)
                // 標準的な輝度計算式: 0.299 * R + 0.587 * G + 0.114 * B
                float monochromeIntensity = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

                // モノクロ強度と_MonochromePowerを使って、MonochromeColorとTint Colorをブレンド
                // MonochromePowerを上げると、モノクロの色がより強く反映される
                fixed4 finalColor = lerp(_Color, _MonochromeColor, pow(monochromeIntensity, _MonochromePower));

                // 元のテクスチャのアルファ値を保持しつつ、最終的な不透明度を適用
                finalColor.a = texColor.a * _Opacity;

                // フォグを適用
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
}
