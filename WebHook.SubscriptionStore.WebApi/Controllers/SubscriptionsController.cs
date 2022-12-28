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

        // GET api/<SubscriptionsController>/5/6
        [HttpGet("{subscriberId}/{id}")]
        public IActionResult Get(string subscriberId, int id)
        {
            try {
                SubscriptionDto result = _subscriptionStore.Find(subscriberId, id);
                return new OkObjectResult(result);
            }
            catch (InvalidOperationException e) {
                if (e.Message.Equals("Sequence contains no elements")) {
                    return new NotFoundResult();
                }
                else {
                    throw;
                }
            }
        }

        // GET api/<SubscriptionsController>/5
        [HttpGet("{subscriberId}")]
        public IActionResult GetAll(string subscriberId)
        {
            IReadOnlyCollection<SubscriptionDto> result = _subscriptionStore.GetAll(subscriberId);
            return new OkObjectResult(result);
        }

        // POST api/<SubscriptionsController>/5
        [HttpPost("{subscriberId}")]
        public void Post([FromBody] SubscriptionDto value)
        {
            _subscriptionStore.Save(value);
        }

        // PUT api/<SubscriptionsController>/5
        [HttpPut("{subscriberId}")]
        public void Put([FromBody] SubscriptionDto value)
        {
            _subscriptionStore.Save(value);
        }

        // DELETE api/<SubscriptionsController>/5/6
        [HttpDelete("{subscriberId}/{id}")]
        public IActionResult Delete(string subscriberId, int id)
        {
            try {
                _subscriptionStore.Delete(subscriberId, id);
                return new NoContentResult();
            }
            catch (InvalidOperationException e) {
                if (e.Message.Equals("Sequence contains no elements")) {
                    return new NotFoundResult();
                }
                else {
                    throw;
                }
            }
        }
    }
}
