using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GenericPool<T> where T : class
{
    private Action<T> mReset;                           //���ö����ί��
    private Func<T> mConstruction;                               //�����¶����ί��
    private Stack<T> stack;                             //��Ŷ���ĳ��ӣ���List�ȶ�̬����Ҳ���ԣ��Ƽ���������
    public GenericPool(Func<T> construction, Action<T> mReset = null)
    {
        this.mConstruction = construction;
        this.mReset = mReset;
        stack = new Stack<T>();
    }
    //�ӳ����л�ȡ����ķ�����˼·�������ӵ�����Ϊ0������ô����¶���ί�д���һ�����󷵻�
    //����ӳ������ó�һ�����󲢷���
    public T New()
    {
        if (stack.Count == 0)
        {
            T t = mConstruction();
            return t;
        }
        else
        {
            T t = stack.Pop();
            if (mReset != null)
                mReset(t);
            return t;
        }
    }

    public void Store(T t)
    {
        stack.Push(t);
    }

    public void Clear()
    {
        stack.Clear();
    }
}