using ECSFrame.ECSFrameBinary;
using ECSFrame.DotsRenderEditor;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using ECS.AsterTask;


namespace ECS.DotsRenderEditor
{
    /// <summary>
    /// 编辑器工具
    /// </summary>
    public static class DotsRenderEditorUtils
    {

        #region 预制统计部分

        public static int StatisticsMeshAndMaterial(PrefabStatisticsParam param)
        {
            var ListRenderInfoStatistics = param.ListRenderInfoStatistics;
            var ListErrorPrefab = param.ListErrorPrefab;
            var listLodGroup = param.ListLodGroups;
            var transform = param.transform;
            var rootPosition = param.RootPosition;

            ListRenderInfoStatistics.Clear();
            ListErrorPrefab.Clear();
            bool enableLodGroupCheck = listLodGroup != null;
            listLodGroup?.Clear();

            var entities = new List<PrefabInfo>();

            var rendereres = transform.GetComponentsInChildren<MeshRenderer>();
            var terrains = transform.GetComponentsInChildren<Terrain>();

            //用于重名分析的字典；
            ConcurrentDictionary<string, Mesh> m_DictMeshes = new ConcurrentDictionary<string, Mesh>();
            ConcurrentDictionary<string, Material> m_DictMaterials = new ConcurrentDictionary<string, Material>();
            ConcurrentDictionary<string, PrefabStatisticsInfo> DictRenderInfoStatistics = new ConcurrentDictionary<string, PrefabStatisticsInfo>();

            #region 对预设部件进行收集

            // 优先预设部件数据
            //if (param.PartGlobalSetting != null && param.PartGlobalSetting.ListPresetPart != null)
            //{
            //    var L = param.PartGlobalSetting.ListPresetPart;
            //    var tempRenderList = new List<MeshRenderer>(3);
            //    for (int i = 0; i < L.Count; i++)
            //    {
            //        var obj = L[i];
            //        if (obj == null)
            //            continue;
            //        EditorUtils.DisplayProgress("预制统计", $"预设部件：{obj}", i, L.Count);
            //        obj.GetComponentsInChildren(tempRenderList);
            //        foreach (var render in tempRenderList)
            //        {
            //            if (render == null)
            //                continue;

            //            var pInfo = new PrefabInfo();
            //            pInfo.SetRender(render);
            //            pInfo.Position = render.transform.position;
            //            pInfo.Rotation = render.transform.rotation;
            //            pInfo.Scale = render.transform.lossyScale;
            //            entities.Add(pInfo);
            //        }
            //    }
            //}

            for (int i = 0; i < rendereres.Length; i++)
            {
                var render = rendereres[i];
                EditorUtils.DisplayProgress("预制统计", $"收集渲染器：{render.gameObject.name}", i, rendereres.Length);
                var noScaleLocalPos = render.transform.position - rootPosition;

                var pInfo = new PrefabInfo();
                pInfo.SetRender(render);
                pInfo.Position = noScaleLocalPos;
                pInfo.Rotation = render.transform.rotation;
                pInfo.Scale = render.transform.lossyScale;

                entities.Add(pInfo);
            }

            int totalTreeCount = 0;
            for (int i = 0; i < terrains.Length; i++)
            {
                var terrain = terrains[i];
                totalTreeCount += terrain.terrainData.treeInstanceCount;
            }
            int curTreeIndex = 0;
            for (int i = 0; i < terrains.Length; i++)
            {
                var terrain = terrains[i];

                var trees = terrain.terrainData.treeInstances;
                var protoTypes = terrain.terrainData.treePrototypes;

                for (int j = 0; j < trees.Length; j++, curTreeIndex++)
                {
                    EditorUtils.DisplayProgress("预制统计", $"收集地形预设：{terrain.gameObject.name}", curTreeIndex, totalTreeCount);
                    var tree = trees[j];
                    var treeId = tree.prototypeIndex;
                    var type = protoTypes[treeId];

                    if (type.prefab == null)
                    {
                        Debug.LogError($"{terrains[i].name} 缺失树预制体 编号 {treeId}");
                        continue;
                    }

                    var trans = type.prefab.transform;

                    var lodGroup = trans.GetComponent<LODGroup>();
                    if (lodGroup != null)
                    {
                        var lods = lodGroup.GetLODs();
                        //这里需要把所有LODGroup的设置都丢进去；
                        foreach (var subLod in lods)
                        {
                            if (subLod.renderers.Length <= 0 || subLod.renderers[0] == null)
                            {
                                var message = $"地形树： {type.prefab} 出错：Lod Group 配置异常！！";
                                Debug.LogError(message);
                                ListErrorPrefab.Add(new PrefabStatisticsErrorInfo(type.prefab, message, type.prefab.transform.GetHierarchyName()));
                                continue;
                            }
                            var r = subLod.renderers[0] as MeshRenderer;
                            if (r != null)
                            {
                                var pInfo = CreatePrefabInfo(r, tree, terrain, rootPosition);
                                entities.Add(pInfo);
                            }
                        }
                        continue;
                    }

                    var render = trans.GetComponent<MeshRenderer>();
                    if (render != null)
                    {
                        var pInfo = CreatePrefabInfo(render, tree, terrain, rootPosition);
                        entities.Add(pInfo);
                    }
                }
            }

            #endregion

            #region 检查所有的预设部件

            int TotalRenderCount = entities.Count;
            bool isParallelFinish = false;
            string curEntityName = null;
            CountdownEvent countdownEvent = new CountdownEvent(TotalRenderCount);
            ConcurrentQueue<string> errorStr = new ConcurrentQueue<string>();

            var task = TaskUtils.Run(() =>
            {
                Parallel.ForEach(entities, entity =>
                {
                    try
                    {
                        curEntityName = entity.Name;
                        if (entity.Renderer != null)
                        {
                            var helper = new PrefabStatisticsHelper()
                            {
                                entity = entity,
                                param = param,
                                m_DictMeshes = m_DictMeshes,
                                m_DictMaterials = m_DictMaterials,
                                DictRenderInfoStatistics = DictRenderInfoStatistics,
                                ErrorStr = errorStr
                            };
                            helper.StatisticsInParallel();
                        }
                        countdownEvent.Signal();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"预制统计：{curEntityName} 出错：{ex.Message}\n{entity.HierarchyName}");
                        Debug.LogError(ex.StackTrace);
                    }
                });

                isParallelFinish = true;
            });

