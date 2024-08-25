using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace ECS.ECS
{
    /// <summary>
    /// Ecs通用
    /// </summary>
    public static class EcsMgr
    {
        public static World CurrentWorld { get; private set; }
        public static EntityManager EntityManager { get; private set; }
        private static EntityCommandBuffer m_commandBuffer;
        public static EntityCommandBuffer commandBuffer
        {
            get
            {
                if (!m_commandBuffer.IsCreated)
                    m_commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
                return m_commandBuffer;
            }
        }

        internal static void UpdateCommandBuffer()
        {
            if (!m_commandBuffer.IsCreated)
                return;

            if (m_commandBuffer.IsEmpty)
                return;
            m_commandBuffer.Playback(EntityManager);
            m_commandBuffer = default;
        }

        public static bool Inited { get; private set; }

        #region 初始化


        public static void InitWorld(string worldName)
        {
            if (Inited)
                return;

            if (CurrentWorld != null && CurrentWorld.IsCreated)
                CurrentWorld?.Dispose();
            CurrentWorld = DefaultWorldInitialization.Initialize(worldName, false);
            EntityManager = CurrentWorld.EntityManager;

            Inited = true;
        }

        public static void Dispose()
        {
            if (!Inited)
                return;

            if (m_commandBuffer.IsCreated)
                m_commandBuffer.Dispose();

            //销毁世界；
            if (CurrentWorld != null && CurrentWorld.IsCreated)
                CurrentWorld.Dispose();
            CurrentWorld = null;
            Inited = false;
        }

        #endregion

        #region 辅助函数

        public static float4x4 GetFloat4x4(this ATransform transform)
        {
            return float4x4.TRS(transform.Position, transform.Rotation, transform.Scale);
        }

        public static float4x4 GetFloat4x4(this ATransform transform, ATransform parent)
        {
            var selfPos = parent.Position + (math.mul(parent.Rotation, transform.Position) * parent.Scale);
            var selfRot = math.mul(parent.Rotation, transform.Rotation);
            var selfScale = transform.Scale * parent.Scale;
            return float4x4.TRS(selfPos, selfRot, selfScale);
        }

        public static void SetComponentData<T>(this Entity entity, T componentData) where T : unmanaged, IComponentData
        {
            if (!entity.IsExist())
                return;
            if (!EntityManager.HasComponent<T>(entity))
                EntityManager.AddComponent<T>(entity);
            EntityManager.SetComponentData(entity, componentData);
        }

        public static T GetComponentData<T>(this Entity entity) where T : unmanaged, IComponentData
        {
            if (!entity.IsExist())
                return default;
            if (!EntityManager.HasComponent<T>(entity))
                return default;
            return EntityManager.GetComponentData<T>(entity);
        }

        public static T GetOrAddComponentData<T>(this Entity entity) where T : unmanaged, IComponentData
        {
            if (EntityManager.HasComponent<T>(entity))
                return EntityManager.GetComponentData<T>(entity);
            var ret = default(T);
            EntityManager.AddComponentData(entity, ret);
            return ret;
        }

        public static void SetOrAddComponentData<T>(this Entity entity, T componentData) where T : unmanaged, IComponentData
        {
            if (EntityManager.HasComponent<T>(entity))
                EntityManager.SetComponentData(entity, componentData);
            EntityManager.AddComponentData(entity, componentData);
        }

        public static DynamicBuffer<T> GetBuffer<T>(this Entity entity, bool isReadOnly = false) where T : unmanaged, IBufferElementData
        {
            return EntityManager.GetBuffer<T>(entity, isReadOnly);
        }

        public static DynamicBuffer<T> AddBuffer<T>(this Entity entity) where T : unmanaged, IBufferElementData
        {
            return EntityManager.AddBuffer<T>(entity);
        }

        public static DynamicBuffer<T> GetOrAddBuffer<T>(this Entity entity) where T : unmanaged, IBufferElementData
        {
            if (EntityManager.HasBuffer<T>(entity))
                return GetBuffer<T>(entity);
            return AddBuffer<T>(entity);
        }

        public static void DestroyImmediately(this Entity entity)
        {
            if (!entity.IsExist())
                return;
            EntityManager.DestroyEntity(entity);
        }

        public static void Destroy(this Entity e)
        {
            if (e.IsExist())
                commandBuffer.DestroyEntity(e);
        }

        public static bool IsExist(this Entity e)
        {
            if (e == Entity.Null)
                return false;
            if (!EntityManager.Exists(e))
                return false;
            return true;
        }

        public static bool IsNaN(this float4x4 val)
        {
            if (math.any(math.isnan(val.c0)))
                return true;
            if (math.any(math.isnan(val.c1)))
                return true;
            if (math.any(math.isnan(val.c2)))
                return true;
            if (math.any(math.isnan(val.c3)))
                return true;
            return false;
        }

        public static Vector3 Position(this Matrix4x4 matrix) { return matrix.GetColumn(3); }

        public static unsafe DynamicBuffer<T> Resize<T>(DynamicBuffer<T> targetAarray, int count) where T : unmanaged
        {
            targetAarray.ResizeUninitialized(count);
            void* destData = targetAarray.GetUnsafePtr();
            UnsafeUtility.MemClear(destData, UnsafeUtility.SizeOf<T>() * count);
            return targetAarray;
        }

        #endregion

    }
}