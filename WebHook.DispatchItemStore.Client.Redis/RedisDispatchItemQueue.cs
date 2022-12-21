using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using WebHook.Core.Models;

namespace WebHook.DispatchItemStore.Client.Redis
{
    public class RedisDispatchItemQueue : IDispatchItemQueue
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly RedisKey _dispatchListKey;
        private readonly RedisKey _inProgressHashKey;
        private readonly Dictionary<Guid, RedisValue> _inProgressItems;
        private readonly DelayQueue<DispatchItem> _retryQueue;

        public RedisDispatchItemQueue(string connectionString = "localhost", string nodeId = "localnode")
        {
            //TODO HACKY DOCKER FIX
#if !DEBUG
                connectionString = "redis:6379";
#endif

            _redis = ConnectionMultiplexer.Connect(connectionString);
            _dispatchListKey = new RedisKey(nameof(_dispatchListKey));
            _inProgressHashKey = new RedisKey(nameof(_inProgressHashKey) + nodeId);
            _inProgressItems = new Dictionary<Guid, RedisValue>();
            _retryQueue = new DelayQueue<DispatchItem>(GetInProgressList());
        }

        private IDatabase Getdb()
        {
            return _redis.GetDatabase();
        }

        private IReadOnlyCollection<DispatchItem> GetInProgressList()
        {
            return Getdb().HashGetAll(_inProgressHashKey).Select(rv => {
                RedisValue pointedValue = Getdb().StringGet(new RedisKey(rv.Name.ToString()));
                DispatchItem returnItem = ToDispatchItem(pointedValue);
                _inProgressItems.Add(returnItem.Id, rv.Value);
                return returnItem;
            }).ToList();
        }

        public async Task EnqueueAsync(DispatchItem item, TimeSpan? delay = null)
        {
            if (delay is null) {
                RedisKey redisKey = new(item.Id.ToString());
                RedisValue redisValue = ToRedisValue(item);
                RedisValue keyAsValue = new(item.Id.ToString());

                //using transactions, could also use Lua via Getdb().ScriptEvaluate...
                ITransaction transaction = Getdb().CreateTransaction();
                Task setStringTask = transaction.StringSetAsync(redisKey, redisValue);
                Task setListTask = transaction.ListRightPushAsync(_dispatchListKey, keyAsValue);
                Task executeTask = transaction.ExecuteAsync();
                await Task.WhenAll(setStringTask, setListTask, executeTask);
            }
            else {
                _retryQueue.Enqueue(item, delay.Value);
            }
        }

        public IReadOnlyList<DispatchItem> GetNext(int count)
        {
            List<DispatchItem> items = new();
            for (int i = 0; i < count; i++) {
                DispatchItem? item = GetNextOrDefault().Result;

                if (item is null) {
                    return items;
                }

                items.Add(item);
            }
            return items;
        }

        public async Task<DispatchItem?> GetNextOrDefault()
        {
            if (_retryQueue.Any()) {
                return _retryQueue.Dequeue();
            }
            RedisValue nextKey = Getdb().ListGetByIndex(_dispatchListKey, 0);

            ITransaction transaction = Getdb().CreateTransaction();
            transaction.AddCondition(Condition.ListIndexEqual(_dispatchListKey, 0, nextKey));
            Task listPop = transaction.ListLeftPopAsync(_dispatchListKey);
            Task hashSet = transaction.HashSetAsync(_inProgressHashKey, nextKey, nextKey);
            Task<bool> execute = transaction.ExecuteAsync();
            await Task.WhenAll(listPop, hashSet, execute);

            if (execute.Result is false) {
                //Transaction failed, try again.
                return await GetNextOrDefault();
            }
            if (nextKey.HasValue is false) {
                return null;
            }
            RedisValue valueItem = Getdb().StringGet(new RedisKey(nextKey.ToString()));
            DispatchItem returnItem = ToDispatchItem(valueItem);
            _inProgressItems.Add(returnItem.Id, nextKey);

            return returnItem;
        }

        public void Remove(DispatchItem item)
        {
            if (_inProgressItems.ContainsKey(item.Id)) {
                Getdb().HashDelete(_inProgressHashKey, _inProgressItems[item.Id]);
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