Shader "SRP/trans"
{
	Properties
	{
		[NoScaleOffset]_MainTex("MainTex",2D) = "white"{}
		_BaseMap("Texture", 2D) = "white" {}
		_TransAlpha("TransAlpha",range(0,1)) = 0.5
		[HDR] _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		[Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
		[KeywordEnum(On, Clip, Dither, Off)] _Shadows ("Shadows", Float) = 0

		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
		[Enum(Off,0,On,1)] _ZWrite("Z Write",Float) = 1
	}

		HLSLINCLUDE

			//#include"packages/packages.unity.cn/com.unity.render - pipelines.core@8.2.0/ShaderLibrary/SpaceTransforms.hlsl"
			//#include"Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
			//#include "PackageCache\com.unity.render-pipelines.core@7.1.8\ShaderLibrary"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include"Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl" 
			//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
			//#include "ShaderLibrary/Common.hlsl"
			//#include "ShaderLibrary/LitInput.hlsl"
			//#include "ShaderLibrary/UnityInput.hlsl"
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
			v2f o =(v2f)0;
			o.uv = v.uv;
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
				Tags{ "LightMode" = "SrpTransparent" }
				ZWrite[_ZWrite]
				//Blend [_SrcBlend][_DstBlend]
				Blend SrcAlpha OneMinusSrcAlpha
				HLSLPROGRAM
				#pragma target 3.5
				#pragma shader_feature _CLIPPING
				#pragma multi_compile_instancing
				#pragma vertex vert
				#pragma fragment frag
				ENDHLSL
			}
		}
}
