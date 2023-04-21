/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal
{
    using System.Collections.Generic;
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary> Only works at Android runtime. </summary>
    public class AsyncTaskExecuter : SingleTon<AsyncTaskExecuter>
    {
        /// <summary> Queue of tasks. </summary>
        public Queue<Action> m_TaskQueue = new Queue<Action>();

#if !UNITY_EDITOR
        public AsyncTaskExecuter()
        {
            NRDebugger.Info("[AsyncTaskExecuter] Start");
            Thread thread = new Thread(RunAsyncTask);
            thread.IsBackground = true;
            thread.Name = "AsyncTaskExecuter";
            thread.Start();
            NRDebugger.Info("[AsyncTaskExecuter] Started");
        }

        private void RunAsyncTask()
        {
            while (true)
            {
                Thread.Sleep(5);
                if (m_TaskQueue.Count != 0)
                {
                    lock (m_TaskQueue)
                    {
                        var task = m_TaskQueue.Dequeue();
                        try
                        {
                            task?.Invoke();
                        }
                        catch (Exception e)
                        {
                            NRDebugger.Error("[AsyncTaskExecuter] Execute async task error:" + e.ToString());
                            throw;
                        }
                    }
                }
            }
        }
#endif

        /// <summary> Executes the action. </summary>
        /// <param name="task"> The task.</param>
        public void RunAction(Action task)
        {
            lock (m_TaskQueue)
            {
#if !UNITY_EDITOR
                m_TaskQueue.Enqueue(task);
#else
                task?.Invoke();
#endif
            }
        }

        /// <summary> Executes a task witch has a timeout opration. </summary>
        /// <param name="task">            The task.</param>
        /// <param name="timeoutOpration"> The timeout opration.If the task does not time out, it is not
        ///                                executed.</param>
        /// <param name="timeout">           The duration of timeout.</param>
        /// <param name="runInMainThread"> Run the action in unity main thread.</param>
        internal void RunAction(Action task, Action timeoutOpration, float timeout, bool runInMainThread)
        {
            var cancleToken = new CancellationTokenSource();
            if (timeout > 0 && timeoutOpration != null)
            {
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay((int)(timeout * 1000));
                    if (cancleToken.IsCancellationRequested)
                    {
                        return;
                    }
                    try
                    {
                        NRDebugger.Info("[AsyncTaskExecuter] Run action timeout...");
                        timeoutOpration?.Invoke();
                    }
                    catch (Exception e)
                    {
                        NRDebugger.Error("[AsyncTaskExecuter] Run action timeout exeption: {0}\n{1}", e.Message, e.StackTrace);
                    }
                }, cancleToken.Token);
            }

            if (runInMainThread)
            {
                MainThreadDispather.QueueOnMainThread(() =>
                {
                    try
                    {
                        task?.Invoke();
                    }
                    catch (Exception e)
                    {
                        NRDebugger.Error("[AsyncTaskExecuter] Run action in main thread exeption: {0}\n{1}", e.Message, e.StackTrace);
                    }
                    cancleToken.Cancel();
                });
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        task?.Invoke();
                    }
                    catch (Exception e)
                    {
                        NRDebugger.Error("[AsyncTaskExecuter] Run action exeption: {0}\n{1}", e.Message, e.StackTrace);
                    }
                    cancleToken.Cancel();
                });
            }
        }
    }
}