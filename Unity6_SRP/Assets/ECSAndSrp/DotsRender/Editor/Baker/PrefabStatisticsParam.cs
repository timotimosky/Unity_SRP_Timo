using ECS.DotsRenderEditor;
using System.Collections.Generic;
using UnityEngine;

namespace ECSFrame.DotsRenderEditor
{
    /// <summary>
    /// 预制统计所需参数
    /// </summary>
    public class PrefabStatisticsParam
    {

        public List<PrefabStatisticsInfo> ListRenderInfoStatistics;
        public List<PrefabStatisticsErrorInfo> ListErrorPrefab;
        public List<LODGroup> ListLodGroups;

        public Transform transform;
        //需要排除的层级，模型属于这个层级时则跳过统计
        public LayerMask ExcludeLayer = -1;
        //错误的层级，模型属于这个层级这报出错误；
        public LayerMask ErrorLayer = -1;
        /// <summary>
        /// 根节点位置
        /// </summary>
        public Vector3 RootPosition;
        public PartConfigGlobalSetting PartGlobalSetting;

    }
}