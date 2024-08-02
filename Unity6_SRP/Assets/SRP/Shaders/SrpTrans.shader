Shader "SRP/trans"
{
	Properties
	{
		[NoScaleOffset]_MainTex("MainTex",2D) = "white"{}
		_TransAlpha("TransAlpha",range(5,50)) = 20
			[Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]
			_DstBlend("Dst Blend",Float) = 0
		[Enum(Off,0,On,1)] _ZWrite("Z Write",Float) = 1
	}

		HLSLINCLUDE

//#include "UnityCG.cginc"

//#include"packages/packages.unity.cn/com.unity.render - pipelines.core@8.2.0/ShaderLibrary/SpaceTransforms.hlsl"
			//#include"Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
			//#include "PackageCache\com.unity.render-pipelines.core@7.1.8\ShaderLibrary"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
//#include "ShaderLibrary/Common.hlsl"
#include "ShaderLibrary/LitInput.hlsl"
#include "ShaderLibrary/UnityInput.hlsl"
			//要计算相机方向，还需要物体的世界空间，所以顶点输出结构体中，还需要增加点世界空间位置的输出参数：
			half4 _CameraPos;
			half _TransAlpha;
		sampler2D _MainTex;
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
		//	float3 normal : NORMAL;
		};

		v2f vert(a2v v)
		{
			v2f o;
		//	UNITY_INITIALIZE_OUTPUT(v2f, o);
			o.uv = v.uv;
			//这边仍然使用的Unity的内置矩阵。目前来看还可以用。如果以后不行了，可以考虑
			//管线自己往里面传矩阵。第一个是MVP矩阵。管线里面传camera.projectMatrix*
			//camera.worldToCameraMatrix之后，再传入一个物体的localToWorldMatrix即可。
			//法线转换可以根据是否统一缩放来进行转换，可以参考目前unitycg.cginc中相关
			//代码，传入M矩阵即可。
			o.position = TransformObjectToHClip(v.position);
			//o.normal = UnityObjectToWorldNormal(v.normal);
			return o;
		}

		half4 frag(v2f i) : SV_Target
		{
			half4 MainTexColor = tex2D(_MainTex, i.uv);

			return half4(MainTexColor.rgb, _TransAlpha);
		}

			ENDHLSL

			SubShader
		{
			Tags{ "Queue" = "Transparent"  "RenderType" = "Transparent" }
				LOD 100
				Pass
			{
				//这边注意一下，Unity的默认光照模型中，是没有BaseLit这个类型的。在过去的默认管线中这么
				//写肯定不行，因为默认管线中的光照必须判断相关的宏定义来获取相关参数。
				//SRP可以随便起名字的。上一节的Unlit也是，默认应该是Always
				Tags{ "LightMode" = "SrpTrans" }
					ZWrite[_ZWrite]
				Blend [_SrcBlend][_DstBlend]
				//Blend SrcAlpha OneMinusSrcAlpha
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				ENDHLSL
			}
		}
}
