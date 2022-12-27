using Dapper;
using Dapper.Contrib.Extensions;
using Npgsql;
using WebHook.Core.Events;
using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client.Postgres.Entities;

namespace WebHook.SubscriptionStore.Client.Postgres
{
    public class PostgresSubscriptionStore : ISubscriptionStore
    {
        private readonly NpgsqlConnection _connection;
        private static readonly string SUBSCRIPTIONS_TABLE = "subscriptions";

        public PostgresSubscriptionStore()
        {
            //TODO load connectionstring from config
            _connection = new NpgsqlConnection("Host=localhost:5432;Database=webhooks;Username=postgres;Password=postgres");
        }

        public void Save(SubscriptionDTO subscriptionDTO)
        {
            SubscriptionEntity entity = Map(subscriptionDTO);
            if (entity.id > 0) {
                _connection.Update(entity);
            }
            else {
                _connection.Insert(entity);
            }
        }

        public void Delete(string SubscriberId, int Id)
        {
            SubscriptionDTO dto = Get(SubscriberId, Id);
            SubscriptionEntity entity = Map(dto);
            _connection.Delete(entity);
        }

        public SubscriptionDTO Get(string SubscriberId, int Id)
        {
            string commandText = $"SELECT * FROM public.\"{SUBSCRIPTIONS_TABLE}\" " +
                   $"WHERE {nameof(SubscriptionEntity.id)}  = @Id AND " +
                   $"{nameof(SubscriptionEntity.subscriber_id)} = @SubscriberId";

            var param = new {
                Id,
                SubscriberId
            };

            SubscriptionEntity subscription = _connection.QueryFirst<SubscriptionEntity>(commandText, param);
            SubscriptionDTO dto = Map(subscription);
            return dto;
        }

        public IReadOnlyList<SubscriptionDTO> GetSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        {
            string commandText = $"SELECT * FROM {SUBSCRIPTIONS_TABLE} " +
                $"WHERE {nameof(SubscriptionEntity.event_id)}  = @EventId AND " +
                $"{nameof(SubscriptionEntity.subscriber_id)} = @SubscriberId AND " +
                $"{nameof(SubscriptionEntity.active)} is true";

            var param = new {
                @event.EventId,
                @event.SubscriberId
            };

            IEnumerable<SubscriptionEntity> result = _connection.Query<SubscriptionEntity>(commandText, param);

            return result.Select(Map).ToList();
        }

        //TODO use auto mapper or some other library for this.
        private static SubscriptionDTO Map(SubscriptionEntity s)
        {
            return new SubscriptionDTO {
                Id = s.id,
                Url = s.url,
                Active = s.active,
                EventId = s.event_id,
                SubscriberId = s.subscriber_id
            };
        }

        private static SubscriptionEntity Map(SubscriptionDTO s)
        {
            return new SubscriptionEntity {
                id = s.Id,
                url = s.Url,
                active = s.Active,
                event_id = s.EventId,
                subscriber_id = s.SubscriberId
            };
        }

        public bool IsActive(int id)
        {
            return _connection.Get<SubscriptionEntity>(id).active;
        }
    }
}
