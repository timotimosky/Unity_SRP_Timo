using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using Conditional = System.Diagnostics.ConditionalAttribute;

//这里也显示出了SRP的第一个好处，虽然都是前向渲染，但DrawCall明显变少了。非SRP虽然也可以单Pass多光源，但除平行光外，
//默认其他4盏光是不会有高光效果的(除非自己改写，或者用它默认传参自己实现)，而且也最多只支持这五盏光，再多，只能分Pass渲染。
namespace Tiny_RenderPipeline
{
    //设计渲染管线和着色器BaseDirLit，使得渲染管线将光源的信息传递给着色器，并在着色器中使用光源信息进行光照效果绘制。


    //每一次需要自定义渲染管线的时候都需要继承于这个RenderPipeline类
    //在一个游戏里，可以写多条渲染管线，并且按照需要在它们之间切换。
    public class CustomRenderPipeline : RenderPipeline
    {
        //我们定义了一个CommandBuffer，将其作为指令的记录表。Unity使用“先记录，后执行”的策略实现渲染管线，
        //就好比我们去餐馆吃饭，可能花了好长时间才把菜点完（比如选择困难症），然后一旦提交给厨房，会一次性把菜做好。
        //Unity的延迟执行体现在CommandBuffer和ScriptableRenderContext的设计中，这两个对象都充当我们的“菜单”。
        //将需要执行的执行记录在菜单上以后，
        //可以使用ScriptableRenderContext.ExecuteCommandBuffer和ScriptableRenderContext.Submit来提交CommandBuffer和ScriptableRenderContext。
        public CommandBuffer myCommandBuffer;

        //执行所有的渲染 
        protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
        {

        }
        //Camera[]需要为每帧分配内存，因此引入List<Camera> 替代。
        protected override void Render(ScriptableRenderContext renderContext, List<Camera> cameras)
        {
            RenderAllCameras(renderContext, cameras);
        }

        //上述代码是将不支持的“错误Shader”的物体于最后渲染，因为我们不关心它的渲染顺序，我们要做的就是将它展现出来，
        //因此使用DrawRendererSettings的SetOverrideMaterial方法，用Unity内置的error shader进行渲染。
        // DrawRendererSettings之所以使用“ForwardBase”作为Pass Name，是因为目前我们的SRP只支持前向光照，而默认的表面着色器是有这个Pass的，
        // 如果还想将其他Shader Pass明确作为错误Shader提示，也可用SetShaderPassName方法添加。

        // Unity的默认表面着色器具有ForwardBase通道，该通道用作第一个正向渲染通道。我们可以使用它来识别具有与默认管道一起使用的材质的对象。
        //通过新的绘图设置选择该通道，并将其与新的默认滤镜设置一起用于渲染。我们不在乎排序或分离不透明渲染器和透明渲染器，因为它们仍然无效。
        Material errorMaterial;

        static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
        };


        //仅DrawDefaultPipeline在编辑器中调用。一种方法是通过Conditional向该方法添加属性。
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void DrawErrorShaderObject(CullingResults cullResults, ScriptableRenderContext renderContext, Camera camera)
        {
            if (errorMaterial == null)
            {
                Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
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

        //剔除：拿到场景中的所有渲染器，然后剔除那些在摄像机视锥范围之外的渲染器。
        bool Cull(Camera camera, ScriptableRenderContext renderContext, out CullingResults cullResults)
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

        //绘制可见的场景物体
        void DrawVisibleGeometry(CullingResults cullResults, Camera camera, ScriptableRenderContext renderContext)
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

            SortingSettings sortSet = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };

            //决定使用何种light mode，对应shader的pass的tag中的LightMode
            DrawingSettings drawSet = new DrawingSettings(BaseLitShaderTagId, sortSet);


            //1.绘制不透明物体
            renderContext.DrawRenderers(cullResults, ref drawSet, ref filtSet);


            // 天空盒在不透明的几何体之后绘制，early-z避免不必要的overdraw。但它会覆盖透明几何体。
            // 发生这种情况是因为透明着色器不会写入深度缓冲区。他们不会隐藏他们身后的任何东西，因为我们可以看穿他们。
            // 解决方案是首先绘制不透明的对象，然后是天空盒，然后才是透明的对象。
            renderContext.DrawSkybox(camera);


            //3.绘制透明物体
            //，RenderQueueRange.transparent在渲染天空盒之后，将队列范围更改为从2501到5000，包括5000，然后再次渲染。
            filtSet.renderQueueRange = RenderQueueRange.transparent;
            sortSet.criteria = SortingCriteria.CommonTransparent;


            //必须指出允许哪种着色器通道
            //由于我们仅在管道中支持未照明的材质，因此我们将使用Unity的默认未照明通道，该通道由SRPDefaultUnlit标识。
            drawSet = new DrawingSettings(unlitShaderTagId, sortSet);

            renderContext.DrawRenderers(cullResults, ref drawSet, ref filtSet);
        }



        void InitLight(CullingResults cullResults, Camera camera, ScriptableRenderContext renderContext)
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
            myCommandBuffer.SetGlobalVector(_CameraPos, cameraPos);

            //利用CommandBuffer进将灯光参数组传入Shader           
            myCommandBuffer.SetGlobalVectorArray(D_LightColor, DLightColors);
            myCommandBuffer.SetGlobalVectorArray(D_LightDir, DLightDirections);


