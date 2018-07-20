using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SchedulerBot.Client.Commands
{
    [Group("event")]
    [Description("Commands for managing events.")]
    public class EventCommands : BaseCommandModule
    {
        [GroupCommand, Description("Create an event.")]
        public async Task Create(CommandContext ctx, params string[] args)
        {
            var extraction = ValidateAndExtract(string.Join(' ', args));
            if (extraction.IsValid)
            {
                var result = extraction.Values;
                await ctx.RespondAsync($"Valid: {result.ToString()}");
            }
            else
            {
                await ctx.RespondAsync($"Invalid: {extraction.ErrorMessage}");
            }
        }

        [Command("list"), Description("Lists all events.")]
        public async Task List(CommandContext ctx)
        {
            await ctx.RespondAsync("Event list");
        }

        [Command("update"), Description("Update an event.")]
        public async Task Update(CommandContext ctx, uint index, [RemainingText] string args)
        {
            await ctx.RespondAsync($"Updating event {index} with args {args}");
        }

        [Command("delete"), Description("Delete an event.")]
        public async Task Delete(CommandContext ctx, uint index)
        {
            await ctx.RespondAsync($"Deleting event {index}");
        }

        private Extraction ValidateAndExtract(string input)
        {
            var results = DateTimeRecognizer.RecognizeDateTime(input, Culture.English);

            if (results.Count > 0 && results.First().TypeName.StartsWith("datetimeV2"))
            {
                var first = results.First();
                var resolutionValues = (IList<Dictionary<string, string>>)first.Resolution["values"];

                var subType = first.TypeName.Split('.').Last();
                if (subType.Contains("date") && !subType.Contains("range"))
                {
                    var moment = resolutionValues.Select(v => DateTime.Parse(v["value"])).FirstOrDefault();
                    if (IsFuture(moment))
                    {
                        return new Extraction
                        {
                            IsValid = true,
                            Values = new[] { moment }
                        };
                    }

                    return new Extraction
                    {
                        IsValid = false,
                        Values = new[] { moment },
                        ErrorMessage = "Date/Time values are in the past."
                    };
                }
                else if (subType.Contains("date") && subType.Contains("range"))
                {
                    var from = DateTime.Parse(resolutionValues.First()["start"]);
                    var to = DateTime.Parse(resolutionValues.First()["end"]);
                    if (IsFuture(from) && IsFuture(to))
                    {
                        return new Extraction
                        {
                            IsValid = true,
                            Values = new[] { from, to }
                        };
                    }

                    var values = new[] { from, to };
                    return new Extraction
                    {
                        IsValid = false,
                        Values = values,
                        ErrorMessage = "Date/Time values are in the past."
                    };
                }
            }

            return new Extraction
            {
                IsValid = false,
                Values = Enumerable.Empty<DateTime>(),
                ErrorMessage = "Invalid date and time"
            };
        }

        private bool IsFuture(DateTime date)
        {
            return date > DateTime.Now;
        }
    }
}
