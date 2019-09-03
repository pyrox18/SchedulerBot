using DSharpPlus.CommandsNext;
using MediatR;

namespace SchedulerBot.Client.Commands
{
    public abstract class BotCommandModule : BaseCommandModule
    {
        protected readonly IMediator _mediator;

        public BotCommandModule(IMediator mediator)
        {
            _mediator = mediator;
        }
    }
}
