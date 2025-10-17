Shader "Custom/UpgradeGlow"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _UpgradeLevel ("Upgrade Level", Range(0, 5)) = 0
        _GlowColor ("Glow Color", Color) = (0,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.0
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2.0
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 2.0
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
        float _UpgradeLevel;
        fixed4 _GlowColor;
        float _GlowIntensity;
        float _PulseSpeed;
        float _EmissionStrength;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            // Calculate glow effect based on upgrade level
            float glowAmount = _UpgradeLevel / 5.0f; // Normalize to 0-1
            glowAmount = saturate(glowAmount);
            
            // Add pulsing effect
            float pulse = sin(_Time.y * _PulseSpeed) * 0.3f + 0.7f;
            glowAmount *= pulse;
            
            // Create glow color
            fixed4 glow = _GlowColor * glowAmount * _GlowIntensity;
            
            // Blend with base color
            c = lerp(c, c + glow, glowAmount * 0.5f);
            
            o.Albedo = c.rgb;
            o.Emission = glow.rgb * _EmissionStrength;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
