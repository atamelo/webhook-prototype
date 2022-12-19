using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebHook.SubscriptionStore.Client.Postgres.Database;

namespace WebHook.SubscriptionStore.Client.Postgres.Extensions
{
    public static class StartupExtensions
    {
        public static void AddSubscriptionStore(this IServiceCollection services)
        {
            //TODO add configuration
            services.AddDbContext<WebhookContext>(options =>
            {
                options.UseNpgsql("Host=localhost:5432;Database=webhooks;Username=postgres;Password=postgres");
                options.EnableSensitiveDataLogging(false);
            });


        }
        public static void CreateDB(this IHost host)
        {
            using (IServiceScope scope = host.Services.CreateScope())
            {
                WebhookContext context =
                    scope.ServiceProvider.GetRequiredService<WebhookContext>();
                DbInitializer.InitializeMockData(context);

            }
        }
    }

}