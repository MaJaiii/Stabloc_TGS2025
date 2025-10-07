Shader "Custom/ShiningSpawnBlock"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Color ("Color", Color) = (1,1,0,1)             // êF
        _BlockIntensity ("Block Intensity",float) = 10  // åıÇÃã≠Ç≥
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _BlockIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, col);

                // êFÇê›íË
                col.rgb = _Color.rgb;
                // åıÇÃã≠Ç≥Çê›íË
                col.rgb *= _BlockIntensity;

                return col;
            }
            ENDCG
        }
    }
}
