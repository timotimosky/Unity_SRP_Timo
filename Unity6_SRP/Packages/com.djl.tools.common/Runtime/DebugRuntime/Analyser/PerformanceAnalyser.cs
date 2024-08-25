using System;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

// ���ڲ������ܵļ�ʱ��
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
        // �������������������Ѿ���ָ�������е�GC�����������Ƕ���Ĵ�
        mCollectionCount = System.GC.CollectionCount(0);

        beginTotalAllocatedMemory = GetCheckMemorySize();

        GC_GetTotalMemory_Begin = GC_GetTotalMemory();
        // Stopwatch�ࣺ�ṩһ�鷽�������ԣ�������׼ȷ�ز�������ʱ��
        // StartNew()����ʼ���µ�Diagnostics.Stopwatchʵ����������ʱ�����㣬Ȼ��ʼ��������ʱ��
        mStopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        // Elapsed���ڻ�ȡ��ǰʵ�������ó���������ʱ��

        float curoff = (System.GC.CollectionCount(0) - mCollectionCount);
        uint totalAllocatedMemory_add = GetCheckMemorySize() - beginTotalAllocatedMemory;
        long offset_gc = GC_GetTotalMemory() - GC_GetTotalMemory_Begin;

        UnityEngine.Debug.LogFormat("Time={0}ms;  GC Count={1};  TotalAllocatedMemory:{2} Bytes;  AllocatedMemory:{4} Bytes;  Tag:{3}", mStopwatch.ElapsedMilliseconds, curoff, totalAllocatedMemory_add, tag, offset_gc);
    }

    public static void PrepareForAnalyser()
    {
        System.GC.Collect(); // ǿ�ƶ����д����м�ʱGC��
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
        System.GC.WaitForPendingFinalizers(); // ����ǰ�̣߳�ֱ�������ս������е��߳���ոö���Ϊֹ��
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
            return (uint)UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();//��ȡΪ������δ�ռ��Ķ��������й��ڴ档
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