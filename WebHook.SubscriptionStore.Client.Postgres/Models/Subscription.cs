using System.ComponentModel.DataAnnotations;

namespace WebHook.SubscriptionStore.Client.Postgres.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }
        public string EventId { get; set; }
        public string TenantId { get; set; }
        public string Url { get; set; }
        public bool Active { get; set; }
    }
}