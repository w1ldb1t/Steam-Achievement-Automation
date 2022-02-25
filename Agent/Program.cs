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
		private static List<Achievement> GetAchievementList(string apiKey, string gameId) {
			List<Achievement> achievements = new List<Achievement>();

			try
			{
				string formattedUrl = String.Format("http://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v0002/?key={0}&appid={1}&l=english&format=json", apiKey, gameId);
				WebRequest schemaReq = WebRequest.Create(formattedUrl);
				Stream objStream = schemaReq.GetResponse().GetResponseStream();
				StreamReader objReader = new StreamReader(objStream);
				string response = objReader.ReadToEnd();

				JObject schema = JObject.Parse(response);
				JEnumerable<JToken> results = schema["game"]["availableGameStats"]["achievements"].Children();

				foreach (JToken result in results)
					achievements.Add(new Achievement(result.SelectToken("displayName").ToString(), result.SelectToken("name").ToString()));
			}
			catch (Exception ex)
            {
				Console.WriteLine("Exception occured! Error message: {0}", ex.Message);
            }

			return achievements;
		}

		public static int Main(string[] args) {
			if (args.Length != 2)
				return (int)ErrorCode.InvalidArgumentCount;

			string apiKey = args[0];
			string appId = args[1];

			if(!SteamAPI.IsSteamRunning())
				return (int)ErrorCode.SteamNotRunning;

			Random rnd = new Random();

			if (!SteamAPI.Init())
				return (int)ErrorCode.SteamApiInitializetionFailure;

			if (!SteamUserStats.RequestCurrentStats())
				return (int)ErrorCode.FailedToFetchStats;

			List<Achievement> achievements = GetAchievementList(apiKey, appId);
			if (achievements.Count == 0)
				return (int)ErrorCode.NoAchievementsFound;
				
			int rnd_index = rnd.Next(achievements.Count);
			Achievement achievement = achievements[rnd_index];
			SteamUserStats.SetAchievement(achievement.GetId());

			if (!SteamUserStats.StoreStats())
				return (int)ErrorCode.FailedToCommit;

			Console.WriteLine("Achievement \"{0}\" unlocked!", achievement.GetDisplayName());
			SteamAPI.Shutdown();
			return (int)ErrorCode.Success;
		}
	}
}
