using Microsoft.AspNetCore.Mvc;

namespace MockWebHookEndpoint.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private ILogger<WebhookController> _logger;

        public WebhookController(ILogger<WebhookController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
            Random random = new();
            int delay = random.Next(500, 7000);
            await Task.Delay(delay);

            
            var errorChance = random.Next(100);

            //2% failure raite
            if (errorChance <= 2)
            {
                //TODO add more codes to deal with
                return new StatusCodeResult(500);
            }

            return new OkResult();
        }
    }
}