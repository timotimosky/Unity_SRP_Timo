using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public enum InsideResult//视锥体检测的结果
{
    Out,//外侧
    In,//包含在内（指整个包围盒都在相机视锥体内）
    Partial//部分包含
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

    private NativeArray<float4> m_FrustumPlanes;//给Ecs用的裁剪面
    private Plane[] CameraSourcePlanes = new Plane[6];//原生获得的裁剪面
    private Camera camera; //主相机，需要外部传入


    private void UpdateFrustumPlanes()
    {
        //通过Unity原生API来获取相机裁剪面
        GeometryUtility.CalculateFrustumPlanes(camera, CameraSourcePlanes);
        //这里因为要给ECS用，所以需要转换为Native数据保存。
        for (int i = 0; i < 6; i++)
        {
            var plane = CameraSourcePlanes[i];
            //保存一个平面方程
            m_FrustumPlanes[i] = new float4(plane.normal, plane.distance);
        }
    }

    /// <summary>
    /// 球状剔除
    /// </summary>
    /// <param name="center">球中心</param>
    /// <param name="radius">半径</param>
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
