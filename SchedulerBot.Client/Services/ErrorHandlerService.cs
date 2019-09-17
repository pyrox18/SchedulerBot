using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using SchedulerBot.Client.Attributes;

namespace SchedulerBot.Client.Services
{
    public class ErrorHandlerService : IErrorHandlerService
    {
        protected readonly ILogger<IErrorHandlerService> _logger;
        protected readonly List<Type> _ignoredExceptionTypes;
        protected readonly Dictionary<string, string> _errorRecordIds;

        protected const string _errorIdKey = "errorId";

        public ErrorHandlerService(ILogger<IErrorHandlerService> logger)
        {
            _logger = logger;

            _ignoredExceptionTypes = new List<Type>
            {
                typeof(ChecksFailedException),
                typeof(CommandNotFoundException),
                typeof(ArgumentException),
                typeof(UnauthorizedException),
                typeof(InvalidOperationException)
            };

            _errorRecordIds = new Dictionary<string, string>();
        }

        public async Task HandleCommandErrorAsync(CommandErrorEventArgs e)
        {
            if (IsIgnorableErrorEvent(e)) return;

            await RecordError(e);

            await e.Context.RespondAsync(GetErrorResponseMessage(e.Exception.Message));
        }

        protected bool IsIgnorableErrorEvent(CommandErrorEventArgs e)
        {
            if (_ignoredExceptionTypes.Contains(e.Exception.GetType()))
            {
                return true;
            }

            if (e.Exception is ChecksFailedException && (e.Exception as ChecksFailedException).FailedChecks
                .Any(x => x.GetType() == typeof(PermissionNodeAttribute)))
            {
                return true;
            }

            return false;
        }

        protected virtual Task RecordError(CommandErrorEventArgs e)
        {
            var errorId = Guid.NewGuid();
            _logger.LogError(e.Exception, $"Command error occurred. Error event ID: {errorId}");

            _errorRecordIds.Add(_errorIdKey, errorId.ToString());

            return Task.CompletedTask;
        }

        protected string GetErrorResponseMessage(string exceptionMessage)
        {
            var sb = new StringBuilder();
            sb.AppendLine("An error has occurred. Please report this in the support server using the `support` command.");
            AppendErrorRecordIds(sb);
            sb.AppendLine("```");
            sb.AppendLine($"{exceptionMessage}");
            sb.AppendLine("```");

            return sb.ToString();
        }

        protected virtual void AppendErrorRecordIds(StringBuilder sb)
        {
            sb.AppendLine($"Error event ID: {_errorRecordIds[_errorIdKey]}");
        }
    }
}
