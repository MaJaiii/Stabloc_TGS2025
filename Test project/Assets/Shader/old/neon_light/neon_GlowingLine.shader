Shader "Custom/MaskedGlowingPattern"
{
    Properties
    {
        // �}�X�N�e�N�X�`���B���������̓\���b�h�ȐF�ɂȂ�A�����������}�X�N�Ƃ��ċ@�\���܂��B
        _MaskTex ("Mask Texture (R=Mask, Black=Solid Color)", 2D) = "white" {}
        // �}�X�N�e�N�X�`���̍��������ɓK�p�����\���b�h�ȐF�B
        _MainColor ("Solid Color (for Black Mask Areas)", Color) = (0,0,0,1)

        // �}�X�N�̔��������ɕ\�������͗l�e�N�X�`���B
        _PatternTex ("Pattern Texture", 2D) = "white" {}
        // �͗l�e�N�X�`���̑S�̂̐F�B
        _PatternTintColor ("Pattern Tint Color", Color) = (1,1,1,1) // �V�����ǉ�
        // �͗l�e�N�X�`���̔����F�B
        _PatternColor ("Pattern Glow Color", Color) = (1,1,1,1)
        // �͗l�e�N�X�`���̔������x�B
        _PatternEmission ("Pattern Emission Strength", Range(0, 10)) = 1.0
        // �͗l�e�N�X�`���̉�]���x�B
        _RotationSpeed ("Pattern Rotation Speed", Range(0, 5)) = 0.5
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
            };

            // �o�[�e�b�N�X�V�F�[�_�[����t���O�����g�V�F�[�_�[�֓n�����f�[�^�\��
            struct v2f
            {
                float2 uv : TEXCOORD0;    // UV���W
                UNITY_FOG_COORDS(1)       // �t�H�O�v�Z�p�̍��W
                float4 vertex : SV_POSITION; // �N���b�v��Ԃł̒��_���W
            };

            // �V�F�[�_�[�v���p�e�B�Ƃ��Ē�`���ꂽ�e�N�X�`���ƃJ���[
            sampler2D _MaskTex;
            float4 _MaskTex_ST; // �}�X�N�e�N�X�`���̃X�P�[���ƃI�t�Z�b�g

            sampler2D _PatternTex;
            float4 _PatternTex_ST; // �p�^�[���e�N�X�`���̃X�P�[���ƃI�t�Z�b�g

            fixed4 _MainColor;
            fixed4 _PatternTintColor; // �V�����ǉ�
            fixed4 _PatternColor;
            float _PatternEmission;
            float _RotationSpeed;

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
                return o;
            }

            // �t���O�����g�V�F�[�_�[
            fixed4 frag (v2f i) : SV_Target
            {
                // �}�X�N�e�N�X�`�����T���v�����O
                fixed4 maskColor = tex2D(_MaskTex, i.uv);

                // �}�X�N�̋��x���v�Z�i�ԃ`�����l�����g�p�j�B
                // maskValue �� 0 (��) �ɋ߂��ق� _MainColor �ɂȂ�A
                // maskValue �� 1 (��) �ɋ߂��ق� _PatternTex ���K�p����܂��B
                float maskValue = maskColor.r;

                // �p�^�[���e�N�X�`����UV����]������
                float2 rotatedUV = i.uv;
                // ���݂̎��ԂƉ�]���x�Ɋ�Â��Ċp�x���v�Z
                float angle = _Time.y * _RotationSpeed;
                float s = sin(angle);
                float c = cos(angle);

                // UV�̒��S (0.5, 0.5) ����ɉ�]������
                float2 pivot = float2(0.5, 0.5);
                rotatedUV -= pivot; // ���S�����_�Ɉړ�
                // ��]�s����쐬
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                // UV����]
                rotatedUV = mul(rotationMatrix, rotatedUV);
                rotatedUV += pivot; // ���̈ʒu�ɖ߂�

                // ��]����UV�Ńp�^�[���e�N�X�`�����T���v�����O
                fixed4 patternTexColor = tex2D(_PatternTex, rotatedUV);

                // �p�^�[���e�N�X�`���ɐV����_PatternTintColor��K�p
                patternTexColor.rgb *= _PatternTintColor.rgb; // �����ŐF����Z���܂�

                // �p�^�[���e�N�X�`���ɔ����F�Ɣ������x��K�p
                // �����͍ŏI�I�ȐF�ɉ��Z����邽�߁AHDR (High Dynamic Range) ���ł����ʓI�ł��B
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
