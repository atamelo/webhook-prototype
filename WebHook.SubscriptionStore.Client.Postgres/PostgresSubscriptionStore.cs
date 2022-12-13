using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using WebHook.Contracts.Events;
using WebHook.SubscriptionStore.Client.Postgres.Database;

namespace WebHook.SubscriptionStore.Client.Postgres
{
    public class PostgresSubscriptionStore : ISubscriptionStore
    {
        private readonly WebhookContext webhookContext;

        public PostgresSubscriptionStore(WebhookContext webhookContext)
        {
            this.webhookContext = webhookContext;
        }
        public IReadOnlyList<string> GetEndpointsFor<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
        {
            //TODO active filters etc
            return webhookContext.Subscriptions.Where(s=>
                s.EventId == @event.EventID && 
                s.TenantId == @event.TenantID &&
                s.Active)
                .Select(s=> s.Url).ToList();
        }
        public bool IsActive(int subscriptionId)
        {
            return webhookContext.Subscriptions.Find(subscriptionId)?.Active ?? false;
        }
    }

}