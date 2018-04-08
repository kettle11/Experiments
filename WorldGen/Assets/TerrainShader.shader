Shader "Custom/TerrainShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_HeightMap("Height map", 2D) = "white" {}
		_Water("Water map", 2D) = "white" {}
		_Flow("Flow map", 2D) = "white" {}
		_HeightScale("Height scale", float) = 30
		_WaterHeightScale("Height", float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			sampler2D _HeightMap;
			sampler2D _Water;
			sampler2D _Flow;

			float4 _HeightMap_ST;
			float _HeightScale;
			float _WaterHeightScale;

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

			v2f vert (appdata v) {
				v2f o;
				float height = tex2Dlod(_HeightMap, float4(v.uv, 0,0)).r;
				float waterHeight = tex2Dlod(_Water, float4(v.uv,0,0)).a;
				o.vertex = v.vertex;

				o.vertex = UnityObjectToClipPos(v.vertex);

				o.vertex.xyz += fixed4(0,1,0,0) * (height * _HeightScale + waterHeight * _WaterHeightScale * 100.0);

				o.uv = TRANSFORM_TEX(v.uv, _HeightMap);

				o.uv = v.uv;
				return o;
			}
			
			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			fixed4 frag (v2f i) : SV_Target
			{
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D (_HeightMap, i.uv) * _Color;
				fixed4 w = tex2D (_Water, i.uv);
				fixed4 flow = tex2D(_Flow, i.uv);

				c = fixed4(c.r * 5, c.r * 5, c.r * 5, 1);

				//if (w.a > .01)
				{
					c.b = w.a * 60.0;//* w.a + .9;

					//float amount = (flow.a + flow.r + flow.g + flow.b) * .6f;
					//c.r += amount;
					//c.g += amount;
				}


				
				if (flow.a > 0 || flow.r > 0 || flow.g > 0 || flow.b > 0)
				{
					//c = flow;
				}
				
				//c = flow * 5;
				return c;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
