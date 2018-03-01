Shader "Custom/CelShader_Surface"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_SpecCol("Specular Color", Color) = (1,1,1,1)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			//PreCompiling functions
			#pragma surface surf CelShading
			#pragma target 3.0 //Supporting DX9

			fixed4 _SpecCol;

			half4 LightingCelShading(SurfaceOutput s, half3 lightDir, half atten)
			{
				half newDot = dot(s.Normal, lightDir);
				
				//Determine if there's a shadow or not (Polarize)
				if (newDot <= 0.3) newDot = 0;
				else newDot = 1;

				//Setup the new color
				half4 color;
				color.rgb = s.Albedo * _LightColor0.rgb * (newDot * atten * 2);
				color.a = s.Alpha * _SpecColor.a;
				return color;
			}

			sampler2D _MainTex;
			fixed4 _Color;

			struct Input
			{
				//UV's of the main texture
				float2 uv_MainTex;
			};

			void surf(Input IN, inout SurfaceOutput o)
			{
				fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = color.rgb;
				o.Alpha = color.a;
			}
			ENDCG
		}
		FallBack "Diffuse" //Just incase none of the subshaders wornm
}
