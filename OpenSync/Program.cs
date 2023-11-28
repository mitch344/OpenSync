using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace OpenSync
{
    internal static class Program
    {
        private static Mutex mutex = new Mutex(true, "OpenSync_Mutex");
        private static Form countdownForm;
        private static Icon appIcon;

        [STAThread]
        static void Main()
        {
            if (!IsApplicationFirstInstance())
            {
                ShowErrorMessageAndExit("Another instance of the application is already running.");
            }

            appIcon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sync_image.ico"));
            string trackingAppsFilePath = ConfigurationLoader.GetTrackingAppsFilePath();

            if (!File.Exists(trackingAppsFilePath))
            {
                ShowCreateTrackingAppsDialog();
            }
            else
            {
                RunOpenSync();
            }

            ReleaseMutexAndClose();
        }

        private static bool IsApplicationFirstInstance()
        {
            return mutex.WaitOne(TimeSpan.Zero, true);
        }

        private static void ShowErrorMessageAndExit(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ReleaseMutexAndClose();
        }

        private static void ShowCreateTrackingAppsDialog()
        {
            countdownForm = CreateCountdownForm();

            DialogResult result = countdownForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                RunOpenSync();
            }
        }

        private static Form CreateCountdownForm()
        {
            Form form = new Form
            {
                Text = "OpenSync",
                Size = new Size(400, 200),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen
            };

            Label countdownLabel = new Label
            {
                Text = "Waiting for file to become available...",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button addButton = new Button
            {
                Text = "Create new TrackingApps.json",
                Dock = DockStyle.Bottom
            };

            addButton.Click += (sender, e) =>
            {
                addButton.Enabled = false;
                CreateNewTrackingPath();
                addButton.Enabled = true;
            };

            Button cancelButton = new Button
            {
                Text = "Cancel",
                Dock = DockStyle.Bottom
            };

            cancelButton.Click += (sender, e) =>
            {
                form.DialogResult = DialogResult.Cancel;
            };

            System.Windows.Forms.Timer countdownTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };

            int remainingTime = 120;

            countdownLabel.Text = $"Waiting for TrackingApps.json to become available... ({remainingTime} seconds)";

            countdownTimer.Tick += (sender, e) =>
            {
                remainingTime--;
                countdownLabel.Text = $"Waiting for TrackingApps.json to become available... ({remainingTime} seconds)";

                if (File.Exists(ConfigurationLoader.GetTrackingAppsFilePath()))
                {
                    countdownTimer.Stop();
                    form.DialogResult = DialogResult.OK;
                }
                else if (remainingTime <= 0)
                {
                    countdownTimer.Stop();
                    form.DialogResult = DialogResult.Cancel;
                }
            };

            form.Controls.Add(countdownLabel);
            form.Controls.Add(addButton);
            form.Controls.Add(cancelButton);
            countdownTimer.Start();

            return form;
        }

        private static void CreateNewTrackingPath()
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a folder to store tracking apps data.";
                DialogResult folderResult = folderDialog.ShowDialog();

                if (folderResult == DialogResult.OK)
                {
                    string selectedFolder = folderDialog.SelectedPath;
                    string trackingAppsFilePath = Path.Combine(selectedFolder, "TrackingApps.json");
                    File.WriteAllText(trackingAppsFilePath, "[]");
                    ConfigurationLoader.UpdateTrackingAppsFilePath(trackingAppsFilePath);
                }
            }
        }

        private static void RunOpenSync()
        {
            MainForm mainForm = new MainForm
            {
                Icon = appIcon
            };

            Application.Run(mainForm);
        }

        private static void ReleaseMutexAndClose()
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Close();
                mutex = null;
            }
        }
    }
}
