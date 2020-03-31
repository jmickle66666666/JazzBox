Shader "JazzBox/Skybox Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
                float3 normal : NORMAL;
            };


            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 uv2 : TEXCOORD1;
                float3 viewDir : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform int _SectorHighlight;
            uniform int _PathHighlight;
            uniform int _SelectType;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = v.uv2;
                o.viewDir = - WorldSpaceViewDir(v.vertex);
                return o;
            }

            float ilerp(float x, float min, float max)
            {
                return (x * (max-min)) + min;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.viewDir);
                float colFade = smoothstep(.75, 1, abs(dir.y));

                float2 uv = float2(
                    (atan2(dir.z, dir.x) / 3.14) * .5 + .5,
                    dir.y
                );

                uv /= _MainTex_ST.xy;
                uv.y /= 3;

                uv.x += _Time.x * .5;
                float4 col = tex2D(_MainTex, uv);
                float4 baseCol = tex2D(_MainTex, float2(1,1));

                return lerp(col, baseCol, colFade);
                // return smoothstep(0.8,0.9,dir.z);
                // return float4(dir,1);



                // float light = ilerp(i.light, .6, 1);
                // fixed4 col = tex2D(_MainTex, i.uv);
                // // float norm = mul(UNITY_MATRIX_V, i.worldNormal).z;
                // // float norm = saturate(i.worldNormal.z + i.worldNormal.x);
                // // norm -= .5;
                // // // norm *= 0.75;
                // // norm = saturate(1.0-norm);
                // bool isSector = i.uv2.z < 2;
                // bool isSelected = i.uv2.z == _SelectType;
                // bool isSectorHighlighted = i.uv2.x == _SectorHighlight;
                // bool isPath = i.uv2.y == _PathHighlight;

                // bool highlighted = isSelected && ((isSectorHighlighted && isPath && !isSector) || (isSectorHighlighted && isSector));
                // float4 h = 0;
                // if (highlighted) {
                //     float size = .125;
                //     i.uv += _Time.x;
                //     h = step((i.uv.x + i.uv.y) % size, size/3) * float4(.6,.3,.1,1) * .66;
                // } 

                // float dist = 1/i.vertex.z;
                // float fog = 1.0 - (dist/_ProjectionParams.z);
                // // float fog = 1.0;

                // return col * fog * light + h;
            }
            ENDCG
        }
        
        // Pass to render object as a shadow caster
        Pass 
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On ZTest LEqual Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f { 
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert( appdata_base v )
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag( v2f i ) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }	
    }
}
