Shader "Custom/GameOverUI"
{
    Properties
    {
        // テクスチャ
        _MainTex ("Texture", 2D) = "white" {}                   // 設定するテクスチャ
        _TextureDensity ("Texture Density", Range(0,1)) = 1     // テクスチャの明るさ
        // 外枠
        _EdgeColor ("Edge Color", Color) = (0,0,0,1)            // 色
        _EdgeShine ("Edge Shine",Range(1,100)) = 1             // 明るさ
        // 面
        _SurfaceColor ("Surface Color", Color) = (1,1,1,1)      // 色       
        _SurfaceDarkness ("Surface Darkness",Range(1,100)) = 1  // 暗さ
        // 透明度
        _Transparency ("_Transparency",Range(0,1)) = 1
        
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"} 
        Blend SrcAlpha OneMinusSrcAlpha       
        ZWrite On
        ZTest LEqual
        Cull Back
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

                // テクスチャの明るさを計算
                float brightness = dot(col.rgb, float3(0.299, 0.587, 0.114));

                // 透明部分は描画しない
                if (col.a < 0.01)
                {
                    discard; 
                }

                // 暗い部分をエッジと判定
                if(brightness < _TextureDensity)
                {                      
                    // 色を設定
                    col.rgb = _EdgeColor.rgb;
                    // 明るさを設定
                    col.rgb *= _EdgeShine;
                }
                // 明るい部分を面と判定
                else
                {
                    // 色を設定
                    col.rgb = _SurfaceColor.rgb;   
                    // 暗さを設定
                    col.rgb *= _SurfaceDarkness;
                }

                // 透明度を設定
                col.a *= _Transparency;

                return col;
            }
            ENDCG 
        }        
    }
}