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

			#include "UnityCG.cginc" // ★ 修正箇所

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
				// メインテクスチャと発光テクスチャの色とアルファを取得
				fixed4 mainTexColor = tex2D(_MainTex, i.uv);
				fixed4 emissionTexColor = tex2D(_EmissionMap, i.uv);
				
				// _MainTexの輝度（グレースケール値）を計算
				float mainBrightness = (mainTexColor.r + mainTexColor.g + mainTexColor.b) / 3.0;

				// _MainTexから通常の色（発光なし）を生成
				// 黒から_BaseColorへ補間する
				fixed3 baseColor = lerp(fixed3(0,0,0), _BaseColor.rgb, mainBrightness);
				
				// _EmissionMapのアルファ値を使って、発光色を生成
				// _EmissionMapが透明な部分(アルファが0)では発光しない
				fixed3 emission = _BaseColor.rgb * _EmissionStrength * emissionTexColor.a;

				// _MainTexの輝度に基づいてアルファを計算
				float mainAlpha = lerp(0.0, 1.0, mainBrightness);

				// 最終的なアルファを決定
				// _MainTexの輝度に基づくアルファと_EmissionMapのアルファのうち、より高い方を採用して重なりを表現
				float finalAlpha = max(mainAlpha, emissionTexColor.a);
				
				// 最終色を返す（通常色 + 発光色）
				return fixed4(baseColor + emission, finalAlpha);
			}
			ENDCG
		}
	}
	FallBack "Standard"
}