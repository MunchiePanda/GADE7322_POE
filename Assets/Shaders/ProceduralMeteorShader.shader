
Shader "Custom/ProceduralMeteorShader"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (1,0.5,0,1)
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 2.0
        _DisplacementAmount ("Displacement Amount", Range(0, 1)) = 0.1
        _DisplacementFrequency ("Displacement Frequency", Range(1, 20)) = 5.0
        _DisplacementSeed ("Displacement Seed", Float) = 0.0
        _Roughness ("Roughness", Range(0, 1)) = 0.8
        _Smoothness ("Smoothness", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        // Main Forward Lit Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Vertex input structure
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            // Data passed from vertex to fragment
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            // Material properties
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _EmissionColor;
                float _EmissionIntensity;
                float _DisplacementAmount;
                float _DisplacementFrequency;
                float _DisplacementSeed;
                float _Roughness;
                float _Smoothness;
            CBUFFER_END

            // Hash function for noise
            float3 hash3(float3 p)
            {
                p = float3(dot(p, float3(127.1, 311.7, 74.7)),
                          dot(p, float3(269.5, 183.3, 246.1)),
                          dot(p, float3(113.5, 271.9, 124.6)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            // 3D Perlin-like noise function
            float noise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                float3 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(lerp(dot(hash3(i + float3(0, 0, 0)), f - float3(0, 0, 0)),
                                     dot(hash3(i + float3(1, 0, 0)), f - float3(1, 0, 0)), u.x),
                               lerp(dot(hash3(i + float3(0, 1, 0)), f - float3(0, 1, 0)),
                                     dot(hash3(i + float3(1, 1, 0)), f - float3(1, 1, 0)), u.x), u.y),
                           lerp(lerp(dot(hash3(i + float3(0, 0, 1)), f - float3(0, 0, 1)),
                                     dot(hash3(i + float3(1, 0, 1)), f - float3(1, 0, 1)), u.x),
                               lerp(dot(hash3(i + float3(0, 1, 1)), f - float3(0, 1, 1)),
                                     dot(hash3(i + float3(1, 1, 1)), f - float3(1, 1, 1)), u.x), u.y), u.z);
            }

            // Vertex shader: applies procedural displacement
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float3 posOS = input.positionOS.xyz;
                // Offset for noise seed
                float3 seedOffset = float3(_DisplacementSeed, _DisplacementSeed * 1.3, _DisplacementSeed * 0.7);
                
                // Multi-octave noise for more detail
                float n1 = noise((posOS + seedOffset) * _DisplacementFrequency);
                float n2 = noise((posOS + seedOffset * 2.0) * _DisplacementFrequency * 2.0);
                float n3 = noise((posOS + seedOffset * 3.0) * _DisplacementFrequency * 0.5);
                
                // Weighted sum of noise octaves
                float displacement = (n1 * 0.6 + n2 * 0.3 + n3 * 0.1) * _DisplacementAmount;
                
                // Displace along normal
                posOS += input.normalOS * displacement;
                
                // Transform to world and clip space
                VertexPositionInputs vertexInput = GetVertexPositionInputs(posOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = input.uv;
                
                return output;
            }

            // Fragment shader: sets up PBR surface and emission
            half4 frag(Varyings input) : SV_Target
            {
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalize(input.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = _Color.rgb;
                surfaceData.alpha = _Color.a;
                surfaceData.emission = _EmissionColor.rgb * _EmissionIntensity;
                surfaceData.metallic = 0.0;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.occlusion = 1.0;

                // Standard URP PBR lighting
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                return color;
            }
            ENDHLSL
        }

        // Shadow caster pass with matching displacement
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Vertex input for shadow pass
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            // Output for shadow pass
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            // Only displacement properties needed for shadow pass
            CBUFFER_START(UnityPerMaterial)
                float _DisplacementAmount;
                float _DisplacementFrequency;
                float _DisplacementSeed;
            CBUFFER_END

            // Hash function for noise (same as main pass)
            float3 hash3(float3 p)
            {
                p = float3(dot(p, float3(127.1, 311.7, 74.7)),
                          dot(p, float3(269.5, 183.3, 246.1)),
                          dot(p, float3(113.5, 271.9, 124.6)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            // 3D Perlin-like noise function (same as main pass)
            float noise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                float3 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(lerp(dot(hash3(i + float3(0, 0, 0)), f - float3(0, 0, 0)),
                                     dot(hash3(i + float3(1, 0, 0)), f - float3(1, 0, 0)), u.x),
                               lerp(dot(hash3(i + float3(0, 1, 0)), f - float3(0, 1, 0)),
                                     dot(hash3(i + float3(1, 1, 0)), f - float3(1, 1, 0)), u.x), u.y),
                           lerp(lerp(dot(hash3(i + float3(0, 0, 1)), f - float3(0, 0, 1)),
                                     dot(hash3(i + float3(1, 0, 1)), f - float3(1, 0, 1)), u.x),
                               lerp(dot(hash3(i + float3(0, 1, 1)), f - float3(0, 1, 1)),
                                     dot(hash3(i + float3(1, 1, 1)), f - float3(1, 1, 1)), u.x), u.y), u.z);
            }

            // Vertex shader for shadow pass: applies same displacement as main pass
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                
                float3 posOS = input.positionOS.xyz;
                float3 seedOffset = float3(_DisplacementSeed, _DisplacementSeed * 1.3, _DisplacementSeed * 0.7);
                
                float n1 = noise((posOS + seedOffset) * _DisplacementFrequency);
                float n2 = noise((posOS + seedOffset * 2.0) * _DisplacementFrequency * 2.0);
                float n3 = noise((posOS + seedOffset * 3.0) * _DisplacementFrequency * 0.5);
                
                float displacement = (n1 * 0.6 + n2 * 0.3 + n3 * 0.1) * _DisplacementAmount;
                posOS += input.normalOS * displacement;
                
                output.positionCS = TransformObjectToHClip(posOS);
                return output;
            }

            // Fragment shader for shadow pass: returns 0 (no color output)
            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
