Shader "Custom/Guide_Core"
{
	Properties
	{
		_Color ("Outline Color", Color) = (1,1,1,1)
		_OutlineWidth ("Outline Width", Range(0, 0.3)) = 0.01
		_GlowIntensity ("Glow Intensity", Range(0, 200)) = 1.0
		// “à‘¤‚ÌŒŠ‚ÌƒTƒCƒY‚ð§Œä
		_InnerRadius ("Inner Radius", Range(0, 1)) = 0.5
		// Œõ‚ª‘–‚é‘¬‚³‚ð§Œä
		_PulseSpeed ("Pulse Speed", Range(0.5, 20.0)) = 2.0
		// ‹——£‚ÌŒW”
		_DistMultiplier ("Distance Multiplier", Range(1, 50)) = 20.0
		// –@ü‚ÌŒW”
		_NormalZMultiplier ("NormalZ Multiplier", Range(1, 50)) = 10.0
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		Pass
		{
			Cull Off
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 worldNormal : TEXCOORD0;
				float isFrontFace : TEXCOORD1;
				float4 objPos : TEXCOORD2;
			};

			fixed4 _Color;
			float _OutlineWidth;
			float _GlowIntensity;
			float _InnerRadius;
			float _PulseSpeed;
			float _DistMultiplier; 
			float _NormalZMultiplier; 

			v2f vert (appdata v)
			{
				v2f o;

				float3 outlineVertex = v.vertex.xyz + v.normal * _OutlineWidth;

				o.vertex = UnityObjectToClipPos(float4(outlineVertex, 1.0));
				o.normal = v.normal;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);

				o.isFrontFace = (dot(v.normal, ObjSpaceViewDir(v.vertex)) > 0.0) ? 1.0 : -1.0;
				o.objPos = v.vertex;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				if (i.isFrontFace > 0)
				{
					float dist = length(i.objPos.xy);

					if (dist < _InnerRadius)
					{
						return fixed4(0,0,0,0);
					}
				}

				float normalZ = i.worldNormal.z;
				float dist = length(i.objPos.xyz);
				// ŒW”‚ð“K—p
				float pulse = sin(dist * _DistMultiplier + normalZ * _NormalZMultiplier + _Time.y * _PulseSpeed) * 0.5 + 0.5;

				fixed3 outlineColor = _Color.rgb * _GlowIntensity * pulse;
				fixed alpha = _Color.a * pulse;

				return fixed4(outlineColor, alpha);
			}
		ENDCG
		}
	}
}