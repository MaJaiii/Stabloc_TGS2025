Shader "Custom/frameanimation"
{
    Properties
    {
        // マスクテクスチャ。黒い部分が透明、白い部分がマスク。
        _MaskTex ("Mask Texture (R=Mask, Black=Transparent)", 2D) = "white" {}
        // マスクテクスチャの黒い部分に適用されるソリッドな色。
        _MainColor ("Solid Color (for Black Mask Areas)", Color) = (0,0,0,1)
        // ソリッドな色の不透明度
        _SolidOpacity ("Solid Color Opacity", Range(0, 1)) = 1.0

        // マスクの白い部分に表示される模様テクスチャ。
        _PatternTex ("Pattern Texture", 2D) = "white" {}
        
        // パターン部分の発光設定
        _PatternColor ("Pattern Glow Color", Color) = (1,1,1,1)
        _PatternEmission ("Pattern Emission Strength", Range(0, 100)) = 1.0

        // 背景部分の発光設定
        _BackgroundColor ("Background Color", Color) = (0,0,0,1)
        _BackgroundGlowColor ("Background Glow Color", Color) = (1,1,1,1)
        _BackgroundEmission ("Background Emission Strength", Range(0, 200)) = 1.0

        // 模様テクスチャの回転速度とスケール
        _RotationSpeed ("Pattern Rotation Speed", Range(0, 5)) = 0.5
        _PatternScale ("Pattern Scale", Range(0.1, 10.0)) = 1.0

        // オブジェクト全体の見た目のスケール
        _ObjectScale ("Object Scale (Visual Only)", Range(0.1, 5.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
            };

            sampler2D _MaskTex;
            sampler2D _PatternTex;

            fixed4 _MainColor;
            float _SolidOpacity;
            fixed4 _BackgroundColor;
            fixed4 _BackgroundGlowColor;
            float _BackgroundEmission;
            fixed4 _PatternColor;
            float _PatternEmission;
            float _RotationSpeed;
            float _PatternScale;

            // オブジェクトの見た目のスケール
            float _ObjectScale;

            v2f vert (appdata v)
            {
                v2f o;
                // ここでオブジェクトの見た目のスケールを適用
                v.vertex.xyz *= _ObjectScale;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o, o.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ここからUV座標の計算ロジックを変更
                float3 normal = normalize(i.worldNormal);
                float2 newUV = i.uv;

                // 上面または下面かを判定
                if (abs(normal.y) > 0.9f)
                {
                    // 上面または下面の場合（Y軸に垂直）
                    // 5x5のグリッドにマッピングするため、UVを5倍にスケール
                    newUV.x = i.uv.x * 5.0f;
                    newUV.y = i.uv.y * 5.0f;
                }
                else
                {
                    // 側面の場合（XまたはZ軸に垂直）
                    // 5x1のグリッドにマッピング
                    // UnityのデフォルトのUVは0～1なので、U座標を5倍に、V座標はそのまま
                    newUV.x = i.uv.x * 5.0f;
                    // V座標（高さ方向）は元のまま
                    newUV.y = i.uv.y * 1.0f;
                }

                fixed4 maskColor = tex2D(_MaskTex, newUV);
                float maskValue = maskColor.r;

                float2 processedUV = newUV;
                float2 pivot = float2(0.5, 0.5);
                processedUV = (processedUV - pivot) * _PatternScale + pivot;

                float rotationDirection = 1.0;
                float3 N = normalize(i.worldNormal);

                if (abs(N.z) > abs(N.x) && abs(N.z) > abs(N.y))
                {
                    rotationDirection = 1.0;
                }
                else if (abs(N.x) > abs(N.y))
                {
                    rotationDirection = -1.0;
                }
                else
                {
                    rotationDirection = 1.0;
                }

                float angle = _Time.y * _RotationSpeed * rotationDirection;
                float s = sin(angle);
                float c = cos(angle);

                processedUV -= pivot;
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                processedUV = mul(rotationMatrix, processedUV);
                processedUV += pivot;

                // ここで新しいUVを使用
                fixed4 patternTexColor = tex2D(_PatternTex, processedUV);
                float patternLuminosity = dot(patternTexColor.rgb, float3(0.299, 0.587, 0.114));

                // 背景と模様の発光を個別に計算
                fixed3 backgroundGlow = _BackgroundGlowColor.rgb * _BackgroundEmission;
                fixed3 patternGlow = _PatternColor.rgb * _PatternEmission;

                // 輝度に基づいて、背景と模様の発光を線形補間
                // patternLuminosityが0(黒)に近づくとbackgroundGlowが、1(白)に近づくとpatternGlowが強くなる
                fixed3 finalPatternEmission = lerp(backgroundGlow, patternGlow, patternLuminosity);

                // パターンテクスチャに最終的な発光色と背景色を適用
                fixed3 finalPatternRGB = _BackgroundColor.rgb + finalPatternEmission;

                // マスクの値に基づいて、_MainColorと発光するパターンを線形補間
                fixed3 finalRGB = lerp(_MainColor.rgb, finalPatternRGB, maskValue);

                float alpha = lerp(_SolidOpacity, 1.0, maskValue);

                fixed4 col = fixed4(finalRGB, alpha);

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}