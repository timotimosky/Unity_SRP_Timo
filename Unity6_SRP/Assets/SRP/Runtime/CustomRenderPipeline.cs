﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//SRP的第一个好处，虽然都是前向渲染，但DrawCall明显变少了。非SRP虽然也可以单Pass多光源，但除平行光外，
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
        public CommandBuffer commandBuffer;

        //执行所有的渲染 
        protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
        {
            RenderAllCameras(renderContext, new List<Camera>(cameras));
        }
        //Camera[]需要为每帧分配内存，因此引入List<Camera> 替代。
        protected override void Render(ScriptableRenderContext renderContext, List<Camera> cameras)
        {
            RenderAllCameras(renderContext, cameras);
        }

        CameraRenderer mCameraRenderer = new CameraRenderer();
        //Render函数用于每一帧执行所有的渲染，
        //Render函数接受两个参数：第一个是被称为ScriptableRenderContext的新概念，我们会在后面介绍，
        //第二个是一个相机数组，包含了所有需要渲染的相机列表。我们一般需要针对列表里每一个相机运行一次管线，
        //但是针对相机种类的不同，可能会使用不同的渲染流程。
        protected void RenderAllCameras(ScriptableRenderContext renderContext, List<Camera> cameras)
        {

            //渲染开始后，创建CommandBuffer;
            if (commandBuffer == null)
                commandBuffer = new CommandBuffer() { name = "First SRP Render CommandBuffer" };

            //所有相机开始逐次渲染
            for (int i = 0; i < cameras.Count; ++i) //不要使用foreach，以后要对摄像机排序
            {
                var camera = cameras[i];
                mCameraRenderer.RenderSingleCamera(camera, renderContext, commandBuffer);
            }
        }

        //在不继续渲染的时候（例如我们切换到了另一个渲染管线）对当前管线进行现场清理。
        protected override void Dispose(bool dis)
        {
            base.Dispose(dis);
            if (commandBuffer != null)
            {
                commandBuffer.Dispose();//释放CommandBuffer
                commandBuffer = null;
            }
        }
    }
}
