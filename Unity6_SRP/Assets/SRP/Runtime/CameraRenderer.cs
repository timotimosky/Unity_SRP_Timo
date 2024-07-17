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
        //同样定义好最大平行光数量
        const int maxDirectionalLights = 4;
        //将灯光参数改为参数组
        //var _LightDir0 = Shader.PropertyToID("_LightDir0");
        //var _LightColor0 = Shader.PropertyToID("_LightColor0");    
        Vector4[] DLightColors = new Vector4[maxDirectionalLights];
        Vector4[] DLightDirections = new Vector4[maxDirectionalLights];


        //同样定义好最大点光数量
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
            //为了在场景视图中看到UI
            //尽管Unity帮我们适配了UI在游戏窗口中显示，但不会在场景窗口显示。
            //UI始终存在于场景窗口中的世界空间中，但是我们必须手动将其注入场景中。
            //为了避免游戏窗口中第二次添加UI。必须在cull之前完成此操作。我们仅在渲染场景窗口时才发出UI几何。
            //cameraType相机等于CameraType.SceneView的时候就是这种情况。
            if (camera.cameraType == CameraType.SceneView)
            {
                //以当前摄像机为参数来添加UI
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
#endif

            //避免没有任何可渲染的物体时，往下渲染
            if (!Cull(out CullingResults cullResults))
            {
                Debug.Log("没有可渲染物体");
                return;
            }

            Setup();


            //初始化一个RendererList
            // RendererListDesc desc = new RendererListDesc();
            // RendererList rendererList = renderContext.CreateRendererList(desc);

            //rendererList.add
            //2.初始化灯光信息到CommandBuffer指令
            //完整的基本的兰伯特光照
            InitLight(cullResults);

            ExecuteBuffer();

            //5.渲染物体的指令填充到上下文
            DrawVisibleGeometry(cullResults);
#if UNITY_EDITOR
            DrawErrorShaderObject(cullResults);
#endif
            //将这个renderlist加入commandBuffer
            // commandBuffer.DrawRendererList(rendererList);
            Submit();
        }

        //绘制可见的场景物体
        void DrawVisibleGeometry(CullingResults cullResults)
        {
            //过滤：决定使用哪些渲染器
            FilteringSettings filtSet = new FilteringSettings(RenderQueueRange.opaque, -1);
            //filtSet.renderQueueRange = RenderQueueRange.opaque;
            //filtSet.layerMask = -1;                


            //相机用于设置排序和剔除层，而DrawingSettings控制使用哪个着色器过程进行渲染。


            //确定是应用正交排序还是基于距离的排序
            //决定使用何种渲染排序顺序 对应shader里的	Tags{ "Queue" = "Geometry" } 这属性(不是这个单一属性)
            //opaque涵盖了从0到2500（包括2500）之间的渲染队列。


            // CommonOpaque排序时，大部分时候，对象从前到后绘制，这对于不透明的对象来说是理想的选择：
            // 如果某物最终被绘制在其他物后面，则可以跳过其隐藏的片段，从而加快渲染速度。
            // 常见的不透明排序选项还考虑了一些其他条件，包括渲染队列和材质。

            SortingSettings sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };

            //决定使用何种light mode，对应shader的pass的tag中的LightMode
            DrawingSettings drawingSettings = new DrawingSettings(BaseLitShaderTagId, sortingSettings);


            //1.绘制不透明物体
            renderContext.DrawRenderers(cullResults, ref drawingSettings, ref filtSet);


            // 天空盒在不透明的几何体之后绘制，early-z避免不必要的overdraw。但它会覆盖透明几何体。
            // 发生这种情况是因为透明着色器不会写入深度缓冲区。他们不会隐藏他们身后的任何东西，因为我们可以看穿他们。
            // 解决方案是首先绘制不透明的对象，然后是天空盒，然后才是透明的对象。
            renderContext.DrawSkybox(camera);


            //3.绘制透明物体
            //，RenderQueueRange.transparent在渲染天空盒之后，将队列范围更改为从2501到5000，包括5000，然后再次渲染。
            filtSet.renderQueueRange = RenderQueueRange.transparent;
            sortingSettings.criteria = SortingCriteria.CommonTransparent;


            //必须指出允许哪种着色器通道
            //由于我们仅在管道中支持未照明的材质，因此我们将使用Unity的默认未照明通道，该通道由SRPDefaultUnlit标识。
            drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);

            renderContext.DrawRenderers(cullResults, ref drawingSettings, ref filtSet);
        }


        //每个setpasscall，都要调用一次
        void ExecuteBuffer()
        {
            //3.复制CommandBuffer指令，填充到上下文
            renderContext.ExecuteCommandBuffer(commandBuffer);
            //CommandBuffer指令可以重用，所以我们手动清理下
            commandBuffer.Clear();
        }



        //将shader中需要的属性参数映射为ID，加速传参
        int D_LightDir = Shader.PropertyToID("_DLightDir");
        int D_LightColor = Shader.PropertyToID("_DLightColor");
        //在设置灯光参数ID下面增加相机参数ID：
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



        //剔除：拿到场景中的所有渲染器，然后剔除那些在摄像机视锥范围之外的渲染器。
        bool Cull( out CullingResults cullResults)
        {
            //渲染器：它是附着在游戏对象上的组件，可将它们转变为可以渲染的东西。通常是一个MeshRenderer组件。

            //首先判断相机是否支持渲染
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters cullParam))
            {
                cullParam.isOrthographic = false;

                //refout用作优化，以防止传递结构的副本，该副本相当大。cullParam是一个结构而不是一个对象是另一种优化，以防止内存分配。
                cullResults = renderContext.Cull(ref cullParam);
                return true;
            }
            cullResults = new CullingResults();
            return false;
        }

        void InitLight(CullingResults cullResults)
        {
            //在剪裁结果中获取灯光并进行参数获取
            var lights = cullResults.visibleLights;

            int dLightIndex = 0;
            int pLightIndex = 0;
            foreach (var light in lights)
            {
                //判断灯光类型
                if (light.lightType == LightType.Directional)
                {
                    //在限定的灯光数量下，获取参数    
                    if (dLightIndex < maxDirectionalLights)
                    {
                        //获取灯光参数,平行光朝向即为灯光Z轴方向。矩阵第一到三列分别为xyz轴项，第四列为位置。
                        Vector4 lightpos = light.localToWorldMatrix.GetColumn(2);
                        //这边获取的灯光的finalColor是灯光颜色乘上强度之后的值，也正好是shader需要的值
                        DLightColors[dLightIndex] = light.finalColor;
                        DLightDirections[dLightIndex] = -lightpos;
                        DLightDirections[dLightIndex].w = 0;//方向光的第四个值(W值)为0，点为1.
                        dLightIndex++;
                    }
                }
                else
                {
                    if (light.lightType != LightType.Point)
                    {
                        //其他类型光源部分
                        continue;
                    }
                    else
                    {
                        if (pLightIndex < maxPointLights)
                        {
                            PLightColors[pLightIndex] = light.finalColor;
                            //将点光源的距离设置塞到颜色的A通道
                            PLightColors[pLightIndex].w = light.range;
                            //矩阵第4列为位置
                            PLightPos[pLightIndex] = light.localToWorldMatrix.GetColumn(3);
                            pLightIndex++;
                        }
                    }
                }
            }

            //传入相机参数。注意是世界空间位置。
            Vector4 cameraPos = camera.transform.position;
            commandBuffer.SetGlobalVector(_CameraPos, cameraPos);

            //利用CommandBuffer进将灯光参数组传入Shader           
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
            //如果不在最前面设置相机，那么后续的ClearRenderTarget将使用draw GL（执行一次空绘制）来清理，比较低效，需要耗费一次set passCall
            //在最前面设置相机参数后，，则可以使用直接 clear(depth + stencil)，清理camera的深度 +模板
            //设置渲染相关相机参数,包含相机的各个矩阵和剪裁平面等
            renderContext.SetupCameraProperties(camera);

            //清除渲染目标
            //无论我们绘制什么，最终都会渲染到相机的渲染目标，默认情况下是帧缓冲区，但也可能是渲染纹理。上一帧的缓冲区或者纹理，不会自动失效
            //之前被绘制到该目标的任何内容仍然存在，这可能会干扰我们现在渲染的图像。
            //为了保证正确的渲染，我们必须清除渲染目标以摆脱其旧内容。
            //CommandBuffer.ClearRenderTarget至少需要三个参数。前两个指示是否应清除深度和颜色数据，这两者都是如此。第三个参数是用于清除的颜色
            //2.设置渲染目标：是否保留深度、背景色。
            var flags = camera.clearFlags;
            commandBuffer.ClearRenderTarget((flags & CameraClearFlags.Depth) != 0, (flags & CameraClearFlags.Color) != 0, camera.backgroundColor);


            //每一次 commandBuffer填充了，必须执行，才有效，不是光执行ClearRenderTarget就可以了
            ExecuteBuffer();

        }

        void Submit()
        {
            commandBuffer.EndSample(sampleName);
            //6.渲染上下文提交gpu
            renderContext.Submit();
        }
    }
}
