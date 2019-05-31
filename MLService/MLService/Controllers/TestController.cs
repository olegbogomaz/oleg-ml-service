using Microsoft.AspNetCore.Mvc;

namespace MLService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "We are up!";
        }
    }
}
