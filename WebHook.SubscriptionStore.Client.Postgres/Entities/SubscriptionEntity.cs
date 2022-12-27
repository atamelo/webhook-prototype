using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebHook.SubscriptionStore.Client.Postgres.Entities;

[Table("subscriptions")]
public class SubscriptionEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    public string event_id { get; set; }
    public string subscriber_id { get; set; }
    public string url { get; set; }
    public bool active { get; set; }
}
