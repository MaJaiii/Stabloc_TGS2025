Shader "Custom/VerticalFadeShader"
{
    Properties
    {
        _ColorA ("Color A (Bottom)", Color) = (1,0,0,1)
        _ColorB ("Color B", Color) = (0,1,0,1)
        _ColorC ("Color C", Color) = (0,0,1,1)
        _ColorD ("Color D (Top)", Color) = (1,1,0,1)
        _EmissionStrength ("Emission Strength", Range(1, 30)) = 1.0
        _FadeRange ("Fade Range", Range(0.001, 1)) = 1.0
        _OverallOpacity ("Overall Opacity", Range(0, 1)) = 1.0
    }
    SubShader
    {
        // レンダリング順序をTransparentよりも後に設定
        Tags { "Queue" = "Transparent+1" "RenderType" = "Transparent" }
        LOD 100
        
        // 通常のブレンドモード
        Blend SrcAlpha OneMinusSrcAlpha
        
        // 深度情報を書き込まないように修正
        ZWrite Off
        ZTest LEqual
        // 裏面を描画しない
        Cull Back
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            
            float4 _ColorA;
            float4 _ColorB;
            float4 _ColorC;
            float4 _ColorD;
            float _EmissionStrength;
            float _FadeRange;
            float _OverallOpacity;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 normalOS : TEXCOORD3;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.positionOS = v.vertex.xyz;
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                o.viewDirWS = normalize(UnityWorldSpaceViewDir(o.vertex));
                o.normalOS = v.normal;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float normalizedY = i.positionOS.y + 0.5;
                
                fixed3 finalColor;
                if (normalizedY < 0.35) {
                    finalColor = lerp(_ColorA.rgb, _ColorB.rgb, normalizedY / 0.35);
                } else if (normalizedY < 0.70) {
                    finalColor = lerp(_ColorB.rgb, _ColorC.rgb, (normalizedY - 0.35) / (0.70 - 0.35));
                } else {
                    finalColor = lerp(_ColorC.rgb, _ColorD.rgb, (normalizedY - 0.70) / (1.0 - 0.70));
                }
                
                finalColor *= _EmissionStrength;
                
                // フェードアウトのアルファを計算 (下に向かって透明になるように修正)
                float alpha = smoothstep(_FadeRange, 0, (1.0 - normalizedY));
                alpha *= _OverallOpacity;
                
                // 上面と下面のアルファをゼロにする
                float3 normalOS = i.normalOS;
                if (dot(normalOS, float3(0, 1, 0)) > 0.95 || dot(normalOS, float3(0, -1, 0)) > 0.95) {
                    alpha = 0;
                }
                
                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
}