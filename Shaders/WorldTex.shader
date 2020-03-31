Shader "JazzBox/WorldTex"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LightDiff ("Light Variance", float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members worldPos)
#pragma exclude_renderers d3d11
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD3;
				float3 worldNormal : TEXCOORD2;

				UNITY_FOG_COORDS(1)
			};

			sampler2D _MainTex;
			sampler2D _FloorTex;
			sampler2D _AltTex;
			sampler2D _FloorAltTex;
			float _LightDiff;

			float4 mix(float4 a, float4 b, float amt) {
				return ((1.0 - amt) * a) + b;
			}

			//get a scalar random value from a 3d value
			float rand(float3 value){
				//make value smaller to avoid artefacts
				float3 smallValue = sin(value);
				//get scalar value from 3d vector
				float random = dot(smallValue, float3(12.9898, 78.233, 37.719));
				//make value more random by making it bigger and then taking teh factional part
				random = frac(sin(random) * 143758.5453);
				return random;
			}
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 worldPos = mul (unity_ObjectToWorld, v.vertex);
				float3 worldNormal = mul( unity_ObjectToWorld, float4( v.normal, 0.0 ) ).xyz;
				o.worldNormal = worldNormal;
				worldNormal = abs(worldNormal);

				worldNormal.x = (worldNormal.x > worldNormal.y && worldNormal.x > worldNormal.z)?1:0;
				worldNormal.y = (worldNormal.y > worldNormal.x && worldNormal.y > worldNormal.z)?1:0;
				worldNormal.z = (worldNormal.z > worldNormal.y && worldNormal.z > worldNormal.x)?1:0;

				o.uv = float2(0,0);
				o.uv.x = (worldPos.z * worldNormal.x) + (worldPos.x * worldNormal.z) + (worldPos.x * worldNormal.y);
				o.uv.y = (worldPos.y * worldNormal.x) + (worldPos.y * worldNormal.z) + (worldPos.z * worldNormal.y);

				o.worldPos = worldPos;

				

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			float2 coolmod(float2 uv, float mod) {
				return ((uv % mod) + mod) % mod;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float isFloor = abs(i.worldNormal.y) > .5;
				fixed4 col;

				i.uv = coolmod(i.uv, 1.0);
				i.uv *= 0.5;
				i.uv = i.uv % 0.5;			

				if (isFloor > 0.5) {
					if (rand(floor(i.worldPos.yxz)) > 0.95) {
						
						i.uv.x += 0.5;
						col = tex2D(_MainTex, i.uv);
					} else {

						col = tex2D(_MainTex, i.uv);
					}
				} else {
					if (rand(floor(i.worldPos.xxz * 2) / 2) > 0.8) {
						i.uv += 0.5;
						col = tex2D(_MainTex, i.uv);
					} else {
						i.uv.y += 0.5;
						col = tex2D(_MainTex, i.uv);
						float mod = floor(rand(floor(i.worldPos.xzz * 2)/2) * 4) / 4;
						mod = 0.66 + (mod * 0.66);

						mod = 1.0 - mod;
						mod *= _LightDiff;
						mod = 1.0 - mod;
						col *= mod;
					}

					if (i.worldNormal.x < -0.5 || i.worldNormal.z > 0.5) col *= 0.45;
				}

				

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				

				return col;
			}
			ENDCG
		}
	}

	Fallback "Diffuse"
}
