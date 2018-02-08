Shader "Unlit/MCVortex"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		//x,y : position of vortex. z . radius of the vertex. w (-1 or 1) side of the vortex
		_VortexPos ("Vortex Pos", Vector) = (1, 1, 1, 1)
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
			float4 _VortexPos;

			inline float4 myMatrixMul(float4x4 mat, float4 v) {
				return float4(
					mat[0][0] * v.x + mat[0][1] * v.y + mat[0][2] * v.z + mat[0][3] * v.w,
					mat[1][0] * v.x + mat[1][1] * v.y + mat[1][2] * v.z + mat[1][3] * v.w,
					mat[2][0] * v.x + mat[2][1] * v.y + mat[2][2] * v.z + mat[2][3] * v.w,
					mat[3][0] * v.x + mat[3][1] * v.y + mat[3][2] * v.z + mat[3][3] * v.w
				);
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = myMatrixMul(UNITY_MATRIX_M, v.vertex);// mul(UNITY_MATRIX_M, v.vertex);i8190 has problem with this
				o.uv = v.uv;

				o.vertex.y = _VortexPos.y + _VortexPos.w * max((o.vertex.y - _VortexPos.y) * _VortexPos.w, 0);
				float2 dir = o.vertex.xy - _VortexPos.xy;				
				float percent = min((dot(dir, dir) / (_VortexPos.z * _VortexPos.z)), 1);
				o.vertex.xy = _VortexPos.xy + dir * percent;

				o.vertex = myMatrixMul(UNITY_MATRIX_VP, o.vertex);// mul(UNITY_MATRIX_VP, o.vertex);i8190 has problem with this
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
