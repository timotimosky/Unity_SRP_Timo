Shader "SRPStudy/trans"
{
	Properties
	{
		_Color("Color Tint", Color) = (0.5,0.5,0.5)
		[NoScaleOffset]_MainTex("MainTex",2D) = "white"{}
		_SpecularPow("SpecularPow",range(5,50)) = 20
	}

	HLSLINCLUDE

	#include "UnityCG.cginc"

	//要计算相机方向，还需要物体的世界空间，所以顶点输出结构体中，还需要增加点世界空间位置的输出参数：
	half4 _CameraPos;
fixed _SpecularPow;
	//这里定义了颜色和基本贴图。这边没有定义贴图的缩放偏移。
	fixed4 _Color;
	sampler2D _MainTex;


	//这边需要平行光参数用于计算
	//  half4 _DLightDir;
	//  fixed4 _DLightColor;

	//定义最多4盏平行光
	#define MAX_DIRECTIONAL_LIGHTS 4
	//将之前的平行光参数改为参数组
	uniform  half4 _DLightDir[MAX_DIRECTIONAL_LIGHTS];
	uniform fixed4 _DLightColor[MAX_DIRECTIONAL_LIGHTS];


	//定义最多4盏点光
	#define MAX_POINT_LIGHTS 4
	uniform half4 _PLightPos[MAX_POINT_LIGHTS];
	uniform fixed4 _PLightColor[MAX_POINT_LIGHTS];


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
	};

	v2f vert(a2v v)
	{
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		o.uv = v.uv;
		//这边仍然使用的Unity的内置矩阵。目前来看还可以用。如果以后不行了，可以考虑
		//管线自己往里面传矩阵。第一个是MVP矩阵。管线里面传camera.projectMatrix*
		//camera.worldToCameraMatrix之后，再传入一个物体的localToWorldMatrix即可。
		//法线转换可以根据是否统一缩放来进行转换，可以参考目前unitycg.cginc中相关
		//代码，传入M矩阵即可。
		o.position = UnityObjectToClipPos(v.position);		
		o.normal = UnityObjectToWorldNormal(v.normal);
		return o;
	}

	half4 frag(v2f i) : SV_Target
	{
		half4 MainTexColor =  tex2D(_MainTex, i.uv);

		//像素着色器中，将计算单平行光部分修改为多平行光
		//定义平行光照参数
		half3 dLight = 0;

		half3 viewDir = normalize(_CameraPos - i.worldPos);
		half3 specular_color;
		//在平行光中循环
		for (int n = 0; n < MAX_DIRECTIONAL_LIGHTS; n++)
		{		
			//计算每盏灯的漫反射
			half light = saturate(dot(i.normal, _DLightDir[n]));	

			fixed specular = 0;
			//判断，仅第一盏光产生高光
			//if (n == 0)
			{
				half3 halfDir = normalize(viewDir + _DLightDir[n].xyz);
				specular = pow(saturate(dot(i.normal, halfDir)), _SpecularPow);
				specular_color += specular* _DLightColor[n].rgb;
			}	
			dLight += (light);
		}
		


		//像素管线中计算点光源光照
		half3 pLight = 0;

		for (int n = 0; n < MAX_POINT_LIGHTS; n++)
		{

			half3 pLightVector = _PLightPos[n].xyz - i.worldPos;
			half3 pLightDir = normalize(pLightVector);
			//距离平方，用于计算点光衰减
			half distanceSqr = max(dot(pLightVector, pLightVector), 0.00001);
			//点光衰减公式pow(max(1 - pow((distance*distance/range*range),2),0),2)
			half pLightAttenuation = pow(max(1 - pow((distanceSqr / (_PLightColor[n].a * _PLightColor[n].a)), 2),0), 2);
			half3 halfDir = normalize(viewDir + pLightDir);
			fixed specular = pow(saturate(dot(i.normal, halfDir)), _SpecularPow);
			half diff =   saturate(dot(i.normal, pLightDir));;
			pLight +=(1 + specular)*diff  *_PLightColor[0].rgb * pLightAttenuation;
		}

		half4 fragColor;
		fragColor.rgb = _Color.rgb *MainTexColor;
		fragColor.rgb = fragColor.rgb * pLight + fragColor.rgb *  dLight+ specular_color;;
		//fragColor.rgb =fragColor.rgb * (dLight +pLight) + specular_color++ specular_color;;
		//点光源与平行光的主要差别有以下几点：
//1、灯光方向是根据灯光位置和被照射物体位置计算得出，而不是CPU端传入的纯灯光参数。
//2、点光源的灯光强弱除了与灯光本身的颜色、强度相关之外，还与点光源与被照射物体之间的距离和点光源的自身照射范围(这是一个非基于物理的引入参数，
//方便进行灯光裁剪以及方便美术控制相关效果)相关。

		return fragColor;
	}

		ENDHLSL

		SubShader
	{
		Tags{ "Queue" = "transparent" }
			LOD 100
			Pass
		{
			//这边注意一下，Unity的默认光照模型中，是没有BaseLit这个类型的。在过去的默认管线中这么
			//写肯定不行，因为默认管线中的光照必须判断相关的宏定义来获取相关参数。但这边我们自己写
			//SRP，所以，这边其实可以随便起名字的。上一节的Unlit也是，默认应该是Always
			Tags{ "LightMode" = "BaseLit" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
	}
}