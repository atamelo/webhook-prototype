namespace WebHook.SubscriptionSotre.Client.Models;

public record SubscriptionDto(int Id, string Url, SubscriptionStatus Status, string EventId, string SubscriberId);

public enum SubscriptionStatus
{
    Active,
    Paused,
    Disabled
}
