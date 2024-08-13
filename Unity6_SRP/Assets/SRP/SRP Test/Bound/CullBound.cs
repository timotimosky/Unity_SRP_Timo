using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public enum InsideResult//��׶����Ľ��
{
    Out,//���
    In,//�������ڣ�ָ������Χ�ж��������׶���ڣ�
    Partial//���ְ���
};

public class CullBound : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private NativeArray<float4> m_FrustumPlanes;//��Ecs�õĲü���
    private Plane[] CameraSourcePlanes = new Plane[6];//ԭ����õĲü���
    private Camera camera; //���������Ҫ�ⲿ����


    private void UpdateFrustumPlanes()
    {
        //ͨ��Unityԭ��API����ȡ����ü���
        GeometryUtility.CalculateFrustumPlanes(camera, CameraSourcePlanes);
        //������ΪҪ��ECS�ã�������Ҫת��ΪNative���ݱ��档
        for (int i = 0; i < 6; i++)
        {
            var plane = CameraSourcePlanes[i];
            //����һ��ƽ�淽��
            m_FrustumPlanes[i] = new float4(plane.normal, plane.distance);
        }
    }

    /// <summary>
    /// ��״�޳�
    /// </summary>
    /// <param name="center">������</param>
    /// <param name="radius">�뾶</param>
    public InsideResult Inside(float3 center, float radius)
    {
        int length = m_FrustumPlanes.Length;
        bool all_in = true;
        for (int i = 0; i < length; i++)
        {
            float4 plane = m_FrustumPlanes[i];
            float3 normal = plane.xyz;
            var distance = math.dot(normal, center) + plane.w;
            if (distance < -radius)
                return InsideResult.Out;

            all_in = all_in && (distance > radius);
        }

        return all_in ? InsideResult.In : InsideResult.Partial;
    }
}
