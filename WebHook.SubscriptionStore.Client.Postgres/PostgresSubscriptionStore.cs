using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using AutoMapper;
using Dapper;
using Dapper.Contrib.Extensions;
using Npgsql;
using WebHook.Core.Events;
using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client.Postgres.Entities;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace WebHook.SubscriptionStore.Client.Postgres
{
    public class PostgresSubscriptionStore : ISubscriptionStore
    {
        private readonly NpgsqlConnection _connection;
        private readonly IMapper _mapper;
        private readonly string SubscriptionsTableName;

        public PostgresSubscriptionStore(IMapper mapper)
        {
            //TODO load connectionstring from config
            _connection = new NpgsqlConnection("Host=localhost:5432;Database=webhooks;Username=postgres;Password=postgres");
            _mapper = mapper;
            SubscriptionsTableName = typeof(SubscriptionEntity).GetCustomAttribute<TableAttribute>()?.Name
                ?? throw new Exception("Entity must be marked with TableAttribute");
        }

        public void Save(SubscriptionDto subscriptionDto)
        {
            SubscriptionEntity entity = _mapper.Map<SubscriptionEntity>(subscriptionDto);
            if (entity.id > 0) {
                _connection.Update(entity);
            }
            else {
                _connection.Insert(entity);
            }
        }

        public void Delete(string SubscriberId, int Id)
        {
            SubscriptionDto Dto = Find(SubscriberId, Id);
            SubscriptionEntity entity = _mapper.Map<SubscriptionEntity>(Dto);
            _connection.Delete(entity);
        }

        public SubscriptionDto Find(string SubscriberId, int Id)
        {
            string commandText = $"SELECT * FROM public.\"{SubscriptionsTableName}\" " +
                   $"WHERE {nameof(SubscriptionEntity.id)}  = @Id";

            var param = new {
                Id
            };

            SubscriptionEntity subscription = _connection.QueryFirst<SubscriptionEntity>(commandText, param);

            SubscriptionDto Dto = _mapper.Map<SubscriptionDto>(subscription);
            return Dto;
        }

        public IReadOnlyCollection<SubscriptionDto> GetAll(string SubscriberId)
        {
            string commandText = $"SELECT * FROM public.\"{SubscriptionsTableName}\" " +
                   $"WHERE {nameof(SubscriptionEntity.subscriber_id)}  = @SubscriberId";

            var param = new {
                SubscriberId
            };

            IEnumerable<SubscriptionEntity> subscriptions = _connection.Query<SubscriptionEntity>(commandText, param);

            List<SubscriptionDto> dtos = subscriptions.Select(_mapper.Map<SubscriptionDto>).ToList();
            return dtos;
        }

        public IReadOnlyList<SubscriptionDto> GetActiveSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        {
            string commandText = $"SELECT * FROM {SubscriptionsTableName} " +
                $"WHERE {nameof(SubscriptionEntity.event_id)}  = @EventId AND " +
                $"{nameof(SubscriptionEntity.subscriber_id)} = @SubscriberId AND " +
                $"{nameof(SubscriptionEntity.active)} is true";

            var param = new {
                @event.EventId,
                @event.SubscriberId
            };

            IEnumerable<SubscriptionEntity> result = _connection.Query<SubscriptionEntity>(commandText, param);

            return result.Select(_mapper.Map<SubscriptionDto>).ToList();
        }

        public bool IsActive(int id)
        {
            return _connection.Get<SubscriptionEntity>(id).active;
        }
    }
}
