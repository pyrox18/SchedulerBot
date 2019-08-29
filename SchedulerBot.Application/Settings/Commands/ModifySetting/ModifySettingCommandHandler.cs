using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Settings.Models;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Settings.Commands.ModifySetting
{
    public class ModifySettingCommandHandler :
        IRequestHandler<ModifyPrefixSettingCommand, PrefixSettingViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;

        public ModifySettingCommandHandler(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<PrefixSettingViewModel> Handle(ModifyPrefixSettingCommand request, CancellationToken cancellationToken = default)
        {
            var calendar = await GetCalendar(request.CalendarId);

            calendar.Prefix = request.NewPrefix;

            await _calendarRepository.UpdateAsync(calendar);

            return new PrefixSettingViewModel
            {
                Prefix = calendar.Prefix
            };
        }

        private async Task<Calendar> GetCalendar(ulong calendarId)
        {
            var calendar = await _calendarRepository.GetByIdAsync(calendarId);
            if (calendar is null)
            {
                throw new CalendarNotInitialisedException(calendarId);
            }

            return calendar;
        }
    }
}
