using UnityEngine;
using UnityEngine.Rendering;
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
        CommandBuffer myCommandBuffer;

        // Dispose函数用于在不继续渲染的时候（例如我们切换到了另一个渲染管线）对当前管线进行现场清理。
        //在Dispose函数里，我们简单地释放CommandBuffer（使用CommandBuffer的Dispose）；而在Render函数里，我们执行所有的渲染。
        protected override void Dispose(bool dis)
        {
            base.Dispose(dis);
            if (myCommandBuffer != null)
            {
                myCommandBuffer.Dispose();
                myCommandBuffer = null;
            }
        }

        protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
        {
            //Render_01_sky_box( renderContext,cameras);
            Render_mutil_light(renderContext, cameras);
        }


        //Render函数用于每一帧执行所有的渲染，
        //Render函数接受两个参数：第一个是被称为ScriptableRenderContext的新概念，我们会在后面介绍，
        //第二个是一个相机数组，包含了所有需要渲染的相机列表。我们一般需要针对列表里每一个相机运行一次管线，
        //但是针对相机种类的不同，可能会使用不同的渲染流程。
        protected  void Render_01_sky_box(ScriptableRenderContext renderContext, Camera[] cameras)
        {
            if (myCommandBuffer == null) myCommandBuffer = new CommandBuffer();

            //对于每一个相机执行操作。
            foreach (var camera in cameras)
            {
                //将上下文设置为当前相机的上下文。
                renderContext.SetupCameraProperties(camera);
                //设置渲染目标的颜色为蓝色。
                myCommandBuffer.ClearRenderTarget(true, true, Color.blue);
                //提交指令队列至当前context处理。
                renderContext.ExecuteCommandBuffer(myCommandBuffer);
                //清空当前指令队列。
                myCommandBuffer.Clear();
                //开始执行上下文
                renderContext.Submit();
            }
        }

        //上述代码是将不支持的“错误Shader”的物体于最后渲染，因为我们不关心它的渲染顺序，我们要做的就是将它展现出来，
        //因此使用DrawRendererSettings的SetOverrideMaterial方法，用Unity内置的error shader进行渲染。
        // DrawRendererSettings之所以使用“ForwardBase”作为Pass Name，是因为目前我们的SRP只支持前向光照，而默认的表面着色器是有这个Pass的，
        // 如果还想将其他Shader Pass明确作为错误Shader提示，也可用SetShaderPassName方法添加。

       // Unity的默认表面着色器具有ForwardBase通道，该通道用作第一个正向渲染通道。我们可以使用它来识别具有与默认管道一起使用的材质的对象。
        //通过新的绘图设置选择该通道，并将其与新的默认滤镜设置一起用于渲染。我们不在乎排序或分离不透明渲染器和透明渲染器，因为它们仍然无效。
        Material errorMaterial;

        //仅DrawDefaultPipeline在编辑器中调用。一种方法是通过Conditional向该方法添加属性。
        [Conditional("UNITY_EDITOR"),Conditional("DEVELOPMENT_BUILD") ]
        private void DrawErrorShaderObject(CullingResults cullResults,ScriptableRenderContext renderContext, Camera camera)
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
            SortingSettings sortSet = new SortingSettings(camera) {};//{ criteria = SortingCriteria.CommonOpaque };
            //决定使用何种light mode，对应shader的pass的tag中的LightMode
            DrawingSettings drawSet = new DrawingSettings(new ShaderTagId("ForwardBase"), sortSet);

            //涵盖了Unity提供的所有着色器
            //现在，使用不受支持的材料的对象显然会显示为不正确。但这仅适用于Unity的默认管道材质，其着色器可以ForwardBase通过。
            //我们还可以使用其他遍历来识别其他内置着色器，特别是PrepassBase，Always，Vertex，VertexLMRGBM和VertexLM。
            //幸运的是，可以通过调用将多个遍添加到绘图设置中SetShaderPassName。名称是此方法的第二个参数。它的第一个参数是控制通行证绘制顺序的索引。
            //我们不在乎，所以任何订单都可以。通过构造函数提供的通道始终具有零索引，只需增加索引即可获得更多通道。
            drawSet.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
            drawSet.SetShaderPassName(2, new ShaderTagId("Always"));
            drawSet.SetShaderPassName(3, new ShaderTagId("Vertex"));
            drawSet.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
            drawSet.SetShaderPassName(5, new ShaderTagId("VertexLM"));
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

        //完整的基本的兰伯特光照脚本
        protected void Render_mutil_light(ScriptableRenderContext renderContext, Camera[] cameras)
        {

            //渲染开始后，创建CommandBuffer;
            if (myCommandBuffer == null)
                myCommandBuffer = new CommandBuffer() { name = "SRP Study CB" };

            //将shader中需要的属性参数映射为ID，加速传参
            var D_LightDir = Shader.PropertyToID("_DLightDir");
            var D_LightColor = Shader.PropertyToID("_DLightColor");
            //在设置灯光参数ID下面增加相机参数ID：
            var _CameraPos = Shader.PropertyToID("_CameraPos");

            var _PLightPos = Shader.PropertyToID("_PLightPos");
            var _PLightColor = Shader.PropertyToID("_PLightColor");

            //所有相机开始逐次渲染
            foreach (var camera in cameras)
            {
                myCommandBuffer.BeginSample("Render "+ camera.name);
 
                //设置渲染相关相机参数,包含相机的各个矩阵和剪裁平面等
                renderContext.SetupCameraProperties(camera);


                //清理myCommandBuffer，设置渲染目标的颜色为摄像机背景
                var flags = camera.clearFlags;

                //myCommandBuffer.ClearRenderTarget(true,true,camera.backgroundColor);

                myCommandBuffer.ClearRenderTarget((flags & CameraClearFlags.Depth) != 0,(flags & CameraClearFlags.Color) != 0,camera.backgroundColor);

#if UNITY_EDITOR
                //为了在场景视图中看到UI
                //尽管Unity帮我们适配了UI在游戏窗口中显示，但不会在场景窗口显示。
                //UI始终存在于场景窗口中的世界空间中，但是我们必须手动将其注入场景中。
                //通过调用static ScriptableRenderContext.EmitWorldGeometryForSceneView方法（以当前摄像机为参数）来添加UI 。
                //必须在cull之前完成此操作。
                //为了避免游戏窗口中第二次添加UI。我们仅在渲染场景窗口时才发出UI几何。
                //cameraType相机的等于时就是这种情况CameraType.SceneView。
                if (camera.cameraType == CameraType.SceneView)
                {
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
                }
#endif


                //剔除：拿到场景中的所有渲染器，然后剔除那些在摄像机视锥范围之外的渲染器。

                //渲染器：它是附着在游戏对象上的组件，可将它们转变为可以渲染的东西。通常是一个MeshRenderer组件。
                ScriptableCullingParameters cullParam = new ScriptableCullingParameters();
                camera.TryGetCullingParameters(out cullParam);
                cullParam.isOrthographic = false;

                //TODO：避免没有任何可渲染的物体时，往下渲染
                CullingResults cullResults = renderContext.Cull(ref cullParam);
                renderContext.DrawSkybox(camera);

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
                myCommandBuffer.EndSample("Render " + camera.name);
                //执行CommandBuffer中的指令
                renderContext.ExecuteCommandBuffer(myCommandBuffer);

                myCommandBuffer.Clear();

                //过滤：决定使用哪些渲染器
                FilteringSettings filtSet = new FilteringSettings(RenderQueueRange.opaque, -1);
                //filtSet.renderQueueRange = RenderQueueRange.opaque;
                //filtSet.layerMask = -1;                


                //相机用于设置排序和剔除层，而DrawingSettings控制使用哪个着色器过程进行渲染。

                //决定使用何种渲染排序顺序 对应shader里的	Tags{ "Queue" = "Geometry" } 这属性(不是这个单一属性)
                //opaque涵盖了从0到2500（包括2500）之间的渲染队列。
                SortingSettings sortSet = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
                //决定使用何种light mode，对应shader的pass的tag中的LightMode
                DrawingSettings drawSet = new DrawingSettings(new ShaderTagId("BaseLit"), sortSet);


                //1.绘制不透明物体
                renderContext.DrawRenderers(cullResults, ref drawSet, ref filtSet);

                //2.绘制天空球,在不透明物体之后绘制。early-z避免不必要的overdraw。
                renderContext.DrawSkybox(camera);




                //3.绘制透明物体
                //，RenderQueueRange.transparent在渲染天空盒之后，将队列范围更改为从2501到5000，包括5000，然后再次渲染。
                filtSet.renderQueueRange = RenderQueueRange.transparent;
                sortSet.criteria = SortingCriteria.CommonTransparent;
                //由于我们仅在管道中支持未照明的材质，因此我们将使用Unity的默认未照明通道，该通道由SRPDefaultUnlit标识。
                drawSet = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), sortSet);
                renderContext.DrawRenderers(cullResults, ref drawSet, ref filtSet);

                //绘制错误
                //由于我们的管道仅支持未着色的着色器，因此不会渲染使用不同着色器的对象，从而使它们不可见。尽管这是正确的，
                //但它掩盖了某些对象使用错误着色器的事实。如果我们使用Unity的错误着色器可视化这些对象，那将是很好的，
                //因此它们显示为明显不正确的洋红色形状。让我们DrawDefaultPipeline为此添加一个专用方法，其中包含一个上下文和一个camera参数。
                //在绘制透明形状之后，我们将在最后调用它。
                DrawErrorShaderObject(cullResults, renderContext, camera);


                //开始执行渲染内容
                renderContext.Submit();
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


    }
}
