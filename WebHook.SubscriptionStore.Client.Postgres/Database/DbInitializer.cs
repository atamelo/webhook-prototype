using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client.Postgres.StorageModels;

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
                SubscriptionStorageModel subscription = new() {
                    event_id = random.Next(1, 10).ToString(),
                    subscriber_id = random.Next(1, 100).ToString(),
                    status = SubscriptionStatus.Active,
                    url = "http://localhost:51084/Webhook",
                    //TODO config this as well
                };

                context.Subscriptions.Add(subscription);
            }
            context.SaveChanges();
        }
    }
}
