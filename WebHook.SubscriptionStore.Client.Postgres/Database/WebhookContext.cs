using Microsoft.EntityFrameworkCore;
using WebHook.SubscriptionStore.Client.Postgres.Entities;

namespace WebHook.SubscriptionStore.Client.Postgres.Database
{
    //TODO
    //Currently only used for db creation, maybe we want to offload this onto a
    //a lighter tool like https://github.com/fluentmigrator/fluentmigrator
    internal class WebhookContext : DbContext
    {
        public WebhookContext(DbContextOptions<WebhookContext> options) : base(options)
        {
        }

        public DbSet<SubscriptionEntity> Subscriptions { get; set; }
    }
}
