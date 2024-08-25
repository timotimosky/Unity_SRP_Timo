using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ������ܻ��࣬����Ԥ�������ؿ��Լ̳�_PoolbaseManager<GameObject>
/// �������ز��ṩ�Զ����ٻ��ƣ������ṩ�˼�ʱ��������Ҫ����д�����߼�
/// ������ǵ���ģʽ������ֻ֧�ֶ�̬����
/// �ṩ��Ԥ���ع��ܽӿڣ�__preLoadAnyOne  ԭ������ʵ�������ݺ�������ʹ��
/// </summary>
public class __PoolBaseManager<T> : MonoBehaviour where T : class, new()
{
    //ctor
    public __PoolBaseManager()
    {
        //��ʼ������
        __initAttribute();
    }
    private int __F = 0;                           //֡��
    private List<T> __POOLLIST;                        //����
    public int __MAXCAMACITY { get; set; }   //�����������
    public int __FRAMEDESTROYCOUNT { get; set; }   //ÿ����֡����
    public bool __ISAUTODESTROY { get; set; }   //�Ƿ��Զ�����

    /// <summary>
    /// ��ʼ�����ԣ�Ĭ�ϳ�������50��������Զ����ٵĻ�5֡����һ��
    /// ����ߴ�������Ҫ�޸ģ�ֱ�ӵ��ô˷�������
    /// </summary>
    public void __initAttribute(int __mc = 50, int __fd = 5)
    {
        //��ʼ��������� 
        __POOLLIST = new List<T>();
        __MAXCAMACITY = __mc;
        __FRAMEDESTROYCOUNT = __fd;
    }

    /// <summary>
    /// Ԥ����һ������
    /// __id : ����
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
    /// Ԥ����һ������
    /// __id : ����
    /// </summary>
    /// <param name="preLoadNum">Pre load number.</param>
    public T __preLoadAnyOne(int __id = -1)
    {
        T __t = __instantiate(__id);
        __recycleOne(__t);
        return __t;
    }

    /// <summary>
    /// ����һ��
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
    /// ����
    /// </summary>
    /// <returns>The one.</returns>
    public void __recycleOne(T __t)
    {
        __recycleAction(__t);
        __POOLLIST.Add(__t);
    }

    /// <summary>
    /// ʵ������Ӧ���Ǹ����麯��
    /// __id ����Ķ���ض��������±�,-1�������
    /// </summary>
    public virtual T __instantiate(int __id)
    {
        return new T();
    }
    /// <summary>
    /// ʵ�����������ý���������������ط�����Ԥ���ػ�������״̬�ķ���
    /// </summary>
    /// <param name="t">T.</param>
    public virtual void __produceOneFinish(T __t)
    {
    }
    /// <summary>
    /// ���գ����麯��
    /// </summary>
    /// <param name="t">T.</param>
    public virtual void __recycleAction(T __t)
    {
    }
    /// <summary>
    /// ���������
    /// ��������зŵ���gameobject�������������Ҫ��д��MonoBehaviour.Destroy(this.gameObject);
    /// </summary>
    public virtual void __destroyManager()
    {
    }

    /// <summary>
    /// �Ӷ����Ƴ�
    /// </summary>
    public void __DestroyFromHead()
    {
        //��ͷ��ɾ��
        if (__POOLLIST.Count > 0 && __POOLLIST[0] != null)
        {
            __destroy(__POOLLIST[0]);
            __POOLLIST.RemoveAt(0);
        }
    }

    /// <summary>
    /// ���ٵ���
    /// </summary>
    public void __DestroyManager()
    {
        __destroyManager();
    }

    /// <summary>
    /// �������ڴ��У�
    /// </summary>
    /// <param name="t">T.</param>
    public virtual void __destroy(T t)
    {
    }

    /// <summary>
    /// ����,�������û�б��DonotDestroyOnLoad ��ת����Ҫ���� 
    /// </summary>
    public void __clear()
    {
        __POOLLIST.Clear();
    }

    /// <summary>
    /// �Զ����ٵļ�ʱ��
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
    /// �Զ�����
    /// </summary>
    public void __autoDestroy()
    {
        __ISAUTODESTROY = true;
    }
}