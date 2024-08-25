using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

struct CullJob : IJobFor
{
    [ReadOnly]//通过声明为只读，允许多个job并行访问数据
    public NativeArray<float4> readFrustumPlanes;//给Ecs用的裁剪面

    [ReadOnly]
    public NativeArray<float3> readCenterList;

    [ReadOnly]
    public NativeArray<float> readRaidusList;

    //默认情况下，容器被假定为读取&写

    //0:外侧
    //1:内
    //2:部分
    public NativeArray<int> out_ifCullList;


    //增量时间必须复制到作业中，因为作业通常没有帧的概念。
    //主线程等待作业同一帧或下一帧，但是job应该独立于job在工作线程上运行的时间来完成工作。
    public float deltaTime;

    // 在作业中实际运行的代码
    public void Execute(int i)
    {
        out_ifCullList[i] = Inside(readCenterList[i], readRaidusList[i]);
    }
    /// <summary>
    /// 球状剔除
    /// </summary>
    /// <param name="center">球中心</param>
    /// <param name="radius">半径</param>
    public int Inside(float3 center, float radius)
    {
        int length = readFrustumPlanes.Length;
        bool all_in = true;
        for (int i = 0; i < length; i++)
        {
            float4 plane = readFrustumPlanes[i];
            float3 normal = plane.xyz;
            var distance = math.dot(normal, center) + plane.w;
            if (distance < -radius)
                return 0;

            all_in = all_in && (distance > radius);
        }

        return all_in ? 1 : 2;
    }

    /// <summary>
    /// 方形包围盒剔除
    /// </summary>
    ///这里唯一的区别就是半径的算法不同，盒子是将其 extern 在对应法线平面上进行投影当做半径，
    ///当然也可以理解为立方体顶点到中心的向量在法线平面上的投影。这里的算法中使用的 c
    ///enter 和 extents 可以直接从 BoxCollider 中把数据抄过来。
    /// <param name="center">盒子中心</param>
    /// <param name="extents">外延尺寸（size的一半）</param>
    public InsideResult Inside(float3 center, float3 extents)
    {
        int length = readFrustumPlanes.Length;
        bool all_in = true;
        for (int i = 0; i < length; i++)
        {
            float4 plane = readFrustumPlanes[i];
            float3 normal = plane.xyz;
            float dist = math.dot(normal, center) + plane.w;
            float radius = math.dot(extents, math.abs(normal));
            if (dist <= -radius)
                return InsideResult.Out;

            all_in &= dist > radius;
        }

        return all_in ? InsideResult.In : InsideResult.Partial;
    }
}
