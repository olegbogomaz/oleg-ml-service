using Microsoft.AspNetCore.Mvc;
using MLService.MLPredictors;
using MLService.Models;

namespace MLService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SDController : ControllerBase
    {
        private ISpamDetectionContextSingleton SpamDetectionContext { get; set; }

        public SDController(ISpamDetectionContextSingleton spamDetectionContext)
        {
            SpamDetectionContext = spamDetectionContext;
        }

        [HttpGet()]
        public ActionResult<string> Get()
        {
            var isSpam = SpamDetectionContext.IsSpam("That's a great idea. It should work.");
            return isSpam ? "Spam" : "Not Spam";
        }

        [HttpPost]
        public ActionResult<string> Post([FromBody] SpamInputParam message)
        {
            var isSpam = SpamDetectionContext.IsSpam(message.SanitizedInput);
            return isSpam ? "Spam" : "Not Spam";
        }
    }
}
