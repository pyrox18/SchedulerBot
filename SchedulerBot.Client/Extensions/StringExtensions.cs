using System;
using System.Text.RegularExpressions;

namespace SchedulerBot.Client.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveCaseInsensitive(this string source, string partToRemove)
        {
            string replaced = Regex.Replace(source, Regex.Escape(partToRemove), "", RegexOptions.IgnoreCase);
            return replaced.Trim();
        }
    }
}
