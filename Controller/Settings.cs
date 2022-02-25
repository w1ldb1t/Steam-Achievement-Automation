using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
    public class Settings
    {
        public Settings(string apiKey, string appId)
        {
            ApiKey = apiKey;
            AppId = appId;
        }
        public string ApiKey { get; private set; }
        public string AppId { get; private set; }
    }
}
