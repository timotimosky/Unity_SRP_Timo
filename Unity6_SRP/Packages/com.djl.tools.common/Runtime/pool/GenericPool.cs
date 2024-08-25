using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GenericPool<T> where T : class
{
    private Action<T> mReset;                           //重置对象的委托
    private Func<T> mConstruction;                               //创建新对象的委托
    private Stack<T> stack;                             //存放对象的池子，用List等动态数组也可以，推荐泛型数组
    public GenericPool(Func<T> construction, Action<T> mReset = null)
    {
        this.mConstruction = construction;
        this.mReset = mReset;
        stack = new Stack<T>();
    }
    //从池子中获取对象的方法，思路是若池子的数量为0，则调用创建新对象委托创建一个对象返回
    //否则从池子中拿出一个对象并返回
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