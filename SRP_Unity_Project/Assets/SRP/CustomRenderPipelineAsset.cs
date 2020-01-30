using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

//在编辑器环境下，加载编辑器所需的资源操作
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

namespace Kata01
{
   // SRP中的裁剪设置
    //SRP中的过滤设置
    //SRP中的渲染设置
    //编写自己的着色器和管线配合

    public class CustomRenderPipelineAsset : RenderPipelineAsset
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Render Pipeline/Kata01/Pipeline Asset")]
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