            #endregion

            var stopwatch = Stopwatch.StartNew();
            while (!isParallelFinish)
            {
                long usedTime = stopwatch.ElapsedMilliseconds;
                float curIndex = countdownEvent.InitialCount - countdownEvent.CurrentCount;
                float progress = curIndex / countdownEvent.InitialCount;
                //计算剩余时间；
                float leftTime = usedTime * (1 - progress) / progress / 1000.0f;
                string leftTimeStr = $"{(int)leftTime / 60}分{(int)leftTime % 60}秒";

                EditorUtils.DisplayProgressBar($"正在统计预制({progress * 100:f2}%)->预计剩余时间：[{leftTimeStr}]", $"【{curEntityName}】：{curIndex}/{TotalRenderCount}", progress);
            }
            stopwatch.Stop();

            if (errorStr.Count > 0)
            {
                //打印错误；
                TaskUtils.Run(async () =>
                {
                    int errMsgCount = errorStr.Count;
                    Debug.LogError($"错误开始打印，总共：{errMsgCount}");

                    while (errorStr.Count > 0)
                    {
                        string msg;
                        if (errorStr.TryDequeue(out msg))
                            Debug.LogError(msg);
                        await Task.Delay(10);
                    }

                    UnityEngine.Debug.LogError($"错误打印完成，总共：{errMsgCount}");
                });
            }

            Debug.Log($"所有预制统计完成 --> [{isParallelFinish},{task.IsCompleted},{task.IsCompletedSuccessfully},{task.IsFaulted}]！");
            EditorUtils.ClearProgressBar();

            return TotalRenderCount;
        }

        private static void StatisticsInParallel(this PrefabStatisticsHelper helper)
        {
            if (!helper.CheckGameObjectLayer())
                return;

            if (!helper.CheckLodGroup())
                return;

            if (!helper.CheckMesh())
                return;

            if (!helper.CheckMaterial())
                return;

            if (!helper.CheckDuplicateMeshAndMat())
                return;

            if (!helper.CheckTransform())
                return;

            if (!helper.SetStaticsInfo(false))
                return;
        }

        private static PrefabInfo CreatePrefabInfo(MeshRenderer renderer, TreeInstance tree, Terrain terrain, Vector3 rootPos)
        {
            var pInfo = new PrefabInfo();
            var tPos = terrain.GetPosition();
            var rot = tree.rotation * Mathf.Rad2Deg;

            pInfo.SetRender(renderer);
            pInfo.Position = Vector3.Scale(tree.position, terrain.terrainData.size) + tPos - rootPos;
            pInfo.Rotation = Quaternion.Euler(0, rot, 0);
            pInfo.Scale = new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);

            return pInfo;
        }

        //public static int4 GetLodID(this PartLodConfig config, List<PartConfig_Local> partList)
        //{
        //    int x = partList.GetPartID(config.Lod_0, false);
        //    int y = partList.GetPartID(config.Lod_1, true);
        //    int z = partList.GetPartID(config.Lod_2, true);
        //    int w = partList.GetPartID(config.Lod_3, true);
        //    return new int4(x, y, z, w);
        //}

        //public static int GetPartID(this List<PartConfig_Local> partList, string uniqueKey, bool markInLod = false)
        //{
        //    if (partList == null)
        //        return -1;
        //    if (string.IsNullOrEmpty(uniqueKey))
        //        return -1;

        //    var partConfig = partList.Find(x => x.Key == uniqueKey);
        //    if (partConfig == null)
        //    {
        //        Debug.LogError($"当前列表中没有找到：{uniqueKey}");
        //        return -1;
        //    }
        //    partConfig.IsInLod = markInLod;
        //    return partConfig.PartID;
        //}

        #endregion

        #region 辅助方法

        public static string GetUniqueKey(this MeshRenderer render)
        {
            if (render == null)
            {
                Debug.LogError("MesheRender 为空！");
                return null;
            }

            var meshFliter = render.GetComponent<MeshFilter>();
            if (meshFliter == null)
            {
                Debug.LogError($"{render.gameObject} MeshFilter 为空！");
                return null;
            }

            var mesh = meshFliter.sharedMesh;
            if (mesh == null)
            {
                Debug.LogError($"{render.gameObject} 没有设置网格！");
                return null;
            }

            var mat = render.sharedMaterial;
            if (mat == null)
            {
                Debug.LogError($"{render.gameObject} 没有设置材质球！");
                return null;
            }

            return GetUniqueKey(mat, mesh);
        }

        public static string GetUniqueKey(Material mat, Mesh mesh)
        {
            return $"{mesh.name}_{mat.name}";
        }

        public static string GetUniqueKey(string mat, string mesh)
        {
            return $"{mesh}_{mat}";
        }


        #endregion

    }
}