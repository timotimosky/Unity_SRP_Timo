using UnityEngine.Rendering;
using UnityEngine;


//在编辑器环境下，加载编辑器所需的资源操作
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

namespace Tiny_RenderPipeline
{
    // SRP中的裁剪设置
    //SRP中的过滤设置
    //SRP中的渲染设置
    //编写自己的着色器和管线配合

    //RP 资源的主要用途是为 Unity 提供一种获取负责渲染的管道对象实例的方法。资产本身只是一个句柄和存储设置的地方。
    [CreateAssetMenu(fileName = "NewCustomRenderPipelineAsset", menuName = "Rendering/Custom Render Pipeline")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset
    {
#if UNITY_EDITOR
        [MenuItem("SRP/Create/Render Pipeline/Pipeline Asset")]
        static void CreateKata01Pipeline()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0, CreateInstance<CreateKata01PipelineAsset>(),
                "Kata01 Pipeline.asset", null, null);
        }

        class CreateKata01PipelineAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var instance = CreateInstance<CustomRenderPipelineAsset>();
                AssetDatabase.CreateAsset(instance, pathName);
            }
        }
#endif
        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipeline();
        }
    }
}
