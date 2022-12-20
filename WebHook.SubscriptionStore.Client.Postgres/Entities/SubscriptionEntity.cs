using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebHook.SubscriptionStore.Client.Postgres.Entities;

public class SubscriptionEntity
{
    [Key]
    public int Id { get; set; }

    public string EventId { get; set; }
    public string TenantId { get; set; }
    public string Url { get; set; }
    public bool Active { get; set; }
}