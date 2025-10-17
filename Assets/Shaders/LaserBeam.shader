Shader "Custom/LaserBeam"
{
    Properties
    {
        _Color ("Laser Color", Color) = (1,0,0,1)
        _MainTex ("Laser Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 5)) = 1.0
        _PulseSpeed ("Pulse Speed", Range(0, 20)) = 5.0
        _ChargeProgress ("Charge Progress", Range(0, 1)) = 0.0
        _BeamWidth ("Beam Width", Range(0.1, 2)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 200
        
        Blend One One // Additive blending
        ZWrite Off
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float _Intensity;
        float _PulseSpeed;
        float _ChargeProgress;
        float _BeamWidth;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            // Create pulsing effect
            float pulse = sin(_Time.y * _PulseSpeed) * 0.3f + 0.7f;
            
            // Charge effect - beam gets brighter as it charges
            float chargeEffect = lerp(0.3f, 1.0f, _ChargeProgress);
            
            // Combine effects
            float finalIntensity = _Intensity * pulse * chargeEffect;
            
            // Apply intensity to color
            c.rgb *= finalIntensity;
            
            // Alpha based on charge progress and pulse
            c.a = _ChargeProgress * pulse * 0.8f;
            
            o.Albedo = c.rgb;
            o.Emission = c.rgb * 2.0f; // Strong emission for laser effect
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
