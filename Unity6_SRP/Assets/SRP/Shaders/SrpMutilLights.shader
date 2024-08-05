Shader "SRP/mutil_lights"
{
	Properties
	{
		_Color("Color Tint", Color) = (0.5,0.5,0.5)
		[NoScaleOffset]_MainTex("MainTex",2D) = "white"{}
		_SpecularPow("SpecularPow",range(5,50)) = 20
	}

	HLSLINCLUDE

	#include"Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 
	#define MAX_POINT_LIGHTS 4
	int _DirectionalLightCount;
	half4  _DirectionalLightDirections[MAX_POINT_LIGHTS];
	real4  _DirectionalLightColors[MAX_POINT_LIGHTS];

	//这里定义了颜色和基本贴图。这边没有定义贴图的缩放偏移。
	real4 _Color;
	sampler2D _MainTex;
	//这边需要平行光参数用于计算
	half4 _DLightDir;
	real4 _DLightColor;
	real _SpecularPow;
	struct a2v
	{
		float4 position : POSITION;
		float2 uv : TEXCOORD0;
		float3 normal : NORMAL;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 normal : NORMAL;
	};

	v2f vert(a2v v)
	{
		v2f o;
		o.uv = v.uv;
		o.position = TransformObjectToHClip(v.position);		
		o.normal = TransformObjectToWorldNormal(v.normal);
		return o;
	}
	

	half4 frag(v2f i) : SV_Target
	{
		//像素管线中计算点光源光照
		half3 pLight = 0;
		half3 viewDir =0;

		// for (int n = 0; n < _DirectionalLightCount; n++)
		// {
		// 	real specular = 0;
		// 	half3 pLightVector = _DirectionalLightDirections[n].xyz - i.position;
		// 	half3 pLightDir = normalize(pLightVector);
		// 	//距离平方，用于计算点光衰减
		// 	half distanceSqr = max(dot(pLightVector, pLightVector), 0.00001);
		// 	//点光衰减公式pow(max(1 - pow((distance*distance/range*range),2),0),2)
		// 	half pLightAttenuation = pow(max(1 - pow((distanceSqr / (_DirectionalLightColors[n].a * _DirectionalLightColors[n].a)), 2),0), 2);
		// 	half3 halfDir = normalize(viewDir + pLightDir);
		// 	half noraml_dir =  saturate(dot(i.normal, halfDir));
		// 	specular = SafePositivePow(noraml_dir, _SpecularPow);
		// 	pLight += (1 + specular) * saturate(dot(i.normal, pLightDir)) * _DirectionalLightColors[n].rgb * pLightAttenuation;
		// }

		half3 lightDir = 0;
		half3 lightRGB = 0;
		half4 lightColor;
		for (int n = 0; n < _DirectionalLightCount; n++) //支持多个直接灯，但其他灯只能被设置为不重要
		{
			lightDir = _DirectionalLightDirections[n];
			lightColor = _DirectionalLightColors[n];

			//1.兰伯特光照计算
			half light = saturate(dot(i.normal,lightDir));
			lightRGB += light * lightColor;

			//2.高光
			real specular = 0;
			half3 pLightVector = lightDir.xyz - i.position;
			half3 pLightDir = normalize(pLightVector);
			//距离平方，用于计算点光衰减
			half distanceSqr = max(dot(pLightVector, pLightVector), 0.00001);
			//点光衰减公式pow(max(1 - pow((distance*distance/range*range),2),0),2)
			half pLightAttenuation = pow(max(1 - pow((distanceSqr / (lightColor.a * lightColor.a)), 2),0), 2);
			half3 halfDir = normalize(viewDir + pLightDir);
			half noraml_dir =  saturate(dot(i.normal, halfDir));
			specular = SafePositivePow(noraml_dir, _SpecularPow);
			pLight += (1 + specular) * saturate(dot(i.normal, pLightDir)) * lightColor.rgb * pLightAttenuation;
		}


		half4 fragColor = half4(_Color.rgb*lightRGB,1.0) * tex2D(_MainTex, i.uv);

		return fragColor;
	}

		ENDHLSL

		SubShader
	{
		Tags{ "Queue" = "Geometry" }
			LOD 100
			Pass
		{
			Tags{ "LightMode" = "SrpLit" }
			HLSLPROGRAM
			#pragma multi_compile _ _LIGHTS_PER_OBJECT
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
	}
}