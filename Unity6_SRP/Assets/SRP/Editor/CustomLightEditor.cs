using UnityEngine;
using UnityEditor;

[CustomEditorForRenderPipeline(typeof(Light), typeof(CustomRenderPipelineAsset))]
class CustomLightEditor : LightEditor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		if (
			!settings.lightType.hasMultipleDifferentValues &&
			(LightType)settings.lightType.enumValueIndex == LightType.Spot
		)
		{
			settings.DrawInnerAndOuterSpotAngle();
			settings.ApplyModifiedProperties();
		}
	}
}