using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using NodaTime;

namespace SchedulerBot.Client.Commands
{
    public class InitializerCommands : BaseCommandModule
    {
        [Command("init"), Description("Initialize the bot with a timezone and a default channel.")]
        public async Task Initialize(CommandContext ctx, string timezone)
        {
            var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timezone);
            if (tz == null)
            {
                await ctx.RespondAsync($"Timezone {timezone} does not exist.");
            }
            else
            {
                await ctx.RespondAsync($"Initializing to timezone {tz.ToString()}");
            }
        }
    }
}
