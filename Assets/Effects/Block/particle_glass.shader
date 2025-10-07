Shader "Custom/particleGlassShader"
{
    Properties
    {
        // メインテクスチャ
        _MainTex ("_MainTex", 2D) = "white" {}
        // 発光強度
        _EmissionStrength ("_EmissionStrength", Range(1, 30)) = 30.0
        // 全体の透明度
        _OverallOpacity ("_OverallOpacity", Range(0, 1)) = 0.2
        // スペキュラーの強さ
        _SpecularStrength ("_SpecularStrength", Range(0, 1)) = 1.0
        // 表面の滑らかさ
        _Smoothness ("_Smoothness", Range(0, 1)) = 1.0
        // 虹色効果の強さ
        _IridescenceStrength ("_IridescenceStrength", Range(0, 1)) = 0.5
        // 虹色効果の色変化速度
        _IridescenceShiftSpeed ("_IridescenceShiftSpeed", Range(0, 5)) = 0.6
    }
    SubShader
    {
        // レンダリングのタグ設定（透過用）
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
        // αブレンド（標準的な透過）
        Blend SrcAlpha OneMinusSrcAlpha
        // 深度書き込みを無効化
        ZWrite Off

        Pass
        {
            // シェーダープログラム開始
            CGPROGRAM
            #pragma vertex vert              // 頂点シェーダー指定
            #pragma fragment frag            // フラグメントシェーダー指定
            #pragma multi_compile_instancing // インスタンシング対応
            #pragma multi_compile_fwdadd     // ライティング追加パス対応

            // ビルトインシェーダー用ライブラリ
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            // プロパティの変数
            float4 _MainTex_ST; 
            sampler2D _MainTex;
            float _EmissionStrength;
            float _OverallOpacity;
            float _SpecularStrength;
            float _Smoothness;
            float _IridescenceStrength;
            float _IridescenceShiftSpeed;

            // 頂点入力構造体
            struct appdata
            {
                float4 vertex : POSITION;   // 頂点座標
                float3 normal : NORMAL;     // 法線
                float2 uv : TEXCOORD0;      // UV座標
                float4 color : COLOR;       // 頂点カラー
                UNITY_VERTEX_INPUT_INSTANCE_ID // インスタンシング用ID
            };

            // 頂点シェーダーからフラグメントシェーダーへ渡すデータ
            struct v2f
            {
                float4 vertex : SV_POSITION; // クリップ座標
                float3 normalWS : TEXCOORD0; // ワールド法線
                float3 viewDirWS : TEXCOORD1;// ワールド空間での視線方向
                float2 uv : TEXCOORD2;       // UV
                float4 color : TEXCOORD3;    // 頂点カラー
                UNITY_VERTEX_OUTPUT_STEREO   // ステレオレンダリング対応
            };

            // 頂点シェーダー
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // クリップ空間へ変換
                o.vertex = UnityObjectToClipPos(v.vertex);
                // UV変換
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // ワールド法線
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                // ワールド空間での視線方向を計算
                o.viewDirWS = normalize(UnityWorldSpaceViewDir(o.vertex));
                // 頂点カラーをそのまま渡す
                o.color = v.color;
                return o;
            }

            // フラグメントシェーダー
            fixed4 frag (v2f i) : SV_Target
            {
                // テクスチャカラーを取得
                float4 texColor = tex2D(_MainTex, i.uv);
                
                // ベースカラーとアルファ（頂点カラーを掛け合わせ）
                fixed3 baseColor = texColor.rgb * i.color.rgb;
                fixed baseAlpha = texColor.a * i.color.a * _OverallOpacity;

                // 法線と視線方向を正規化
                fixed3 normalWS = normalize(i.normalWS);
                fixed3 viewDirWS = normalize(i.viewDirWS);
                
                // 環境光源の方向と色
                fixed3 lightDirWS = _WorldSpaceLightPos0.xyz;
                fixed3 lightColor = _LightColor0.rgb;
                
                // 反射ベクトルを計算
                fixed3 reflectionDirWS = reflect(-lightDirWS, normalWS);
                
                // スペキュラ計算（滑らかさを強調するため指数を大きく）
                fixed spec = pow(saturate(dot(reflectionDirWS, viewDirWS)), _Smoothness * 100.0);
                fixed3 specularColor = lightColor * spec * _SpecularStrength;
                
                // 虹色効果のオフセットを時間で変化
                fixed iridescenceOffset = _Time.y * _IridescenceShiftSpeed;
                // 視線方向と法線の内積
                fixed iridescence = dot(normalWS, viewDirWS);
                // サイン波で虹色を生成（RGBをずらして位相を変える）
                fixed3 iridescenceColor = sin((iridescence + iridescenceOffset) * 3.14159 * 2.0 + fixed3(0, 2, 4)) * 0.5 + 0.5;
                
                // ベースカラーと虹色をブレンドし、スペキュラを加算
                fixed3 finalColor = lerp(baseColor, iridescenceColor, iridescence * _IridescenceStrength) + specularColor;
                // 発光強度を乗算
                finalColor *= _EmissionStrength;

                // 最終アルファ
                fixed finalAlpha = baseAlpha;

                // 出力
                return fixed4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}
