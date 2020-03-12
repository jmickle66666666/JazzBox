Shader "Custom/print"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Contrast ("Contrast", Range(0.01,10)) = 0.5
        _DotSpacing ("Dot Spacing", Range(0,1)) = 0.5
        _Thick ("Thick", Range(0.001,10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 200
        // Cull Off

        Pass
        {
            Cull Front
            // ZTest Always
            // Offset 1, 1
            // ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : POSITION;

            };

            float4 _Color;
            float _Thick;

            v2f vert (appdata v)
            {
                v2f o;
                v.vertex.xyz += normalize(v.normal) * _Thick;
                v.normal.xyz *= -1;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Ramp fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

        struct JazzSurface
        {
            float3 Albedo;
            float Alpha;
            float2 screenPos;
            float3 Normal;
            float3 Emission;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Contrast;
        float _DotSpacing;

        float4 LightingRamp (JazzSurface s, float3 lightDir, float atten) {
            float NdotL = dot (s.Normal, lightDir);

            // NdotL -= 0.4;

            NdotL -= 0.5;
            NdotL *= _Contrast;
            NdotL += 0.5;
            NdotL = saturate(NdotL);

            // float2 coords = s.screenPos.xy / s.screenPos.w;
            // if (s.screenPos.x % 0.1 < 0.05) return float4(0,0,0,1);

            // return float4(s.screenPos.x, s.screenPos.y, 0, 1);
            float off = (s.screenPos.y+_DotSpacing/2) % (_DotSpacing*2) > _DotSpacing ? 0 : 1;
            float2 nier = float2(0,0);
            nier.y = round(s.screenPos.y / _DotSpacing) * _DotSpacing;
            s.screenPos.x -= off * _DotSpacing * 0.5;
            nier.x = round(s.screenPos.x / _DotSpacing) * _DotSpacing;

            // nier.x += s.screenPos.y % _DotSpacing > 0.5 ? 0 : 0.1;



            float dist = distance(s.screenPos, nier);
            dist /= _DotSpacing;
            // float _DotSize *= 0.72;

            float dotsize = 1.0 - NdotL;
            dist = smoothstep(dotsize, dist, 1);
            // float dist = 1;
            // if (s.screenPos.x % 0.2 < 0.1) dist = 0;
            // dist -= .1;
            // dist = smoothstep(0, dist, 1);
            return dist;

            // NdotL = smoothstep(0, NdotL, 1);


            return half4(s.Albedo,s.Alpha) * NdotL;
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout JazzSurface o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);

            float2 coords = IN.screenPos.xy / IN.screenPos.w;
            coords.y /= _ScreenParams.x / _ScreenParams.y;
            // coords /= IN.screenPos.wz;
            // coords.y /= _ScreenParams.y;
            // if (coords.x % 0.2 < 0.1) c = float4(1,0,0,1);
            // if (coords.y % 0.2 < 0.1) c = float4(0,1,0,1);

            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.screenPos = coords;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
