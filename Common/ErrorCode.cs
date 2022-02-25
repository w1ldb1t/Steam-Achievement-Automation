enum ErrorCode : int
{
    Success = 0,
    SteamNotRunning = 1,
    SteamApiInitializetionFailure = 2,
    FailedToFetchStats = 3,
    NoAchievementsFound = 4,
    FailedToCommit = 5,
}