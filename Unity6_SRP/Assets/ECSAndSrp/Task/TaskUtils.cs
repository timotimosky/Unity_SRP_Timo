/*
 *Copyright(C) 2023 by Chief All rights reserved.
 *Unity版本：2023.2.5f1c1 
 *作者:Chief  
 *创建日期: 2024-01-19 
 *模块说明：并行任务通用
 *版本: 1.0
*/

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ECS.AsterTask
{
    /// <summary>
    /// 安全的TPL方法
    /// </summary>
    public static class TaskUtils
    {

        #region 安全的TPL启动

        public static Task Run(Action action, Action callback = null)
        {
            if (action == null)
                return Task.CompletedTask;

            var task = Task.Run(() =>
            {
                try
                {
#if TASK_PROFILE
                    var method = action.Method;
                    if (method == null)
                        return;

                    Profiler.BeginThreadProfiling("GYTask", $"GYTask_{Task.CurrentId}");
                    Profiler.BeginSample(method.GetMethodName());
#endif

                    action?.Invoke();
                    AsterCommandRunner.MainthreadCallBack(callback);

#if TASK_PROFILE
                    Profiler.EndSample();
                    Profiler.EndThreadProfiling();
#endif
                }
                catch (AggregateException ex)
                {
                    var method = action.Method;
                    LogException(ex, method.GetMethodName());
                }
            });

            return task;
        }

        public static Task Run(Func<Task> action, Action callback = null)
        {
            if (action == null)
                return Task.CompletedTask;

            var task = Task.Run(async () =>
            {
                try
                {
#if TASK_PROFILE
                    var method = action.Method;
                    if (method == null)
                        return;

                    Profiler.BeginThreadProfiling("GYTask", $"GYTask_{Task.CurrentId}");
                    Profiler.BeginSample(method.GetMethodName());
#endif
                    //在子线程运行全部任务，并等待完成以收集错误
                    await action?.Invoke();
                    AsterCommandRunner.MainthreadCallBack(callback);

#if TASK_PROFILE
                    Profiler.EndSample();
                    Profiler.EndThreadProfiling();
#endif
                }
                catch (AggregateException ex)
                {
                    var method = action.Method;
                    LogException(ex, method.GetMethodName());
                }
            });

            return task;
        }

        private static void LogException(AggregateException aggregateException, string methodName)
        {
           DebugTool.LogError($"并行任务：{methodName} 出错：{aggregateException.Message}");
           DebugTool.LogError(aggregateException);
        }

        #endregion

        #region 辅助方法

        private static string GetMethodName(this MethodInfo info)
        {
            if (info == null)
                return string.Empty;

            var type = info.DeclaringType;
            var typeNameSpace = type.Namespace;
            var typeName = type.Name;
            var methodName = info.Name;
            return $"{typeNameSpace}.{typeName}.{methodName}";
        }


        #region 异步多线程文件读写工具

        public static Task ReadFromFile(I_AsyncSerializable obj, string filePath)
        {
            var task = Task.Run(async () =>
             {
                 using (FileStream fs = File.OpenRead(filePath))
                 {
                     using (BinaryReader reader = new BinaryReader(fs))
                     {
                         await obj.Read(reader);
                     }
                 }
             });
            return task;
        }

        public static Task ReadFromBytesArray(I_AsyncSerializable obj, byte[] bytes)
        {
            var task = Task.Run(async () =>
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        await obj.Read(reader);
                    }
                }

            });
            return task;
        }

        public static Task WriteToFile(I_AsyncSerializable obj, string filePath)
        {
            var task = Task.Run(async () =>
            {
                using (FileStream fs = File.Open(filePath, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        await obj.Write(writer);
                    }
                }
            });
            return task;
        }

        #endregion

        #endregion

    }
}
