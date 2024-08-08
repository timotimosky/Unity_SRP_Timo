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
							#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
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


	//uniform float3 _LightDirection;

	struct a2v
	{
		float4 position : POSITION;
		float2 uv : TEXCOORD0;
		//需要模型法线进行计算光照
		float3 normal : NORMAL;
	};

	struct v2f
	{
		float4 positionCS : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 positionWS : TEXCOORD1;
		//法线传入像素管线计算像素光照
		float3 normalWS : NORMAL;

		#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
			float4 shadowCoord : TEXCOORD2;
		#endif
	};

	v2f vert(a2v v)
	{
		v2f o = (v2f)0;
		o.uv = v.uv;
		o.positionCS = TransformObjectToHClip(v.position.xyz);	
		o.normalWS = TransformObjectToWorldNormal(v.normal);
		o.positionWS = TransformObjectToWorld(v.position.xyz);//模型转世界空间

		//使用内置的Shadows.hlsl 文件中提供的方法计算阴影偏移（bias）,然后将其转换到裁剪空间中：
		float4 positionCS = TransformWorldToHClip(ApplyShadowBias(o.positionWS, o.normalWS, _LightDirection));
		//然后确保阴影偏移后的位置不会超出裁剪空间
		#if UNITY_REVERSED_Z
			o.positionCS.z = min(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
		 #else
			o.positionCS.z = max(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
		#endif

		#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)

			o.shadowCoord = TransformWorldToShadowCoord(o.worldPos);

		#endif


		return o;
	}

	half4 frag(v2f i) : SV_Target
	{
		half4 MainTexColor =  tex2D(_MainTex, i.uv);

		half3 viewDir = normalize(_CameraPos - i.positionWS);
		half3 specular_color;

		half3 lightDir = 0;
		half3 dLightRGB = 0;
		half4 lightColor;
		for (int n = 0; n < _DirectionalLightCount; n++)//支持多个直接灯，但其他灯只能被设置为不重要
		{		
			lightDir = _DirectionalLightDirections[n];
			lightColor = _DirectionalLightColors[n];

			//计算每盏灯的漫反射
			half light = saturate(dot(i.normalWS,lightDir));	

			//仅第一盏光产生高光
			if (n == 0)
			{
				half3 halfDir = normalize(viewDir + lightDir.xyz);
				half specular = pow(saturate(dot(i.normalWS, halfDir)), _SpecularPow);
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


			half3 pLightVector = lightDir.xyz - i.positionWS;
			half3 pLightDir = normalize(pLightVector);
			//距离平方，用于计算点光衰减
			half distanceSqr = max(dot(pLightVector, pLightVector), 0.00001);
			//点光衰减公式pow(max(1 - pow((distance*distance/range*range),2),0),2)
			half pLightAttenuation = pow(max(1 - pow((distanceSqr / (lightColor.a * lightColor.a)), 2),0), 2);
			half3 halfDir = normalize(viewDir + pLightDir);
			half specular = pow(saturate(dot(i.normalWS, halfDir)), _SpecularPow);
			half diff =   saturate(dot(i.normalWS, pLightDir));;
			pLight +=(1 + specular)*diff  *lightColor.rgb * pLightAttenuation;
		}

		half3 fragColor= _Color.rgb *MainTexColor * (dLightRGB);//+ specular_color;

		//点光源与平行光的主要差别有以下几点：
		//1、灯光方向是根据灯光位置和被照射物体位置计算得出，而不是CPU端传入的纯灯光参数。
		//2、点光源的灯光强弱除了与灯光本身的颜色、强度相关之外，还与点光源与被照射物体之间的距离和点光源的自身照射范围(这是一个非基于物理的引入参数，
		//方便进行灯光裁剪以及方便美术控制相关效果)相关。


		//获取主光源阴影坐标：
		#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
			float4 shadowCoord = input.shadowCoord;
		#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
			float4 shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
		#else
			float4 shadowCoord = float4(0, 0, 0, 0);
		#endif
		//获取主光源结构体（计算主光源的阴影衰减）然后混合阴影：
		Light main = GetMainLight(shadowCoord);
		fragColor *= main.shadowAttenuation;
		//ShadowCaster
		//float4  shadowCoord = TransformWorldToShadowCoord(i.positionWS);
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

			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS //开启额外光源
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS //主光源阴影
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE //主光源层级阴影是否开启
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS //额外光源阴影
			#pragma multi_compile _ _SHADOWS_SOFT //软阴影

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

			#pragma multi_compile _ _CASCADE_BLEND_SOFT
			#pragma multi_compile_instancing
			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment
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