//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace Game
{
    using System;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Rendering;
    using UnityEngine;
    using UnityEngine.Jobs;
    using UnityEngine.Rendering;

    /// <summary>
    /// The test script for batch render.
    /// </summary>
    public sealed class BatchRender : MonoBehaviour
    {
        [SerializeField]
        private Mesh mesh;

        [SerializeField]
        private Mesh lowMesh;

        [SerializeField]
        private float lodDis;

        [SerializeField]
        private Material material;

        public bool log = false;

        private BatchRendererGroup batchRendererGroup;

        NativeArray<CullData> cullData;
        private void Awake()
        {
            batchRendererGroup = new BatchRendererGroup(this.OnPerformCulling);
            cullData = new NativeArray<CullData>(25000, Allocator.Persistent);

            for (int j = 0; j < 50; j++)
            {
                var pos = new float3[50];
                var rot = new quaternion[50];
                var scale = new float3[50];
                for (int i = 0; i < 50; i++)
                {
                    pos[i] = new float3(i * 2, 0, j*2);
                    rot[i] = quaternion.identity;
                    scale[i] = new float3(1.0f, 1.0f, 1.0f);
                }
                this.AddBatch(100*j, 50, pos, rot, scale);
            }
        }
        public void AddBatch(int offset,int count,float3[] pos, quaternion[] rot, float3[] scale)
        {
            AABB localBond;
            localBond.Center = this.mesh.bounds.center;
            localBond.Extents = this.mesh.bounds.extents;
            var batchIndex = this.batchRendererGroup.AddBatch(
                this.mesh,
                0,
                this.material,
                0,
                ShadowCastingMode.On,
                true,
                false,
                new Bounds(Vector3.zero, 1000 * Vector3.one),
                count,
                null,
                null);
            var matrices = this.batchRendererGroup.GetBatchMatrices(batchIndex);
            for (int i=0;i< count; i++)
            {
                matrices[i] = float4x4.TRS(pos[i], rot[i], scale[i]);
                var aabb = AABB.Transform(matrices[i], localBond);
                cullData[offset + i] = new CullData()
                {
                    bound = aabb,
                    position = pos[i],
                    minDistance = 0,
                    maxDistance = lodDis,
                };
            }

            batchIndex = this.batchRendererGroup.AddBatch(
                this.lowMesh,
                0,
                this.material,
                0,
                ShadowCastingMode.On,
                true,
                false,
                new Bounds(Vector3.zero, 1000 * Vector3.one),
                count,
                null,
                null);
            matrices = this.batchRendererGroup.GetBatchMatrices(batchIndex);
            for (int i = 0; i < count; i++)
            {
                matrices[i] = float4x4.TRS(pos[i], rot[i], scale[i]);
                var aabb = AABB.Transform(matrices[i], localBond);
                cullData[offset + count + i] = new CullData()
                {
                    bound = aabb,
                    position = pos[i],
                    minDistance = lodDis,
                    maxDistance = 10000,
                };
            }
        }
        private void OnDestroy()
        {
            if (this.batchRendererGroup != null)
            {
                cullingDependency.Complete();
                this.batchRendererGroup.Dispose();
                this.batchRendererGroup = null;
                cullData.Dispose();
            }
        }
        JobHandle cullingDependency;
        private JobHandle OnPerformCulling(
            BatchRendererGroup rendererGroup,
            BatchCullingContext cullingContext)
        {
            var planes = FrustumCullPlanes.BuildSOAPlanePackets(cullingContext.cullingPlanes, Allocator.TempJob);
            var lodParams = LODGroupExtensions.CalculateLODParams(cullingContext.lodParameters);
            var cull = new MyCullJob()
            {
                Planes = planes,
                LODParams = lodParams,
                IndexList = cullingContext.visibleIndices,
                Batches = cullingContext.batchVisibility,
                CullDatas = cullData,
            };
            var handle = cull.Schedule(100, 32, cullingDependency);
            cullingDependency = JobHandle.CombineDependencies(handle, cullingDependency);
            return handle;
        }
        struct CullData
        {
            public AABB bound;
            public float3 position;
            public float minDistance;
            public float maxDistance;
        }

        [BurstCompile]
        struct MyCullJob : IJobParallelFor
        {
            [ReadOnly] public LODGroupExtensions.LODParams LODParams;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<FrustumCullPlanes.PlanePacket4> Planes;
            [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<CullData> CullDatas;

            [NativeDisableParallelForRestriction] public NativeArray<BatchVisibility> Batches;
            [NativeDisableParallelForRestriction] public NativeArray<int> IndexList;

            public void Execute(int index)
            {
                var bv = Batches[index];
                var visibleInstancesIndex = 0;
                var isOrtho = LODParams.isOrtho;
                var DistanceScale = LODParams.distanceScale;
                for (int j = 0; j < bv.instancesCount; ++j)
                {
                    var cullData =  CullDatas[index* 50 + j];
                    var rootLodDistance = math.select(DistanceScale * math.length(LODParams.cameraPos - cullData.position), DistanceScale, isOrtho);
                    var rootLodIntersect = (rootLodDistance < cullData.maxDistance) && (rootLodDistance >= cullData.minDistance);
                    if (rootLodIntersect)
                    {
                        var chunkIn = FrustumCullPlanes.Intersect2NoPartial(Planes, cullData.bound);
                        if (chunkIn != FrustumCullPlanes.IntersectResult.Out)
                        {
                            IndexList[bv.offset + visibleInstancesIndex] = j;
                            visibleInstancesIndex++;
                        }
                    }
                }
                bv.visibleCount = visibleInstancesIndex;
                Batches[index] = bv;
            }
        }
    }
}
