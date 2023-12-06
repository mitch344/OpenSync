using System.Diagnostics;

namespace OpenSync
{
    partial class ShowBackupsForm : Form
    {
        private TrackingApp trackingApp;
        private BackupManager backupManager;
        private ListView listViewBackups;
        private ContextMenuStrip contextMenu;

        public ShowBackupsForm(TrackingApp trackingApp)
        {
            Text = $"Backups for {trackingApp.ProcessToTrack}";
            Width = 500;
            Height = 300;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            CenterToParent();
            this.trackingApp = trackingApp;
            backupManager = new BackupManager();
            InitializeView();
            PopulateListView();
            InitializeContextMenu();
        }

        private void InitializeView()
        {
            listViewBackups = new ListView
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(Width - 30, Height - 100),
                View = View.Details,
                FullRowSelect = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                MultiSelect = false
            };

            listViewBackups.DoubleClick += ListViewBackupsDoubleClick;
            int backupColumnWidth = listViewBackups.Width - 10;
            listViewBackups.Columns.Add("Backup", backupColumnWidth, HorizontalAlignment.Left);

            Controls.Add(listViewBackups);
        }

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();
            ToolStripMenuItem showSourceMenuItem = new ToolStripMenuItem("Open in File Explorer");
            showSourceMenuItem.Click += ShowSourceMenuItemClick;
            contextMenu.Items.Add(showSourceMenuItem);


            string folder = Path.Combine(Application.StartupPath, "Icons", "folder_image.ico");
            if (File.Exists(folder))
            {
                Icon icon = new Icon(folder);
                showSourceMenuItem.Image = icon.ToBitmap();
            }


            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("Delete");
            deleteMenuItem.Click += DeleteMenuItemClick;
            contextMenu.Items.Add(deleteMenuItem);

            string deleteIcon = Path.Combine(Application.StartupPath, "Icons", "delete_image.ico");
            if (File.Exists(folder))
            {
                Icon icon = new Icon(deleteIcon);
                deleteMenuItem.Image = icon.ToBitmap();
            }


            listViewBackups.ContextMenuStrip = contextMenu;
        }

        private void ShowSourceMenuItemClick(object sender, EventArgs e)
        {
            if (listViewBackups.SelectedItems.Count == 1)
            {
                string selectedBackupFolderName = listViewBackups.SelectedItems[0].Tag as string;

                if (!string.IsNullOrEmpty(selectedBackupFolderName))
                {
                    try
                    {
                        string fullPath = Path.Combine(trackingApp.Destination, trackingApp.ProcessToTrack, selectedBackupFolderName);
                        Process.Start("explorer.exe", fullPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening folder: {ex.Message}", "Open Folder Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void PopulateListView()
        {
            List<string> backups = backupManager.GetBackupsForProcess(trackingApp);
            List<ListViewItem> listViewItems = new List<ListViewItem>();

            foreach (string backupFolderName in backups)
            {
                DateTime? backupDateTime = BackupManager.ExtractDateTimeFromFolderName(backupFolderName);

                if (backupDateTime.HasValue)
                {
                    ListViewItem item = new ListViewItem(backupDateTime.Value.ToString("yyyy-MM-dd hh:mm:ss tt"));
                    item.Tag = backupFolderName;
                    listViewItems.Add(item);
                }
            }

            listViewItems.Sort((item1, item2) =>
            {
                DateTime date1 = DateTime.Parse(item1.Text);
                DateTime date2 = DateTime.Parse(item2.Text);
                return date2.CompareTo(date1);
            });

            listViewBackups.Items.AddRange(listViewItems.ToArray());
        }


        private void ListViewBackupsDoubleClick(object sender, EventArgs e)
        {
            if (listViewBackups.SelectedItems.Count > 0)
            {
                string selectedBackupFolderName = listViewBackups.SelectedItems[0].Tag as string;

                if (!string.IsNullOrEmpty(selectedBackupFolderName))
                {
                    DateTime? backupDateTime = BackupManager.ExtractDateTimeFromFolderName(selectedBackupFolderName);

                    if (backupDateTime.HasValue)
                    {
                        string timestamp = backupDateTime.Value.ToString("yyyy-MM-dd hh:mm:ss tt");
                        string message = $"Do you want to restore the backup from {timestamp} to '{trackingApp.Source}'?";
                        DialogResult result = MessageBox.Show(message, "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            try
                            {
                                backupManager.RestoreBackupForProcess(trackingApp, selectedBackupFolderName);
                                MessageBox.Show("Restore completed successfully.", "Restore Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Restore failed: {ex.Message}", "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void DeleteMenuItemClick(object sender, EventArgs e)
        {
            if (listViewBackups.SelectedItems.Count > 0)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete the selected backup(s)?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    foreach (ListViewItem selectedItem in listViewBackups.SelectedItems)
                    {
                        string selectedBackupFolderName = selectedItem.Tag as string;

                        if (!string.IsNullOrEmpty(selectedBackupFolderName))
                        {
                            try
                            {
                                backupManager.DeleteBackup(trackingApp, selectedBackupFolderName);
                                listViewBackups.Items.Remove(selectedItem);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Delete failed: {ex.Message}", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select one item to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
