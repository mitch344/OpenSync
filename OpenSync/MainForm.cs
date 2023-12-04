using Newtonsoft.Json;
using System.Diagnostics;

namespace OpenSync
{
    public class MainForm : Form
    {
        private ProcessTracker processTracker;
        private BackupManager backupManager;
        private Dictionary<string, bool> processStartedFlags = new Dictionary<string, bool>();
        private Dictionary<string, string> initialChecksums = new Dictionary<string, string>();
        private List<TrackingApp> trackingApps;
        private ListView listViewTrackingApps;
        private ToolStripMenuItem addItem;
        private ToolStripMenuItem restoreLatestBackupItem;
        private string trackingAppsFilePath;
        private NotifyIcon notifyIcon;
        private bool isFormClosed = false;
        private Button refreshButton;

        public MainForm()
        {
            InitializeMainForm();
            CenterToScreen();
            LoadConfigurationFile();
            InitializeProcessTracker();
            backupManager = new BackupManager();
            PopulateListView(trackingApps);
        }

        private void InitializeMainForm()
        {
            Text = "OpenSync";
            Width = 600;
            Height = 300;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            InitializeSystemTrayLauncher();
            InitializeSyncButton();
            InitializeListView();
            InitializeContextMenu();
        }

        private void InitializeSyncButton()
        {
            refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(10, 10)
            };
            refreshButton.Click += RefreshButton_Click;
            Controls.Add(refreshButton);
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            refreshButton.Enabled = false;
            RefreshTrackingAppsFromJSON();
            refreshButton.Enabled = true;
        }

        private void InitializeSystemTrayLauncher()
        {
            string appName = "OpenSync";
            notifyIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Text = appName,
                Visible = true
            };

            var trayContextMenu = new ContextMenuStrip();
            var exitMenuItem = new ToolStripMenuItem("Exit", null, SystemTrayExitClick);
            trayContextMenu.Items.Add(exitMenuItem);

