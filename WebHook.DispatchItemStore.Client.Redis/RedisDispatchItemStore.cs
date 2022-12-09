using Newtonsoft.Json;
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
        Dictionary<Guid, RedisValue> inFlightItems = new();
        public RedisDispatchItemStore(string connectionString = "localhost")
        {
            //TODO FIX
            //Hack until I get env variables and config for docker setup
            #if !DEBUG
                connectionString = "redis:6379";
            #endif

            redis = ConnectionMultiplexer.Connect(connectionString);
            dispatchListKey = new RedisKey(nameof(dispatchListKey));


            //Does this need to be a per container list? thinking about processing this list on boot. 
            inFlightListKey = new RedisKey(nameof(inFlightListKey));
        }
        public IReadOnlyCollection<DispatchItem> GetInFlightList()
        {

             return db.ListRange(inFlightListKey).Select(rv =>
             {
                 DispatchItem returnItem = ToDispatchItem(rv);
                 inFlightItems.Add(returnItem.Id, rv);
                 return returnItem;
             }).ToList();

        }
        public DispatchItem? GetNextOrDefault()
        {
            RedisValue item = db.ListMove(dispatchListKey, inFlightListKey, ListSide.Left, ListSide.Right);

            if (item.HasValue is false)
                return null;

            DispatchItem returnItem = ToDispatchItem(item);
            inFlightItems.Add(returnItem.Id, item);

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
            if (inFlightItems.ContainsKey(item.Id))
            {
                db.ListRemove(inFlightListKey, inFlightItems[item.Id]);
            }

        }
        private DispatchItem ToDispatchItem(RedisValue value)
        {
            JObject obj = JObject.Parse(value.ToString());
            var jsonSerializer = new JsonSerializer();
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