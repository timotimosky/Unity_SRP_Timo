using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

struct CullJob : IJobFor
{
    [ReadOnly]//ͨ������Ϊֻ����������job���з�������
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