            myCommandBuffer.SetGlobalVectorArray(_PLightColor, PLightColors);
            myCommandBuffer.SetGlobalVectorArray(_PLightPos, PLightPos);
        }

        public string sampleName = "Render camera";
        void Setup(Camera camera, ScriptableRenderContext renderContext)
        {
            sampleName = "Render camera =>" + camera.name;
            myCommandBuffer.BeginSample(sampleName);

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
            myCommandBuffer.ClearRenderTarget((flags & CameraClearFlags.Depth) != 0, (flags & CameraClearFlags.Color) != 0, camera.backgroundColor);


            //每一次 myCommandBuffer填充了，必须执行，才有效，不是光执行ClearRenderTarget就可以了
            ExecuteBuffer(camera, renderContext);

        }

        void Submit(Camera camera, ScriptableRenderContext renderContext)
        {
            myCommandBuffer.EndSample(sampleName);
            //6.渲染上下文提交gpu
            renderContext.Submit();
        }

        //每个setpasscall，都要调用一次
        void ExecuteBuffer(Camera camera, ScriptableRenderContext renderContext)
        {
            //3.复制CommandBuffer指令，填充到上下文
            renderContext.ExecuteCommandBuffer(myCommandBuffer);
            //CommandBuffer指令可以重用，所以我们手动清理下
            myCommandBuffer.Clear();
        }

        void RenderSingleCamera(Camera camera, ScriptableRenderContext renderContext)
        {

            //避免没有任何可渲染的物体时，往下渲染
            if (!Cull(camera, renderContext, out CullingResults cullResults))
            {
                Debug.Log("没有可渲染物体");
                return;
            }

            Setup(camera, renderContext);

#if UNITY_EDITOR
            //为了在场景视图中看到UI
            //尽管Unity帮我们适配了UI在游戏窗口中显示，但不会在场景窗口显示。
            //UI始终存在于场景窗口中的世界空间中，但是我们必须手动将其注入场景中。
            //通过调用static ScriptableRenderContext.EmitWorldGeometryForSceneView方法（以当前摄像机为参数）来添加UI 。
            //必须在cull之前完成此操作。
            //为了避免游戏窗口中第二次添加UI。我们仅在渲染场景窗口时才发出UI几何。
            //cameraType相机等于CameraType.SceneView的时候就是这种情况。
            if (camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
#endif
            //初始化一个RendererList
            // RendererListDesc desc = new RendererListDesc();
            // RendererList rendererList = renderContext.CreateRendererList(desc);
            renderContext.DrawSkybox(camera);
            //rendererList.add
            //2.初始化灯光信息到CommandBuffer指令
            //完整的基本的兰伯特光照
            InitLight(cullResults, camera, renderContext);

            ExecuteBuffer(camera, renderContext);

            //5.渲染物体的指令填充到上下文
            DrawVisibleGeometry(cullResults, camera, renderContext);
            //绘制错误
            //由于我们的管道仅支持未着色的着色器，因此不会渲染使用不同着色器的对象，从而使它们不可见。尽管这是正确的，
            //但它掩盖了某些对象使用错误着色器的事实。如果我们使用Unity的错误着色器可视化这些对象，那将是很好的，
            //因此它们显示为明显不正确的洋红色形状。让我们DrawDefaultPipeline为此添加一个专用方法，其中包含一个上下文和一个camera参数。
            //在绘制透明形状之后，我们将在最后调用它。
            DrawErrorShaderObject(cullResults, renderContext, camera);

            //将这个renderlist加入myCommandBuffer
            // myCommandBuffer.DrawRendererList(rendererList);
            Submit(camera, renderContext);
        }

        //将shader中需要的属性参数映射为ID，加速传参
        int D_LightDir = Shader.PropertyToID("_DLightDir");
        int D_LightColor = Shader.PropertyToID("_DLightColor");
        //在设置灯光参数ID下面增加相机参数ID：
        int _CameraPos = Shader.PropertyToID("_CameraPos");

        int _PLightPos = Shader.PropertyToID("_PLightPos");
        int _PLightColor = Shader.PropertyToID("_PLightColor");


        //Render函数用于每一帧执行所有的渲染，
        //Render函数接受两个参数：第一个是被称为ScriptableRenderContext的新概念，我们会在后面介绍，
        //第二个是一个相机数组，包含了所有需要渲染的相机列表。我们一般需要针对列表里每一个相机运行一次管线，
        //但是针对相机种类的不同，可能会使用不同的渲染流程。
        protected void RenderAllCameras(ScriptableRenderContext renderContext, List<Camera> cameras)
        {

            //渲染开始后，创建CommandBuffer;
            if (myCommandBuffer == null)
                myCommandBuffer = new CommandBuffer() { name = "First SRP Render CommandBuffer" };

            //所有相机开始逐次渲染
            for (int i = 0; i < cameras.Count; ++i) //不要使用foreach，以后要对摄像机排序
            {
                var camera = cameras[i];
                RenderSingleCamera(camera, renderContext);
            }
        }

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

        //在不继续渲染的时候（例如我们切换到了另一个渲染管线）对当前管线进行现场清理。
        protected override void Dispose(bool dis)
        {
            base.Dispose(dis);
            if (myCommandBuffer != null)
            {
                myCommandBuffer.Dispose();//释放CommandBuffer
                myCommandBuffer = null;
            }
        }

    }

}
