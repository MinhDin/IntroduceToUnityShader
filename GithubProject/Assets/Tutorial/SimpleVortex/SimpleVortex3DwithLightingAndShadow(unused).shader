Shader "Unlit/SimplePortalWithLightAndShadow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc" // for _LightColor0
		/*	struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};*/

			struct v2f
			{
				float2 uv : TEXCOORD0;
				fixed4 diff : COLOR0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert(appdata_base v)
			{
				v2f o;
				//o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex = mul(UNITY_MATRIX_M, v.vertex);
				o.uv = v.texcoord;

				//portal
				float centerVortexY = 3;
				float vortexW = 3;
				//left vortex
				float centerVortexX = -12;
				float startVortexX = centerVortexX + vortexW;

				int flag = clamp(o.vertex.x - startVortexX, 0, 1);
				flag = floor(flag + 0.99);

				float distanceToCenter = max(o.vertex.x - centerVortexX, 0);
				float percent = pow(min((distanceToCenter / vortexW), 1), 1.5);
				distanceToCenter = distanceToCenter * percent;

				//right vortex
				float centerVortexX2 = 12;
				float startVortexX2 = centerVortexX2 - vortexW;

				int flag2 = clamp(startVortexX2 - o.vertex.x, 0, 1);
				flag2 = floor(flag2 + 0.99);

				float distanceToCenter2 = max(centerVortexX2 - o.vertex.x, 0);
				float percent2 = pow(min((distanceToCenter2 / vortexW), 1), 1.5);
				distanceToCenter2 = distanceToCenter2 * percent2;

				o.vertex.x = o.vertex.x * flag * flag2 + (centerVortexX + distanceToCenter) * (1 - flag) + (centerVortexX2 - distanceToCenter2) * (1 - flag2);
				o.vertex.y = o.vertex.y * flag * flag2 + (centerVortexY + (o.vertex.y - centerVortexY) * percent) * (1 - flag) + (centerVortexY + (o.vertex.y - centerVortexY) * percent2) * (1 - flag2);

				//done
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
				o.diff.rgb += ShadeSH9(half4(worldNormal, 1));

				o.vertex = mul(UNITY_MATRIX_VP, o.vertex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= i.diff;
				return col;
			}
			ENDCG
		}

		Pass
			{
				Tags{ "LightMode" = "ShadowCaster" }

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
				#include "UnityCG.cginc"

				struct v2f {
					V2F_SHADOW_CASTER;
				};

				v2f vert(appdata_base v)
				{
					v2f o;
					//o.vertex = UnityObjectToClipPos(v.vertex);
					o.pos = mul(UNITY_MATRIX_M, v.vertex);
					//portal
					float centerVortexY = 3;
					float vortexW = 3;
					//left vortex
					float centerVortexX = -12;
					float startVortexX = centerVortexX + vortexW;

					int flag = clamp(o.pos.x - startVortexX, 0, 1);
					flag = floor(flag + 0.99);

					float distanceToCenter = max(o.pos.x - centerVortexX, 0);
					float percent = pow(min((distanceToCenter / vortexW), 1), 1.5);
					distanceToCenter = distanceToCenter * percent;

					//right vortex
					float centerVortexX2 = 12;
					float startVortexX2 = centerVortexX2 - vortexW;

					int flag2 = clamp(startVortexX2 - o.pos.x, 0, 1);
					flag2 = floor(flag2 + 0.99);

					float distanceToCenter2 = max(centerVortexX2 - o.pos.x, 0);
					float percent2 = pow(min((distanceToCenter2 / vortexW), 1), 1.5);
					distanceToCenter2 = distanceToCenter2 * percent2;

					o.pos.x = o.pos.x * flag * flag2 + (centerVortexX + distanceToCenter) * (1 - flag) + (centerVortexX2 - distanceToCenter2) * (1 - flag2);
					o.pos.y = o.pos.y * flag * flag2 + (centerVortexY + (o.pos.y - centerVortexY) * percent) * (1 - flag) + (centerVortexY + (o.pos.y - centerVortexY) * percent2) * (1 - flag2);

					//done
					o.pos = mul(UNITY_MATRIX_VP, o.pos);

					//TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					SHADOW_CASTER_FRAGMENT(i)
				}
				ENDCG
			}
	}
}
