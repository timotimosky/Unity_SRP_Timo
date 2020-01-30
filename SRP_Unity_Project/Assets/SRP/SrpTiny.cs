using UnityEngine;
using UnityEngine.Rendering;


//这里也显示出了SRP的第一个好处，虽然都是前向渲染，但DrawCall明显变少了。非SRP虽然也可以单Pass多光源，但除平行光外，
//默认其他4盏光是不会有高光效果的(除非自己改写，或者用它默认传参自己实现)，而且也最多只支持这五盏光，再多，只能分Pass渲染。
namespace Kata01
{
    //设计渲染管线和着色器BaseDirLit，使得渲染管线将光源的信息传递给着色器，并在着色器中使用光源信息进行光照效果绘制。


    //每一次需要自定义渲染管线的时候都需要继承于这个RenderPipeline类
    //在一个游戏里，可以写多条渲染管线，并且按照需要在它们之间切换。
    public class CustomRenderPipeline : RenderPipeline
    {
        //我们定义了一个CommandBuffer，将其作为指令的记录表。Unity使用“先记录，后执行”的策略实现渲染管线，
        //就好比我们去餐馆吃饭，可能花了好长时间才把菜点完（比如选择困难症），然后一旦提交给厨房，会一次性把菜做好。
        //Unity的延迟执行体现在CommandBuffer和ScriptableRenderContext（很快就会讲到）的设计中，这两个对象都充当我们的“菜单”。
        //将需要执行的执行记录在菜单上以后，
        //可以使用ScriptableRenderContext.ExecuteCommandBuffer和ScriptableRenderContext.Submit来提交CommandBuffer和ScriptableRenderContext。
        CommandBuffer myCommandBuffer;

        //这个向量用于保存平行光方向。
        Vector3 _LightDir;

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
          //  Render_Light(renderContext, cameras);
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

//使用Unlit.shader新建两个材质，选择您喜欢的颜色。
//在场景里新建几个几何体，将材质应用在几何体上。

//设置相机的背景色为您喜欢的颜色。
  

//将Shader的Queue Tag从Geometry改成Transparent，看一下有什么变化。
//将Pass的Lightmode Tag从Unlit改成UnLit、UNLIT、unlit、unLit，看一下有什么变化，再改成Un-Lit，看一下有什么变化。
//分析一下这些现象的原因，并给出解释。并且避免出现类似错误^-^。

//本案例虽然只给出了一个Unlit的效果，但是请读者自由发挥，尝试为shader添加更多的效果，比如尝试给模型添加纹理贴图。在下一章节中，我们将开始为场景添加光照了，


