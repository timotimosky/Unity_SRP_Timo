Shader "Custom/Kata02/Unlit"
{
	Properties
	{
		_Color("Color Tint", Color) = (0.5,0.5,0.5)
	}

	HLSLINCLUDE

	#include "UnityCG.cginc"

	uniform float4 _Color;

	struct a2v
	{
		float4 position : POSITION;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
	};

	v2f vert(a2v v)
	{
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f,o);
		o.position = UnityObjectToClipPos(v.position);
		return o;
	}

	half4 frag(v2f v) : SV_Target
	{
		half4 fragColor = half4(_Color.rgb,1.0);
		return fragColor;
	}

	ENDHLSL

	SubShader
	{ 
		Tags{ "Queue" = "Geometry" }
		LOD 100
		Pass
		{
			Tags {"LightMode" = "BaseLit"}
			//Tags {"LightMode" = "Unlit"}

			//SRP不再使用CG 而是使用HLSL
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			ENDHLSL
		}
	}

}