using Microsoft.EntityFrameworkCore;
using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client.Postgres.Entities;

namespace WebHook.SubscriptionStore.Client.Postgres.Database
{
    public class WebhookContext : DbContext
    {
        public WebhookContext(DbContextOptions<WebhookContext> options) : base(options)
        {
        }

        public DbSet<SubscriptionEntity> Subscriptions { get; set; }
    }
}