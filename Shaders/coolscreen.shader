Shader "Jazz/coolscreen" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_VSize ("Vertical Size", float) = 1.0
	_Colors ("Unique Colors", float) = 256.0

	// This controls the thickness of the outline (roughly)
	_DepthMix ("Depth Mix", Range(0.1, 0.9)) = 0.5
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 100
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _VSize;
			float _Colors;
			uniform sampler2D _CameraDepthTexture;
			float _DepthMix;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			float3 HUEtoRGB(in float H)
			{
				float R = abs(H * 6 - 3) - 1;
				float G = 2 - abs(H * 6 - 2);
				float B = 2 - abs(H * 6 - 4);
				return saturate(float3(R,G,B));
			}

			float Epsilon = 1e-10;

			float3 RGBtoHCV(in float3 RGB)
			{
				// Based on work by Sam Hocevar and Emil Persson
				float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
				float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
				float C = Q.x - min(Q.w, Q.y);
				float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
				return float3(H, C, Q.x);
			}

			float3 HSVtoRGB(in float3 HSV)
			{
				float3 RGB = HUEtoRGB(HSV.x);
				return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}

			float3 RGBtoHSV(in float3 RGB)
			{
				float3 HCV = RGBtoHCV(RGB);
				float S = HCV.y / (HCV.z + Epsilon);
				return float3(HCV.x, S, HCV.z);
			}

			float4 mix(float4 a, float4 b, float amt)
			{
				return ((a * amt) + (b * (1.0 - amt)));
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Calculate the screen width from the aspect ratio
				float _HSize = (_ScreenParams.x / _ScreenParams.y) * _VSize;

				i.texcoord.x = floor(i.texcoord.x * _HSize) / _HSize;
				i.texcoord.y = floor(i.texcoord.y * _VSize) / _VSize;

				// half d = step((_Time.w*2) % 1.0, 0.5);

				// Here we sample the depth buffer around the given pixel, to judge changes in depth

				float mdepth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.texcoord).r);
				float2 offset = float2(_DepthMix/_HSize, _DepthMix/_VSize); // _DepthMix controls the thickness of the outline
				float diff = 0.0;

				for (float a = -offset.x; a <= offset.x; a += offset.x) {
					for (float b = -offset.y; b <= offset.y; b += offset.y) {
						float ndepth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.texcoord + float2(a, b)).r);
						diff += abs(mdepth - ndepth);
					}
				}

				// If the depth is greater than a given threshold we immediately return solid black
				if (2.0 - diff < _DepthMix) return 0;

				fixed4 col = tex2D(_MainTex, i.texcoord);

				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a);

				half3 ccol = RGBtoHSV(col.xyz);

				// Store the color ratios to maintain color when quantizing
				// float rgRatio = ccol.g / ccol.r;
				// float rbRatio = ccol.b / ccol.r;
				// float r = floor(ccol.r * _Colors) / _Colors;

				// ccol = half3(r, r * rgRatio, r * rbRatio);
				//return float4(sin(i.texcoord.y * _VSize * 1.57),0,0,1);
				// if ( < 0.5) {
					// ccol.y -= (1.0/4.0) * step(sin((_Time.w*16.0) + i.texcoord.y * _VSize ),0);
				// }

				ccol = round(ccol * _Colors) / _Colors;

				ccol = HSVtoRGB(ccol);

				return float4(ccol.r, ccol.g, ccol.b, 1.0);
			}

			
			

		ENDCG
	}
}

}
