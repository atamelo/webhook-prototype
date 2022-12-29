using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebHook.SubscriptionStore.Client.Postgres.Database;

namespace WebHook.SubscriptionStore.Client.Postgres.Extensions
{
    public static class StartupExtensions
    {
        public static void AddSubscriptionStore(this IServiceCollection services)
        {
            //TODO add configuration
            services.AddDbContext<WebhookContext>(options => {
                options.UseNpgsql("Host=localhost:5432;Database=webhooks;Username=postgres;Password=postgres");
                options.EnableSensitiveDataLogging(false);
            });
            services.AddSingleton<ISubscriptionStore, PostgresSubscriptionStore>();
        }

        public static void CreateDB(this IServiceProvider services)
        {
            using (IServiceScope scope = services.CreateScope()) {
                WebhookContext context =
                    scope.ServiceProvider.GetRequiredService<WebhookContext>();
                DbInitializer.InitializeMockData(context);
            }
        }
    }
}
