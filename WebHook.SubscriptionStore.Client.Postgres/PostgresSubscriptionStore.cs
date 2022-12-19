using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using WebHook.Contracts.Events;
using WebHook.SubscriptionStore.Client.Postgres.Database;
using WebHook.SubscriptionStore.Client.Models;
using Microsoft.Extensions.Caching.Memory;

namespace WebHook.SubscriptionStore.Client.Postgres
{
    public class PostgresSubscriptionStore : ISubscriptionStore
    {
        private readonly WebhookContext webhookContext;

        public PostgresSubscriptionStore(WebhookContext webhookContext)
        {
            this.webhookContext = webhookContext;
            
        }
        

        //TODO we need CRUD operations


        //TODO offload to redis or something so its shared across system
        //right now cache is super hacky and assumes subscriptsions never change
        Dictionary<string, IReadOnlyList<Subscription>> cache = new();
        public IReadOnlyList<Subscription> GetSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        {
            string key = $"{@event.EventID}_{@event.TenantID}";
            if (cache.ContainsKey(key) is false)
            {
                cache.Add(key, webhookContext.Subscriptions.Where(s =>
                s.EventId == @event.EventID &&
                s.TenantId == @event.TenantID &&
                s.Active).ToList());
            }
            return cache[key];
        }
        public bool IsActive(int subscriptionId)
        {
            return webhookContext.Subscriptions.Find(subscriptionId)?.Active ?? false;
        }
    }
}