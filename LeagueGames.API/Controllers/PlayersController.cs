using LeagueGames.API.DTOs;
using LeagueGames.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeagueGames.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class PlayersController : ControllerBase
{
    private readonly PlayerRankService _playerRankService;

    public PlayersController(PlayerRankService playerRankService)
    {
        _playerRankService = playerRankService;
    }

    [HttpPost("save-highest-rank")]
    public async Task<IActionResult> SaveHighestRank([FromBody] LeagueEntryDto request)
    {
        // Verifica se a request possui erros de validação
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new { Errors = errors });
        }

        try
        {
            await _playerRankService.SaveHighestRank(request);
            return Ok("Elo salvo com sucesso para a temporada atual.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
