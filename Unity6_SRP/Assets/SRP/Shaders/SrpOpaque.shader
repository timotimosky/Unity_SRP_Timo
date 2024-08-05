Shader "SRP/Opaque"
{
	Properties
	{
		_Color("Color Tint", Color) = (0.5,0.5,0.5)
		[NoScaleOffset]_MainTex("MainTex",2D) = "white"{}
		_SpecularPow("SpecularPow",range(5,50)) = 20
	}

	HLSLINCLUDE
	//#include"Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"	 

	half4 _CameraPos;
	half4 _SpecularPow;

	half4 _Color;
	sampler2D _MainTex;

	//定义最多4盏平行光
	#define MAX_DIRECTIONAL_LIGHTS 4
	int _DirectionalLightCount;
	uniform  half4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHTS];
	uniform real4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHTS];


	//定义最多4盏点光
	#define MAX_POINT_LIGHTS 4
	int _OtherLightCount;
	uniform half4 _OtherLightPositions[MAX_POINT_LIGHTS];
	uniform half4 _OtherLightColors[MAX_POINT_LIGHTS];


	struct a2v
	{
		float4 position : POSITION;
		float2 uv : TEXCOORD0;
		//需要模型法线进行计算光照
		float3 normal : NORMAL;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 worldPos : TEXCOORD1;
		//法线传入像素管线计算像素光照
		float3 normal : NORMAL;
		float4 shadowCoord: TEXCOORD2;
	};

	v2f vert(a2v v)
	{
		v2f o = (v2f)0;
		o.uv = v.uv;
		o.position = TransformObjectToHClip(v.position.xyz);	
		o.normal = TransformObjectToWorldNormal(v.normal);
		
		o.worldPos =v.position.xyz;
		return o;
	}

	half4 frag(v2f i) : SV_Target
	{
		half4 MainTexColor =  tex2D(_MainTex, i.uv);

		half3 viewDir = normalize(_CameraPos - i.worldPos);
		half3 specular_color =0;

		half3 lightDir = 0;
		half3 dLightRGB = 0;
		half4 lightColor;
		for (int n = 0; n < _DirectionalLightCount; n++)//支持多个直接灯，但其他灯只能被设置为不重要
		{		
			lightDir = _DirectionalLightDirections[n];
			lightColor = _DirectionalLightColors[n];

			//计算每盏灯的漫反射
			half light = saturate(dot(i.normal,lightDir));	

			//仅第一盏光产生高光
			if (n == 0)
			{
				half3 halfDir = normalize(viewDir + lightDir.xyz);
				half specular = pow(saturate(dot(i.normal, halfDir)), _SpecularPow);
				specular_color += specular* lightColor.rgb;
			}	
			dLightRGB += light * lightColor;
		}
		


		//像素管线中计算点光源光照
		half3 pLight = 0;

		for (int n = 0; n < _OtherLightCount; n++)
		{
			lightDir = _OtherLightPositions[n];
			lightColor = _OtherLightColors[n];


			half3 pLightVector = lightDir.xyz - i.worldPos;
			half3 pLightDir = normalize(pLightVector);
			//距离平方，用于计算点光衰减
			half distanceSqr = max(dot(pLightVector, pLightVector), 0.00001);
			//点光衰减公式pow(max(1 - pow((distance*distance/range*range),2),0),2)
			half pLightAttenuation = pow(max(1 - pow((distanceSqr / (lightColor.a * lightColor.a)), 2),0), 2);
			half3 halfDir = normalize(viewDir + pLightDir);
			half specular = pow(saturate(dot(i.normal, halfDir)), _SpecularPow);
			half diff =   saturate(dot(i.normal, pLightDir));;
			pLight +=(1 + specular)*diff  *lightColor.rgb * pLightAttenuation;
		}

		half3 fragColor= _Color.rgb *MainTexColor * (dLightRGB);//+ specular_color;

		//点光源与平行光的主要差别有以下几点：
		//1、灯光方向是根据灯光位置和被照射物体位置计算得出，而不是CPU端传入的纯灯光参数。
		//2、点光源的灯光强弱除了与灯光本身的颜色、强度相关之外，还与点光源与被照射物体之间的距离和点光源的自身照射范围(这是一个非基于物理的引入参数，
		//方便进行灯光裁剪以及方便美术控制相关效果)相关。

		//ShadowCaster
		float4  shadowCoord = TransformWorldToShadowCoord(i.worldPos);
		Light mainLight = GetMainLight(shadowCoord);
		half shadow = MainLightRealtimeShadow(shadowCoord);
		return  float4(fragColor *  shadow,1);
	}

		ENDHLSL

	SubShader
	{
		Tags{ "Queue" = "Geometry" }
		LOD 100
		Pass
		{
			Tags { "LightMode" = "SrpLit" }
			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#pragma multi_compile _ _LIGHTS_PER_OBJECT
			#pragma multi_compile  _CASCADE_BLEND_SOFT
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass 
		{
			Tags {
				"LightMode" = "ShadowCaster"
			}

			ColorMask 0

			HLSLPROGRAM
			#pragma target 3.5
			#pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
			#pragma multi_compile_instancing
			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment
			#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
			ENDHLSL
		}
		//lightMap
		// Pass {
		// 	Tags {
		// 		"LightMode" = "Meta"
		// 	}

		// 	Cull Off

		// 	HLSLPROGRAM
		// 	#pragma target 3.5
		// 	#pragma vertex UnityMetaVertexPosition
		// 	#pragma fragment UnityMetaFragment
		// 	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/MetaPass.hlsl"
		// 	ENDHLSL
		// }
	}
}