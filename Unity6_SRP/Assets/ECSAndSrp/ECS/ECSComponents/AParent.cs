using Unity.Entities;
using UnityEngine;

/// <summary>
/// ������
/// �������Job������õģ�
/// </summary>
public struct AParent : IComponentData
{
    public Entity Parent;

    public AParent(ASharedParent shareP) { Parent = shareP.Parent; }
}
