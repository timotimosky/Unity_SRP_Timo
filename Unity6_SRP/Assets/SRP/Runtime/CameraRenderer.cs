using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public partial class CameraRenderer {

	const string bufferName = "Render Camera";

	static ShaderTagId
		unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
		litShaderTagId = new ShaderTagId("CustomLit"),
		SrpForwardTagId = new ShaderTagId("SrpForward"),
		SrpTransTagId = new ShaderTagId("SrpTrans");

	static int frameBufferId = Shader.PropertyToID("_CameraFrameBuffer");

	//CommandBuffer，是指令记录表。Unity使用“先记录，后执行”的策略实现渲染管线，
	//好比去餐馆吃饭，可能花了好长时间才把菜点完，然后一旦提交给厨房，会一次性把菜做好。
	//Unity的延迟执行体现在CommandBuffer和ScriptableRenderContext的设计中，这两个对象都充当我们的“菜单”。
	//将需要执行的执行记录在菜单上以后，
	//可以使用ScriptableRenderContext.ExecuteCommandBuffer和ScriptableRenderContext.Submit来提交
	//CommandBuffer和ScriptableRenderContext。

	//我们用一个独立的命令缓冲区来为阴影工作，所以我们在帧调试器中看到阴影和常规场景是在独立的区域渲染的。

	//像绘制天空盒这样的任务我们可以通过特有的方法来控制，但是其他的命令只能通过单独的Command Buffer（命令缓冲区）
	//我们用一个独立的CommandBuffer来为阴影工作，所以我们在帧调试器中看到阴影和常规场景是在独立的区域渲染的。
	//CommandBuffers在scriptable rendering pipeline添加之前就已经存在，所以它不是实验性的，
	//现在我们在绘制skybox之前创建一个commandbuffer对象。
	//我们通过ExecuteCommandBuffe方法让上下文执行这个buffer，这个命令不会立即执行，他只是把它copy到上下文的内部buffer中。
	CommandBuffer commandBuffer = new CommandBuffer {
		name = bufferName
	};

	ScriptableRenderContext renderContext;

	Camera camera;

	CullingResults cullingResults;

	Lighting lighting = new Lighting();

	PostFXStack postFXStack = new PostFXStack();

	bool useHDR;

	public void RenderSingleCamera (ScriptableRenderContext context, Camera camera, bool allowHDR,bool useDynamicBatching, bool useGPUInstancing,
		bool useLightsPerObject,ShadowSettings shadowSettings, PostFXSettings postFXSettings) 
	{
		this.renderContext = context;
		this.camera = camera;

		PrepareBuffer();
		PrepareForSceneWindow();
		if (!Cull(shadowSettings.maxDistance)) 
		{
			Debug.LogError("没有可见物体");
			return;
		}
		useHDR = allowHDR && camera.allowHDR;

		//我们可以使用命令缓冲区注入分析器样本,将出现分析器和帧调试器。 
		//这是通过调用来完成的 BeginSample 和 EndSample 在适当的点,这是开始 设置 和 提交 在我们的例子中。 
		//这两种方法都必须提供相同的样品名称,我们将使用缓冲区的名字。
		commandBuffer.BeginSample(SampleName);
		
		//渲染前，我们先提交一次空buffer到上下文
		//执行一个空的command buffer什么都不会做，添加它是为了清空渲染对象，避免受到之前渲染结果的影响。
		//这可以通过命令缓冲区实现，但不能直接通过上下文实现。
		//ExecuteBuffer();

        //如果不在最前面设置相机，那么后续的ClearRenderTarget将使用draw GL（执行一次空绘制）来清理，比较低效，需要耗费一次set passCall
		//我们也可以不使用之前的空buffer提交了

        //在最前面设置相机参数后，，则可以使用直接 clear(depth + stencil)，清理camera的深度 +模板
        //设置渲染相关相机参数,包含相机的各个矩阵和剪裁平面等
        renderContext.SetupCameraProperties(camera);



        //设置光照和阴影贴图
        lighting.Setup(context, cullingResults, shadowSettings, useLightsPerObject);
		//后处理
		postFXStack.Setup(context, camera, postFXSettings, useHDR);

		commandBuffer.EndSample(SampleName);

		//设置上下文和buffer。清空
		Setup();
		//绘制可见几何体
		DrawVisibleGeometry(useDynamicBatching, useGPUInstancing, useLightsPerObject);
		//绘制错误着色器
		DrawUnsupportedShaders();
		DrawGizmosBeforeFX();
		if (postFXStack.IsActive) 
		{
			postFXStack.Render(frameBufferId);
		}
		DrawGizmosAfterFX();
		Cleanup();
		//真正执行渲染内容
		Submit();
	}
	//剔除：拿到场景中的所有渲染器，然后剔除那些在摄像机视锥范围之外的渲染器(通常是一个MeshRenderer组件)。
	bool Cull (float maxShadowDistance)
    {   
		//渲染器：它是附着在游戏对象上的组件，可将它们转变为可以渲染的东西。通常是一个MeshRenderer组件。
        //首先判断相机是否支持渲染
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters cullParam)) 
		{
			//   cullParam.isOrthographic = false;
			cullParam.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            //TODO：避免没有任何可渲染的物体时，往下渲染
            //refout用作优化，以防止传递结构的副本，该副本相当大。cullParam是一个结构而不是一个对象是另一种优化，以防止内存分配。
            cullingResults = renderContext.Cull(ref cullParam);		
			return true;
		}
		return false;
	}

	void Setup () 
	{
		//设置渲染相关相机参数,包含相机的各个矩阵和剪裁平面等
		//为了渲染天空盒和整个场景，我们必须要设置view-projection矩阵，
		//这个变换矩阵将摄像机的位置和方向(视图矩阵)与摄像机的透视或正投影(投影矩阵)相结合。
		//可以在frame debugger中看到这个矩阵unity_MatrixVP. 是shader中的一个属性。
		//此时，unity_MatrixVP矩阵都是一样的，我们通过SetupCameraProperties这个方法来传递摄像机的属性给上下文，
		//renderContext.SetupCameraProperties(camera);




		//根据flags来清理
		CameraClearFlags flags = camera.clearFlags;

		if (postFXStack.IsActive) 
		{
			if (flags > CameraClearFlags.Color) 
			{
				flags = CameraClearFlags.Color;
			}
			commandBuffer.GetTemporaryRT(frameBufferId, camera.pixelWidth, camera.pixelHeight,
				32, FilterMode.Bilinear, useHDR ?RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
			commandBuffer.SetRenderTarget(frameBufferId,RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
		}

		//我们可以通过调用ClearRenderTarget方法添加一个一个清理命令。
		//第一个参数表示深度信息是否清除，第二个参数表示color信息是否清除，第三个参数是清理的color值，如果使用。
		//例如，让我们清除深度数据，忽略颜色数据，使用Color.clear 作为清除颜色。
		commandBuffer.ClearRenderTarget(
			flags <= CameraClearFlags.Depth,
			flags == CameraClearFlags.Color,
			flags == CameraClearFlags.Color ?camera.backgroundColor.linear : Color.clear
		);
		commandBuffer.BeginSample(SampleName);
		//第二次提交buffer
		ExecuteBuffer();
	}

	void Cleanup () {
		lighting.Cleanup();
		if (postFXStack.IsActive) {
			commandBuffer.ReleaseTemporaryRT(frameBufferId);
		}
	}
    public void Dispose()
	{
        if (commandBuffer != null)
        {
          //  commandBuffer.Release();
            //commandBuffer.Clear();

            commandBuffer.Dispose();//释放CommandBuffer
            commandBuffer = null;
        }
    }


	void Submit () {
		commandBuffer.EndSample(SampleName);
		ExecuteBuffer();
		renderContext.Submit();
	}

	void ExecuteBuffer () {
		renderContext.ExecuteCommandBuffer(commandBuffer);
        //命令缓冲区会在unity的原生层开辟空间来去存储命令。所以如果我们不再需要这些资源，我们最好马上释放它。
        //我们可以在调用ExecuteCommandBuffer方法之后调用Release方法来释放它。
        commandBuffer.Release();
		commandBuffer.Clear();
		
	}

	void DrawVisibleGeometry (bool useDynamicBatching, bool useGPUInstancing, bool useLightsPerObject) 
	{
		PerObjectData lightsPerObjectFlags = useLightsPerObject ?PerObjectData.LightData | PerObjectData.LightIndices :PerObjectData.None;

		var sortingSettings = new SortingSettings(camera) {
			criteria = SortingCriteria.CommonOpaque
		};

		//决定使用何种light mode，对应shader的pass的tag中的LightMode

		//1.绘制SRPDefaultUnlit

		//相机用于设置排序和剔除层，而DrawingSettings控制使用哪个着色器Pass进行渲染。
		DrawingSettings drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) 
		{
			enableDynamicBatching = useDynamicBatching,
			enableInstancing = useGPUInstancing,
			perObjectData =
				PerObjectData.ReflectionProbes |
				PerObjectData.Lightmaps | PerObjectData.ShadowMask |
				PerObjectData.LightProbe | PerObjectData.OcclusionProbe |
				PerObjectData.LightProbeProxyVolume |
				PerObjectData.OcclusionProbeProxyVolume |
				lightsPerObjectFlags
		};
		drawingSettings.SetShaderPassName(1, litShaderTagId);
		drawingSettings.SetShaderPassName(2, SrpForwardTagId);
		drawingSettings.SetShaderPassName(3, SrpTransTagId);

		//过滤：决定使用哪些渲染器
		// FilteringSettings filtSet = new FilteringSettings(RenderQueueRange.opaque, -1);
		//FilteringSettings filtSet = new FilteringSettings(RenderQueueRange.all);



		//决定使用何种渲染排序顺序 对应shader里的   Tags{ "Queue" = "Geometry" } 这属性(不是这个单一属性)
		//opaque涵盖了从0到2500（包括2500）之间的渲染队列。
		FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

		//2.绘制CustomLit
		renderContext.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);


		//3.绘制天空球,在不透明物体之后绘制。early-z避免不必要的overdraw。
		//由摄像机的Clear flags控制是否真的绘制Skybox。 
		renderContext.DrawSkybox(camera);

		//4.绘制透明物体
		sortingSettings.criteria = SortingCriteria.CommonTransparent;
		drawingSettings.sortingSettings = sortingSettings;
		filteringSettings.renderQueueRange = RenderQueueRange.transparent;

		renderContext.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
	}
}