using Unity.Entities;

namespace ECS.Sample
{
    /// <summary>
    /// 生成代码
    /// </summary>
    public class SampleBaker : Baker<SampleBakerAuthoring>
    {
        public override void Bake(SampleBakerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ASampleData_ID(authoring.GetIndex));
            var go = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpawnerData() { gameObject = go, SpawnRate = 2 });
        }

    }
}