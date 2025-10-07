Shader "Custom/NeonTextureShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_BaseColor ("_BaseColor", Color) = (0,1,0,1) // 緑
		_EmissionStrength ("Emission Strength", Range(0,200)) = 2
		_scale("Scale", Float) = 1
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _BaseColor;
			float _EmissionStrength;
			float _scale;

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
				v.vertex.xyz *= _scale;
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{


				// テクスチャの色を取得
				fixed4 texColor = tex2D(_MainTex, i.uv);

				// 透明部分（アルファが低い部分）は黒扱い
				if (texColor.a < 0.1)
				{
					return fixed4(0,0,0,1);
				}

				// 黒部分の検出
				float isBlack = step(texColor.r + texColor.g + texColor.b, 0.05);
				// 白部分の検出
				float isWhite = step(0.95, texColor.r) * step(0.95, texColor.g) * step(0.95, texColor.b);

				// 出力色の決定
				fixed3 finalColor = texColor.rgb;

				if (isBlack > 0.5)
				{
					// 黒は背景色に置き換え
					finalColor = _BaseColor.rgb;
				}
				else if (isWhite > 0.5)
				{
					// 白はそのまま白
					finalColor = fixed3(1,1,1);
				}


				// ネオン発光効果（Emission）
				fixed3 emission = finalColor * _EmissionStrength;

				return fixed4(finalColor + emission, 1.0);
			}
			ENDCG
		}
	}
}
