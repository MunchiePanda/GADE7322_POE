Shader "Custom/WaterVertexDisplace"
{
	Properties
	{
		_Color ("Color", Color) = (0.2,0.5,0.8,0.6)
		_Amplitude ("Amplitude", Float) = 0.05
		_Frequency ("Frequency", Float) = 1.5
		_Speed ("Speed", Float) = 1.0
		_InstanceOffset ("InstanceOffset", Vector) = (0,0,0,0)
	}
	SubShader
	{
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

			fixed4 _Color;
			float _Amplitude, _Frequency, _Speed;
			float4 _InstanceOffset;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 col : COLOR0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				float t = _Time.y * _Speed + dot(_InstanceOffset.xy, float2(3.11,5.73));
				float wave = sin((v.vertex.x + _InstanceOffset.z) * _Frequency + t) * _Amplitude;
				wave += cos((v.vertex.z + _InstanceOffset.w) * _Frequency * 0.8 + t * 1.2) * (_Amplitude * 0.6);
				float4 displaced = v.vertex;
				displaced.y += wave;
				o.pos = UnityObjectToClipPos(displaced);
				o.col = _Color;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return i.col;
			}
			ENDCG
		}
	}
}



