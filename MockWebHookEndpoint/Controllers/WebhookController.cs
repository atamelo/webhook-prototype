using Microsoft.AspNetCore.Mvc;
using MockWebHookEndpoint.Logic;

namespace MockWebHookEndpoint.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly MockHelper _helper;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(MockHelper helper, ILogger<WebhookController> logger)
        {
            _helper = helper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
            await _helper.RandomDelayAsync();
            int errorChance = _helper.RandomNext(100);
            if (errorChance <= 2) {
                //TODO add more codes to deal with
                return new StatusCodeResult(500);
            }
            return new OkResult();
        }
    }
}
