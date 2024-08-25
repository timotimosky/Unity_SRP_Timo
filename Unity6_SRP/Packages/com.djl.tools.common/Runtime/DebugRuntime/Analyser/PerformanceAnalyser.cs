using System;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

// 用于测试性能的计时类
public sealed class PerformanceAnalyser : IDisposable
{
    private readonly string tag;
    private readonly int mCollectionCount;
    private readonly Stopwatch mStopwatch;
    private readonly uint beginTotalAllocatedMemory;

    private readonly long GC_GetTotalMemory_Begin;

    public PerformanceAnalyser(string inTag)
    {
        PrepareForAnalyser();
        tag = inTag;
        // 返回自启动进程以来已经对指定代进行的GC次数，参数是对象的代
        mCollectionCount = System.GC.CollectionCount(0);

        beginTotalAllocatedMemory = GetCheckMemorySize();

        GC_GetTotalMemory_Begin = GC_GetTotalMemory();
        // Stopwatch类：提供一组方法和属性，可用于准确地测量运行时间
        // StartNew()：初始化新的Diagnostics.Stopwatch实例，将运行时间置零，然后开始测量运行时间
        mStopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        // Elapsed用于获取当前实例测量得出的总运行时间

        float curoff = (System.GC.CollectionCount(0) - mCollectionCount);
        uint totalAllocatedMemory_add = GetCheckMemorySize() - beginTotalAllocatedMemory;
        long offset_gc = GC_GetTotalMemory() - GC_GetTotalMemory_Begin;

        UnityEngine.Debug.LogFormat("Time={0}ms;  GC Count={1};  TotalAllocatedMemory:{2} Bytes;  AllocatedMemory:{4} Bytes;  Tag:{3}", mStopwatch.ElapsedMilliseconds, curoff, totalAllocatedMemory_add, tag, offset_gc);
    }

    public static void PrepareForAnalyser()
    {
        System.GC.Collect(); // 强制对所有代进行即时GC。
        if (UnityEngine.Scripting.GarbageCollector.isIncremental)
        {
#if UNITY_EDITOR
            PlayerSettings.gcIncremental = false;
#endif
            UnityEngine.Debug.Log("Mono used size " + Profiler.GetMonoUsedSizeLong() + " Bytes, GC.Collect not yet finished");
        }
        else
        {
           // UnityEngine.Debug.Log("GarbageCollector is not Incremental");
        }
        //GarbageCollector.CollectIncremental();
        System.GC.WaitForPendingFinalizers(); // 挂起当前线程，直到处理终结器队列的线程清空该队列为止。
        System.GC.Collect();
    }

    public static float Byte2MB(uint bytes)
    {
        return (float)bytes / (1024 * 1024);
    }

    public static uint GetCheckMemorySize()
    {
        //if (Application.isEditor)
        //    return (uint)UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / 2;
        //else
            return (uint)UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();//获取为活动对象和未收集的对象分配的托管内存。
        // return Profiler.usedHeapSize;
    }
    //public uint GetCheckMemorySize()
    //{
    //    if (Application.isEditor)
    //        return (uint)UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 2;
    //    else
    //        return (uint)UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
    //    // return Profiler.usedHeapSize;
    //}
    public static long GC_GetTotalMemory()
    {
        return System.GC.GetTotalMemory(true);
    }
}