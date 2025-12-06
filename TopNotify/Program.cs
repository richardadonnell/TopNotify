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

        // Backing fields for guarded properties
        private static StoreContext? _context;
        private static Daemon.Daemon? _background;
        private static AppManager? _gui;
        private static IEnumerable<Process>? _validTopNotifyInstances;
        private static Logger? _logger;

        /// <summary>
        /// Store context for in-app purchases. Only available after GUI initialization.
        /// </summary>
        public static StoreContext Context
        {
            get => _context ?? throw new InvalidOperationException("StoreContext has not been initialized. This is only available in GUI mode after App() has been called.");
            set => _context = value;
        }

        /// <summary>
        /// Background daemon instance. Only available in daemon mode.
        /// </summary>
        public static Daemon.Daemon Background
        {
            get => _background ?? throw new InvalidOperationException("Background daemon has not been initialized. This is only available in daemon mode.");
            set => _background = value;
        }

        /// <summary>
        /// GUI app manager instance. Only available in GUI mode.
        /// </summary>
        public static AppManager GUI
        {
            get => _gui ?? throw new InvalidOperationException("GUI has not been initialized. This is only available in settings mode.");
            set => _gui = value;
        }

        /// <summary>
        /// Collection of other TopNotify process instances.
        /// </summary>
        public static IEnumerable<Process> ValidTopNotifyInstances
        {
            get => _validTopNotifyInstances ?? throw new InvalidOperationException("ValidTopNotifyInstances has not been initialized. Call Main() first.");
            set => _validTopNotifyInstances = value;
        }

        /// <summary>
        /// Serilog logger instance. Available after logging initialization in Main().
        /// </summary>
        public static Logger Logger
        {
            get => _logger ?? throw new InvalidOperationException("Logger has not been initialized. Ensure logging is set up before accessing.");
            set => _logger = value;
        }

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
                App().GetAwaiter().GetResult();
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
                .With((w) => { if (w is Win32WebWindow win) win.BackgroundMode = Win32WebWindow.WindowBackgroundMode.Acrylic; })
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

