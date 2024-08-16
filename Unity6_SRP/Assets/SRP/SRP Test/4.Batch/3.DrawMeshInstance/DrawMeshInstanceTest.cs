﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;


//非常适合草地绘制

//1.先收集场景里的所有物体
public class DrawMeshInstanceTest : MonoBehaviour {

	static int
		baseColorId = Shader.PropertyToID("_BaseColor"),
		metallicId = Shader.PropertyToID("_Metallic"),
		smoothnessId = Shader.PropertyToID("_Smoothness");

	public List<PerObjectMaterialProperties> materialProperties;
    public List<PerObjectMaterialProperties> needRenderMaterialProperties;
    public GameObject m_prefab;
    CullBound cullBound;
    //预制体可能由多个mesh组成
    MeshFilter[] meshFs;

    Mesh[] sharedMesh;
    Renderer[] renders;
    Material[] sharedMaterial;

    [SerializeField]
	Mesh mesh = default;

	[SerializeField]
	Material material = default;

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
    //这个变量类似于unity5.6材质属性的Enable Instance Variants勾选项
    public bool turnOnInstance = true;

    public bool ifCull = true;

    public bool useCommandBuffer = false;

    public int TestNum = 1023;
    public int layer = 0;

    public Camera bufferCamera;
    public void InitFakerData()
    {
        materialProperties = new List<PerObjectMaterialProperties>(TestNum);
        for (int i = 0; i < TestNum; i++)
        {
            PerObjectMaterialProperties perObjectMaterialProperties = new PerObjectMaterialProperties();
            materialProperties.Add(perObjectMaterialProperties);
            materialProperties[i].position = Random.insideUnitSphere * 100f;
            materialProperties[i].scale = Vector3.one * Random.Range(0.5f, 1.5f);
            materialProperties[i].rotation = Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f);

            materialProperties[i].SetMatrix4X4();
            materialProperties[i].baseColor = new Vector4(Random.value, Random.value, Random.value, Random.Range(0.5f, 1f));
            materialProperties[i].metallic = Random.value < 0.25f ? 1f : 0f;
            materialProperties[i].smoothness = Random.Range(0.05f, 0.95f);
        }
    }




    public void CollectionDrawData()
	{
        if(ifCull)
            cullResult = cullBound.ExcuteCullJob(materialProperties);

        needRenderMaterialProperties = new List<PerObjectMaterialProperties>();
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
            var positions = new Vector3[needRenderCount];
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

    public void DrawInstanceOnce()
    {
        SetBlock();

        if (useCommandBuffer)
        {
            CommandBufferForDrawMeshInstanced();
            Debug.LogError("渲染..........Buffer===");
        }
        else if (turnOnInstance && EnableInstancing())
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

            if (mesh)
                Graphics.DrawMeshInstanced(mesh, 0, material, matrix4x4s, matrix4x4s.Length, block,
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



    void OnPostRender() //RequireComponent camera
    {
        int i = 0;
        foreach (PerObjectMaterialProperties prop in materialProperties)
        {
           // matrix4x4s[i] = prop.matrix4X4;
            i++;

           // Graphics.DrawMeshNow(mesh, new Vector3(i, i, i), Quaternion.identity);
        }
    }

    public bool EnableInstancing()
    {
        material.enableInstancing = true;
        if (!material.enableInstancing)
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

        if (mesh == null)
            mesh = m_prefab.GetComponent<MeshFilter>().sharedMesh;

        material = mMeshRenderer.sharedMaterial;
        if (material == null)
            material = m_prefab.GetComponent<MeshRenderer>().sharedMaterial;

        //如果一个预制体 由多个mesh组成，则需要绘制多少次
        if (mesh == null)
        {
            meshFs = m_prefab.GetComponentsInChildren<MeshFilter>();
            sharedMesh = new Mesh[meshFs.Length];
            for (int i=0;i<meshFs.Length; i++)
            {
                sharedMesh[i] = meshFs[i].sharedMesh;
            }
        }
        if (material == null)
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
    }
    NativeArray<int> cullResult;
    // 两者都需要在for循环内部，每次单独提交
    // 但 drawmeshNow 用于OnPostRender
    // DrawMeshInstanced 用于Update中
    void Update () {

        CollectionDrawData();

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
        if (useCommandBuffer)
        {
            ClearCommandBufferDraw();

            commandBuffer = CommandBufferPool.Get("DrawMeshInstanced");
            commandBuffer.DrawMeshInstanced(mesh, 0, material,0, matrix4x4s, matrix4x4s.Length, block);
            bufferCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
        }
    }
        
    CommandBuffer commandBuffer = null;
}