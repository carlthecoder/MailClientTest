using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public class TaskOrderScheduler : TaskScheduler
    {
        private readonly LinkedList<Task> tasks = new LinkedList<Task>();

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return tasks;
        }

        protected override void QueueTask(Task task)
        {
            lock (tasks)
            {
                tasks.AddFirst(task);

                ExecuteOnThreadPool();
            }
        }

        private void ExecuteOnThreadPool()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                // Process all available items in the queue.
                while (true)
                {
                    Task item;
                    lock (tasks)
                    {
                        if (tasks.Count == 0)
                            break;

                        item = tasks.First.Value;
                        tasks.RemoveFirst();
                    }

                    TryExecuteTask(item);
                }

            }, null);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return TryExecuteTask(task);
        }
    }
}
