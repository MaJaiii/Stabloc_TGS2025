Shader "Custom/AnimatedCubeShader"
{
    Properties
    {
        // ベーステクスチャ
        _BaseTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseEmission ("Base Emission Strength", Range(0, 100)) = 3.0

        // ラインテクスチャ
        _LineTex ("Line Texture", 2D) = "white" {}
        _BeaconLineColor ("Line Color", Color) = (1,1,1,1)
        _LineEmission ("Line Emission Strength", Range(0, 100)) = 40.0

        // スケールアニメーション用
        _XZScale ("XZ Scale", Range(0.0, 10.0)) = 1.0
        _YScale ("Y Scale", Range(0.0, 10.0)) = 1.0

        // 回転アニメーション用
        _RotationAngle ("Rotation Angle", Range(0, 360)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            sampler2D _BaseTex;
            fixed4 _BaseColor;
            float _BaseEmission;
            
            sampler2D _LineTex;
            fixed4 _BeaconLineColor;
            float _LineEmission;

            float _XZScale;
            float _YScale;
            float _RotationAngle;

            v2f vert (appdata v)
            {
                v2f o;

                float4 scaledVertex = v.vertex;
                scaledVertex.x *= _XZScale;
                scaledVertex.z *= _XZScale;
                scaledVertex.y *= _YScale;

                float rad = _RotationAngle * UNITY_PI / 180.0;
                float s = sin(rad);
                float c = cos(rad);
                float3x3 rotationMatrix = float3x3(
                    c, 0, s,
                    0, 1, 0,
                    -s, 0, c
                );
                
                scaledVertex.xyz = mul(rotationMatrix, scaledVertex.xyz);

                o.vertex = UnityObjectToClipPos(scaledVertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 上下面の法線を検出して透明にする
                if (abs(i.worldNormal.y) > 0.9f)
                {
                    discard;
                }

                fixed4 baseTex = tex2D(_BaseTex, i.uv);
                fixed4 lineTex = tex2D(_LineTex, i.uv);
                
                // ベース色にテクスチャを乗算し、発光を乗算して最終色を決定
                fixed3 finalBaseColor = baseTex.rgb * _BaseColor.rgb * _BaseEmission;
                
                // ライン色にテクスチャを乗算し、発光を乗算して最終色を決定
                fixed3 finalLineColor = lineTex.rgb * _BeaconLineColor.rgb * _LineEmission;
                
                // ラインのアルファ値に基づいて、ベースとラインの色をブレンド
                fixed3 finalRGB = lerp(finalBaseColor, finalLineColor, lineTex.a);
                
                // アルファ値はラインテクスチャのアルファ値に依存
                fixed alpha = lerp(baseTex.a, 1.0, lineTex.a);

                fixed4 col = fixed4(finalRGB, alpha);
                return col;
            }
            ENDCG
        }
    }
}