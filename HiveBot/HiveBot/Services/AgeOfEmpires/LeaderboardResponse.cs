using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HiveBot.Services.AgeOfEmpires
{
    public record LeaderboardResponse
    {
        public int Count { get; init; }
        
        public IEnumerable<LeaderboardRecord> Items { get; init; }
    }

    public record LeaderboardRecord
    {
        public string GameId { get; init; }

        public string UserId { get; init; }

        public int RlUserId { get; init; }

        public string Username { get; init; }

        public string AvatarUrl { get; init; }

        public int? PlayerNumber { get; init; }

        public int Elo { get; init; }

        public int EloRating { get; init; }

        public int Rank { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameRegion Region { get; init; }

        public int Wins { get; init; }

        public float WinPercent { get; init; }

        public int Losses { get; init; }

        public int WinStreak { get; init; }
    }
}