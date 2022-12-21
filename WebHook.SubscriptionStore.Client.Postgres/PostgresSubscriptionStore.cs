using WebHook.Core.Events;
using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client.Postgres.Database;
using WebHook.SubscriptionStore.Client.Postgres.Entities;

namespace WebHook.SubscriptionStore.Client.Postgres
{
    public class PostgresSubscriptionStore : ISubscriptionStore
    {
        private readonly WebhookContext _webhookContext;

        //TODO offload to redis or something so its shared across system
        //right now cache is super hacky and assumes subscriptsions never change
        private readonly Dictionary<string, IReadOnlyList<SubscriptionDTO>> _cache;

        public PostgresSubscriptionStore(WebhookContext webhookContext)
        {
            _cache = new();
            _webhookContext = webhookContext;
        }

        //TODO we need CRUD operations

        public IReadOnlyList<SubscriptionDTO> GetSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        {
            string key = $"{@event.EventId}_{@event.SubscriberId}";
            if (_cache.ContainsKey(key) is false) {
                List<SubscriptionEntity> subscriptions = _webhookContext.Subscriptions.Where(s =>
                s.EventId == @event.EventId &&
                s.TenantId == @event.SubscriberId &&
                s.Active).ToList();

                List<SubscriptionDTO> subscriptionDTOs = subscriptions.Select(Map).ToList();
                _cache.Add(key, subscriptionDTOs);
            }
            return _cache[key];
        }

        //TODO use auto mapper or some other library for this.
        private static SubscriptionDTO Map(SubscriptionEntity s)
        {
            return new SubscriptionDTO { Url = s.Url };
        }

        public bool IsActive(int subscriptionId)
        {
            return _webhookContext.Subscriptions.Find(subscriptionId)?.Active ?? false;
        }
    }
}
