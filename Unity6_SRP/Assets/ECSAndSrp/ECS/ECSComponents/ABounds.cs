using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ABounds : IComponentData
{
    public float3 Center;
    public float3 Size;

    public ABounds(Bounds bound)
    {
        Center = bound.center;
        Size = bound.extents;
    }

}
