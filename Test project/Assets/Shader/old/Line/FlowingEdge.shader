Shader "Custom/EdgeFlowGlow_Param_Fixed"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (0,1,1,1)
        _Speed ("Speed", Float) = 1.0
        _GlowWidth ("Glow Width", Float) = 0.1
        _GlowOuter1 ("Outer Width 1", Float) = 0.3
        _GlowOuter2 ("Outer Width 2", Float) = 0.5
        _GlowStrength ("Glow Strength", Float) = 3.0
        _Outer1Strength ("Outer1 Strength", Float) = 0.3
        _Outer2Strength ("Outer2 Strength", Float) = 0.15
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha One
        Cull Off
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
                float uvx : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            float _Speed;
            float _GlowWidth;
            float _GlowOuter1;
            float _GlowOuter2;
            float _GlowStrength;
            float _Outer1Strength;
            float _Outer2Strength;
            float4 _GlowColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uvx = v.uv.x;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);

                return o;
            }

           fixed4 frag(v2f i) : SV_Target
{
    // 法線と視線ベクトルの内積を計算
    float facing = dot(i.worldNormal, i.viewDir);
    
    // facingの絶対値を使用することで、法線の向きに関わらず正面かどうかを判断する
    float facingAbs = abs(facing);

    // 光の強度に影響を与えるため、0.0から1.0の範囲にクランプ
    float finalFacing = saturate(facingAbs);

    float t = frac(_Time.y * _Speed);
    float d = abs(frac(i.uvx - t));

    // 中心光
    float glow = smoothstep(_GlowWidth, 0.0, d);

    // 外側ぼかし
    glow += smoothstep(_GlowOuter1, 0.0, d) * _Outer1Strength;
    glow += smoothstep(_GlowOuter2, 0.0, d) * _Outer2Strength;

    glow = saturate(glow);

    // グローの強度に最終的な向きの値を適用
    fixed4 col = _GlowColor * glow * _GlowStrength;
    
    // アルファ値に最終的な向きの値を適用
    col.a = glow * finalFacing;

    return col;
}
            ENDCG
        }
    }
}