using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace HiveBot.Responders
{
    public class SlashCommandConfigurator : IResponder<IGuildCreate>
    {
        private readonly ILogger _logger;
        private readonly SlashService _slash;
        private readonly bool _supportSlash = true;

        public SlashCommandConfigurator(ILogger<SlashCommandConfigurator> logger, SlashService slash)
        {
            _slash = slash;
            _logger = logger;

            // Check we can actually support slash commands
            var slashSupport = _slash.SupportsSlashCommands();
            if (!slashSupport.IsSuccess)
            {
                _logger.LogWarning("The registered commands of the bot don't support slash commands: {Reason}",
                    slashSupport.Error?.Message);
                _supportSlash = false;
            }
        }

        public async Task<Result> RespondAsync(IGuildCreate gatewayEvent,
            CancellationToken ct = new CancellationToken())
        {
            if (!_supportSlash)
                return Result.FromSuccess();

            var update = await _slash.UpdateSlashCommandsAsync(gatewayEvent.ID, ct);
            if (!update.IsSuccess)
                _logger.LogWarning("Failed to update slash commands: {Reason}", update.Error?.Message);
            return update.IsSuccess ? Result.FromSuccess() : Result.FromError(update.Error);
        }
    }
}