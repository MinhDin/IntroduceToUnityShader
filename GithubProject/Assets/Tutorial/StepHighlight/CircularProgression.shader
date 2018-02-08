Shader "Unlit/CircularProgression"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Inner("InnerRadius", Float) = 0.75
		_Outer("OuterRadius", Float) = 0.9
		_Percent("Percent", Float) = 0.75
		[HideInInspector]
		_Head("Head", Vector) = (0, 0, 0, 0)
		_PercentFixTop("PercentFixTop", Float) = 0.02
		_PercentBlur("PercentBlur", Float) = 0.3
	}
	SubShader
	{
		Tags 
		{
			"RenderType"="Transparent" 
			"Queue" = "Transparent"
		}

		Cull Off
		Lighting Off
		ZWrite Off
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
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			float _Inner;
			float _Outer;
			float _Percent;
			float4 _Head;
			float _PercentFixTop;
			float _PercentBlur;

			fixed4 frag (v2f i) : SV_Target
			{				
				float2 root = float2(0, 1);
				float2 uv = float2((i.uv.x - 0.5) * 2, (i.uv.y - 0.5) * 2);
				float radius = length(uv);
				float side = abs(uv.x) / uv.x;//left -1; right 1
				float angle = acos((dot(uv, root) * side) / length(uv)) / 3.14159 / 2;
				//float percent = ((-abs(uv.x) / uv.x) + 1) * 0.5 + (dot(uv, root) + 1) * 0.5;
				//float percent = (side + 1) * 0.5 * 0.5 + (dot(uv, root) * side + 1) * 0.5 * 0.5 ;
				float percent = (side * (-1) + 1) * 0.5 * 0.5 + angle;
				
				//color
				int a = percent / _Percent;
				float2 uvTex;

				float percentFixTop = percent + _PercentFixTop;
				uvTex.x = (percentFixTop - floor(percentFixTop)) / (_Percent + _PercentFixTop);
				uvTex.y = (radius - _Inner) / (_Outer - _Inner);

				fixed4 col = tex2D(_MainTex, uvTex);

				//mask check
				int innerCheck = step(_Inner, radius);// floor((radius - _Inner) + 1);
				int outerCheck = floor((_Outer - radius) + 1);
				int angleCheck = floor((_Percent - percent) + 1);
				//int extraHead1 = floor((((_Outer - _Inner) / 2) - length(uv - _Head.xy)) + 1);
				//extraHead1 = clamp(extraHead1, 0, 1);
				//int extraHead2 = floor((((_Outer - _Inner) / 2) - length(uv - _Head.zw)) + 1);
				//extraHead2 = clamp(extraHead2, 0, 1);

				//int finalCheck = innerCheck * outerCheck * angleCheck + extraHead1 + extraHead2;
				//finalCheck = clamp(finalCheck, 0, 1);

				//blur
				float distanceHalf = (_Outer - _Inner) / 2;
				float distanceBlur = distanceHalf * (1 - _PercentBlur);
				float distanceCore = abs(radius - (distanceHalf + _Inner));
				float distanceExtra = min(length(uv - _Head.xy), length(uv - _Head.zw));
				int isCore = innerCheck * outerCheck * angleCheck;
				float distance = max(distanceCore * isCore, distanceExtra * (1 - isCore));
				
				float alphaBlur = (distance - distanceBlur) / (distanceHalf - distanceBlur);
				alphaBlur = 1 - clamp(alphaBlur, 0, 1);

				int isCoreColor = floor(alphaBlur);

				return fixed4(col.xyz * isCoreColor + (fixed3(0, 0, 1) + col.xyz * pow(alphaBlur, 6)) * (1 - isCoreColor), alphaBlur);
			}
			ENDCG
		}
	}
}
