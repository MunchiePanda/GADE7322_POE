Shader "Custom/EnemyHealthGradient"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _HealthPercent ("Health Percent", Range(0, 1)) = 1.0
        _HealthyColor ("Healthy Color", Color) = (0,1,0,1)
        _DamagedColor ("Damaged Color", Color) = (1,1,0,1)
        _CriticalColor ("Critical Color", Color) = (1,0,0,1)
        _TransitionSpeed ("Transition Speed", Range(0, 10)) = 2.0
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
        float _HealthPercent;
        fixed4 _HealthyColor;
        fixed4 _DamagedColor;
        fixed4 _CriticalColor;
        float _TransitionSpeed;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            // Determine health color based on health percentage
            fixed4 healthColor;
            if (_HealthPercent > 0.6f)
            {
                // Healthy: green to yellow transition
                float t = (_HealthPercent - 0.6f) / 0.4f;
                healthColor = lerp(_DamagedColor, _HealthyColor, t);
            }
            else if (_HealthPercent > 0.3f)
            {
                // Damaged: yellow to red transition
                float t = (_HealthPercent - 0.3f) / 0.3f;
                healthColor = lerp(_CriticalColor, _DamagedColor, t);
            }
            else
            {
                // Critical: red with pulsing effect
                float pulse = sin(_Time.y * _TransitionSpeed * 2) * 0.3f + 0.7f;
                healthColor = _CriticalColor * pulse;
            }
            
            // Blend base color with health color
            c = lerp(c, healthColor, 0.6f);
            
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