            notifyIcon.Click += SystemTrayClick;
            notifyIcon.ContextMenuStrip = trayContextMenu;

            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                isFormClosed = true;
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
            }
        }

        private void InitializeListView()
        {
            listViewTrackingApps = new ListView
            {
                Location = new Point(0, refreshButton.Bottom + 10),
                Size = new Size(Width - 20, Height - 100 - refreshButton.Height - 10),
                View = View.Details,
                FullRowSelect = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            listViewTrackingApps.MultiSelect = false;
            listViewTrackingApps.DoubleClick += ListViewItemDoubleClick;

            int listViewWidth = listViewTrackingApps.ClientSize.Width;
            int column1Width = (int)(listViewWidth * 0.20);
            int column2Width = (int)(listViewWidth * 0.40);
            int column3Width = (int)(listViewWidth * 0.40);

            listViewTrackingApps.Columns.Add("Process to Track", column1Width, HorizontalAlignment.Left);
            listViewTrackingApps.Columns.Add("Source", column2Width, HorizontalAlignment.Left);
            listViewTrackingApps.Columns.Add("Destination", column3Width, HorizontalAlignment.Left);

            Controls.Add(listViewTrackingApps);
        }

        private void InitializeContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            addItem = new ToolStripMenuItem("Add");
            addItem.Click += ContextMenuAddItemClick;
            contextMenu.Items.Add(addItem);

            string addIcon = Path.Combine(Application.StartupPath, "Icons", "add_image.ico");
            if (File.Exists(addIcon))
            {
                Icon icon = new Icon(addIcon);
                addItem.Image = icon.ToBitmap();
            }

            restoreLatestBackupItem = new ToolStripMenuItem("Restore to Latest Backup");
            restoreLatestBackupItem.Click += ContextMenuRestoreLatestVersionItemClick;
            contextMenu.Items.Add(restoreLatestBackupItem);

            string restoreIcon = Path.Combine(Application.StartupPath, "Icons", "restore_image.ico");
            if (File.Exists(restoreIcon))
            {
                Icon icon = new Icon(restoreIcon);
                restoreLatestBackupItem.Image = icon.ToBitmap();
            }

            var showBackupsItem = new ToolStripMenuItem("Backups");
            showBackupsItem.Click += ContextMenuShowBackupsItemClick;
            contextMenu.Items.Add(showBackupsItem);


            string backupIcon = Path.Combine(Application.StartupPath, "Icons", "backups_image.ico");
            if (File.Exists(backupIcon))
            {
                Icon icon = new Icon(backupIcon);
                showBackupsItem.Image = icon.ToBitmap();
            }


            var openSourcePathItem = new ToolStripMenuItem("Open Source Path in File Explorer");
            openSourcePathItem.Click += ContextMenuOpenSourcePathItemClick;
            contextMenu.Items.Add(openSourcePathItem);

            string folder = Path.Combine(Application.StartupPath, "Icons", "folder_image.ico");
            if (File.Exists(folder))
            {
                Icon icon = new Icon(folder);
                openSourcePathItem.Image = icon.ToBitmap();
            }

            var deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += DeleteListViewItemClick;
            contextMenu.Items.Add(deleteItem);

            string deleteIcon = Path.Combine(Application.StartupPath, "Icons", "delete_image.ico");
            if (File.Exists(deleteIcon))
            {
                Icon icon = new Icon(deleteIcon);
                deleteItem.Image = icon.ToBitmap();
            }

            contextMenu.Opening += (sender, e) =>
            {
                if (listViewTrackingApps.SelectedItems.Count > 0)
                {
                    addItem.Visible = false;
                    restoreLatestBackupItem.Visible = true;
                    deleteItem.Visible = true;
                    showBackupsItem.Visible = true;
                }
                else
                {
                    addItem.Visible = true;
                    restoreLatestBackupItem.Visible = false;
                    deleteItem.Visible = false;
                    showBackupsItem.Visible = false;
                }
            };

            listViewTrackingApps.ContextMenuStrip = contextMenu;
        }

        private void LoadConfigurationFile()
        {
            try
            {
                trackingAppsFilePath = ConfigurationLoader.GetTrackingAppsFilePath();
                trackingApps = LoadTrackingAppsConfiguration(trackingAppsFilePath);
            }
            catch (FileNotFoundException ex)
            {
                ShowConfigurationErrorMessageBox("File Not Found", ex.Message);
                trackingApps = new List<TrackingApp>();
            }
            catch (InvalidOperationException ex)
            {
                ShowConfigurationErrorMessageBox("Configuration Error", ex.Message);
                trackingApps = new List<TrackingApp>();
            }
        }

        private void InitializeProcessTracker()
        {
            processTracker = new ProcessTracker(trackingApps);
            processTracker.ProcessStarted += OnProcessStarted;
            processTracker.ProcessStopped += OnProcessStopped;

            foreach (var app in trackingApps)
                processStartedFlags[app.ProcessToTrack] = false;
        }

        private List<TrackingApp> LoadTrackingAppsConfiguration(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The configuration file was not found: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            List<TrackingApp> trackingApps = JsonConvert.DeserializeObject<List<TrackingApp>>(jsonContent);

            if (trackingApps == null)
            {
                throw new InvalidOperationException("The configuration file is empty or not formatted correctly.");
            }

            return trackingApps;
        }

        private void PopulateListView(List<TrackingApp> trackingApps)
        {
            listViewTrackingApps.Items.Clear();
            foreach (var app in trackingApps)
            {
                var item = new ListViewItem(app.ProcessToTrack);
                item.SubItems.Add(app.Source);
                item.SubItems.Add(app.Destination);
                item.Tag = app;
                listViewTrackingApps.Items.Add(item);
            }
        }

        private void ListViewItemDoubleClick(object sender, EventArgs e)
        {
            if (listViewTrackingApps.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewTrackingApps.SelectedItems[0];
                TrackingApp appToModify = selectedItem.Tag as TrackingApp;

                if (appToModify != null)
                {
                    TrackingAppEditForm editForm = new TrackingAppEditForm(appToModify, UpdateTrackingApp)
                    {
                        Icon = Icon
                    };
                    editForm.ShowDialog();
                }
            }
        }

        private void ContextMenuOpenSourcePathItemClick(object sender, EventArgs e)
        {
            if (listViewTrackingApps.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewTrackingApps.SelectedItems[0];
                TrackingApp appToOpen = selectedItem.Tag as TrackingApp;

                if (appToOpen != null)
                {
                    string sourcePath = appToOpen.Source;

                    if (File.Exists(sourcePath))
                    {
                        string folderPath = Path.GetDirectoryName(sourcePath);
                        if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                        {
                            Process.Start("explorer.exe", folderPath);
                        }
                    }
                    else if (Directory.Exists(sourcePath))
                    {
                        Process.Start("explorer.exe", sourcePath);
                    }
                    else
                    {
                        MessageBox.Show($"The source path '{sourcePath}' does not exist.", "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DeleteListViewItemClick(object sender, EventArgs e)
        {
            if (listViewTrackingApps.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewTrackingApps.SelectedItems[0];
                TrackingApp appToDelete = selectedItem.Tag as TrackingApp;

                if (appToDelete != null)
                {
                    DialogResult result = MessageBox.Show(
                        $"Are you sure you want to delete the selected item '{appToDelete.ProcessToTrack}'?",
                        "Confirm Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        listViewTrackingApps.Items.Remove(selectedItem);
                        trackingApps.Remove(appToDelete);
                        SaveTrackingAppsConfiguration();
                    }
                }
            }
        }

        private void ContextMenuRestoreLatestVersionItemClick(object sender, EventArgs e)
        {
            if (listViewTrackingApps.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewTrackingApps.SelectedItems[0];
                TrackingApp appToRestore = selectedItem.Tag as TrackingApp;

                try
                {
                    string message = $"Do you want to restore the latest version of '{appToRestore.ProcessToTrack}' to '{appToRestore.Source}'?\nThis will overwrite the current version.";
                    DialogResult result = MessageBox.Show(message, "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        backupManager.RestoreLatestVersion(appToRestore);
                        MessageBox.Show("Restore completed successfully.", "Restore Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Restore failed: {ex.Message}", "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ContextMenuAddItemClick(object sender, EventArgs e)
        {
            var newTrackingApp = new TrackingApp();
            TrackingAppEditForm editForm = new TrackingAppEditForm(newTrackingApp, AddTrackingApp);
            editForm.Show();
        }

        private void ContextMenuShowBackupsItemClick(object sender, EventArgs e)
        {
            if (listViewTrackingApps.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewTrackingApps.SelectedItems[0];
                TrackingApp selectedApp = selectedItem.Tag as TrackingApp;

                if (selectedApp != null)
                {
                    ShowBackupsForm backupsForm = new ShowBackupsForm(selectedApp)
                    {
                        Icon = Icon
                    };
                    backupsForm.ShowDialog();
                }
            }
        }

        private void SystemTrayClick(object sender, EventArgs e)
        {
            if (e is MouseEventArgs mouseEventArgs && mouseEventArgs.Button == MouseButtons.Right)
            {
                return;
            }

            if (isFormClosed)
            {
                RefreshTrackingAppsFromJSON();
                Show();
                WindowState = FormWindowState.Normal;
                Left = Screen.PrimaryScreen.WorkingArea.Right - Width;
                Top = Screen.PrimaryScreen.WorkingArea.Bottom - Height;
                ShowInTaskbar = true;
                isFormClosed = false;
            }
            else if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
                Left = Screen.PrimaryScreen.WorkingArea.Right - Width;
                Top = Screen.PrimaryScreen.WorkingArea.Bottom - Height;
                ShowInTaskbar = true;
            }
        }

        private void SystemTrayExitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AddTrackingApp(TrackingApp newApp)
        {
            trackingApps.Add(newApp);
            PopulateListView(trackingApps);
            SaveTrackingAppsConfiguration();
        }

        private void UpdateTrackingApp(TrackingApp updatedApp)
        {
            ListViewItem itemToUpdate = null;
            int indexToUpdate = -1;
            foreach (ListViewItem item in listViewTrackingApps.Items)
            {
                TrackingApp app = item.Tag as TrackingApp;
                if (app != null && app.ProcessToTrack == updatedApp.ProcessToTrack)
                {
                    itemToUpdate = item;
                    indexToUpdate = item.Index;
                    break;
                }
            }

            if (itemToUpdate != null && indexToUpdate != -1)
            {
                itemToUpdate.SubItems[0].Text = updatedApp.ProcessToTrack;
                itemToUpdate.SubItems[1].Text = updatedApp.Source;
                itemToUpdate.SubItems[2].Text = updatedApp.Destination;

                TrackingApp oldApp = itemToUpdate.Tag as TrackingApp;
                string oldName = oldApp.ProcessToTrack;

                trackingApps[indexToUpdate].Source = updatedApp.Source;
                trackingApps[indexToUpdate].Destination = updatedApp.Destination;

                processStartedFlags.Remove(oldName);
                processStartedFlags[updatedApp.ProcessToTrack] = false;

                processTracker.UpdateTrackedProcess(indexToUpdate, updatedApp.ProcessToTrack);

                itemToUpdate.Tag = updatedApp;

                SaveTrackingAppsConfiguration();
            }
        }

        private void SaveTrackingAppsConfiguration()
        {
            string jsonContent = JsonConvert.SerializeObject(trackingApps, Formatting.Indented);
            File.WriteAllText(trackingAppsFilePath, jsonContent);
        }

        private void OnProcessStarted(object sender, TrackingApp trackedApp)
        {
            processStartedFlags[trackedApp.ProcessToTrack] = true;
            string sourcePath = trackedApp.Source;
            string initialChecksum = backupManager.CalculateSHA256Checksum(sourcePath);
            initialChecksums[sourcePath] = initialChecksum;
        }

        private void OnProcessStopped(object sender, TrackingApp trackedApp)
        {
            string sourcePath = trackedApp.Source;
            string newChecksum = backupManager.CalculateSHA256Checksum(sourcePath);

            if (!initialChecksums.ContainsKey(sourcePath))
            {
                return;
            }

            string initialChecksum = initialChecksums[sourcePath];

            if (!initialChecksum.Equals(newChecksum))
            {
                DialogResult result = MessageBox.Show(
                    $"A change has occurred in the source for '{trackedApp.ProcessToTrack}'. Do you want to create a backup?",
                    "OpenSync",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    backupManager.PerformBackup(trackedApp);
                    MessageBox.Show("Backup created successfully.", "Backup Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RefreshTrackingAppsFromJSON()
        {
            try
            {
                List<TrackingApp> newTrackingApps = LoadTrackingAppsConfiguration(trackingAppsFilePath);
                var addedTrackingApps = newTrackingApps.Except(trackingApps, new TrackingAppEqualityComparer()).ToList();

                if (addedTrackingApps.Count > 0)
                {
                    foreach (var app in addedTrackingApps)
                    {
                        processStartedFlags[app.ProcessToTrack] = false;
                    }

                    trackingApps.AddRange(addedTrackingApps);
                    PopulateListView(trackingApps);
                }
            }
            catch (FileNotFoundException ex)
            {
                ShowConfigurationErrorMessageBox("File Not Found", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ShowConfigurationErrorMessageBox("Configuration Error", ex.Message);
            }
        }

        private class TrackingAppEqualityComparer : IEqualityComparer<TrackingApp>
        {
            public bool Equals(TrackingApp x, TrackingApp y)
            {
                return x.ProcessToTrack == y.ProcessToTrack &&
                       x.Source == y.Source &&
                       x.Destination == y.Destination;
            }

            public int GetHashCode(TrackingApp obj)
            {
                return obj.ProcessToTrack.GetHashCode() ^
                       obj.Source.GetHashCode() ^
                       obj.Destination.GetHashCode();
            }
        }

        private void ShowConfigurationErrorMessageBox(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
