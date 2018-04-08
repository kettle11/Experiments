// AlanZucconi.com: http://www.alanzucconi.com/?p=4539
Shader "Hidden/Invert"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
	
	SubShader
	{
		// Required to work
		ZTest Always Cull Off ZWrite Off
		Fog{ Mode off }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;

			float4 frag(v2f_img i) : COLOR
			{
				float4 c = tex2D(_MainTex, i.uv);

				return 1 - c;
			}
			ENDCG
		}
	}
}