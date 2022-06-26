using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Controller
{
    internal class Program
    {
        #region pInvoke

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }

        private enum ShowWindowCommands
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        private const uint SC_CLOSE = 0xF060;
        private const uint MF_ENABLED = 0x00000000;
        private const uint MF_DISABLED = 0x00000002;

        #endregion

        private static NotifyIcon Tray = default(NotifyIcon);
        private static IntPtr CurrentWindow = default(IntPtr);

        private static Settings GetSettings()
        {
            try
            {
                var jsonText = File.ReadAllText("settings.json");
                JObject schema = JObject.Parse(jsonText);
                return new Settings(schema["apiKey"].ToString(), schema["steamId64"].ToString(), schema["appId"].ToString(),
                                    schema["minMinutes"].ToObject<int>(), schema["maxMinutes"].ToObject<int>());
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void UnlockAchievements()
        {
            Random rnd = new Random();
            // read settings given by the user
            var settings = GetSettings();
            if (settings == null)
            {
                Console.WriteLine("Settings file could not be found!");
                return;
            }

            // We need this, otherwise steam api won't be able to initialize
            File.WriteAllText(string.Format("{0}\\steam_appid.txt", AppDomain.CurrentDomain.BaseDirectory), settings.AppId);

            while (true)
            {
                // get a random interval of minutes between the min/max, that the app is going to sleep
                int mins = rnd.Next(settings.MinMinutes, settings.MaxMinutes);
                Console.WriteLine("Unlocking next achievement in {0} minutes ...", mins);
                Thread.Sleep((int)TimeSpan.FromMinutes(mins).TotalMilliseconds);

                // create the args of the child process
                string procArgs = String.Format("{0} {1} {2}", settings.ApiKey, settings.SteamId64, settings.AppId);
                // create the child process and wait for it to exit
                Process process = Process.Start("agent.exe", procArgs);
                process.WaitForExit();
                // grab the exit code,
                ErrorCode exit_code = (ErrorCode)process.ExitCode;
                // and use the exit code to deduce the outcome
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
                    case ErrorCode.InvalidArgumentCount:
                        Console.WriteLine("Argument mismatch! Make sure all files are on the same version!");
                        Environment.Exit(1);
                        break;
                    default:
                        Console.WriteLine("Unknown error code {0}.", process.ExitCode);
                        break;
                }
            }
        }

        static void Main()
        {
            // set console title & output format
            Console.Title = "Steam Achievement Manager";
            Console.SetOut(new DatedTextWriter());

            // get the window handle
            CurrentWindow = GetConsoleWindow();

            // disable the close button
            EnableMenuItem(GetSystemMenu(CurrentWindow, false), SC_CLOSE, (uint)(MF_ENABLED | MF_DISABLED));

            // add exit button to the context tray menu
            MenuItem mExit = new MenuItem("Exit", new EventHandler(
                (object sender, EventArgs e) => { 
                    Tray.Dispose(); 
                    Environment.Exit(0); 
                }));
            ContextMenu Menu = new ContextMenu(new MenuItem[] { mExit });

            // setup the tray icon information
            Tray = new NotifyIcon()
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Visible = true,
                Text = Console.Title,
                ContextMenu = Menu
            };
            // create tray handler
            Tray.DoubleClick += new EventHandler(
                (object sender, EventArgs e) => { 
                    ShowWindow(CurrentWindow, (int)ShowWindowCommands.Restore); 
                });

            // detect when the console window is minimized and hide it
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    WINDOWPLACEMENT wPlacement = new WINDOWPLACEMENT();
                    GetWindowPlacement(CurrentWindow, ref wPlacement);
                    if (wPlacement.showCmd == (int)ShowWindowCommands.ShowMinimized)
                        ShowWindow(CurrentWindow, (int)ShowWindowCommands.Hide);
                    // 15 ms delay to avoid high CPU usage
                    using (AutoResetEvent AREv = new AutoResetEvent(false))
                        AREv.WaitOne(15, true);
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            // start unlocking achievements
            new Thread(UnlockAchievements).Start();

            // run event handler
            Application.Run();
        }
    }
}
