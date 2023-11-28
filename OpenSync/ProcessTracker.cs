using System.Diagnostics;
using System.Timers;

namespace OpenSync
{
    internal class ProcessTracker
    {
        private List<TrackingApp> trackingApps;
        private System.Timers.Timer timer;

        public delegate void ProcessEventHandler(object sender, TrackingApp trackedApp);

        public event ProcessEventHandler ProcessStarted;
        public event ProcessEventHandler ProcessStopped;

        public ProcessTracker(List<TrackingApp> apps)
        {
            trackingApps = apps;
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            timer = new System.Timers.Timer
            {
                Interval = 1000
            };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var trackedApp in trackingApps)
            {
                bool isRunning = IsProcessRunning(trackedApp.ProcessToTrack);

                if (isRunning && !trackedApp.IsRunning)
                {
                    trackedApp.IsRunning = true;
                    if (ProcessStarted != null)
                    {
                        ProcessStarted(this, trackedApp);
                    }
                }
                else if (!isRunning && trackedApp.IsRunning)
                {
                    trackedApp.IsRunning = false;
                    if (ProcessStopped != null)
                    {
                        ProcessStopped(this, trackedApp);
                    }
                }
            }
        }

        private bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        public void UpdateTrackedProcess(int index, string newName)
        {
            if (index >= 0 && index < trackingApps.Count)
            {
                TrackingApp appToUpdate = trackingApps[index];
                appToUpdate.ProcessToTrack = newName;
            }
        }

        public void RemoveTrackedProcess(int index)
        {
            if (index >= 0 && index < trackingApps.Count)
            {
                trackingApps.RemoveAt(index);
            }
        }
    }
}
