using Unity.Entities;

/// <summary>
/// 共享的 ComponentData ；辅助Chunk分组。
/// </summary>
public struct ASharedParent : ISharedComponentData
{
    public Entity Parent;

    public ASharedParent(AParent p) { Parent = p.Parent; }
}
