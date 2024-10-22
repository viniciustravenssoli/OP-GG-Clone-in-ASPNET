using Microsoft.AspNetCore.Routing;
using static OPGGBlazorWebAssembly.Models;

namespace LeahueGames.Services;

public interface IRiotService
{
    Task<string> GetPlayerPUUID(string playerName, string tag = "BR1");
    Task<string> GetPlayerID(string playerName, string region = "BR1");
    Task<string> GetPlayerNameById(string id);
    Task<List<Root2>> GetPlayerRankedData(string playerId);
    Task<List<string>> GetGameIDs(string playerName, string tag = "BR1");
    Task<List<Root>> GetMatchDataArray(string playerName, int numGames = 5, string tag = "BR1");
    Task<Rank> GetPlayersAsync();
    Task SavePlayerInfo(Root2 rankedInfo);
}
