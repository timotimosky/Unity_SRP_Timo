using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ATransform : IComponentData
{
    public float3 Position;
    public quaternion Rotation;
    public float3 Scale;

    public ATransform(float3 position, quaternion rotation, float3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}
