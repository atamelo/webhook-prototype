namespace WebHook.SubscriptionSotre.Client.Models;

public class SubscriptionDto
{
    public int Id { get; set; }
    public string Url { get; set; }
    public bool Active { get; set; }
    public string EventId { get; set; }
    public string SubscriberId { get; set; }
}
