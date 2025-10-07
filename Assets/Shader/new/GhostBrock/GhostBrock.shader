Shader "Custom/GhostBrock"
{
    Properties
    {
        // �e�N�X�`��
        _MainTex ("Texture", 2D) = "white" {}                   // �ݒ肷��e�N�X�`��
        _TextureDensity ("Texture Density", float) = 0.5        // �e�N�X�`���̐F
        // �O�g
        _EdgeColor ("Edge Color", Color) = (0,0,0,1)            // �F
        _EdgeShine ("Edge Shine",Range(0,50)) = 30              // ���邳
        // ��
        _SurfaceColor ("Surface Color", Color) = (1,1,1,1)      // �F       
        _SurfaceDarkness ("Surface Darkness",Range(0,1)) = 0.3  // �Â�
        // �����x
        _Transparency ("_Transparency",Range(0,1)) = 0.95
        
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"} 
        Blend SrcAlpha OneMinusSrcAlpha
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
                fixed4	color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0; 
                UNITY_FOG_COORDS(1) 
                float4 vertex : SV_POSITION; 
            };

            sampler2D _MainTex; 
            float4 _MainTex_ST;            
            float4 _EdgeColor;
            float _EdgeShine;
            float4 _SurfaceColor;
            float _SurfaceDarkness;
            float _Transparency;
            float _TextureDensity;

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
                fixed4 col = tex2D(_MainTex, i.uv); 

                // �e�N�X�`���̖��邳���v�Z
                float brightness = dot(col.rgb, float3(0.299, 0.587, 0.114));

                // �Â��������G�b�W�Ɣ���
                if(brightness < _TextureDensity)
                {                      
                    // �F��ݒ�
                    col.rgb = _EdgeColor.rgb;
                    // ���邳��ݒ�
                    col.rgb *= _EdgeShine;
                }
                // ���邢������ʂƔ���
                else
                {
                    // �F��ݒ�
                    col.rgb = _SurfaceColor.rgb;   
                    // �Â���ݒ�
                    col.rgb *= _SurfaceDarkness;
                }

                // �����x��ݒ�
                col.a *= _Transparency;

                return col;
            }
            ENDCG 
        }
    }
}