using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using SchedulerBot.Client.Extensions;
using SharpRaven;
using SharpRaven.Data;
using System.Text;
using System.Threading.Tasks;

namespace SchedulerBot.Client.Services
{
    public class SentryErrorHandlerService : ErrorHandlerService
    {
        protected readonly IRavenClient _ravenClient;

        protected const string _sentryEventIdKey = "sentryEventId";

        public SentryErrorHandlerService(ILogger<IErrorHandlerService> logger, IRavenClient ravenClient) :
            base(logger)
        {
            _ravenClient = ravenClient;
        }

        protected override async Task RecordError(CommandErrorEventArgs e)
        {
            await base.RecordError(e);

            e.Exception.Data.Add("ErrorEventId", _errorRecordIds[_errorIdKey]);
            e.Exception.Data.Add("Message", e.Context.Message);
            e.Exception.Data.Add("Command", e.Command.QualifiedName);
            e.Exception.Data.Add("User", e.Context.Member.GetUsernameAndDiscriminator());
            e.Exception.Data.Add("UserId", e.Context.Member.Id);
            e.Exception.Data.Add("ShardId", e.Context.Client.ShardId);

            var sentryEventId = await _ravenClient.CaptureAsync(new SentryEvent(e.Exception));
            _errorRecordIds.Add(_sentryEventIdKey, sentryEventId);
        }

        protected override void AppendErrorRecordIds(StringBuilder sb)
        {
            base.AppendErrorRecordIds(sb);

            sb.AppendLine($"Sentry event ID: {_errorRecordIds[_sentryEventIdKey]}");
        }
    }
}
