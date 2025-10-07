Shader "Custom/ghostbloc"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1) // �S�̂̐F
        _MonochromeColor ("Monochrome Color", Color) = (0,0,0,1) // ���m�N���̔Z���ɉ����ēK�p����F
        _MonochromePower ("Monochrome Power", Range(0, 5)) = 1.0 // ���m�N���̐F�t���̋���
        _Opacity ("Overall Opacity", Range(0, 1)) = 1.0 // �S�̂̕s�����x
    }
    SubShader
    {
        // �����x��L���ɂ��邽�߂�RenderType��Transparent�ɕύX
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        // �A���t�@�u�����h��L���ɂ��AZWrite���I�t�ɂ���
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
            fixed4 _Color; // �S�̂̐F
            fixed4 _MonochromeColor; // ���m�N���̔Z���ɉ����ēK�p����F
            float _MonochromePower; // ���m�N���̐F�t���̋���
            float _Opacity; // �S�̂̕s�����x

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
                // �e�N�X�`�����T���v�����O
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // �e�N�X�`���̃��m�N�����x���v�Z (�P�x)
                // �W���I�ȋP�x�v�Z��: 0.299 * R + 0.587 * G + 0.114 * B
                float monochromeIntensity = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

                // ���m�N�����x��_MonochromePower���g���āAMonochromeColor��Tint Color���u�����h
                // MonochromePower���グ��ƁA���m�N���̐F����苭�����f�����
                fixed4 finalColor = lerp(_Color, _MonochromeColor, pow(monochromeIntensity, _MonochromePower));

                // ���̃e�N�X�`���̃A���t�@�l��ێ����A�ŏI�I�ȕs�����x��K�p
                finalColor.a = texColor.a * _Opacity;

                // �t�H�O��K�p
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
}
