using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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

        [HttpGet("{subscriberId}/{id}")]
        public IActionResult GetSubscriptionFor(string subscriberId, int id)
        {
            SubscriptionDto? result = _subscriptionStore.GetSubscriptionFor(id);
            if (result is null) {
                return new NotFoundResult();
            }
            if (result.SubscriberId.Equals(subscriberId, StringComparison.OrdinalIgnoreCase) is false) {
                return new ObjectResult("SubscriberId mismatch") { StatusCode = 401 };
            }
            return new OkObjectResult(result);
        }

        // GET api/<SubscriptionsController>/5
        [HttpGet("{subscriberId}")]
        public IActionResult GetSubscriptionsFor(string subscriberId)
        {
            IReadOnlyCollection<SubscriptionDto> result = _subscriptionStore.GetSubscriptionsFor(subscriberId);
            return new OkObjectResult(result);
        }

        // POST api/<SubscriptionsController>/5
        [HttpPost("{subscriberId}")]
        public IActionResult CreateSubscription(string subscriberId, [FromBody] SubscriptionDto value)
        {
            if (value.SubscriberId.Equals(subscriberId, StringComparison.OrdinalIgnoreCase) is false) {
                return new ObjectResult("SubscriberId mismatch") { StatusCode = 401 };
            }
            int id = _subscriptionStore.CreateSubscription(value);
            return new OkObjectResult(id);
        }

        // PUT api/<SubscriptionsController>/5
        [HttpPut("{subscriberId}")]
        public IActionResult UpdateSubscription(string subscriberId, [FromBody] SubscriptionDto value)
        {
            if (value.SubscriberId.Equals(subscriberId, StringComparison.OrdinalIgnoreCase) is false) {
                return new ObjectResult("SubscriberId mismatch") { StatusCode = 401 };
            }
            _subscriptionStore.UpdateSubscription(value);
            return new NoContentResult();
        }

        // DELETE api/<SubscriptionsController>/5/6
        [HttpDelete("{subscriberId}/{id}")]
        public IActionResult DeleteSubscription(string subscriberId, int id)
        {
            _subscriptionStore.DeleteSubscription(id);
            return new NoContentResult();
        }
    }
}
