using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace HiveBot.Commands
{
    public class MetaCommands : CommandGroup
    {
        private readonly FeedbackService _feedback;
        private readonly IDiscordRestUserAPI _user;
        private readonly ICommandContext _context;

        public MetaCommands(FeedbackService feedback, IDiscordRestUserAPI user, ICommandContext context)
        {
            _feedback = feedback;
            _user = user;
            _context = context;
        }

        [Command("invite")]
        [Description("Generate an invite for your server")]
        public async Task<IResult> CreateInvite()
        {
            var currentUser = await _user.GetCurrentUserAsync();
            if (!currentUser.IsSuccess)
            {
                var errorResponse =
                    await _feedback.SendContextualErrorAsync($"Failed to get current user. ``{currentUser.Error?.Message}``",
                        _context.User.ID);
                return errorResponse.IsSuccess ? Result.FromSuccess() : Result.FromError(errorResponse);
            }
            
            var inviteLink =
                $"https://discord.com/api/oauth2/authorize?client_id={currentUser.Entity.ID}&permissions=2147485697&scope=bot%20applications.commands";
            return await _feedback.SendContextualEmbedAsync(new Embed(
                Description: $"[Click here]({inviteLink}) to invite me to your server!",
                Colour: _feedback.Theme.Primary,
                Timestamp: DateTimeOffset.UtcNow,
                Footer: new EmbedFooter(VersionUtility.Version)));
        }
    }
}