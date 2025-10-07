Shader "Custom/CornerGlowShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        [HDR] _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowPower ("Glow Power", Range(0, 1000)) = 1
        _GlowFalloff ("Glow Falloff", Range(1, 50)) = 2
        _Opacity ("Opacity", Range(0, 1)) = 1 
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            fixed4 _Color;
            sampler2D _MainTex;
            fixed4 _GlowColor;
            float _GlowPower;
            float _GlowFalloff;
            float _Opacity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color;

                float glow00 = pow(1.0 - distance(i.uv, float2(0,0)), _GlowFalloff);
                float glow10 = pow(1.0 - distance(i.uv, float2(1,0)), _GlowFalloff);
                float glow01 = pow(1.0 - distance(i.uv, float2(0,1)), _GlowFalloff);
                float glow11 = pow(1.0 - distance(i.uv, float2(1,1)), _GlowFalloff);

                float totalGlow = max(max(glow00, glow10), max(glow01, glow11));
                
                float glowAmount = totalGlow * _GlowPower;

                fixed4 finalEmission = _GlowColor * glowAmount;

                fixed4 finalColor = texColor + finalEmission;
                
                finalColor.a = _Opacity;

                return finalColor;
            }
            ENDCG
        }
    }
}