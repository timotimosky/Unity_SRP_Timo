using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Sample
{
    //*模块说明：Aster示例：同步数据
    public struct ASampleData_ID : IComponentData
    {
        public int ID;

        public ASampleData_ID(int aId) { ID = aId; }
    }

    public struct SpawnerData : IComponentData
    {
        public float SpawnRate;
        public Entity gameObject;
    }


}