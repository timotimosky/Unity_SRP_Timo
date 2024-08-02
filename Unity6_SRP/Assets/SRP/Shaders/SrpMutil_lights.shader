Shader "SRP/mutil_lights"
{
	Properties
	{
		_Color("Color Tint", Color) = (0.5,0.5,0.5)
		[NoScaleOffset]_MainTex("MainTex",2D) = "white"{}
	}

	HLSLINCLUDE

	#include "UnityCG.cginc"
			//定义最多4盏点光
	#define MAX_POINT_LIGHTS 4
		half4 _PLightPos[MAX_POINT_LIGHTS];
	fixed4 _PLightColor[MAX_POINT_LIGHTS];

	//这里定义了颜色和基本贴图。这边没有定义贴图的缩放偏移。
	fixed4 _Color;
	sampler2D _MainTex;
	//这边需要平行光参数用于计算
	half4 _DLightDir;
	fixed4 _DLightColor;

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
		//可以考虑管线自己往里面传矩阵。第一个是MVP矩阵。管线里面传camera.projectMatrix*camera.worldToCameraMatrix之后，
		//再传入一个物体的localToWorldMatrix即可。
		//法线转换可以根据是否统一缩放来进行转换，可以参考目前unitycg.cginc中相关代码，传入M矩阵即可。
		o.position = UnityObjectToClipPos(v.position);
		o.normal = UnityObjectToWorldNormal(v.normal);
		return o;
	}

	half4 frag(v2f i) : SV_Target
	{
		//像素管线中计算点光源光照
		half3 pLight = 0;
		for (int n = 0; n < MAX_POINT_LIGHTS; n++)
		{
			fixed specular = 0;
			half3 pLightVector = _PLightPos[n].xyz - i.worldPos;
			half3 pLightDir = normalize(pLightVector);
			//距离平方，用于计算点光衰减
			half distanceSqr = max(dot(pLightVector, pLightVector), 0.00001);
			//点光衰减公式pow(max(1 - pow((distance*distance/range*range),2),0),2)
			half pLightAttenuation = pow(max(1 - pow((distanceSqr / (_PLightColor[n].a * _PLightColor[n].a)), 2),0), 2);
			//half3 halfDir = normalize(viewDir + pLightDir);
		//	specular = pow(saturate(dot(i.normal, halfDir)), _SpecularPow);
			//pLight += (1 + specular) * saturate(dot(i.normal, pLightDir)) * _PLightColor[n].rgb * pLightAttenuation;
		}


		half4 fragColor = half4(_Color.rgb,1.0) * tex2D(_MainTex, i.uv);
		//获得光照参数，进行兰伯特光照计算
		half light = saturate(dot(i.normal, _DLightDir));
		fragColor.rgb *= light * _DLightColor;
		return fragColor;
	}

		ENDHLSL

		SubShader
	{
		Tags{ "Queue" = "Geometry" }
			LOD 100
			Pass
		{
			Tags{ "LightMode" = "SrpUnlit" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
	}
}
