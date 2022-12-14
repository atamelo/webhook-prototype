using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using WebHook.Contracts.Events;
using WebHook.SubscriptionStore.Client.Postgres.Database;
using WebHook.SubscriptionStore.Client.Models;

namespace WebHook.SubscriptionStore.Client.Postgres
{
    public class PostgresSubscriptionStore : ISubscriptionStore
    {
        private readonly WebhookContext webhookContext;

        public PostgresSubscriptionStore(WebhookContext webhookContext)
        {
            this.webhookContext = webhookContext;
        }
        public IReadOnlyList<Subscription> GetSubscriptionsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        {
            //TODO active filters etc
            return webhookContext.Subscriptions.Where(s=>
                s.EventId == @event.EventID && 
                s.TenantId == @event.TenantID &&
                s.Active).ToList();
        }
        public bool IsActive(int subscriptionId)
        {
            return webhookContext.Subscriptions.Find(subscriptionId)?.Active ?? false;
        }
    }

}