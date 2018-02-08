Shader "Unlit/HeatDistort"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Displacement ("_Displacement", 2D) = "black" {}
		_BGTex ("BG Tex", 2D) = "white" {}
		_DisplacementScale("Displacement Scale", Float) = 1.6
		_FireTongueScale("Fire Tongue Scale" , Float) = 0.2
		_BGScale("Background Scale", Float) = 0.7
		_SpeedScale("Speed Scale", Float) = 0.0185
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque"}
		// /Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZTest LEqual
		ZWrite On
		
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
				fixed4 color : COLOR0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR0;
				float2 uv2 : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _BGTex_ST;
			sampler2D _BGTex;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				o.uv2 = TRANSFORM_TEX(v.uv, _BGTex); 
				return o;
			}
			
			sampler2D _Displacement;
			float _DisplacementScale;
			float _FireTongueScale;
			float _BGScale;
			float _SpeedScale;

			fixed4 frag (v2f i) : SV_Target
			{	
				float2 displace = i.uv2;
				displace.y = i.uv2.y - _Time.y * _SpeedScale;
				//displace.x = ((displace.x - 0.5) * (1 - 1 * i.uv.y)) + 0.5;
				fixed4 dis = tex2D(_Displacement, displace * _DisplacementScale);
				dis.x -= 0.5;
				dis.y -= 0.5;
				
				displace = dis.xy  * (1 - i.uv.y) * (0.5 - abs(i.uv.x - 0.5));
				fixed4 col = tex2D(_MainTex,i.uv + displace * _FireTongueScale) * i.color;
				fixed4 bg = tex2D(_BGTex, i.uv2 + displace * _BGScale);

				col.rgb = lerp(bg.rgb, col.rgb, col.a);
				col.a = 1;
				return col;
			}
			ENDCG
		}
	}
}
