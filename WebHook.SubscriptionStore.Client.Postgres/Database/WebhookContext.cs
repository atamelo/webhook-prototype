using Microsoft.EntityFrameworkCore;
using WebHook.SubscriptionStore.Client.Postgres.Entities;

namespace WebHook.SubscriptionStore.Client.Postgres.Database
{
    internal class WebhookContext : DbContext
    {
        public WebhookContext(DbContextOptions<WebhookContext> options) : base(options)
        {
        }

        public DbSet<SubscriptionEntity> Subscriptions { get; set; }
    }
}
