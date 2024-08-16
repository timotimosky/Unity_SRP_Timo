using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public enum InsideResult//��׶����Ľ��
{
    Out=0,//���
    In,//�������ڣ�ָ������Χ�ж��������׶���ڣ�
    Partial//���ְ���
};

public class CullBound 
{
    struct CullJob : IJobFor
    {
        [ReadOnly]
        public NativeArray<float4> readFrustumPlanes;//��Ecs�õĲü���

        [ReadOnly]
        public NativeArray<float3> readCenterList;

        [ReadOnly]
        public NativeArray<float> readRaidusList;

        //Ĭ������£��������ٶ�Ϊ��ȡ&д

        //0:���
        //1:��
        //2:����
        public NativeArray<int> out_ifCullList;


        //����ʱ����븴�Ƶ���ҵ�У���Ϊ��ҵͨ��û��֡�ĸ��
        //���̵߳ȴ���ҵͬһ֡����һ֡������jobӦ�ö�����job�ڹ����߳������е�ʱ������ɹ�����
        public float deltaTime;

        // ����ҵ��ʵ�����еĴ���
        public void Execute(int i)
        {
            out_ifCullList[i] = Inside(readCenterList[i], readRaidusList[i]);
        }
        /// <summary>
        /// ��״�޳�
        /// </summary>
        /// <param name="center">������</param>
        /// <param name="radius">�뾶</param>
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
        /// ���ΰ�Χ���޳�
        /// </summary>
        ///����Ψһ��������ǰ뾶���㷨��ͬ�������ǽ��� extern �ڶ�Ӧ����ƽ���Ͻ���ͶӰ�����뾶��
        ///��ȻҲ�������Ϊ�����嶥�㵽���ĵ������ڷ���ƽ���ϵ�ͶӰ��������㷨��ʹ�õ� c
        ///enter �� extents ����ֱ�Ӵ� BoxCollider �а����ݳ�������
        /// <param name="center">��������</param>
        /// <param name="extents">���ӳߴ磨size��һ�룩</param>
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

    private NativeArray<float4> frustumPlanes;//��Ecs�õĲü���
    private NativeArray<float3> centerList;//��Ecs�õĲü���
    private NativeArray<float> raidusList;//��Ecs�õĲü���
    private NativeArray<int> ifCullList;//��Ecs�õĲü���
    private Plane[] CameraSourcePlanes = new Plane[6];//ԭ����õĲü���
    private Camera camera; //���������Ҫ�ⲿ����

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


    public NativeArray<int> ExcuteCullJob(List<PerObjectMaterialProperties> materialProperties)
    {
        UpdateFrustumPlanes();
        int cout = materialProperties.Count;
        if (persistentCount < cout)
        {
            persistentCount = cout;
            Persistent();
            Debug.LogError("����һ��Ԥ����"+ cout);
        }
        for (var i = 0; i < cout; i++)
        {
            PerObjectMaterialProperties mPerObjectMaterialProperties = materialProperties[i];
            if (mPerObjectMaterialProperties == null)
                Debug.LogError("����˼��");
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
