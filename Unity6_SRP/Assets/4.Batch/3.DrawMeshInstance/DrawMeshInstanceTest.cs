using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Graphics = UnityEngine.Graphics;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;


public enum DrawMode
{
    Defalt=0,
    GpuInstance=1,
    ComondInstance=2,
    DrawMeshNow=3,
    DrawMesh =4,
    InstancedInstanceByComputeBuffer=5,
}

//非常适合草地绘制

//1.先收集场景里的所有物体
public class DrawMeshInstanceTest : MonoBehaviour {

	static int
		baseColorId = Shader.PropertyToID("_BaseColor"),
		metallicId = Shader.PropertyToID("_Metallic"),
		smoothnessId = Shader.PropertyToID("_Smoothness");

	public List<PerObjectMaterialProperties> materialProperties;
    public List<PerObjectMaterialProperties> needRenderMaterialProperties = new List<PerObjectMaterialProperties>();
    public GameObject m_prefab;
    CullBound cullBound;
    //预制体可能由多个mesh组成
    MeshFilter[] meshFs;

    Mesh[] sharedMesh;
    Renderer[] renders;
    Material[] sharedMaterial;

    [SerializeField]
	Mesh instanceMesh = default;

	[SerializeField]
	Material instanceMaterial = default;


    //ComputeBuffer绘制的间接实例，要用特殊shader
    [SerializeField]
    Material InstancedIndirectMaterial = default;

    [SerializeField]
	LightProbeProxyVolume lightProbeVolume = null;

    [SerializeField]
    ShadowCastingMode shadowCastingMode;//阴影选项

    [SerializeField]
    public bool receiveShadow =true;//阴影

    const int MaxInstanceCount = 1023;

    Matrix4x4[] matrix4x4s;
	Vector4[] baseColors = new Vector4[MaxInstanceCount];
	float[]
		metallic = new float[MaxInstanceCount],
		smoothness = new float[MaxInstanceCount];

	MaterialPropertyBlock block;
    MeshRenderer mMeshRenderer;
    public DrawMode drawMode = DrawMode.Defalt;
    public bool ifCull = true;

    public int instanceCount = 1023;
    public int cachedInstanceCount = 0;
    private int cachedSubMeshIndex = -1;
    public int subMeshIndex = 0;

    public int layer = 0;

    public Camera bufferCamera;
    public void InitFakerData()
    {

        if (instanceMesh != null)
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);
        materialProperties = new List<PerObjectMaterialProperties>(instanceCount);
        for (int i = 0; i < instanceCount; i++)
        {
            PerObjectMaterialProperties perObjectMaterialProperties = new PerObjectMaterialProperties();
            perObjectMaterialProperties.position = Random.insideUnitSphere * 100f;
            perObjectMaterialProperties.scale = Vector3.one * Random.Range(0.5f, 1.5f);
            perObjectMaterialProperties.rotation = Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f);

            perObjectMaterialProperties.SetMatrix4X4();
            perObjectMaterialProperties.baseColor = new Vector4(Random.value, Random.value, Random.value, Random.Range(0.5f, 1f));
            perObjectMaterialProperties.metallic = Random.value < 0.25f ? 1f : 0f;
            perObjectMaterialProperties.smoothness = Random.Range(0.05f, 0.95f);

