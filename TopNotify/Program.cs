#define TRACE // Enable Trace.WriteLine

using System;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Toolkit.Uwp.Notifications;
using TopNotify.Daemon;
using TopNotify.Common;
using TopNotify.GUI;
using IgniteView.Core;
using IgniteView.Desktop;
using System.Reflection;
using System.Runtime.InteropServices;
using Windows.Services.Store;
using Serilog;
using Serilog.Core;

namespace TopNotify.Common
{
    public static class Program
    {
        private const string SettingsArg = "--settings";

        public static StoreContext Context { get; set; } = null!;
        public static Daemon.Daemon Background { get; set; } = null!;
        public static AppManager GUI { get; set; } = null!;
        public static IEnumerable<Process> ValidTopNotifyInstances { get; set; } = null!;
        public static Logger Logger { get; set; } = null!;

        public static bool IsDaemonRunning => ValidTopNotifyInstances.Any((p) => {
            try
            {
                ProcessCommandLine.Retrieve(p, out string commandLine, ProcessCommandLine.Parameter.CommandLine);
                return !commandLine.ToLower().Contains(SettingsArg);
            }
            catch
            {
                // Intentionally ignored: Process may have exited or be inaccessible
            }
            return false;
        });

        public static bool IsGUIRunning => ValidTopNotifyInstances.Any((p) => {
            try
            {
                ProcessCommandLine.Retrieve(p, out string commandLine, ProcessCommandLine.Parameter.CommandLine);
                return commandLine.ToLower().Contains(SettingsArg);
            }
            catch
            {
                // Intentionally ignored: Process may have exited or be inaccessible
            }
            return false;
        });

        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
            {
                NotificationTester.MessageBox("Something went wrong with TopNotify", "Unfortunately, TopNotify has crashed. Details: " + e.ExceptionObject.ToString());
            };

            //By Default, The App Will Be Launched In Daemon Mode
            //Daemon Mode Is A Background Process That Handles Changing The Position Of Notifications
            //If The "--settings" Arg Is Used, Then The App Will Launch In Settings Mode
            //Settings Mode Shows A GUI That Can Be Used To Configure The App
            //These Mode Switches Ensure All Functions Of The App Use The Same Executable

            //Find Other Instances Of TopNotify
            ValidTopNotifyInstances = Process.GetProcessesByName("TopNotify").Where((p) => {
                try
                {
                    return !p.HasExited && p.Id != Process.GetCurrentProcess().Id;
                }
                catch
                {
                    // Intentionally ignored: Process may have exited during enumeration
                }
                return false;
            });

            var isGUIRunning = IsGUIRunning;
            var isDaemonRunning = IsDaemonRunning;

            #if !GUI_DEBUG
            if (!args.Contains(SettingsArg) && isDaemonRunning && !isGUIRunning)
            {
                //Open GUI Instead Of Daemon
                TrayIcon.LaunchSettingsMode(null!, null!);
                Environment.Exit(1);
            }
            else if (args.Contains(SettingsArg) && isGUIRunning)
            {
                //Exit To Prevent Multiple GUIs
                Environment.Exit(2);
            }
            else if (!args.Contains(SettingsArg) && isDaemonRunning && isGUIRunning)
            {
                //Exit To Prevent Multiple Daemons
                Environment.Exit(3);
            }
            #endif

            DesktopPlatformManager.Activate(); // Needed here to initiate plugin DLL loading

            #if !GUI_DEBUG
            if (args.Contains(SettingsArg))
            #else
            if (true)
            #endif
            {
                // Initialize Logging For GUI
                Logger = new LoggerConfiguration()
                    .WriteTo.File(Path.Join(Settings.GetAppDataFolder(), "gui.log"), rollingInterval: RollingInterval.Infinite)
                    .CreateLogger();
                Logging.WriteWatermark("GUI");

                // Open The GUI App In Settings Mode
                GUI = new ViteAppManager();
                _ = App();
            }
            else
            {
                // Initialize Logging For Daemon
                Logger = new LoggerConfiguration()
                    .WriteTo.File(Path.Join(Settings.GetAppDataFolder(), "daemon.log"), rollingInterval: RollingInterval.Infinite)
                    .CreateLogger();
                Logging.WriteWatermark("daemon");

                // Open The Background Daemon
                Background = new Daemon.Daemon();
            }

        }

        public static async Task App()
        {
            // Copy The Wallpaper File So That The GUI Can Access It
            WallpaperFinder.CopyWallpaper();
            AppManager.Instance.RegisterDynamicFileRoute("/wallpaper.jpg", WallpaperFinder.WallpaperRoute);

            var mainWindow =
                WebWindow.Create()
                .WithTitle("TopNotify")
                .WithBounds(new LockedWindowBounds((int)(400f * ResolutionFinder.GetScale()), (int)(650f * ResolutionFinder.GetScale())))
                .With((w) => ((w as Win32WebWindow)!).BackgroundMode = Win32WebWindow.WindowBackgroundMode.Acrylic)
                .WithoutTitleBar()
                .Show();

            Context = StoreContext.GetDefault();
            WinRT.Interop.InitializeWithWindow.Initialize(Context, mainWindow.NativeHandle);

            // Clean Up
            GUI.OnCleanUp += () =>
            {
                WallpaperFinder.CleanUp();
                ToastNotificationManagerCompat.Uninstall();
            };

            GUI.Run();
        }

    }
}

