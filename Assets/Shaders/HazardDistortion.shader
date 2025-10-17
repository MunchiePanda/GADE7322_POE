Shader "Custom/HazardDistortion"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _DistortionAmount ("Distortion Amount", Range(0, 2)) = 0.5
        _DistortionSpeed ("Distortion Speed", Range(0, 10)) = 1.0
        _DistortionScale ("Distortion Scale", Vector) = (1, 1, 1, 1)
        _WaveFrequency ("Wave Frequency", Range(0, 20)) = 5.0
        _HazardType ("Hazard Type", Range(0, 3)) = 0 // 0=lava, 1=ice, 2=wind
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float _DistortionAmount;
        float _DistortionSpeed;
        float4 _DistortionScale;
        float _WaveFrequency;
        float _HazardType;

        struct Input
        {
            float2 uv_MainTex;
        };

        // Vertex displacement function
        void vert(inout appdata_full v)
        {
            float time = _Time.y * _DistortionSpeed;
            
            // Different distortion patterns based on hazard type
            float3 distortion = float3(0, 0, 0);
            
            if (_HazardType < 1) // Lava - bubbling effect
            {
                distortion.x = sin(time + v.texcoord.x * _WaveFrequency) * _DistortionAmount;
                distortion.z = cos(time + v.texcoord.y * _WaveFrequency) * _DistortionAmount;
                distortion.y = sin(time * 2 + v.texcoord.x * _WaveFrequency * 2) * _DistortionAmount * 0.5f;
            }
            else if (_HazardType < 2) // Ice - crystalline ripples
            {
                distortion.x = sin(time * 0.5f + v.texcoord.x * _WaveFrequency * 2) * _DistortionAmount * 0.7f;
                distortion.z = cos(time * 0.5f + v.texcoord.y * _WaveFrequency * 2) * _DistortionAmount * 0.7f;
                distortion.y = sin(time + v.texcoord.x * _WaveFrequency + v.texcoord.y * _WaveFrequency) * _DistortionAmount * 0.3f;
            }
            else // Wind - flowing waves
            {
                distortion.x = sin(time + v.texcoord.y * _WaveFrequency) * _DistortionAmount * 1.2f;
                distortion.z = cos(time * 1.5f + v.texcoord.x * _WaveFrequency) * _DistortionAmount * 0.8f;
                distortion.y = sin(time * 0.8f + v.texcoord.x * _WaveFrequency * 0.5f) * _DistortionAmount * 0.4f;
            }
            
            // Apply distortion with scale
            v.vertex.xyz += distortion * _DistortionScale.xyz;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            // Add subtle color variation based on hazard type
            if (_HazardType < 1) // Lava - red tint
            {
                c = lerp(c, fixed4(1, 0.3f, 0, c.a), 0.3f);
            }
            else if (_HazardType < 2) // Ice - blue tint
            {
                c = lerp(c, fixed4(0.3f, 0.7f, 1, c.a), 0.3f);
            }
            else // Wind - white/gray tint
            {
                c = lerp(c, fixed4(0.8f, 0.8f, 0.9f, c.a), 0.2f);
            }
            
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
