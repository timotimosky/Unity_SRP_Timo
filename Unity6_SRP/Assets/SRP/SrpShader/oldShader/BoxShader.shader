// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
Shader "DJL/BoxShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	//_nearDist("Near Disttance",float) = 0

//[PerRendererData]-表示纹理属性将以MaterialPropertyBlock的形式来自每个渲染器数据。
//材质检查器更改了这些属性的纹理插槽UI。
	_Color("Color", Color) = (1, 0.98, 0.9, 1)
		//_farDist("Far Distance",float) = 30
		_Density("Density",Range(0,10)) = 1.43
		 roundMax("roundMax",float) = 5.5
	}
		SubShader
	{
		Tags { "Queue" = "Geometry"  "RenderType" = "Opaque"}  //0.instance必须指定渲染队列
		//Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Stencil
			{
				Ref 1
				Comp Always
				Pass Replace
		//Fail Keep
		//ZFail Replace
	}
		Tags { "LightMode" = "ForwardBase"}
		Cull Off
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0
		// make fog work
		//#pragma multi_compile_fog

		#include "UnityCG.cginc"
		#include "Lighting.cginc" 
		#pragma   multi_compile_instancing
		#pragma multi_compile_fwdbase_fullshadows
		#include "AutoLight.cginc"
		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		UNITY_INSTANCING_BUFFER_END(Props)


		float _nearDist;

		float _farDist;
		float _Density;
		float roundMax;
		struct a2v
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID   //1
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;

			//	UNITY_FOG_COORDS(1)
				float4 pos : SV_POSITION;
				//LIGHTING_COORDS(2, 4)
				//float4 posWorld : TEXCOORD2;
				SHADOW_COORDS(2)
				float4 depth : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID   //2
				float3 worldNormal : TEXCOORD4;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(a2v v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);//3
				UNITY_TRANSFER_INSTANCE_ID(v, o);//3
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
				o.depth = o.pos;//命名为pos的值，如果我们这个pass计算阴影，Unity默认在TRANSFER_SHADOW 会转换它到阴影空间，所以我们再单独保存一次

				//o.depth.z = -o.depth.z;

				//o.depth.w = (_farDist - _nearDist)*o.depth.w;
				//o.depth.w = o.depth.w;
				//o.depth.x = -o.depth.x;
				//o.depth.y = o.depth.y;
				//o.uv = v.uv ;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				TRANSFER_SHADOW(o);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				i.uv = TRANSFORM_TEX(i.uv, _MainTex);
			// sample the texture
			//fixed4 col = tex2D(_MainTex, i.uv);
			// apply fog
			//UNITY_LIGHT_ATTENUATION(atten, IN, wpos);
			//UNITY_APPLY_FOG(i.fogCoord, col);

			UNITY_SETUP_INSTANCE_ID(i);//4

			//fixed sinNow =	UNITY_ACCESS_INSTANCED_PROP(Props, _Sin);
			fixed4 m_Color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);//5


			float dis = sqrt(i.depth.x * i.depth.x + i.depth.y * i.depth.y);
			float fg = dis / roundMax / i.depth.w;///depth.w 把z缩放回01空间的z

			//if (i.depth.z > _nearDist&&i.depth.z < _farDist)

			//{

				//fg = dis /i.depth.w;

			/*}

			else if (i.depth.z > _farDist)

			{

				fg = _farDist / i.depth.w;

			}*/

			fixed shadow = SHADOW_ATTENUATION(i);

			fixed4 col = tex2D(_MainTex, i.uv) * shadow;
			//* shadow;
			//return (1-fg) * _Density*col;
			return col * m_Color;
			//return col;
		}
		ENDCG
	}
	}
		FallBack "Diffuse"
}
