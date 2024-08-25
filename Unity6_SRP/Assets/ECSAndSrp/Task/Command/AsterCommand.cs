/*
 *Copyright(C) 2023 by Chief All rights reserved.
 *Unity版本：2023.2.5f1c1 
 *作者:Chief  
 *创建日期: 2024-01-19 
 *模块说明：通用并行命令
 *版本: 1.0
*/

namespace ECS.AsterTask
{
    /// <summary>
    /// 通用并行命令
    /// </summary>
    public abstract class AsterCommand<T> : I_ConcurrentPoolItem, I_AsterCommand where T : AsterCommand<T>, new()
    {

        private static ConcurrentPool mPool = new ConcurrentPool(Create);

        private static T Create() { return new T(); }

        public static void ClearAll() { mPool.Clear(); }

        public static T Get()
        {
            return mPool.Get<T>();
        }

        public void Put() { mPool.Put(this); }

        public virtual void OnCreate() { }

        public virtual void OnGet() { }

        public virtual void OnPut() { }

        public abstract void Execude();

        /// <summary>
        /// 在多线程执行此命令
        /// </summary>
        public virtual bool InParallel => false;

        /// <summary>
        /// 缓慢执行的命令（不重要命令）
        /// </summary>
        public virtual bool IsSlowCmd => false;

        /// <summary>
        /// 需要快速执行的命令；
        /// </summary>
        public virtual bool IsFascCmd => false;
    }
}