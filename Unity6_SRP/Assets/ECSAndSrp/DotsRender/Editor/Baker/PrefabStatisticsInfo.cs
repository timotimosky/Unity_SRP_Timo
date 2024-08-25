using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using ECS;
using ECSFrame.DotsRenderEditor;
namespace ECS.DotsRenderEditor
{
    /// <summary>
    /// 预制统计信息
    /// </summary>
    [Serializable]
    public class PrefabStatisticsInfo 
    {


        [HideInInspector]
        public Transform Trans;

       // [VerticalGroup("Key")]
        public string UniqueKey;

      //  [VerticalGroup("网格")]
        public Mesh mesh;

      //  [VerticalGroup("网格顶点数")]
        public int MeshVertexCount
        {
            get { return mesh == null ? 0 : mesh.vertexCount; }
            set { }
        }

       // [VerticalGroup("材质球")]
        public Material material;
       // [VerticalGroup("引用计数")]
        public int RefrenceCount => RefrenceTargets.Count;

       // [VerticalGroup("缩放边界")]
        public string ScaleMinMaxStr
        {
            get { return $"{ScaleMinMax.x} => {ScaleMinMax.y} "; }
            set { }
        }

       // [VerticalGroup("网格尺寸")]
        public float MeshBounds
        {
            get
            {
                if (mesh == null)
                    return 0;
                return mesh.bounds.extents.magnitude;
            }
            set { }
        }

        //[VerticalGroup("推荐缩放精度")]
        public float SuggestScalePrecision
        {
            get { return 1.0f / MeshBounds; }
            set { }
        }

        [HideInInspector]
        public Vector2 ScaleMinMax = new Vector2(float.MaxValue, float.MinValue);

        /// <summary>
        /// 缓存引用的对象；
        /// </summary>
        [HideInInspector]
        public List<PrefabInfo> RefrenceTargets = new List<PrefabInfo>(2048);

        public void AddRefrenceTarget(PrefabInfo target)
        {
            //if (RefrenceTargets.Contains(target))
            //    return;耗时非常严重，先不判定 Contains

            RefrenceTargets.Add(target);

            float3 scale = target.Scale;
            float scaleMin = math.min(math.min(scale.x, scale.y), scale.z);
            float scaleMax = math.max(math.max(scale.x, scale.y), scale.z);

            ScaleMinMax.x = math.min(ScaleMinMax.x, scaleMin);
            ScaleMinMax.y = math.max(ScaleMinMax.y, scaleMax);
        }

        public bool HasSameTransform(PrefabInfo target)
        {
            bool hasSameTrasform = false;

            Parallel.ForEach(RefrenceTargets, (info, loopState) =>
            {
                if (math.any(info.Position != target.Position))
                    return;

                if (math.any(info.Rotation.value != target.Rotation.value))
                    return;

                if (math.any(info.Scale != target.Scale))
                    return;

                hasSameTrasform = true;
                loopState.Break();
            });

            return hasSameTrasform;
        }

        public void CopyToParent(Transform parent)
        {
            foreach (var item in RefrenceTargets)
            {
                var newObj = UnityEngine.Object.Instantiate(item.Renderer.gameObject, parent, true);
                newObj.name = UniqueKey;
                newObj.transform.localPosition = item.Position;
                newObj.transform.localRotation = item.Rotation;
                newObj.transform.localScale = item.Scale;

                var count = newObj.transform.childCount;
                for (int i = count - 1; i >= 0; i--)
                {
                    var child = newObj.transform.GetChild(i);
                    child.gameObject.Destory();
                }
            }
        }

    }
}
