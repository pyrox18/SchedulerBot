using System;

namespace SchedulerBot.Client.Exceptions
{
    public class PermissionNodeNotFoundException : Exception
    {
        public PermissionNodeNotFoundException(string node) :
            base($"Permission node not found for string {node}")
        {
        }
    }
}
