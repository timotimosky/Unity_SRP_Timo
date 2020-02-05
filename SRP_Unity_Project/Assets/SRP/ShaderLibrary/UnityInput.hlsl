#ifndef CUSTOM_UNITY_INPUT_INCLUDED
#define CUSTOM_UNITY_INPUT_INCLUDED

float4x4 unity_ObjectToWorld;

#endif

#ifndef CUSTOM_COMMON_INCLUDED
#define CUSTOM_COMMON_INCLUDED

#include "UnityInput.hlsl"

float3 TransformObjectToWorld (float3 positionOS) {
	return mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
}
	
#endif

#include "Assets/SRP/ShaderLibrary/Common.hlsl"

float4 UnlitPassVertex (float3 positionOS : POSITION) : SV_POSITION {
	float3 positionWS = TransformObjectToWorld(positionOS.xyz);
	return float4(positionWS, 1.0);
}