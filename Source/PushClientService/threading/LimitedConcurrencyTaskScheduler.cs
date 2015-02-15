using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace PushClientService.threading
{
    public class LimitedConcurrencyTaskScheduler : TaskScheduler
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(LimitedConcurrencyTaskScheduler));

        private bool _processingQueue;

        private readonly int _maxConcurrencyLevel;
        private readonly LinkedList<Task> _taskQueue;
        private int _executingTasks;

        public LimitedConcurrencyTaskScheduler(int maxConcurrencyLevel)
        {
            _maxConcurrencyLevel = maxConcurrencyLevel;
            _taskQueue = new LinkedList<Task>();
            _executingTasks = 0;
        }

        public override int MaximumConcurrencyLevel
        {
            get { return _maxConcurrencyLevel; }
        }

        public int QueuedOrExecutingTasks
        {
            get { return _taskQueue.Count; }
        }

        protected override void QueueTask(Task task)
        {
            lock (_taskQueue)
            {
                _taskQueue.AddLast(task);
                TriggerWork();
            }
        }

        private void TriggerWork()
        {
            if (_executingTasks == _maxConcurrencyLevel)
                return;

            _executingTasks++;
            ThreadPool.UnsafeQueueUserWorkItem(ProcessQueue, _executingTasks);
        }

        private void ProcessQueue(object state)
        {
            Thread.CurrentThread.Name = "WorkerThread" + (int) state;

            _log.InfoFormat("Worker thread started...");

            _processingQueue = true;
            try
            {
                while (true)
                {
                    var nextTaskIndex = _taskQueue.Count;

                    Task task;
                    if (!TryDequeue(out task)) break;

                    _log.InfoFormat("Executing task {0}.", nextTaskIndex);
                    TryExecuteTask(task);
                }
            }
            finally
            {
                _processingQueue = false;
                _executingTasks--;
                _log.InfoFormat("Worker thread ended.");
            }
        }

        protected bool TryDequeue(out Task task)
        {
            lock (_taskQueue)
            {
                if (_taskQueue.Count == 0)
                {
                    task = null;
                    return false;
                }

                task = _taskQueue.First.Value;
                _taskQueue.RemoveFirst();
                return true;
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!_processingQueue)
                return false;

            if (taskWasPreviouslyQueued && !TryDequeue(task))
                return false;

            return TryExecuteTask(task);
        }

        protected override bool TryDequeue(Task task)
        {
            lock (_taskQueue)
            {
                return _taskQueue.Remove(task);
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(_taskQueue, ref lockTaken);
                if (lockTaken)
                    return _taskQueue;
                else
                    throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_taskQueue);
            }
        }
    }
}