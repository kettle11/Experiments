Shader "Unlit/Diffusion"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HeightMap ("HeightMap", 2D) = "white" {}
		_FlowMap ("FlowMap", 2D) = "white" {}
		_Pixels ("Pixels", float) = 256
		_FluidDensity ("Fluid Density", float) = 1
		_Gravity ("Gravity Acceleration", float) = 9.81
		_PipeLength("Distance between two pixels", float) = 1
		_PipeCrossSectionArea("Cross sectional pipe area", float) = 1
		_TimeStep("Time step", float) = 1
		_HeightScale("Height scale", float) = 30
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

			sampler2D _MainTex;
			sampler2D _FlowMap;
			sampler2D _HeightMap;

			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float _Pixels;
			float _FluidDensity;
			float _Gravity;
			float _PipeLength;
			float _PipeCrossSectionArea;
			float _TimeStep;
			float _HeightScale;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Cell center
				/*
				fixed2 uv = floor(((i.uv * _Pixels) / _Pixels) + 0.5);
			
				// Neighbour cells
				
				float cl = tex2D(_MainTex, i.uv + fixed2(-s, 0)).b;	// Centre Left
				float tc = tex2D(_MainTex, i.uv + fixed2(0, -s)).b;// Top Center
				float cc = tex2D(_MainTex, i.uv + fixed2(0, 0)).b;	// Centre Center
				float bc = tex2D(_MainTex, i.uv + fixed2(0, +s)).b;	// Bottom Center
				float cr = tex2D(_MainTex, i.uv + fixed2(+s, 0)).b;	// Centre Right
				*/
			
				half s = _MainTex_TexelSize.x;
			
				fixed2 uv = i.uv;// + fixed2(.5, .5) * s;

				float waterHere = tex2D(_MainTex, uv).a;
				float heightHere = waterHere + tex2D(_HeightMap, uv).r;

				// Water + terrain height at neighbors
				float n1 = tex2D(_MainTex, uv + fixed2(-s,  0)).a + tex2D(_HeightMap, uv + fixed2(-s,  0)).r * _HeightScale; // Middle left
				float n2 = tex2D(_MainTex, uv + fixed2( 0, -s)).a + tex2D(_HeightMap, uv + fixed2( 0, -s)).r * _HeightScale; // Upper middle
				float n3 = tex2D(_MainTex, uv + fixed2( s,  0)).a + tex2D(_HeightMap, uv + fixed2( s,  0)).r * _HeightScale; // Middle right
				float n4 = tex2D(_MainTex, uv + fixed2( 0,  s)).a + tex2D(_HeightMap, uv + fixed2( 0,  s)).r * _HeightScale; // Middle lower
				
				float gravity = _Gravity;
				// Static pressure difference with neighbors
				// d1 -> Static pressure 1
				float d1 = _FluidDensity * gravity * (heightHere - n1); 
				float d2 = _FluidDensity * gravity * (heightHere - n2);
				float d3 = _FluidDensity * gravity * (heightHere - n3);
				float d4 = _FluidDensity * gravity * (heightHere - n4);

				// Acceleration along pipes to neighbors
				// a1 -> acceleration 1
				float a1 = d1 / (_FluidDensity * _PipeLength);
				float a2 = d2 / (_FluidDensity * _PipeLength);
				float a3 = d3 / (_FluidDensity * _PipeLength);
				float a4 = d4 / (_FluidDensity * _PipeLength);

				float deltaTime = _TimeStep;

				fixed4 flowHere = tex2D(_FlowMap, uv);

				// Flow to neighbors
				// f1 -> Flow 1
				float f1 = flowHere.r;////tex2D(_FlowMap, uv + fixed2(-s,  0)); // Current flow between middle left and here
				float f2 = flowHere.g;//tex2D(_FlowMap, uv + fixed2( 0, -s)); 
				float f3 = flowHere.b;//tex2D(_FlowMap, uv + fixed2( s,  0)); 
				float f4 = flowHere.a;//tex2D(_FlowMap, uv + fixed2( 0,  s)); 

				// f1n -> Flow 1 new 
				float f1n = max(f1 + deltaTime * _PipeCrossSectionArea * a1,0); // Flow left
				float f2n = max(f2 + deltaTime * _PipeCrossSectionArea * a2,0); // Flow up
				float f3n = max(f3 + deltaTime * _PipeCrossSectionArea * a3,0); // Flow right
				float f4n = max(f4 + deltaTime * _PipeCrossSectionArea * a4,0); // Flow down

				if (uv.x - s < 0)
				{
					f1n = 0;
				}

				if (uv.x + s >= 1)
				{
					f3n = 0;
				}

				if (uv.y + s >= 1)
				{
					f2n = 0;
				}

				if (uv.y - s < 0)
				{
					f4n = 0;
				}

				float outflowTotal = deltaTime * (f1n + f2n + f3n + f4n);

				// If there's more being transferred than is available here, scale back the water transfer.

				if ( outflowTotal > (waterHere * _PipeLength * _PipeLength)) // Is this conditional necessary?
				{
					float scaleConstant = (_PipeLength * _PipeLength * waterHere) / outflowTotal;

					f1n = f1n * scaleConstant;
					f2n = f2n * scaleConstant;
					f3n = f3n * scaleConstant;
					f4n = f4n * scaleConstant;
				}

				/*
				f1n = max(f1n, .00001);
				f2n = max(f2n, .00001);
				f3n = max(f3n, .00001);
				f4n = max(f4n, .00001);*/

				return fixed4(f1n, f2n, f3n, f4n);
			}

			ENDCG
		}
	}
}
