using System;
using System.Collections.Concurrent;
using Unity.Mathematics;

namespace ECS.AsterTask
{
    /// <summary>
    /// 命令运行器；
    /// </summary>
    public class AsterCommandRunner : MonoBehaviourSingleton<AsterCommandRunner>
    {

        //[LabelText("每帧允许最多命令数量")]
        public int MaxCommandPerframe = 512;

        //主线程运行的队列
        private ConcurrentQueue<I_AsterCommand> mCommandQueue = new ConcurrentQueue<I_AsterCommand>();
        //主线程运行的最高优先级快速队列；
        private ConcurrentQueue<I_AsterCommand> mFastCommandQueue = new ConcurrentQueue<I_AsterCommand>();
        //不重要的命令
        private ConcurrentQueue<I_AsterCommand> mSlowCommandQueue = new ConcurrentQueue<I_AsterCommand>();
        //多线程的命令，跑在多线程，谨慎选择可执行的命令；
        private ConcurrentQueue<I_AsterCommand> mParallelCommandQueue = new ConcurrentQueue<I_AsterCommand>();


        private void OnDestroy()
        {
            mCommandQueue.Clear();
            mSlowCommandQueue.Clear();
            mParallelCommandQueue.Clear();
            mFastCommandQueue.Clear();
        }

        public void Enqueue(I_AsterCommand cmd)
        {
            if (cmd.InParallel)
                mParallelCommandQueue.Enqueue(cmd);
            else if (cmd.IsSlowCmd)
                mSlowCommandQueue.Enqueue(cmd);
            else if (cmd.IsFascCmd)
                mFastCommandQueue.Enqueue(cmd);
            else
                mCommandQueue.Enqueue(cmd);
        }

        private void Update()
        {
            SetMaxFrameCount();
            UpdateCmd();
        }

        #region 计算每帧执行的命令数

      //  [LabelText("当前允许每帧命令数量")]
        private int CurrentFrameCommandCount;

        public void SetMaxFrameCount()
        {
            float frameRatio = math.clamp(AsterUtils.FPS / AsterUtils.TargetFrameRate, 0, 1);
            CurrentFrameCommandCount = math.max(4, (int)(frameRatio * MaxCommandPerframe));//最少执行4个；
        }

        #endregion

        #region 命令执行

        private void UpdateCmd()
        {
            if (!mParallelCommandQueue.IsEmpty)//避免无意义的线程开销；
                TaskUtils.Run(ExecudeParallelQueue);

            //立即完成所有的快速命令；
            ExecudeCmdQueue(mFastCommandQueue, int.MaxValue);

            if (!mCommandQueue.IsEmpty)
                ExecudeCmdQueue(mCommandQueue, CurrentFrameCommandCount);

            if (AsterUtils.GameFrameCount % 4 == 0)
            {
                int slowCount = 1 + (CurrentFrameCommandCount / 2);
                ExecudeCmdQueue(mSlowCommandQueue, slowCount);
            }
        }

        private void ExecudeParallelQueue()
        {
            ExecudeCmdQueue(mParallelCommandQueue, MaxCommandPerframe);
        }



        public static void MainthreadCallBack(Action callback, bool slow = false, bool fast = false)
        {
            if (callback == null)
                return;

            var cmd = AsterMainthreadCommand.Get();
            cmd.CallBack = callback;
            cmd.SlowCmd = slow;
            cmd.FascCmd = fast;
            AsterCommandRunner.Instance.Enqueue(cmd);
        }

        public static void MainthreadCallBack(Action<object> callback, object param, bool slow = false, bool fast = false)
        {
            if (callback == null)
                return;

            var cmd = AsterMainthreadCommandWithParam.Get();
            cmd.CallBack = callback;
            cmd.param = param;
            cmd.SlowCmd = slow;
            cmd.FascCmd = fast;
            AsterCommandRunner.Instance.Enqueue(cmd);
        }



        private static void ExecudeCmdQueue(ConcurrentQueue<I_AsterCommand> queue, int frameMaxCommanCount = 512)
        {
            int count = queue.Count;
            if (count == 0)
                return;

            count = math.min(count, math.max(count / 3, frameMaxCommanCount));//一次至少要执行1/3的队列，避免一直执行不完；

#if TASK_PROFILE 
            float startTime = AsterUtils.GameTime; 
#endif

            //执行命令
            I_AsterCommand cmd;
            for (int i = 0; i < count; i++)
            {
                if (!queue.TryDequeue(out cmd))
                    continue;

                cmd.Execude();
                cmd.Put();
            }

#if TASK_PROFILE
            float curTime = AsterUtils.GameTime;
            string threadID = Task.CurrentId == null ? "Main" : Task.CurrentId.ToString();
            Logger.Log($"--->[大世界] ：[{threadID}] 执行大世界异步命令*{count}，总耗时：{curTime - startTime} s");
#endif
        }

        #endregion

    }
}