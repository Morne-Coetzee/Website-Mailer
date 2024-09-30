using Microsoft.AspNetCore.Mvc;
using Website.Mailer.Api.Models;
using Website.Mailer.Api.Services;

namespace Website.Mailer.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [ApiKeyAuthentication]
    public class EmailController(EmailService emailService, ILogger<EmailController> logger) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromForm] EmailRequestModel request)
        {
            try
            {
                logger.LogInformation("Start {Controller}.{Method}", nameof(EmailController), nameof(Post));
                await emailService.SendAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ExceptionMessage}", ex.Message);
                throw;
            }
        }
    }
}
