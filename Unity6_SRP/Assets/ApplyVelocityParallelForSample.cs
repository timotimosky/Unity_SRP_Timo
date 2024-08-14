using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

class ApplyVelocityParallelForSample : MonoBehaviour
{
    struct VelocityJob : IJobFor
    {
        //作业声明将在作业中访问的所有数据
        //通过声明为只读，允许多个作业并行访问数据
        [ReadOnly]
        public NativeArray<Vector3> velocity;

        //默认情况下，容器被假定为读取&写
        public NativeArray<Vector3> position;


        //增量时间必须复制到作业中，因为作业通常没有帧的概念。
        //主线程等待作业同一帧或下一帧，但是job应该独立于job在工作线程上运行的时间来完成工作。
        public float deltaTime;

        // 在作业中实际运行的代码
        public void Execute(int i)
        {
            //根据时间和速度来移动位置
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

        //安排作业立即在主线程上运行。第一个参数是要执行多少次迭代。
        job.Run(position.Length);


        //调度作业在一个工作线程上稍后运行。
        //第一个参数是每次执行多少次迭代。
        //第二个参数是用于该作业的依赖项的JobHandle。
        //依赖项用于确保在依赖项完成执行后作业在工作线程上执行。
        //在这种情况下，我们不需要我们的工作依赖于任何东西，所以我们可以使用默认的。
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
