Shader "Custom/SoapBubbleShader"
{
    Properties
    {
        _ColorA ("Color A", Color) = (1,0,0,1)
        _ColorB ("Color B", Color) = (0,1,0,1)
        _ColorC ("Color C", Color) = (0,0,1,1)
        _ColorD ("Color D", Color) = (1,1,0,1)

        _EmissionStrength ("Emission Strength", Range(1, 30)) = 1.0
        _OverallOpacity ("Overall Opacity", Range(0, 1)) = 1.0
        _FadeRange ("Fade Range", Range(0.001, 1)) = 1.0
        
        // シャボン玉用のプロパティ
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 5.0
        _IridescenceStrength ("Iridescence Strength", Range(0, 1)) = 1.0
        _IridescenceShiftSpeed ("Iridescence Shift Speed", Range(0, 5)) = 1.0 // 虹色の変化速度
    }
    SubShader
    {
        // レンダリング順序をTransparentに設定
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" "RenderPipeline" = "UniversalRenderPipeline" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        
        // 深度を書き込まず、深度テストは手前か同深度で行う
        ZWrite Off
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _ALPHATEST_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 normalOS : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorA;
                float4 _ColorB;
                float4 _ColorC;
                float4 _ColorD;
                float _EmissionStrength;
                float _OverallOpacity;
                float _FadeRange;
                float _FresnelPower;
                float _IridescenceStrength;
                float _IridescenceShiftSpeed;
            CBUFFER_END
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceNormalizeViewDir(TransformObjectToWorld(IN.positionOS.xyz));
                OUT.normalOS = IN.normalOS;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normalOS = IN.normalOS;

                float normalized_coord_for_color;
                if (abs(normalOS.y) < 0.5) {
                    if (abs(normalOS.z) > 0.5) {
                        normalized_coord_for_color = IN.positionOS.x;
                    } 
                    else {
                        normalized_coord_for_color = IN.positionOS.z;
                    }
                    normalized_coord_for_color += 0.5;
                } else {
                    normalized_coord_for_color = 0.5;
                }
                
                half3 baseColor;
                if (normalized_coord_for_color < 0.33) {
                    baseColor = lerp(_ColorA.rgb, _ColorB.rgb, normalized_coord_for_color * 3.0);
                } else if (normalized_coord_for_color < 0.66) {
                    baseColor = lerp(_ColorB.rgb, _ColorC.rgb, (normalized_coord_for_color - 0.33) * 3.0);
                } else {
                    baseColor = lerp(_ColorC.rgb, _ColorD.rgb, (normalized_coord_for_color - 0.66) * 3.0);
                }
                
                // フレネル効果: 視線と法線の角度が浅いほど明るく
                float fresnel = pow(saturate(1.0 - dot(IN.normalWS, IN.viewDirWS)), _FresnelPower);
                
                // 虹色効果に時間の要素を追加
                float iridescenceOffset = _Time.y * _IridescenceShiftSpeed; // 時間に応じてオフセットをかける
                float iridescence = dot(normalize(IN.normalWS), normalize(IN.viewDirWS));
                half3 iridescenceColor = sin((iridescence + iridescenceOffset) * 3.14159 * 2.0 + float3(0, 2, 4)) * 0.5 + 0.5;
                
                half3 finalColor = lerp(baseColor, iridescenceColor, fresnel * _IridescenceStrength);
                finalColor *= _EmissionStrength;

                float normalizedY = IN.positionOS.y + 0.5;
                float alpha = smoothstep(0, _FadeRange, (1.0 - normalizedY));
                
                alpha = lerp(alpha, 1.0, fresnel);
                
                alpha *= _OverallOpacity;

                float3 normalWS = normalize(IN.normalWS);
                
                if (dot(normalWS, float3(0, 1, 0)) > 0.95 || dot(normalWS, float3(0, -1, 0)) > 0.95) {
                    alpha = 0;
                }

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}