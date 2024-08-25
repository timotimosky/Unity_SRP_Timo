using System;
using UnityEngine;

namespace ECSFrame.DotsRenderEditor
{
    /// <summary>
    /// 错误信息
    /// </summary>
    [Serializable]
    public class PrefabStatisticsErrorInfo 
    {
 
      //  [VerticalGroup("模型")]
        public GameObject ErrorTarget;

        //[VerticalGroup("错误信息")]
        public string ErrorMessage;



       // [VerticalGroup("路径")]
        public string Path;

        public PrefabStatisticsErrorInfo(GameObject errorTarget, string error, string path)
        {
            ErrorTarget = errorTarget;
            ErrorMessage = error;
            Path = path;
        }

    }
}
