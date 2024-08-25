/*
 *Copyright(C) 2023 by Chief All rights reserved.
 *Unity版本：2022.2.0b14 
 *作者:Chief  
 *创建日期: 2022-12-07 
 *模块说明：模板信息
 *版本: 1.0
*/

using Unity.Collections;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace ECS.Sample
{
    /// <summary>
    /// 样例的旋转情况
    /// </summary>
    public partial class SampleRotateSystem : SystemBase
    {

        private EntityQuery mEntityQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

            mEntityQuery = GetEntityQuery(
                ComponentType.ReadOnly<ASampleData_ID>()
                );
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            Dependency = new SampleRotateSystem_Update()
            {
                SampleDataID_RO = GetComponentTypeHandle<ASampleData_ID>(true)
            }.ScheduleParallel(mEntityQuery, Dependency);
        }

        [BurstCompile]
        private struct SampleRotateSystem_Update : IJobChunk
        {
            [ReadOnly] internal ComponentTypeHandle<ASampleData_ID> SampleDataID_RO;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var sampleDataID_array = chunk.GetNativeArray(SampleDataID_RO);
                for (int i = 0; i < chunk.Count; i++)
                {
                    var sampleID = sampleDataID_array[i];

                }
            }
        }

    }
}