#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

#endif
using UnityEngine;
using UnityEngine.Rendering;
//RP 资源的主要用途是为 Unity 提供一种获取负责渲染的管道对象实例的方法。资产本身只是一个句柄和存储设置的地方。
[CreateAssetMenu(fileName = "New Srp RenderPipeline Asset", menuName = "SRP/New Render Pipeline Asset")]
public class CustomRenderPipelineAsset : RenderPipelineAsset {

	[SerializeField]
	bool allowHDR = true;

	[SerializeField]
	bool
		useDynamicBatching = true,
		useGPUInstancing = true,
		useSRPBatcher = true,
		useLightsPerObject = true;

	[SerializeField]
	ShadowSettings shadows = default;

	[SerializeField]
	PostFXSettings postFXSettings = default;

#if UNITY_EDITOR
    [MenuItem("SRP/Create/Render Pipeline Asset")]
    static void CreateSrpPipeline()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateSrpPipelineAsset>(),  "Timo Srp Pipeline Asset.asset", null, null);
    }

    protected override RenderPipeline CreatePipeline () {
		return new CustomRenderPipeline(
			allowHDR, useDynamicBatching, useGPUInstancing, useSRPBatcher,
			useLightsPerObject, shadows, postFXSettings
		);
	}

    class CreateSrpPipelineAsset : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var instance = CreateInstance<CustomRenderPipelineAsset>();
            AssetDatabase.CreateAsset(instance, pathName);
        }
    }
#endif
}