#if UNITY_EDITOR_OSX
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NightOwl.macOS {
    public class AppearanceMonitor
    {
        [DllImport("AutoDarkTheme-Native-macOS")]
        public static extern bool IsDarkAppearance();


        private Task monitorTask;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();


        public event EventHandler AppearanceChanged;
        private void RaiseAppearanceChanged()
        {
            var handler = AppearanceChanged;
            handler?.Invoke(typeof(UserPreferences), EventArgs.Empty);
        }


        public void Start()
        {
            monitorTask = new Task(async () =>
            {
                bool wasDarkAppearance = IsDarkAppearance();

                while (!tokenSource.Token.IsCancellationRequested)
                {
                    var isDarkAppearance = IsDarkAppearance();
                    if (isDarkAppearance != wasDarkAppearance)
                    {
                        RaiseAppearanceChanged();
                        wasDarkAppearance = isDarkAppearance;
                    }

                    await Task.Delay(1000, tokenSource.Token);
                }
            }, tokenSource.Token);
            monitorTask.Start();
        }

        public void Stop()
        {
            tokenSource.Cancel();
        }
    }
}
#endif