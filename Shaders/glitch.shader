Shader "JazzBox/glitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitchTex ("Glitch", 2D) = "white" {}
        _Heavy ("Heavy", float) = 10
        _Stren ("Stren", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Heavy;
            sampler2D _GlitchTex;
            float _Stren;

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
                float heavy = tex2D(_MainTex, i.uv).r;



                heavy = lerp(0, heavy, _Stren);
                // heavy *= (1/_Heavy);
                i.uv = floor(i.uv / heavy) * heavy;
                // return heavy;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
