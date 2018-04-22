Shader "Custom/Platform"
{
	Properties
	{
		_MainTex("Top Texture", 2D) = "white" {}
		_MainTexSide("Side/Bottom Texture", 2D) = "white" {}
		_Normal("Normal/Noise", 2D) = "bump" {}
		_Scale("Top Scale", Range(0,2)) = 1
		_SideScale("Side Scale", Range(0,2)) = 1
		_NoiseScale("Noise Scale", Range(0,4)) = 1
		_TopSpread("TopSpread", Range(0,0.5)) = 0.1
		_EdgeWidth("EdgeWidth", Range(0,0.5)) = 0.5
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200
			CGPROGRAM
			#pragma surface surf CelShading
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

			sampler2D _MainTex, _MainTexSide, _Normal;
			float _Scale, _SideScale, _NoiseScale;
			float  _TopSpread, _EdgeWidth;

			struct Input {
				float2 uv_MainTex: TEXCOORD0;
				float3 worldPos;
				float3 worldNormal;
				float3 viewDir;
			};

			inline float3 sampleTriplanar(sampler2D tex, float3 worldPos, float scale, float3 blendVec) {
				float3 x = tex2D(tex, worldPos.zy * scale);
				float3 y = tex2D(tex, worldPos.zx * scale);
				float3 z = tex2D(tex, worldPos.xy * scale);
		
				float3 sampledTexture = lerp(lerp(y, x, blendVec.x), z, blendVec.z);
				return sampledTexture;
			}

			void surf(Input IN, inout SurfaceOutput o) {
				float3 blendNormal = saturate(pow(IN.worldNormal * 1.4f, 4));
			
				float3 noiseTexture = sampleTriplanar(_Normal, IN.worldPos, _NoiseScale, blendNormal);
				float3 topTexture = sampleTriplanar(_MainTex, IN.worldPos, _Scale, blendNormal);
				float3 sideTexture = sampleTriplanar(_MainTexSide, IN.worldPos, _SideScale, blendNormal);

				float worldNormalDotNoise = dot(IN.worldNormal - noiseTexture, fixed3(0,1,0));
			
				float3 topTextureResult  = step(_TopSpread, worldNormalDotNoise) * topTexture;
				float3 sideTextureResult = step(worldNormalDotNoise, _TopSpread) * sideTexture;

				float3 topTextureEdgeResult = step(_TopSpread, worldNormalDotNoise) *
					step(worldNormalDotNoise, _TopSpread + _EdgeWidth) * -0.15f;

				o.Albedo = (topTextureResult + sideTextureResult + topTextureEdgeResult);// *_Color;
			}

		ENDCG
		}

			Fallback "Diffuse"
}
