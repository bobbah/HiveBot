namespace HiveBot.Services.AgeOfEmpires
{
    public record LeaderboardRequest
    {
        public GameRegion Region { get; init; }
        public string Versus => "players";
        public string MatchType => "unranked";
        public string TeamSize { get; init; }
        public string SearchPlayer { get; init; }
    }
}