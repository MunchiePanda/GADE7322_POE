// Shader: Custom/WaterVertexDisplace
// Purpose: Renders a water surface with animated vertex displacement to simulate waves.
// Usage: Apply to a mesh to create a dynamic, semi-transparent water effect in Unity.

Shader "Custom/WaterVertexDisplace"
{
	Properties
	{
		// The base color and alpha (transparency) of the water
		_Color ("Color", Color) = (0.2,0.5,0.8,0.6)
		// Controls the height of the waves
		_Amplitude ("Amplitude", Float) = 0.05
		// Controls the frequency (spacing) of the waves
		_Frequency ("Frequency", Float) = 1.5
		// Controls the speed of wave animation
		_Speed ("Speed", Float) = 1.0
		// Used for per-instance offsetting (for instanced rendering)
		_InstanceOffset ("InstanceOffset", Vector) = (0,0,0,0)
	}
	SubShader
	{
		// Render as transparent, with alpha blending, and no depth writing
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Back

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			// Shader property variables
			fixed4 _Color;
			float _Amplitude, _Frequency, _Speed;
			float4 _InstanceOffset;

			// Vertex input structure
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			// Vertex-to-fragment structure
			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 col : COLOR0;
			};

			// Vertex shader: displaces vertices vertically to create wave motion
			v2f vert (appdata v)
			{
				v2f o;
				// Calculate time-based offset for animation, with per-instance variation
				float t = _Time.y * _Speed + dot(_InstanceOffset.xy, float2(3.11,5.73));
				// Calculate wave displacement using sine and cosine for more natural movement
				float wave = sin((v.vertex.x + _InstanceOffset.z) * _Frequency + t) * _Amplitude;
				wave += cos((v.vertex.z + _InstanceOffset.w) * _Frequency * 0.8 + t * 1.2) * (_Amplitude * 0.6);
				float4 displaced = v.vertex;
				displaced.y += wave; // Move vertex up/down
				o.pos = UnityObjectToClipPos(displaced); // Transform to clip space
				o.col = _Color; // Pass color to fragment shader
				return o;
			}

			// Fragment shader: outputs the color (with transparency)
			fixed4 frag (v2f i) : SV_Target
			{
				return i.col;
			}
			ENDCG
		}
	}
}
