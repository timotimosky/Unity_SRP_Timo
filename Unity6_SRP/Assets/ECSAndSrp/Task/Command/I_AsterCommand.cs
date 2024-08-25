namespace ECS.AsterTask
{
    /// <summary>
    /// 并行命令接口；
    /// 线程安全；
    /// </summary>
    public interface I_AsterCommand 
    {

        /// <summary>
        /// 执行命令，一般来讲，执行完成之后直接回收
        /// </summary>
        void Execude();

        /// <summary>
        /// 回收
        /// </summary>
        void Put();

        /// <summary>
        /// 并行（在多线程）执行
        /// </summary>
        bool InParallel { get; }

        /// <summary>
        /// 慢速执行；
        /// 一般是不重要任务；
        /// </summary>
        bool IsSlowCmd { get; }

        /// <summary>
        /// 最高优先级的快速任务，需要第一时间立即完成；
        /// </summary>
        bool IsFascCmd { get; }

    }
}