using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public enum InsideResult//��׶����Ľ��
{
    Out=0,//���
    In,//�������ڣ�ָ������Χ�ж��������׶���ڣ�
    Partial//���ְ���
};


public enum JonRunMode//job��ִ��ģʽ
{
    immediatelyInMainThread = 0,//���������߳�������
    RunInJobThread,//��һ�������߳����Ժ����С�
    Partial//���ְ���
};
/// <summary>
/// �����Χ�вü���
/// </summary>
public struct CullBound 
{

    private NativeArray<float4> frustumPlanes;//��Ecs�õĲü���
    private NativeArray<float3> centerList;//��Ecs�õĲü���
    private NativeArray<float> raidusList;//��Ecs�õĲü���
    private NativeArray<int> ifCullList;//��Ecs�õĲü���
    private Plane[] CameraSourcePlanes;//ԭ����õĲü���
    private Camera camera; //���������Ҫ�ⲿ����

    private int persistentCount;

    public CullBound(Camera camera)
    {
        this.camera= camera;
        CameraSourcePlanes = new Plane[6];//ԭ����õĲü���
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
        //ͨ��Unityԭ��API����ȡ����ü���
        GeometryUtility.CalculateFrustumPlanes(camera, CameraSourcePlanes);
        //������ΪҪ��ECS�ã�������Ҫת��ΪNative���ݱ��档
        for (int i = 0; i < 6; i++)
        {
            var plane = CameraSourcePlanes[i];
            //����һ��ƽ�淽��
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
            Debug.LogError("����һ��Ԥ����" + cout);
        }
        for (var i = 0; i < cout; i++)
        {
            PerObjectMaterialProperties mPerObjectMaterialProperties = materialProperties[i];
            if (mPerObjectMaterialProperties == null)
                Debug.LogError("����˼��");
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
        //������ҵ���������߳������С���һ��������Ҫִ�ж��ٴε�����
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
            Debug.LogError("����һ��Ԥ����" + cout);
        }
        for (var i = 0; i < cout; i++)
        {
            PerObjectMaterialProperties mPerObjectMaterialProperties = materialProperties[i];
            if (mPerObjectMaterialProperties == null)
                Debug.LogError("����˼��");
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
        
            //������ҵ��һ�������߳����Ժ����С�
            //��һ��������ÿ��ִ�ж��ٴε�����
            //�ڶ������������ڸ���ҵ���������JobHandle��
            //����������ȷ�������������ִ�к���ҵ�ڹ����߳���ִ�С�
            //����������£����ǲ���Ҫ���ǵĹ����������κζ������������ǿ���ʹ��Ĭ�ϵġ�
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
