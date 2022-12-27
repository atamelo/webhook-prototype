using Microsoft.AspNetCore.Mvc;
using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client;

namespace WebHook.SubscriptionStore.WebApi.Controllers
{
    //TODO security

    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionStore _subscriptionStore;
        private readonly ILogger<SubscriptionsController> _logger;

        public SubscriptionsController(ISubscriptionStore subscriptionStore, ILogger<SubscriptionsController> logger)
        {
            _subscriptionStore = subscriptionStore;
            _logger = logger;
        }

        // GET api/<SubscriptionsController>/5
        [HttpGet("{subscriberId}/{id}")]
        public SubscriptionDTO Get(string subscriberId, int id)
        {
            return _subscriptionStore.Get(subscriberId, id);
        }

        // POST api/<SubscriptionsController>
        [HttpPost("{subscriberId}")]
        public void Post([FromBody] SubscriptionDTO value)
        {
            _subscriptionStore.Save(value);
        }

        // PUT api/<SubscriptionsController>/5
        [HttpPut("{subscriberId}")]
        public void Put([FromBody] SubscriptionDTO value)
        {
            _subscriptionStore.Save(value);
        }

        // DELETE api/<SubscriptionsController>/5
        [HttpDelete("{subscriberId}/{id}")]
        public void Delete(string subscriberId, int id)
        {
            _subscriptionStore.Delete(subscriberId, id);
        }
    }
}
