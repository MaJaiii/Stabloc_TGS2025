Shader "Custom/NeonTextureShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_EmissionMap ("Emission Map", 2D) = "white" {}
		_BaseColor ("_BaseColor", Color) = (0,1,0,1)
		_EmissionStrength ("Emission Strength", Range(0,20)) = 2
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc" // �� �C���ӏ�

			sampler2D _MainTex;
			sampler2D _EmissionMap;
			float4 _MainTex_ST;
			float4 _BaseColor;
			float _EmissionStrength;

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
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// ���C���e�N�X�`���Ɣ����e�N�X�`���̐F�ƃA���t�@���擾
				fixed4 mainTexColor = tex2D(_MainTex, i.uv);
				fixed4 emissionTexColor = tex2D(_EmissionMap, i.uv);
				
				// _MainTex�̋P�x�i�O���[�X�P�[���l�j���v�Z
				float mainBrightness = (mainTexColor.r + mainTexColor.g + mainTexColor.b) / 3.0;

				// _MainTex����ʏ�̐F�i�����Ȃ��j�𐶐�
				// ������_BaseColor�֕�Ԃ���
				fixed3 baseColor = lerp(fixed3(0,0,0), _BaseColor.rgb, mainBrightness);
				
				// _EmissionMap�̃A���t�@�l���g���āA�����F�𐶐�
				// _EmissionMap�������ȕ���(�A���t�@��0)�ł͔������Ȃ�
				fixed3 emission = _BaseColor.rgb * _EmissionStrength * emissionTexColor.a;

				// _MainTex�̋P�x�Ɋ�Â��ăA���t�@���v�Z
				float mainAlpha = lerp(0.0, 1.0, mainBrightness);

				// �ŏI�I�ȃA���t�@������
				// _MainTex�̋P�x�Ɋ�Â��A���t�@��_EmissionMap�̃A���t�@�̂����A��荂�������̗p���ďd�Ȃ��\��
				float finalAlpha = max(mainAlpha, emissionTexColor.a);
				
				// �ŏI�F��Ԃ��i�ʏ�F + �����F�j
				return fixed4(baseColor + emission, finalAlpha);
			}
			ENDCG
		}
	}
	FallBack "Standard"
}