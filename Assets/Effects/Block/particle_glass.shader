Shader "Custom/particleGlassShader"
{
    Properties
    {
        // ���C���e�N�X�`��
        _MainTex ("_MainTex", 2D) = "white" {}
        // �������x
        _EmissionStrength ("_EmissionStrength", Range(1, 30)) = 30.0
        // �S�̂̓����x
        _OverallOpacity ("_OverallOpacity", Range(0, 1)) = 0.2
        // �X�y�L�����[�̋���
        _SpecularStrength ("_SpecularStrength", Range(0, 1)) = 1.0
        // �\�ʂ̊��炩��
        _Smoothness ("_Smoothness", Range(0, 1)) = 1.0
        // ���F���ʂ̋���
        _IridescenceStrength ("_IridescenceStrength", Range(0, 1)) = 0.5
        // ���F���ʂ̐F�ω����x
        _IridescenceShiftSpeed ("_IridescenceShiftSpeed", Range(0, 5)) = 0.6
    }
    SubShader
    {
        // �����_�����O�̃^�O�ݒ�i���ߗp�j
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
        // ���u�����h�i�W���I�ȓ��߁j
        Blend SrcAlpha OneMinusSrcAlpha
        // �[�x�������݂𖳌���
        ZWrite Off

        Pass
        {
            // �V�F�[�_�[�v���O�����J�n
            CGPROGRAM
            #pragma vertex vert              // ���_�V�F�[�_�[�w��
            #pragma fragment frag            // �t���O�����g�V�F�[�_�[�w��
            #pragma multi_compile_instancing // �C���X�^���V���O�Ή�
            #pragma multi_compile_fwdadd     // ���C�e�B���O�ǉ��p�X�Ή�

            // �r���g�C���V�F�[�_�[�p���C�u����
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            // �v���p�e�B�̕ϐ�
            float4 _MainTex_ST; 
            sampler2D _MainTex;
            float _EmissionStrength;
            float _OverallOpacity;
            float _SpecularStrength;
            float _Smoothness;
            float _IridescenceStrength;
            float _IridescenceShiftSpeed;

            // ���_���͍\����
            struct appdata
            {
                float4 vertex : POSITION;   // ���_���W
                float3 normal : NORMAL;     // �@��
                float2 uv : TEXCOORD0;      // UV���W
                float4 color : COLOR;       // ���_�J���[
                UNITY_VERTEX_INPUT_INSTANCE_ID // �C���X�^���V���O�pID
            };

            // ���_�V�F�[�_�[����t���O�����g�V�F�[�_�[�֓n���f�[�^
            struct v2f
            {
                float4 vertex : SV_POSITION; // �N���b�v���W
                float3 normalWS : TEXCOORD0; // ���[���h�@��
                float3 viewDirWS : TEXCOORD1;// ���[���h��Ԃł̎�������
                float2 uv : TEXCOORD2;       // UV
                float4 color : TEXCOORD3;    // ���_�J���[
                UNITY_VERTEX_OUTPUT_STEREO   // �X�e���I�����_�����O�Ή�
            };

            // ���_�V�F�[�_�[
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // �N���b�v��Ԃ֕ϊ�
                o.vertex = UnityObjectToClipPos(v.vertex);
                // UV�ϊ�
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // ���[���h�@��
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                // ���[���h��Ԃł̎����������v�Z
                o.viewDirWS = normalize(UnityWorldSpaceViewDir(o.vertex));
                // ���_�J���[�����̂܂ܓn��
                o.color = v.color;
                return o;
            }

            // �t���O�����g�V�F�[�_�[
            fixed4 frag (v2f i) : SV_Target
            {
                // �e�N�X�`���J���[���擾
                float4 texColor = tex2D(_MainTex, i.uv);
                
                // �x�[�X�J���[�ƃA���t�@�i���_�J���[���|�����킹�j
                fixed3 baseColor = texColor.rgb * i.color.rgb;
                fixed baseAlpha = texColor.a * i.color.a * _OverallOpacity;

                // �@���Ǝ��������𐳋K��
                fixed3 normalWS = normalize(i.normalWS);
                fixed3 viewDirWS = normalize(i.viewDirWS);
                
                // �������̕����ƐF
                fixed3 lightDirWS = _WorldSpaceLightPos0.xyz;
                fixed3 lightColor = _LightColor0.rgb;
                
                // ���˃x�N�g�����v�Z
                fixed3 reflectionDirWS = reflect(-lightDirWS, normalWS);
                
                // �X�y�L�����v�Z�i���炩�����������邽�ߎw����傫���j
                fixed spec = pow(saturate(dot(reflectionDirWS, viewDirWS)), _Smoothness * 100.0);
                fixed3 specularColor = lightColor * spec * _SpecularStrength;
                
                // ���F���ʂ̃I�t�Z�b�g�����Ԃŕω�
                fixed iridescenceOffset = _Time.y * _IridescenceShiftSpeed;
                // ���������Ɩ@���̓���
                fixed iridescence = dot(normalWS, viewDirWS);
                // �T�C���g�œ��F�𐶐��iRGB�����炵�Ĉʑ���ς���j
                fixed3 iridescenceColor = sin((iridescence + iridescenceOffset) * 3.14159 * 2.0 + fixed3(0, 2, 4)) * 0.5 + 0.5;
                
                // �x�[�X�J���[�Ɠ��F���u�����h���A�X�y�L���������Z
                fixed3 finalColor = lerp(baseColor, iridescenceColor, iridescence * _IridescenceStrength) + specularColor;
                // �������x����Z
                finalColor *= _EmissionStrength;

                // �ŏI�A���t�@
                fixed finalAlpha = baseAlpha;

                // �o��
                return fixed4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}
