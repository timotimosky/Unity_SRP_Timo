using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Tiny_RenderPipeline
{
    public partial class CameraRenderer
    {
        Camera camera;
        ScriptableRenderContext renderContext;
        CommandBuffer commandBuffer;
        //ͬ����������ƽ�й�����
        const int maxDirectionalLights = 4;
        //���ƹ������Ϊ������
        //var _LightDir0 = Shader.PropertyToID("_LightDir0");
        //var _LightColor0 = Shader.PropertyToID("_LightColor0");    
        Vector4[] DLightColors = new Vector4[maxDirectionalLights];
        Vector4[] DLightDirections = new Vector4[maxDirectionalLights];


        //ͬ����������������
        const int maxPointLights = 4;
        Vector4[] PLightColors = new Vector4[maxPointLights];
        Vector4[] PLightPos = new Vector4[maxPointLights];

        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        static ShaderTagId BaseLitShaderTagId = new ShaderTagId("BaseLit");


        public void RenderSingleCamera(Camera camera, ScriptableRenderContext renderContext, CommandBuffer mCommandBuffer)
        {
            this.camera = camera;
            this.renderContext = renderContext;
            this.commandBuffer = mCommandBuffer;

#if UNITY_EDITOR
            //Ϊ���ڳ�����ͼ�п���UI
            //����Unity������������UI����Ϸ��������ʾ���������ڳ���������ʾ��
            //UIʼ�մ����ڳ��������е�����ռ��У��������Ǳ����ֶ�����ע�볡���С�
            //Ϊ�˱�����Ϸ�����еڶ������UI��������cull֮ǰ��ɴ˲��������ǽ�����Ⱦ��������ʱ�ŷ���UI���Ρ�
            //cameraType�������CameraType.SceneView��ʱ��������������
            if (camera.cameraType == CameraType.SceneView)
            {
                //�Ե�ǰ�����Ϊ���������UI
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
#endif

            //����û���κο���Ⱦ������ʱ��������Ⱦ
            if (!Cull(out CullingResults cullResults))
            {
                Debug.Log("û�п���Ⱦ����");
                return;
            }

            Setup();


            //��ʼ��һ��RendererList
            // RendererListDesc desc = new RendererListDesc();
            // RendererList rendererList = renderContext.CreateRendererList(desc);

            //rendererList.add
            //2.��ʼ���ƹ���Ϣ��CommandBufferָ��
            //�����Ļ����������ع���
            InitLight(cullResults);

            ExecuteBuffer();

            //5.��Ⱦ�����ָ����䵽������
            DrawVisibleGeometry(cullResults);
#if UNITY_EDITOR
            DrawErrorShaderObject(cullResults);
#endif
            //�����renderlist����commandBuffer
            // commandBuffer.DrawRendererList(rendererList);
            Submit();
        }

        //���ƿɼ��ĳ�������
        void DrawVisibleGeometry(CullingResults cullResults)
        {
            //���ˣ�����ʹ����Щ��Ⱦ��
            FilteringSettings filtSet = new FilteringSettings(RenderQueueRange.opaque, -1);
            //filtSet.renderQueueRange = RenderQueueRange.opaque;
            //filtSet.layerMask = -1;                


            //�����������������޳��㣬��DrawingSettings����ʹ���ĸ���ɫ�����̽�����Ⱦ��


            //ȷ����Ӧ�����������ǻ��ھ��������
            //����ʹ�ú�����Ⱦ����˳�� ��Ӧshader���	Tags{ "Queue" = "Geometry" } ������(���������һ����)
            //opaque�����˴�0��2500������2500��֮�����Ⱦ���С�


            // CommonOpaque����ʱ���󲿷�ʱ�򣬶����ǰ������ƣ�����ڲ�͸���Ķ�����˵�������ѡ��
            // ���ĳ�����ձ���������������棬��������������ص�Ƭ�Σ��Ӷ��ӿ���Ⱦ�ٶȡ�
            // �����Ĳ�͸������ѡ�������һЩ����������������Ⱦ���кͲ��ʡ�

            SortingSettings sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };

            //����ʹ�ú���light mode����Ӧshader��pass��tag�е�LightMode
            DrawingSettings drawingSettings = new DrawingSettings(BaseLitShaderTagId, sortingSettings);


            //1.���Ʋ�͸������
            renderContext.DrawRenderers(cullResults, ref drawingSettings, ref filtSet);


            // ��պ��ڲ�͸���ļ�����֮����ƣ�early-z���ⲻ��Ҫ��overdraw�������Ḳ��͸�������塣
            // ���������������Ϊ͸����ɫ������д����Ȼ����������ǲ����������������κζ�������Ϊ���ǿ��Կ������ǡ�
            // ������������Ȼ��Ʋ�͸���Ķ���Ȼ������պУ�Ȼ�����͸���Ķ���
            renderContext.DrawSkybox(camera);


            //3.����͸������
            //��RenderQueueRange.transparent����Ⱦ��պ�֮�󣬽����з�Χ����Ϊ��2501��5000������5000��Ȼ���ٴ���Ⱦ��
            filtSet.renderQueueRange = RenderQueueRange.transparent;
            sortingSettings.criteria = SortingCriteria.CommonTransparent;


            //����ָ������������ɫ��ͨ��
            //�������ǽ��ڹܵ���֧��δ�����Ĳ��ʣ�������ǽ�ʹ��Unity��Ĭ��δ����ͨ������ͨ����SRPDefaultUnlit��ʶ��
            drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);

            renderContext.DrawRenderers(cullResults, ref drawingSettings, ref filtSet);
        }


        //ÿ��setpasscall����Ҫ����һ��
        void ExecuteBuffer()
        {
            //3.����CommandBufferָ���䵽������
            renderContext.ExecuteCommandBuffer(commandBuffer);
            //CommandBufferָ��������ã����������ֶ�������
            commandBuffer.Clear();
        }



        //��shader����Ҫ�����Բ���ӳ��ΪID�����ٴ���
        int D_LightDir = Shader.PropertyToID("_DLightDir");
        int D_LightColor = Shader.PropertyToID("_DLightColor");
        //�����õƹ����ID���������������ID��
        int _CameraPos = Shader.PropertyToID("_CameraPos");

        int _PLightPos = Shader.PropertyToID("_PLightPos");
        int _PLightColor = Shader.PropertyToID("_PLightColor");




        RenderTexture shadowMap;
        //void RenderShadows(ScriptableRenderContext context)
        //{
        //    shadowMap = RenderTexture.GetTemporary(512, 512, 16, RenderTextureFormat.Shadowmap);
        //    shadowMap.filterMode = FilterMode.Bilinear;
        //    shadowMap.wrapMode = TextureWrapMode.Clamp;
        //    CoreUtils.SetRenderTarget(shadowBuffer, shadowMap, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.Depth);
        //    shadowBuffer.BeginSample("Render Shadows");
        //    context.ExecuteCommandBuffer(shadowBuffer);
        //    shadowBuffer.Clear();


        //    shadowBuffer.EndSample("Render shadows");
        //    context.ExecuteCommandBuffer(shadowBuffer);
        //    shadowBuffer.Clear();
        //}



        //�޳����õ������е�������Ⱦ����Ȼ���޳���Щ���������׶��Χ֮�����Ⱦ����
        bool Cull( out CullingResults cullResults)
        {
            //��Ⱦ�������Ǹ�������Ϸ�����ϵ�������ɽ�����ת��Ϊ������Ⱦ�Ķ�����ͨ����һ��MeshRenderer�����

            //�����ж�����Ƿ�֧����Ⱦ
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters cullParam))
            {
                cullParam.isOrthographic = false;

                //refout�����Ż����Է�ֹ���ݽṹ�ĸ������ø����൱��cullParam��һ���ṹ������һ����������һ���Ż����Է�ֹ�ڴ���䡣
                cullResults = renderContext.Cull(ref cullParam);
                return true;
            }
            cullResults = new CullingResults();
            return false;
        }

        void InitLight(CullingResults cullResults)
        {
            //�ڼ��ý���л�ȡ�ƹⲢ���в�����ȡ
            var lights = cullResults.visibleLights;

            int dLightIndex = 0;
            int pLightIndex = 0;
            foreach (var light in lights)
            {
                //�жϵƹ�����
                if (light.lightType == LightType.Directional)
                {
                    //���޶��ĵƹ������£���ȡ����    
                    if (dLightIndex < maxDirectionalLights)
                    {
                        //��ȡ�ƹ����,ƽ�й⳯��Ϊ�ƹ�Z�᷽�򡣾����һ�����зֱ�Ϊxyz���������Ϊλ�á�
                        Vector4 lightpos = light.localToWorldMatrix.GetColumn(2);
                        //��߻�ȡ�ĵƹ��finalColor�ǵƹ���ɫ����ǿ��֮���ֵ��Ҳ������shader��Ҫ��ֵ
                        DLightColors[dLightIndex] = light.finalColor;
                        DLightDirections[dLightIndex] = -lightpos;
                        DLightDirections[dLightIndex].w = 0;//�����ĵ��ĸ�ֵ(Wֵ)Ϊ0����Ϊ1.
                        dLightIndex++;
                    }
                }
                else
                {
                    if (light.lightType != LightType.Point)
                    {
                        //�������͹�Դ����
                        continue;
                    }
                    else
                    {
                        if (pLightIndex < maxPointLights)
                        {
                            PLightColors[pLightIndex] = light.finalColor;
                            //�����Դ�ľ�������������ɫ��Aͨ��
                            PLightColors[pLightIndex].w = light.range;
                            //�����4��Ϊλ��
                            PLightPos[pLightIndex] = light.localToWorldMatrix.GetColumn(3);
                            pLightIndex++;
                        }
                    }
                }
            }

            //�������������ע��������ռ�λ�á�
            Vector4 cameraPos = camera.transform.position;
            commandBuffer.SetGlobalVector(_CameraPos, cameraPos);

            //����CommandBuffer�����ƹ�����鴫��Shader           
            commandBuffer.SetGlobalVectorArray(D_LightColor, DLightColors);
            commandBuffer.SetGlobalVectorArray(D_LightDir, DLightDirections);


            commandBuffer.SetGlobalVectorArray(_PLightColor, PLightColors);
            commandBuffer.SetGlobalVectorArray(_PLightPos, PLightPos);
        }
        public string sampleName = "Render camera";

        void Setup()
        {
            sampleName = "Render camera =>" + camera.name;
            commandBuffer.BeginSample(sampleName);
            commandBuffer.name = sampleName;
            //���������ǰ�������������ô������ClearRenderTarget��ʹ��draw GL��ִ��һ�οջ��ƣ��������Ƚϵ�Ч����Ҫ�ķ�һ��set passCall
            //����ǰ��������������󣬣������ʹ��ֱ�� clear(depth + stencil)������camera����� +ģ��
            //������Ⱦ����������,��������ĸ�������ͼ���ƽ���
            renderContext.SetupCameraProperties(camera);

            //�����ȾĿ��
            //�������ǻ���ʲô�����ն�����Ⱦ���������ȾĿ�꣬Ĭ���������֡����������Ҳ��������Ⱦ������һ֡�Ļ������������������Զ�ʧЧ
            //֮ǰ�����Ƶ���Ŀ����κ�������Ȼ���ڣ�����ܻ��������������Ⱦ��ͼ��
            //Ϊ�˱�֤��ȷ����Ⱦ�����Ǳ��������ȾĿ���԰���������ݡ�
            //CommandBuffer.ClearRenderTarget������Ҫ����������ǰ����ָʾ�Ƿ�Ӧ�����Ⱥ���ɫ���ݣ������߶�����ˡ������������������������ɫ
            //2.������ȾĿ�꣺�Ƿ�����ȡ�����ɫ��
            var flags = camera.clearFlags;
            commandBuffer.ClearRenderTarget((flags & CameraClearFlags.Depth) != 0, (flags & CameraClearFlags.Color) != 0, camera.backgroundColor);


            //ÿһ�� commandBuffer����ˣ�����ִ�У�����Ч�����ǹ�ִ��ClearRenderTarget�Ϳ�����
            ExecuteBuffer();

        }

        void Submit()
        {
            commandBuffer.EndSample(sampleName);
            //6.��Ⱦ�������ύgpu
            renderContext.Submit();
        }
    }
}
