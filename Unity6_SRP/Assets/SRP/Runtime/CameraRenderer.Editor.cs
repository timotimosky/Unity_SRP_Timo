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
        //���������ǽ���֧�ֵġ�����Shader���������������Ⱦ����Ϊ���ǲ�����������Ⱦ˳������Ҫ���ľ��ǽ���չ�ֳ�����
        //���ʹ��DrawRendererSettings��SetOverrideMaterial��������Unity���õ�error shader������Ⱦ��
        // DrawRendererSettings֮����ʹ�á�ForwardBase����ΪPass Name������ΪĿǰ���ǵ�SRPֻ֧��ǰ����գ���Ĭ�ϵı�����ɫ���������Pass�ģ�
        // ������뽫����Shader Pass��ȷ��Ϊ����Shader��ʾ��Ҳ����SetShaderPassName������ӡ�

        // Unity��Ĭ�ϱ�����ɫ������ForwardBaseͨ������ͨ��������һ��������Ⱦͨ�������ǿ���ʹ������ʶ�������Ĭ�Ϲܵ�һ��ʹ�õĲ��ʵĶ���
        //ͨ���µĻ�ͼ����ѡ���ͨ�������������µ�Ĭ���˾�����һ��������Ⱦ�����ǲ��ں��������벻͸����Ⱦ����͸����Ⱦ������Ϊ������Ȼ��Ч��
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

        //���ƴ���
        //�������ǵĹܵ���֧��δ��ɫ����ɫ������˲�����Ⱦʹ�ò�ͬ��ɫ���Ķ��󣬴Ӷ�ʹ���ǲ��ɼ�������������ȷ�ģ�
        //�����ڸ���ĳЩ����ʹ�ô�����ɫ������ʵ���������ʹ��Unity�Ĵ�����ɫ�����ӻ���Щ�����ǽ��Ǻܺõģ�
        //���������ʾΪ���Բ���ȷ�����ɫ��״��������DrawDefaultPipelineΪ�����һ��ר�÷��������а���һ�������ĺ�һ��camera������
        //�ڻ���͸����״֮�����ǽ�������������

        //��DrawDefaultPipeline�ڱ༭���е��á�һ�ַ�����ͨ��Conditional��÷���������ԡ�
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

            //�������ǽ��ڹܵ���֧��δ�����Ĳ��ʣ�������ǽ�ʹ��Unity��Ĭ��δ����ͨ������ͨ����SRPDefaultUnlit��ʶ��
            // new FilteringSettings(RenderQueueRange.opaque, -1);
            var filterSettings = new FilteringSettings();
            SortingSettings sortSet = new SortingSettings(camera) { };//{ criteria = SortingCriteria.CommonOpaque };
                                                                      //����ʹ�ú���light mode����Ӧshader��pass��tag�е�LightMode

            //�ɹ��ߵ�Unity��Ĭ�ϲ��ʲ��ᱻʶ��Ҳ����ShaderTagId==ForwardBase�Ĳ���
            //���������ܱ�ʶ���������ɫ����PrepassBase��Always��Vertex��VertexLMRGBM��VertexLM���޷��ú�ɫ��ǳ�������������Ҫ��ӽ������ú�ɫ�Ĵ���shader������
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
                //�������Ӽ�������ͼ��Ч��֮ǰ��֮��
                renderContext.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                renderContext.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        }

#endif
    }
}