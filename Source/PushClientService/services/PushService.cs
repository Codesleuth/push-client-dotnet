using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using PushClientService.models;
using PushClientService.threading;

namespace PushClientService.services
{
    public class PushService : IPushService
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(PushService));

        private readonly TaskFactory _taskFactory;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IPayloadPusher _payloadPusher;
        private readonly int _maxPushQueue;
        private readonly LimitedConcurrencyTaskScheduler _taskScheduler;

        public PushService(IPayloadPusher payloadPusher, int concurrentPushes, int maxPushQueue)
        {
            _payloadPusher = payloadPusher;
            _maxPushQueue = maxPushQueue;
            _taskScheduler = new LimitedConcurrencyTaskScheduler(concurrentPushes);
            _taskFactory = new TaskFactory(_taskScheduler);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public bool Push(PushHeaders headers, JObject body)
        {
            if (_taskScheduler.QueuedOrExecutingTasks > _maxPushQueue)
            {
                _log.ErrorFormat("Queued push tasks max reached ({0}); rejecting push.", _taskScheduler.QueuedOrExecutingTasks);
                return false;
            }
            _log.Info("Queueing new push task.");
            _taskFactory.StartNew(PushPayload, new PushPayload(headers, body));
            return true;
        }

        public bool Push(JObject data)
        {
            if (_taskScheduler.QueuedOrExecutingTasks > _maxPushQueue)
            {
                _log.ErrorFormat("Queued push tasks max reached ({0}); rejecting push.", _taskScheduler.QueuedOrExecutingTasks);
                return false;
            }
            _log.Info("Queueing new push task.");
            _taskFactory.StartNew(PushPayload, new PushPayload(new PushHeaders(), data));
            return true;
        }

        private void PushPayload(object data)
        {
            _log.Info("Pushing payload...");
            _payloadPusher.Push(data);
            _log.Info("Payload pushed.");
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }

    public class PushPayload
    {
        public PushHeaders Headers { get; private set; }
        public JObject Body { get; private set; }

        public PushPayload(PushHeaders headers, JObject body)
        {
            Headers = headers;
            Body = body;
        }
    }
}