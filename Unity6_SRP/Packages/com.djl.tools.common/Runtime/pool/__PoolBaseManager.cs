using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 对象池总基类，比如预设体对象池可以继承_PoolbaseManager<GameObject>
/// 这个对象池不提供自动销毁机制，但是提供了计时器，有需要可以写销毁逻辑
/// 对象池是单例模式，所以只支持动态加载
/// 提供了预加载功能接口：__preLoadAnyOne  原理是先实例化内容后放入池中使用
/// </summary>
public class __PoolBaseManager<T> : MonoBehaviour where T : class, new()
{
    //ctor
    public __PoolBaseManager()
    {
        //初始化属性
        __initAttribute();
    }
    private int __F = 0;                           //帧数
    private List<T> __POOLLIST;                        //池子
    public int __MAXCAMACITY { get; set; }   //最大容器数量
    public int __FRAMEDESTROYCOUNT { get; set; }   //每多少帧销毁
    public bool __ISAUTODESTROY { get; set; }   //是否自动销毁

    /// <summary>
    /// 初始化属性，默认池子容量50个，如果自动销毁的话5帧销毁一个
    /// 如果尺寸属性需要修改，直接调用此方法即可
    /// </summary>
    public void __initAttribute(int __mc = 50, int __fd = 5)
    {
        //初始化相关属性 
        __POOLLIST = new List<T>();
        __MAXCAMACITY = __mc;
        __FRAMEDESTROYCOUNT = __fd;
    }

    /// <summary>
    /// 预加载一批对象
    /// __id : 种类
    /// </summary>
    /// <param name="preLoadNum">Pre load number.</param>
    public void __preLoadAnyOne(int __preLoadNum, int __id = -1)
    {
        __MAXCAMACITY = (__preLoadNum > __MAXCAMACITY) ? __preLoadNum : __MAXCAMACITY;
        for (int __i = 0; __i < __preLoadNum; __i++)
        {
            T __t = __instantiate(__id);
            __recycleOne(__t);
        }
    }
    /// <summary>
    /// 预加载一个对象
    /// __id : 种类
    /// </summary>
    /// <param name="preLoadNum">Pre load number.</param>
    public T __preLoadAnyOne(int __id = -1)
    {
        T __t = __instantiate(__id);
        __recycleOne(__t);
        return __t;
    }

    /// <summary>
    /// 生成一个
    /// </summary>
    public T __produceOne(int __id = -1)
    {
        T __t = null;
        if (0 == __POOLLIST.Count)
        {
            __t = __instantiate(__id);
        }
        else
        {
            __t = __POOLLIST[0];
            __POOLLIST.RemoveAt(0);
        }
        __produceOneFinish(__t);
        return __t;
    }

    /// <summary>
    /// 回收
    /// </summary>
    /// <returns>The one.</returns>
    public void __recycleOne(T __t)
    {
        __recycleAction(__t);
        __POOLLIST.Add(__t);
    }

    /// <summary>
    /// 实例化，应该是个纯虚函数
    /// __id 随机的对象池对象种类下标,-1代表随机
    /// </summary>
    public virtual T __instantiate(int __id)
    {
        return new T();
    }
    /// <summary>
    /// 实例化或者重用结束，可以在这个地方重新预加载或者重置状态的方法
    /// </summary>
    /// <param name="t">T.</param>
    public virtual void __produceOneFinish(T __t)
    {
    }
    /// <summary>
    /// 回收，纯虚函数
    /// </summary>
    /// <param name="t">T.</param>
    public virtual void __recycleAction(T __t)
    {
    }
    /// <summary>
    /// 清理管理器
    /// 比如池子中放的是gameobject，那这个方法需要重写：MonoBehaviour.Destroy(this.gameObject);
    /// </summary>
    public virtual void __destroyManager()
    {
    }

    /// <summary>
    /// 从顶部移除
    /// </summary>
    public void __DestroyFromHead()
    {
        //从头部删除
        if (__POOLLIST.Count > 0 && __POOLLIST[0] != null)
        {
            __destroy(__POOLLIST[0]);
            __POOLLIST.RemoveAt(0);
        }
    }

    /// <summary>
    /// 销毁单例
    /// </summary>
    public void __DestroyManager()
    {
        __destroyManager();
    }

    /// <summary>
    /// 清理（从内存中）
    /// </summary>
    /// <param name="t">T.</param>
    public virtual void __destroy(T t)
    {
    }

    /// <summary>
    /// 清理,如果对象没有标记DonotDestroyOnLoad 跳转场景要清理 
    /// </summary>
    public void __clear()
    {
        __POOLLIST.Clear();
    }

    /// <summary>
    /// 自动销毁的计时器
    /// </summary>
    void Update()
    {
        if (__ISAUTODESTROY)
        {
            __F += 1;
            if (__F >= __FRAMEDESTROYCOUNT)
            {
                __F = __F - __FRAMEDESTROYCOUNT;
                __DestroyFromHead();
            }
        }
    }

    /// <summary>
    /// 自动销毁
    /// </summary>
    public void __autoDestroy()
    {
        __ISAUTODESTROY = true;
    }
}