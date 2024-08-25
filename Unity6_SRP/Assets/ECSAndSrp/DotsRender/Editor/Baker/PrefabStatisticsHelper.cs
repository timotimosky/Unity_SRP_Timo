using ECS.DotsRenderEditor;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ECSFrame.DotsRenderEditor
{
    /// <summary>
    /// 预制统计辅助
    /// </summary>
    public class PrefabStatisticsHelper 
    {

        public PrefabInfo entity;

        private Material material => entity.SharedMaterial;

        private Mesh mesh => entity.SharedMesh;

        private LODGroup lodGroup => entity.lodGroup;
        private bool enableLodGroupCheck => lodGroup != null;

        private bool IsLod0Mesh => entity.IsLod0Mesh;

        private int SubMeshCount => entity.SubMeshCount;


        public ConcurrentDictionary<string, Mesh> m_DictMeshes;
        public ConcurrentDictionary<string, Material> m_DictMaterials;
        public ConcurrentDictionary<string, PrefabStatisticsInfo> DictRenderInfoStatistics;
        public ConcurrentQueue<string> ErrorStr;

        public PrefabStatisticsParam param;

        private List<PrefabStatisticsInfo> ListRenderInfoStatistics => param.ListRenderInfoStatistics;
        private List<PrefabStatisticsErrorInfo> ListErrorPrefab => param.ListErrorPrefab;

        private List<LODGroup> ListLodGroups => param.ListLodGroups;
        private LOD[] lods => entity.Lods;

        public string Prefix_Mat;
        public string Prefix_Mesh;
        public bool CheckGameObjectLayer()
        {
            int layer = 1 << entity.Layer;

            //需要排除的层级
            if (param.ExcludeLayer > 0)
            {
                if ((layer & param.ExcludeLayer) == layer)
                    return false;
            }

            //错误的层级
            if (param.ErrorLayer > 0)
            {
                if ((layer & param.ErrorLayer) == layer)
                {
                    var message = $"{entity.HierarchyName} 出错：不能是层级：{entity.Layer}";
                    ErrorStr.Enqueue(message);
                    ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                    return false;
                }
            }

            return true;
        }

        public bool CheckLodGroup()
        {
            if (lodGroup == null)
                return true;

            //有 Lod 的情况，统一按照Lod0进行处理
            var lod_0 = lods[0];
            var lod0Renders = lod_0.renderers;
            if (lod0Renders == null || lod0Renders.Length == 0)
            {
                var message = $"{entity.HierarchyName} 出错：Lod Group 配置丢失！";
                ErrorStr.Enqueue(message);
                ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                return false;
            }
            var r = lod0Renders[0] as MeshRenderer;

            if (r == null)
            {
                var message = $"{entity.HierarchyName} 出错：没有 Lod 0 配置";
                ErrorStr.Enqueue(message);
                ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                return false;
            }

            for (int i = 0; i < lods.Length; i++)
            {
                var lod = lods[i];

                if (lod.screenRelativeTransitionHeight <= 0)
                {
                    var message = $"{entity.HierarchyName} 出错：有不合理LOD_{i}距离：{lod.screenRelativeTransitionHeight}";
                    ErrorStr.Enqueue(message);
                    ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                    continue;
                }
            }

            return true;
        }

        public bool CheckMesh()
        {
            if (mesh == null)
            {
                var message = $"{entity.HierarchyName} 出错：没有网格";
                ErrorStr.Enqueue(message);
                ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                return false;
            }

            //查找不支持GPU Instance 的类型：
            if (SubMeshCount > 1)
            {
                var message = $"{entity.Name} 有多个网格 ：{entity.SubMeshCount} ";
                ErrorStr.Enqueue(message);
                ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                return false;
            }

            return true;
        }

        public bool CheckMaterial()
        {
            if (material == null)
            {
                var message = $"{entity.HierarchyName} 出错：没有材质球";
                ErrorStr.Enqueue(message);
                ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                return false;
            }

            if (entity.SharedMaterialsCount > 1)
            {
                var message = $"{entity.Name} 有多个材质球 ：{entity.SharedMaterialsCount}";
                ErrorStr.Enqueue(message);
                ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                return false;
            }

            if (!entity.EnableInstancing)
            {
                var message = $"{entity.Name} 材质未开启GPU Instance ：{entity.MaterialName}";
                ErrorStr.Enqueue(message);
                ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                return false;
            }

            return true;
        }

        /// <summary>
        /// /分析重名材质球和网格
        /// </summary>
        /// <returns></returns>
        public bool CheckDuplicateMeshAndMat()
        {
            //分析重名网格
            string meshName = entity.MeshName;
            if (m_DictMeshes.TryGetValue(meshName, out Mesh existMesh))
            {
                if (existMesh != mesh)
                {
                    var message = $"有重名网格：{meshName}";
                    ErrorStr.Enqueue(message);
                    ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                    return false;
                }
            }
            else
                m_DictMeshes[meshName] = mesh;

            //分析重名材质球
            string matName = entity.MaterialName;
            if (m_DictMaterials.TryGetValue(matName, out Material existMaterial))
            {
                if (existMaterial != material)
                {
                    var message = $"有重名材质球：{meshName}";
                    ErrorStr.Enqueue(message);
                    ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                    return false;
                }
            }
            else
                m_DictMaterials[matName] = material;


            if (!string.IsNullOrEmpty(Prefix_Mat))
            {
                if (!matName.StartsWith(Prefix_Mat))
                {
                    var message = $"材质球没有以{Prefix_Mat}开头：{matName}";
                    ErrorStr.Enqueue(message);
                    ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                    return false;
                }
            }

            //网格和材质球不能重名
            if (matName == meshName)
            {
                var message = $"{entity.Name} 材质球和网格重名 ：{entity.MaterialName} | {entity.MeshName}";
                ErrorStr.Enqueue(message);
                ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                return false;
            }

            return true;
        }

        public bool CheckTransform()
        {
            var scale = entity.Scale;
            if (math.any(scale < 0.01f) || math.any(scale > 655.35f))
            {
                var message = $"{entity.HierarchyName} 缩放 {scale} 不符规范，需要在 0.01 ~ 655.35 之间！";
                ErrorStr.Enqueue(message);
                ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置统计信息
        /// </summary>
        /// <param name="checkSameTransform">是否检测有重复摆放的情况</param>
        /// <returns></returns>
        public bool SetStaticsInfo(bool checkSameTransform = true)
        {
            string uniqueKey = "";
                //LocalUtils.GetUniqueKey(entity.MaterialName, entity.MeshName);

            //统计渲染组合的信息结果
            PrefabStatisticsInfo info;
            if (!DictRenderInfoStatistics.TryGetValue(uniqueKey, out info))
            {
                lock (DictRenderInfoStatistics)
                {
                    if (!DictRenderInfoStatistics.TryGetValue(uniqueKey, out info))
                    {
                        info = new PrefabStatisticsInfo()
                        {
                            material = material,
                            mesh = mesh,
                            UniqueKey = uniqueKey,
                            Trans = entity.transform
                        };

                        DictRenderInfoStatistics[uniqueKey] = info;
                        ListRenderInfoStatistics.Add(info);

                        if (enableLodGroupCheck && lodGroup != null && IsLod0Mesh)
                            ListLodGroups?.Add(lodGroup);
                    }
                }
            }

            //不能重复摆放；
            if (checkSameTransform && info.HasSameTransform(entity))
            {
                var message = $"{entity.HierarchyName} 重复摆放 ：{entity.Position} !";
                ErrorStr.Enqueue(message);
                ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(entity.gameObject, message, entity.HierarchyName));
                return false;
            }

            lock (info)
            {
                info.AddRefrenceTarget(entity);
            }
            return true;
        }


    }
}
