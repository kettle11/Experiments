Shader "PaintWater"
{
    // Takes a standard RGBA texture and paints it to a RGBAFLOAT 32 texture by scaling the original texture
	Properties
	{
		_MainTex ("Main texture", 2D) = "white" {}
        _Water ("Water", 2D) = "white" {}
        _ScaleFactor("Scale texture", float) = 10
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
            sampler2D _Water;
            float4 _MainTex_ST;
            float _ScaleFactor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{   
				fixed4 c = tex2D(_MainTex, i.uv);
                fixed4 water = tex2D(_Water, i.uv);
                water += c * _ScaleFactor;
				return water;
			}

			ENDCG
		}
	}
}
