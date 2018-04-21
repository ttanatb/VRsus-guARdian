Shader "Unlit/Grass"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_GradTex("Gradient", 2D) = "white" {}

		_WaveSpeed("Wave Speed", float) = 1.0
		_WaveAmp("Wave Amp", float) = 1.0

	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 200
		Cull Off
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
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _GradTex;
			float4 _MainTex_ST;
			float _WaveSpeed;
			float _WaveAmp;
			
			v2f vert (appdata v)
			{
				v2f o;
				v.vertex.x += sin(v.vertex.x * _WaveAmp + _Time.y * _WaveSpeed) * v.color.w;  // 
				v.vertex.y += sin(v.vertex.y * _WaveAmp + _Time.y * _WaveSpeed) * v.color.w;  // 
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
			float alpha = .5;
				clip(col.a-.1);
				return col;
			}
			ENDCG
		}
	}
}
