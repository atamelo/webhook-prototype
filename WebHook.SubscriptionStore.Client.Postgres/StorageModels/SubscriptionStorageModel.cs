using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebHook.SubscriptionSotre.Client.Models;

namespace WebHook.SubscriptionStore.Client.Postgres.StorageModels;

[Table("subscriptions")]
internal class SubscriptionStorageModel
{
    public static SubscriptionStorageModel FromDto(SubscriptionDto subscriptionDto)
    {
        SubscriptionStatus status = SubscriptionStatus.Active;
        if (subscriptionDto.Disabled)
            status = SubscriptionStatus.Disabled;
        if (subscriptionDto.Paused)
            status = SubscriptionStatus.Paused;

        SubscriptionStorageModel entity = new SubscriptionStorageModel() {
            active = status,
            event_id = subscriptionDto.EventId,
            id = subscriptionDto.Id,
            subscriber_id = subscriptionDto.SubscriberId,
            url = subscriptionDto.Url
        };
        return entity;
    }

    public SubscriptionDto ToDto()
    {
        bool _active = active.Equals(SubscriptionStatus.Active);
        bool _disabled = active.Equals(SubscriptionStatus.Disabled);
        bool _paused = active.Equals(SubscriptionStatus.Paused);

        return new SubscriptionDto(id, url, _active, _paused, _disabled, event_id, subscriber_id);
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; init; }

    public string event_id { get; init; }
    public string subscriber_id { get; init; }
    public string url { get; init; }
    public SubscriptionStatus active { get; init; }
}

internal enum SubscriptionStatus
{
    Active,
    Paused,
    Disabled
}
