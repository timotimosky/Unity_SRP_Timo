using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public enum InsideResult//视锥体检测的结果
{
    Out=0,//外侧
    In,//包含在内（指整个包围盒都在相机视锥体内）
    Partial//部分包含
};

public class CullBound 
{
    struct CullJob : IJobFor
    {
        [ReadOnly]
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

    private NativeArray<float4> frustumPlanes;//给Ecs用的裁剪面
    private NativeArray<float3> centerList;//给Ecs用的裁剪面
    private NativeArray<float> raidusList;//给Ecs用的裁剪面
    private NativeArray<int> ifCullList;//给Ecs用的裁剪面
    private Plane[] CameraSourcePlanes = new Plane[6];//原生获得的裁剪面
    private Camera camera; //主相机，需要外部传入

    private int persistentCount = 1023;

    public CullBound(Camera camera)
    {
        this.camera= camera;
        frustumPlanes = new NativeArray<float4>(6, Allocator.Persistent);
        Persistent();
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


    public NativeArray<int> ExcuteCullJob(List<PerObjectMaterialProperties> materialProperties)
    {
        UpdateFrustumPlanes();
        int cout = materialProperties.Count;
        if (persistentCount < cout)
        {
            persistentCount = cout;
            Persistent();
            Debug.LogError("进行一次预分配"+ cout);
        }
        for (var i = 0; i < cout; i++)
        {
            PerObjectMaterialProperties mPerObjectMaterialProperties = materialProperties[i];
            if (mPerObjectMaterialProperties == null)
                Debug.LogError("不可思议");
            centerList[i] = mPerObjectMaterialProperties.position;
            raidusList[i] = mPerObjectMaterialProperties.scale.x*10;
        }

        // Initialize the job data
        var job = new CullJob()
        {
            //NativeArray.Copy()
            readFrustumPlanes = frustumPlanes,
            readCenterList = centerList,
            readRaidusList = raidusList,
            out_ifCullList= ifCullList
        };

        job.Run(cout);
        return job.out_ifCullList;
    }
}
