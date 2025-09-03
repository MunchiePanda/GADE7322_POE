Shader "Custom/VertexDisplacement"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _DisplacementAmount ("Displacement Amount", Range(0, 0.1)) = 0.02
        _DisplacementSpeed ("Displacement Speed", Range(0, 10)) = 1.0
        _DisplacementScale ("Displacement Scale", Vector) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        float4 _Color;
        float _DisplacementAmount;
        float _DisplacementSpeed;
        float4 _DisplacementScale;

        struct Input
        {
            float2 uv_MainTex;
        };

        // Vertex displacement in vertex shader
        void vert(inout appdata_full v)
        {
            float time = _Time.y * _DisplacementSpeed;
            float displacement = sin(time + v.texcoord.x * 10.0) * _DisplacementAmount;
            v.vertex.xyz += v.normal * displacement * _DisplacementScale;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
