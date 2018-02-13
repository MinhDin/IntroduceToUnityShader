Shader "Custom/EnvironmentOverlay"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_TransitionTex("Transition Texture", 2D) = "white" {}
		_GlowTex("Glow Texture", 2D) = "black" {}

		_NormalizedTime("Normalized Time", Range (0.0, 1.0)) = 0.0
		_DisplayInfo("Display Info", Vector) = (8.5, 13.6, 0.0, 0.0)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
		}

		Cull Off
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _TransitionTex;
			sampler2D _GlowTex;

			float _NormalizedTime;

			float4 _DisplayInfo;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 tc = i.uv;

				float2 halfSize = _DisplayInfo.xy;
				float2 center = _DisplayInfo.zw;

				float2 p = (-1.0 + 2.0 * i.uv) * halfSize - center;

				float len = length(p);
				float2 uv = i.uv + (p / len) * cos(len * 1.0 - _NormalizedTime * 6.0) * 0.003 * halfSize * min(_NormalizedTime * 7.5, 1.0);

				float3 col = tex2D(_MainTex, uv).xyz;
				
				float transit = tex2D(_TransitionTex, uv).x;
				
				float alpha = clamp(len * 0.5 - _NormalizedTime * 25.0 + transit * 2.5, 0.0, 1.0);

				col += alpha * tex2D(_GlowTex, fixed2(alpha, 0.0)).xyz;
				return fixed4(col, alpha);
			}

			ENDCG
		}
	}
}
