using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
    public class Settings
    {
        public Settings(string apiKey, string steamId64, List<string> appId, int minMinutes, int maxMinutes)
        {
            ApiKey = apiKey;
            SteamId64 = steamId64;
            AppId = appId;
            MinMinutes = minMinutes;
            MaxMinutes = maxMinutes;
        }
        public string ApiKey { get; private set; }
        public string SteamId64 { get; private set; }
        public List<string> AppId { get; private set; }
        public int MinMinutes { get; private set; }
        public int MaxMinutes { get; private set; }
    }
}
