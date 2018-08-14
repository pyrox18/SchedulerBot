using System;

namespace SchedulerBot.Data.Models
{
    public class Permission
    {
        public Guid Id { get; set; }
        public Calendar Calendar { get; set; }
        public PermissionType Type { get; set; }
        public PermissionNode Node { get; set; }
        public ulong TargetId { get; set; }
        public bool IsDenied { get; set; }
    }

    public enum PermissionNode
    {
        EventCreate,
        EventUpdate,
        EventDelete,
        EventList,
        PrefixShow,
        PrefixModify,
        DefaultChannelShow,
        DefaultChannelModify,
        TimezoneShow,
        TimezoneModify,
        PermsShow,
        PermsModify,
        PermsNodes,
        Ping,
        EventRSVP,
        Time
    }

    public enum PermissionType
    {
        Role,
        User,
        Everyone
    }
}
