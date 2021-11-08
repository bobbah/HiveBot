using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using HiveBot.Services.AgeOfEmpires;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace HiveBot.Commands
{
    [Group("aoe")]
    public class AgeOfEmpiresCommands : CommandGroup
    {
        private readonly AgeOfEmpiresService _aoe;
        private FeedbackService _feedback;

        public AgeOfEmpiresCommands(AgeOfEmpiresService aoe, FeedbackService feedback)
        {
            _aoe = aoe;
            _feedback = feedback;
        }

        [Command("whois")]
        [Description("Looks up statistics on an Age Of Empires IV player")]
        public async Task<IResult> Whois(string username)
        {
            var responses = new Dictionary<MatchSize, LeaderboardRecord>();
            foreach (var matchSize in Enum.GetValues<MatchSize>())
            {
                var response = await _aoe.GetLeaderboardStats(username, matchSize);
                if (response != null)
                    responses.Add(matchSize, response);
            }

            if (responses.Count == 0)
                return await _feedback.SendContextualErrorAsync(
                    "Username not found on rankings API, user must have at least 10 matches on record in one of " +
                    "the game sizes (1v1, 2v2, etc). This make take over an hour to update after the match is complete.");


            var fields = new List<IEmbedField>();
            foreach (var response in responses)
            {
                var acronym = GetMatchSizeAcroynm(response.Key);
                fields.Add(new EmbedField($"{acronym} ELO", $"{response.Value.Elo}", true));
                fields.Add(new EmbedField($"{acronym} Ranking", $"{response.Value.Rank}", true));
                fields.Add(new EmbedField($"{acronym} Win/Loss",
                    $"{response.Value.Wins}W/{response.Value.Losses}L ({response.Value.WinPercent}%)",
                    true));
            }

            var embed = new Embed(
                Title: $"Stats for {username}",
                Colour: Color.Green,
                Footer: new EmbedFooter("HiveBot Databanks | AoEIV API"),
                Timestamp: DateTimeOffset.UtcNow,
                Fields: fields,
                Description: "Rankings are only shown for categories in which the player has at least 10 matches on" +
                             " record. Games can take up to an hour to effect stats. Username is case sensitive."
            );

            return await _feedback.SendContextualEmbedAsync(embed);
        }

        private static string GetMatchSizeAcroynm(MatchSize size) => size switch
        {
            MatchSize.OneVersusOne => "1v1",
            MatchSize.TwoVersusTwo => "2v2",
            MatchSize.ThreeVersusThree => "3v3",
            MatchSize.FourVersusFour => "4v4",
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };
    }
}