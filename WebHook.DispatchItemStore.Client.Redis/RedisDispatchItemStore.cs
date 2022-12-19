using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace WebHook.DispatchItemStore.Client.Redis
{
    public class RedisDispatchItemStore : IDispatchItemStore
    {
        ConnectionMultiplexer redis;
        IDatabase db => redis.GetDatabase();

        readonly RedisKey dispatchListKey;
        readonly RedisKey inProgressListKey;
        Dictionary<Guid, RedisValue> inProgressItems = new();
        Queue<DispatchItem> retryQueue = new Queue<DispatchItem>();
        public RedisDispatchItemStore(string connectionString = "localhost", string nodeId = "localnode")
        {
            //TODO HACKY DOCKER FIX
            #if !DEBUG
                connectionString = "redis:6379";
            #endif

            redis = ConnectionMultiplexer.Connect(connectionString);
            dispatchListKey = new RedisKey(nameof(dispatchListKey));
            inProgressListKey = new RedisKey(nameof(inProgressListKey) + nodeId);
            retryQueue = new Queue<DispatchItem>(GetInProgressList());
        }
        private IReadOnlyCollection<DispatchItem> GetInProgressList()
        {

             return db.ListRange(inProgressListKey).Select(rv =>
             {
                 DispatchItem returnItem = ToDispatchItem(rv);
                 inProgressItems.Add(returnItem.Id, rv);
                 return returnItem;
             }).ToList();

        }
        public void DelayRequeue(DispatchItem item, TimeSpan delay)
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(delay);
                retryQueue.Enqueue(item);
            });
        }
        public IReadOnlyList<DispatchItem> GetNext(int count)
        {
            List<DispatchItem> items = new();
            for (int i = 0; i < count; i++)
            {
                DispatchItem? item = GetNextOrDefault();
                
                if (item is null) return items;

                items.Add(item);
            }
            return items;
        }
        public DispatchItem? GetNextOrDefault()
        {
            if (retryQueue.Any())
            {
                return retryQueue.Dequeue();
            }

            RedisValue item = db.ListMove(dispatchListKey, inProgressListKey, ListSide.Left, ListSide.Right);

            if (item.HasValue is false)
                return null;

            DispatchItem returnItem = ToDispatchItem(item);
            inProgressItems.Add(returnItem.Id, item);

            return returnItem;
        }
        public void Put(DispatchItem item)
        {
            RedisValue redisValue = ToRedisValue(item);
            db.ListRightPush(dispatchListKey, redisValue);
        }
        public void Remove(DispatchItem item)
        {
            if (inProgressItems.ContainsKey(item.Id))
            {
                db.ListRemove(inProgressListKey, inProgressItems[item.Id]);
                inProgressItems.Remove(item.Id);
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