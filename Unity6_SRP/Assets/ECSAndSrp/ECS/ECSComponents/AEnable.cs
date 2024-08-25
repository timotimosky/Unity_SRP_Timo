using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal struct AEnable : IComponentData
{
    public bool ParentEnable;

    public bool EnableSelf;

    public bool Enable => EnableSelf && ParentEnable;

    public AEnable(bool enable)
    {
        ParentEnable = true;
        EnableSelf = enable;
    }

}