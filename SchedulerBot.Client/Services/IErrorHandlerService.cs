using DSharpPlus.CommandsNext;
using System.Threading.Tasks;

namespace SchedulerBot.Client.Services
{
    public interface IErrorHandlerService
    {
        Task HandleCommandErrorAsync(CommandErrorEventArgs e);
    }
}
