using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

class ApplyVelocityParallelForSample : MonoBehaviour
{
    struct VelocityJob : IJobFor
    {
        //��ҵ����������ҵ�з��ʵ���������
        //ͨ������Ϊֻ������������ҵ���з�������
        [ReadOnly]
        public NativeArray<Vector3> velocity;

        //Ĭ������£��������ٶ�Ϊ��ȡ&д
        public NativeArray<Vector3> position;


        //����ʱ����븴�Ƶ���ҵ�У���Ϊ��ҵͨ��û��֡�ĸ��
        //���̵߳ȴ���ҵͬһ֡����һ֡������jobӦ�ö�����job�ڹ����߳������е�ʱ������ɹ�����
        public float deltaTime;

        // ����ҵ��ʵ�����еĴ���
        public void Execute(int i)
        {
            //����ʱ����ٶ����ƶ�λ��
            position[i] = position[i] + velocity[i] * deltaTime;
        }
    }

    public void Update()
    {
        var position = new NativeArray<Vector3>(500, Allocator.Persistent);

        var velocity = new NativeArray<Vector3>(500, Allocator.Persistent);
        for (var i = 0; i < velocity.Length; i++)
            velocity[i] = new Vector3(0, 10, 0);

        // Initialize the job data
        var job = new VelocityJob()
        {
            deltaTime = Time.deltaTime,
            position = position,
            velocity = velocity
        };

        //������ҵ���������߳������С���һ��������Ҫִ�ж��ٴε�����
        job.Run(position.Length);


        //������ҵ��һ�������߳����Ժ����С�
        //��һ��������ÿ��ִ�ж��ٴε�����
        //�ڶ������������ڸ���ҵ���������JobHandle��
        //����������ȷ�������������ִ�к���ҵ�ڹ����߳���ִ�С�
        //����������£����ǲ���Ҫ���ǵĹ����������κζ������������ǿ���ʹ��Ĭ�ϵġ�
        JobHandle sheduleJobDependency = new JobHandle();
        JobHandle sheduleJobHandle = job.Schedule(position.Length, sheduleJobDependency);

        // Schedule job to run on parallel worker threads.
        // First parameter is how many for-each iterations to perform.
        // The second parameter is the batch size,
        //   essentially the no-overhead innerloop that just invokes Execute(i) in a loop.
        //   When there is a lot of work in each iteration then a value of 1 can be sensible.
        //   When there is very little work values of 32 or 64 can make sense.
        // The third parameter is a JobHandle to use for this job's dependencies.
        //   Dependencies are used to ensure that a job executes on worker threads after the dependency has completed execution.
        JobHandle sheduleParralelJobHandle = job.ScheduleParallel(position.Length, 64, sheduleJobHandle);

        // Ensure the job has completed.
        // It is not recommended to Complete a job immediately,
        // since that reduces the chance of having other jobs run in parallel with this one.
        // You optimally want to schedule a job early in a frame and then wait for it later in the frame.
        sheduleParralelJobHandle.Complete();

        Debug.Log(job.position[0]);

        // Native arrays must be disposed manually.
        position.Dispose();
        velocity.Dispose();
    }
}
