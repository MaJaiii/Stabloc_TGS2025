Shader "Custom/AnimatedCubeShader"
{
    Properties
    {
        // �x�[�X�e�N�X�`��
        _BaseTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseEmission ("Base Emission Strength", Range(0, 100)) = 3.0

        // ���C���e�N�X�`��
        _LineTex ("Line Texture", 2D) = "white" {}
        _BeaconLineColor ("Line Color", Color) = (1,1,1,1)
        _LineEmission ("Line Emission Strength", Range(0, 100)) = 40.0

        // �X�P�[���A�j���[�V�����p
        _XZScale ("XZ Scale", Range(0.0, 10.0)) = 1.0
        _YScale ("Y Scale", Range(0.0, 10.0)) = 1.0

        // ��]�A�j���[�V�����p
        _RotationAngle ("Rotation Angle", Range(0, 360)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            
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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            sampler2D _BaseTex;
            fixed4 _BaseColor;
            float _BaseEmission;
            
            sampler2D _LineTex;
            fixed4 _BeaconLineColor;
            float _LineEmission;

            float _XZScale;
            float _YScale;
            float _RotationAngle;

            v2f vert (appdata v)
            {
                v2f o;

                float4 scaledVertex = v.vertex;
                scaledVertex.x *= _XZScale;
                scaledVertex.z *= _XZScale;
                scaledVertex.y *= _YScale;

                float rad = _RotationAngle * UNITY_PI / 180.0;
                float s = sin(rad);
                float c = cos(rad);
                float3x3 rotationMatrix = float3x3(
                    c, 0, s,
                    0, 1, 0,
                    -s, 0, c
                );
                
                scaledVertex.xyz = mul(rotationMatrix, scaledVertex.xyz);

                o.vertex = UnityObjectToClipPos(scaledVertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // �㉺�ʂ̖@�������o���ē����ɂ���
                if (abs(i.worldNormal.y) > 0.9f)
                {
                    discard;
                }

                fixed4 baseTex = tex2D(_BaseTex, i.uv);
                fixed4 lineTex = tex2D(_LineTex, i.uv);
                
                // �x�[�X�F�Ƀe�N�X�`������Z���A��������Z���čŏI�F������
                fixed3 finalBaseColor = baseTex.rgb * _BaseColor.rgb * _BaseEmission;
                
                // ���C���F�Ƀe�N�X�`������Z���A��������Z���čŏI�F������
                fixed3 finalLineColor = lineTex.rgb * _BeaconLineColor.rgb * _LineEmission;
                
                // ���C���̃A���t�@�l�Ɋ�Â��āA�x�[�X�ƃ��C���̐F���u�����h
                fixed3 finalRGB = lerp(finalBaseColor, finalLineColor, lineTex.a);
                
                // �A���t�@�l�̓��C���e�N�X�`���̃A���t�@�l�Ɉˑ�
                fixed alpha = lerp(baseTex.a, 1.0, lineTex.a);

                fixed4 col = fixed4(finalRGB, alpha);
                return col;
            }
            ENDCG
        }
    }
}