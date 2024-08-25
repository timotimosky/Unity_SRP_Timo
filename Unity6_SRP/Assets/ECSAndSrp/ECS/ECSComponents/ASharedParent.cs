using Unity.Entities;

/// <summary>
/// ����� ComponentData ������Chunk���顣
/// </summary>
public struct ASharedParent : ISharedComponentData
{
    public Entity Parent;

    public ASharedParent(AParent p) { Parent = p.Parent; }
}
