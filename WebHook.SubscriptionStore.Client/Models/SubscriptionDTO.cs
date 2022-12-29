namespace WebHook.SubscriptionSotre.Client.Models;

public record SubscriptionDto(int Id, string Url, bool Active, string EventId, string SubscriberId);
