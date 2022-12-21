using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using WebHook.Core.Models;

namespace WebHook.DispatchItemStore.Client.Redis
{
    public class RedisDispatchItemStore : IDispatchItemStore
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly RedisKey _dispatchListKey;
        private readonly RedisKey _inProgressListKey;
        private readonly Dictionary<Guid, RedisValue> _inProgressItems;
        private readonly Queue<DispatchItem> _retryQueue;

        public RedisDispatchItemStore(string connectionString = "localhost", string nodeId = "localnode")
        {
            //TODO HACKY DOCKER FIX
#if !DEBUG
                connectionString = "redis:6379";
#endif

            _redis = ConnectionMultiplexer.Connect(connectionString);
            _dispatchListKey = new RedisKey(nameof(_dispatchListKey));
            _inProgressListKey = new RedisKey(nameof(_inProgressListKey) + nodeId);
            _retryQueue = new Queue<DispatchItem>(GetInProgressList());
            _inProgressItems = new Dictionary<Guid, RedisValue>();
        }

        private IDatabase Getdb()
        {
            return _redis.GetDatabase();
        }

        private IReadOnlyCollection<DispatchItem> GetInProgressList()
        {
            return Getdb().ListRange(_inProgressListKey).Select(rv => {
                DispatchItem returnItem = ToDispatchItem(rv);
                _inProgressItems.Add(returnItem.Id, rv);
                return returnItem;
            }).ToList();
        }

        public void Enqueue(DispatchItem item, TimeSpan delay)
        {
            Task.Factory.StartNew(async () => {
                await Task.Delay(delay);
                _retryQueue.Enqueue(item);
            });
        }

        public IReadOnlyList<DispatchItem> GetNext(int count)
        {
            List<DispatchItem> items = new();
            for (int i = 0; i < count; i++) {
                DispatchItem? item = GetNextOrDefault();

                if (item is null) {
                    return items;
                }

                items.Add(item);
            }
            return items;
        }

        public DispatchItem? GetNextOrDefault()
        {
            if (_retryQueue.Any()) {
                return _retryQueue.Dequeue();
            }

            RedisValue item = Getdb().ListMove(_dispatchListKey, _inProgressListKey, ListSide.Left, ListSide.Right);

            if (item.HasValue is false) {
                return null;
            }

            DispatchItem returnItem = ToDispatchItem(item);
            _inProgressItems.Add(returnItem.Id, item);

            return returnItem;
        }

        public void Enqueue(DispatchItem item)
        {
            RedisValue redisValue = ToRedisValue(item);
            Getdb().ListRightPush(_dispatchListKey, redisValue);
        }

        public void Remove(DispatchItem item)
        {
            if (_inProgressItems.ContainsKey(item.Id)) {
                Getdb().ListRemove(_inProgressListKey, _inProgressItems[item.Id]);
                _inProgressItems.Remove(item.Id);
            }
        }

        private DispatchItem ToDispatchItem(RedisValue value)
        {
            JObject obj = JObject.Parse(value.ToString());
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new EventConverter());
            DispatchItem returnItem = obj.ToObject<DispatchItem>(jsonSerializer);
            return returnItem;
        }

        private RedisValue ToRedisValue(DispatchItem item)
        {
            JObject jObject = JObject.FromObject(item);
            RedisValue redisValue = new(jObject.ToString());
            return redisValue;
        }
    }
}
