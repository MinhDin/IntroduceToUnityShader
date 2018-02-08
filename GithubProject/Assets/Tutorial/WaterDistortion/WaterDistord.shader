Shader "Unlit/WaterDistord"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DuDvMap ("TexDuDv", 2D) = "white" {}
		_DuDvScale("DuDv", Float) = 0.1
		_TimeScale("TimeScale", Float) = 0.3
		_DistordStrength("DistordStrength", Float) = 0.005

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		GrabPass
        {
            "_BehindWater"
        }

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
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float4 grabPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _DuDvMap;
			float4 _MainTex_ST;
			float _TimeScale;
			float _DuDvScale;
			float _DistordStrength;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				o.grabPos = ComputeGrabScreenPos(o.vertex);
				return o;
			}
			
			sampler2D _BehindWater;
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 nor = tex2D(_DuDvMap, float2(i.uv.x + _Time.x * _TimeScale, i.uv.y + _Time.x * _TimeScale) * _DuDvScale);
				nor.rg = nor.rg * _DistordStrength;
				nor.b = 0;
				nor.a = 0;
				fixed3 col = tex2Dproj(_BehindWater, i.grabPos + nor).rgb * (1 - i.color.a)  + i.color.rgb * i.color.a;
				return fixed4(col, 1.0);
			}
			ENDCG
		}
	}
}
