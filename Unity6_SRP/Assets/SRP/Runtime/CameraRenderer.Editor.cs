using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

partial class CameraRenderer {

	partial void DrawGizmosBeforeFX ();

	partial void DrawGizmosAfterFX ();
	
	partial void DrawUnsupportedShaders ();

	partial void PrepareForSceneWindow ();

	partial void PrepareBuffer ();

#if UNITY_EDITOR

	static ShaderTagId[] legacyShaderTagIds = {
		new ShaderTagId("Always"),
		new ShaderTagId("ForwardBase"),
		new ShaderTagId("PrepassBase"),
		new ShaderTagId("Vertex"),
		new ShaderTagId("VertexLMRGBM"),
		new ShaderTagId("VertexLM")
	};

	static Material errorMaterial;

	string SampleName { get; set; }

	partial void DrawGizmosBeforeFX () {
		if (Handles.ShouldRenderGizmos()) {
			renderContext.DrawGizmos(camera, GizmoSubset.PreImageEffects);
		}
	}

	partial void DrawGizmosAfterFX () {
		if (Handles.ShouldRenderGizmos()) {
			renderContext.DrawGizmos(camera, GizmoSubset.PostImageEffects);
		}
	}


	//上述代码是将不支持的“错误Shader”的物体于最后渲染，因为我们不关心它的渲染顺序，我们要做的就是将它展现出来，
	//因此使用DrawRendererSettings的SetOverrideMaterial方法，用Unity内置的error shader进行渲染。
	// DrawRendererSettings之所以使用“ForwardBase”作为Pass Name，是因为目前我们的SRP只支持前向光照，而默认的表面着色器是有这个Pass的，
	// 如果还想将其他Shader Pass明确作为错误Shader提示，也可用SetShaderPassName方法添加。

	// Unity的默认表面着色器具有ForwardBase通道，该通道用作第一个正向渲染通道。我们可以使用它来识别具有与默认管道一起使用的材质的对象。
	//通过新的绘图设置选择该通道，并将其与新的默认滤镜设置一起用于渲染。我们不在乎排序或分离不透明渲染器和透明渲染器，因为它们仍然无效。

	//由于我们的管道仅支持未着色的着色器，因此不会渲染使用不同着色器的对象，从而使它们不可见。尽管这是正确的，
	//但它掩盖了某些对象使用错误着色器的事实。如果我们使用Unity的错误着色器可视化这些对象，那将是很好的，
	//因此它们显示为明显不正确的洋红色形状。让我们DrawDefaultPipeline为此添加一个专用方法，其中包含一个上下文和一个camera参数。
	//在绘制透明形状之后，我们将在最后调用它。

	//仅DrawDefaultPipeline在编辑器中调用。一种方法是通过Conditional向该方法添加属性。
	//	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	partial void DrawUnsupportedShaders () {
		if (errorMaterial == null) {
			errorMaterial =new Material(Shader.Find("Hidden/InternalErrorShader"))
			{
				hideFlags = HideFlags.HideAndDontSave
			};
		}

		//涵盖了Unity提供的所有着色器
		//现在，使用不受支持的材料的对象显然会显示为不正确。但这仅适用于Unity的默认管道材质，其着色器可以ForwardBase通过。
		//我们还可以使用其他遍历来识别其他内置着色器，特别是PrepassBase，Always，Vertex，VertexLMRGBM和VertexLM。
		//幸运的是，可以通过调用将多个遍添加到绘图设置中SetShaderPassName。名称是此方法的第二个参数。它的第一个参数是控制通行证绘制顺序的索引。
		//我们不在乎，所以任何订单都可以。通过构造函数提供的通道始终具有零索引，只需增加索引即可获得更多通道。
		//决定使用何种light mode，对应shader的pass的tag中的LightMode
		DrawingSettings drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera)) 
		{
			overrideMaterial = errorMaterial
			//overrideMaterialPassIndex = 0;
		};

		for (int i = 1; i < legacyShaderTagIds.Length; i++) {
			drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
		}
		//由于我们仅在管道中支持未照明的材质，因此我们将使用Unity的默认未照明通道，该通道由SRPDefaultUnlit标识。
		// new FilteringSettings(RenderQueueRange.opaque, -1);
		FilteringSettings filteringSettings = FilteringSettings.defaultValue;
		renderContext.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
	}


    //尽管Unity帮我们适配了UI在游戏窗口中显示，但不会在场景窗口显示。
    //UI始终存在于场景窗口中的世界空间中，但是我们必须手动将其注入场景中。
    //为了避免游戏窗口中第二次添加UI。必须在cull之前完成此操作。我们仅在渲染场景窗口时才发出UI几何。
    //cameraType相机等于CameraType.SceneView的时候就是这种情况。
    //在场景编辑界面显示UI
    partial void PrepareForSceneWindow () {
#if UNITY_EDITOR
        if (camera.cameraType == CameraType.SceneView) {
			ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
		}
#endif
    }

    partial void PrepareBuffer () {
		Profiler.BeginSample("Editor Only");
		commandBuffer.name = SampleName = camera.name;
		Profiler.EndSample();
	}

#else

	const string SampleName = bufferName;

#endif
}