Shader "Roobos/Toon"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Sharpness ("Sharpness", float) = 1.0
        _LightEdge ("Light Edge", Range(0,1)) = .5
        _ShadowStrength ("Shadow Strength", Range(0,1)) = .5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Ramp fullforwardshadows 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        float _Sharpness;
        float _LightEdge;
        float _ShadowStrength;

        struct Input
        {
            float2 uv_MainTex;
        };

        half4 LightingRamp (SurfaceOutput s, half3 lightDir, half atten) {
            half NdotL = dot (s.Normal, lightDir);
            half diff = NdotL * _Sharpness - (_Sharpness * _LightEdge);
            diff = saturate(diff);
            // return diff;
            // diff *= _ShadowStrength;
            // diff /= 3;
            // half3 ramp = tex2D (_Ramp, float2(diff)).rgb;
            // half4 c;
            // c.rgb = s.Albedo * _LightColor0.rgb * ramp * atten;
            // c.a = s.Alpha;
            half4 c;
            // c.rgb = s.Albedo;
            c.rgb = lerp(s.Albedo * (1.0 - _ShadowStrength), s.Albedo, diff);
            c.a = 1.0;
            // return s.Albedo;
            return c;
            // return atten;
        }

        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
