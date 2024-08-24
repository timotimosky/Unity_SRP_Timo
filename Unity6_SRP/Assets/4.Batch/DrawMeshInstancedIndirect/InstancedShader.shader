 Shader "Instanced/InstancedShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader {

        Pass {

            Tags {"LightMode"="ForwardBase"}

            HLSLPROGRAM
            //URP必须要加这一句预编译指令：这个预编译指令主要是用来告诉unity我们需要drawIndirect
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup
            #pragma vertex vert
            #pragma fragment frag
           // #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

        #if SHADER_TARGET >= 45
            StructuredBuffer<float4> positionBuffer;
        #endif

        	struct a2v {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				//使用 GPU 实例化时，对象索引也可用作顶点属性
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID //不能直接从SV_InstanceID中拿instanceID，必须先定义在顶点的输入和输出结构中。
            };
            //setup方法里面是用来处理InstanceID计算出来后去怎么处理数据，如果用不到可以不填充这个方法，但是必须实现。
            void setup()
            {

            }

            void rotate2D(inout float2 v, float r)
            {
                float s, c;
                sincos(r, s, c);
                v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
            }

            v2f vert (a2v v, uint instanceID : SV_InstanceID)
            {
                #if SHADER_TARGET >= 45
                    float4 data = positionBuffer[instanceID];
                #else
                    float4 data = 0;
                #endif

               v2f o;
                //从输入中提取索引，并将其存储在其他实例化宏所依赖的全局静态变量中
				UNITY_SETUP_INSTANCE_ID(v); 
				//使实例索引在frag中可用
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.pos = TransformObjectToHClip(v.vertex.xyz);
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = v.uv;

                return o;
            }

            real4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                real4 albedo = tex2D(_MainTex, i.uv.xy);
                real4 output = real4(albedo.rgb , albedo.w);
                //real4 m_OutsideColor = UNITY_ACCESS_INSTANCED_PROP(Props, _OutsideColor);
                return output;
            }

            ENDHLSL
        }
    }
}