        //完整的基本的兰伯特光照脚本
        //天空球要在不透明物体之后绘制。因为天空球肯定会被前景挡住，之后绘制会被深度检测优化，而避免不必要的overdraw。不透明一般是从前往后绘制，也是这个原理，
        //所以，利用unity自己的内置排序挺好，省的自己进行深度排序了。透明物体一般是从后往前绘制的，方便颜色混合。也有各种顺序无关的绘制方式，等等，以后有空再慢慢说。
        protected  void Render_Light(ScriptableRenderContext renderContext, Camera[] cameras)
        {

            //渲染开始后，创建CommandBuffer;
            if (myCommandBuffer == null) 
                myCommandBuffer = new CommandBuffer() { name = "SRP Study CB" };

            //将shader中需要的属性参数映射为ID，加速传参
            var _LightDir0 = Shader.PropertyToID("_DLightDir");
            var _LightColor0 = Shader.PropertyToID("_DLightColor");
            //在设置灯光参数ID下面增加相机参数ID：
            var _CameraPos = Shader.PropertyToID("_CameraPos");


            //同上一节，所有相机开始逐次渲染
            foreach (var camera in cameras)
            {
                renderContext.DrawSkybox(camera);


                //设置渲染相关相机参数,包含相机的各个矩阵和剪裁平面等

                //
                renderContext.SetupCameraProperties(camera);


                //清理myCommandBuffer，设置渲染目标的颜色为灰色。
                 myCommandBuffer.ClearRenderTarget(true, true, Color.gray);
                var flags = camera.clearFlags;
                //myCommandBuffer.ClearRenderTarget((flags & CameraClearFlags.Depth) != 0, (flags & CameraClearFlags.Color) != 0, camera.backgroundColor);
                renderContext.DrawSkybox(camera);

                //同上一节的剪裁
                ScriptableCullingParameters cullParam = new ScriptableCullingParameters();
                camera.TryGetCullingParameters(out cullParam);
                cullParam.isOrthographic = false;
                CullingResults cullResults = renderContext.Cull(ref cullParam);


   // 如果现在有两个不透明的物体叠在一起，我们先绘制哪个呢？如果我们先绘制远的那个，那么近的那个在渲染的时候就会重新绘制一次重叠区域。
              //  因此我们先绘制近的那个，再绘制远的那个物体时，重叠区域的像素会因为无法通过深度测试而不被渲染，大大降低了渲染压力
//Unity SRP中为我们提供了简单的排序方法，只需指定Sort Flag就能排序。对于不透明物体和透明物体，只需指定drawSettings.sorting.flags。
                // 我们的绘制顺序
                //1绘制不透明物体
                //2 绘制天空盒
                //3绘制透明物体
                //4.绘制错误shader的物体

                //在剪裁结果中获取灯光并进行参数获取
                var lights = cullResults.visibleLights;
                myCommandBuffer.name = "Render Lights";
                foreach (var light in lights)
                {
                    //判断灯光类型
                    if (light.lightType != LightType.Directional) continue;
                    //获取灯光参数,平行光朝向即为灯光Z轴方向。矩阵第一到三列分别为xyz轴项，第四列为位置。
                    Vector4 lightpos = light.localToWorldMatrix.GetColumn(2);
                    //灯光方向反向。默认管线中，unity提供的平行光方向也是灯光反向。光照计算决定
                    Vector4 lightDir = -lightpos;
                    //方向的第四个值(W值)为0，点为1.
                    lightDir.w = 0;
                    //这边获取的灯光的finalColor是灯光颜色乘上强度之后的值，也正好是shader需要的值
                    Color lightColor = light.finalColor;
                    //利用CommandBuffer进行参数传递。
                    myCommandBuffer.SetGlobalVector(_LightDir0, lightDir);
                    myCommandBuffer.SetGlobalColor(_LightColor0, lightColor);

                    //传入相机参数。注意是世界空间位置。
                    Vector4 cameraPos = camera.transform.position;
                    myCommandBuffer.SetGlobalVector(_CameraPos, cameraPos);
                }
                //执行CommandBuffer中的指令
                renderContext.ExecuteCommandBuffer(myCommandBuffer);
                myCommandBuffer.Clear();

                //同上节，过滤
                FilteringSettings filtSet = new FilteringSettings(RenderQueueRange.opaque, -1);
                //filtSet.renderQueueRange = RenderQueueRange.opaque;
                //filtSet.layerMask = -1;                

                //同上节，设置Renderer Settings
                //注意在构造的时候就需要传入Lightmode参数，对应shader的pass的tag中的LightMode
                SortingSettings sortSet = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
                DrawingSettings drawSet = new DrawingSettings(new ShaderTagId("BaseLit"), sortSet);

                //绘制物体
                renderContext.DrawRenderers(cullResults, ref drawSet, ref filtSet);

                //绘制天空球
               renderContext.DrawSkybox(camera);

                //绘制错误物体
                DrawErrorShaderObject(cullResults,renderContext, camera);

                //开始执行渲染内容
                renderContext.Submit();
            }
        }


       //上述代码是将不支持的“错误Shader”的物体于最后渲染，因为我们不关心它的渲染顺序，我们要做的就是将它展现出来，
            //因此使用DrawRendererSettings的SetOverrideMaterial方法，用Unity内置的error shader进行渲染。
       // DrawRendererSettings之所以使用“ForwardBase”作为Pass Name，是因为目前我们的SRP只支持前向光照，而默认的表面着色器是有这个Pass的，
           // 如果还想将其他Shader Pass明确作为错误Shader提示，也可用SetShaderPassName方法添加。
        Material errorMaterial;
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

            //var drawSettings = new DrawRendererSettings(camera, new ShaderPassName("ForwardBase"));
            //drawSettings.SetShaderPassName(1, new ShaderPassName("PrepassBase"));
            //drawSettings.SetShaderPassName(2, new ShaderPassName("Always"));
            //drawSettings.SetShaderPassName(3, new ShaderPassName("Vertex"));
            //drawSettings.SetShaderPassName(4, new ShaderPassName("VertexLMRGBM"));
            //drawSettings.SetShaderPassName(5, new ShaderPassName("VertexLM"));
            //drawSettings.SetOverrideMaterial(errorMaterial, 0);
            //var filterSettings = new FilterRenderersSettings(true);
            //renderContext.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

