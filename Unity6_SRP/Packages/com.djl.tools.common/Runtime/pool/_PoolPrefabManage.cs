using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 预设体管理池
/// 我们希望预设体管理池能有一下功能
/// 1、预设体都是GameObject对象，所以它继承_PoolBaseManager<GameObject>
/// 2、因为最后我们使用的内存池管理都是单例，所以只能动态加载预设体（实际稍微复杂的游戏，需要内存池控制的对象群一般都是要动态的，比如跑酷游戏
/// 场景很多，每个场景障碍物的预设体不同，放在不同的资源路径下，如果不用动态控制很难统一管理，所以我把目前需要使用内存池的预设体都做成动态加载），
/// 既然动态加载，所有这些预设体分门别类放入Resources资源下，等待load
/// 3、我们希望预设体可以被预先加载，所以提供预先加载的接口：
/// </summary>
public class _PoolPrefabManage : __PoolBaseManager<GameObject>
{
    //当前种类的预设体群（动态加载）
    public List<GameObject> _prefabs;

    #region ctor
    public _PoolPrefabManage()
    {
        _prefabs = new List<GameObject>();
    }
    #endregion

    void Awake()
    {
        //初始化prefab
        _reLoadPrefabs();
    }

    /// <summary>
    /// 如果移除管理器，清理链表
    /// </summary>
    public virtual void OnDestroy()
    {
        //销毁管理器的时候会自动调用清理池子的操作
        //（销毁管理器有两种方式：1、当池子切换场景不会销毁的时候，在合适的时机调用总基类的__DestroyManager方法；2、当池子切换场景被销毁的时候自动执行）
        // 也就是说当池子管理器实例化的过程中调用了DontDestroyOnLoad(singleton)  那么请在合适的时候调用__DestroyManager方法手动移除它
        // 当池子管理器实例化的过程中没有调用DontDestroyOnLoad(singleton) 那么切换场景的时候自然会走进来，总之，这边做池子销毁后的通用逻辑即可
        //清理链表
        __clear();
    }

    /// <summary>
    /// 清理管理器
    /// </summary>
    public override void __destroyManager()
    {
        MonoBehaviour.Destroy(this.gameObject);
    }

    /// <summary>
    /// 实例化，应该是个纯虚函数
    /// </summary>
    public override GameObject __instantiate(int _id)
    {
        if (0 == _prefabs.Count) return null;
        //随机从预设体群中实例化
        if (-1 == _id) return (GameObject)MonoBehaviour.Instantiate(_prefabs[Random.Range(0, _prefabs.Count - 1)]);
        else return (GameObject)MonoBehaviour.Instantiate(_prefabs[_id]);
    }
    /// <summary>
    /// 生成对象结束
    /// </summary>
    /// <param name="t">T.</param>
    /// <param name="o">O.</param>
    public override void __produceOneFinish(GameObject o)
    {
        o.SetActive(true);
    }
    /// <summary>
    /// 回收,预设体我们希望直接隐藏，等待下次使用
    /// </summary>
    /// <param name="t">T.</param>
    public override void __recycleAction(GameObject o)
    {
        o.SetActive(false);
    }

    /// <summary>
    /// 清理（从内存中）
    /// </summary>
    /// <param name="t">T.</param>
    public override void __destroy(GameObject o)
    {
        MonoBehaviour.Destroy(o);
    }

    /// <summary>
    /// 方法必须重写，加载预设的内容不一样
    /// </summary>
    public virtual void _reLoadPrefabs()
    {
    }

    /// <summary>
    /// 获取预设种类数量
    /// </summary>
    public int _getPrefabsTypesNum()
    {
        return _prefabs.Count;
    }
}