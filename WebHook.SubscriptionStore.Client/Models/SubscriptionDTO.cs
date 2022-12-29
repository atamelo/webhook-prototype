namespace WebHook.SubscriptionSotre.Client.Models;

public record SubscriptionDto(int Id, string Url, bool Active, bool Paused, bool Disabled, string EventId, string SubscriberId);
