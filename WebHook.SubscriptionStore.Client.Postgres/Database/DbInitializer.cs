using WebHook.SubscriptionStore.Client.Models;

namespace WebHook.SubscriptionStore.Client.Postgres.Database
{
    public static class DbInitializer
    {

        public static void InitializeMockData(WebhookContext context)
        {
            context.Database.EnsureCreated();
            if (context.Subscriptions.Any()) return;

            Random random = new();
            for (int i = 1; i < 1000; i++)
            {
                Subscription subscription = new();
                subscription.Id = i;
                subscription.EventId = random.Next(1, 10).ToString();
                subscription.TenantId = random.Next(1, 100).ToString();
                subscription.Active = true;
                subscription.Url = "http://localhost:5678/webhook";

                context.Subscriptions.Add(subscription);
            }
            context.SaveChanges();
        }
    }
}