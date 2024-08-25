using Unity.Entities;
using UnityEngine;

//组织每个物体的参数
[DisallowMultipleComponent]
public class PerObjectMaterialProperties : IComponentData
{

	public bool dirty = false;

   // transfrom.localToWorldMatrix;
	public Matrix4x4 matrix4X4 = Matrix4x4.identity;

    public Vector3 position= Vector3.zero;
    public Vector3 scale;

    public Quaternion rotation;
    static int
		baseColorId = Shader.PropertyToID("_BaseColor"),
		cutoffId = Shader.PropertyToID("_Cutoff"),
		metallicId = Shader.PropertyToID("_Metallic"),
		smoothnessId = Shader.PropertyToID("_Smoothness"),
		emissionColorId = Shader.PropertyToID("_EmissionColor");

	static MaterialPropertyBlock arrayBlock;

	[SerializeField]
    public Color baseColor = Color.white;

	[SerializeField, Range(0f, 1f)]
    public float alphaCutoff = 0.5f, metallic = 0f, smoothness = 0.5f;

	[SerializeField, ColorUsage(false, true)]
	Color emissionColor = Color.black;


    public void SetObjPropertyBlock()
	{
        if (arrayBlock == null)
        {
            arrayBlock = new MaterialPropertyBlock();
        }
        arrayBlock.SetColor(baseColorId, baseColor);
        arrayBlock.SetFloat(cutoffId, alphaCutoff);
        arrayBlock.SetFloat(metallicId, metallic);
        arrayBlock.SetFloat(smoothnessId, smoothness);
        arrayBlock.SetColor(emissionColorId, emissionColor);

        matrix4X4 = Matrix4x4.TRS(position, rotation, scale);
    }


    public void SetMatrix4X4()
    {
        matrix4X4 = Matrix4x4.TRS(position, rotation, scale);
    }
    public void SetPos()
    {
        matrix4X4.SetColumn(3, position);
    }

    public void SetScale(Vector3 localScale)
    {
        matrix4X4.m00 = Mathf.Max(1, localScale.x);
        matrix4X4.m11 = Mathf.Max(1, localScale.y);
        matrix4X4.m22 = Mathf.Max(1, localScale.z);
    }
}