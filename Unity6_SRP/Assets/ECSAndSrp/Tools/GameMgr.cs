using ECS.ECS;
using System;
using UnityEngine;

namespace ECS
{
    public class GameMgr : MonoBehaviourSingleton<GameMgr>
    {

      //  [NonSerialized, ShowInInspector, DisplayAsString, LabelText("游戏时间（RealTime）")]
        internal static float GameTime;
        // ShowInInspector, DisplayAsString, LabelText("当前帧数")
        [NonSerialized]
        internal static int GameFrameCount;

      //  [NonSerialized, ShowInInspector, DisplayAsString, LabelText("目标帧数")]
        internal static int TargetFrameRate=30;

     //   [NonSerialized, ShowInInspector, DisplayAsString, LabelText("当前帧率")]
        internal static float FPS;

        private void Start()
        {
            EcsMgr.InitWorld("EcsWorld");
        }

        private void Update()
        {
            GameTime = Time.realtimeSinceStartup;
            GameFrameCount = Time.frameCount;
            TargetFrameRate = Mathf.Max(1, Application.targetFrameRate);
            FPS = 1 / Time.deltaTime;
            EcsMgr.UpdateCommandBuffer();
        }

        private void OnDestroy()
        {
            EcsMgr.Dispose();
        }
    }
}