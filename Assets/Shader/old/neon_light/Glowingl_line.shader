Shader "Custom/MaskedGlowingPattern"
{
    Properties
    {
        // マスクテクスチャ。黒い部分はソリッドな色、白い部分がマスク。
        _MaskTex ("Mask Texture (R=Mask, Black=Solid Color)", 2D) = "white" {}
        // マスクテクスチャの黒い部分に適用されるソリッドな色。
        _MainColor ("Solid Color (for Black Mask Areas)", Color) = (0,0,0,1)

        // マスクの白い部分に表示される模様テクスチャ。
        _PatternTex ("Pattern Texture", 2D) = "white" {}
        // 模様テクスチャの全体の色。黒い部分はこの色、白い部分は白。
        _BackgroundColor ("_BackgroundColor", Color) = (1,1,1,1)
        // 模様テクスチャの発光色。
        _PatternColor ("Pattern Glow Color", Color) = (1,1,1,1)
        // 模様テクスチャの発光強度。
        _PatternEmission ("Pattern Emission Strength", Range(0, 10)) = 1.0
        // 模様テクスチャの回転速度。
        _RotationSpeed ("Pattern Rotation Speed", Range(0, 5)) = 0.5
        // 模様テクスチャのスケール。
        _PatternScale ("Pattern Scale", Range(0.1, 10.0)) = 1.0
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
                float3 normal : NORMAL;   // 法線ベクトルを追加
            };

            // バーテックスシェーダーからフラグメントシェーダーへ渡されるデータ構造
            struct v2f
            {
                float2 uv : TEXCOORD0;    // UV座標
                UNITY_FOG_COORDS(1)       // フォグ計算用の座標
                float4 vertex : SV_POSITION; // クリップ空間での頂点座標
                float3 worldNormal : TEXCOORD1; // ワールド空間の法線ベクトルを追加
            };

            // シェーダープロパティとして定義されたテクスチャとカラー
            sampler2D _MaskTex;
            float4 _MaskTex_ST; // マスクテクスチャのスケールとオフセット

            sampler2D _PatternTex;
            float4 _PatternTex_ST; // パターンテクスチャのスケールとオフセット

            fixed4 _MainColor;
            fixed4 _BackgroundColor;
            fixed4 _PatternColor;
            float _PatternEmission;
            float _RotationSpeed;
            float _PatternScale;

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

                // ワールド空間の法線ベクトルを計算してフラグメントシェーダーに渡す
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            // フラグメントシェーダー
            fixed4 frag (v2f i) : SV_Target
            {
                // マスクテクスチャをサンプリング
                fixed4 maskColor = tex2D(_MaskTex, i.uv);

                // マスクの強度を計算（赤チャンネルを使用）。
                float maskValue = maskColor.r;

                // パターンテクスチャのUVを回転およびスケールさせる
                float2 processedUV = i.uv;

                // スケールを適用
                // pivotを中心にしてUVをスケールする
                float2 pivot = float2(0.5, 0.5);
                processedUV = (processedUV - pivot) * _PatternScale + pivot;

                // ワールド空間の法線に基づいて回転方向を決定
                float rotationDirection = 1.0; // デフォルトは左回り

                // 法線を正規化
                float3 N = normalize(i.worldNormal);

                // 各面に対して回転方向を設定
                // Z軸（前後）に近い場合
                if (abs(N.z) > abs(N.x) && abs(N.z) > abs(N.y))
                {
                    // 前面 (Z > 0) または背面 (Z < 0) は左回り (rotationDirection = 1.0)
                    rotationDirection = 1.0;
                }
                // X軸（左右）に近い場合
                else if (abs(N.x) > abs(N.y))
                {
                    // 左面 (X < 0) または右面 (X > 0) は右回り
                    rotationDirection = -1.0;
                }
                // Y軸（上下）に近い場合
                else
                {
                    // 上面 (Y > 0) または下面 (Y < 0) は右回り
                    rotationDirection = 1.0;
                }


                // 回転を適用
                // 現在の時間と回転速度、そして決定された回転方向に基づいて角度を計算
                float angle = _Time.y * _RotationSpeed * rotationDirection;
                float s = sin(angle);
                float c = cos(angle);

                // pivotを中心に回転させる
                processedUV -= pivot; // 中心を原点に移動
                // 回転行列を作成
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                // UVを回転
                processedUV = mul(rotationMatrix, processedUV);
                processedUV += pivot; // 元の位置に戻す

                // 処理されたUVでパターンテクスチャをサンプリング
                fixed4 patternTexColor = tex2D(_PatternTex, processedUV);

                // パターンテクスチャの輝度を計算
                // 標準的な輝度計算式: 0.299 * R + 0.587 * G + 0.114 * B
                // 黒に近いほど0、白に近いほど1
                float patternLuminosity = dot(patternTexColor.rgb, float3(0.299, 0.587, 0.114));

                // 輝度に基づいて、_PatternTintColor と白の間で補間
                // 輝度が0(黒)に近いほど _PatternTintColor に、1(白)に近いほど白 (fixed3(1,1,1)) になるようにブレンド
                fixed3 tintedPatternRGB = lerp(_BackgroundColor.rgb, fixed3(1,1,1), patternLuminosity);

                // 補間された色をパターンテクスチャの色として適用
                patternTexColor.rgb = tintedPatternRGB;

                // パターンテクスチャに発光色と発光強度を適用
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
