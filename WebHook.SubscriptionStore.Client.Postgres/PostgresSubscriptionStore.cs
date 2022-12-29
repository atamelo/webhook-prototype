using System.Reflection;
using Dapper;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using Npgsql;
using StackExchange.Redis;
using WebHook.Core.Events;
using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client.Postgres.StorageModels;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace WebHook.SubscriptionStore.Client.Postgres
{
    public class PostgresSubscriptionStore : ISubscriptionStore
    {
        private readonly NpgsqlConnection _connection;
        private readonly string SubscriptionsTableName;
        private readonly ConnectionMultiplexer _redis;
        private readonly RedisKey subscriptionKey;

        public PostgresSubscriptionStore()
        {
            //TODO load connectionstring from config
            _connection = new NpgsqlConnection("Host=localhost:5432;Database=webhooks;Username=postgres;Password=postgres");
            SubscriptionsTableName = typeof(SubscriptionStorageModel).GetCustomAttribute<TableAttribute>()?.Name
                ?? throw new Exception("Entity must be marked with TableAttribute");
            _redis = ConnectionMultiplexer.Connect("localhost");
            subscriptionKey = new RedisKey("subscriptions");
        }

        private IDatabase database => _redis.GetDatabase();

        //TODO cache layer

        public int CreateSubscription(SubscriptionDto subscriptionDto)
        {
            SubscriptionStorageModel entity = SubscriptionStorageModel.FromDto(subscriptionDto);
            if (entity.id == 0) {
                int id = (int)_connection.Insert(entity);
                return id;
            }
            else {
                throw new Exception("Subscription already has an Id and has been inserted.");
            }
        }

        public void UpdateSubscription(SubscriptionDto subscriptionDto)
        {
            SubscriptionStorageModel entity = SubscriptionStorageModel.FromDto(subscriptionDto);
            if (entity.id > 0) {
                _connection.Update(entity);
                DeleteFromCache(subscriptionDto);
            }
            else {
                throw new Exception("Subscription must have a valid Id to be updated.");
            }
        }

        public void DeleteSubscription(string SubscriberId, int Id)
        {
            SubscriptionDto? Dto = GetSubscriptionFor(SubscriberId, Id);

            if (Dto is null)
                return;

            SubscriptionStorageModel entity = SubscriptionStorageModel.FromDto(Dto);
            _connection.Delete(entity);
            DeleteFromCache(Dto);
        }

        public SubscriptionDto? GetSubscriptionFor(string SubscriberId, int Id)
        {
            RedisValue cached = FindInCache(Id);
            if (cached.HasValue) {
                return ToDto(cached);
            }
            string commandText = $"SELECT * FROM public.\"{SubscriptionsTableName}\" " +
                   $"WHERE {nameof(SubscriptionStorageModel.id)}  = @Id";

            var param = new {
                Id
            };

            SubscriptionStorageModel subscription = _connection.QueryFirstOrDefault<SubscriptionStorageModel>(commandText, param);

            if (subscription is null)
                return null;

            SubscriptionDto Dto = subscription.ToDto();
            AddToCache(Dto);
            return Dto;
        }

        private void AddToCache(SubscriptionDto dto)
        {
            RedisValue idKey = new RedisValue(dto.Id.ToString());
            string value = JsonConvert.SerializeObject(dto);
            RedisValue cacheObj = new RedisValue(value);
            HashEntry[] hashEntries = new HashEntry[] { new(idKey, cacheObj) };
            database.HashSet(subscriptionKey, hashEntries);
        }

        private SubscriptionDto ToDto(RedisValue cached)
        {
            return JsonConvert.DeserializeObject<SubscriptionDto>(cached.ToString());
        }

        private void DeleteFromCache(SubscriptionDto subscriptionDto) =>
            database.HashDelete(subscriptionKey, new RedisValue(subscriptionDto.Id.ToString()));

        private RedisValue FindInCache(int id)
        {
            RedisValue idKey = new RedisValue(id.ToString());
            bool exists = database.HashExists(subscriptionKey, idKey);
            if (exists) {
                return database.HashGet(subscriptionKey, idKey);
            }
            return RedisValue.Null;
        }

        public IReadOnlyCollection<SubscriptionDto> GetSubscriptionsFor(string SubscriberId)
        {
            string commandText = $"SELECT * FROM public.\"{SubscriptionsTableName}\" " +
                   $"WHERE {nameof(SubscriptionStorageModel.subscriber_id)}  = @SubscriberId";

            var param = new {
                SubscriberId
            };

            IEnumerable<SubscriptionStorageModel> subscriptions = _connection.Query<SubscriptionStorageModel>(commandText, param);

            List<SubscriptionDto> dtos = subscriptions.Select(e => e.ToDto()).ToList();
            return dtos;
        }

        public IReadOnlyList<SubscriptionDto> GetActiveSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        {
            string commandText = $"SELECT * FROM {SubscriptionsTableName} " +
                $"WHERE {nameof(SubscriptionStorageModel.event_id)}  = @EventId AND " +
                $"{nameof(SubscriptionStorageModel.subscriber_id)} = @SubscriberId AND " +
                $"{nameof(SubscriptionStorageModel.active)} is true";

            var param = new {
                @event.EventId,
                @event.SubscriberId
            };

            IEnumerable<SubscriptionStorageModel> result = _connection.Query<SubscriptionStorageModel>(commandText, param);

            return result.Select(r => r.ToDto()).ToList();
        }

        public bool IsActive(int id)
        {
            return _connection.Get<SubscriptionStorageModel>(id).active;
        }
    }
}
