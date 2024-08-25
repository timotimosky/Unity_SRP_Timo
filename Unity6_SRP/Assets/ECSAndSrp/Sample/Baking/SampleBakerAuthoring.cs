using UnityEngine;

namespace ECS.Sample
{
    /// <summary>
    /// 生成源
    /// </summary>
    public class SampleBakerAuthoring : MonoBehaviour
    {

        [SerializeField]
     //   [LabelText("开始编号")]
        private int StartIndex = 10;

        public int GetIndex { get { return StartIndex; } }

    }

}