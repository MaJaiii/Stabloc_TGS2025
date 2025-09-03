Shader "Custom/MaskedGlowingPattern"
{
    Properties
    {
        // マスクテクスチャ。黒い部分はソリッドな色になり、白い部分がマスクとして機能します。
        _MaskTex ("Mask Texture (R=Mask, Black=Solid Color)", 2D) = "white" {}
        // マスクテクスチャの黒い部分に適用されるソリッドな色。
        _MainColor ("Solid Color (for Black Mask Areas)", Color) = (0,0,0,1)

        // マスクの白い部分に表示される模様テクスチャ。
        _PatternTex ("Pattern Texture", 2D) = "white" {}
        // 模様テクスチャの全体の色。
        _PatternTintColor ("Pattern Tint Color", Color) = (1,1,1,1) // 新しく追加
        // 模様テクスチャの発光色。
        _PatternColor ("Pattern Glow Color", Color) = (1,1,1,1)
        // 模様テクスチャの発光強度。
        _PatternEmission ("Pattern Emission Strength", Range(0, 10)) = 1.0
        // 模様テクスチャの回転速度。
        _RotationSpeed ("Pattern Rotation Speed", Range(0, 5)) = 0.5
    }
    SubShader
    {
        // レンダリングタイプを不透明に設定
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // フォグを有効にするためのコンパイルディレクティブ
            #pragma multi_compile_fog

            // Unityの共通CGインクルードファイル
            #include "UnityCG.cginc"

            // アプリケーションからバーテックスシェーダーへ渡されるデータ構造
            struct appdata
            {
                float4 vertex : POSITION; // 頂点座標
                float2 uv : TEXCOORD0;    // UV座標
            };

            // バーテックスシェーダーからフラグメントシェーダーへ渡されるデータ構造
            struct v2f
            {
                float2 uv : TEXCOORD0;    // UV座標
                UNITY_FOG_COORDS(1)       // フォグ計算用の座標
                float4 vertex : SV_POSITION; // クリップ空間での頂点座標
            };

            // シェーダープロパティとして定義されたテクスチャとカラー
            sampler2D _MaskTex;
            float4 _MaskTex_ST; // マスクテクスチャのスケールとオフセット

            sampler2D _PatternTex;
            float4 _PatternTex_ST; // パターンテクスチャのスケールとオフセット

            fixed4 _MainColor;
            fixed4 _PatternTintColor; // 新しく追加
            fixed4 _PatternColor;
            float _PatternEmission;
            float _RotationSpeed;

            // バーテックスシェーダー
            v2f vert (appdata v)
            {
                v2f o;
                // オブジェクト空間の頂点座標をクリップ空間に変換
                o.vertex = UnityObjectToClipPos(v.vertex);
                // メッシュのUV座標をマスクテクスチャのスケールとオフセットで変換
                o.uv = TRANSFORM_TEX(v.uv, _MaskTex);
                // フォグ座標を計算
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // フラグメントシェーダー
            fixed4 frag (v2f i) : SV_Target
            {
                // マスクテクスチャをサンプリング
                fixed4 maskColor = tex2D(_MaskTex, i.uv);

                // マスクの強度を計算（赤チャンネルを使用）。
                // maskValue が 0 (黒) に近いほど _MainColor になり、
                // maskValue が 1 (白) に近いほど _PatternTex が適用されます。
                float maskValue = maskColor.r;

                // パターンテクスチャのUVを回転させる
                float2 rotatedUV = i.uv;
                // 現在の時間と回転速度に基づいて角度を計算
                float angle = _Time.y * _RotationSpeed;
                float s = sin(angle);
                float c = cos(angle);

                // UVの中心 (0.5, 0.5) を基準に回転させる
                float2 pivot = float2(0.5, 0.5);
                rotatedUV -= pivot; // 中心を原点に移動
                // 回転行列を作成
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                // UVを回転
                rotatedUV = mul(rotationMatrix, rotatedUV);
                rotatedUV += pivot; // 元の位置に戻す

                // 回転したUVでパターンテクスチャをサンプリング
                fixed4 patternTexColor = tex2D(_PatternTex, rotatedUV);

                // パターンテクスチャに新しい_PatternTintColorを適用
                patternTexColor.rgb *= _PatternTintColor.rgb; // ここで色を乗算します

                // パターンテクスチャに発光色と発光強度を適用
                // 発光は最終的な色に加算されるため、HDR (High Dynamic Range) 環境でより効果的です。
                fixed3 glowingPattern = patternTexColor.rgb * _PatternColor.rgb * _PatternEmission;

                // マスクの値に基づいて、_MainColor と発光するパターンを線形補間
                // maskValue が 0 の場合 _MainColor、1 の場合 glowingPattern
                fixed3 finalRGB = lerp(_MainColor.rgb, glowingPattern, maskValue);

                // 最終的な色（アルファ値は不透明として1.0）
                fixed4 col = fixed4(finalRGB, 1.0);

                // フォグを適用
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
