using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HiveBot.Services.AgeOfEmpires
{
    public class AgeOfEmpiresService
    {
        public const string ApiUrl = "https://api.ageofempires.com/api/";
        private readonly HttpClient _client;

        public AgeOfEmpiresService(HttpClient client)
        {
            _client = client;
        }

        public async Task<LeaderboardRecord> GetLeaderboardStats(string name, MatchSize size)
        {
            var payload = new LeaderboardRequest()
            {
                TeamSize = DecodeMatchSize(size),
                SearchPlayer = name
            };
            var response = await _client.PostAsync(ApiUrl + "/ageiv/Leaderboard",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.Default, "application/json"));

            // Case when there is no player found
            if (response.StatusCode == HttpStatusCode.NoContent)
                return null;

            return JsonSerializer.Deserialize<LeaderboardResponse>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                })?.Items.FirstOrDefault();
        }

        private static string DecodeMatchSize(MatchSize size) => size switch
        {
            MatchSize.OneVersusOne => "1v1",
            MatchSize.TwoVersusTwo => "2v2",
            MatchSize.ThreeVersusThree => "3v3",
            MatchSize.FourVersusFour => "4v4",
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };
    }
}