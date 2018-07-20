using System;
using System.Collections;

namespace SchedulerBot.Client
{
    public class Extraction
    {
        public bool IsValid { get; set; }
        public IEnumerable Values { get; set; }
        public string ErrorMessage { get; set; }
    }
}