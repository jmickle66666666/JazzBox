// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Jazz/VertexSurfaceComp" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Compression ("Vertex Compression", float) = 100.0
	}
	SubShader {
		Pass { 
		Tags { "LightMode"="ShadowCaster" "Queue"="Transparent+1000000" }
		LOD 200

		ColorMask 0

		ZWrite On
		Blend One Zero
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0
		#include "UnityCG.cginc"
		#pragma multi_compile_fog
		float _Compression;

		struct v2f
		{
			float4 vertex : SV_POSITION;
		};

		v2f vert (appdata_base v)
		{
			
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.vertex = round(o.vertex * _Compression) / _Compression;
			UNITY_TRANSFER_FOG(o,o.vertex);
			return o;
		}

		fixed4 frag (v2f i) : SV_Target
		{
			return fixed4(0.0,1.0,1.0,0.0);
		}
		
		ENDCG
		}
	}
	SubShader
	{
		Tags { "Queue" = "Transparent+100000000" }
		LOD 100

		Pass
		{
			//ZTest Less
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
			float _Compression;
			
			v2f vert (appdata v)
			{
				
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex = round(o.vertex * _Compression) / _Compression;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	//FallBack "Diffuse"
}
