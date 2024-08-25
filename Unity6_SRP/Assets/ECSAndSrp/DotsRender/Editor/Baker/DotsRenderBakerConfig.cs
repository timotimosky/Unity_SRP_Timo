using System.Collections.Generic;
using UnityEngine;

namespace ECSFrame.DotsRenderEditor
{
    /// <summary>
    /// -渲染部分编辑器工具---烘培配置
    /// </summary>
    [CreateAssetMenu(fileName = "DotsRenderBakerConfig", menuName = "ECSFrame/DotsRender烘培配置", order = 1)]
    public class DotsRenderBakerConfig : ScriptableObject
    {

       // [LabelText("目标预制")]
        public List<GameObject> ListPrefabs = new List<GameObject>();

       // [Button("开始烘焙", ButtonSizes.Medium)]
        public void StartBake()
        {

        }

    }
}