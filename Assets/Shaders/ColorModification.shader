Shader "Custom/ColorModification"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _DamageColor ("Damage Color", Color) = (1,0,0,1)
        _DamageIntensity ("Damage Intensity", Range(0, 1)) = 0.0
        _DamageFadeSpeed ("Damage Fade Speed", Range(0, 10)) = 1.0
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
        fixed4 _DamageColor;
        float _DamageIntensity;
        float _DamageFadeSpeed;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            // Blend between base color and damage color based on intensity
            c = lerp(c, _DamageColor, _DamageIntensity);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
