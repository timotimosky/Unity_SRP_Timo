using System.Collections;
/********************************
 * Author：	    djl
 * Date：       2016//
 * Version：	V 0.1.0	
 * 
 *******************************/

public class IEnumeratorTask
{
    public System.Action<IEnumeratorTask> Finishhandler;

    IEnumerator coroutine;
    public bool running;
    public  bool paused;



    public IEnumeratorTask(IEnumerator c, System.Action<IEnumeratorTask> handler = null)
    {
        Finishhandler = handler;
        coroutine = c;
    }



    public void DoUpdate()
    {
       //deltaTime = Time.deltaTime;
       // _lastUpdateTime += deltaTime;

        // if (running)
        {
            if (paused)
                 return;
            else
            {
                if (coroutine != null && coroutine.MoveNext())
                {
                     return;
                }
                else
                {
                    running = false;
                    if (Finishhandler != null)
                    {
                        Finishhandler(this);
                    }
                    TaskManager.nowTaskNum--;
                }
            }
        }

        //DebugTool.LogJL("协程减少一个 nowTaskNum=" + nowTaskNum);
    }

}