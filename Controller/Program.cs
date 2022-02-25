using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Controller
{
    internal class Program
    {
        public static Settings GetSettings()
        {
            var jsonText = File.ReadAllText("settings.json");
            JObject schema = JObject.Parse(jsonText);
            return new Settings(schema["ApiKey"].ToString(), schema["AppId"].ToString());
        }

        static void Main()
        {
            Console.Title = "Steam Achievement Manager";

            var settings = GetSettings();
            Random rnd = new Random();

            // We need this, otherwise steam api won't be able to initialize
            File.WriteAllText(string.Format("{0}\\steam_appid.txt", AppDomain.CurrentDomain.BaseDirectory), settings.AppId);

            while (true)
            {
                int mins = rnd.Next(5, 180);
                Console.WriteLine("Unlocking next achievement in {0} minutes ...", mins);
                Thread.Sleep((int)TimeSpan.FromMinutes(mins).TotalMilliseconds);

                string procArgs = String.Format("{0} {1}", settings.ApiKey, settings.AppId);
                Process process = Process.Start("agent.exe", procArgs);
                process.WaitForExit();
                ErrorCode exit_code = (ErrorCode)process.ExitCode;

                switch (exit_code)
                {
                    case ErrorCode.Success:
                        Console.WriteLine("New achievement unlocked!");
                        break;
                    case ErrorCode.SteamNotRunning:
                        Console.WriteLine("Steam not running!");
                        Environment.Exit(1);
                        break;
                    case ErrorCode.SteamApiInitializetionFailure:
                        Console.WriteLine("Steam API initialization failure!");
                        Environment.Exit(1);
                        break;
                    case ErrorCode.FailedToFetchStats:
                        Console.WriteLine("Failed to fetch statistics for current game!");
                        break;
                    case ErrorCode.NoAchievementsFound:
                        Console.WriteLine("No unlocked achievements found for current game!");
                        Environment.Exit(1);
                        break;
                    case ErrorCode.FailedToCommit:
                        Console.WriteLine("Achievement changes could not be saved!");
                        break;
                    default:
                        Console.WriteLine("Unknown error code {0}.", process.ExitCode);
                        break;
                }
            }
        }
    }
}
