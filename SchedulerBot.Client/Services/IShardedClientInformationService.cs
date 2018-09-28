using System.Threading.Tasks;

namespace SchedulerBot.Client.Services
{
    public interface IShardedClientInformationService
    {
        int GetTotalGuildCount();
        int GetTotalUserCount();
    }
}
