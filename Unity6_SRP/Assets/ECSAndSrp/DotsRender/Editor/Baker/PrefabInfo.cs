using Unity.Mathematics;
using UnityEngine;
using ECS;

namespace ECSFrame.DotsRenderEditor
{
    public struct PrefabInfo
    {
        public float3 Position;
        public quaternion Rotation;
        public float3 Scale;

        //以下是Render的信息
        public MeshRenderer Renderer;
        public string Name;
        public string HierarchyName;
        public int Layer;
        public GameObject gameObject;
        public Transform transform;

        public void SetRender(MeshRenderer render)
        {
            Renderer = render;
            HierarchyName = render.transform.GetHierarchyName();
            SharedMaterialsCount = render.sharedMaterials.Length;
            gameObject = render.gameObject;
            transform = render.transform;
            Layer = render.gameObject.layer;
            Name = render.gameObject.name;
            SetMesh(render.GetComponent<MeshFilter>().sharedMesh);
            SetMaterial(render.sharedMaterial);
            SetLods(render.GetComponentInParent<LODGroup>(true));
        }

        //网格
        public Mesh SharedMesh;
        public int SubMeshCount;
        public string MeshName;

        public void SetMesh(Mesh mesh)
        {
            if (mesh == null)
                return;

            SharedMesh = mesh;
            SubMeshCount = mesh.subMeshCount;
            MeshName = mesh.name;
        }

        //材质球
        public Material SharedMaterial;
        public int SharedMaterialsCount;
        public bool EnableInstancing;
        public string MaterialName;

        public void SetMaterial(Material mat)
        {
            if (mat == null)
                return;

            SharedMaterial = mat;
            EnableInstancing = mat.enableInstancing;
            MaterialName = mat.name;
        }

        //LODGroup
        public LODGroup lodGroup;
        public LOD[] Lods;
        public bool IsLod0Mesh;

        public void SetLods(LODGroup group)
        {
            lodGroup = group;
            if (group == null)
                return;

            Lods = group.GetLODs();
            var r = GetLod0MeshRender(Lods);
            if (r == null)
                return;

            var lodMeshFliter = r.GetComponent<MeshFilter>();
            var lodMesh = lodMeshFliter.sharedMesh;
            IsLod0Mesh = lodMesh == SharedMesh && SharedMaterial == r.sharedMaterial;
        }

        public static MeshRenderer GetLod0MeshRender(LOD[] Lods)
        {
            if (Lods == null || Lods.Length == 0)
                return null;

            var renders = Lods[0].renderers;
            if (renders == null || renders.Length == 0)
                return null;
            return renders[0] as MeshRenderer;
        }

    }
}
