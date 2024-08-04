Shader "SRP/Post FX Stack" {
	
	SubShader {
		Cull Off
		ZTest Always
		ZWrite Off
		
		HLSLINCLUDE
		#include"Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 
		//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 		//#include "Package/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
 		//#include "PackageCache/com.unity.render-pipelines.universal/ShaderLibrary/Color.hlsl"
// #include "PackageCache/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#include "ShaderLibrary/PostFXStackPasses.hlsl"
		ENDHLSL
		
		Pass {
			Name "Bloom Add"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment BloomAddPassFragment
			ENDHLSL
		}
		
		Pass {
			Name "Bloom Horizontal"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment BloomHorizontalPassFragment
			ENDHLSL
		}

		Pass {
			Name "Bloom Prefilter"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment BloomPrefilterPassFragment
			ENDHLSL
		}
		
		Pass {
			Name "Bloom Prefilter Fireflies"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment BloomPrefilterFirefliesPassFragment
			ENDHLSL
		}
		
		Pass {
			Name "Bloom Scatter"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment BloomScatterPassFragment
			ENDHLSL
		}
		
		Pass {
			Name "Bloom Scatter Final"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment BloomScatterFinalPassFragment
			ENDHLSL
		}
		
		Pass {
			Name "Bloom Vertical"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment BloomVerticalPassFragment
			ENDHLSL
		}
		
		Pass {
			Name "Copy"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment CopyPassFragment
			ENDHLSL
		}
		
		Pass {
			Name "Tone Mapping ACES"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment ToneMappingACESPassFragment
			ENDHLSL
		}
		
		Pass {
			Name "Tone Mapping Neutral"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment ToneMappingNeutralPassFragment
			ENDHLSL
		}
		
		Pass {
			Name "Tone Mapping Reinhard"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex DefaultPassVertex
				#pragma fragment ToneMappingReinhardPassFragment
			ENDHLSL
		}
	}
}