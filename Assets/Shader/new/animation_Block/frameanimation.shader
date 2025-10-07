Shader "Custom/frameanimation"
{
    Properties
    {
        // �}�X�N�e�N�X�`���B���������������A�����������}�X�N�B
        _MaskTex ("Mask Texture (R=Mask, Black=Transparent)", 2D) = "white" {}
        // �}�X�N�e�N�X�`���̍��������ɓK�p�����\���b�h�ȐF�B
        _MainColor ("Solid Color (for Black Mask Areas)", Color) = (0,0,0,1)
        // �\���b�h�ȐF�̕s�����x
        _SolidOpacity ("Solid Color Opacity", Range(0, 1)) = 1.0

        // �}�X�N�̔��������ɕ\�������͗l�e�N�X�`���B
        _PatternTex ("Pattern Texture", 2D) = "white" {}
        
        // �p�^�[�������̔����ݒ�
        _PatternColor ("Pattern Glow Color", Color) = (1,1,1,1)
        _PatternEmission ("Pattern Emission Strength", Range(0, 100)) = 1.0

        // �w�i�����̔����ݒ�
        _BackgroundColor ("Background Color", Color) = (0,0,0,1)
        _BackgroundGlowColor ("Background Glow Color", Color) = (1,1,1,1)
        _BackgroundEmission ("Background Emission Strength", Range(0, 200)) = 1.0

        // �͗l�e�N�X�`���̉�]���x�ƃX�P�[��
        _RotationSpeed ("Pattern Rotation Speed", Range(0, 5)) = 0.5
        _PatternScale ("Pattern Scale", Range(0.1, 10.0)) = 1.0

        // �I�u�W�F�N�g�S�̂̌����ڂ̃X�P�[��
        _ObjectScale ("Object Scale (Visual Only)", Range(0.1, 5.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
            };

            sampler2D _MaskTex;
            sampler2D _PatternTex;

            fixed4 _MainColor;
            float _SolidOpacity;
            fixed4 _BackgroundColor;
            fixed4 _BackgroundGlowColor;
            float _BackgroundEmission;
            fixed4 _PatternColor;
            float _PatternEmission;
            float _RotationSpeed;
            float _PatternScale;

            // �I�u�W�F�N�g�̌����ڂ̃X�P�[��
            float _ObjectScale;

            v2f vert (appdata v)
            {
                v2f o;
                // �����ŃI�u�W�F�N�g�̌����ڂ̃X�P�[����K�p
                v.vertex.xyz *= _ObjectScale;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o, o.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ��������UV���W�̌v�Z���W�b�N��ύX
                float3 normal = normalize(i.worldNormal);
                float2 newUV = i.uv;

                // ��ʂ܂��͉��ʂ��𔻒�
                if (abs(normal.y) > 0.9f)
                {
                    // ��ʂ܂��͉��ʂ̏ꍇ�iY���ɐ����j
                    // 5x5�̃O���b�h�Ƀ}�b�s���O���邽�߁AUV��5�{�ɃX�P�[��
                    newUV.x = i.uv.x * 5.0f;
                    newUV.y = i.uv.y * 5.0f;
                }
                else
                {
                    // ���ʂ̏ꍇ�iX�܂���Z���ɐ����j
                    // 5x1�̃O���b�h�Ƀ}�b�s���O
                    // Unity�̃f�t�H���g��UV��0�`1�Ȃ̂ŁAU���W��5�{�ɁAV���W�͂��̂܂�
                    newUV.x = i.uv.x * 5.0f;
                    // V���W�i���������j�͌��̂܂�
                    newUV.y = i.uv.y * 1.0f;
                }

                fixed4 maskColor = tex2D(_MaskTex, newUV);
                float maskValue = maskColor.r;

                float2 processedUV = newUV;
                float2 pivot = float2(0.5, 0.5);
                processedUV = (processedUV - pivot) * _PatternScale + pivot;

                float rotationDirection = 1.0;
                float3 N = normalize(i.worldNormal);

                if (abs(N.z) > abs(N.x) && abs(N.z) > abs(N.y))
                {
                    rotationDirection = 1.0;
                }
                else if (abs(N.x) > abs(N.y))
                {
                    rotationDirection = -1.0;
                }
                else
                {
                    rotationDirection = 1.0;
                }

                float angle = _Time.y * _RotationSpeed * rotationDirection;
                float s = sin(angle);
                float c = cos(angle);

                processedUV -= pivot;
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                processedUV = mul(rotationMatrix, processedUV);
                processedUV += pivot;

                // �����ŐV����UV���g�p
                fixed4 patternTexColor = tex2D(_PatternTex, processedUV);
                float patternLuminosity = dot(patternTexColor.rgb, float3(0.299, 0.587, 0.114));

                // �w�i�Ɩ͗l�̔������ʂɌv�Z
                fixed3 backgroundGlow = _BackgroundGlowColor.rgb * _BackgroundEmission;
                fixed3 patternGlow = _PatternColor.rgb * _PatternEmission;

                // �P�x�Ɋ�Â��āA�w�i�Ɩ͗l�̔�������`���
                // patternLuminosity��0(��)�ɋ߂Â���backgroundGlow���A1(��)�ɋ߂Â���patternGlow�������Ȃ�
                fixed3 finalPatternEmission = lerp(backgroundGlow, patternGlow, patternLuminosity);

                // �p�^�[���e�N�X�`���ɍŏI�I�Ȕ����F�Ɣw�i�F��K�p
                fixed3 finalPatternRGB = _BackgroundColor.rgb + finalPatternEmission;

                // �}�X�N�̒l�Ɋ�Â��āA_MainColor�Ɣ�������p�^�[������`���
                fixed3 finalRGB = lerp(_MainColor.rgb, finalPatternRGB, maskValue);

                float alpha = lerp(_SolidOpacity, 1.0, maskValue);

                fixed4 col = fixed4(finalRGB, alpha);

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}