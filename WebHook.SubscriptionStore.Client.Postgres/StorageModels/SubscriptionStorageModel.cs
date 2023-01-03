using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebHook.SubscriptionSotre.Client.Models;

namespace WebHook.SubscriptionStore.Client.Postgres.StorageModels;

[Table("subscriptions")]
internal class SubscriptionStorageModel
{
    public static SubscriptionStorageModel FromDto(SubscriptionDto subscriptionDto)
    {
        SubscriptionStorageModel entity = new SubscriptionStorageModel() {
            status = subscriptionDto.Status,
            event_id = subscriptionDto.EventId,
            id = subscriptionDto.Id,
            subscriber_id = subscriptionDto.SubscriberId,
            url = subscriptionDto.Url
        };
        return entity;
    }

    public SubscriptionDto ToDto()
    {
        return new SubscriptionDto(id, url, status, event_id, subscriber_id);
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; init; }

    public string event_id { get; init; }
    public string subscriber_id { get; init; }
    public string url { get; init; }
    public SubscriptionStatus status { get; init; }
}
