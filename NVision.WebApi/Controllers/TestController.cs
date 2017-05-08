using System.Collections.Generic;
using System.Web.Http;

namespace NVision.WebApi.Controllers
{
    public class TestController : ApiController
    {
        [HttpGet]
        [Route("health")]
        public IHttpActionResult Health()
        {

            return Ok("I'm alive !");
        }
    }
}
