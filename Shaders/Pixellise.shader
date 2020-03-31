Shader "JazzBox/Pixellise" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_VSize ("Vertical Size", float) = 1.0
	_Colors ("Unique Colors", float) = 256.0
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
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Calculate the screen width from the aspect ratio
				float _HSize = (_ScreenParams.x / _ScreenParams.y) * _VSize;

				i.texcoord.x = floor(i.texcoord.x * _HSize) / _HSize;
				i.texcoord.y = floor(i.texcoord.y * _VSize) / _VSize;

				fixed4 col = tex2D(_MainTex, i.texcoord);

				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a);

				// Store the color ratios to maintain color when quantizing
				float rgRatio = col.g / col.r;
				float rbRatio = col.b / col.r;
				float r = floor(col.r * _Colors) / _Colors;

				col = float4(r, r * rgRatio, r * rbRatio, col.a);

				return col;
			}
		ENDCG
	}
}

}
