using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public enum InsideResult//视锥体检测的结果
{
    Out=0,//外侧
    In,//包含在内（指整个包围盒都在相机视锥体内）
    Partial//部分包含
};


public enum JonRunMode//job的执行模式
{
    immediatelyInMainThread = 0,//立即在主线程上运行
    RunInJobThread,//在一个工作线程上稍后运行。
    Partial//部分包含
};
/// <summary>
/// 相机包围盒裁剪；
/// </summary>
public struct CullBound 
{

    private NativeArray<float4> frustumPlanes;//给Ecs用的裁剪面
    private NativeArray<float3> centerList;//给Ecs用的裁剪面
    private NativeArray<float> raidusList;//给Ecs用的裁剪面
    private NativeArray<int> ifCullList;//给Ecs用的裁剪面
    private Plane[] CameraSourcePlanes;//原生获得的裁剪面
    private Camera camera; //主相机，需要外部传入

    private int persistentCount;

    public CullBound(Camera camera)
    {
        this.camera= camera;
        CameraSourcePlanes = new Plane[6];//原生获得的裁剪面
        frustumPlanes = new NativeArray<float4>(6, Allocator.Persistent);
        persistentCount = 1023;
        centerList = new NativeArray<float3>(persistentCount, Allocator.Persistent);
        raidusList = new NativeArray<float>(persistentCount, Allocator.Persistent);
        ifCullList = new NativeArray<int>(persistentCount, Allocator.Persistent);
        jobExcuteMode = JonRunMode.RunInJobThread;
    }

    private void Persistent()
    {
        centerList = new NativeArray<float3>(persistentCount, Allocator.Persistent);
        raidusList = new NativeArray<float>(persistentCount, Allocator.Persistent);
        ifCullList = new NativeArray<int>(persistentCount, Allocator.Persistent);
    }

    private void UpdateFrustumPlanes()
    {
        //通过Unity原生API来获取相机裁剪面
        GeometryUtility.CalculateFrustumPlanes(camera, CameraSourcePlanes);
        //这里因为要给ECS用，所以需要转换为Native数据保存。
        for (int i = 0; i < 6; i++)
        {
            var plane = CameraSourcePlanes[i];
            //保存一个平面方程
            frustumPlanes[i] = new float4(plane.normal, plane.distance);
        }
    }

    public bool ShouldCullCell(Vector3 cellPosition, float scale, Vector3 posWS, int CellSize = 10
, float probeCullingDistance = 200.0f)
    {
        var cellSize = scale * CellSize;
        var originWS = posWS;
        Vector3 cellCenterWS = cellPosition * cellSize + originWS + Vector3.one * (cellSize / 2.0f);

        // We do coarse culling with cell, finer culling later.
        float distanceRoundedUpWithCellSize = Mathf.CeilToInt(probeCullingDistance / cellSize) * cellSize;

        //if (Vector3.Distance(camera.transform.position, cellCenterWS) > distanceRoundedUpWithCellSize)
        //    return true;

        var volumeAABB = new Bounds(cellCenterWS, cellSize * Vector3.one);

        return !GeometryUtility.TestPlanesAABB(CameraSourcePlanes, volumeAABB);
    }

    public JonRunMode jobExcuteMode;


    public NativeArray<int> ExcuteCullJob(List<PerObjectMaterialProperties> materialProperties)
    {
        UpdateFrustumPlanes();
        int cout = materialProperties.Count;
        if (persistentCount < cout)
        {
            persistentCount = cout;
            Persistent();
            Debug.LogError("进行一次预分配" + cout);
        }
        for (var i = 0; i < cout; i++)
        {
            PerObjectMaterialProperties mPerObjectMaterialProperties = materialProperties[i];
            if (mPerObjectMaterialProperties == null)
                Debug.LogError("不可思议");
            centerList[i] = mPerObjectMaterialProperties.position;
            raidusList[i] = mPerObjectMaterialProperties.scale.x * 10;
        }

        var job = new CullJob()
        {
            //NativeArray.Copy()
            readFrustumPlanes = frustumPlanes,
            readCenterList = centerList,
            readRaidusList = raidusList,
            out_ifCullList = ifCullList
        };
        //安排作业立即在主线程上运行。第一个参数是要执行多少次迭代。
        job.Run(cout);

        // Native arrays must be disposed manually.
        // centerList.Dispose();

        return job.out_ifCullList;
    }



    public void UpdateCheckJob(JobHandle mJobHandle)
    {

        if (mJobHandle.IsCompleted)
        {
            //OnCompleted();
        }
    }

    public JobHandle ExcuteCullJobHandle(List<PerObjectMaterialProperties> materialProperties)
    {
        UpdateFrustumPlanes();
        int cout = materialProperties.Count;
        if (persistentCount < cout)
        {
            persistentCount = cout;
            Persistent();
            Debug.LogError("进行一次预分配" + cout);
        }
        for (var i = 0; i < cout; i++)
        {
            PerObjectMaterialProperties mPerObjectMaterialProperties = materialProperties[i];
            if (mPerObjectMaterialProperties == null)
                Debug.LogError("不可思议");
            centerList[i] = mPerObjectMaterialProperties.position;
            raidusList[i] = mPerObjectMaterialProperties.scale.x * 10;
        }

        var job = new CullJob()
        {
            //NativeArray.Copy()
            readFrustumPlanes = frustumPlanes,
            readCenterList = centerList,
            readRaidusList = raidusList,
            out_ifCullList = ifCullList
        };
        
            //调度作业在一个工作线程上稍后运行。
            //第一个参数是每次执行多少次迭代。
            //第二个参数是用于该作业的依赖项的JobHandle。
            //依赖项用于确保在依赖项完成执行后作业在工作线程上执行。
            //在这种情况下，我们不需要我们的工作依赖于任何东西，所以我们可以使用默认的。
            JobHandle sheduleJobDependency = new JobHandle();
            JobHandle sheduleJobHandle = job.Schedule(cout,sheduleJobDependency);

            // Schedule job to run on parallel worker threads.
            // First parameter is how many for-each iterations to perform.
            // The second parameter is the batch size,
            //   essentially the no-overhead innerloop that just invokes Execute(i) in a loop.
            //   When there is a lot of work in each iteration then a value of 1 can be sensible.
            //   When there is very little work values of 32 or 64 can make sense.
            // The third parameter is a JobHandle to use for this job's dependencies.
            //   Dependencies are used to ensure that a job executes on worker threads after the dependency has completed execution.
            JobHandle sheduleParralelJobHandle = job.ScheduleParallel(cout, 64, sheduleJobHandle);

            // Ensure the job has completed.
            // It is not recommended to Complete a job immediately,
            // since that reduces the chance of having other jobs run in parallel with this one.
            // You optimally want to schedule a job early in a frame and then wait for it later in the frame.
            sheduleParralelJobHandle.Complete();
       
        return sheduleJobHandle;
    }
}
