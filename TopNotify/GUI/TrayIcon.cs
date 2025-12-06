using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using TopNotify.Daemon;
using TopNotify.Resources;

namespace TopNotify.GUI
{
    /// <summary>
    /// This class makes heavy use of reflection because winforms isnt actually referenced in the project.
    /// </summary>
    public static class TrayIcon
    {
        public static Assembly WinForms { get; set; } = null!;
        public static dynamic? Application { get; set; } = null;


        /// <summary>
        /// Dynamically Loads Winforms And Sets Up A Tray Icon.
        /// </summary>
        public static void Setup()
        {
            // Hardcoded path is intentional: We specifically need .NET Framework WinForms
            // (not .NET Core WinForms) for tray icon functionality on Windows.
            // LoadFile is intentional: We need to load from a specific path, not by assembly name.
#pragma warning disable S1075 // URIs should not be hardcoded
#pragma warning disable S3885 // Use Assembly.Load instead of LoadFile
            WinForms = Assembly.LoadFile(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Windows.Forms.dll");
#pragma warning restore S3885
#pragma warning restore S1075

            AppDomain.CurrentDomain.AssemblyResolve += FindAssembly;

            dynamic? notify = null;
            dynamic? menuStrip = null;
            dynamic? handler = null;

            //Find WinForms Types
            foreach (Type type in WinForms.GetExportedTypes())
            {
                if (type.Name == "Application")
                {
                    Application = type.GetMethods()
                        .FirstOrDefault(method => method.Name == "Run" && method.IsStatic && method.GetParameters().Length == 0);
                }
                else if (type.Name == "NotifyIcon")
                {
                    notify = Activator.CreateInstance(type);
                }
                else if (type.Name == "ContextMenuStrip")
                {
                    menuStrip = Activator.CreateInstance(type);
                }
                else if (type.Name == "ToolStripItemClickedEventHandler")
                {
                    var onTrayButtonClickedMethod = typeof(TrayIcon).GetMethod(
                        nameof(OnTrayButtonClicked),
                        BindingFlags.Public | BindingFlags.Static);

                    if (onTrayButtonClickedMethod == null)
                    {
                        throw new InvalidOperationException(
                            $"Failed to find method '{nameof(OnTrayButtonClicked)}' on type '{nameof(TrayIcon)}'. " +
                            "Ensure the method exists and is public static.");
                    }

                    handler = Delegate.CreateDelegate(type, onTrayButtonClickedMethod);
                }
            }

            //Use WinForms Methods To Create A Tray Icon
            notify!.Visible = true;
            notify.Icon = Util.FindAppIcon();
            notify.Text = Strings.TrayIconTooltip;
            notify.DoubleClick += new EventHandler(LaunchSettingsMode);
            notify.ContextMenuStrip = menuStrip;
            notify.ContextMenuStrip.Items.Add(Strings.TrayMenuBugReport);
            notify.ContextMenuStrip.Items.Add(Strings.TrayMenuQuit);
            notify.ContextMenuStrip.ItemClicked += handler;
        }

        //Quick And Dirty Method Of Loading WinForms Dependencies
        private static Assembly? FindAssembly(object? sender, ResolveEventArgs args)
        {

            if (args.Name.StartsWith("Accessibility"))
            {
                // Hardcoded path is intentional: We specifically need .NET Framework Accessibility
                // for WinForms tray icon functionality on Windows.
#pragma warning disable S1075 // URIs should not be hardcoded
                return Assembly.LoadFile(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Accessibility.dll");
#pragma warning restore S1075
            }

            return null;
        }

        public static void MainLoop()
        {
            Application!.Invoke(null, null);
        }


        public static void OnTrayButtonClicked(object Sender, EventArgs e)
        {
            var item = e.GetType().GetProperty("ClickedItem")!.GetValue(e)!;
            var itemText = item.GetType().GetProperty("Text")!.GetValue(item)!.ToString();

            if (itemText == Strings.TrayMenuBugReport)
            {
                BugReport.DisplayBugReport(BugReport.CreateBugReport());
            }
            else if (itemText == Strings.TrayMenuQuit)
            {
                Quit();
            }
        }

        public static void Quit()
        {
            //Kill Other Instances
            var instances = Process.GetProcessesByName("TopNotify");
            foreach (var instance in instances)
            {
                if (instance.Id != Process.GetCurrentProcess().Id)
                {
                    try
                    {
                        instance.Kill();
                    }
                    catch
                    {
                        // Intentionally ignored: Process may have already exited or we may not
                        // have permission to kill it. Either way, we're exiting anyway.
                    }
                }
            }


            Environment.Exit(0);
        }

        public static void LaunchSettingsMode(object? Sender, EventArgs e)
        {
            try
            {
                var exe = Util.FindExe();
                var psi = new ProcessStartInfo(exe, "--settings" + (Debugger.IsAttached ? " --debug-process" : "")); // Use Debug Args If Needed
                psi.UseShellExecute = false;
                psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Process.Start(psi);
            }
            catch (Exception)
            {
                // Intentionally ignored: If we can't launch settings mode, there's nothing
                // the user can do about it from the tray icon context. Silently fail.
            }
        }

    }
}
