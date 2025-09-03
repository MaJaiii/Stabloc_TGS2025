Shader "Custom/ObjectFlowGlow"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (0.0, 0.7, 1.0, 1.0)
        _FlowSpeed ("Flow Speed", Float) = 1.0
        _FlowWidth ("Flow Width", Float) = 0.05
        _FlowIntensity ("Flow Intensity", Float) = 5.0
        _FresnelPower ("Fresnel Power", Range(1.0, 10.0)) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Float) = 2.0
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            fixed4 _GlowColor;
            float _FlowSpeed;
            float _FlowWidth;
            float _FlowIntensity;
            float _FresnelPower;
            float _FresnelIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float scrollU = i.uv.x - _Time.y * _FlowSpeed;
                float flow = smoothstep(_FlowWidth, 0.0, abs(frac(scrollU) - 0.5) * 2.0);
                flow *= _FlowIntensity;

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = 1.0 - saturate(dot(i.worldNormal, viewDir));
                fresnel = pow(fresnel, _FresnelPower);
                fresnel *= _FresnelIntensity;

                float totalGlow = flow + fresnel;
                fixed4 finalColor = _GlowColor * totalGlow;

                finalColor.a = saturate(totalGlow);

                return finalColor;
            }
            ENDCG
        }
    }
}