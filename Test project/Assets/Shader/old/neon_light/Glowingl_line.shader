Shader "Custom/MaskedGlowingPattern"
{
    Properties
    {
        // �}�X�N�e�N�X�`���B���������̓\���b�h�ȐF�A�����������}�X�N�B
        _MaskTex ("Mask Texture (R=Mask, Black=Solid Color)", 2D) = "white" {}
        // �}�X�N�e�N�X�`���̍��������ɓK�p�����\���b�h�ȐF�B
        _MainColor ("Solid Color (for Black Mask Areas)", Color) = (0,0,0,1)

        // �}�X�N�̔��������ɕ\�������͗l�e�N�X�`���B
        _PatternTex ("Pattern Texture", 2D) = "white" {}
        // �͗l�e�N�X�`���̑S�̂̐F�B���������͂��̐F�A���������͔��B
        _BackgroundColor ("_BackgroundColor", Color) = (1,1,1,1)
        // �͗l�e�N�X�`���̔����F�B
        _PatternColor ("Pattern Glow Color", Color) = (1,1,1,1)
        // �͗l�e�N�X�`���̔������x�B
        _PatternEmission ("Pattern Emission Strength", Range(0, 10)) = 1.0
        // �͗l�e�N�X�`���̉�]���x�B
        _RotationSpeed ("Pattern Rotation Speed", Range(0, 5)) = 0.5
        // �͗l�e�N�X�`���̃X�P�[���B
        _PatternScale ("Pattern Scale", Range(0.1, 10.0)) = 1.0
    }
    SubShader
    {
        // �����_�����O�^�C�v��s�����ɐݒ�
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // �t�H�O��L���ɂ��邽�߂̃R���p�C���f�B���N�e�B�u
            #pragma multi_compile_fog

            // Unity�̋���CG�C���N���[�h�t�@�C��
            #include "UnityCG.cginc"

            // �A�v���P�[�V��������o�[�e�b�N�X�V�F�[�_�[�֓n�����f�[�^�\��
            struct appdata
            {
                float4 vertex : POSITION; // ���_���W
                float2 uv : TEXCOORD0;    // UV���W
                float3 normal : NORMAL;   // �@���x�N�g����ǉ�
            };

            // �o�[�e�b�N�X�V�F�[�_�[����t���O�����g�V�F�[�_�[�֓n�����f�[�^�\��
            struct v2f
            {
                float2 uv : TEXCOORD0;    // UV���W
                UNITY_FOG_COORDS(1)       // �t�H�O�v�Z�p�̍��W
                float4 vertex : SV_POSITION; // �N���b�v��Ԃł̒��_���W
                float3 worldNormal : TEXCOORD1; // ���[���h��Ԃ̖@���x�N�g����ǉ�
            };

            // �V�F�[�_�[�v���p�e�B�Ƃ��Ē�`���ꂽ�e�N�X�`���ƃJ���[
            sampler2D _MaskTex;
            float4 _MaskTex_ST; // �}�X�N�e�N�X�`���̃X�P�[���ƃI�t�Z�b�g

            sampler2D _PatternTex;
            float4 _PatternTex_ST; // �p�^�[���e�N�X�`���̃X�P�[���ƃI�t�Z�b�g

            fixed4 _MainColor;
            fixed4 _BackgroundColor;
            fixed4 _PatternColor;
            float _PatternEmission;
            float _RotationSpeed;
            float _PatternScale;

            // �o�[�e�b�N�X�V�F�[�_�[
            v2f vert (appdata v)
            {
                v2f o;
                // �I�u�W�F�N�g��Ԃ̒��_���W���N���b�v��Ԃɕϊ�
                o.vertex = UnityObjectToClipPos(v.vertex);
                // ���b�V����UV���W���}�X�N�e�N�X�`���̃X�P�[���ƃI�t�Z�b�g�ŕϊ�
                o.uv = TRANSFORM_TEX(v.uv, _MaskTex);
                // �t�H�O���W���v�Z
                UNITY_TRANSFER_FOG(o,o.vertex);

                // ���[���h��Ԃ̖@���x�N�g�����v�Z���ăt���O�����g�V�F�[�_�[�ɓn��
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            // �t���O�����g�V�F�[�_�[
            fixed4 frag (v2f i) : SV_Target
            {
                // �}�X�N�e�N�X�`�����T���v�����O
                fixed4 maskColor = tex2D(_MaskTex, i.uv);

                // �}�X�N�̋��x���v�Z�i�ԃ`�����l�����g�p�j�B
                float maskValue = maskColor.r;

                // �p�^�[���e�N�X�`����UV����]����уX�P�[��������
                float2 processedUV = i.uv;

                // �X�P�[����K�p
                // pivot�𒆐S�ɂ���UV���X�P�[������
                float2 pivot = float2(0.5, 0.5);
                processedUV = (processedUV - pivot) * _PatternScale + pivot;

                // ���[���h��Ԃ̖@���Ɋ�Â��ĉ�]����������
                float rotationDirection = 1.0; // �f�t�H���g�͍����

                // �@���𐳋K��
                float3 N = normalize(i.worldNormal);

                // �e�ʂɑ΂��ĉ�]������ݒ�
                // Z���i�O��j�ɋ߂��ꍇ
                if (abs(N.z) > abs(N.x) && abs(N.z) > abs(N.y))
                {
                    // �O�� (Z > 0) �܂��͔w�� (Z < 0) �͍���� (rotationDirection = 1.0)
                    rotationDirection = 1.0;
                }
                // X���i���E�j�ɋ߂��ꍇ
                else if (abs(N.x) > abs(N.y))
                {
                    // ���� (X < 0) �܂��͉E�� (X > 0) �͉E���
                    rotationDirection = -1.0;
                }
                // Y���i�㉺�j�ɋ߂��ꍇ
                else
                {
                    // ��� (Y > 0) �܂��͉��� (Y < 0) �͉E���
                    rotationDirection = 1.0;
                }


                // ��]��K�p
                // ���݂̎��ԂƉ�]���x�A�����Č��肳�ꂽ��]�����Ɋ�Â��Ċp�x���v�Z
                float angle = _Time.y * _RotationSpeed * rotationDirection;
                float s = sin(angle);
                float c = cos(angle);

                // pivot�𒆐S�ɉ�]������
                processedUV -= pivot; // ���S�����_�Ɉړ�
                // ��]�s����쐬
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                // UV����]
                processedUV = mul(rotationMatrix, processedUV);
                processedUV += pivot; // ���̈ʒu�ɖ߂�

                // �������ꂽUV�Ńp�^�[���e�N�X�`�����T���v�����O
                fixed4 patternTexColor = tex2D(_PatternTex, processedUV);

                // �p�^�[���e�N�X�`���̋P�x���v�Z
                // �W���I�ȋP�x�v�Z��: 0.299 * R + 0.587 * G + 0.114 * B
                // ���ɋ߂��ق�0�A���ɋ߂��ق�1
                float patternLuminosity = dot(patternTexColor.rgb, float3(0.299, 0.587, 0.114));

                // �P�x�Ɋ�Â��āA_PatternTintColor �Ɣ��̊Ԃŕ��
                // �P�x��0(��)�ɋ߂��ق� _PatternTintColor �ɁA1(��)�ɋ߂��قǔ� (fixed3(1,1,1)) �ɂȂ�悤�Ƀu�����h
                fixed3 tintedPatternRGB = lerp(_BackgroundColor.rgb, fixed3(1,1,1), patternLuminosity);

                // ��Ԃ��ꂽ�F���p�^�[���e�N�X�`���̐F�Ƃ��ēK�p
                patternTexColor.rgb = tintedPatternRGB;

                // �p�^�[���e�N�X�`���ɔ����F�Ɣ������x��K�p
                fixed3 glowingPattern = patternTexColor.rgb * _PatternColor.rgb * _PatternEmission;

                // �}�X�N�̒l�Ɋ�Â��āA_MainColor �Ɣ�������p�^�[������`���
                // maskValue �� 0 �̏ꍇ _MainColor�A1 �̏ꍇ glowingPattern
                fixed3 finalRGB = lerp(_MainColor.rgb, glowingPattern, maskValue);


                // �ŏI�I�ȐF�i�A���t�@�l�͕s�����Ƃ���1.0�j
                fixed4 col = fixed4(finalRGB, 1.0);

                // �t�H�O��K�p
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
