using Microsoft.AspNetCore.Mvc;
using MovieBot.Service.Services.BotConfigurations.Handlers;
using Telegram.Bot.Types;

namespace MovieBot.Api.Controllers.Bot;

[ApiController]
[Route("api/[controller]/[action]")]
public class BotController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update,
    [FromServices] UpdateHandler updateHandler)
    {
        await updateHandler.HandleUpdateAsync(update);

        return Ok();
    }
}
