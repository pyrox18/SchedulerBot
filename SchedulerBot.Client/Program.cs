using System;
using System.Text;

namespace SchedulerBot.Client
{
    class Program
    {
        static void Main(string[] args = null)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var bot = new Bot();
            bot.Run().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
