Shader "Custom/PieceShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _PaletteTex ("Palette Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _CurrentPalette ("Palette Color", Float) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "PreviewType"="Plane" }
        LOD 100
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _PaletteTex;
            float4 _MainTex_ST;
            float4 _PaletteTex_TexelSize;
            fixed4 _Color;
            float _CurrentPalette;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            /*
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                return texColor * _Color;
            }
            */

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);

                float paletteWidth = _PaletteTex_TexelSize.z;
                float paletteHeight = _PaletteTex_TexelSize.w;

                float stepX = 1.0 / paletteWidth;
                float stepY = 1.0 / paletteHeight;

                for (int j = 0; j < paletteWidth; j++)
                {
                    //float2 sourceUV = float2((j + 0.5) * stepX, 0.5 * stepY);
                    float2 sourceUV = float2((j + 0.5) * stepX, 1.0 - (0.5 * stepY));
                    float4 sourceColor = tex2D(_PaletteTex, sourceUV);

                    float3 diff = texColor.rgb - sourceColor.rgb;

                    if (abs(diff.r) < 0.1 && abs(diff.g) < 0.1 && abs(diff.b) < 0.1){
                        float2 targetUV = float2((j + 0.5) * stepX, 1.0 - ((_CurrentPalette + 0.5) * stepY));
                        float4 targetColor = tex2D(_PaletteTex, targetUV);

                        texColor.rgb = targetColor.rgb;
                        
                    }
                    
                }


                return texColor * _Color;
            }
            ENDCG
        }
    }
}
