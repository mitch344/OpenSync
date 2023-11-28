using System.Diagnostics;

namespace OpenSync
{
    public partial class ProcessWindow : Form
    {
        private List<string> allProcessNames;
        public event Action<string> ProcessSelected;

        private TableLayoutPanel tableLayoutPanel;
        private TextBox searchBox;
        private Button searchButton;
        private Button refreshButton;
        private ListBox listBox;

        public ProcessWindow(Action<string> onProcessSelected = null)
        {
            InitializeProcessWindow();
            CenterToParent();
            RefreshProcessList();
            ProcessSelected += onProcessSelected;
        }

        private void InitializeProcessWindow()
        {
            Text = "ProcessWindow";
            Width = 400;
            Height = 450;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            InitializeTableLayoutPanel();
            InitializeSearchBox();
            InitializeSearchButton();
            InitializeRefreshButton();
            InitializeListBox();
        }

        private void InitializeTableLayoutPanel()
        {
            tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 50),
                    new ColumnStyle(SizeType.Percent, 50)
                },
                RowStyles =
                {
                    new RowStyle(SizeType.Percent, 10),
                    new RowStyle(SizeType.Percent, 10),
                    new RowStyle(SizeType.Percent, 70),
                }
            };
            Controls.Add(tableLayoutPanel);
        }

        private void InitializeSearchBox()
        {
            searchBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5)
            };
            tableLayoutPanel.Controls.Add(searchBox, 0, 0);
            tableLayoutPanel.SetColumnSpan(searchBox, 2);
        }

        private void InitializeSearchButton()
        {
            searchButton = new Button
            {
                Text = "Search",
                Dock = DockStyle.Fill,
                Margin = new Padding(5)
            };
            searchButton.Click += SearchButtonClick;
            tableLayoutPanel.Controls.Add(searchButton, 1, 0);
        }

        private void InitializeRefreshButton()
        {
            refreshButton = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Fill,
                Margin = new Padding(5)
            };
            refreshButton.Click += RefreshButtonClick;
            tableLayoutPanel.Controls.Add(refreshButton, 0, 1);
        }

        private void InitializeListBox()
        {
            listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                SelectionMode = SelectionMode.One,
            };
            listBox.DoubleClick += ListBoxDoubleClick;
            tableLayoutPanel.Controls.Add(listBox, 0, 2);
            tableLayoutPanel.SetColumnSpan(listBox, 2);
        }

        private void UpdateListBox(List<string> items = null)
        {
            listBox.Items.Clear();
            if (items != null && items.Any())
            {
                listBox.Items.AddRange(items.ToArray());
            }
            else if (allProcessNames != null)
            {
                listBox.Items.AddRange(allProcessNames.ToArray());
            }
        }

        private void RefreshProcessList()
        {
            Process[] processes = Process.GetProcesses();
            allProcessNames = processes.Select(process => process.ProcessName)
                                       .Distinct(StringComparer.OrdinalIgnoreCase)
                                       .ToList();
            UpdateListBox();
        }

        private void ListBoxDoubleClick(object sender, EventArgs e)
        {
            if (listBox.SelectedItem is string selectedProcess)
            {
                ProcessSelected?.Invoke(selectedProcess);
                Close();
            }
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            RefreshProcessList();
        }

        private void SearchButtonClick(object sender, EventArgs e)
        {
            RefreshProcessList();
            string searchTerm = searchBox.Text.Trim();
            var filteredProcessNames = allProcessNames
                .Where(name => name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            UpdateListBox(filteredProcessNames.Count > 0 ? filteredProcessNames : null);
        }
    }
}
