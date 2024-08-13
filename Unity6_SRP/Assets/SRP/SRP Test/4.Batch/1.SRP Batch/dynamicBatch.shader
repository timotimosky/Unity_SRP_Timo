Shader "DJL/dynamicBatch" {
	Properties {
		
		_Sin("Sin", Float) = 0
		_MainTex("Main Tex", 2D) = "white"{}

		_OutsideColor("Outside Color", Color) = (1, 0.98, 0.9, 1)
		_EmbossSize("Emboss Size", int) = 0
		_AlphaT("Alpha T", Float) = 1
		
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
	}
	SubShader {

		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		Stencil
		{
		   Ref[_Stencil]
		   Comp[_StencilComp]
		   Pass[_StencilOp]
		   ReadMask[_StencilReadMask]
		   WriteMask[_StencilWriteMask]
		}

		ColorMask[_ColorMask]
		
		Pass { 

			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			float _Sin;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
   
			real4 _OutsideColor;
			int _EmbossSize;
			float _AlphaT;
			
			struct a2v {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			v2f vert(a2v v) {
				v2f o;
				//UNITY_SETUP_INSTANCE_ID放在顶点着色器和片段着色器中最开始的地方，用来访问全局unity_InstanceID
				//因为现在VBO相同，但GPU需要InstanceID来访问不同的PBO(也就是我们的材质块传递进来的东西)
				//当需要将实例化ID传到片段着色器时，在顶点着色器中添加UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_SETUP_INSTANCE_ID(v); 
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.pos = TransformObjectToHClip(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = v.uv;
				return o;
			}
			
			real4 frag(v2f i) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(i);
				//real sinNow =	UNITY_ACCESS_INSTANCED_PROP(Props, _Sin);
				real4 m_OutsideColor = UNITY_ACCESS_INSTANCED_PROP(Props, _OutsideColor);
				real3 color = real3(tex2D(_MainTex, i.uv.xy).rgb + real3(0.1, 0.1, 0.1) * sin(_Sin))+m_OutsideColor.rgb;
				return real4(color, 1);
			}

			ENDHLSL
		} 

	}
}