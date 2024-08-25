 
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Mathematics;

namespace ECS
{
    [StructLayout(LayoutKind.Sequential)]
#if ENABLE_UNITY_COLLECTIONS_CHECKS
    [NativeContainer]
#endif
     public unsafe struct NativeQueueEx
    {
        // The actual pointer to the allocated count needs to have restrictions relaxed so jobs can be schedled with this container
        [NativeDisableUnsafePtrRestriction]
        int* m_Buffer;
        int m_Capacity;
        [NativeDisableUnsafePtrRestriction]
        int* m_Count;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;
        // The dispose sentinel tracks memory leaks. It is a managed type so it is cleared to null when scheduling a job
        // The job cannot dispose the container, and no one else can dispose it until the job has run so it is ok to not pass it along
        // This attribute is required, without it this native container cannot be passed to a job since that would give the job access to a managed object
        [NativeSetClassTypeToNullOnSchedule]
        DisposeSentinel m_DisposeSentinel;
#endif

        // Keep track of where the memory for this was allocated
        Allocator m_AllocatorLabel;

        public NativeQueueEx(int capacity, Allocator label)
        {

            m_AllocatorLabel = label;
            m_Capacity = capacity;
            // Allocate native memory for a single integer
            m_Buffer = (int*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>() * capacity, UnsafeUtility.AlignOf<int>(), label);
            m_Count = (int*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>(), UnsafeUtility.AlignOf<int>(), label);
            // Create a dispose sentinel to track memory leaks. This also creates the AtomicSafetyHandle
#if ENABLE_UNITY_COLLECTIONS_CHECKS

            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, label);

#endif
            // Initialize the count to 0 to avoid uninitialized data
            Clear();
        }

        internal void CreateIndics()
        {
            for (int i = 0; i < m_Capacity; i++)
            {
                *(m_Buffer + i) = i;
            }
            *m_Count = m_Capacity;
        }

        internal void CopyFrom(NativeQueueEx other)
        {
            int other_count = other.Count;
            int count = math.clamp(*m_Count, 0, m_Capacity);
            if (other_count > 0)
            {
                UnsafeUtility.MemCpy(m_Buffer + count, other.GetPtr(), UnsafeUtility.SizeOf<int>() * other_count);

                *m_Count = count + other_count;
            }
            else
                *m_Count = count;
        }

        public void Clear()
        {
            UnsafeUtility.MemClear(m_Buffer, UnsafeUtility.SizeOf<int>() * m_Capacity);
            *m_Count = 0;
        }

        public bool IsCreated { get { return m_Buffer != null; } }

        internal void* GetPtr()
        {
            return m_Buffer;
        }

        public int Count { get { return *m_Count; } }

        public void Dispose()
        {
            // Let the dispose sentinel know that the data has been freed so it does not report any memory leaks
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            UnsafeUtility.Free(m_Count, m_AllocatorLabel);
            UnsafeUtility.Free(m_Buffer, m_AllocatorLabel);

            m_Buffer = null; m_Capacity = 0; m_Count = null;
        }

        public Concurrent ToConcurrent()
        {
            Concurrent concurrent;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            //AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            concurrent.m_Safety = m_Safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref concurrent.m_Safety);
#endif

            concurrent.m_Capacity = m_Capacity;
            concurrent.m_Buffer = m_Buffer;
            concurrent.m_Count = m_Count;

            return concurrent;
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        [NativeContainer]
#endif
        // This attribute is what makes it possible to use NativeCounter.Concurrent in a ParallelFor job
        [NativeContainerIsAtomicWriteOnly]
       public unsafe  struct Concurrent
        {
            // Copy of the pointer from the full NativeCounter
            [NativeDisableUnsafePtrRestriction]
            internal int* m_Buffer;
            internal int m_Capacity;
            [NativeDisableUnsafePtrRestriction]
            internal int* m_Count;

            // Copy of the AtomicSafetyHandle from the full NativeCounter. The dispose sentinel is not copied since this inner struct does not own the memory and is not responsible for freeing it
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            internal AtomicSafetyHandle m_Safety;
#endif

            public void Enqueue(int element)
            {
                // Increment still needs to check for write permissions
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                int idx = Interlocked.Increment(ref (*m_Count)) - 1;
                m_Buffer[idx] = element;
            }

            public int Dequeue()
            {
                // Increment still needs to check for write permissions
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                int idx = Interlocked.Decrement(ref (*m_Count));
                if (idx >= 0)
                {
                    return m_Buffer[idx];
                }

                return -1;
            }
        }
    }
}