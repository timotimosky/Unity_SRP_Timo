using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/********************************
* Author：	    djl
* Date：       2016/8/6
* Version：	V 0.1.0	
* 
*******************************/
public class TaskManager : MonoBehaviour
{
    private static int MaxTask = 4; //最大执行数

    public static int nowTaskNum = 0; //当前执行数

    private static Queue<IEnumeratorTask> TaskWaitQueue = new Queue<IEnumeratorTask>();

    public static TaskManager singleton;

    //每个阶段，先完成后，停止task，然后加入，再开启task，为了管理方便
    public static bool running;

    void Awake()
    {
        singleton = this;
    }

    /// <summary>
    /// 设置为后台下载
    /// </summary>
    public void SetBack()
    {
        MaxTask = 1;
    }

    /// <summary>
    /// 设置为快速下载
    /// </summary>
    public void SetFast()
    {
        MaxTask = 4;
    }


    //协程是否执行完毕
    public static bool ifOver() 
    {
        //TODO: &running
        return nowTaskNum <= 0 && TaskWaitQueue.Count == 0;
    }

    void Update()
    {
        if (running == false)
        {
            return;
        }

        if (TaskWaitQueue.Count <= 0)
            return;

        if (nowTaskNum < MaxTask)
        {
            IEnumeratorTask executeTask  =TaskWaitQueue.Peek();
            executeTask.DoUpdate();
            nowTaskNum++;
        }
    }

    public static void CreateTask(IEnumerator coroutine, System.Action<IEnumeratorTask> handler = null)
    {
        IEnumeratorTask tIEnumerat = new IEnumeratorTask(coroutine, handler);
        TaskWaitQueue.Enqueue(tIEnumerat);
    }

}