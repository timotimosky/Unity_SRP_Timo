using System;
using System.Collections.Concurrent;

namespace ECS.AsterTask
{
    /// <summary>
    /// 并行对象池
    /// </summary>
    public class ConcurrentPool
    {

        public ConcurrentPool() { m_CreateMethod = null; }

        public ConcurrentPool(Func<I_ConcurrentPoolItem> createMethod) { m_CreateMethod = createMethod; }

        private Func<I_ConcurrentPoolItem> m_CreateMethod;

        private ConcurrentBag<I_ConcurrentPoolItem> m_Bag = new ConcurrentBag<I_ConcurrentPoolItem>();

        public T Get<T>() where T : class, I_ConcurrentPoolItem, new()
        {
            I_ConcurrentPoolItem obj;

            //对象池空了，创建一个；
            if (!m_Bag.TryTake(out obj))
            {
                if (m_CreateMethod == null)
                    obj = new T();
                else
                    obj = m_CreateMethod();
                obj.OnCreate();
            }

            obj.OnGet();

#if UNITY_EDITOR
            if (!(obj is T))
                DebugTool.LogError($"{obj} 与目标类型并不相符！");
#endif

            return obj as T;
        }

        public void Put(I_ConcurrentPoolItem obj)
        {
            if (obj == null)
                return;
            obj.OnPut();
            m_Bag.Add(obj);
        }

        public void Clear()
        {
            m_Bag.Clear();
        }

    }
}