using ErrorHandlingMiddlewareExample.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ErrorHandlingMiddlewareExample.Controllers
{
    [Route("api/examples")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    public class ExamplesController : ControllerBase
    {
        [HttpGet("success")]
        [ProducesResponseType(200)]
        public IActionResult GetSuccess()
        {
            return Ok("success");
        }

        [HttpGet("bad-request")]
        [ProducesResponseType(400)]
        public IActionResult GetBadRequest()
        {
            throw new BadRequestException("Field 1 had an error.");
        }

        [HttpGet("not-found")]
        [ProducesResponseType(404)]
        public IActionResult GetNotFound()
        {
            throw new ResourceNotFoundException("SomeResource", "123");
        }

        [HttpGet("server-error")]
        [ProducesResponseType(500)]
        public IActionResult GetServerError()
        {
            throw new Exception("Something went wrong.");
        }
    }
}