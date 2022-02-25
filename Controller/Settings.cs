using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
    public class Settings
    {
        public Settings(string apiKey, string appId, int minMinutes, int maxMinutes)
        {
            ApiKey = apiKey;
            AppId = appId;
            MinMinutes = minMinutes;
            MaxMinutes = maxMinutes;
        }
        public string ApiKey { get; private set; }
        public string AppId { get; private set; }
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
    }
}
