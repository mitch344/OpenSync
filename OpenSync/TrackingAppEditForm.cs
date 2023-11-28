using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenSync
{
    internal class TrackingAppEditForm : Form
    {
        private TrackingApp trackingApp;
        public delegate void UpdateTrackingAppDelegate(TrackingApp updatedApp);
        private UpdateTrackingAppDelegate updateCallback;
        private Label labelProcessToTrack;
        private Label labelSource;
        private Label labelDestination;
        private TextBox textBoxProcessToTrack;
        private TextBox textBoxSource;
        private TextBox textBoxDestination;
        private Button selectProcessButton;
        private Button buttonSourceFolder;
        private Button buttonSourceFile;
        private Button buttonDestinationFolder;
        private Button buttonSave;

        public TrackingAppEditForm(TrackingApp trackingApp, UpdateTrackingAppDelegate updateCallback)
        {
            this.trackingApp = trackingApp;
            this.updateCallback = updateCallback;
            InitializeTrackingAppEditForm();
            CenterToParent();
        }

        private void InitializeTrackingAppEditForm()
        {
            Text = "Edit Tracking App";
            Size = new Size(500, 275);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            labelProcessToTrack = new Label
            {
                Text = "Process to Track:",
                Location = new Point(20, 20),
                AutoSize = true
            };

            labelSource = new Label
            {
                Text = "Source:",
                Location = new Point(20, 60),
                AutoSize = true
            };

            labelDestination = new Label
            {
                Text = "Destination:",
                Location = new Point(20, 100),
                AutoSize = true
            };

            textBoxProcessToTrack = new TextBox
            {
                Text = trackingApp.ProcessToTrack,
                Location = new Point(160, 20),
                Size = new Size(300, 30)
            };

            textBoxSource = new TextBox
            {
                Text = trackingApp.Source,
                Location = new Point(160, 60),
                Size = new Size(300, 30)
            };

            textBoxDestination = new TextBox
            {
                Text = trackingApp.Destination,
                Location = new Point(160, 100),
                Size = new Size(300, 30)
            };

            selectProcessButton = new Button
            {
                Text = "Select Process",
                Location = new Point(20, 140),
                Size = new Size(120, 30)
            };

            buttonSourceFolder = new Button
            {
                Text = "Folder",
                Location = new Point(160, 140),
                AutoSize = true
            };

            buttonSourceFile = new Button
            {
                Text = "File",
                Location = new Point(230, 140),
                AutoSize = true
            };

            buttonDestinationFolder = new Button
            {
                Text = "Destination",
                Location = new Point(320, 140),
                AutoSize = true
            };

            buttonSave = new Button
            {
                Text = "Save",
                Location = new Point(160, 180),
                AutoSize = true
            };

            selectProcessButton.Click += SelectButtonClick;
            buttonSave.Click += ButtonSave_Click;
            buttonSourceFolder.Click += (sender, e) => { OpenFolderDialog(textBoxSource); };
            buttonSourceFile.Click += (sender, e) => { OpenFileDialog(textBoxSource); };
            buttonDestinationFolder.Click += (sender, e) => { OpenFolderDialog(textBoxDestination); };

            Controls.Add(labelProcessToTrack);
            Controls.Add(labelSource);
            Controls.Add(labelDestination);
            Controls.Add(textBoxProcessToTrack);
            Controls.Add(textBoxSource);
            Controls.Add(textBoxDestination);
            Controls.Add(selectProcessButton);
            Controls.Add(buttonSourceFolder);
            Controls.Add(buttonSourceFile);
            Controls.Add(buttonDestinationFolder);
            Controls.Add(buttonSave);
        }

        private void OpenFolderDialog(TextBox targetTextBox)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    targetTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void OpenFileDialog(TextBox targetTextBox)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "All files (*.*)|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    targetTextBox.Text = dialog.FileName;
                }
            }
        }

        private void ProcessSelected(string selectedProcess)
        {
            textBoxProcessToTrack.Text = selectedProcess;
        }

        private void SelectButtonClick(object sender, EventArgs e)
        {
            using (var processWindow = new ProcessWindow())
            {
                processWindow.ProcessSelected += ProcessSelected;
                processWindow.Icon = this.Icon;
                processWindow.ShowDialog();
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            string processToTrack = textBoxProcessToTrack.Text.Trim();
            string sourcePathInput = textBoxSource.Text.Trim();
            string destinationPathInput = textBoxDestination.Text.Trim();

            string sourcePathResolved = ResolvePath(sourcePathInput);
            string destinationPathResolved = ResolvePath(destinationPathInput);

            if (IsPathValid(sourcePathResolved))
            {
                if (IsPathValidAsDirectory(destinationPathResolved))
                {
                    trackingApp.Source = sourcePathInput;
                    trackingApp.Destination = destinationPathInput;
                    trackingApp.ProcessToTrack = processToTrack;
                    updateCallback?.Invoke(trackingApp);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid destination path. It should be a directory.");
                }
            }
            else
            {
                MessageBox.Show("Invalid source path. It should be a directory or a file.");
            }
        }

        private bool IsPathValid(string path)
        {
            return System.IO.Directory.Exists(path) || System.IO.File.Exists(path);
        }

        private bool IsPathValidAsDirectory(string path)
        {
            return System.IO.Directory.Exists(path);
        }

        private string ResolvePath(string path)
        {
            return System.Environment.ExpandEnvironmentVariables(path);
        }
    }
}
