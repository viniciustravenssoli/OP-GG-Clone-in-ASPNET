using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static OPGGBlazorWebAssembly.Models;

namespace LeahueGames.Services;

public class RiotService : IRiotService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly int _maxRetryAttempts;
    private readonly int _baseDelayMilliseconds;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(6);
    private const string RankCacheKey = "RankCache";

    public RiotService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _apiKey = configuration["RiotAPIKey"] ?? throw new ArgumentException("Riot API Key not found in configuration.");

        if (!int.TryParse(configuration["MaxRetryAttempts"], out _maxRetryAttempts))
        {
            throw new ArgumentException("Invalid value for MaxRetryAttempts in configuration.");
        }

        if (!int.TryParse(configuration["BaseDelayMilliseconds"], out _baseDelayMilliseconds))
        {
            throw new ArgumentException("Invalid value for BaseDelayMilliseconds in configuration.");
        }
    }


    public async Task<string> GetPlayerPUUID(string playerName, string region = "BR1")
    {
        if (string.IsNullOrEmpty(playerName))
            throw new ArgumentException("Player name cannot be null or empty.", nameof(playerName));

        try
        {
            string url = $"https://americas.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{playerName}/{region}?api_key={_apiKey}";
            var response = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

            dynamic jsonResponse = JsonConvert.DeserializeObject(response);

            Console.WriteLine(jsonResponse.ToString());

            return jsonResponse.puuid ?? string.Empty;
        }
        catch (HttpRequestException ex)
        {
            // Log exception
            Console.WriteLine($"Error fetching PUUID for player {playerName}: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<string> GetPlayerID(string playerName, string region = "BR1")
    {
        try
        {
            var response = await GetPlayerPUUID(playerName, region);

            string puuidUrl = $"https://br1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/{response}?api_key={_apiKey}";
            var idResponse = await _httpClient.GetStringAsync(puuidUrl).ConfigureAwait(false);
            dynamic idJsonResponse = JsonConvert.DeserializeObject(idResponse);

            return idJsonResponse.id ?? string.Empty;
        }
        catch (HttpRequestException ex)
        {
            // Log exception
            Console.WriteLine($"Error fetching player ID for {playerName}: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<List<Root2>> GetPlayerRankedData(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
            throw new ArgumentException("Player ID cannot be null or empty.", nameof(playerId));

        try
        {
            string url = $"https://br1.api.riotgames.com/lol/league/v4/entries/by-summoner/{playerId}?api_key={_apiKey}";
            var response = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<List<Root2>>(response);
        }
        catch (HttpRequestException ex)
        {
            // Log exception
            Console.WriteLine($"Error fetching ranked data for player ID {playerId}: {ex.Message}");
            return new List<Root2>();
        }
    }

    public async Task<List<string>> GetGameIDs(string playerName, string region = "BR1")
    {
        var puuid = await GetPlayerPUUID(playerName, region).ConfigureAwait(false);

        if (string.IsNullOrEmpty(puuid))
        {
            return new List<string>();
        }

        try
        {
            string url = $"https://americas.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?api_key={_apiKey}";
            var response = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<List<string>>(response);
        }
        catch (HttpRequestException ex)
        {
            // Log exception
            Console.WriteLine($"Error fetching game IDs for player {playerName}: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<List<Root>> GetMatchDataArray(string playerName, int numGames = 5, string region = "BR1")
    {
        var gameIds = await GetGameIDs(playerName, region).ConfigureAwait(false);

        if (gameIds.Count == 0)
        {
            return new List<Root>();
        }

        var tasks = gameIds
            .Take(numGames)
            .Select(matchId => GetMatchData(matchId))
            .ToList();

        try
        {
            var result = await Task.WhenAll(tasks).ConfigureAwait(false);
            return result.ToList();
        }
        catch (Exception ex)
        {
            // Log exception
            Console.WriteLine($"Error fetching match data for {playerName}: {ex.Message}");
            return new List<Root>();
        }
    }

    private async Task<Root> GetMatchData(string matchId)
    {
        try
        {
            string url = $"https://americas.api.riotgames.com/lol/match/v5/matches/{matchId}?api_key={_apiKey}";
            var response = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<Root>(response);
        }
        catch (HttpRequestException ex)
        {
            // Log exception
            Console.WriteLine($"Error fetching match data for match ID {matchId}: {ex.Message}");
            return null;
        }
    }

    public async Task<Rank> GetPlayersAsync()
    {
        // Tente obter os dados do cache
        if (_cache.TryGetValue(RankCacheKey, out Rank cachedRank))
        {
            return cachedRank;
        }

        try
        {
            string url = $"https://br1.api.riotgames.com/lol/league/v4/challengerleagues/by-queue/RANKED_SOLO_5x5/?api_key={_apiKey}";
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var rank = JsonConvert.DeserializeObject<Rank>(jsonResponse);

                // Buscar o nome atualizado de cada invocador usando `summonerId`
                foreach (var player in rank.entries)
                {
                    player.summonerName = await GetPlayerNameById(player.summonerId);
                }

                // Salvar no cache com uma duração de 6 horas
                _cache.Set(RankCacheKey, rank, _cacheDuration);
                Console.WriteLine("Cache salvo com sucesso");
                return rank;
            }
            else
            {
                throw new HttpRequestException($"Failed to fetch data. Status code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            // Log exception
            Console.WriteLine($"Error fetching players data: {ex.Message}");
            throw;
        }
    }


    public async Task<string> GetPlayerNameById(string id)
    {
        string summonerUrl = $"https://br1.api.riotgames.com/lol/summoner/v4/summoners/{id}?api_key={_apiKey}";

        try
        {
            var summonerData = await ExecuteWithRetries<SummonerResponse>(summonerUrl);
            if (summonerData == null)
            {
                return "Erro: Excedeu o limite de requisições. Tente novamente mais tarde.";
            }

            string puuid = summonerData.Puuid;
            string accountUrl = $"https://americas.api.riotgames.com/riot/account/v1/accounts/by-puuid/{puuid}?api_key={_apiKey}";

            var accountData = await ExecuteWithRetries<AccountResponse>(accountUrl);
            if (accountData == null)
            {
                return "Erro: Excedeu o limite de requisições. Tente novamente mais tarde.";
            }

            return accountData.GameName;
        }
        catch (Exception ex)
        {
            // Log the exception (ex) here if needed
            return $"Erro inesperado: {ex.Message}";
        }
    }


    public async Task SavePlayerInfo(Root2 rankedInfo)
    {
        try
        {
            string apiEndpoint = "https://localhost:7210/api/Players/save-highest-rank";
            var jsonContent = JsonConvert.SerializeObject(rankedInfo);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Player info saved successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to save player info. Status code: {response.ToString()}");
            }
        }
        catch (HttpRequestException ex)
        {
            // Log exception
            Console.WriteLine($"Error saving player info: {ex.Message}");
        }
    }

    private async Task<T> ExecuteWithRetries<T>(string url) where T : class
    {
        for (int attempt = 0; attempt < _maxRetryAttempts; attempt++)
        {
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(_baseDelayMilliseconds * (attempt + 1));
                continue;
            }

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }

            // Return null if a non-retriable error occurs
            if (!response.IsSuccessStatusCode && attempt == _baseDelayMilliseconds - 1)
            {
                return null;
            }
        }

        return null;
    }


}
