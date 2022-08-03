# Steam Achievement Automation

SAA is a console application built in C#, meant to be run exclusively on Windows. Even though it's much simpler and less fancy than [SAM](https://github.com/gibbed/SteamAchievementManager.git), it allows you to unlock Steam achievements over time, and also it does not alter your game hours (like [samrewritten](https://github.com/PaulCombal/SamRewritten)) while doing so.

## Usage

The application does not need any special patameters. You just need to put a `settings.json` file in the same folder as the built application:

```json
{
    "ApiKey": "A98XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "steamId64": "7684XXXXXXXXXXXXX",
    "AppId": ["1569040", "1238860"],
    "minMinutes": "5",
    "maxMinutes": "150"
}
```
The `ApiKey` you get it directly through [the Steam portal](https://steamcommunity.com/dev/apikey). The `AppId` is the unique Steam identifier for the game you want to unlock achievements for, and you can find it [through SteamDB](https://steamdb.info/apps/).

## Project Structure
In order to be able to query and alter a game's statistics on Steam, we use the Steam API in order to disguise our program as the actual game we want to fake. That has as a consequence for Steam to increase our play time into that game, for as long as the process is alive. To counter that, we use one process called the Agent, which does the actual achievement opening, and closes immediately once it has unlocked it, and one program called the Controller, that opens the agent every X minutes, in order to unlock a new achievement.
