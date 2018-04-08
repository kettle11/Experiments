Shader "WaterFlowStep"
{
	Properties
	{
		_Water ("Water", 2D) = "white" {}
		_FlowMap ("FlowMap", 2D) = "white" {}
        _PipeLength("Distance between two pixels", float) = 1
        _TimeStep("Time step", float) = 1
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
            sampler2D _FlowMap;

            float4 _Water_TexelSize;
            float4 _Water_ST;

            float _PipeLength;
            float _TimeStep;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _Water);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                half s = _Water_TexelSize.x;

                fixed2 uv = i.uv;// + fixed2(.5, .5) * s;

				fixed4 waterHere = tex2D(_Water, uv);
                fixed4 flowHere = tex2D(_FlowMap, uv);
                
                float deltaTime = _TimeStep;

                // Outflows
                float f1 = -flowHere.r; // Left
                float f2 = -flowHere.g; // Up
                float f3 = -flowHere.b; // Right
                float f4 = -flowHere.a; // Down

                // In flows, these are positive
                float f5 = tex2D(_FlowMap, uv + fixed2(-s,  0)).b; // Left neighbor
                float f6 = tex2D(_FlowMap, uv + fixed2( 0, -s)).a; // Upper neighbor
                float f7 = tex2D(_FlowMap, uv + fixed2( s,  0)).r; // Right
                float f8 = tex2D(_FlowMap, uv + fixed2( 0,  s)).g; // Lower

                if (uv.x - s <= 0)
				{
					f1 = 0; // left
                    f5 = 0;
				}

				if (uv.x + s >= 1)
				{
					f3 = 0; // Right
                    f7 = 0;
				}

				if (uv.y + s >= 1)
				{
					f2 = 0; // Up
                    f6 = 0;
				}

				if (uv.y - s <= 0)
				{
					f4 = 0;  //Down
                    f8 = 0;
				}
                
                //waterHere.a = waterHere.a + (deltaTime / (_PipeLength * _PipeLength)) * (f1 + f2 + f3 + f4 + (f5 + f6 + f7 + f8));
                waterHere.a = max(waterHere.a + (deltaTime * (f1 + f2 + f3 + f4 + (f5 + f6 + f7 + f8))), 0);
				return waterHere;
			}

			ENDCG
		}
	}
}
