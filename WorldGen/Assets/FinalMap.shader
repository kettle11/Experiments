Shader "FinalMap"
{
	Properties
	{
		_Water ("Water", 2D) = "white" {}
		_HeightMap ("HeightMap", 2D) = "white" {}
		_FlowMap ("FlowMap", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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

			sampler2D _Water;
            sampler2D _HeightMap;
            float4 _HeightMap_ST;
            float4 _Water_TexelSize;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _HeightMap);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{   
                half s = _Water_TexelSize.x;
			
				fixed4 waterValue = tex2D(_Water, i.uv);
                fixed4 heightMap = tex2D(_HeightMap, i.uv);
                
                heightMap.b = waterValue.a * 100;

                if (waterValue.a > 0)
                {
                    heightMap = fixed4(0,0,waterValue.a * 20,1);
                }

                fixed2 uv = i.uv;


                if (uv.x - s <= 0)
				{
					return fixed4(0,1,0,0);
				}

				if (uv.x + s >= 1)
				{
					return fixed4(0,1,0,0);
				}

				if (uv.y + s >= 1)
				{
					return fixed4(0,1,0,0);
				}

				if (uv.y - s <= 0)
				{
					return fixed4(0,1,0,0);
				}

				return heightMap;
			}

			ENDCG
		}
	}
}
