using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GpuInstancingTest : MonoBehaviour
{
    public GameObject preObj;
    public Mesh mesh;
    Material material;

    List<Matrix4x4> matrices = new List<Matrix4x4>();

    void Start()
    {
        material = preObj.GetComponent<MeshRenderer>().sharedMaterial;
        material.enableInstancing = true;
        if (!material.enableInstancing)
        {
            Debug.LogError("无法开启 !material.enableInstancing");
            enabled = false;
            return;
        }
        if (!SystemInfo.supportsInstancing)
        {
            Debug.LogError("无法开启 !SystemInfo.supportsInstancing");
            enabled = false;
            return;
        }
        block = new MaterialPropertyBlock();
        mesh = preObj.GetComponent<MeshFilter>().mesh;

    }
    
    MaterialPropertyBlock block = null;
    void OnPostRender() //camera
    {
        // SetPass to 0 if the material doesnt have a texture.
        //如果材质没有个纹理，SetPass为0

        //for (int i = 0; i < 20; ++i)
        //{
        //    float r = Random.Range(0, 1f);
        //    float g = Random.Range(0, 1f);
        //    float b = Random.Range(0, 1f);
        //    Matrix4x4 mt = Matrix4x4.TRS(transform.position + (i * 1.1f) * Vector3.up, transform.rotation, Vector3.one);
        //    matrices.Add(mt);

        //    preObj.GetComponent<MeshRenderer>().GetPropertyBlock(block);
        //    block.SetColor("_Color", new Color(r, g, b, 1));
        //     block.SetTexture("_MainTex", null);
        //    preObj.GetComponent<MeshRenderer>().SetPropertyBlock(block);

        //    Graphics.DrawMeshNow(mesh, new Vector3(i, i, i), Quaternion.identity);
        //}
    }

    // 两者都需要在for循环内部，每次单独提交
    // 但 drawmeshNow 用于OnPostRender
    // DrawMeshInstanced 用于Update中
    void Update()
    {
        MeshRenderer mMeshRenderer = preObj.GetComponent<MeshRenderer>();
        for (int i = 0; i < 100; ++i)
        {
            //for (int j = 0; j < 100; ++j)
            {
                Vector3 pos = preObj.transform.position + new Vector3(i * 1.2f, i * 1.2f, i * 1.2f);

                Matrix4x4 mt = Matrix4x4.TRS(pos, preObj.transform.rotation, Vector3.one);
                matrices.Add(mt);

                //  GameObject go = Instantiate(prefab);
                //go.transform.Translate(new Vector3(Mathf.Cos(i), Mathf.Sin(i), 0) * i/10f);

                float r = Random.Range(0, 1f);
                float g = Random.Range(0, 1f);
                float b = Random.Range(0, 1f);

                mMeshRenderer.GetPropertyBlock(block);
                block.SetColor("_OutsideColor", new Color(i / 50.0f, i / 10.0f, i / 50.0f, 1));
                //block.SetFloat("_Sin", i);
                //当每个对象的材料只有几个属性不同时，建议使用此方法。与每个对象具有一个完全不同的材质相比，这具有更高的内存效率。
                mMeshRenderer.SetPropertyBlock(block);


                // public static void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, 
                //int layer, Camera camera, int submeshIndex, MaterialPropertyBlock properties);
                // Graphics.DrawMeshInstanced(mesh, 0, material, matrices.ToArray(), 1, block);
               Graphics.DrawMesh(mesh, pos, Quaternion.identity, material, 2 << 1, Camera.main, 0, block); ;

            }
        }

    }
}