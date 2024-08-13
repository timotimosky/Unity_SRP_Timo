using UnityEngine;

//组织每个物体的参数
[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour {


	public bool dirty = false;

	public Matrix4x4 matrix4X4 = Matrix4x4.identity;


    static int
		baseColorId = Shader.PropertyToID("_BaseColor"),
		cutoffId = Shader.PropertyToID("_Cutoff"),
		metallicId = Shader.PropertyToID("_Metallic"),
		smoothnessId = Shader.PropertyToID("_Smoothness"),
		emissionColorId = Shader.PropertyToID("_EmissionColor");

	static MaterialPropertyBlock arrayBlock;

	[SerializeField]
	Color baseColor = Color.white;

	[SerializeField, Range(0f, 1f)]
	float alphaCutoff = 0.5f, metallic = 0f, smoothness = 0.5f;

	[SerializeField, ColorUsage(false, true)]
	Color emissionColor = Color.black;

	void Awake () {
        mMeshRenderer = GetComponent<MeshRenderer>();
        EnableInstancing();
        SetObjPropertyBlock();
    }

	void OnValidate () {
		SetObjPropertyBlock();
    }

    Material material;
    MeshRenderer mMeshRenderer;
    public bool EnableInstancing()
    {
        material = mMeshRenderer.sharedMaterial;
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


    public void SetObjPropertyBlock()
	{
        mMeshRenderer.GetPropertyBlock(arrayBlock);
        if (arrayBlock == null)
        {
            arrayBlock = new MaterialPropertyBlock();
        }
        arrayBlock.SetColor(baseColorId, baseColor);
        arrayBlock.SetFloat(cutoffId, alphaCutoff);
        arrayBlock.SetFloat(metallicId, metallic);
        arrayBlock.SetFloat(smoothnessId, smoothness);
        arrayBlock.SetColor(emissionColorId, emissionColor);
        mMeshRenderer.SetPropertyBlock(arrayBlock);

        matrix4X4 = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
    }


    public void SetPos()
    {
        matrix4X4.SetColumn(3, transform.position);
    }

    public void SetScale(Vector3 localScale)
    {
        matrix4X4.m00 = Mathf.Max(1, localScale.x);
        matrix4X4.m11 = Mathf.Max(1, localScale.y);
        matrix4X4.m22 = Mathf.Max(1, localScale.z);
    }
}