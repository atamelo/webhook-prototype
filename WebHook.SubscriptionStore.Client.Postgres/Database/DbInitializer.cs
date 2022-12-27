using WebHook.SubscriptionStore.Client.Postgres.Entities;

namespace WebHook.SubscriptionStore.Client.Postgres.Database
{
    internal static class DbInitializer
    {
        internal static void InitializeMockData(WebhookContext context)
        {
            //TODO migrations

            //TODO config all this out
            context.Database.EnsureCreated();
            if (context.Subscriptions.Any()) {
                return;
            }

            Random random = new();
            for (int i = 1; i < 1000; i++) {
                SubscriptionEntity subscription = new();
                subscription.event_id = random.Next(1, 10).ToString();
                subscription.subscriber_id = random.Next(1, 100).ToString();
                subscription.active = true;
                subscription.url = "http://localhost:5678/webhook";//TODO config this as well

                context.Subscriptions.Add(subscription);
            }
            context.SaveChanges();
        }
    }
}
