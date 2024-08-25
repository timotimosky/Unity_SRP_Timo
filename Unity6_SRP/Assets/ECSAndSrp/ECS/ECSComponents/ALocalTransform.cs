using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


/// <summary>
/// 本地Transform；
/// 如果有父对象用这个；
/// </summary>
public struct ALocalTransform : IComponentData
{
    public float3 Position;
    public quaternion Rotation;
    public float3 Scale;

    public ALocalTransform(float3 position, quaternion rotation, float3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public ALocalTransform(ATransform transform)
    {
        Position = transform.Position;
        Rotation = transform.Rotation;
        Scale = transform.Scale;
    }

    public ATransform UpdateByParent(ATransform parentTransform)
    {
        float3 nPos = parentTransform.Position + parentTransform.Scale * math.mul(parentTransform.Rotation, Position);
        float3 nScale = parentTransform.Scale * Scale;
        quaternion nRot = math.mul(parentTransform.Rotation, Rotation);
        return new ATransform(nPos, nRot, nScale);
    }

}
