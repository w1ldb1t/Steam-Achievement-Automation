using System;
using System.Threading;
using System.Net;
using System.IO;
using Steamworks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Agent
{
	class Program
	{
		private static JEnumerable<JToken> FetchAchievementNamesForGame(string apiKey, string gameId)
        {
			string formattedUrl = String.Format("http://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v0002/?key={0}&appid={1}&l=english&format=json", apiKey, gameId);
			WebRequest schemaReq = WebRequest.Create(formattedUrl);
			Stream objStream = schemaReq.GetResponse().GetResponseStream();
			StreamReader objReader = new StreamReader(objStream);
			string response = objReader.ReadToEnd();

			JObject schema = JObject.Parse(response);
			JEnumerable<JToken> results = schema["game"]["availableGameStats"]["achievements"].Children();

			return results;
		}
		private static JEnumerable<JToken> FetchAchievementPercentagesForGame(string apiKey, string gameId)
        {
			string formattedUrl = String.Format("https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v2/?key={0}&gameid={1}&l=english&format=json", apiKey, gameId);
			WebRequest schemaReq = WebRequest.Create(formattedUrl);
			Stream objStream = schemaReq.GetResponse().GetResponseStream();
			StreamReader objReader = new StreamReader(objStream);
			string response = objReader.ReadToEnd();

			JObject schema = JObject.Parse(response);
			JEnumerable<JToken> results = schema["achievementpercentages"]["achievements"].Children();

			return results;
		}
		private static JEnumerable<JToken> FetchAchievementStatsForGame(string apiKey, string steamId64, string gameId)
        {
			string formattedUrl = String.Format("https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?key={0}&steamid={1}&appid={2}&l=english&format=json", apiKey, steamId64, gameId);
			WebRequest schemaReq = WebRequest.Create(formattedUrl);
			Stream objStream = schemaReq.GetResponse().GetResponseStream();
			StreamReader objReader = new StreamReader(objStream);
			string response = objReader.ReadToEnd();

			JObject schema = JObject.Parse(response);
			JEnumerable<JToken> results = schema["playerstats"]["achievements"].Children();

			return results;
		}
		private static List<Achievement> GetAchievementList(string apiKey, string steamId64, string gameId) {
			List<Achievement> achievements = new List<Achievement>();

			JEnumerable<JToken> achievementNames = FetchAchievementNamesForGame(apiKey, gameId);
			JEnumerable<JToken> achievementPercentages = FetchAchievementPercentagesForGame(apiKey, gameId);
			JEnumerable<JToken> achievementStats = FetchAchievementStatsForGame(apiKey, steamId64, gameId);

			foreach (JToken result in achievementNames)
            {
				string achievementName = result.SelectToken("name").ToString();

				double percentage = double.MaxValue;
                foreach (var item in achievementPercentages)
                {
					if(item.SelectToken("name").ToString() == achievementName)
                    {
						percentage = item.SelectToken("percent").ToObject<double>();
						break;
                    }
                }

				bool unlocked = false;
				foreach(var item in achievementStats)
                {
					if(item.SelectToken("apiname").ToString() == achievementName)
						if(item.SelectToken("achieved").ToObject<bool>())
							unlocked = true;
                }

				achievements.Add(new Achievement(achievementName, percentage, unlocked));
			}

			return achievements;
		}

		public static int Main(string[] args) {
			if (args.Length != 3)
				return (int)ErrorCode.InvalidArgumentCount;

			string apiKey = args[0];
			string steamId64 = args[1];
			string appId = args[2];

			if(!SteamAPI.IsSteamRunning())
				return (int)ErrorCode.SteamNotRunning;

			if (!SteamAPI.Init())
				return (int)ErrorCode.SteamApiInitializetionFailure;

			if (!SteamUserStats.RequestCurrentStats())
				return (int)ErrorCode.FailedToFetchStats;

			List<Achievement> achievements = GetAchievementList(apiKey, steamId64, appId);
			achievements.RemoveAll(x => x.Unlocked == true);
			achievements.Sort((x, y) => x.Percent.CompareTo(y.Percent));
			achievements.Reverse();

			if (achievements == null)
				return (int)ErrorCode.FailedToFetchStats;
			if (achievements.Count == 0)
				return (int)ErrorCode.NoAchievementsFound;
				
			// unlock the most frequently locked achievement
			SteamUserStats.SetAchievement(achievements[0].Id);

			if (!SteamUserStats.StoreStats())
				return (int)ErrorCode.FailedToCommit;

			SteamAPI.Shutdown();
			return (int)ErrorCode.Success;
		}
	}
}
