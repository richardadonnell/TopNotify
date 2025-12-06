using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace TopNotify.Daemon
{
    public class ReadAloudInterceptor : Interceptor, IDisposable
    {
        private readonly Lazy<SpeechSynthesizer> _synthesizer = new(
            () => new SpeechSynthesizer(),
            LazyThreadSafetyMode.ExecutionAndPublication);
        private bool _disposed;

        public override void OnNotification(UserNotification notification)
        {
            if (!Settings.ReadAloud) { return; }

            _synthesizer.Value.SetOutputToDefaultAudioDevice();
            _synthesizer.Value.Speak(GetNotificationAsText(notification));


            base.OnNotification(notification);
        }


        /// <summary>
        /// Returns A Text-To-Speech Friendly String Based On A Notification's Contents
        /// </summary>
        public static string GetNotificationAsText(UserNotification notification)
        {
            var text = new StringBuilder("New notification from");
            text.Append(notification.AppInfo.DisplayInfo.DisplayName).Append(".\n");
            foreach (var binding in notification.Notification.Visual.Bindings)
            {
                foreach (var notificationText in binding.GetTextElements())
                {
                    text.Append(notificationText.Text).Append(".\n");
                }
            }

            return text.ToString();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing && _synthesizer.IsValueCreated)
            {
                _synthesizer.Value.Dispose();
            }

            _disposed = true;
        }
    }
}
