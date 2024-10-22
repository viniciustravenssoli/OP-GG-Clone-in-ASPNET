using LeagueGames.API.Data;
using LeagueGames.API.DTOs;
using LeagueGames.API.Models;

namespace LeagueGames.API.Services;

public class PlayerRankService
{
    private readonly ApplicationDbContext _dbContext;

    public PlayerRankService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveHighestRank(LeagueEntryDto request)
    {
        // // Buscar Summoner ID
        // var summoner = await _riotApiService.GetSummonerByNameAndTagline(nickname, tagline);
        // if (summoner == null)
        //     throw new Exception("Summoner not found");

        // // Buscar Informações Ranqueadas
        // var rankedInfo = await _riotApiService.GetRankedInfoBySummonerId(summoner.Id);
        // if (rankedInfo == null || !rankedInfo.Any())
        //     throw new Exception("No ranked information found");

        int currentSeason = GetCurrentSeason();


        // Verificar se já existe um registro
        var playerRank = _dbContext.PlayerRanks.FirstOrDefault(p =>
        p.summonerId == request.summonerId && p.Season == currentSeason);

        if (playerRank == null)
        {
            // Se não existir, adicione um novo
            playerRank = new PlayerRank
            {
                summonerId = request.summonerId,
                HighestTier = request.tier,
                HighestRank = request.rank,
                LeaguePoints = request.leaguePoints,
                Season = currentSeason,
                LastUpdated = DateTime.Now
            };
            _dbContext.PlayerRanks.Add(playerRank);
        }
        else
        {
            // Verifique se o novo rank é maior que o salvo
            if (IsNewRankHigher(request, playerRank))
            {
                playerRank.HighestTier = request.tier;
                playerRank.HighestRank = request.rank;
                playerRank.LeaguePoints = request.leaguePoints;
                playerRank.LastUpdated = DateTime.Now;
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private int GetCurrentSeason()
    {
        // Lógica de exemplo para determinar a temporada atual
        var now = DateTime.UtcNow;
        if (now.Month >= 1 && now.Month <= 4)
            return 12; // Temporada 12, fase inicial
        if (now.Month >= 5 && now.Month <= 8)
            return 12; // Temporada 12, fase intermediária
        if (now.Month >= 9 && now.Month <= 12)
            return 12; // Temporada 12, fase final

        return 11; // Por padrão, Temporada 11
    }

    private bool IsNewRankHigher(LeagueEntryDto newRank, PlayerRank existingRank)
    {
        int newRankValue = GetRankValue(newRank.tier, newRank.rank);
        int existingRankValue = GetRankValue(existingRank.HighestTier, existingRank.HighestRank);

        if (newRankValue > existingRankValue)
        {
            return true;
        }
        else if (newRankValue == existingRankValue)
        {
            // Se Tier e Rank são iguais, comparar pelos LeaguePoints
            return newRank.leaguePoints > existingRank.LeaguePoints;
        }

        return false;
    }

    private int GetRankValue(string tier, string rank)
    {
        // Define uma ordem para os tiers e ranks
        var tierValues = new Dictionary<string, int>
        {
            { "Iron", 1 }, { "Bronze", 2 }, { "Silver", 3 }, { "Gold", 4 },
            { "Platinum", 5 }, { "Diamond", 6 }, { "Master", 7 },
            { "Grandmaster", 8 }, { "Challenger", 9 }
        };

        var rankValues = new Dictionary<string, int>
        {
            { "IV", 1 }, { "III", 2 }, { "II", 3 }, { "I", 4 }
        };

        int tierValue = tierValues.TryGetValue(tier, out var tValue) ? tValue : 0;
        int rankValue = rankValues.TryGetValue(rank, out var rValue) ? rValue : 0;

        return (tierValue * 10) + rankValue; // Multiplicando por 10 para dar peso maior ao tier
    }
}
