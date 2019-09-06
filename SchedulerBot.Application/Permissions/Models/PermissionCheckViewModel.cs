namespace SchedulerBot.Application.Permissions.Models
{
    public class PermissionCheckViewModel
    {
        public bool IsPermitted { get; set; }

        public PermissionCheckViewModel(bool isPermitted)
        {
            IsPermitted = isPermitted;
        }
    }
}