            materialProperties.Add(perObjectMaterialProperties);
        }
        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }




    public void GetCullingResults()
	{
        if(ifCull)
            cullResult = cullBound.ExcuteCullJob(materialProperties);
  
        needRenderMaterialProperties.Clear();
        for (int i=0; i < materialProperties.Count; i++)
        {
            PerObjectMaterialProperties prop= materialProperties[i];
            //bool outOrIn = cullBound.ShouldCullCell(prop.position,prop.scale.x,prop.position);
            //if (outOrIn && ifCull)
            if (ifCull &&cullResult[i] == 0)
            {
                //Debug.Log("需要被剔除");
            }
            else
                needRenderMaterialProperties.Add(prop);
        }
    }

    public int needRenderIndex = 0;
    int needRenderCount =0;


    private void SetBlock()
    {
        matrix4x4s = new Matrix4x4[needRenderCount];
        for (int i = 0; i < needRenderCount; i++)
        {
            PerObjectMaterialProperties prop = needRenderMaterialProperties[i];

            if (prop.dirty)
            {
                prop.SetObjPropertyBlock();
                prop.dirty = false;
            }

            matrix4x4s[i] = (prop.matrix4X4);
            baseColors[i] = prop.baseColor;
            metallic[i] = prop.metallic;
            smoothness[i] = prop.smoothness;
        }


        block.SetVectorArray(baseColorId, baseColors);
        block.SetFloatArray(metallicId, metallic);
        block.SetFloatArray(smoothnessId, smoothness);
        //如果probe灯光存在
        if (!lightProbeVolume)
        {
            Vector3[] positions = new Vector3[needRenderCount];
            for (int i = 0; i < matrix4x4s.Length; i++)
            {
                positions[i] = matrix4x4s[i].GetColumn(3);
            }
            var lightProbes = new SphericalHarmonicsL2[needRenderCount];
            var occlusionProbes = new Vector4[needRenderCount];
            LightProbes.CalculateInterpolatedLightAndOcclusionProbes(
                positions, lightProbes, occlusionProbes
            );
            block.CopySHCoefficientArraysFrom(lightProbes);
            block.CopyProbeOcclusionArrayFrom(occlusionProbes);
        }
    }

    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    Vector4[] computePositions;
    private void SetComputeBuffer()
    {
        if (positionBuffer != null)
            positionBuffer.Release();

        Debug.Log("设置剔除后的data=" + needRenderCount);
        positionBuffer = new ComputeBuffer(needRenderCount, 16);
        computePositions = new Vector4[needRenderCount];

        for (int i = 0; i < needRenderCount; i++)
        {
            PerObjectMaterialProperties prop = needRenderMaterialProperties[i];
            computePositions[i] = new Vector4(prop.position.x, prop.position.y, prop.position.z, prop.scale.x);
        }


        positionBuffer.SetData(computePositions);
        InstancedIndirectMaterial.SetBuffer("positionBuffer", positionBuffer);


        // Indirect args
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);

        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }


    public void DrawInstanceOnce()
    {
        SetBlock();

        if (drawMode == DrawMode.ComondInstance)
        {
            CommandBufferForDrawMeshInstanced();
            Debug.LogError("渲染..........Buffer===");
        }
        else if (drawMode == DrawMode.GpuInstance && EnableInstancing())
        {
            /*
            Mesh 索要绘制的网格
            submeshIndex 要绘制网格的哪个子集,由多个网格组成的才用得到，一般为0
            Material 材质
            Matrix4x4s 矩阵数组
            count 数量 最多只能绘制MaxInstanceCount个实例，也就是说count<=MaxInstanceCount。
            properties 步骤3中创建的MaterialPropertyBlock，用来传递属性
            还有很多参数，不过画草的话用不到
            */
            LightProbeUsage mLightProbeUsage = lightProbeVolume ? LightProbeUsage.UseProxyVolume : LightProbeUsage.CustomProvided;

            if (instanceMesh)
                Graphics.DrawMeshInstanced(instanceMesh, 0, instanceMaterial, matrix4x4s, matrix4x4s.Length, block,
                shadowCastingMode, receiveShadow, layer, null, mLightProbeUsage, lightProbeVolume);
            else
            {
                for (int i = 0; i < sharedMesh.Length; ++i)
                {
                    Graphics.DrawMeshInstanced(sharedMesh[i], 0, sharedMaterial[i],
                        matrix4x4s, matrix4x4s.Length, block, shadowCastingMode, receiveShadow, layer, null, mLightProbeUsage, lightProbeVolume);
                }
            }
        }
        // public static void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, 
        //int layer, Camera camera, int submeshIndex, MaterialPropertyBlock properties);
        // Graphics.DrawMesh(mesh, pos, Quaternion.identity, material, 2 << 1, Camera.main, 0, block); ;
    }

    public bool EnableInstancing()
    {
        instanceMaterial.enableInstancing = true;
        if (!instanceMaterial.enableInstancing)
        {
            Debug.LogError("无法开启 !material.enableInstancing");
            enabled = false;
            return false;
        }
        if (!SystemInfo.supportsInstancing)
        {
            Debug.LogError("无法开启 !SystemInfo.supportsInstancing");
            enabled = false;
            return false;
        }
        return true;
    }



    void Awake () {
        mMeshRenderer = m_prefab.GetComponent<MeshRenderer>();
        InitFakerData();
        Shader.EnableKeyword("LIGHTMAP_ON");//开启lightmap
                                            //Shader.DisableKeyword("LIGHTMAP_OFF")
        bufferCamera = Camera.main;
        cullBound = new CullBound(Camera.main);
        var meshFilter = m_prefab.GetComponent<MeshFilter>();

        if (instanceMesh == null)
            instanceMesh = m_prefab.GetComponent<MeshFilter>().sharedMesh;

        instanceMaterial = mMeshRenderer.sharedMaterial;
        if (instanceMaterial == null)
            instanceMaterial = m_prefab.GetComponent<MeshRenderer>().sharedMaterial;

        //如果一个预制体 由多个mesh组成，则需要绘制多少次
        if (instanceMesh == null)
        {
            meshFs = m_prefab.GetComponentsInChildren<MeshFilter>();
            sharedMesh = new Mesh[meshFs.Length];
            for (int i=0;i<meshFs.Length; i++)
            {
                sharedMesh[i] = meshFs[i].sharedMesh;
            }
        }
        if (instanceMaterial == null)
        {
            renders = m_prefab.GetComponentsInChildren<Renderer>();
            sharedMaterial = new Material[renders.Length];
            for (int i = 0; i < renders.Length; i++)
            {
                sharedMaterial[i] = renders[i].sharedMaterial;
            }
        }

        shadowCastingMode = ShadowCastingMode.On;

        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }

        //只用于间接实例化
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    }

    void Start()
    {

        CommandBufferForDrawMeshInstanced();
    }

    NativeArray<int> cullResult;
    // 两者都需要在for循环内部，每次单独提交
    // 但 drawmeshNow 用于OnPostRender
    // DrawMeshInstanced 用于Update中
    void Update () 
    {
        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
        {
            InitFakerData();
            if (drawMode == DrawMode.InstancedInstanceByComputeBuffer)
            {
                SetComputeBuffer();
            }
        }
        if (drawMode == DrawMode.DrawMeshNow) 
        {
            //内置管线DrawMeshNow是在OnRenderObject中被调用，URP则是endContextRendering
           // UnityEngine.Rendering.RenderPipelineManager.endContextRendering -= CallBackDraw;
           // UnityEngine.Rendering.RenderPipelineManager.endContextRendering += CallBackDraw;
        }
        else if (drawMode == DrawMode.InstancedInstanceByComputeBuffer)
        {
            // Render
            Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, InstancedIndirectMaterial,
                new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),
                argsBuffer);
        }
        else
            RealDraw();
    }


    void CallBackDraw(ScriptableRenderContext mScriptableRenderContext, List<Camera> cameras)
    {
        if (drawMode != DrawMode.DrawMeshNow)
            return;
        Debug.Log("实例化绘制");
        GetCullingResults();
        SetBlock();
        int i = 0;
        foreach (PerObjectMaterialProperties prop in materialProperties)
        {
            instanceMaterial.SetPass(0);
            i++;
            Graphics.DrawMeshNow(instanceMesh, prop.position, prop.rotation);
        }
    }
    void OnDrawGizmos()
    {
        if (drawMode != DrawMode.DrawMeshNow)
            return;
        Debug.Log("实例化绘制");
        GetCullingResults();
        SetBlock();
        int i = 0;
        foreach (PerObjectMaterialProperties prop in materialProperties)
        {
            i++;
            Graphics.DrawMeshNow(instanceMesh, prop.position, prop.rotation);
        }
    }

    private void OnGUI()
    {

        GUI.Label(new Rect(265, 25, 200, 30), "Instance Count: " + instanceCount.ToString());
        instanceCount = (int)GUI.HorizontalSlider(new Rect(150, 80, 200, 30), (float)instanceCount, 1.0f, 100000.0f);

        if (GUILayout.Button("<size=50>当位置发生变化时候在更新</size>"))
        {
            if (drawMode == DrawMode.ComondInstance)
            {
                CommandBufferForDrawMeshInstanced();
            }
        }
    }



    void RealDraw()
    {
        GetCullingResults();

        if (drawMode == DrawMode.DrawMesh)
        {
            int i = 0;
            foreach (PerObjectMaterialProperties prop in needRenderMaterialProperties)
            {
                i++;
                Graphics.DrawMesh(instanceMesh, prop.position, prop.rotation, instanceMaterial, 0);
            }
            return;     
        }


        needRenderIndex = 0;
        while (true)
        {
            needRenderCount = needRenderMaterialProperties.Count - needRenderIndex;
            if (needRenderCount < 1)
                return;

            if (needRenderCount > MaxInstanceCount)
            {
                needRenderCount = MaxInstanceCount;
            }
            needRenderIndex += needRenderCount;

            DrawInstanceOnce();
        }
    }


    void ClearCommandBufferDraw()
    {
        if (commandBuffer != null)
        {
            if(bufferCamera==null)
                bufferCamera =Camera.main;
            bufferCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }

    void CommandBufferForDrawMeshInstanced()
    {
        if (drawMode != DrawMode.ComondInstance)
        {
            return;
        }
         ClearCommandBufferDraw();
        commandBuffer = CommandBufferPool.Get("DrawMeshInstanced");
        commandBuffer.DrawMeshInstanced(instanceMesh, 0, instanceMaterial, 0, matrix4x4s, matrix4x4s.Length, block);
        bufferCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
    }
        
    CommandBuffer commandBuffer = null;
}