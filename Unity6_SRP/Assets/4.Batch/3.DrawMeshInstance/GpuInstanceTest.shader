Shader "DJL/GpuInstanceTest" {
	Properties {
		
		_Sin("Sin", Float) = 0
		_MainTex("Main Tex", 2D) = "white"{}
		_TemplateTex ("Template Tex", 2D) = "white"{}

		_OutsideColor("Outside Color", Color) = (1, 0.98, 0.9, 1)
		_OutsideSize("Outside Size", int) = 0
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
			//multi_compile_instancing使Unity 生成两种着色器变体，一种支持 GPU 实例化，另一种不支持 GPU 实例化。
			//材质中还出现了一个切换选项
			#pragma multi_compile_instancing
			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			//constBuffer传参处理 ：必须用数组引用替换
			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(real4, _OutsideColor)
			UNITY_INSTANCING_BUFFER_END(Props)
			float _Sin;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			sampler2D _TemplateTex;
			float4 _TemplateTex_TexelSize;
   
			//real4 _OutsideColor;
            int _OutsideSize;
			int _EmbossSize;
			float _AlphaT;
			
			struct a2v {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				//使用 GPU 实例化时，对象索引也可用作顶点属性
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

				//从输入中提取索引，并将其存储在其他实例化宏所依赖的全局静态变量中
				UNITY_SETUP_INSTANCE_ID(v); 
				//使实例索引在frag中可用
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.pos = TransformObjectToHClip(v.vertex.xyz);
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = v.uv;
				return o;
			}
			
			real4 frag(v2f i) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(i);
				real4 tc = tex2D(_TemplateTex, i.uv.zw);
				//real sinNow =	UNITY_ACCESS_INSTANCED_PROP(Props, _Sin);
				real4 m_OutsideColor = UNITY_ACCESS_INSTANCED_PROP(Props, _OutsideColor);
				real3 color = real3(tex2D(_MainTex, i.uv.xy).rgb + real3(0.1, 0.1, 0.1) * sin(_Sin));
				if (_OutsideSize > 0)
				{
					float refg = 1 - _OutsideSize * 0.1;
					float l = step(refg, tc.g) * tc.g;
					return real4(lerp(color, m_OutsideColor.rgb, l), tc.a);
				}
				real v1 = i.uv.y + 30 * _MainTex_TexelSize.y;
				real v2 = i.uv.y - 30 * _MainTex_TexelSize.y;
				real u1 = i.uv.x - 18 * _MainTex_TexelSize.x;
				real u2 = i.uv.x + 18 * _MainTex_TexelSize.x;
				if (tc.b > 0)
				{
					real4 c1 = tex2D(_TemplateTex, i.uv.zw + float2(5, 5) * _TemplateTex_TexelSize.xy);
					real4 c2 = tex2D(_TemplateTex, i.uv.zw + float2(-5, -5) * _TemplateTex_TexelSize.xy);
					if (abs(c2.a - c1.a) > 0.8)
					{
						int a = step(0, c2.a - c1.a);
						color = lerp(color, a * real3(0.9, 0.9, 0.9) + (1 - a) * real3(0.45, 0.45, 0.45), tc.b);
					}
				}
				/*
				[unroll(10)]
                for (int j = 1; j < _EmbossSize + 1; j++)
                {
					[unroll(18)]
					for(int k = 0; k < 18; k++)
					{
						real4 c1 = tex2D(_TemplateTex, i.uv.zw + j * 1.414 * float2(cos(5 * k), sin(5 * k)) * _TemplateTex_TexelSize.xy);
						real4 c2 = tex2D(_TemplateTex, i.uv.zw - j * 1.414 * float2(cos(5 * k), sin(5 * k)) * _TemplateTex_TexelSize.xy);
						if (abs(c2.a - c1.a) > 0.8)
						{
							int a = step(0, c2.a - c1.a);
							color = lerp(color, a * real3(0.9, 0.9, 0.9) + (1 - a) * real3(0.45, 0.45, 0.45), 3 * pow(1 - j * 0.18, 2) - 2 * pow(1 - j * 0.18, 3));
							break;
						}
					}
                }
				*/
				if (v1 > 1 || v2 < 0 || u1 < 0 || u2 > 1)
					color = lerp(color, real3(1, 1, 1), 0.4);
				return real4(color, tc.a) * _AlphaT+m_OutsideColor;
			}

			ENDHLSL
		} 

	}
}