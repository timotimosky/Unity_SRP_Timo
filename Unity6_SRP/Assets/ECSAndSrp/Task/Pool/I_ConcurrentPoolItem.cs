namespace ECS.AsterTask
{
    /// <summary>
    /// 并行对象池接口
    /// </summary>
    public interface I_ConcurrentPoolItem 
    {

        /// <summary>
        /// 创建时调用
        /// </summary>
        void OnCreate();

        /// <summary>
        /// 获取时调用
        /// </summary>
        void OnGet();

        /// <summary>
        /// 回收时调用
        /// </summary>
        void OnPut();

    }
}