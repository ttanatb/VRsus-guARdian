// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Platform"
{
	Properties
	{
		_TopTex ("Top Texture", 2D) = "white" {}
		_BotTex("Bottom Texture",  2D) = "white" {}
	}
	SubShader
	{
		
		Lighting Off
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 pos : POSITION;
				//float4 color : COLOR0;
				float4 fragPos : COLOR1;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : POSITION;
				float4 color : COLOR0; // first colour 
				float4 fragPos : COLOR1; // 2nd colour
				float2  uv : TEXCOORD0; // first UV coordinate
			};

			sampler2D _TopTex;
			float4 _TopTex_ST;
			sampler2D _BotTex;
			float4 _BotTex_ST;		
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.fragPos = o.pos;
				o.uv = TRANSFORM_TEX(v.normal, _TopTex);
				//o.color = float4 (1.0, 1.0, 1.0, 1);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 q = i.uv.xy / float2(1,1);
				float3 originalCol = tex2D(_TopTex, float2(q.x, q.y)).xyz;
				float3 col = tex2D(_BotTex, float2(i.uv.x, i.uv.y)).xyz;
				float comp = smoothstep(0.1, 0.9, sin(0.5));
				col = lerp(col, originalCol, clamp(-2.0 + 2.0*q.y + 3.0*comp, 0.0, 1.0));
				return float4(col, 1);

				//fixed4 topTex = tex2D(_TopTex, i.uv);
				//fixed4 botTex = tex2D(_BotTex, i.uv);
				//fixed4 col = lerp(topTex, botTex, i.vertex);
				//return col;
			}
			ENDCG
		}
	}
}
