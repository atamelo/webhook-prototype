using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Runtime.InteropServices.JavaScript;

namespace WebHook.DispatchItemStore.Client.Redis
{
    public class RedisDispatchItemStore : IDispatchItemStore
    {
        ConnectionMultiplexer redis;
        IDatabase db => redis.GetDatabase();

        readonly RedisKey dispatchListKey;
        readonly RedisKey inFlightListKey;
        public RedisDispatchItemStore(string connectionString = "localhost")
        {
            redis = ConnectionMultiplexer.Connect(connectionString);
            dispatchListKey = new RedisKey(nameof(dispatchListKey));
            inFlightListKey = new RedisKey(nameof(inFlightListKey));
        }
        public DispatchItem GetNext()
        {
            RedisValue item = db.ListMove(dispatchListKey, inFlightListKey, ListSide.Left, ListSide.Right);
            DispatchItem returnItem = ToDispatchItem(item);
            return returnItem;
        }        
        public void PersistChanges()
        {

        }
        public void Put(DispatchItem item)
        {
            RedisValue redisValue = ToRedisValue(item);
            db.ListRightPush(dispatchListKey, redisValue);
        }
        public void Remove(DispatchItem item)
        {
            RedisValue redisValue = ToRedisValue(item);
            db.ListRemove(inFlightListKey, redisValue);
        }
        private DispatchItem ToDispatchItem(RedisValue value)
        {
            JObject obj = JObject.Parse(value.ToString());
            DispatchItem returnItem = obj.ToObject<DispatchItem>();
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