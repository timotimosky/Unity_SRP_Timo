using System;

namespace ECS.AsterTask
{
    /// <summary>
    /// 主线程调用的回调函数
    /// </summary>
    public class AsterMainthreadCommand : AsterCommand<AsterMainthreadCommand>
    {
        public override bool InParallel => false;

        public bool SlowCmd = false;

        public override bool IsSlowCmd => SlowCmd;

        public bool FascCmd = false;
        public override bool IsFascCmd => FascCmd;


        public Action CallBack;

        public override void Execude()
        {
            CallBack?.Invoke();
        }
    }

    public class AsterMainthreadCommandWithParam : AsterCommand<AsterMainthreadCommandWithParam>
    {

        public override bool InParallel => false;

        public bool SlowCmd = false;

        public override bool IsSlowCmd => SlowCmd;

        public bool FascCmd = false;
        public override bool IsFascCmd => FascCmd;


        public Action<object> CallBack;
        public object param;

        public override void Execude()
        {
            CallBack?.Invoke(param);
        }
    }

}