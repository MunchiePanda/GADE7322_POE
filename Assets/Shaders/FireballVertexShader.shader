Shader "Custom/FireballVertexShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _FireIntensity ("Fire Intensity", Range(0, 1)) = 0.5
        _FireSpeed ("Fire Speed", Range(0, 10)) = 1.0
        _FireScale ("Fire Scale", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float _FireIntensity;
        float _FireSpeed;
        float _FireScale;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample the texture
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Add fire effect
            float fireEffect = sin(_Time.y * _FireSpeed + IN.uv_MainTex.y * 10) * _FireIntensity;
            fireEffect = pow(fireEffect, 2) * _FireScale;
            c.rgb += float3(fireEffect, fireEffect * 0.5, 0);

            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
