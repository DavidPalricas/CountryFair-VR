Shader "CarnivalPop"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Saturation ("Saturação (Vibrance)", Range(0, 3)) = 1.5
        _Contrast ("Contraste", Range(0, 2)) = 1.2
        _Tint ("Filtro de Cor (Carnival)", Color) = (1, 0.95, 0.85, 1) // Um tom amarelado quente
        _Vignette ("Vignette (Foco VR)", Range(0, 1)) = 0.2
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

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
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _Saturation;
            float _Contrast;
            fixed4 _Tint;
            float _Vignette;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 1. APLICAR CONTRASTE
                // Move o ponto médio para 0.5, escala e volta
                col.rgb = (col.rgb - 0.5) * _Contrast + 0.5;

                // 2. APLICAR SATURAÇÃO (Luma based)
                // Calcula a luminância (preto e branco)
                float lum = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
                // Interpola entre cinzento e a cor original
                col.rgb = lerp(float3(lum, lum, lum), col.rgb, _Saturation);

                // 3. APLICAR TINT (Ambiente quente)
                col.rgb *= _Tint.rgb;

                // 4. VIGNETTE SIMPLES (Essencial para VR para focar o centro)
                float2 dist = (i.uv - 0.5) * 1.25;
                dist.x = 1 - dot(dist, dist) * _Vignette;
                col.rgb *= saturate(dist.x);

                return col;
            }
            ENDCG
        }
    }
}