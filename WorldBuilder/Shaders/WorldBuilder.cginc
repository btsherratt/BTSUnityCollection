// SKFX World Builder

#pragma shader_feature SKFX_WB_INSTANCING_ENABLED

#if SHADER_TARGET >= 45 && defined(SKFX_WB_INSTANCING_ENABLED) && !defined(UNITY_INSTANCING_ENABLED)

	#define SKFX_WB_GLOBALS StructuredBuffer<float4x4> _SKFXWorldBuilderInstanceData;

	#define SKFX_WB_VERTEX_INPUT_INSTANCE_ID uint SKFXWBInstanceID : SV_InstanceID;

	#define SKFX_WB_SETUP_O2W_MATRIX(vtx) unity_ObjectToWorld = _SKFXWorldBuilderInstanceData[vtx.SKFXWBInstanceID];
	#define SKFX_WB_SETUP_MATRICES(vtx) SKFX_WB_SETUP_O2W_MATRIX(vtx)

#else

	#define SKFX_WB_GLOBALS /* Nothing... */
	#define SKFX_WB_VERTEX_INPUT_INSTANCE_ID /* Nothing... */
	#define SKFX_WB_SETUP_O2W_MATRIX(vtx) /* Nothing... */
	#define SKFX_WB_SETUP_MATRICES(vtx) /* Nothing... */

#endif