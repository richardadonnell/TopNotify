using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using static TopNotify.Daemon.NativeInterceptor;

namespace TopNotify.Daemon
{
    public class InterceptorManager
    {
        #region WinAPI Methods

        #endregion

        public static InterceptorManager Instance { get; set; } = null!;
        public List<Interceptor> Interceptors { get; set; } = new();

        public Settings CurrentSettings { get; set; } = null!;

        public int TimeSinceReflow { get; set; } = 0;
        public const int ReflowTimeout = 50;

        public ConcurrentDictionary<uint, Action?> CleanUpFunctions { get; } = new ConcurrentDictionary<uint, Action?>(); // Maps HandledNotifications to the associated clean up function
        public UserNotificationListener Listener { get; set; } = null!;
        public bool CanListenToNotifications { get; set; } = false;

        public static Interceptor[] InstalledInterceptors { get; } =
        {
            new NativeInterceptor(),
            new DiscoveryInterceptor(),
            new SoundInterceptor(),
            new ReadAloudInterceptor()
        };

        public void Start()
        {
            Instance = this;
            CurrentSettings = Settings.Get();

            Interceptors.AddRange(InstalledInterceptors.Where(i => i.ShouldEnable()));

            Listener = UserNotificationListener.Current;
            Task.Run(async () =>
            {

                //Ask For Permissions To Read Notifications
                var access = await Listener.RequestAccessAsync();
                if (access != UserNotificationListenerAccessStatus.Allowed)
                {
                    var msg = "Failed To Start Notification Listener: Permission Denied";
                    DaemonErrorHandler.ThrowNonCritical(new DaemonError("listener_failure_no_permission", msg));
                    return;
                }


                try
                {
                    //Throws a COM exception if not packaged into an MSIX app
                    //Currently no workaround
                    Listener.NotificationChanged += OnNotificationChanged;
                }
                catch (Exception)
                {
                    var msg = "Failed To Start Notification Listener: Not Packaged";
                    DaemonErrorHandler.ThrowNonCritical(new DaemonError("listener_failure_not_packaged", msg));
                    return;
                }

                CanListenToNotifications = true;

            });

            foreach (Interceptor i in Interceptors)
            {
                i.Start();
            }

            MainLoop();
        }

        /// <summary>
        /// Daemon main loop - intentionally runs indefinitely until process termination.
        /// </summary>
#pragma warning disable S2190 // Loops and recursions should not be infinite
        public void MainLoop()
        {
            while (true)
            {
                TimeSinceReflow++;

                if (TimeSinceReflow > ReflowTimeout)
                {
                    TimeSinceReflow = 0;
                    Reflow();
                }

                Update();

                Thread.Sleep(10);
            }
        }
#pragma warning restore S2190

        public void Reflow()
        {
            foreach (Interceptor i in Interceptors)
            {
                try
                {
                    i.Reflow();
                }
                catch (Exception)
                {
                    // Intentionally ignored: Individual interceptor failures should not
                    // affect other interceptors or crash the daemon's main loop.
                }
            }
        }

        public void Update()
        {
            foreach (Interceptor i in Interceptors)
            {
                try
                {
                    i.Update();
                }
                catch (Exception)
                {
                    // Intentionally ignored: Individual interceptor failures should not
                    // affect other interceptors or crash the daemon's main loop.
                }
            }
        }

        // Called from C++ to safely invoke the OnKeyUpdate method
        public static void TryOnKeyUpdate()
        {
            if (Instance == null) { return; }
            Instance.OnKeyUpdate();
        }

        public void OnKeyUpdate()
        {
            foreach (Interceptor i in Interceptors)
            {
                try
                {
                    i.OnKeyUpdate();
                }
                catch (Exception)
                {
                    // Intentionally ignored: Individual interceptor failures should not
                    // affect other interceptors or crash the daemon's main loop.
                }
            }
        }

        // Runs When A New Notification Is Added Or Removed
        public async void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            var userNotifications = await Listener.GetNotificationsAsync(NotificationKinds.Toast);
            var userNotification = userNotifications.FirstOrDefault(n => n.Id == args.UserNotificationId);

            if (args.ChangeKind == UserNotificationChangedKind.Added && userNotification != null)
            {
                foreach (Interceptor i in Interceptors)
                {
                    try
                    {
                        i.OnNotification(userNotification);
                    }
                    catch
                    {
                        // Intentionally ignored: Individual interceptor failures should not
                        // affect other interceptors or crash the notification handler.
                    }
                }
            }

            Update();
        }

        public void OnSettingsChanged()
        {
            CurrentSettings = Settings.Get();

            foreach (Interceptor i in Interceptors)
            {
                try
                {
                    i.Restart();
                    i.Reflow();
                }
                catch (Exception)
                {
                    // Intentionally ignored: Individual interceptor failures should not
                    // affect other interceptors during settings refresh.
                }
            }
        }
    }
}
