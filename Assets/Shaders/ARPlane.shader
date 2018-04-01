Shader "Custom/ARPlane"
{
	Properties {
		_MainTex("Texture", 2D) = "white" {}
		_TintColor("Tint Color", Color) = (0,0,1,0)
	}
    SubShader {
	    	Pass {
	    		// Render the Occlusion shader before all
				// opaque geometry to prime the depth buffer.
				Tags { "Queue"="Geometry" }

				ZWrite On
				ZTest LEqual
				Blend SrcAlpha OneMinusSrcAlpha

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
					float4 position : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _TintColor;

				v2f vert (appdata input)
				{
					v2f output;

					output.position = UnityObjectToClipPos(input.vertex);
					output.uv = TRANSFORM_TEX(input.uv, _MainTex);
					return output;
				}

				fixed4 frag (v2f input) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, input.uv);
					col.r *= _TintColor.r;
					col.g *= _TintColor.g;
					col.b *= _TintColor.b;
					return col;
				}
				ENDCG
	    	}
	}
}
