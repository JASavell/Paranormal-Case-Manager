// INSTRUCTIONS FOR USE:
// 1. Create a NEW C# Windows Forms App project in Visual Studio
// 2. Delete Form1.cs, Form1.Designer.cs, and Form1.resx from the project
// 3. Add a new Class file called "ParanormalApp.cs"
// 4. Replace ALL contents of that file with this code
// 5. Build and Run

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.VisualBasic;
using iText = iTextSharp.text;
using iTextPdf = iTextSharp.text.pdf;
using System.Text;

namespace ParanormalInvestigator
{
    // Data Models
    [Serializable]
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string AlternatePhone { get; set; }           // NEW
        public string Email { get; set; }
        public string LocationName { get; set; }             // NEW
        public string LocationType { get; set; }
        public string OwnerStatus { get; set; }              // NEW ("Own","Rent","Other")
        public int OccupantsCount { get; set; }              // NEW
        public DateTime DateAdded { get; set; }
        public string ClientPhotoPath { get; set; }
        public string LocationPhotoPath { get; set; }
        public List<PhenomenonLog> Logs { get; set; } = new List<PhenomenonLog>();
        public List<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
        public List<Report> Reports { get; set; } = new List<Report>();
        public List<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
        public List<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
        public List<ParanormalEvent> ParanormalEvents { get; set; } = new List<ParanormalEvent>();
       
    }

    [Serializable]
    public class Report
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }

    [Serializable]
    public class TodoItem
    {
        public int Id { get; set; }
        public string Task { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime DateAdded { get; set; }
    }

    [Serializable]
    public class CalendarEvent
    {
        public int Id { get; set; }
        public DateTime EventDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    [Serializable]
    public class ParanormalEvent
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int ExpectedAttendees { get; set; }
        public decimal Budget { get; set; }
        public string Notes { get; set; }
        public DateTime DateCreated { get; set; }
    }

    [Serializable]
    public class PhenomenonLog
    {
        public int Id { get; set; }
        public DateTime DateTimeOccurred { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Witnesses { get; set; }
    }

    [Serializable]
    public class MediaFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string MediaCategory { get; set; } // "Image", "Video", "Audio", "Note"
        public DateTime DateAdded { get; set; }
        public string Description { get; set; }
        public string NoteContent { get; set; } // For text notes
        public string NoteImagePath { get; set; } // For images in notes
        public string VideoThumbnailPath { get; set; } // Cached video thumbnail
    }

    public class MainForm : Form
    {
        private List<Client> clients = new List<Client>();
        private Client selectedClient;
        private int nextClientId = 1;
        private int nextLogId = 1;
        private int nextMediaId = 1;
        private int nextReportId = 1;
        private int nextTodoId = 1;
        private int nextCalendarEventId = 1;
        private int nextParanormalEventId = 1;
        private string dataFilePath = "paranormal_data.xml";

        private TabControl tabControl;
        private ListBox clientListBox;
        private TextBox txtName, txtAddress, txtPhone, txtEmail;
        private ComboBox cmbLocationType;
        private DataGridView dgvLogs;
        private RichTextBox rtbClientInfo;
        private PictureBox pbClientPhoto, pbLocationPhoto;
        private TabControl mediaTabControl;
        private FlowLayoutPanel flpImages, flpVideos, flpAudio, flpNotes;
        private RichTextBox rtbReportEditor;
        private ListBox lstTodoItems;
        private MonthCalendar calClientCalendar;
        private TextBox txtTodoItem, txtEventName, txtEventLocation, txtEventDescription;
        private DateTimePicker dtpEventDate;
        private ComboBox cmbEventType;
        private ListBox lstEvents;
        private TextBox txtLocationName, txtAltPhone;
        private ComboBox cmbOwnerStatus;
        private NumericUpDown nudOccupants;
        private Button btnExportIcs;
        private Button btnOpenGoogle;

        public MainForm()
        {
            InitializeComponents();
            LoadData();
            RefreshClientList();
        }

        private void InitializeComponents()
        {
            this.Text = "Paranormal Investigation Manager";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Set icon
            if (File.Exists("myicon.ico"))
            {
                try
                {
                    this.Icon = new Icon("myicon.ico");
                }
                catch { }
            }

            // Create main tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Tab 1: Client Management
            TabPage clientTab = new TabPage("Clients");
            clientTab.Name = "Clients";
            CreateClientTab(clientTab);
            tabControl.TabPages.Add(clientTab);

            // Tab 2: Phenomenon Logs
            TabPage logsTab = new TabPage("Phenomenon Logs");
            logsTab.Name = "Phenomenon Logs";
            CreateLogsTab(logsTab);
            tabControl.TabPages.Add(logsTab);

            // Tab 3: Media Files
            TabPage mediaTab = new TabPage("Media Files");
            mediaTab.Name = "Media Files";
            CreateMediaTab(mediaTab);
            tabControl.TabPages.Add(mediaTab);

            // Tab 4: Notes
            TabPage notesTab = new TabPage("Notes");
            notesTab.Name = "Notes";
            CreateNotesTab(notesTab);
            tabControl.TabPages.Add(notesTab);

            // Tab 5: Reports
            TabPage reportsTab = new TabPage("Reports");
            reportsTab.Name = "Reports";
            CreateReportsTab(reportsTab);
            tabControl.TabPages.Add(reportsTab);

            // Tab 6: To-Do & Calendar
            TabPage todoTab = new TabPage("To-Do & Calendar");
            todoTab.Name = "To-Do & Calendar";
            CreateTodoCalendarTab(todoTab);
            tabControl.TabPages.Add(todoTab);

            // Tab 7: Event Planning
            TabPage eventsTab = new TabPage("Event Planning");
            eventsTab.Name = "Event Planning";
            CreateEventPlanningTab(eventsTab);
            tabControl.TabPages.Add(eventsTab);

            this.Controls.Add(tabControl);
        }

        private void CreateClientTab(TabPage tab)
        {
            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill
            };

            // Set left panel to 25% of the tab width
            tab.Resize += (s, e) =>
            {
                split.SplitterDistance = (int)(tab.ClientSize.Width * 0.25);
            };

            // Left panel - Client list
            Panel leftPanel = new Panel { Dock = DockStyle.Fill };
            Label lblClients = new Label
            {
                Text = "Clients",
                Dock = DockStyle.Top,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5)
            };

            // Add search bar
            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                Padding = new Padding(5)
            };

            TextBox txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10),
                // PlaceholderText = "Search clients..." // <-- Remove this line
            };

            // Add this workaround for placeholder text:
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Text = "Search clients...";
            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == "Search clients...")
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };
            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "Search clients...";
                    txtSearch.ForeColor = Color.Gray;
                }
            };

            txtSearch.TextChanged += (s, e) =>
            {
                FilterClientList(txtSearch.Text);
            };

            searchPanel.Controls.Add(txtSearch);

            clientListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10)
            };
            clientListBox.SelectedIndexChanged += ClientListBox_SelectedIndexChanged;

            Button btnAddClient = new Button
            {
                Text = "Add New Client",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font("Arial", 10)
            };
            btnAddClient.Click += BtnAddClient_Click;

            leftPanel.Controls.Add(clientListBox);
            leftPanel.Controls.Add(searchPanel);
            leftPanel.Controls.Add(lblClients);
            leftPanel.Controls.Add(btnAddClient);
            split.Panel1.Controls.Add(leftPanel);

            // Right panel - Client details with equal three columns
            Panel rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

            // Column 1 - Client Information
            Panel column1 = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(5) };

            Label lblClientInfo = new Label { Text = "Client Information", Location = new Point(10, 10), Width = 200, Font = new Font("Arial", 11, FontStyle.Bold), AutoSize = false };

            Label lblName = new Label { Text = "Name:", Location = new Point(10, 40), Width = 100, Height = 20, AutoSize = false };
            txtName = new TextBox { Location = new Point(10, 60), Width = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            Label lblAddress = new Label { Text = "Address:", Location = new Point(10, 90), Width = 100, Height = 20, AutoSize = false };
            txtAddress = new TextBox { Location = new Point(10, 110), Width = 200, Height = 60, Multiline = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            Label lblPhone = new Label { Text = "Phone:", Location = new Point(10, 180), Width = 100, Height = 20, AutoSize = false };
            txtPhone = new TextBox { Location = new Point(10, 200), Width = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            Label lblEmail = new Label { Text = "Email:", Location = new Point(10, 230), Width = 100, Height = 20, AutoSize = false };
            txtEmail = new TextBox { Location = new Point(10, 250), Width = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            Label lblLocationType = new Label { Text = "Location Type:", Location = new Point(10, 280), Width = 100, Height = 20, AutoSize = false };
            cmbLocationType = new ComboBox
            {
                Location = new Point(10, 300),
                Width = 176,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            cmbLocationType.Items.AddRange(new object[] {
                "Residential - House",
                "Residential - Apartment",
                "Commercial Building",
                "Historical Site",
                "Cemetery",
                "Hospital/Medical",
                "School/University",
                "Hotel/Inn",
                "Church/Religious",
                "Abandoned Building",
                "Outdoor Location",
                "Other"
            });

            // NEW: Location Name
            Label lblLocationName = new Label { Text = "Location Name:", Location = new Point(10, 335), Width = 100, Height = 18, AutoSize = false };
            txtLocationName = new TextBox { Location = new Point(10, 355), Width = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            // NEW: Alternate Phone
            Label lblAltPhone = new Label { Text = "Alt Phone:", Location = new Point(10, 385), Width = 100, Height = 18, AutoSize = false };
            txtAltPhone = new TextBox { Location = new Point(10, 405), Width = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            // NEW: Owner Status (dropdown)
            Label lblOwnerStatus = new Label { Text = "Owner Status:", Location = new Point(10, 435), Width = 100, Height = 18, AutoSize = false };
            cmbOwnerStatus = new ComboBox { Location = new Point(10, 455), Width = 176, DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            cmbOwnerStatus.Items.AddRange(new object[] { "Own", "Rent", "Other" });

            // NEW: Number of individuals living/staying
            Label lblOccupants = new Label { Text = "Occupants:", Location = new Point(10, 485), Width = 100, Height = 18, AutoSize = false };
            nudOccupants = new NumericUpDown { Location = new Point(10, 505), Width = 80, Minimum = 0, Maximum = 1000, Value = 1, Anchor = AnchorStyles.Top | AnchorStyles.Left };

            // Move Save/Delete buttons down so they don't overlap new controls
            Button btnSaveClient = new Button
            {
                Text = "Save Client",
                Location = new Point(10, 545), // moved down
                Width = 95,
                Height = 35,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnSaveClient.Click += BtnSaveClient_Click;

            Button btnDeleteClient = new Button
            {
                Text = "Delete Client",
                Location = new Point(115, 545), // moved down
                Width = 95,
                Height = 35,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnDeleteClient.Click += BtnDeleteClient_Click;

            Button btnOpenMaps = new Button
            {
                Text = "Open in Maps",
                Location = new Point(10, 585),
                Width = 200,
                Height = 35,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnOpenMaps.Click += BtnOpenInMaps_Click;

            column1.Controls.AddRange(new Control[] {
                lblClientInfo, lblName, txtName, lblAddress, txtAddress, lblPhone, txtPhone,
                lblEmail, txtEmail, lblLocationType, cmbLocationType,
                // new controls
                lblLocationName, txtLocationName, lblAltPhone, txtAltPhone,
                lblOwnerStatus, cmbOwnerStatus, lblOccupants, nudOccupants,
                // buttons
                btnSaveClient, btnDeleteClient, btnOpenMaps
            });

            // Column 2 - Client Photo
            Panel column2 = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(5) };

            Label lblClientPhotoTitle = new Label { Text = "Client Photo", Location = new Point(10, 10), Width = 200, Font = new Font("Arial", 11, FontStyle.Bold), AutoSize = false };

            pbClientPhoto = new PictureBox
            {
                Location = new Point(10, 40),
                Width = 200,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Button btnAddClientPhoto = new Button
            {
                Text = "Add Photo",
                Location = new Point(10, 250),
                Width = 95,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnAddClientPhoto.Click += BtnAddClientPhoto_Click;

            Button btnRemoveClientPhoto = new Button
            {
                Text = "Remove",
                Location = new Point(115, 250),
                Width = 95,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnRemoveClientPhoto.Click += BtnRemoveClientPhoto_Click;

            column2.Controls.AddRange(new Control[] {
                lblClientPhotoTitle, pbClientPhoto, btnAddClientPhoto, btnRemoveClientPhoto
            });

            // Column 3 - Location Photo
            Panel column3 = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(5) };

            Label lblLocationPhotoTitle = new Label { Text = "Location Photo", Location = new Point(10, 10), Width = 200, Font = new Font("Arial", 11, FontStyle.Bold), AutoSize = false };

            pbLocationPhoto = new PictureBox
            {
                Location = new Point(10, 40),
                Width = 200,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Button btnAddLocationPhoto = new Button
            {
                Text = "Add Photo",
                Location = new Point(10, 250),
                Width = 95,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnAddLocationPhoto.Click += BtnAddLocationPhoto_Click;

            Button btnRemoveLocationPhoto = new Button
            {
                Text = "Remove",
                Location = new Point(115, 250),
                Width = 95,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnRemoveLocationPhoto.Click += BtnRemoveLocationPhoto_Click;

            rtbClientInfo = new RichTextBox
            {
                // Dock to the bottom of the right column so resizing never lets the border go past the panel edge
                Dock = DockStyle.Bottom,
                Height = 400,
                ReadOnly = true,
                Font = new Font("Arial", 9),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10)
            };

            column3.Controls.AddRange(new Control[] {
                lblLocationPhotoTitle, pbLocationPhoto, btnAddLocationPhoto, btnRemoveLocationPhoto, rtbClientInfo
            });

            mainLayout.Controls.Add(column1, 0, 0);
            mainLayout.Controls.Add(column2, 1, 0);
            mainLayout.Controls.Add(column3, 2, 0);

            rightPanel.Controls.Add(mainLayout);
            split.Panel2.Controls.Add(rightPanel);
            tab.Controls.Add(split);
        }

        private void CreateLogsTab(TabPage tab)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Label lblSelectClient = new Label
            {
                Text = "Select a client from the Clients tab to view and add phenomenon logs",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 10, FontStyle.Italic)
            };

            dgvLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            Button btnAddLog = new Button
            {
                Text = "Add Phenomenon Log",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font("Arial", 10)
            };
            btnAddLog.Click += BtnAddLog_Click;

            Button btnDeleteLog = new Button
            {
                Text = "Delete Selected Log",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font("Arial", 10)
            };
            btnDeleteLog.Click += BtnDeleteLog_Click;

            panel.Controls.Add(dgvLogs);
            panel.Controls.Add(btnDeleteLog);
            panel.Controls.Add(btnAddLog);
            panel.Controls.Add(lblSelectClient);
            tab.Controls.Add(panel);
        }

        private void CreateMediaTab(TabPage tab)
        {
            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Label lblSelectClient = new Label
            {
                Text = "Select a client from the Clients tab to view and add media files",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 10, FontStyle.Italic)
            };

            // Create tab control for media categories (without Notes)
            mediaTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Images Tab
            TabPage imagesTab = new TabPage("Images");
            flpImages = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            imagesTab.Controls.Add(flpImages);
            mediaTabControl.TabPages.Add(imagesTab);

            // Videos Tab
            TabPage videosTab = new TabPage("Videos");
            flpVideos = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            videosTab.Controls.Add(flpVideos);
            mediaTabControl.TabPages.Add(videosTab);

            // Audio Tab
            TabPage audioTab = new TabPage("Audio");
            flpAudio = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            audioTab.Controls.Add(flpAudio);
            mediaTabControl.TabPages.Add(audioTab);

            // Button panel
            Panel buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };

            Button btnAddImage = new Button
            {
                Text = "Add Image",
                Location = new Point(10, 10),
                Width = 120,
                Height = 35
            };
            btnAddImage.Click += BtnAddImage_Click;

            Button btnAddVideo = new Button
            {
                Text = "Add Video",
                Location = new Point(140, 10),
                Width = 120,
                Height = 35
            };
            btnAddVideo.Click += BtnAddVideo_Click;

            Button btnAddAudio = new Button
            {
                Text = "Add Audio",
                Location = new Point(270, 10),
                Width = 120,
                Height = 35
            };
            btnAddAudio.Click += BtnAddAudio_Click;

            buttonPanel.Controls.AddRange(new Control[] { btnAddImage, btnAddVideo, btnAddAudio });

            mainPanel.Controls.Add(mediaTabControl);
            mainPanel.Controls.Add(buttonPanel);
            mainPanel.Controls.Add(lblSelectClient);
            tab.Controls.Add(mainPanel);
        }

        private void CreateNotesTab(TabPage tab)
        {
            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Label lblSelectClient = new Label
            {
                Text = "Select a client from the Clients tab to view and add notes",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 10, FontStyle.Italic)
            };

            flpNotes = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            Button btnAddNote = new Button
            {
                Text = "Add Note",
                Dock = DockStyle.Bottom,
                Height = 50,
                Font = new Font("Arial", 10)
            };
            btnAddNote.Click += BtnAddNote_Click;

            mainPanel.Controls.Add(flpNotes);
            mainPanel.Controls.Add(btnAddNote);
            mainPanel.Controls.Add(lblSelectClient);
            tab.Controls.Add(mainPanel);
        }

        private void CreateReportsTab(TabPage tab)
        {
            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Label lblSelectClient = new Label
            {
                Text = "Select a client from the Clients tab to write and manage reports",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 10, FontStyle.Italic)
            };

            // Report list on the left
            Panel leftPanel = new Panel { Dock = DockStyle.Left, Width = 250, Padding = new Padding(5) };

            Label lblReports = new Label
            {
                Text = "Reports:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            ListBox lstReports = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 9)
            };

            Button btnNewReport = new Button
            {
                Text = "New Report",
                Dock = DockStyle.Bottom,
                Height = 35
            };

            leftPanel.Controls.Add(lstReports);
            leftPanel.Controls.Add(btnNewReport);
            leftPanel.Controls.Add(lblReports);

            // Editor on the right
            Panel rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };

            TextBox txtReportTitle = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Text = ""
            };

            Label lblTitleHint = new Label
            {
                Text = "Report Title",
                ForeColor = Color.Gray,
                Font = new Font("Arial", 10, FontStyle.Italic),
                Location = new Point(5, 3),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            txtReportTitle.Controls.Add(lblTitleHint);
            txtReportTitle.TextChanged += (s, e) => lblTitleHint.Visible = string.IsNullOrEmpty(txtReportTitle.Text);
            txtReportTitle.Enter += (s, e) => lblTitleHint.Visible = false;
            txtReportTitle.Leave += (s, e) => lblTitleHint.Visible = string.IsNullOrEmpty(txtReportTitle.Text);

            // Formatting toolbar
            Panel toolbarPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(5) };

            // Font controls
            Label lblFont = new Label { Text = "Font:", Location = new Point(5, 8), Width = 35, AutoSize = false };
            ComboBox cmbFont = new ComboBox { Location = new Point(45, 5), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (FontFamily font in FontFamily.Families)
            {
                cmbFont.Items.Add(font.Name);
            }
            cmbFont.SelectedItem = "Arial";

            Label lblSize = new Label { Text = "Size:", Location = new Point(205, 8), Width = 35, AutoSize = false };
            ComboBox cmbSize = new ComboBox { Location = new Point(245, 5), Width = 60, DropDownStyle = ComboBoxStyle.DropDownList };
            for (int i = 8; i <= 72; i += 2)
            {
                cmbSize.Items.Add(i);
            }
            cmbSize.SelectedItem = 11;

            Button btnBold = new Button { Text = "B", Location = new Point(315, 5), Width = 30, Height = 25, Font = new Font("Arial", 10, FontStyle.Bold) };
            Button btnItalic = new Button { Text = "I", Location = new Point(350, 5), Width = 30, Height = 25, Font = new Font("Arial", 10, FontStyle.Italic) };
            Button btnUnderline = new Button { Text = "U", Location = new Point(385, 5), Width = 30, Height = 25, Font = new Font("Arial", 10, FontStyle.Underline) };

            Button btnBullet = new Button { Text = "• List", Location = new Point(425, 5), Width = 60, Height = 25 };
            Button btnNumber = new Button { Text = "1. List", Location = new Point(490, 5), Width = 60, Height = 25 };

            // Alignment and color
            Button btnLeft = new Button { Text = "⬅", Location = new Point(5, 38), Width = 35, Height = 25 };
            Button btnCenter = new Button { Text = "↔", Location = new Point(45, 38), Width = 35, Height = 25 };
            Button btnRight = new Button { Text = "➡", Location = new Point(85, 38), Width = 35, Height = 25 };

            Button btnColor = new Button { Text = "Color", Location = new Point(130, 38), Width = 60, Height = 25 };
            Button btnHighlight = new Button { Text = "Highlight", Location = new Point(195, 38), Width = 70, Height = 25 };

            // Insert image button
            Button btnInsertImage = new Button { Text = "Insert Image", Location = new Point(270, 38), Width = 100, Height = 25 };

            // Wire up formatting events
            cmbFont.SelectedIndexChanged += (s, e) => ApplyFont(cmbFont.SelectedItem.ToString(), (int)cmbSize.SelectedItem);
            cmbSize.SelectedIndexChanged += (s, e) => ApplyFont(cmbFont.SelectedItem.ToString(), (int)cmbSize.SelectedItem);
            btnBold.Click += (s, e) => ApplyStyle(FontStyle.Bold);
            btnItalic.Click += (s, e) => ApplyStyle(FontStyle.Italic);
            btnUnderline.Click += (s, e) => ApplyStyle(FontStyle.Underline);
            btnBullet.Click += (s, e) => ApplyBullets();
            btnNumber.Click += (s, e) => ApplyNumbering();
            btnLeft.Click += (s, e) => ApplyAlignment(HorizontalAlignment.Left);
            btnCenter.Click += (s, e) => ApplyAlignment(HorizontalAlignment.Center);
            btnRight.Click += (s, e) => ApplyAlignment(HorizontalAlignment.Right);
            btnColor.Click += (s, e) => ApplyTextColor();
            btnHighlight.Click += (s, e) => ApplyHighlight();
            btnInsertImage.Click += (s, e) => InsertImageIntoReport();

            toolbarPanel.Controls.AddRange(new Control[] {
                lblFont, cmbFont, lblSize, cmbSize, btnBold, btnItalic, btnUnderline,
                btnBullet, btnNumber, btnLeft, btnCenter, btnRight, btnColor, btnHighlight, btnInsertImage
            });

            rtbReportEditor = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 11),
                Margin = new Padding(0, 5, 0, 0)
            };

            Panel buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };

            Button btnSaveReport = new Button
            {
                Text = "Save Report",
                Location = new Point(5, 10),
                Width = 100,
                Height = 35
            };

            Button btnPrintReport = new Button
            {
                Text = "Print Report",
                Location = new Point(115, 10),
                Width = 100,
                Height = 35
            };
            btnPrintReport.Click += (s, e) => PrintReport();

            Button btnExportTxt = new Button
            {
                Text = "Export TXT",
                Location = new Point(225, 10),
                Width = 90,
                Height = 35
            };

            Button btnExportPdf = new Button
            {
                Text = "Export PDF",
                Location = new Point(325, 10),
                Width = 90,
                Height = 35
            };

            Button btnDeleteReport = new Button
            {
                Text = "Delete Report",
                Location = new Point(425, 10),
                Width = 100,
                Height = 35
            };

            buttonPanel.Controls.AddRange(new Control[] { btnSaveReport, btnPrintReport, btnExportTxt, btnExportPdf, btnDeleteReport });

            rightPanel.Controls.Add(rtbReportEditor);
            rightPanel.Controls.Add(toolbarPanel);
            rightPanel.Controls.Add(buttonPanel);
            rightPanel.Controls.Add(txtReportTitle);

            mainPanel.Controls.Add(rightPanel);
            mainPanel.Controls.Add(leftPanel);
            mainPanel.Controls.Add(lblSelectClient);
            tab.Controls.Add(mainPanel);

            // Store references
            tab.Tag = new { ReportList = lstReports, TitleBox = txtReportTitle };

            // Wire up events after storing references
            lstReports.SelectedIndexChanged += (s, e) => LoadSelectedReport(lstReports, txtReportTitle);
            btnNewReport.Click += (s, e) => CreateNewReport(lstReports, txtReportTitle);
            btnSaveReport.Click += (s, e) => SaveCurrentReport(txtReportTitle.Text, lstReports);
            btnExportTxt.Click += (s, e) => ExportReport(txtReportTitle.Text);
            btnExportPdf.Click += (s, e) => ExportReportToPdf(txtReportTitle.Text);
            btnDeleteReport.Click += (s, e) => DeleteReport(lstReports, txtReportTitle);
        }

        private void CreateTodoCalendarTab(TabPage tab)
        {
            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Label lblSelectClient = new Label
            {
                Text = "Select a client from the Clients tab to manage to-do items and calendar",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 10, FontStyle.Italic)
            };

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };

            // Left: To-Do List
            Panel todoPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };

            Label lblTodo = new Label
            {
                Text = "To-Do List:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Arial", 11, FontStyle.Bold)
            };

            lstTodoItems = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10),
                SelectionMode = SelectionMode.One
            };
            lstTodoItems.DrawMode = DrawMode.OwnerDrawFixed;
            lstTodoItems.DrawItem += LstTodoItems_DrawItem;
            lstTodoItems.MouseDoubleClick += LstTodoItems_MouseDoubleClick;

            Panel todoInputPanel = new Panel { Dock = DockStyle.Bottom, Height = 80 };

            txtTodoItem = new TextBox
            {
                Location = new Point(5, 10),
                Width = 200,
                Height = 25,
                Text = ""
            };

            Label lblTodoHint = new Label
            {
                Text = "New task...",
                ForeColor = Color.Gray,
                Font = new Font("Arial", 9, FontStyle.Italic),
                Location = new Point(3, 3),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            txtTodoItem.Controls.Add(lblTodoHint);
            txtTodoItem.TextChanged += (s, e) => lblTodoHint.Visible = string.IsNullOrEmpty(txtTodoItem.Text);
            txtTodoItem.Enter += (s, e) => lblTodoHint.Visible = false;
            txtTodoItem.Leave += (s, e) => lblTodoHint.Visible = string.IsNullOrEmpty(txtTodoItem.Text);

            Button btnAddTodo = new Button
            {
                Text = "Add",
                Location = new Point(210, 10),
                Width = 60,
                Height = 25
            };
            btnAddTodo.Click += BtnAddTodo_Click;

            Button btnDeleteTodo = new Button
            {
                Text = "Delete",
                Location = new Point(5, 45),
                Width = 100,
                Height = 25
            };
            btnDeleteTodo.Click += BtnDeleteTodo_Click;

            todoInputPanel.Controls.AddRange(new Control[] { txtTodoItem, btnAddTodo, btnDeleteTodo });

            todoPanel.Controls.Add(lstTodoItems);
            todoPanel.Controls.Add(todoInputPanel);
            todoPanel.Controls.Add(lblTodo);

            // Right: Calendar
            Panel calendarPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };

            Label lblCalendar = new Label
            {
                Text = "Calendar:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Arial", 11, FontStyle.Bold)
            };

            calClientCalendar = new MonthCalendar
            {
                Location = new Point(10, 35),
                MaxSelectionCount = 1
            };

            Label lblEvents = new Label
            {
                Text = "Events on selected date:",
                Location = new Point(10, 220),
                Width = 200,
                Height = 20,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            ListBox lstCalendarEvents = new ListBox
            {
                Location = new Point(10, 245),
                Width = 300,
                Height = 120,
                Font = new Font("Arial", 9)
            };

            TextBox txtEventTitle = new TextBox
            {
                Location = new Point(10, 375),
                Width = 200,
                Height = 25,
                Text = ""
            };

            Label lblEventHint = new Label
            {
                Text = "Event title...",
                ForeColor = Color.Gray,
                Font = new Font("Arial", 9, FontStyle.Italic),
                Location = new Point(3, 3),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            txtEventTitle.Controls.Add(lblEventHint);
            txtEventTitle.TextChanged += (s, e) => lblEventHint.Visible = string.IsNullOrEmpty(txtEventTitle.Text);
            txtEventTitle.Enter += (s, e) => lblEventHint.Visible = false;
            txtEventTitle.Leave += (s, e) => lblEventHint.Visible = string.IsNullOrEmpty(txtEventTitle.Text);

            Button btnAddEvent = new Button
            {
                Text = "Add Event",
                Location = new Point(220, 375),
                Width = 90,
                Height = 25
            };

            Button btnDeleteEvent = new Button
            {
                Text = "Delete Event",
                Location = new Point(10, 410),
                Width = 100,
                Height = 25
            };

            Button btnExportIcsLocal = new Button
            {
                Text = "Export .ics (Apple/Other)",
                Location = new Point(10,445),
                Width = 170,
                Height = 30
            };
            btnExportIcsLocal.Click += (s, e) => ExportSelectedDateToIcs();

            Button btnOpenGoogleLocal = new Button
            {
                Text = "Open in Google Calendar",
                Location = new Point(190,445),
                Width = 170,
                Height = 30
            };
            btnOpenGoogleLocal.Click += (s, e) => OpenSelectedDateInGoogle();

            // store references in fields so other methods can use them
            btnExportIcs = btnExportIcsLocal;
            btnOpenGoogle = btnOpenGoogleLocal;

            calendarPanel.Controls.AddRange(new Control[] {
                lblCalendar, calClientCalendar, lblEvents, lstCalendarEvents,
                txtEventTitle, btnAddEvent, btnDeleteEvent, btnExportIcsLocal, btnOpenGoogleLocal
            });

            split.Panel1.Controls.Add(todoPanel);
            split.Panel2.Controls.Add(calendarPanel);

            mainPanel.Controls.Add(split);
            mainPanel.Controls.Add(lblSelectClient);
            tab.Controls.Add(mainPanel);

            // Store reference and wire up events AFTER controls are created
            tab.Tag = lstCalendarEvents;
            calClientCalendar.DateChanged += CalClientCalendar_DateChanged;
            btnAddEvent.Click += (s, e) => AddCalendarEvent(txtEventTitle, lstCalendarEvents);
            btnDeleteEvent.Click += (s, e) => DeleteCalendarEvent(lstCalendarEvents);
        }

        private void CreateEventPlanningTab(TabPage tab)
        {
            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Label lblTitle = new Label
            {
                Text = "Paranormal Event Planning - Fundraisers, Public Ghost Hunts & Charity Events",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill
            };

            // Set left panel to 25% of the tab width
            tab.Resize += (s, e) =>
            {
                split.SplitterDistance = (int)(tab.ClientSize.Width * 0.5);
            };

            // Left: Event List
            Panel leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };

            Label lblEvents = new Label
            {
                Text = "Planned Events:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lstEvents = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 9)
            };
            lstEvents.SelectedIndexChanged += LstEvents_SelectedIndexChanged;

            Button btnNewEvent = new Button
            {
                Text = "New Event",
                Dock = DockStyle.Bottom,
                Height = 35
            };
            btnNewEvent.Click += BtnNewEvent_Click;

            leftPanel.Controls.Add(lstEvents);
            leftPanel.Controls.Add(btnNewEvent);
            leftPanel.Controls.Add(lblEvents);

            // Right: Event Details
            Panel rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), AutoScroll = true };

            Label lblEventName = new Label { Text = "Event Name:", Location = new Point(10, 15), Width = 120 };
            txtEventName = new TextBox { Location = new Point(140, 12), Width = 300 };

            Label lblEventType = new Label { Text = "Event Type:", Location = new Point(10, 50), Width = 120 };
            cmbEventType = new ComboBox { Location = new Point(140, 47), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEventType.Items.AddRange(new object[] {
                "Fundraiser", "Public Ghost Hunt", "Charity Event",
                "Investigation Event", "Conference", "Workshop", "Other"
            });

            Label lblEventDate = new Label { Text = "Event Date:", Location = new Point(10, 85), Width = 120 };
            dtpEventDate = new DateTimePicker { Location = new Point(140, 82), Width = 300 };

            Label lblEventLocation = new Label { Text = "Location:", Location = new Point(10, 120), Width = 120 };
            txtEventLocation = new TextBox { Location = new Point(140, 117), Width = 300, Height = 60, Multiline = true };

            Label lblAttendees = new Label { Text = "Expected Attendees:", Location = new Point(10, 190), Width = 120 };
            NumericUpDown nudAttendees = new NumericUpDown { Location = new Point(140, 187), Width = 100, Maximum = 10000 };

            Label lblBudget = new Label { Text = "Budget ($):", Location = new Point(10, 225), Width = 120 };
            NumericUpDown nudBudget = new NumericUpDown { Location = new Point(140, 222), Width = 100, Maximum = 1000000, DecimalPlaces = 2 };

            Label lblEventDesc = new Label { Text = "Description:", Location = new Point(10, 260), Width = 120 };
            txtEventDescription = new TextBox { Location = new Point(140, 257), Width = 300, Height = 100, Multiline = true, ScrollBars = ScrollBars.Vertical };

            Label lblNotes = new Label { Text = "Planning Notes:", Location = new Point(10, 370), Width = 120 };
            TextBox txtEventNotes = new TextBox { Location = new Point(140,367), Width = 300, Height = 100, Multiline = true, ScrollBars = ScrollBars.Vertical };

            Button btnSaveEvent = new Button { Text = "Save Event", Location = new Point(140, 480), Width = 100, Height = 35 };
            btnSaveEvent.Click += (s, e) => SaveParanormalEvent(nudAttendees, nudBudget, txtEventNotes);

            Button btnDeleteEvent = new Button { Text = "Delete Event", Location = new Point(250, 480), Width = 100, Height = 35 };
            btnDeleteEvent.Click += BtnDeleteEvent_Click;

            Button btnExportEventIcs = new Button
            {
                Text = "Export Event .ics",
                Location = new Point(10, 520),
                Width = 170,
                Height = 30
            };
            btnExportEventIcs.Click += (s, e) => ExportSelectedParanormalEventToIcs();

            Button btnOpenGoogleEvent = new Button
            {
                Text = "Open Event in Google Calendar",
                Location = new Point(190, 520),
                Width = 260,
                Height = 30
            };
            btnOpenGoogleEvent.Click += (s, e) => OpenSelectedParanormalEventInGoogle();

            rightPanel.Controls.AddRange(new Control[] { btnExportEventIcs, btnOpenGoogleEvent });

            rightPanel.Controls.AddRange(new Control[] {
                lblEventName, txtEventName, lblEventType, cmbEventType, lblEventDate, dtpEventDate,
                lblEventLocation, txtEventLocation, lblAttendees, nudAttendees, lblBudget, nudBudget,
                lblEventDesc, txtEventDescription, lblNotes, txtEventNotes, btnSaveEvent, btnDeleteEvent
            });

            split.Panel1.Controls.Add(leftPanel);
            split.Panel2.Controls.Add(rightPanel);

            mainPanel.Controls.Add(split);
            mainPanel.Controls.Add(lblTitle);
            tab.Controls.Add(mainPanel);

            // Store references
            tab.Tag = new { Attendees = nudAttendees, Budget = nudBudget, Notes = txtEventNotes };
        }

        private void RefreshClientList()
        {
            clientListBox.Items.Clear();
            foreach (var client in clients)
            {
                string locationType = !string.IsNullOrEmpty(client.LocationType) ? client.LocationType : "No Type";
                clientListBox.Items.Add($"{client.Name} - {locationType}");
            }
        }

        private void ClientListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (clientListBox.SelectedIndex >= 0)
            {
                selectedClient = clients[clientListBox.SelectedIndex];
                DisplayClientDetails();
                RefreshLogs();
                RefreshMedia();
                RefreshReports();
                RefreshTodoList();
                RefreshCalendarEvents();
                // Make sure the Event Planning list is refreshed when a client is selected
                RefreshEventList();
            }
            else
            {
                selectedClient = null;
                // Clear dependent views when no client is selected
                RefreshEventList();
                dgvLogs.DataSource = null;
                flpImages.Controls.Clear();
                flpVideos.Controls.Clear();
                flpAudio.Controls.Clear();
                flpNotes.Controls.Clear();
            }
        }

        // 5) Update DisplayClientDetails(...) to show and populate the new controls and include them in the summary:
        private void DisplayClientDetails()
        {
            if (selectedClient != null)
            {
                txtName.Text = selectedClient.Name;
                txtAddress.Text = selectedClient.Address;
                txtPhone.Text = selectedClient.Phone;
                txtAltPhone.Text = selectedClient.AlternatePhone;           // NEW
                txtEmail.Text = selectedClient.Email;

                // Set location name
                txtLocationName.Text = selectedClient.LocationName ?? "";   // NEW

                // Set location type
                if (!string.IsNullOrEmpty(selectedClient.LocationType))
                {
                    cmbLocationType.SelectedItem = selectedClient.LocationType;
                }
                else
                {
                    cmbLocationType.SelectedIndex = -1;
                }

                // Owner status & occupants
                if (!string.IsNullOrEmpty(selectedClient.OwnerStatus))
                {
                    cmbOwnerStatus.SelectedItem = selectedClient.OwnerStatus;
                }
                else
                {
                    cmbOwnerStatus.SelectedIndex = -1;
                }
                nudOccupants.Value = Math.Max(nudOccupants.Minimum, Math.Min(nudOccupants.Maximum, selectedClient.OccupantsCount)); // NEW

                // Load client photo
                if (!string.IsNullOrEmpty(selectedClient.ClientPhotoPath) && File.Exists(selectedClient.ClientPhotoPath))
                {
                    try
                    {
                        pbClientPhoto.Image = Image.FromFile(selectedClient.ClientPhotoPath);
                    }
                    catch
                    {
                        pbClientPhoto.Image = null;
                    }
                }
                else
                {
                    pbClientPhoto.Image = null;
                }

                // Load location photo
                if (!string.IsNullOrEmpty(selectedClient.LocationPhotoPath) && File.Exists(selectedClient.LocationPhotoPath))
                {
                    try
                    {
                        pbLocationPhoto.Image = Image.FromFile(selectedClient.LocationPhotoPath);
                    }
                    catch
                    {
                        pbLocationPhoto.Image = null;
                    }
                }
                else
                {
                    pbLocationPhoto.Image = null;
                }

                rtbClientInfo.Clear();
                rtbClientInfo.AppendText($"Client Summary\n");
                rtbClientInfo.AppendText($"===================\n\n");
                rtbClientInfo.AppendText($"Name: {selectedClient.Name}\n");
                rtbClientInfo.AppendText($"Location Name: {selectedClient.LocationName}\n");    // NEW
                rtbClientInfo.AppendText($"Address: {selectedClient.Address}\n");
                rtbClientInfo.AppendText($"Phone: {selectedClient.Phone}\n");
                if (!string.IsNullOrEmpty(selectedClient.AlternatePhone))
                {
                    rtbClientInfo.AppendText($"Alt Phone: {selectedClient.AlternatePhone}\n");    // NEW
                }
                rtbClientInfo.AppendText($"Email: {selectedClient.Email}\n");
                if (!string.IsNullOrEmpty(selectedClient.LocationType))
                {
                    rtbClientInfo.AppendText($"Location Type: {selectedClient.LocationType}\n");
                }
                if (!string.IsNullOrEmpty(selectedClient.OwnerStatus))
                {
                    rtbClientInfo.AppendText($"Owner Status: {selectedClient.OwnerStatus}\n");    // NEW
                }
                rtbClientInfo.AppendText($"Occupants: {selectedClient.OccupantsCount}\n\n");      // NEW
                rtbClientInfo.AppendText($"Date Added: {selectedClient.DateAdded.ToShortDateString()}\n\n");
                rtbClientInfo.AppendText($"Phenomenon Logs: {selectedClient.Logs.Count}\n");
                rtbClientInfo.AppendText($"Media Files: {selectedClient.MediaFiles.Count}\n");
                rtbClientInfo.AppendText($"Reports: {selectedClient.Reports.Count}\n");
            }
        }

        private void BtnAddClient_Click(object sender, EventArgs e)
        {
            ClearClientFields();
            selectedClient = null;
        }

        private void BtnAddClientPhoto_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Select Client Photo"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pbClientPhoto.Image = Image.FromFile(ofd.FileName);
                    if (selectedClient != null)
                    {
                        selectedClient.ClientPhotoPath = ofd.FileName;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRemoveClientPhoto_Click(object sender, EventArgs e)
        {
            pbClientPhoto.Image = null;
            if (selectedClient != null)
            {
                selectedClient.ClientPhotoPath = null;
            }
        }

        private void BtnAddLocationPhoto_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Select Location Photo"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pbLocationPhoto.Image = Image.FromFile(ofd.FileName);
                    if (selectedClient != null)
                    {
                        selectedClient.LocationPhotoPath = ofd.FileName;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRemoveLocationPhoto_Click(object sender, EventArgs e)
        {
            pbLocationPhoto.Image = null;
            if (selectedClient != null)
            {
                selectedClient.LocationPhotoPath = null;
            }
        }

        private void BtnSaveClient_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a client name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedClient == null)
            {
                selectedClient = new Client
                {
                    Id = nextClientId++,
                    DateAdded = DateTime.Now
                };
                clients.Add(selectedClient);
            }

            selectedClient.Name = txtName.Text;
            selectedClient.Address = txtAddress.Text;
            selectedClient.Phone = txtPhone.Text;
            selectedClient.AlternatePhone = txtAltPhone.Text;                // NEW
            selectedClient.Email = txtEmail.Text;
            selectedClient.LocationName = txtLocationName.Text;              // NEW
            selectedClient.LocationType = cmbLocationType.SelectedItem?.ToString();
            selectedClient.OwnerStatus = cmbOwnerStatus.SelectedItem?.ToString(); // NEW
            selectedClient.OccupantsCount = (int)nudOccupants.Value;         // NEW

            SaveData();
            RefreshClientList();
            DisplayClientDetails();
            MessageBox.Show("Client saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDeleteClient_Click(object sender, EventArgs e)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete {selectedClient.Name}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                clients.Remove(selectedClient);
                selectedClient = null;
                ClearClientFields();
                SaveData();
                RefreshClientList();
            }
        }

        private void BtnAddLog_Click(object sender, EventArgs e)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LogForm logForm = new LogForm();
            if (logForm.ShowDialog() == DialogResult.OK)
            {
                var log = new PhenomenonLog
                {
                    Id = nextLogId++,
                    DateTimeOccurred = logForm.DateTimeOccurred,
                    Type = logForm.PhenomenonType,
                    Description = logForm.Description,
                    Location = logForm.LocationText,
                    Witnesses = logForm.Witnesses
                };
                selectedClient.Logs.Add(log);
                SaveData();
                RefreshLogs();
                DisplayClientDetails();
            }
        }

        private void BtnDeleteLog_Click(object sender, EventArgs e)
        {
            if (dgvLogs.SelectedRows.Count > 0)
            {
                var log = dgvLogs.SelectedRows[0].DataBoundItem as PhenomenonLog;
                selectedClient.Logs.Remove(log);
                SaveData();
                RefreshLogs();
                DisplayClientDetails();
            }
        }

        private void RefreshLogs()
        {
            if (selectedClient != null)
            {
                dgvLogs.DataSource = null;
                dgvLogs.DataSource = selectedClient.Logs.OrderByDescending(l => l.DateTimeOccurred).ToList();
            }
        }

        private void BtnAddImage_Click(object sender, EventArgs e)
        {
            AddMediaFile("Image", "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp");
        }

        private void BtnAddVideo_Click(object sender, EventArgs e)
        {
            AddMediaFile("Video", "Video Files|*.mp4;*.avi;*.mov;*.wmv;*.mkv");
        }

        private void BtnAddAudio_Click(object sender, EventArgs e)
        {
            AddMediaFile("Audio", "Audio Files|*.mp3;*.wav;*.m4a;*.wma;*.aac");
        }

        private void BtnAddNote_Click(object sender, EventArgs e)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NoteForm noteForm = new NoteForm();
            if (noteForm.ShowDialog() == DialogResult.OK)
            {
                var note = new MediaFile
                {
                    Id = nextMediaId++,
                    FileName = noteForm.NoteTitle,
                    MediaCategory = "Note",
                    DateAdded = DateTime.Now,
                    Description = noteForm.NoteTitle,
                    NoteContent = noteForm.NoteContent,
                    NoteImagePath = noteForm.NoteImagePath
                };
                selectedClient.MediaFiles.Add(note);
                SaveData();
                RefreshMedia();
                DisplayClientDetails();
            }
        }

        private void AddMediaFile(string category, string filter)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = filter,
                Multiselect = true
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in ofd.FileNames)
                {
                    var media = new MediaFile
                    {
                        Id = nextMediaId++,
                        FileName = Path.GetFileName(file),
                        FilePath = file,
                        FileType = Path.GetExtension(file),
                        MediaCategory = category,
                        DateAdded = DateTime.Now,
                        Description = ""
                    };

                    // Generate video thumbnail
                    if (category == "Video")
                    {
                        media.VideoThumbnailPath = GenerateVideoThumbnail(file);
                    }

                    selectedClient.MediaFiles.Add(media);
                }
                SaveData();
                RefreshMedia();
                DisplayClientDetails();
            }
        }

        private string GenerateVideoThumbnail(string videoPath)
        {
            try
            {
                string thumbnailDir = Path.Combine(Path.GetDirectoryName(dataFilePath), "thumbnails");
                if (!Directory.Exists(thumbnailDir))
                {
                    Directory.CreateDirectory(thumbnailDir);
                }

                string thumbnailPath = Path.Combine(thumbnailDir, Path.GetFileNameWithoutExtension(videoPath) + "_thumb.jpg");

                // Use ffmpeg if available, otherwise return null
                if (File.Exists("ffmpeg.exe") || IsFFmpegInPath())
                {
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "ffmpeg",
                            Arguments = $"-i \"{videoPath}\" -ss 00:00:01.000 -vframes 1 \"{thumbnailPath}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    process.WaitForExit(5000); // Wait max 5 seconds

                    if (File.Exists(thumbnailPath))
                    {
                        return thumbnailPath;
                    }
                }
            }
            catch
            {
                // If thumbnail generation fails, return null
            }
            return null;
        }

        private bool IsFFmpegInPath()
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit(1000);
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private void RefreshMedia()
        {
            if (selectedClient != null)
            {
                flpImages.Controls.Clear();
                flpVideos.Controls.Clear();
                flpAudio.Controls.Clear();
                flpNotes.Controls.Clear();

                foreach (var media in selectedClient.MediaFiles.OrderByDescending(m => m.DateAdded))
                {
                    if (media.MediaCategory == "Image")
                    {
                        flpImages.Controls.Add(CreateMediaThumbnail(media));
                    }
                    else if (media.MediaCategory == "Video")
                    {
                        flpVideos.Controls.Add(CreateMediaThumbnail(media));
                    }
                    else if (media.MediaCategory == "Audio")
                    {
                        flpAudio.Controls.Add(CreateMediaThumbnail(media));
                    }
                    else if (media.MediaCategory == "Note")
                    {
                        flpNotes.Controls.Add(CreateNoteThumbnail(media));
                    }
                }
            }
        }

        private Control CreateMediaThumbnail(MediaFile media)
        {
            Panel panel = new Panel
            {
                Width = 120,
                Height = 120,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            PictureBox pb = new PictureBox
            {
                Width = 100,
                Height = 80,
                Top = 5,
                Left = 10,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,
                BackColor = Color.LightGray
            };

            Label lbl = new Label
            {
                Text = media.FileName,
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 8),
                AutoEllipsis = true
            };

            // Load image or thumbnail
            try
            {
                if (media.MediaCategory == "Image" && File.Exists(media.FilePath))
                {
                    pb.Image = Image.FromFile(media.FilePath);
                }
                else if (media.MediaCategory == "Video" && !string.IsNullOrEmpty(media.VideoThumbnailPath) && File.Exists(media.VideoThumbnailPath))
                {
                    pb.Image = Image.FromFile(media.VideoThumbnailPath);
                }
                else if (media.MediaCategory == "Audio")
                {
                    // Use a default audio icon
                    pb.Image = SystemIcons.Information.ToBitmap();
                }
                else
                {
                    pb.Image = SystemIcons.Question.ToBitmap();
                }
            }
            catch
            {
                pb.Image = SystemIcons.Warning.ToBitmap();
            }

            panel.Controls.Add(pb);
            panel.Controls.Add(lbl);

            // Optionally, add click event to open the file
            EventHandler clickHandler = (s, e) =>
            {
                if (!string.IsNullOrEmpty(media.FilePath) && File.Exists(media.FilePath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(media.FilePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Unable to open file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };
            panel.Click += clickHandler;
            pb.Click += clickHandler;
            lbl.Click += clickHandler;

            return panel;
        }

        private Control CreateNoteThumbnail(MediaFile media)
        {
            Panel panel = new Panel
            {
                Width = 120,
                Height = 120,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            PictureBox pb = new PictureBox
            {
                Width = 100,
                Height = 80,
                Top = 5,
                Left = 10,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,
                BackColor = Color.LightYellow
            };

            Label lbl = new Label
            {
                Text = media.FileName,
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 8, FontStyle.Bold),
                AutoEllipsis = true
            };

            // Load note image if available
            try
            {
                if (!string.IsNullOrEmpty(media.NoteImagePath) && File.Exists(media.NoteImagePath))
                {
                    pb.Image = Image.FromFile(media.NoteImagePath);
                }
                else
                {
                    // Use a default note icon or color
                    pb.Image = SystemIcons.Information.ToBitmap();
                }
            }
            catch
            {
                pb.Image = SystemIcons.Warning.ToBitmap();
            }

            panel.Controls.Add(pb);
            panel.Controls.Add(lbl);

            // Tooltip for note content
            ToolTip tip = new ToolTip();
            tip.SetToolTip(panel, media.NoteContent ?? "");

            // Optionally, add click event to open a dialog with the note content
            EventHandler clickHandler = (s, e) =>
            {
                MessageBox.Show(media.NoteContent ?? "(No content)", media.FileName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            panel.Click += clickHandler;
            pb.Click += clickHandler;
            lbl.Click += clickHandler;

            return panel;
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    using (FileStream fs = new FileStream(dataFilePath, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<Client>));
                        clients = (List<Client>)serializer.Deserialize(fs);

                        nextClientId = clients.Any() ? clients.Max(c => c.Id) + 1 : 1;
                        nextLogId = clients.SelectMany(c => c.Logs).Any() ? clients.SelectMany(c => c.Logs).Max(l => l.Id) + 1 : 1;
                        nextMediaId = clients.SelectMany(c => c.MediaFiles).Any() ? clients.SelectMany(c => c.MediaFiles).Max(m => m.Id) + 1 : 1;
                        nextReportId = clients.SelectMany(c => c.Reports).Any() ? clients.SelectMany(c => c.Reports).Max(r => r.Id) + 1 : 1;
                        nextTodoId = clients.SelectMany(c => c.TodoItems).Any() ? clients.SelectMany(c => c.TodoItems).Max(t => t.Id) + 1 : 1;
                        nextCalendarEventId = clients.SelectMany(c => c.CalendarEvents).Any() ? clients.SelectMany(c => c.CalendarEvents).Max(ev => ev.Id) + 1 : 1;
                        nextParanormalEventId = clients.SelectMany(c => c.ParanormalEvents).Any() ? clients.SelectMany(c => c.ParanormalEvents).Max(ev => ev.Id) + 1 : 1;
                    }
                }
                else
                {
                    clients = new List<Client>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                clients = new List<Client>();
            }
        }

        private void SaveData()
        {
            try
            {
                using (FileStream fs = new FileStream(dataFilePath, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Client>));
                    serializer.Serialize(fs, clients);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterClientList(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "Search clients...")
            {
                RefreshClientList();
            }
            else
            {
                string lower = searchText.ToLowerInvariant();
                clientListBox.Items.Clear();

                for (int i = 0; i < clients.Count; i++)
                {
                    var client = clients[i];
                    if ((client.Name != null && client.Name.ToLowerInvariant().Contains(lower)) ||
                        (client.Address != null && client.Address.ToLowerInvariant().Contains(lower)) ||
                        (client.LocationName != null && client.LocationName.ToLowerInvariant().Contains(lower)) ||
                        (client.LocationType != null && client.LocationType.ToLowerInvariant().Contains(lower)))
                    {
                        string locationType = !string.IsNullOrEmpty(client.LocationType) ? client.LocationType : "No Type";
                        clientListBox.Items.Add($"{client.Name} - {locationType}");
                    }
                }
            }
        }

        private void ClearClientFields()
        {
            txtName.Text = "";
            txtAddress.Text = "";
            txtPhone.Text = "";
            txtAltPhone.Text = "";
            txtEmail.Text = "";
            txtLocationName.Text = "";
            cmbLocationType.SelectedIndex = -1;
            cmbOwnerStatus.SelectedIndex = -1;
            nudOccupants.Value = 1;
            pbClientPhoto.Image = null;
            pbLocationPhoto.Image = null;
            rtbClientInfo.Clear();
        }

        private void BtnOpenInMaps_Click(object sender, EventArgs e)
        {
            if (selectedClient == null || string.IsNullOrWhiteSpace(selectedClient.Address))
            {
                MessageBox.Show("Please enter an address first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string address = Uri.EscapeDataString(selectedClient.Address);
                string url = $"https://www.google.com/maps/search/?api=1&query={address}";

                try
                {
                    System.Diagnostics.Process.Start(url);
                }
                catch
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening maps: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFont(string fontName, int fontSize)
        {
            if (rtbReportEditor.SelectionLength > 0)
            {
                rtbReportEditor.SelectionFont = new Font(fontName, fontSize, rtbReportEditor.SelectionFont.Style);
            }
            else
            {
                rtbReportEditor.Font = new Font(fontName, fontSize);
            }
        }

        private void ApplyStyle(FontStyle style)
        {
            if (rtbReportEditor.SelectionLength > 0)
            {
                Font currentFont = rtbReportEditor.SelectionFont;
                FontStyle newStyle = currentFont.Style ^ style;
                rtbReportEditor.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
            }
        }

        private void ApplyBullets()
        {
            rtbReportEditor.SelectionBullet = !rtbReportEditor.SelectionBullet;
        }

        private void ApplyNumbering()
        {
            if (rtbReportEditor.SelectionLength == 0)
            {
                rtbReportEditor.SelectionStart = rtbReportEditor.GetFirstCharIndexOfCurrentLine();
                rtbReportEditor.SelectionLength = 0;
            }

            string[] lines = rtbReportEditor.SelectedText.Split('\n');
            string numberedText = "";
            for (int i = 0; i < lines.Length; i++)
            {
                numberedText += $"{i + 1}. {lines[i].TrimStart()}\n";
            }
            rtbReportEditor.SelectedText = numberedText.TrimEnd('\n');
        }

        private void ApplyAlignment(HorizontalAlignment alignment)
        {
            rtbReportEditor.SelectionAlignment = alignment;
        }

        private void ApplyTextColor()
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                rtbReportEditor.SelectionColor = colorDialog.Color;
            }
        }

        private void ApplyHighlight()
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                rtbReportEditor.SelectionBackColor = colorDialog.Color;
            }
        }

        private void InsertImageIntoReport()
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Insert Image into Report"
            })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Helper.InsertImageIntoReport(rtbReportEditor, ofd.FileName);
                }
            }
        }

        private void PrintReport()
        {
            MessageBox.Show("Print functionality - Connect to system printer\n\nContent: " + rtbReportEditor.Text.Substring(0, Math.Min(100, rtbReportEditor.Text.Length)) + "...", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CreateNewReport(ListBox lstReports, TextBox txtReportTitle)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var report = new Report
            {
                Id = nextReportId++,
                Title = "New Report",
                Content = @"{\rtf1\ansi\deff0{\fonttbl{\f0 Arial;}}\f0\fs22 }",
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now
            };
            selectedClient.Reports.Add(report);
            SaveData();
            RefreshReportList(lstReports);
            lstReports.SelectedIndex = lstReports.Items.Count - 1;
        }

        private void LoadSelectedReport(ListBox lstReports, TextBox txtReportTitle)
        {
            if (lstReports.SelectedItem is Report report)
            {
                txtReportTitle.Text = report.Title ?? "";

                if (string.IsNullOrEmpty(report.Content))
                {
                    rtbReportEditor.Clear();
                    return;
                }

                try
                {
                    rtbReportEditor.Rtf = report.Content;
                }
                catch (ArgumentException)
                {
                    rtbReportEditor.Text = report.Content;
                    try
                    {
                        report.Content = rtbReportEditor.Rtf;
                        report.DateModified = DateTime.Now;
                        SaveData();
                    }
                    catch { }
                }
            }
            else
            {
                txtReportTitle.Text = "";
                rtbReportEditor.Text = "";
            }
        }

        private void SaveCurrentReport(string title, ListBox lstReports)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Report report;
            if (lstReports.SelectedItem is Report existingReport)
            {
                report = existingReport;
            }
            else
            {
                report = new Report
                {
                    Id = nextReportId++,
                    DateCreated = DateTime.Now
                };
                selectedClient.Reports.Add(report);
            }

            report.Title = string.IsNullOrWhiteSpace(title) ? "Untitled Report" : title;
            report.Content = rtbReportEditor.Rtf;
            report.DateModified = DateTime.Now;
            SaveData();

            int currentIndex = lstReports.SelectedIndex;
            RefreshReportList(lstReports);

            if (currentIndex >= 0 && currentIndex < lstReports.Items.Count)
            {
                lstReports.SelectedIndex = currentIndex;
            }
            else
            {
                lstReports.SelectedItem = report;
            }

            MessageBox.Show("Report saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportReport(string title)
        {
            if (string.IsNullOrWhiteSpace(rtbReportEditor.Text))
            {
                MessageBox.Show("Report is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text Files|*.txt|Rich Text Format|*.rtf|All Files|*.*",
                FileName = title + ".txt"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (sfd.FileName.EndsWith(".rtf"))
                    {
                        rtbReportEditor.SaveFile(sfd.FileName, RichTextBoxStreamType.RichText);
                    }
                    else
                    {
                        File.WriteAllText(sfd.FileName, rtbReportEditor.Text);
                    }
                    MessageBox.Show("Report exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportReportToPdf(string title)
        {
            if (string.IsNullOrWhiteSpace(rtbReportEditor.Rtf) && string.IsNullOrWhiteSpace(rtbReportEditor.Text))
            {
                MessageBox.Show("Report is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = (string.IsNullOrWhiteSpace(title) ? "Report" : title) + ".pdf"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                using (iText.Document document = new iText.Document(iText.PageSize.LETTER, 36, 36, 36, 36))
                {
                    iTextPdf.PdfWriter.GetInstance(document, fs);
                    document.Open();

                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        var titleFont = iText.FontFactory.GetFont(iText.FontFactory.HELVETICA_BOLD, 16);
                        document.Add(new iText.Paragraph(title, titleFont));
                        document.Add(iText.Chunk.NEWLINE);
                    }

                    var bodyFont = iText.FontFactory.GetFont(iText.FontFactory.HELVETICA, 11);
                    string text = rtbReportEditor.Text ?? string.Empty;
                    string[] lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    foreach (string line in lines)
                    {
                        document.Add(new iText.Paragraph(line, bodyFont));
                    }

                    document.Close();
                }

                MessageBox.Show("PDF exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteReport(ListBox lstReports, TextBox txtReportTitle)
        {
            if (selectedClient == null || lstReports.SelectedItem == null) return;

            var report = lstReports.SelectedItem as Report;
            var res = MessageBox.Show($"Delete report '{report.Title}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                selectedClient.Reports.Remove(report);
                SaveData();
                RefreshReportList(lstReports);
                rtbReportEditor.Clear();
                txtReportTitle.Clear();
            }
        }

        private void RefreshReportList(ListBox lstReports)
        {
            if (lstReports == null) return;

            int selectedIndex = lstReports.SelectedIndex;
            lstReports.DataSource = null;

            if (selectedClient != null && selectedClient.Reports != null && selectedClient.Reports.Count > 0)
            {
                lstReports.DataSource = selectedClient.Reports.ToList();
                lstReports.DisplayMember = "Title";

                if (selectedIndex >= 0 && selectedIndex < lstReports.Items.Count)
                {
                    lstReports.SelectedIndex = selectedIndex;
                }
            }
        }

        private void RefreshReports()
        {
            var tab = tabControl.TabPages["Reports"];
            if (tab?.Tag != null)
            {
                dynamic refs = tab.Tag;
                ListBox lstReports = refs.ReportList;
                TextBox txtReportTitle = refs.TitleBox;

                lstReports.DataSource = null;

                if (selectedClient != null && selectedClient.Reports != null && selectedClient.Reports.Count > 0)
                {
                    lstReports.DataSource = selectedClient.Reports.ToList();
                    lstReports.DisplayMember = "Title";
                }

                rtbReportEditor.Clear();
                txtReportTitle.Clear();
            }
        }

        private void BtnAddTodo_Click(object sender, EventArgs e)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTodoItem.Text)) return;

            var todo = new TodoItem
            {
                Id = nextTodoId++,
                Task = txtTodoItem.Text,
                IsCompleted = false,
                DateAdded = DateTime.Now
            };
            selectedClient.TodoItems.Add(todo);
            txtTodoItem.Clear();
            SaveData();
            RefreshTodoList();
        }

        private void BtnDeleteTodo_Click(object sender, EventArgs e)
        {
            if (selectedClient == null || lstTodoItems.SelectedItem == null) return;

            var todo = lstTodoItems.SelectedItem as TodoItem;
            selectedClient.TodoItems.Remove(todo);
            SaveData();
            RefreshTodoList();
        }

        private void LstTodoItems_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();
            var todo = lstTodoItems.Items[e.Index] as TodoItem;
            var font = todo.IsCompleted ? new Font(e.Font, FontStyle.Strikeout) : e.Font;
            var color = todo.IsCompleted ? Color.Gray : e.ForeColor;

            e.Graphics.DrawString(todo.Task, font, new SolidBrush(color), e.Bounds);
            e.DrawFocusRectangle();
        }

        private void LstTodoItems_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lstTodoItems.IndexFromPoint(e.Location);
            if (index >= 0 && index < lstTodoItems.Items.Count)
            {
                var todo = lstTodoItems.Items[index] as TodoItem;
                todo.IsCompleted = !todo.IsCompleted;
                SaveData();
                lstTodoItems.Invalidate();
            }
        }

        private void RefreshTodoList()
        {
            if (selectedClient != null)
            {
                lstTodoItems.DataSource = null;
                lstTodoItems.DataSource = selectedClient.TodoItems;
                lstTodoItems.DisplayMember = "Task";
            }
        }

        private void CalClientCalendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            RefreshCalendarEvents();
        }

        private void AddCalendarEvent(TextBox txtTitle, ListBox lstEvents)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Please enter an event title.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var calEvent = new CalendarEvent
            {
                Id = nextCalendarEventId++,
                EventDate = calClientCalendar.SelectionStart,
                Title = txtTitle.Text,
                Description = ""
            };
            selectedClient.CalendarEvents.Add(calEvent);
            txtTitle.Clear();
            SaveData();
            RefreshCalendarEvents();
        }

        private void DeleteCalendarEvent(ListBox lstEvents)
        {
            if (selectedClient == null || lstEvents.SelectedItem == null) return;

            var calEvent = lstEvents.SelectedItem as CalendarEvent;
            selectedClient.CalendarEvents.Remove(calEvent);
            SaveData();
            RefreshCalendarEvents();
        }

        private void RefreshCalendarEvents()
        {
            var tab = tabControl.TabPages["To-Do & Calendar"];
            if (tab?.Tag is ListBox lstEvents && selectedClient != null && calClientCalendar != null)
            {
                var selectedDate = calClientCalendar.SelectionStart.Date;
                var events = selectedClient.CalendarEvents.Where(ev => ev.EventDate.Date == selectedDate).ToList();
                lstEvents.DataSource = null;
                lstEvents.DataSource = events;
                lstEvents.DisplayMember = "Title";
            }
        }

        private List<CalendarEvent> GetEventsForSelectedDate()
        {
            if (selectedClient == null || calClientCalendar == null) return new List<CalendarEvent>();
            var d = calClientCalendar.SelectionStart.Date;
            return selectedClient.CalendarEvents.Where(ev => ev.EventDate.Date == d).ToList();
        }

        private void ExportSelectedDateToIcs()
        {
            var events = GetEventsForSelectedDate();
            if (events == null || events.Count == 0)
            {
                MessageBox.Show("No events on the selected date to export.", "Export .ics", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string ics = GenerateIcsContent(events);
                string fileName = $"MAPS_Events_{calClientCalendar.SelectionStart:yyyyMMdd}.ics";
                string path = Path.Combine(Path.GetTempPath(), fileName);
                File.WriteAllText(path, ics, Encoding.UTF8);

                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch
                {
                    try
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = path,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Unable to open file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting .ics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateIcsContent(List<CalendarEvent> events)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//MAPS ParanormalInvestigator//EN");

            foreach (var ev in events)
            {
                var uid = Guid.NewGuid().ToString();
                var dtstamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
                var dtstart = ev.EventDate.Date.ToString("yyyyMMdd");
                var dtend = ev.EventDate.Date.AddDays(1).ToString("yyyyMMdd");

                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"UID:{uid}");
                sb.AppendLine($"DTSTAMP:{dtstamp}");
                sb.AppendLine($"DTSTART;VALUE=DATE:{dtstart}");
                sb.AppendLine($"DTEND;VALUE=DATE:{dtend}");
                sb.AppendLine($"SUMMARY:{EscapeIcsText(ev.Title ?? "Event")}");
                if (!string.IsNullOrEmpty(ev.Description))
                    sb.AppendLine($"DESCRIPTION:{EscapeIcsText(ev.Description)}");
                sb.AppendLine("END:VEVENT");
            }

            sb.AppendLine("END:VCALENDAR");
            return sb.ToString();
        }

        private string EscapeIcsText(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "").Replace(";", "\\;").Replace(",", "\\,");
        }

        private void OpenSelectedDateInGoogle()
        {
            var events = GetEventsForSelectedDate();
            if (events == null || events.Count == 0)
            {
                MessageBox.Show("No events on the selected date to open in Google Calendar.", "Google Calendar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                foreach (var ev in events)
                {
                    var start = ev.EventDate.Date.ToString("yyyyMMdd");
                    var end = ev.EventDate.Date.AddDays(1).ToString("yyyyMMdd");

                    var url = new StringBuilder();
                    url.Append("https://www.google.com/calendar/render?action=TEMPLATE");
                    url.Append("&text=").Append(Uri.EscapeDataString(ev.Title ?? "Event"));
                    url.Append("&dates=").Append(Uri.EscapeDataString($"{start}/{end}"));
                    url.Append("&details=").Append(Uri.EscapeDataString(ev.Description ?? ""));
                    url.Append("&location=").Append(Uri.EscapeDataString(selectedClient?.Address ?? ""));

                    try
                    {
                        System.Diagnostics.Process.Start(url.ToString());
                    }
                    catch
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url.ToString()) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Google Calendar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LstEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstEvents.SelectedItem is ParanormalEvent selectedEvent)
            {
                var tab = tabControl.TabPages["Event Planning"];
                if (tab?.Tag != null)
                {
                    dynamic refs = tab.Tag;
                    NumericUpDown nudAttendees = refs.Attendees;
                    NumericUpDown nudBudget = refs.Budget;
                    TextBox txtEventNotes = refs.Notes;

                    txtEventName.Text = selectedEvent.EventName;
                    cmbEventType.SelectedItem = selectedEvent.EventType;
                    dtpEventDate.Value = selectedEvent.EventDate;
                    txtEventLocation.Text = selectedEvent.Location;
                    nudAttendees.Value = Math.Max(nudAttendees.Minimum, Math.Min(nudAttendees.Maximum, selectedEvent.ExpectedAttendees));
                    nudBudget.Value = Math.Max(nudBudget.Minimum, Math.Min(nudBudget.Maximum, selectedEvent.Budget));
                    txtEventDescription.Text = selectedEvent.Description;
                    txtEventNotes.Text = selectedEvent.Notes;
                }
            }
            else
            {
                ClearEventFields();
            }
        }

        private void ClearEventFields()
        {
            txtEventName.Text = "";
            cmbEventType.SelectedIndex = -1;
            dtpEventDate.Value = DateTime.Now;
            txtEventLocation.Text = "";
            txtEventDescription.Text = "";
            var tab = tabControl.TabPages["Event Planning"];
            if (tab?.Tag != null)
            {
                dynamic refs = tab.Tag;
                NumericUpDown nudAttendees = refs.Attendees;
                NumericUpDown nudBudget = refs.Budget;
                TextBox txtEventNotes = refs.Notes;
                nudAttendees.Value = nudAttendees.Minimum;
                nudBudget.Value = nudBudget.Minimum;
                txtEventNotes.Text = "";
            }
        }

        private void BtnNewEvent_Click(object sender, EventArgs e)
        {
            ClearEventFields();
            lstEvents.ClearSelected();
        }

        private void SaveParanormalEvent(NumericUpDown nudAttendees, NumericUpDown nudBudget, TextBox txtEventNotes)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ParanormalEvent evt;
            if (lstEvents.SelectedItem is ParanormalEvent existingEvent)
            {
                evt = existingEvent;
            }
            else
            {
                evt = new ParanormalEvent
                {
                    Id = nextParanormalEventId++,
                    DateCreated = DateTime.Now
                };
                selectedClient.ParanormalEvents.Add(evt);
            }

            evt.EventName = txtEventName.Text;
            evt.EventType = cmbEventType.SelectedItem?.ToString();
            evt.EventDate = dtpEventDate.Value;
            evt.Location = txtEventLocation.Text;
            evt.Description = txtEventDescription.Text;
            evt.ExpectedAttendees = (int)nudAttendees.Value;
            evt.Budget = nudBudget.Value;
            evt.Notes = txtEventNotes.Text;

            SaveData();
            RefreshEventList();
            MessageBox.Show("Event saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshEventList()
        {
            if (lstEvents == null) return;
            lstEvents.DataSource = null;
            if (selectedClient != null && selectedClient.ParanormalEvents != null && selectedClient.ParanormalEvents.Count > 0)
            {
                lstEvents.DataSource = selectedClient.ParanormalEvents.ToList();
                lstEvents.DisplayMember = "EventName";
            }
        }

        private void BtnDeleteEvent_Click(object sender, EventArgs e)
        {
            if (selectedClient == null || lstEvents.SelectedItem == null)
            {
                MessageBox.Show("Please select an event to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var evt = lstEvents.SelectedItem as ParanormalEvent;
            var res = MessageBox.Show($"Delete event '{evt.EventName}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                selectedClient.ParanormalEvents.Remove(evt);
                SaveData();
                RefreshEventList();
                ClearEventFields();
            }
        }

        private void ExportSelectedParanormalEventToIcs()
        {
            if (selectedClient == null || lstEvents.SelectedItem == null)
            {
                MessageBox.Show("Please select an event to export.", "Export .ics", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var evt = lstEvents.SelectedItem as ParanormalEvent;
            if (evt == null) return;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("BEGIN:VCALENDAR");
                sb.AppendLine("VERSION:2.0");
                sb.AppendLine("PRODID:-//MAPS ParanormalInvestigator//EN");
                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"UID:{Guid.NewGuid()}");
                sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}");
                sb.AppendLine($"DTSTART;VALUE=DATE:{evt.EventDate:yyyyMMdd}");
                sb.AppendLine($"DTEND;VALUE=DATE:{evt.EventDate.AddDays(1):yyyyMMdd}");
                sb.AppendLine($"SUMMARY:{EscapeIcsText(evt.EventName ?? "Event")}");
                if (!string.IsNullOrEmpty(evt.Description))
                    sb.AppendLine($"DESCRIPTION:{EscapeIcsText(evt.Description)}");
                sb.AppendLine($"LOCATION:{EscapeIcsText(evt.Location ?? string.Empty)}");
                sb.AppendLine("END:VEVENT");
                sb.AppendLine("END:VCALENDAR");

                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "iCalendar Files|*.ics",
                    FileName = $"MAPS_Event_{evt.EventDate:yyyyMMdd}.ics"
                };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Event exported as .ics successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting .ics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenSelectedParanormalEventInGoogle()
        {
            if (selectedClient == null || lstEvents.SelectedItem == null)
            {
                MessageBox.Show("Please select an event to open in Google Calendar.", "Google Calendar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var evt = lstEvents.SelectedItem as ParanormalEvent;
            if (evt == null) return;

            try
            {
                var start = evt.EventDate.Date.ToString("yyyyMMdd");
                var end = evt.EventDate.Date.AddDays(1).ToString("yyyyMMdd");

                var url = new StringBuilder();
                url.Append("https://www.google.com/calendar/render?action=TEMPLATE");
                url.Append("&text=").Append(Uri.EscapeDataString(evt.EventName ?? "Event"));
                url.Append("&dates=").Append(Uri.EscapeDataString($"{start}/{end}"));
                url.Append("&details=").Append(Uri.EscapeDataString(evt.Description ?? ""));
                url.Append("&location=").Append(Uri.EscapeDataString(evt.Location ?? ""));

                try
                {
                    System.Diagnostics.Process.Start(url.ToString());
                }
                catch
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url.ToString()) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Google Calendar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    } // End of MainForm class
} // End of MainForm class

    public class LogForm : Form
    {
        public DateTime DateTimeOccurred { get; private set; }
        public string PhenomenonType { get; private set; }
        public string Description { get; private set; }
        public string LocationText { get; private set; }
        public string Witnesses { get; private set; }

        private DateTimePicker dtpOccurred;
        private ComboBox cmbType;
        private TextBox txtDescription, txtLocation, txtWitnesses;

        public LogForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Add Phenomenon Log";
            this.Size = new Size(550, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(500, 400);

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 6,
                ColumnCount = 2,
                Padding = new Padding(15)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            Label lblDateTime = new Label { Text = "Date/Time:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            dtpOccurred = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MM/dd/yyyy hh:mm tt"
            };

            Label lblType = new Label { Text = "Type:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            cmbType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new object[] { "Apparition", "EVP", "Cold Spot", "Object Movement", "Shadow Figure", "Strange Sound", "EMF Spike", "Other" });
            cmbType.SelectedIndex = 0;

            Label lblLocation = new Label { Text = "Location:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            txtLocation = new TextBox { Dock = DockStyle.Fill };

            Label lblWitnesses = new Label { Text = "Witnesses:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            txtWitnesses = new TextBox { Dock = DockStyle.Fill };

            Label lblDescription = new Label { Text = "Description:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft };
            txtDescription = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };

            Panel buttonPanel = new Panel { Dock = DockStyle.Fill };
            Button btnOK = new Button { Text = "Save", Width = 90, Height = 35, DialogResult = DialogResult.OK };
            btnOK.Click += (s, e) =>
            {
                DateTimeOccurred = dtpOccurred.Value;
                PhenomenonType = cmbType.SelectedItem?.ToString();
                Description = txtDescription.Text;
                LocationText = txtLocation.Text;
                Witnesses = txtWitnesses.Text;
            };

            Button btnCancel = new Button { Text = "Cancel", Width = 90, Height = 35, DialogResult = DialogResult.Cancel };
            buttonPanel.Controls.AddRange(new Control[] { btnOK, btnCancel });
            buttonPanel.Resize += (s, e) =>
            {
                btnOK.Location = new Point(buttonPanel.Width - 200, 5);
                btnCancel.Location = new Point(buttonPanel.Width - 95, 5);
            };

            mainLayout.Controls.Add(lblDateTime, 0, 0);
            mainLayout.Controls.Add(dtpOccurred, 1, 0);
            mainLayout.Controls.Add(lblType, 0, 1);
            mainLayout.Controls.Add(cmbType, 1, 1);
            mainLayout.Controls.Add(lblLocation, 0, 2);
            mainLayout.Controls.Add(txtLocation, 1, 2);
            mainLayout.Controls.Add(lblWitnesses, 0, 3);
            mainLayout.Controls.Add(txtWitnesses, 1, 3);
            mainLayout.Controls.Add(lblDescription, 0, 4);
            mainLayout.Controls.Add(txtDescription, 1, 4);
            mainLayout.Controls.Add(buttonPanel, 0, 5);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            this.Controls.Add(mainLayout);
        }
    }

    public class NoteForm : Form
    {
        public string NoteTitle { get; private set; }
        public string NoteContent { get; private set; }
        public string NoteImagePath { get; private set; }

        private TextBox txtTitle;
        private RichTextBox rtbContent;
        private PictureBox pbNoteImage;

        public NoteForm(string title = "", string content = "", string imagePath = "")
        {
            NoteTitle = title;
            NoteContent = content;
            NoteImagePath = imagePath;
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Paranormal Investigation Note";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(600, 500);

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            Label lblTitle = new Label { Text = "Note Title:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Arial", 10, FontStyle.Bold) };
            txtTitle = new TextBox { Dock = DockStyle.Fill, Font = new Font("Arial", 10), Text = NoteTitle };

            Panel imagePanel = new Panel { Dock = DockStyle.Fill };
            pbNoteImage = new PictureBox
            {
                Width = 200, Height = 150, Location = new Point(5, 5),
                BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.LightGray
            };

            if (!string.IsNullOrEmpty(NoteImagePath) && File.Exists(NoteImagePath))
            {
                try { pbNoteImage.Image = Image.FromFile(NoteImagePath); } catch { }
            }

            Button btnAddImage = new Button { Text = "Add Image", Location = new Point(215, 5), Width = 100, Height = 30 };
            btnAddImage.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try { pbNoteImage.Image = Image.FromFile(ofd.FileName); NoteImagePath = ofd.FileName; }
                    catch (Exception ex) { MessageBox.Show($"Error loading image: {ex.Message}"); }
                }
            };

            Button btnRemoveImage = new Button { Text = "Remove Image", Location = new Point(215, 40), Width = 100, Height = 30 };
            btnRemoveImage.Click += (s, e) => { pbNoteImage.Image = null; NoteImagePath = null; };

            imagePanel.Controls.AddRange(new Control[] { pbNoteImage, btnAddImage, btnRemoveImage });

            Label lblContent = new Label { Text = "Note Content:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft, Font = new Font("Arial", 10, FontStyle.Bold) };
            rtbContent = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Arial", 10), Text = NoteContent };

            Panel buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            Button btnSave = new Button { Text = "Save", Width = 90, Height = 35, DialogResult = DialogResult.OK };
            btnSave.Click += (s, e) => { NoteTitle = txtTitle.Text; NoteContent = rtbContent.Text; };
            Button btnCancel = new Button { Text = "Cancel", Width = 90, Height = 35, DialogResult = DialogResult.Cancel };
            buttonPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });
            buttonPanel.Resize += (s, e) =>
            {
                btnSave.Location = new Point(buttonPanel.Width - 200, 7);
                btnCancel.Location = new Point(buttonPanel.Width - 95, 7);
            };

            mainLayout.Controls.Add(lblTitle, 0, 0);
            mainLayout.Controls.Add(txtTitle, 1, 0);
            mainLayout.Controls.Add(imagePanel, 0, 1);
            mainLayout.SetColumnSpan(imagePanel, 2);
            mainLayout.Controls.Add(lblContent, 0, 2);
            mainLayout.SetColumnSpan(lblContent, 2);
            mainLayout.Controls.Add(rtbContent, 0, 3);
            mainLayout.SetColumnSpan(rtbContent, 2);
            mainLayout.Controls.Add(buttonPanel, 0, 4);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            this.Controls.Add(mainLayout);
        }
    }

    public static class Helper
    {
        public static void InsertImageIntoReport(RichTextBox rtbReportEditor, string imagePath)
        {
            if (File.Exists(imagePath))
            {
                try
                {
                    using (Image img = Image.FromFile(imagePath))
                    {
                        Clipboard.SetImage(img);
                        rtbReportEditor.Focus();
                        rtbReportEditor.Paste();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error inserting image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
