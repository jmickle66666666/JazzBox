Shader "Jazz/WorldSpaceUnlitDepth"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale ("Texture Scale", float) = 1.0
        _Color ("Color", Color) = (1,1,1,1)
        _AddColor ("Add Color", Color) = (0,0,0,0)
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
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD3;
				float3 worldNormal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _MainTex_TexelSize;
            float _Scale;
            fixed4 _Color;
            fixed4 _AddColor;

            v2f vert (appdata_base v)
            {
                v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

                float3 worldPos = mul (unity_ObjectToWorld, v.vertex);
                // float3 worldPos = v.vertex;
				float3 worldNormal = mul( unity_ObjectToWorld, float4( v.normal, 0.0 ) ).xyz;
				o.worldNormal = worldNormal;
				worldNormal = abs(worldNormal);

				worldNormal.x = (worldNormal.x > worldNormal.y && worldNormal.x > worldNormal.z)?1:0;
				worldNormal.y = (worldNormal.y > worldNormal.x && worldNormal.y > worldNormal.z)?1:0;
				worldNormal.z = (worldNormal.z > worldNormal.y && worldNormal.z > worldNormal.x)?1:0;

                o.uv = float2(0,0);
				o.uv.x = (worldPos.z * worldNormal.x) + (worldPos.x * worldNormal.z) + (worldPos.x * worldNormal.y);
				o.uv.y = (worldPos.y * worldNormal.x) + (worldPos.y * worldNormal.z) + (worldPos.z * worldNormal.y);
                // o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                float ratio = _MainTex_TexelSize.x / _MainTex_TexelSize.y;
                o.uv.x *= ratio;
                o.uv.xy /= _Scale;

                o.uv = TRANSFORM_TEX(o.uv, _MainTex);

				o.worldPos = worldPos;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col + _AddColor;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
