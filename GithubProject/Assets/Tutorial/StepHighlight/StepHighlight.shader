Shader "Unlit/StepHighlight"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HighlightBegin ("Hightlight Begin", Float) = 0.5
		_HighlightLength ("Hightlight Length", Float) = 0.2		
		_ScaleStrength ("Scale Strength", Float) = 0.6
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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
				//float2 highlight : TEXCOORD1;
				float2 y : TEXCOORD2;
			};

			sampler2D _MainTex;
			float _HighlightBegin;
			float _HighlightLength;
			float _ScaleStrength;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				//highlight
				//float posRescale = (o.vertex.y + 1) / 2;
				//fixed flagUnder = step(_HighlightBegin, posRescale);
				//fixed flagOver = step(posRescale, _HighlightBegin + _HighlightLength);

				//o.highlight.x = flagUnder * flagOver * (1 - abs(posRescale - (_HighlightBegin + _HighlightLength / 2)) / (_HighlightLength / 2));
				//o.highlight.y = 0;
				
				o.y.x = o.vertex.y;
				o.y.y = 1;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				
				float highlight = 0;
				float posRescale = (i.y.x + 1) / 2;
				fixed flagUnder = step(_HighlightBegin, posRescale);
				fixed flagOver = step(posRescale, _HighlightBegin + _HighlightLength);
				highlight = flagUnder * flagOver * (1 - abs(posRescale - (_HighlightBegin + _HighlightLength / 2)) / (_HighlightLength / 2));
				col.rgb =  col.rgb * (1 + _ScaleStrength * highlight);

				//col.rgb =  col.rgb * (1 + _ScaleStrength * i.highlight.x);
				return col;
			}
			ENDCG
		}
	}

	Fallback "Unlit/TransZTest"
}
