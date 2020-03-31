Shader "JazzBox/WorldSpaceStandard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _AddColor ("Add Color", Color) = (0,0,0,0)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Scale ("Texture Scale", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _AddColor;
        fixed4 _MainTex_TexelSize;
        float _Scale;

        void vert (inout appdata_full v) {
            float3 worldPos = mul (unity_ObjectToWorld, v.vertex);
            float3 worldNormal = mul( unity_ObjectToWorld, float4( v.normal, 0.0 ) ).xyz;
            //o.worldNormal = worldNormal;
            worldNormal = abs(worldNormal);

            worldNormal.x = (worldNormal.x > worldNormal.y && worldNormal.x > worldNormal.z)?1:0;
            worldNormal.y = (worldNormal.y > worldNormal.x && worldNormal.y > worldNormal.z)?1:0;
            worldNormal.z = (worldNormal.z > worldNormal.y && worldNormal.z > worldNormal.x)?1:0;

            v.texcoord = float4(0,0,0,0);
            v.texcoord.x = (worldPos.z * worldNormal.x) + (worldPos.x * worldNormal.z) + (worldPos.x * worldNormal.y);
            v.texcoord.y = (worldPos.y * worldNormal.x) + (worldPos.y * worldNormal.z) + (worldPos.z * worldNormal.y);


            float ratio = _MainTex_TexelSize.x / _MainTex_TexelSize.y;
            v.texcoord.x *= ratio;
            v.texcoord.xy /= _Scale;
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb + _AddColor;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
