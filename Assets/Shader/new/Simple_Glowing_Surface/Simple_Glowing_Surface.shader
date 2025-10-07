Shader "Custom/Simple_Glowing_Surface"
{
    Properties
    {
        _MainTex ("Base (RGB) / Mask (A)", 2D) = "white" {}
        _LineColor ("_LineColor (White areas)", Color) = (1,1,1,1)
        _BaseColor ("_BaseColor", Color) = (0,0,0,1)
        _GlowColor ("Glow Color (Black areas)", Color) = (1,0,0,1) // ���������̔����F
        _LineGlowIntensity ("Line Glow Intensity", Range(0.0, 10.0)) = 1.0 // ���C���̔����̋���
        _BaseGlowIntensity ("Base Glow Intensity", Range(0.0, 10.0)) = 1.0 // �ʂ̔����̋���
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5 // �e�N�X�`����臒l�i�����̋��E�j
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
            fixed4 _LineColor;
            fixed4 _BaseColor;
            fixed4 _GlowColor;
            float _LineGlowIntensity;
            float _BaseGlowIntensity;
            float _Cutoff;

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
                // �e�N�X�`������F���T���v�����O
                fixed4 tex = tex2D(_MainTex, i.uv);

                // �e�N�X�`���̋P�x�i�O���[�X�P�[���l�j���g�p
                // ���������ƍ��������̋��E��_Cutoff�Œ���
                float mask = step(_Cutoff, tex.r); // tex.r �� _Cutoff ���傫���i���邢�j�Ȃ�1.0�A�������i�Â��j�Ȃ�0.0

                fixed4 finalColor;
                fixed3 emission = 0; // ����������������

                if (mask > 0.5) // �������� (tex.r > _Cutoff)
                {
                    finalColor.rgb = _LineColor.rgb;
                    finalColor.a = _LineColor.a;

                    // ���C���̔������x���ʂ̃v���p�e�B�Ő���
                    emission = _LineColor.rgb * _LineGlowIntensity;
                }
                else // �������� (tex.r <= _Cutoff)
                {
                    finalColor.rgb = _BaseColor.rgb;
                    finalColor.a = _BaseColor.a;

                    // ���������ɂ̂ݔ�����K�p
                    emission = _GlowColor.rgb * _BaseGlowIntensity;
                }
                
                finalColor.rgb += emission; // �ŏI�F�ɔ��������Z

                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
}