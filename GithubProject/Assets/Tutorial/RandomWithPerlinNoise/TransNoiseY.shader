Shader "Unlit/TransNoiseY"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Texture", 2D) = "white" {}
		_NoiseStrength("Noise Strength", Float) = 0.1
		_NoiseScale("Noise Scale", Float) = 0.2
		_ScrollStep("Scroll Step", Float) = 1
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
			};

			sampler2D _MainTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _NoiseTex;
			float _NoiseScale;
			float _NoiseStrength;
			float _ScrollStep;
			fixed4 frag (v2f i) : SV_Target
			{
				i.uv.y += tex2D(_NoiseTex, (i.uv.x * _ScrollStep + _NoiseScale * _Time.y) * 0.2) * _NoiseStrength;// * sqrt(1 - i.uv.x);				
				
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