            // SortingSettings sortSet = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
            // DrawingSettings drawSet = new DrawingSettings(new ShaderTagId("ForwardBase"), sortSet);
            // drawSet.SetOverrideMaterial(errorMaterial, 0);
            // renderContext.DrawRenderers(cullResults, ref drawSet, errorFilter);
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
        //天空球要在不透明物体之后绘制。因为天空球肯定会被前景挡住，之后绘制会被深度检测优化，而避免不必要的overdraw。不透明一般是从前往后绘制，也是这个原理，
        //所以，利用unity自己的内置排序挺好，省的自己进行深度排序了。透明物体一般是从后往前绘制的，方便颜色混合。也有各种顺序无关的绘制方式，等等，以后有空再慢慢说。
        protected  void Render_mutil_light(ScriptableRenderContext renderContext, Camera[] cameras)
        {

            //渲染开始后，创建CommandBuffer;
            if (myCommandBuffer == null)
                myCommandBuffer = new CommandBuffer() { name = "SRP Study CB" };

            //将shader中需要的属性参数映射为ID，加速传参
            var _LightDir0 = Shader.PropertyToID("_DLightDir");
            var _LightColor0 = Shader.PropertyToID("_DLightColor");
            //在设置灯光参数ID下面增加相机参数ID：
            var _CameraPos = Shader.PropertyToID("_CameraPos");

            var _PLightPos = Shader.PropertyToID("_PLightPos");
            var _PLightColor = Shader.PropertyToID("_PLightColor");

            //同上一节，所有相机开始逐次渲染
            foreach (var camera in cameras)
            {
               // renderContext.DrawSkybox(camera);


                //设置渲染相关相机参数,包含相机的各个矩阵和剪裁平面等

                renderContext.SetupCameraProperties(camera);


                //清理myCommandBuffer，设置渲染目标的颜色为摄像机背景
                var flags = camera.clearFlags;

                //myCommandBuffer.ClearRenderTarget(true,true,camera.backgroundColor);

                myCommandBuffer.ClearRenderTarget((flags & CameraClearFlags.Depth) != 0,(flags & CameraClearFlags.Color) != 0,camera.backgroundColor);


                //renderContext.DrawSkybox(camera);

                //同上一节的剪裁
                ScriptableCullingParameters cullParam = new ScriptableCullingParameters();
                camera.TryGetCullingParameters(out cullParam);
                cullParam.isOrthographic = false;
                CullingResults cullResults = renderContext.Cull(ref cullParam);


                //在剪裁结果中获取灯光并进行参数获取
                var lights = cullResults.visibleLights;
                myCommandBuffer.name = "Render Lights";
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
                myCommandBuffer.SetGlobalVectorArray(_LightColor0, DLightColors);
                myCommandBuffer.SetGlobalVectorArray(_LightDir0, DLightDirections);


                myCommandBuffer.SetGlobalVectorArray(_PLightColor, PLightColors);
                myCommandBuffer.SetGlobalVectorArray(_PLightPos, PLightPos);

                //执行CommandBuffer中的指令
                renderContext.ExecuteCommandBuffer(myCommandBuffer);
                myCommandBuffer.Clear();

                //同上节，过滤
                FilteringSettings filtSet = new FilteringSettings(RenderQueueRange.opaque, -1);
                //filtSet.renderQueueRange = RenderQueueRange.opaque;
                //filtSet.layerMask = -1;                

                //同上节，设置Renderer Settings
                //注意在构造的时候就需要传入Lightmode参数，对应shader的pass的tag中的LightMode
                SortingSettings sortSet = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
                DrawingSettings drawSet = new DrawingSettings(new ShaderTagId("BaseLit"), sortSet);

                //1.绘制不透明物体
                renderContext.DrawRenderers(cullResults, ref drawSet, ref filtSet);

                //2.绘制天空球
                renderContext.DrawSkybox(camera);

                //3.绘制透明物体
                 sortSet = new SortingSettings(camera) { criteria = SortingCriteria.BackToFront };
                 drawSet = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), sortSet);
                filtSet.renderQueueRange = RenderQueueRange.transparent;
                renderContext.DrawRenderers(cullResults, ref drawSet, ref filtSet);

#if UNITY_EDITOR
                //为了在场景视图中看到UI
                if (camera.cameraType == CameraType.SceneView)
                {
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
                }
#endif 
                //开始执行渲染内容
                renderContext.Submit();
            }
        }
    }
}
