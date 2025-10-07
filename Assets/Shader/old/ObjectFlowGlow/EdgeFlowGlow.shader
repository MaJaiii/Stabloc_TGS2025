Shader "Custom/EdgeFlowGlow"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (0,1,1,1)
        _Speed ("Speed", Float) = 1.0
        _Density ("Density", Float) = 1.0
        _GlowWidth ("Glow Width", Float) = 0.1
        _GlowOuter1 ("Outer Width 1", Float) = 0.3
        _GlowOuter2 ("Outer Width 2", Float) = 0.5
        _GlowStrength ("Glow Strength", Float) = 3.0
        _Outer1Strength ("Outer1 Strength", Float) = 0.3
        _Outer2Strength ("Outer2 Strength", Float) = 0.15
        _Transparency ("Transparency", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha One
        Cull Back
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            float _Speed;
            float _Density;
            float _GlowWidth;
            float _GlowOuter1;
            float _GlowOuter2;
            float _GlowStrength;
            float _Outer1Strength;
            float _Outer2Strength;
            float4 _GlowColor;
            float _Transparency;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // ローカル座標を基にUVを再マッピング
                float3 localPosAbs = abs(v.vertex.xyz);
                float2 remappedUV;

                if (localPosAbs.x > localPosAbs.y && localPosAbs.x > localPosAbs.z) // 左右の面
                {
                    remappedUV = v.vertex.zy;
                }
                else if (localPosAbs.y > localPosAbs.x && localPosAbs.y > localPosAbs.z) // 上下の面
                {
                    remappedUV = v.vertex.xz;
                }
                else // 前後の面
                {
                    remappedUV = v.vertex.xy;
                }
                o.uv = remappedUV;
                
                // 後続の計算のために値を渡す
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 法線と視線ベクトルの内積を計算
                float facing = dot(i.worldNormal, i.viewDir);
                float facingAbs = abs(facing);
                float finalFacing = saturate(facingAbs);

                // 密度を考慮してUVをスケール
                float2 uvScaled = i.uv.xy * _Density;

                // 斜め方向のベクトル
                float2 dir = float2(1, 1);
                
                // UV座標に時間と斜め方向ベクトルを加えてスクロール
                float2 scrolledUV = uvScaled + dir * _Time.y * _Speed;

                // 途切れのない斜めパターンの距離を計算
                float patternValue = frac(scrolledUV.x + scrolledUV.y);
                float d = abs(patternValue - 0.5) * 2.0;

                // 中心光
                float glow = smoothstep(_GlowWidth, 0.0, d);

                // 外側ぼかし
                glow += smoothstep(_GlowOuter1, 0.0, d) * _Outer1Strength;
                glow += smoothstep(_GlowOuter2, 0.0, d) * _Outer2Strength;

                glow = saturate(glow);

                fixed4 col = _GlowColor * glow * _GlowStrength;
                
                // アルファ値に最終的な向きの値と、新しい透明度プロパティを適用
                col.a = glow * finalFacing * _Transparency;

                return col;
            }
            ENDCG
        }
    }
}