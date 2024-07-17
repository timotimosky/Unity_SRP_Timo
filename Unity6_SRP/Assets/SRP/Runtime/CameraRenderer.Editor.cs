using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace Tiny_RenderPipeline
{
    public partial class CameraRenderer
    {
        partial void DrawGizmos();

        partial void DrawErrorShaderObject(CullingResults cullResults);
#if UNITY_EDITOR
        //上述代码是将不支持的“错误Shader”的物体于最后渲染，因为我们不关心它的渲染顺序，我们要做的就是将它展现出来，
        //因此使用DrawRendererSettings的SetOverrideMaterial方法，用Unity内置的error shader进行渲染。
        // DrawRendererSettings之所以使用“ForwardBase”作为Pass Name，是因为目前我们的SRP只支持前向光照，而默认的表面着色器是有这个Pass的，
        // 如果还想将其他Shader Pass明确作为错误Shader提示，也可用SetShaderPassName方法添加。

        // Unity的默认表面着色器具有ForwardBase通道，该通道用作第一个正向渲染通道。我们可以使用它来识别具有与默认管道一起使用的材质的对象。
        //通过新的绘图设置选择该通道，并将其与新的默认滤镜设置一起用于渲染。我们不在乎排序或分离不透明渲染器和透明渲染器，因为它们仍然无效。
        Material errorMaterial;

        //Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),
         new ShaderTagId("Opaque"),
        };

        //绘制错误
        //由于我们的管道仅支持未着色的着色器，因此不会渲染使用不同着色器的对象，从而使它们不可见。尽管这是正确的，
        //但它掩盖了某些对象使用错误着色器的事实。如果我们使用Unity的错误着色器可视化这些对象，那将是很好的，
        //因此它们显示为明显不正确的洋红色形状。让我们DrawDefaultPipeline为此添加一个专用方法，其中包含一个上下文和一个camera参数。
        //在绘制透明形状之后，我们将在最后调用它。

        //仅DrawDefaultPipeline在编辑器中调用。一种方法是通过Conditional向该方法添加属性。
        //  [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        partial void DrawErrorShaderObject(CullingResults cullResults)
        {
            if (errorMaterial == null)
            {
                Shader errorShader = Shader.Find("DJL/BoxShader");
                // Shader.Find("Hidden/InternalErrorShader");
                Debug.LogError("11111111");
                errorMaterial = new Material(errorShader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            //由于我们仅在管道中支持未照明的材质，因此我们将使用Unity的默认未照明通道，该通道由SRPDefaultUnlit标识。
            // new FilteringSettings(RenderQueueRange.opaque, -1);
            var filterSettings = new FilteringSettings();
            SortingSettings sortSet = new SortingSettings(camera) { };//{ criteria = SortingCriteria.CommonOpaque };
                                                                      //决定使用何种light mode，对应shader的pass的tag中的LightMode

            //旧管线的Unity的默认材质不会被识别，也就是ShaderTagId==ForwardBase的材质
            //但其他不能被识别的内置着色器（PrepassBase，Always，Vertex，VertexLMRGBM和VertexLM）无法用红色标记出来，所以我们要添加进来，用红色的错误shader来绘制
            var drawSet = new DrawingSettings(legacyShaderTagIds[1], sortSet);
            for (int i = 1; i < legacyShaderTagIds.Length; i++)
            {
                drawSet.SetShaderPassName(i, legacyShaderTagIds[i]);
            }
            drawSet.overrideMaterial = errorMaterial;
            drawSet.overrideMaterialPassIndex = 0;
            renderContext.DrawRenderers(cullResults, ref drawSet, ref filterSettings);
        }

        partial void DrawGizmos()
        {
            if (Handles.ShouldRenderGizmos())
            {
                //有两个子集，用于图像效果之前和之后
                renderContext.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                renderContext.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        }

#endif
    }
}