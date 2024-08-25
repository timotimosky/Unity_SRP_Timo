//#define SAFE_CHECK
//#define ENABLE_UNITY_COLLECTIONS_CHECKS
#undef ENABLE_UNITY_COLLECTIONS_CHECKS
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace ECS
{
    [StructLayout(LayoutKind.Sequential)]
#if ENABLE_UNITY_COLLECTIONS_CHECKS
    [NativeContainer]
#endif 
    unsafe public struct NativeMultiCounter
    {
        // The actual pointer to the allocated count needs to have restrictions relaxed so jobs can be schedled with this container
        [NativeDisableUnsafePtrRestriction]
        int* m_Counter;

        int m_Length;
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


        public NativeMultiCounter( NativeArray<int> externalData )
        {
            m_AllocatorLabel = Allocator.None;

            m_Length  = externalData.Length;
            // Allocate native memory for a single integer
            m_Counter = (int*)externalData.GetUnsafePtr();

            // Create a dispose sentinel to track memory leaks. This also creates the AtomicSafetyHandle
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, Allocator.TempJob);
#endif
            // Initialize the count to 0 to avoid uninitialized data
            Reset();
        }


        public NativeMultiCounter(int count,Allocator label)
        {
            // This check is redundant since we always use an int which is blittable.
            // It is here as an example of how to check for type correctness for generic types.
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (!UnsafeUtility.IsBlittable<int>())
            throw new ArgumentException(string.Format("{0} used in NativeMultiCounter<{0}> must be blittable", typeof(int)));
#endif
            m_AllocatorLabel = label;

            m_Length = count;
            // Allocate native memory for a single integer
            m_Counter = (int*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>()* count, UnsafeUtility.AlignOf<int>(), label);

            // Create a dispose sentinel to track memory leaks. This also creates the AtomicSafetyHandle
#if ENABLE_UNITY_COLLECTIONS_CHECKS
 
        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, label);
 
#endif
            // Initialize the count to 0 to avoid uninitialized data
            Reset();
        }

        public void Reset()
        {
            UnsafeUtility.MemClear( m_Counter, UnsafeUtility.SizeOf<int>()*m_Length );
        }

        internal void Increment(int index)
        {
            // Verify that the caller has write permission on this data.
            // This is the race condition protection, without these checks the AtomicSafetyHandle is useless
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
#if SAFE_CHECK
            if (index < m_Length)
#endif
           (*(m_Counter+index) )++;
        }

        public int Get(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
#if SAFE_CHECK
            if (index < m_Length)
#endif
            return *(m_Counter + index);
        }

        public void Set(int index,int value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
#if SAFE_CHECK
            if (index < m_Length)
#endif
            *(m_Counter + index) = value;
        }

        internal bool IsCreated
        {
            get { return m_Counter != null; }
        }

        public void Dispose()
        {
            // Let the dispose sentinel know that the data has been freed so it does not report any memory leaks
#if ENABLE_UNITY_COLLECTIONS_CHECKS 
      DisposeSentinel.Dispose( ref m_Safety, ref m_DisposeSentinel); 
#endif
            if(m_AllocatorLabel != Allocator.None)
            {
                UnsafeUtility.Free(m_Counter, m_AllocatorLabel);
            }
            m_Counter = null; m_Length = 0;
        }

        public Concurrent ToConcurrent()
        {
            Concurrent concurrent;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
        concurrent.m_Safety = m_Safety;
        AtomicSafetyHandle.UseSecondaryVersion(ref concurrent.m_Safety);
#endif

            concurrent.m_Counter = m_Counter;
            concurrent.m_Length  = m_Length;

            return concurrent;
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        [NativeContainer]
#endif
        // This attribute is what makes it possible to use NativeCounter.Concurrent in a ParallelFor job
        [NativeContainerIsAtomicWriteOnly]
        unsafe public struct Concurrent
        {
            // Copy of the pointer from the full NativeCounter
            [NativeDisableUnsafePtrRestriction]
            internal int* m_Counter;

            internal int m_Length;

            // Copy of the AtomicSafetyHandle from the full NativeCounter. The dispose sentinel is not copied since this inner struct does not own the memory and is not responsible for freeing it
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            internal AtomicSafetyHandle m_Safety;
#endif

            public int Increment( int index )
            {
                // Increment still needs to check for write permissions
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety); 
            if ( index>=0 && index < m_Length)
#endif
                return Interlocked.Increment(ref *(m_Counter+index));
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                else
                {
                  //Debug.LogError("Inc Error:"+index);
                  return 0;
               }
#endif
            }

            public int Get(int index)
            {
                // Increment still needs to check for write permissions
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
                if (index >= 0 && index < m_Length)
#endif
                    return  *(m_Counter + index);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                else
                {
                    ////Debug.LogError("Inc Error:" + index);
                    return 0;
                }
#endif
            }

            public int Decrement(int index)
            {
                // Increment still needs to check for write permissions
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
                if (index >= 0 && index < m_Length)
#endif
                    return Interlocked.Decrement(ref *(m_Counter + index));
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                else
                {
                    // Debug.LogError("Dec Error:" + index);
                    return 0;
                }
#endif
            }
        }
    }
}
