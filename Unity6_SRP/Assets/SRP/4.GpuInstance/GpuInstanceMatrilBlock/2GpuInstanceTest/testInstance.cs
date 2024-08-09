using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class testInstance : MonoBehaviour
{
    //材质用到的mesh
    Mesh mesh;
    Material mat;
    public GameObject m_prefab;
    Matrix4x4[] matrix;
    ShadowCastingMode castShadows;//阴影选项
    public int InstanceCount = 10;
    //树的预制体由树干和树叶两个mesh组成
    MeshFilter[] meshFs;
    Renderer[] renders;

    //这个变量类似于unity5.6材质属性的Enable Instance Variants勾选项
    public bool turnOnInstance = true;
    void Start()
    {
        if (m_prefab == null)
            return;
        Shader.EnableKeyword("LIGHTMAP_ON");//开启lightmap
                                            //Shader.DisableKeyword("LIGHTMAP_OFF");
        var mf = m_prefab.GetComponent<MeshFilter>();
        if (mf)
        {
            mesh = m_prefab.GetComponent<MeshFilter>().sharedMesh;
            mat = m_prefab.GetComponent<Renderer>().sharedMaterial;
        }
        //如果一个预制体 由多个mesh组成，则需要绘制多少次
        if (mesh == null)
        {
            meshFs = m_prefab.GetComponentsInChildren<MeshFilter>();
        }
        if (mat == null)
        {
            renders = m_prefab.GetComponentsInChildren<Renderer>();
        }
        matrix = new Matrix4x4[InstanceCount];

        castShadows = ShadowCastingMode.On;

        //随机生成位置与缩放
        for (int i = 0; i < InstanceCount; i++)
        {
            ///   random position
            float x = Random.Range(-50, 50);
            float y = Random.Range(-3, 3);
            float z = Random.Range(-50, 50);
            matrix[i] = Matrix4x4.identity;   ///   set default identity
            //设置位置
            matrix[i].SetColumn(3, new Vector4(x, 0.5f, z, 1));  /// 4th colummn: set   position
            //设置缩放
            //matrix[i].m00   = Mathf.Max(1, x);
            //matrix[i].m11   = Mathf.Max(1, y);
            //matrix[i].m22   = Mathf.Max(1, z);
        }
    }

    MaterialPropertyBlock props = null;

    void Update()

    //.通过Matrix4x4.TRS(position, rotation, scale)；把所有草的坐标算出来，存入数组中。
    //3.通过MaterialPropertyBlock给shader传递需要修改的值，一般用SetVectorArray或者SetFloatArray，因为我们希望这一批草或其它什么有不同的属性。
    //  Graphics.DrawMeshInstanced(Mesh, submeshIndex, Material, matrices, count, properties);
    /*
        Mesh 索要绘制的网格
        submeshIndex 要绘制网格的哪个子集,由多个网格组成的才用得到，一般为0
        Material 材质
        matrices 在步骤2中计算的矩阵数组
        count 数量 最多只能绘制1023个实例，也就是说count<=1023。
        properties 步骤3中创建的MaterialPropertyBlock，用来传递属性
        还有很多参数，不过画草的话用不到	
    */

    {//Graphics.DrawMeshInstanced() 在update函数实例化
        if (turnOnInstance)
        {
            //props = obj.GetComponent<MeshRenderer>().GetPropertyBlock(block);
            castShadows = ShadowCastingMode.On;
            if (mesh)
                Graphics.DrawMeshInstanced(mesh, 0, mat, matrix, matrix.Length, props, castShadows, true, 0, null);
            else
            {
                for (int i = 0; i < meshFs.Length; ++i)
                {
                    Graphics.DrawMeshInstanced(meshFs[i].sharedMesh, 0, renders[i].sharedMaterial, matrix, matrix.Length, props, castShadows, true, 0, null);
                }
            }
        }
    }
}