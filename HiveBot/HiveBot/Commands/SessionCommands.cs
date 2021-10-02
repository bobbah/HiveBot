using System;
using System.ComponentModel;
using System.Threading.Tasks;
using HiveBot.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace HiveBot.Commands
{
    [Group("session")]
    public class SessionCommands : CommandGroup
    {
        private readonly IDiscordRestChannelAPI _channel;
        private readonly ICommandContext _context;
        private readonly FeedbackService _feedback;
        private readonly UserChannelResolverService _channelResolver;

        public SessionCommands(IDiscordRestChannelAPI channel, ICommandContext context, FeedbackService feedback,
            UserChannelResolverService channelResolver)
        {
            _channel = channel;
            _context = context;
            _feedback = feedback;
            _channelResolver = channelResolver;
        }

        [Command("create")]
        [Description("Creates a session for the specified application, and sends an invite")]
        public async Task<IResult> CreateApplicationSession(string application)
        {
            if (!Snowflake.TryParse(application, out var appSnowflake))
            {
                return await _feedback.SendContextualErrorAsync($"Invalid snowflake. ``{appSnowflake}``",
                    _context.User.ID);
            }

            return await CreateSession(appSnowflake.Value);
        }

        [Command("youtube")]
        [Description("Starts a session of YouTube Together")]
        public async Task<IResult> CreateYoutubeSession() => await CreateSession(new Snowflake(880218394199220334));

        [Command("poker")]
        [Description("Starts a session of poker")]
        public async Task<IResult> CreatePokerSession() => await CreateSession(new Snowflake(755827207812677713));

        [Command("betrayal")]
        [Description("Starts a session of betrayal")]
        public async Task<IResult> CreateBetrayalSession() => await CreateSession(new Snowflake(773336526917861400));

        [Command("fishing")]
        [Description("Starts a session of fishing")]
        public async Task<IResult> CreateFishingSession() => await CreateSession(new Snowflake(814288819477020702));

        [Command("chess")]
        [Description("Starts a session of chess")]
        public async Task<IResult> CreateChessSession() => await CreateSession(new Snowflake(832012774040141894));

        [Command("lettertile")]
        [Description("Starts a session of lettertile")]
        public async Task<IResult> CreateLetterSession() => await CreateSession(new Snowflake(879863686565621790));

        [Command("wordsnack")]
        [Description("Starts a session of wordsnack")]
        public async Task<IResult> CreateWordSnackSession() => await CreateSession(new Snowflake(879863976006127627));

        [Command("doodlecrew")]
        [Description("Starts a session of doodlecrew")]
        public async Task<IResult> CreateDoodleCrewSession() => await CreateSession(new Snowflake(878067389634314250));

        private async Task<IResult> CreateSession(Snowflake application)
        {
            // Check if the user is currently in a voice channel
            var voiceChannel = _channelResolver.GetChannel(_context.User.ID, _context.GuildID.Value);
            if (!voiceChannel.HasValue)
            {
                return await _feedback.SendContextualErrorAsync("You are not in a voice channel. Try rejoining it.",
                    _context.User.ID);
            }

            // Attempt to create the session
            var createdInvite = await _channel.CreateChannelInviteAsync(
                voiceChannel.Value,
                targetType: InviteTarget.EmbeddedApplication,
                targetApplicationID: application,
                isTemporary: true,
                maxUses: 1,
                maxAge: TimeSpan.FromMinutes(1),
                isUnique: true,
                reason: "Generated invite for embedded application");

            if (!createdInvite.IsSuccess)
            {
                return await _feedback.SendContextualErrorAsync(
                    $"Could not create a session for this application. ``{createdInvite.Error?.Message}``",
                    _context.User.ID);
            }

            return await _feedback.SendContextualSuccessAsync(
                $"Use this invite to start the application, note this invite expires in one minute: https://discord.gg/{createdInvite.Entity.Code}");
        }
    }
}