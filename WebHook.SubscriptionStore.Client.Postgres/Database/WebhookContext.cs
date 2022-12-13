using Microsoft.EntityFrameworkCore;
using WebHook.SubscriptionStore.Client.Postgres.Models;

namespace WebHook.SubscriptionStore.Client.Postgres.Database
{
    public class WebhookContext : DbContext
    {
        public WebhookContext(DbContextOptions<WebhookContext> options) : base(options)
        {
        }
        public DbSet<Subscription> Subscriptions { get; set; }

    }
}