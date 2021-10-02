using System.Threading;
using System.Threading.Tasks;
using HiveBot.Services;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace HiveBot.Responders
{
    public class ChannelPresenceTracker : IResponder<IVoiceStateUpdate>
    {
        private readonly UserChannelResolverService _channelResolver;

        public ChannelPresenceTracker(UserChannelResolverService channelResolver)
        {
            _channelResolver = channelResolver;
        }
        
        public async Task<Result> RespondAsync(IVoiceStateUpdate gatewayEvent,
            CancellationToken ct = new CancellationToken())
        {
            if (gatewayEvent.ChannelID.HasValue)
                _channelResolver.UserJoined(gatewayEvent.GuildID.Value, gatewayEvent.UserID,
                    gatewayEvent.ChannelID.Value);
            else
                _channelResolver.UserLeft(gatewayEvent.GuildID.Value, gatewayEvent.UserID);
            
            return Result.FromSuccess();
        }
    }
}