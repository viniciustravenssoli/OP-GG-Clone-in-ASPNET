namespace LeagueGames.API.Models;

public class PlayerRank
{
    public int Id { get; set; }
    public string summonerId { get; set; }
    public string HighestTier { get; set; }
    public string HighestRank { get; set; }
    public int LeaguePoints { get; set; }
    public int Season { get; set; } // Adiciona o número da temporada
    public DateTime LastUpdated { get; set; }
}
