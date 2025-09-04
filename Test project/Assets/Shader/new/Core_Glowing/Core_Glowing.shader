Shader "Custom/Core_Glow"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0.0, 100.0)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _GlowIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // î≠åıêFÇåvéZ
                fixed3 finalColor = _Color.rgb;
                fixed3 emission = _Color.rgb * _GlowIntensity;
                
                return fixed4(finalColor + emission, _Color.a);
            }
            ENDCG
        }
    }
    FallBack "Standard"
}