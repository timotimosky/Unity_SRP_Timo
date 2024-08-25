using Unity.Entities;
using UnityEngine;

/// <summary>
/// 父对象；
/// 这个是在Job里可以用的；
/// </summary>
public struct AParent : IComponentData
{
    public Entity Parent;

    public AParent(ASharedParent shareP) { Parent = shareP.Parent; }
}
