using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Ԥ��������
/// ����ϣ��Ԥ������������һ�¹���
/// 1��Ԥ���嶼��GameObject�����������̳�_PoolBaseManager<GameObject>
/// 2����Ϊ�������ʹ�õ��ڴ�ع����ǵ���������ֻ�ܶ�̬����Ԥ���壨ʵ����΢���ӵ���Ϸ����Ҫ�ڴ�ؿ��ƵĶ���Ⱥһ�㶼��Ҫ��̬�ģ������ܿ���Ϸ
/// �����ܶ࣬ÿ�������ϰ����Ԥ���岻ͬ�����ڲ�ͬ����Դ·���£�������ö�̬���ƺ���ͳһ���������Ұ�Ŀǰ��Ҫʹ���ڴ�ص�Ԥ���嶼���ɶ�̬���أ���
/// ��Ȼ��̬���أ�������ЩԤ������ű������Resources��Դ�£��ȴ�load
/// 3������ϣ��Ԥ������Ա�Ԥ�ȼ��أ������ṩԤ�ȼ��صĽӿڣ�
/// </summary>
public class _PoolPrefabManage : __PoolBaseManager<GameObject>
{
    //��ǰ�����Ԥ����Ⱥ����̬���أ�
    public List<GameObject> _prefabs;

    #region ctor
    public _PoolPrefabManage()
    {
        _prefabs = new List<GameObject>();
    }
    #endregion

    void Awake()
    {
        //��ʼ��prefab
        _reLoadPrefabs();
    }

    /// <summary>
    /// ����Ƴ�����������������
    /// </summary>
    public virtual void OnDestroy()
    {
        //���ٹ�������ʱ����Զ�����������ӵĲ���
        //�����ٹ����������ַ�ʽ��1���������л������������ٵ�ʱ���ں��ʵ�ʱ�������ܻ����__DestroyManager������2���������л����������ٵ�ʱ���Զ�ִ�У�
        // Ҳ����˵�����ӹ�����ʵ�����Ĺ����е�����DontDestroyOnLoad(singleton)  ��ô���ں��ʵ�ʱ�����__DestroyManager�����ֶ��Ƴ���
        // �����ӹ�����ʵ�����Ĺ�����û�е���DontDestroyOnLoad(singleton) ��ô�л�������ʱ����Ȼ���߽�������֮��������������ٺ��ͨ���߼�����
        //��������
        __clear();
    }

    /// <summary>
    /// ���������
    /// </summary>
    public override void __destroyManager()
    {
        MonoBehaviour.Destroy(this.gameObject);
    }

    /// <summary>
    /// ʵ������Ӧ���Ǹ����麯��
    /// </summary>
    public override GameObject __instantiate(int _id)
    {
        if (0 == _prefabs.Count) return null;
        //�����Ԥ����Ⱥ��ʵ����
        if (-1 == _id) return (GameObject)MonoBehaviour.Instantiate(_prefabs[Random.Range(0, _prefabs.Count - 1)]);
        else return (GameObject)MonoBehaviour.Instantiate(_prefabs[_id]);
    }
    /// <summary>
    /// ���ɶ������
    /// </summary>
    /// <param name="t">T.</param>
    /// <param name="o">O.</param>
    public override void __produceOneFinish(GameObject o)
    {
        o.SetActive(true);
    }
    /// <summary>
    /// ����,Ԥ��������ϣ��ֱ�����أ��ȴ��´�ʹ��
    /// </summary>
    /// <param name="t">T.</param>
    public override void __recycleAction(GameObject o)
    {
        o.SetActive(false);
    }

    /// <summary>
    /// �������ڴ��У�
    /// </summary>
    /// <param name="t">T.</param>
    public override void __destroy(GameObject o)
    {
        MonoBehaviour.Destroy(o);
    }

    /// <summary>
    /// ����������д������Ԥ������ݲ�һ��
    /// </summary>
    public virtual void _reLoadPrefabs()
    {
    }

    /// <summary>
    /// ��ȡԤ����������
    /// </summary>
    public int _getPrefabsTypesNum()
    {
        return _prefabs.Count;
    }
}