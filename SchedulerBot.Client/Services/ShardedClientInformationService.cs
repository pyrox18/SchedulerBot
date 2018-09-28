using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;

namespace SchedulerBot.Client.Services
{
    public class ShardedClientInformationService : IShardedClientInformationService
    {
        private readonly DiscordShardedClient _shardedClient;

        public ShardedClientInformationService(DiscordShardedClient shardedClient) => _shardedClient = shardedClient;

        public int GetTotalGuildCount()
        {
            return _shardedClient.ShardClients.Values.Sum(c => c.Guilds.Count);
        }

        public int GetTotalUserCount()
        {
            return _shardedClient.ShardClients.Values.Sum(c => c.Guilds.Values.Sum(g => g.MemberCount));
        }
    }
}
