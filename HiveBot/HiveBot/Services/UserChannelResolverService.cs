using System.Collections.Concurrent;
using System.Threading.Tasks;
using Remora.Discord.Core;

namespace HiveBot.Services
{
    public class UserChannelResolverService
    {
        private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<Snowflake, Snowflake>> _userMap;

        public UserChannelResolverService()
        {
            _userMap = new ConcurrentDictionary<Snowflake, ConcurrentDictionary<Snowflake, Snowflake>>();
        }

        public void UserJoined(Snowflake server, Snowflake user, Snowflake channel)
        {
            _userMap
                .GetOrAdd(user, _ => new ConcurrentDictionary<Snowflake, Snowflake>())
                .AddOrUpdate(server, _ => channel, (_, _) => channel);
        }

        public void UserLeft(Snowflake server, Snowflake user)
        {
            if (_userMap.TryGetValue(user, out var servers))
            {
                _userMap.TryRemove(server, out _);
            }
        }

        public Snowflake? GetChannel(Snowflake user, Snowflake server)
        {
            if (_userMap.TryGetValue(user, out var servers))
            {
                return servers.TryGetValue(server, out var channel) ? channel : null;
            }

            return null;
        }
    }
}