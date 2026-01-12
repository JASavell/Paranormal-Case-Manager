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
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;

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
        public DateTime? DueDate { get; set; }  // NEW: Optional due date with time
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
        public string ContactPerson { get; set; }      // NEW
        public string ContactTelephone { get; set; }   // NEW
        public decimal Price { get; set; }             // NEW
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
        private TextBox txtTodoItem, txtEventName, txtEventLocation, txtEventDescription, txtContactPerson, txtContactTelephone;
        private DateTimePicker dtpEventDate;
        private ComboBox cmbEventType;
        private ListBox lstEvents;
        private TextBox txtLocationName, txtAltPhone;
        private ComboBox cmbOwnerStatus;
        private NumericUpDown nudOccupants, nudPrice;
        private RichTextBox rtbEventDetails;
        private Button btnExportIcs;
        private Button btnOpenGoogle;
        private ParanormalEvent selectedEvent; // ADD THIS LINE
        
        // Add field for image preview form
        private Form imagePreviewForm;

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
        private void LstTodoItems_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || lstTodoItems.Items.Count == 0)
                return;

            var item = lstTodoItems.Items[e.Index] as TodoItem;
            if (item == null)
            {
                e.DrawBackground();
                e.Graphics.DrawString(lstTodoItems.Items[e.Index].ToString(), e.Font, Brushes.Black, e.Bounds);
                e.DrawFocusRectangle();
                return;
            }

            // Set background color
            e.DrawBackground();

            // Draw completed items with strikeout and gray color
            Font font = item.IsCompleted
                ? new Font(e.Font, FontStyle.Strikeout)
                : e.Font;
            Brush brush = item.IsCompleted
                ? Brushes.Gray
                : Brushes.Black;

            e.Graphics.DrawString(item.Task, font, brush, e.Bounds);

            e.DrawFocusRectangle();
        }
        private void LstTodoItems_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 40; // Taller items to show date/time
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

            // Add search panel
            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(5)
            };

            Label lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(5, 12),
                Width = 60,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            TextBox txtLogSearch = new TextBox
            {
                Location = new Point(70, 10),
                Width = 250,
                Font = new Font("Arial", 10)
            };

            // Add placeholder text functionality
            txtLogSearch.ForeColor = Color.Gray;
            txtLogSearch.Text = "Search logs by type, description, location, or witnesses...";
            txtLogSearch.GotFocus += (s, e) =>
            {
                if (txtLogSearch.Text == "Search logs by type, description, location, or witnesses...")
                {
                    txtLogSearch.Text = "";
                    txtLogSearch.ForeColor = Color.Black;
                }
            };
            txtLogSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtLogSearch.Text))
                {
                    txtLogSearch.Text = "Search logs by type, description, location, or witnesses...";
                    txtLogSearch.ForeColor = Color.Gray;
                }
            };

            txtLogSearch.TextChanged += (s, e) =>
            {
                FilterPhenomenonLogs(txtLogSearch.Text);
            };

            Button btnClearSearch = new Button
            {
                Text = "Clear",
                Location = new Point(330, 9),
                Width = 60,
                Height = 25
            };
            btnClearSearch.Click += (s, e) =>
            {
                txtLogSearch.Text = "";
                txtLogSearch.ForeColor = Color.Black;
                FilterPhenomenonLogs("");
            };

            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtLogSearch, btnClearSearch });

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
            panel.Controls.Add(searchPanel);
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
                        pbClientPhoto.Image = System.Drawing.Image.FromFile(selectedClient.ClientPhotoPath);
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
                        pbLocationPhoto.Image = System.Drawing.Image.FromFile(selectedClient.LocationPhotoPath);
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
                    pbClientPhoto.Image = System.Drawing.Image.FromFile(ofd.FileName);
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
                    pbLocationPhoto.Image = System.Drawing.Image.FromFile(ofd.FileName);
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
        private void FilterPhenomenonLogs(string searchText)
        {
            if (selectedClient == null)
            {
                dgvLogs.DataSource = null;
                return;
            }

            // If search is empty or is the placeholder text, show all logs
            if (string.IsNullOrWhiteSpace(searchText) ||
                searchText == "Search logs by type, description, location, or witnesses...")
            {
                RefreshLogs();
                return;
            }

            string lower = searchText.ToLowerInvariant();

            var filteredLogs = selectedClient.Logs
                .Where(log =>
                    (log.Type ?? "").ToLowerInvariant().Contains(lower) ||
                    (log.Description ?? "").ToLowerInvariant().Contains(lower) ||
                    (log.Location ?? "").ToLowerInvariant().Contains(lower) ||
                    (log.Witnesses ?? "").ToLowerInvariant().Contains(lower) ||
                    log.DateTimeOccurred.ToString().ToLowerInvariant().Contains(lower))
                .OrderByDescending(l => l.DateTimeOccurred)
                .ToList();

            dgvLogs.DataSource = null;
            dgvLogs.DataSource = filteredLogs;
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
                Height = 180,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Tag = media
            };

            PictureBox pb = new PictureBox
            {
                Width = 100,
                Height = 80,
                Top = 5,
                Left = 10,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,
                BackColor = Color.LightGray,
                Cursor = Cursors.Hand
            };

            Label lbl = new Label
            {
                Text = media.FileName,
                Location = new Point(0, 90),
                Width = 120,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 8),
                AutoEllipsis = true
            };

            Label lblDesc = new Label
            {
                Text = string.IsNullOrEmpty(media.Description) ? "(no content)" : media.Description,
                Location = new Point(0, 120),
                Width = 120,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 7, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoEllipsis = true
            };

            Button btnEditText = new Button
            {
                Text = "Edit Text",
                Location = new Point(10, 152),
                Width = 100,
                Height = 22,
                Font = new Font("Arial", 7),
                FlatStyle = FlatStyle.Flat
            };
            btnEditText.Click += (s, e) => EditMediaDescription(media);

            // Load image or thumbnail
            try
            {
                if (media.MediaCategory == "Image" && File.Exists(media.FilePath))
                {
                    pb.Image = System.Drawing.Image.FromFile(media.FilePath);

                    // Add mouse hover events for full-size preview
                    pb.MouseEnter += (s, e) => ShowImagePreview(media.FilePath, pb);
                    pb.MouseLeave += (s, e) => HideImagePreview();
                }
                else if (media.MediaCategory == "Video" && !string.IsNullOrEmpty(media.VideoThumbnailPath) && File.Exists(media.VideoThumbnailPath))
                {
                    pb.Image = System.Drawing.Image.FromFile(media.VideoThumbnailPath);

                    // Add mouse hover for video thumbnail
                    pb.MouseEnter += (s, e) => ShowImagePreview(media.VideoThumbnailPath, pb);
                    pb.MouseLeave += (s, e) => HideImagePreview();
                }
                else if (media.MediaCategory == "Audio")
                {
                    // Try to load audio.jpg, fallback to system icon if not found
                    if (File.Exists("audio.jpg"))
                    {
                        pb.Image = System.Drawing.Image.FromFile("audio.jpg");
                    }
                    else
                    {
                        pb.Image = SystemIcons.Information.ToBitmap();
                    }
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
            panel.Controls.Add(lblDesc);
            panel.Controls.Add(btnEditText);

            ToolTip tip = new ToolTip();
            tip.SetToolTip(panel, media.Description ?? "");
            tip.SetToolTip(lblDesc, media.Description ?? "");

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
            pb.Click += clickHandler;

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem editDescItem = new ToolStripMenuItem("Edit Description");
            editDescItem.Click += (s, e) => EditMediaDescription(media);
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (s, e) => DeleteMediaFile(media);
            contextMenu.Items.Add(editDescItem);
            contextMenu.Items.Add(deleteItem);
            panel.ContextMenuStrip = contextMenu;

            return panel;
        }
        private void DeleteMediaFile(MediaFile media)
        {
            if (selectedClient == null || media == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{media.FileName}'?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                selectedClient.MediaFiles.Remove(media);
                SaveData();
                RefreshMedia();
                DisplayClientDetails();
            }
        }
        private void EditMediaDescription(MediaFile media)
        {
            if (media == null)
                return;

            string desc = Microsoft.VisualBasic.Interaction.InputBox(
                "Edit description for this media file:",
                "Edit Description",
                media.Description ?? "");

            if (desc != null)
            {
                media.Description = desc;
                SaveData();
                RefreshMedia();
            }
        }
        private Control CreateNoteThumbnail(MediaFile media)
        {
            Panel panel = new Panel
            {
                Width = 120,
                Height = 160, // Reduced height since we removed the button
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Tag = media
            };

            PictureBox pb = new PictureBox
            {
                Width = 100,
                Height = 80,
                Top = 5,
                Left = 10,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,
                BackColor = Color.LightYellow,
                Cursor = Cursors.Hand
            };

            Label lbl = new Label
            {
                Text = media.FileName,
                Location = new Point(0, 90),
                Width = 120,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 8, FontStyle.Bold),
                AutoEllipsis = true
            };

            string excerptText = "(no content)";
            if (!string.IsNullOrEmpty(media.NoteContent))
            {
                excerptText = media.NoteContent.Length > 50 
                    ? media.NoteContent.Substring(0, 50) + "..." 
                    : media.NoteContent;
                excerptText = excerptText.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
            }

            Label lblDesc = new Label
            {
                Text = excerptText,
                Location = new Point(0, 120),
                Width = 120,
                Height = 40, // Increased height to use the space from removed button
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 7, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoEllipsis = true
            };

            // Load note image if available
            try
            {
                if (!string.IsNullOrEmpty(media.NoteImagePath) && File.Exists(media.NoteImagePath))
                {
                    pb.Image = System.Drawing.Image.FromFile(media.NoteImagePath);
                    
                    // Add mouse hover events for full-size preview
                    pb.MouseEnter += (s, e) => ShowImagePreview(media.NoteImagePath, pb);
                    pb.MouseLeave += (s, e) => HideImagePreview();
                }
                else
                {
                    pb.Image = SystemIcons.Information.ToBitmap();
                }
            }
            catch
            {
                pb.Image = SystemIcons.Warning.ToBitmap();
            }

            panel.Controls.Add(pb);
            panel.Controls.Add(lbl);
            panel.Controls.Add(lblDesc);
            // Removed: btnEditText button and its event handler

            ToolTip tip = new ToolTip();
            tip.SetToolTip(panel, media.NoteContent ?? "(No content)");
            tip.SetToolTip(lblDesc, media.NoteContent ?? "(No content)");

            EventHandler clickHandler = (s, e) =>
            {
                MessageBox.Show(media.NoteContent ?? "(No content)", media.FileName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            panel.Click += clickHandler;
            pb.Click += clickHandler;

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            // Removed: "Edit Description" menu item
            ToolStripMenuItem editNoteItem = new ToolStripMenuItem("Edit Note");
            editNoteItem.Click += (s, e) => EditNote(media);
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (s, e) => DeleteMediaFile(media);
            contextMenu.Items.Add(editNoteItem);
            contextMenu.Items.Add(deleteItem);
            panel.ContextMenuStrip = contextMenu;

            return panel;
        }
        private void EditNote(MediaFile media)
        {
            if (media == null)
                return;

            // Use the constructor that accepts existing values to properly load the image
            NoteForm noteForm = new NoteForm(media.FileName, media.NoteContent, media.NoteImagePath);

            if (noteForm.ShowDialog() == DialogResult.OK)
            {
                media.FileName = noteForm.NoteTitle;
                media.Description = noteForm.NoteTitle;
                media.NoteContent = noteForm.NoteContent;
                media.NoteImagePath = noteForm.NoteImagePath;
                SaveData();
                RefreshMedia();
            }
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

        private void FilterClientList(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "Search clients...")
            {
                RefreshClientList();
                return;
            }

            string lower = searchText.ToLowerInvariant();
            clientListBox.Items.Clear();

            foreach (var client in clients)
            {
                if ((client.Name ?? "").ToLowerInvariant().Contains(lower) ||
                    (client.Address ?? "").ToLowerInvariant().Contains(lower) ||
                    (client.LocationName ?? "").ToLowerInvariant().Contains(lower) ||
                    (client.LocationType ?? "").ToLowerInvariant().Contains(lower))
                {
                    string locationType = !string.IsNullOrEmpty(client.LocationType) ? client.LocationType : "No Type";
                    clientListBox.Items.Add($"{client.Name} - {locationType}");
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

            string address = Uri.EscapeDataString(selectedClient.Address);
            string url = $"https://www.google.com/maps/search/?api=1&query={address}";
            try { System.Diagnostics.Process.Start(url); }
            catch { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); }
        }

        private void ApplyFont(string fontName, int fontSize)
        {
            if (rtbReportEditor.SelectionLength > 0 && rtbReportEditor.SelectionFont != null)
                rtbReportEditor.SelectionFont = new Font(fontName, fontSize, rtbReportEditor.SelectionFont.Style);
            else
                rtbReportEditor.Font = new Font(fontName, fontSize);
        }

        private void ApplyStyle(FontStyle style)
        {
            if (rtbReportEditor.SelectionLength > 0 && rtbReportEditor.SelectionFont != null)
            {
                var f = rtbReportEditor.SelectionFont;
                rtbReportEditor.SelectionFont = new Font(f.FontFamily, f.Size, f.Style ^ style);
            }
        }

        private void ApplyBullets()
        {
            rtbReportEditor.SelectionBullet = !rtbReportEditor.SelectionBullet;
        }

        private void ApplyNumbering()
        {
            string[] lines = (rtbReportEditor.SelectedText.Length > 0 ? rtbReportEditor.SelectedText : rtbReportEditor.Text)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            rtbReportEditor.SelectedText = string.Join(Environment.NewLine, lines.Select((l, i) => $"{i + 1}. {l.TrimStart()}"));
        }

        private void ApplyAlignment(HorizontalAlignment alignment)
        {
            rtbReportEditor.SelectionAlignment = alignment;
        }

        private void ApplyTextColor()
        {
            using (var dlg = new ColorDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    rtbReportEditor.SelectionColor = dlg.Color;
            }
        }

        private void ApplyHighlight()
        {
            using (var dlg = new ColorDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    rtbReportEditor.SelectionBackColor = dlg.Color;
            }
        }

        private void InsertImageIntoReport()
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                    Helper.InsertImageIntoReport(rtbReportEditor, ofd.FileName);
            }
        }

        private void PrintReport()
        {
            if (string.IsNullOrWhiteSpace(rtbReportEditor.Text))
            {
                MessageBox.Show("Report is empty. Nothing to print.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Create a PrintDocument
                System.Drawing.Printing.PrintDocument printDoc = new System.Drawing.Printing.PrintDocument();
                
                // Store the current position in the RichTextBox
                int checkPrint = 0;

                printDoc.PrintPage += (sender, e) =>
                {
                    // Print the RichTextBox content
                    checkPrint = rtbReportEditor.Print(checkPrint, rtbReportEditor.TextLength, e);

                    // Check if there are more pages to print
                    if (checkPrint < rtbReportEditor.TextLength)
                        e.HasMorePages = true;
                    else
                        e.HasMorePages = false;
                };

                // Show print dialog
                System.Windows.Forms.PrintDialog printDialog = new System.Windows.Forms.PrintDialog();
                printDialog.Document = printDoc;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSelectedReport(ListBox lstReports, TextBox txtReportTitle)
        {
            if (lstReports.SelectedItem is Report report)
            {
                txtReportTitle.Text = report.Title ?? "";
                if (!string.IsNullOrEmpty(report.Content))
                {
                    try { rtbReportEditor.Rtf = report.Content; }
                    catch { rtbReportEditor.Text = report.Content; }
                }
                else rtbReportEditor.Clear();
            }
            else
            {
                txtReportTitle.Text = "";
                rtbReportEditor.Clear();
            }
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
            lstReports.SelectedItem = report;
            txtReportTitle.Text = report.Title;
        }

        private void SaveCurrentReport(string title, ListBox lstReports)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var report = lstReports.SelectedItem as Report ?? new Report { Id = nextReportId++, DateCreated = DateTime.Now };
            if (!selectedClient.Reports.Contains(report))
                selectedClient.Reports.Add(report);

            report.Title = string.IsNullOrWhiteSpace(title) ? "Untitled Report" : title;
            report.Content = rtbReportEditor.Rtf;
            report.DateModified = DateTime.Now;

            SaveData();
            RefreshReportList(lstReports);
            lstReports.SelectedItem = report;
            MessageBox.Show("Report saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportReport(string title)
        {
            if (string.IsNullOrWhiteSpace(rtbReportEditor.Text))
            {
                MessageBox.Show("Report is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Text Files|*.txt|Rich Text Format|*.rtf", FileName = title + ".txt" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase))
                        rtbReportEditor.SaveFile(sfd.FileName, RichTextBoxStreamType.RichText);
                    else
                        File.WriteAllText(sfd.FileName, rtbReportEditor.Text);
                }
            }
        }

        private void ExportReportToPdf(string title)
        {
            if (string.IsNullOrWhiteSpace(rtbReportEditor.Text))
            {
                MessageBox.Show("Report is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = (string.IsNullOrWhiteSpace(title) ? "Report" : title) + ".pdf"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Create PDF document
                        PdfWriter writer = new PdfWriter(sfd.FileName);
                        PdfDocument pdf = new PdfDocument(writer);
                        Document document = new Document(pdf);

                        // Add title
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            Paragraph titlePara = new Paragraph(title);
                            titlePara.SetFontSize(18);
                            document.Add(titlePara);
                            document.Add(new Paragraph("\n"));
                        }

                        // Extract and add content with images
                        AddRichTextContentToPdf(document, rtbReportEditor);

                        // Close document
                        document.Close();

                        MessageBox.Show("Report exported to PDF successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting PDF: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void AddRichTextContentToPdf(Document document, RichTextBox rtb)
        {
            try
            {
                // First, add all the text content
                string plainText = rtb.Text;
                
                if (!string.IsNullOrEmpty(plainText))
                {
                    // Split into paragraphs and add them
                    string[] paragraphs = plainText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (string para in paragraphs)
                    {
                        if (!string.IsNullOrWhiteSpace(para))
                        {
                            document.Add(new Paragraph(para.Trim()));
                        }
                    }
                }

                // Now try to extract images from RTF using Windows Forms rendering
                // Save RTF content
                string tempRtfPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".rtf");
                rtb.SaveFile(tempRtfPath, RichTextBoxStreamType.RichText);
                
                // Read RTF content to check for images
                string rtfContent = File.ReadAllText(tempRtfPath);
                
                // Check if RTF contains embedded images (pngblip, jpegblip, etc.)
                if (rtfContent.Contains("\\pngblip") || rtfContent.Contains("\\jpegblip") || 
                    rtfContent.Contains("\\emfblip") || rtfContent.Contains("\\wmetafile"))
                {
                    // Use a different approach: render the RichTextBox to extract images
                    using (RichTextBox tempRtb = new RichTextBox())
                    {
                        tempRtb.LoadFile(tempRtfPath, RichTextBoxStreamType.RichText);
                        
                        // Try to extract images by selecting and copying each character
                        bool foundImage = false;
                        for (int i = 0; i < tempRtb.TextLength; i++)
                        {
                            tempRtb.Select(i, 1);
                            
                            // Check if this position contains an object (image)
                            if (tempRtb.SelectionType == RichTextBoxSelectionTypes.Object ||
                                tempRtb.SelectionType == (RichTextBoxSelectionTypes.Text | RichTextBoxSelectionTypes.Object))
                            {
                                try
                                {
                                    // Clear clipboard and copy
                                    Clipboard.Clear();
                                    System.Threading.Thread.Sleep(50); // Small delay for clipboard
                                    tempRtb.Copy();
                                    System.Threading.Thread.Sleep(50); // Small delay for clipboard
                                    
                                    if (Clipboard.ContainsImage())
                                    {
                                        foundImage = true;
                                        using (System.Drawing.Image img = Clipboard.GetImage())
                                        {
                                            if (img != null)
                                            {
                                                // Save image to temp file
                                                string tempImgPath = Path.Combine(Path.GetTempPath(), 
                                                    Guid.NewGuid().ToString() + ".png");
                                                img.Save(tempImgPath, System.Drawing.Imaging.ImageFormat.Png);
                                                
                                                // Add to PDF
                                                try
                                                {
                                                    iText.Layout.Element.Image pdfImage = 
                                                        new iText.Layout.Element.Image(
                                                            iText.IO.Image.ImageDataFactory.Create(tempImgPath));
                                                    
                                                    // Scale to fit page
                                                    float maxWidth = document.GetPdfDocument().GetDefaultPageSize().GetWidth() - 100;
                                                    if (pdfImage.GetImageWidth() > maxWidth)
                                                    {
                                                        pdfImage.ScaleToFit(maxWidth, 600);
                                                    }
                                                    
                                                    document.Add(new Paragraph("\n")); // Add spacing
                                                    document.Add(pdfImage);
                                                    document.Add(new Paragraph("\n")); // Add spacing
                                                }
                                                finally
                                                {
                                                    // Clean up temp image
                                                    try { File.Delete(tempImgPath); } catch { }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception imgEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error extracting image: {imgEx.Message}");
                                    // Continue to next character
                                }
                            }
                        }
                        
                        if (!foundImage && (rtfContent.Contains("\\pngblip") || rtfContent.Contains("\\jpegblip")))
                        {
                            // Fallback: Try to extract images from RTF hex data
                            ExtractImagesFromRtfHex(document, rtfContent);
                        }
                    }
                }
                
                // Clean up temp file
                try { File.Delete(tempRtfPath); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Warning: Some images may not have been exported.\n\nError: {ex.Message}",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ExtractImagesFromRtfHex(Document document, string rtfContent)
        {
            try
            {
                // Look for PNG images in RTF (they are stored as hex)
                int pngIndex = rtfContent.IndexOf("\\pngblip");
                
                while (pngIndex != -1)
                {
                    // Find the start of hex data (after \pngblip)
                    int hexStart = rtfContent.IndexOf(' ', pngIndex) + 1;
                    int hexEnd = rtfContent.IndexOf('}', hexStart);
                    
                    if (hexEnd > hexStart)
                    {
                        string hexData = rtfContent.Substring(hexStart, hexEnd - hexStart);
                        // Remove any RTF formatting codes
                        hexData = System.Text.RegularExpressions.Regex.Replace(hexData, @"\\[a-z]+\d*\s?", "");
                        hexData = hexData.Replace("\r", "").Replace("\n", "").Replace(" ", "");
                        
                        try
                        {
                            // Convert hex to bytes
                            byte[] imageBytes = new byte[hexData.Length / 2];
                            for (int i = 0; i < imageBytes.Length; i++)
                            {
                                imageBytes[i] = Convert.ToByte(hexData.Substring(i * 2, 2), 16);
                            }
                            
                            // Create image from bytes
                            string tempImgPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
                            File.WriteAllBytes(tempImgPath, imageBytes);
                            
                            try
                            {
                                // Add to PDF
                                iText.Layout.Element.Image pdfImage = new iText.Layout.Element.Image(
                                    iText.IO.Image.ImageDataFactory.Create(tempImgPath));
                                
                                float maxWidth = document.GetPdfDocument().GetDefaultPageSize().GetWidth() - 100;
                                if (pdfImage.GetImageWidth() > maxWidth)
                                {
                                    pdfImage.ScaleToFit(maxWidth, 600);
                                }
                                
                                document.Add(new Paragraph("\n"));
                                document.Add(pdfImage);
                                document.Add(new Paragraph("\n"));
                            }
                            finally
                            {
                                try { File.Delete(tempImgPath); } catch { }
                            }
                        }
                        catch
                        {
                            // Skip this image if conversion fails
                        }
                    }
                    
                    // Look for next image
                    pngIndex = rtfContent.IndexOf("\\pngblip", hexEnd);
                }
            }
            catch
            {
                // Silently fail - this is a fallback method
            }
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

            // Create 2-column vertical layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // ===== LEFT COLUMN: TO-DO LIST =====
            Panel todoColumn = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };

            Label lblTodo = new Label
            {
                Text = "To-Do List",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // REDUCED panel height and moved all controls UP by ~1 inch (96 pixels)
            Panel addTodoPanel = new Panel { Dock = DockStyle.Bottom, Height = 130, Padding = new Padding(5) };

            // All Y coordinates REDUCED by 96 pixels to move UP
            Label lblTask = new Label { Text = "Task:", Location = new Point(5, 5), Height = 14, Width = 80 };
            txtTodoItem = new TextBox
            {
                Location = new Point(5, 20),  // Was 25, now 20 (moved up 5px to fit better)
                Width = 300,
                Font = new Font("Arial", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblDueDate = new Label { Text = "Due Date:", Location = new Point(5, 45), Height = 14, Width = 80 };  // Was 55
            DateTimePicker dtpTodoDueDate = new DateTimePicker
            {
                Location = new Point(5, 65),  // Was 75
                Width = 145,
                Format = DateTimePickerFormat.Short
            };

            Label lblDueTime = new Label { Text = "Time:", Location = new Point(160, 45), Height = 14, Width = 50 };  // Was 55
            DateTimePicker dtpTodoTime = new DateTimePicker
            {
                Location = new Point(160, 65),  // Was 75
                Width = 145,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            CheckBox chkNoDueDate = new CheckBox
            {
                Text = "No Due Date",
                Location = new Point(315, 45),  // Was 55
                Width = 100,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            chkNoDueDate.CheckedChanged += (s, e) =>
            {
                dtpTodoDueDate.Enabled = !chkNoDueDate.Checked;
                dtpTodoTime.Enabled = !chkNoDueDate.Checked;
            };

            Button btnAddTodo = new Button
            {
                Text = "Add To-Do",
                Location = new Point(5, 95),  // Was 105
                Width = 150,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            btnAddTodo.Click += (s, e) =>
            {
                if (selectedClient == null)
                {
                    MessageBox.Show("Please select a client first.", "No Client Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtTodoItem.Text))
                {
                    MessageBox.Show("Please enter a task description.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create the todo item with or without due date based on checkbox
                TodoItem todo;
                if (chkNoDueDate.Checked)
                {
                    // No due date
                    todo = new TodoItem
                    {
                        Id = nextTodoId++,
                        Task = txtTodoItem.Text,
                        IsCompleted = false,
                        DateAdded = DateTime.Now,
                        DueDate = null
                    };
                }
                else
                {
                    // Combine date and time
                    DateTime dueDateTime = dtpTodoDueDate.Value.Date.Add(dtpTodoTime.Value.TimeOfDay);
                    todo = new TodoItem
                    {
                        Id = nextTodoId++,
                        Task = txtTodoItem.Text,
                        IsCompleted = false,
                        DateAdded = DateTime.Now,
                        DueDate = dueDateTime
                    };
                }

                selectedClient.TodoItems.Add(todo);
                SaveData();
                RefreshTodoList();
                RefreshCalendarDates();

                // Clear the form
                txtTodoItem.Text = "";
                dtpTodoDueDate.Value = DateTime.Now;
                dtpTodoTime.Value = DateTime.Now;
                chkNoDueDate.Checked = false;
            };

            addTodoPanel.Controls.AddRange(new Control[] {
                lblTask, txtTodoItem, lblDueDate, dtpTodoDueDate,
                lblDueTime, dtpTodoTime, chkNoDueDate, btnAddTodo
            });

            // CHANGED: Added a spacer panel to reduce the listbox height by 1/3
            // This panel will take up 1/3 of the remaining space
            Panel spacerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 0  // Will be calculated dynamically
            };

            // Use resize event to set spacer to 1/3 of available height
            todoColumn.Resize += (s, e) =>
            {
                // Calculate available height (total - title - add panel)
                int availableHeight = todoColumn.Height - lblTodo.Height - addTodoPanel.Height - 10;
                // Make spacer 1/3 of available height
                spacerPanel.Height = availableHeight / 3;
            };

            lstTodoItems = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 9),
                DrawMode = DrawMode.OwnerDrawVariable
            };
            lstTodoItems.DrawItem += LstTodoItems_DrawItem;
            lstTodoItems.MeasureItem += LstTodoItems_MeasureItem;
            lstTodoItems.DoubleClick += LstTodoItems_DoubleClick;

            // NEW: Add single click event to navigate to the todo item's date
            lstTodoItems.SelectedIndexChanged += (s, e) =>
            {
                if (lstTodoItems.SelectedItem is TodoItem selectedTodo && selectedTodo.DueDate.HasValue)
                {
                    // Navigate the calendar to the todo item's due date
                    calClientCalendar.SetDate(selectedTodo.DueDate.Value);
                    // This will trigger the DateChanged event which refreshes the day tasks
                }
            };

            // Context menu for to-do items
            ContextMenuStrip todoContextMenu = new ContextMenuStrip();
            ToolStripMenuItem markCompleteItem = new ToolStripMenuItem("Toggle Complete");
            markCompleteItem.Click += (s, e) =>
            {
                if (lstTodoItems.SelectedItem is TodoItem todo)
                {
                    todo.IsCompleted = !todo.IsCompleted;
                    SaveData();
                    RefreshTodoList();
                }
            };

            ToolStripMenuItem deleteTodoItem = new ToolStripMenuItem("Delete");
            deleteTodoItem.Click += (s, e) =>
            {
                if (lstTodoItems.SelectedItem is TodoItem todo)
                {
                    var result = MessageBox.Show($"Delete task '{todo.Task}'?", "Confirm Delete",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        selectedClient.TodoItems.Remove(todo);
                        SaveData();
                        RefreshTodoList();
                        RefreshCalendarDates();
                    }
                }
            };

            ToolStripMenuItem exportToGoogleItem = new ToolStripMenuItem("Export to Google Calendar");
            exportToGoogleItem.Click += (s, e) =>
            {
                if (lstTodoItems.SelectedItem is TodoItem todo)
                {
                    ExportTodoToGoogleCalendar(todo);
                }
            };

            todoContextMenu.Items.AddRange(new ToolStripItem[] { markCompleteItem, deleteTodoItem, new ToolStripSeparator(), exportToGoogleItem });
            lstTodoItems.ContextMenuStrip = todoContextMenu;

            // Add controls in the correct order (bottom to top for docked bottom controls)
            todoColumn.Controls.Add(lstTodoItems);
            todoColumn.Controls.Add(spacerPanel);  // NEW: Spacer panel reduces listbox height
            todoColumn.Controls.Add(addTodoPanel);
            todoColumn.Controls.Add(lblTodo);

            // ===== RIGHT COLUMN: CALENDAR =====
            Panel calendarColumn = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };

            Label lblCalendar = new Label
            {
                Text = "Calendar",
                Dock = DockStyle.Top,
                Height = 20,
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            calClientCalendar = new MonthCalendar
            {
                Dock = DockStyle.Top,
                MaxSelectionCount = 1
            };
            calClientCalendar.DateChanged += CalClientCalendar_DateChanged;

            // Panel to show tasks for selected date
            Panel selectedDatePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblSelectedDate = new Label
            {
                Text = "Tasks for Selected Date:",
                Dock = DockStyle.Top,
                Height = 20,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            ListBox lstDayTasks = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 9),
                DrawMode = DrawMode.OwnerDrawVariable,
                Name = "lstDayTasks"
            };
            lstDayTasks.DrawItem += LstDayTasks_DrawItem;
            lstDayTasks.MeasureItem += LstDayTasks_MeasureItem;

            Button btnExportDay = new Button
            {
                Text = "Export All to Google Calendar",
                Dock = DockStyle.Bottom,
                Height = 35,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnExportDay.Click += (s, e) => ExportDayToGoogleCalendar(calClientCalendar.SelectionStart, lstDayTasks);

            selectedDatePanel.Controls.Add(lstDayTasks);
            selectedDatePanel.Controls.Add(btnExportDay);
            selectedDatePanel.Controls.Add(lblSelectedDate);

            calendarColumn.Controls.Add(selectedDatePanel);
            calendarColumn.Controls.Add(calClientCalendar);
            calendarColumn.Controls.Add(lblCalendar);

            // Add columns to main layout
            mainLayout.Controls.Add(todoColumn, 0, 0);
            mainLayout.Controls.Add(calendarColumn, 1, 0);

            mainPanel.Controls.Add(mainLayout);
            mainPanel.Controls.Add(lblSelectClient);
            tab.Controls.Add(mainPanel);
        }
        private void DisplayEventDetails(ParanormalEvent evt)
        {
            if (rtbEventDetails == null || evt == null)
                return;

            rtbEventDetails.Clear();
            rtbEventDetails.SelectionFont = new Font("Arial", 14, FontStyle.Bold);
            rtbEventDetails.SelectionColor = Color.DarkBlue;
            rtbEventDetails.AppendText($"{evt.EventName}\n");

            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Regular);
            rtbEventDetails.SelectionColor = Color.Black;
            rtbEventDetails.AppendText("=".PadRight(50, '=') + "\n\n");

            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Bold);
            rtbEventDetails.AppendText("Event Type: ");
            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Regular);
            rtbEventDetails.AppendText($"{evt.EventType ?? "N/A"}\n\n");

            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Bold);
            rtbEventDetails.AppendText("Date: ");
            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Regular);
            rtbEventDetails.AppendText($"{evt.EventDate.ToLongDateString()} at {evt.EventDate.ToShortTimeString()}\n\n");

            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Bold);
            rtbEventDetails.AppendText("Location: ");
            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Regular);
            rtbEventDetails.AppendText($"{evt.Location ?? "N/A"}\n\n");

            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Bold);
            rtbEventDetails.AppendText("Contact Person: ");
            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Regular);
            rtbEventDetails.AppendText($"{evt.ContactPerson ?? "N/A"}\n\n");

            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Bold);
            rtbEventDetails.AppendText("Contact Phone: ");
            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Regular);
            rtbEventDetails.AppendText($"{evt.ContactTelephone ?? "N/A"}\n\n");

            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Bold);
            rtbEventDetails.AppendText("Price: ");
            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Regular);
            rtbEventDetails.SelectionColor = Color.DarkGreen;
            rtbEventDetails.AppendText($"${evt.Price:N2}\n\n");
            rtbEventDetails.SelectionColor = Color.Black;

            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Bold);
            rtbEventDetails.AppendText("Description:\n");
            rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Regular);
            rtbEventDetails.AppendText($"{evt.Description ?? "No description provided."}\n\n");

            rtbEventDetails.SelectionFont = new Font("Arial", 9, FontStyle.Italic);
            rtbEventDetails.SelectionColor = Color.Gray;
            rtbEventDetails.AppendText($"Created: {evt.DateCreated.ToShortDateString()}");
        }

        private void ClearEventDetails()
        {
            if (rtbEventDetails != null && selectedEvent == null)
            {
                rtbEventDetails.Clear();
                rtbEventDetails.SelectionFont = new Font("Arial", 10, FontStyle.Italic);
                rtbEventDetails.SelectionColor = Color.Gray;
                rtbEventDetails.AppendText("Select an event from the list to view details...");
            }
        }

        private void FilterAllEvents(string searchText)
        {
            if (lstEvents == null)
                return;

            lstEvents.Items.Clear();

            if (string.IsNullOrWhiteSpace(searchText) ||
                searchText == "Search by event name, type, location, or description...")
            {
                RefreshEventList();
                return;
            }

            string lower = searchText.ToLowerInvariant();
            var allMatchingEvents = new List<(Client client, ParanormalEvent evt)>();

            foreach (var client in clients)
            {
                var matchingEvents = client.ParanormalEvents
                    .Where(evt =>
                        (evt.EventName ?? "").ToLowerInvariant().Contains(lower) ||
                        (evt.EventType ?? "").ToLowerInvariant().Contains(lower) ||
                        (evt.Location ?? "").ToLowerInvariant().Contains(lower) ||
                        (evt.Description ?? "").ToLowerInvariant().Contains(lower) ||
                        evt.EventDate.ToString().ToLowerInvariant().Contains(lower))
                    .Select(evt => (client, evt));

                allMatchingEvents.AddRange(matchingEvents);
            }

            var sortedEvents = allMatchingEvents.OrderByDescending(x => x.evt.EventDate);

            foreach (var item in sortedEvents)
            {
                lstEvents.Items.Add($"{item.evt.EventDate.ToShortDateString()} - {item.evt.EventName} ({item.evt.EventType}) - Client: {item.client.Name}");
            }

            if (lstEvents.Items.Count == 0)
            {
                lstEvents.Items.Add("No events found matching your search.");
            }
        }

        private void RefreshEventList()
        {
            if (lstEvents == null)
                return;

            lstEvents.Items.Clear();
            if (selectedClient != null)
            {
                foreach (var ev in selectedClient.ParanormalEvents.OrderByDescending(e => e.EventDate))
                {
                    lstEvents.Items.Add($"{ev.EventDate.ToShortDateString()} - {ev.EventName} ({ev.EventType})");
                }
            }
        }

        private void BtnExportToGoogleCalendar_Click(object sender, EventArgs e)
        {
            if (selectedEvent == null)
            {
                MessageBox.Show("Please select an event to export.", "No Event Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string startDate = selectedEvent.EventDate.ToString("yyyyMMdd'T'HHmmss");
                string endDate = selectedEvent.EventDate.AddHours(2).ToString("yyyyMMdd'T'HHmmss");

                StringBuilder description = new StringBuilder();
                if (!string.IsNullOrEmpty(selectedEvent.Description))
                    description.Append(selectedEvent.Description);
                if (!string.IsNullOrEmpty(selectedEvent.EventType))
                {
                    if (description.Length > 0) description.Append("\n\n");
                    description.Append($"Type: {selectedEvent.EventType}");
                }
                if (!string.IsNullOrEmpty(selectedEvent.ContactPerson))
                {
                    if (description.Length > 0) description.Append("\n");
                    description.Append($"Contact: {selectedEvent.ContactPerson}");
                }
                if (!string.IsNullOrEmpty(selectedEvent.ContactTelephone))
                {
                    if (description.Length > 0) description.Append("\n");
                    description.Append($"Phone: {selectedEvent.ContactTelephone}");
                }
                if (selectedEvent.Price > 0)
                {
                    if (description.Length > 0) description.Append("\n");
                    description.Append($"Price: ${selectedEvent.Price:N2}");
                }

                StringBuilder googleCalUrl = new StringBuilder("https://calendar.google.com/calendar/render?action=TEMPLATE");
                googleCalUrl.Append($"&text={Uri.EscapeDataString(selectedEvent.EventName ?? "Event")}");
                googleCalUrl.Append($"&dates={startDate}/{endDate}");
                
                if (!string.IsNullOrEmpty(selectedEvent.Location))
                    googleCalUrl.Append($"&location={Uri.EscapeDataString(selectedEvent.Location)}");
                
                if (description.Length > 0)
                    googleCalUrl.Append($"&details={Uri.EscapeDataString(description.ToString())}");

                googleCalUrl.Append($"&ctz={Uri.EscapeDataString(TimeZoneInfo.Local.Id)}");

                string url = googleCalUrl.ToString();

                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                    MessageBox.Show("Opening Google Calendar in your browser...", "Google Calendar",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open browser.\n\nURL:\n{url}\n\nError: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    try
                    {
                        Clipboard.SetText(url);
                        MessageBox.Show("URL copied to clipboard.", "URL Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Google Calendar link: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportTodoToGoogleCalendar(TodoItem todo)
        {
            if (selectedClient == null || todo == null)
                return;

            try
            {
                string clientName = selectedClient.Name ?? "Unknown Client";
                DateTime startDate = todo.DueDate ?? DateTime.Now;
                DateTime endDate = startDate.AddHours(1);

                StringBuilder googleCalUrl = new StringBuilder("https://calendar.google.com/calendar/render?action=TEMPLATE");
                googleCalUrl.Append($"&text={Uri.EscapeDataString($"{clientName} - {todo.Task}")}");
                googleCalUrl.Append($"&dates={startDate:yyyyMMdd'T'HHmmss}/{endDate:yyyyMMdd'T'HHmmss}");
                googleCalUrl.Append($"&details={Uri.EscapeDataString($"Client: {clientName}\nTask: {todo.Task}")}");
                googleCalUrl.Append($"&ctz={Uri.EscapeDataString(TimeZoneInfo.Local.Id)}");

                string url = googleCalUrl.ToString();

                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                    MessageBox.Show("Opening Google Calendar in your browser...", "Google Calendar",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open browser.\n\nURL:\n{url}\n\nError: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    try
                    {
                        Clipboard.SetText(url);
                        MessageBox.Show("URL copied to clipboard.", "URL Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Google Calendar link: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void RefreshReports()
        {
            var reportsTab = tabControl.TabPages["Reports"];
            if (reportsTab?.Tag != null)
            {
                var refs = reportsTab.Tag as dynamic;
                if (refs != null)
                {
                    RefreshReportList(refs.ReportList);
                }
            }
        }

        private void RefreshReportList(ListBox lstReports)
        {
            lstReports.Items.Clear();
            if (selectedClient != null)
            {
                lstReports.DisplayMember = "Title";
                foreach (var report in selectedClient.Reports.OrderByDescending(r => r.DateModified))
                {
                    lstReports.Items.Add(report);
                }
            }
        }

        private void DeleteReport(ListBox lstReports, TextBox txtReportTitle)
        {
            if (selectedClient == null || lstReports.SelectedItem == null)
            {
                MessageBox.Show("Please select a report to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var report = lstReports.SelectedItem as Report;
            var result = MessageBox.Show($"Are you sure you want to delete '{report.Title}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                selectedClient.Reports.Remove(report);
                SaveData();
                RefreshReportList(lstReports);
                txtReportTitle.Text = "";
                rtbReportEditor.Clear();
            }
        }

        private void ShowImagePreview(string imagePath, PictureBox sourcePictureBox)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return;

            try
            {
                if (imagePreviewForm != null && !imagePreviewForm.IsDisposed)
                    imagePreviewForm.Close();

                imagePreviewForm = new Form
                {
                    FormBorderStyle = FormBorderStyle.None,
                    StartPosition = FormStartPosition.Manual,
                    BackColor = Color.Black,
                    ShowInTaskbar = false,
                    TopMost = true
                };

                PictureBox previewPictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = System.Drawing.Image.FromFile(imagePath)
                };

                imagePreviewForm.Controls.Add(previewPictureBox);

                Point screenPoint = sourcePictureBox.PointToScreen(new Point(sourcePictureBox.Width + 10, 0));
                imagePreviewForm.Location = screenPoint;
                imagePreviewForm.Size = new Size(400, 400);

                imagePreviewForm.Show();
            }
            catch { }
        }

        private void HideImagePreview()
        {
            if (imagePreviewForm != null && !imagePreviewForm.IsDisposed)
            {
                imagePreviewForm.Close();
                imagePreviewForm = null;
            }
        }

        private void RefreshTodoList()
        {
            if (lstTodoItems == null)
                return;

            lstTodoItems.Items.Clear();
            if (selectedClient != null)
            {
                foreach (var todo in selectedClient.TodoItems
                    .OrderBy(t => t.IsCompleted)
                    .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
                    .ThenByDescending(t => t.DateAdded))
                {
                    lstTodoItems.Items.Add(todo);
                }

                RefreshCalendarDates();
            }
        }

        private void RefreshCalendarDates()
        {
            if (calClientCalendar != null && selectedClient != null)
            {
                var datesWithTasks = selectedClient.TodoItems
                    .Where(t => t.DueDate.HasValue)
                    .Select(t => t.DueDate.Value.Date)
                    .Distinct()
                    .ToArray();

                calClientCalendar.BoldedDates = datesWithTasks;
            }
        }

        private void RefreshCalendarEvents()
        {
            RefreshCalendarDates();
            if (calClientCalendar != null)
            {
                RefreshDayTasks(calClientCalendar.SelectionStart);
            }
        }

        private void RefreshDayTasks(DateTime selectedDate)
        {
            if (selectedClient == null)
                return;

            var calendarTab = tabControl.TabPages["To-Do & Calendar"];
            if (calendarTab == null)
                return;

            ListBox lstDayTasks = FindControl<ListBox>(calendarTab, "lstDayTasks");
            if (lstDayTasks == null)
                return;

            lstDayTasks.Items.Clear();

            var tasksForDay = selectedClient.TodoItems
                .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == selectedDate.Date)
                .OrderBy(t => t.DueDate.Value.TimeOfDay)
                .ToList();

            foreach (var task in tasksForDay)
            {
                lstDayTasks.Items.Add(task);
            }

            if (tasksForDay.Count == 0)
            {
                lstDayTasks.Items.Add("No tasks for this date");
            }
        }

        private T FindControl<T>(Control parent, string name) where T : Control
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is T && ctrl.Name == name)
                    return ctrl as T;

                T found = FindControl<T>(ctrl, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void LstDayTasks_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            ListBox lst = sender as ListBox;
            var item = lst.Items[e.Index] as TodoItem;
            if (item == null)
                return;

            e.DrawBackground();

            Font taskFont = item.IsCompleted ? new Font(e.Font, FontStyle.Strikeout) : new Font(e.Font, FontStyle.Bold);
            Brush taskBrush = item.IsCompleted ? Brushes.Gray : Brushes.Black;

            e.Graphics.DrawString(item.Task, taskFont, taskBrush, new PointF(e.Bounds.Left + 5, e.Bounds.Top + 3));

            if (item.DueDate.HasValue)
            {
                string timeText = item.DueDate.Value.ToString("h:mm tt");
                Font timeFont = new Font(e.Font.FontFamily, 8, FontStyle.Italic);
                Brush timeBrush = item.IsCompleted ? Brushes.Gray : Brushes.DarkBlue;

                if (!item.IsCompleted && item.DueDate.Value < DateTime.Now)
                    timeBrush = Brushes.Red;

                e.Graphics.DrawString(timeText, timeFont, timeBrush, new PointF(e.Bounds.Left + 5, e.Bounds.Top + 22));
            }

            e.DrawFocusRectangle();
        }

        private void LstDayTasks_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 40;
        }

        private void LstTodoItems_DoubleClick(object sender, EventArgs e)
        {
            if (lstTodoItems.SelectedItem is TodoItem todo)
            {
                todo.IsCompleted = !todo.IsCompleted;
                SaveData();
                RefreshTodoList();
            }
        }

        private void CalClientCalendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            RefreshDayTasks(e.Start);
        }

        private void ExportDayToGoogleCalendar(DateTime selectedDate, ListBox lstDayTasks)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "No Client Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tasksForDay = selectedClient.TodoItems
                .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == selectedDate.Date && !t.IsCompleted)
                .ToList();

            if (tasksForDay.Count == 0)
            {
                MessageBox.Show("No tasks to export for this date.", "No Tasks",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (var task in tasksForDay)
            {
                ExportTodoToGoogleCalendar(task);
                System.Threading.Thread.Sleep(500);
            }
        }

        private void CreateEventPlanningTab(TabPage tab)
        {
            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Label lblSelectClient = new Label
            {
                Text = "View events for selected client or search all events across all clients",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 10, FontStyle.Italic)
            };

            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(5)
            };

            Label lblSearch = new Label
            {
                Text = "Search All Events:",
                Location = new Point(5, 12),
                Width = 110,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            TextBox txtEventSearch = new TextBox
            {
                Location = new Point(120, 10),
                Width = 300,
                Font = new Font("Arial", 10)
            };

            txtEventSearch.ForeColor = Color.Gray;
            txtEventSearch.Text = "Search by event name, type, location, or description...";
            txtEventSearch.GotFocus += (s, e) =>
            {
                if (txtEventSearch.Text == "Search by event name, type, location, or description...")
                {
                    txtEventSearch.Text = "";
                    txtEventSearch.ForeColor = Color.Black;
                }
            };
            txtEventSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtEventSearch.Text))
                {
                    txtEventSearch.Text = "Search by event name, type, location, or description...";
                    txtEventSearch.ForeColor = Color.Gray;
                }
            };

            txtEventSearch.TextChanged += (s, e) =>
            {
                FilterAllEvents(txtEventSearch.Text);
            };

            Button btnClearEventSearch = new Button
            {
                Text = "Clear",
                Location = new Point(430, 9),
                Width = 60,
                Height = 25
            };
            btnClearEventSearch.Click += (s, e) =>
            {
                txtEventSearch.Text = "";
                txtEventSearch.ForeColor = Color.Black;
                RefreshEventList();
            };

            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtEventSearch, btnClearEventSearch });

            Panel contentPanel = new Panel { Dock = DockStyle.Fill };
            Panel column1 = new Panel { Dock = DockStyle.Left, Width = 350, Padding = new Padding(5) };

            Label lblEventsList = new Label
            {
                Text = "Events",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lstEvents = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 9)
            };

            lstEvents.SelectedIndexChanged += (s, e) =>
            {
                if (lstEvents.SelectedIndex >= 0 && selectedClient != null)
                {
                    var eventItem = selectedClient.ParanormalEvents
                        .OrderByDescending(ev => ev.EventDate)
                        .ElementAtOrDefault(lstEvents.SelectedIndex);

                    if (eventItem != null)
                    {
                        selectedEvent = eventItem;
                        DisplayEventDetails(eventItem);
                    }
                }
                else
                {
                    selectedEvent = null;
                    ClearEventDetails();
                }
            };

            column1.Controls.Add(lstEvents);
            column1.Controls.Add(lblEventsList);

            Panel column2 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), AutoScroll = true };

            Label lblFormTitle = new Label
            {
                Text = "Add New Event",
                Location = new Point(10, 10),
                Width = 200,
                Font = new Font("Arial", 11, FontStyle.Bold),
                AutoSize = false
            };

            Label lblEventName = new Label { Text = "Event Name:", Location = new Point(10, 45), Width = 100 };
            txtEventName = new TextBox { Location = new Point(120, 45), Width = 200 };

            Label lblEventType = new Label { Text = "Type:", Location = new Point(10, 75), Width = 100 };
            cmbEventType = new ComboBox
            {
                Location = new Point(120, 75),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEventType.Items.AddRange(new object[] {
                "Investigation", "Public Event", "Private Event", "Lecture", "Workshop", "Other"
            });

            Label lblEventDate = new Label { Text = "Date:", Location = new Point(10, 105), Width = 100 };
            dtpEventDate = new DateTimePicker { Location = new Point(120, 105), Width = 200 };

            Label lblEventLocation = new Label { Text = "Location:", Location = new Point(10, 135), Width = 100 };
            txtEventLocation = new TextBox { Location = new Point(120, 135), Width = 200 };

            Label lblContactPerson = new Label { Text = "Contact Person:", Location = new Point(10, 165), Width = 100 };
            txtContactPerson = new TextBox { Location = new Point(120, 165), Width = 200 };

            Label lblContactTelephone = new Label { Text = "Contact Phone:", Location = new Point(10, 195), Width = 100 };
            txtContactTelephone = new TextBox { Location = new Point(120, 195), Width = 200 };

            Label lblPrice = new Label { Text = "Price ($):", Location = new Point(10, 225), Width = 100 };
            nudPrice = new NumericUpDown
            {
                Location = new Point(120, 225),
                Width = 200,
                DecimalPlaces = 2,
                Maximum = 999999,
                Minimum = 0,
                ThousandsSeparator = true
            };

            Label lblEventDescription = new Label { Text = "Description:", Location = new Point(10, 255), Width = 100 };
            txtEventDescription = new TextBox { Location = new Point(120, 255), Width = 200, Height = 60, Multiline = true };

            Button btnAddEvent = new Button
            {
                Text = "Add Event",
                Location = new Point(10, 325),
                Width = 100,
                Height = 35
            };
            btnAddEvent.Click += (s, e) =>
            {
                if (selectedClient == null)
                {
                    MessageBox.Show("Please select a client first from the Clients tab.", "No Client Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEventName.Text))
                {
                    MessageBox.Show("Please enter an event name.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEventName.Focus();
                    return;
                }

                var ev = new ParanormalEvent
                {
                    Id = nextParanormalEventId++,
                    EventName = txtEventName.Text,
                    EventType = cmbEventType.SelectedItem?.ToString() ?? "Other",
                    EventDate = dtpEventDate.Value,
                    Location = txtEventLocation.Text,
                    Description = txtEventDescription.Text,
                    ContactPerson = txtContactPerson.Text,
                    ContactTelephone = txtContactTelephone.Text,
                    Price = nudPrice.Value,
                    DateCreated = DateTime.Now
                };
                selectedClient.ParanormalEvents.Add(ev);
                SaveData();
                RefreshEventList();

                txtEventName.Text = "";
                txtEventLocation.Text = "";
                txtEventDescription.Text = "";
                txtContactPerson.Text = "";
                txtContactTelephone.Text = "";
                nudPrice.Value = 0;
                cmbEventType.SelectedIndex = -1;
                dtpEventDate.Value = DateTime.Now;

                MessageBox.Show($"Event '{ev.EventName}' added successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            Button btnDeleteEvent = new Button
            {
                Text = "Delete Event",
                Location = new Point(120, 325),
                Width = 100,
                Height = 35
            };
            btnDeleteEvent.Click += (s, e) =>
            {
                if (selectedClient == null || lstEvents.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select an event to delete.", "No Event Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var eventToDelete = selectedClient.ParanormalEvents
                    .OrderByDescending(ev => ev.EventDate)
                    .ElementAtOrDefault(lstEvents.SelectedIndex);

                if (eventToDelete != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete the event '{eventToDelete.EventName}'?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        selectedClient.ParanormalEvents.Remove(eventToDelete);
                        SaveData();
                        RefreshEventList();
                        selectedEvent = null;
                        ClearEventDetails();
                        MessageBox.Show("Event deleted successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            column2.Controls.Add(lblFormTitle);
            column2.Controls.Add(lblEventName);
            column2.Controls.Add(txtEventName);
            column2.Controls.Add(lblEventType);
            column2.Controls.Add(cmbEventType);
            column2.Controls.Add(lblEventDate);
            column2.Controls.Add(dtpEventDate);
            column2.Controls.Add(lblEventLocation);
            column2.Controls.Add(txtEventLocation);
            column2.Controls.Add(lblContactPerson);
            column2.Controls.Add(txtContactPerson);
            column2.Controls.Add(lblContactTelephone);
            column2.Controls.Add(txtContactTelephone);
            column2.Controls.Add(lblPrice);
            column2.Controls.Add(nudPrice);
            column2.Controls.Add(lblEventDescription);
            column2.Controls.Add(txtEventDescription);
            column2.Controls.Add(btnAddEvent);
            column2.Controls.Add(btnDeleteEvent);

            Panel column3 = new Panel { Dock = DockStyle.Right, Width = 400, Padding = new Padding(10) };

            Label lblEventDetailsTitle = new Label
            {
                Text = "Event Details",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            rtbEventDetails = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Arial", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            Panel exportPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(5)
            };

            Button btnExportToGoogleCalendar = new Button
            {
                Text = "Export to Google Calendar",
                Dock = DockStyle.Fill,
                Height = 40,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnExportToGoogleCalendar.Click += BtnExportToGoogleCalendar_Click;

            exportPanel.Controls.Add(btnExportToGoogleCalendar);

            column3.Controls.Add(rtbEventDetails);
            column3.Controls.Add(exportPanel);
            column3.Controls.Add(lblEventDetailsTitle);

            contentPanel.Controls.Add(column2);
            contentPanel.Controls.Add(column3);
            contentPanel.Controls.Add(column1);

            mainPanel.Controls.Add(contentPanel);
            mainPanel.Controls.Add(searchPanel);
            mainPanel.Controls.Add(lblSelectClient);
            tab.Controls.Add(mainPanel);
        }
    } // End of MainForm classs

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
            this.Text = "Add Phenomenon Log";
            this.Size = new Size(550, 450);
            this.StartPosition = FormStartPosition.CenterParent;

            // Set icon
            if (File.Exists("myicon.ico"))
            {
                try
                {
                    this.Icon = new Icon("myicon.ico");
                }
                catch { }
            }

            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            Label lblDateTime = new Label { Text = "Date/Time:", Location = new Point(10, 10), Width = 100 };
            dtpOccurred = new DateTimePicker { Location = new Point(120, 10), Width = 300, Format = DateTimePickerFormat.Custom, CustomFormat = "MM/dd/yyyy hh:mm tt" };

            Label lblType = new Label { Text = "Type:", Location = new Point(10, 45), Width = 100 };
            cmbType = new ComboBox { Location = new Point(120, 45), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new object[] { "Apparition", "EVP", "Cold Spot", "Object Movement", "Shadow Figure", "Strange Sound", "EMF Spike", "Other" });
            cmbType.SelectedIndex = 0;

            Label lblLocation = new Label { Text = "Location:", Location = new Point(10, 80), Width = 100 };
            txtLocation = new TextBox { Location = new Point(120, 80), Width = 300 };

            Label lblWitnesses = new Label { Text = "Witnesses:", Location = new Point(10, 115), Width = 100 };
            txtWitnesses = new TextBox { Location = new Point(120, 115), Width = 300 };

            Label lblDescription = new Label { Text = "Description:", Location = new Point(10, 150), Width = 100 };
            txtDescription = new TextBox { Location = new Point(120, 150), Width = 300, Height = 150, Multiline = true, ScrollBars = ScrollBars.Vertical };

            Button btnOK = new Button { Text = "Save", Location = new Point(120, 320), Width = 90, Height = 35, DialogResult = DialogResult.OK };
            btnOK.Click += (s, e) =>
            {
                DateTimeOccurred = dtpOccurred.Value;
                PhenomenonType = cmbType.SelectedItem?.ToString();
                Description = txtDescription.Text;
                LocationText = txtLocation.Text;
                Witnesses = txtWitnesses.Text;
            };

            Button btnCancel = new Button { Text = "Cancel", Location = new Point(220, 320), Width = 90, Height = 35, DialogResult = DialogResult.Cancel };

            panel.Controls.AddRange(new Control[] { lblDateTime, dtpOccurred, lblType, cmbType, lblLocation, txtLocation, lblWitnesses, txtWitnesses, lblDescription, txtDescription, btnOK, btnCancel });
            this.Controls.Add(panel);
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
                    using (System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath))
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
        }
    }

    public static class RichTextBoxExtensions
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private const int WM_USER = 0x0400;
        private const int EM_FORMATRANGE = WM_USER + 57;

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct CHARRANGE
        {
            public int cpMin, cpMax;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct FORMATRANGE
        {
            public IntPtr hdc, hdcTarget;
            public RECT rc, rcPage;
            public CHARRANGE chrg;
        }

        public static int Print(this RichTextBox richTextBox, int charFrom, int charTo, System.Drawing.Printing.PrintPageEventArgs e)
        {
            const double anInch = 14.4;
            RECT rectToPrint = new RECT
            {
                Top = (int)(e.MarginBounds.Top * anInch),
                Bottom = (int)(e.MarginBounds.Bottom * anInch),
                Left = (int)(e.MarginBounds.Left * anInch),
                Right = (int)(e.MarginBounds.Right * anInch)
            };

            RECT rectPage = new RECT
            {
                Top = (int)(e.PageBounds.Top * anInch),
                Bottom = (int)(e.PageBounds.Bottom * anInch),
                Left = (int)(e.PageBounds.Left * anInch),
                Right = (int)(e.PageBounds.Right * anInch)
            };

            IntPtr hdc = e.Graphics.GetHdc();
            FORMATRANGE fmtRange = new FORMATRANGE
            {
                chrg = new CHARRANGE { cpMax = charTo, cpMin = charFrom },
                hdc = hdc,
                hdcTarget = hdc,
                rc = rectToPrint,
                rcPage = rectPage
            };

            IntPtr lparam = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(System.Runtime.InteropServices.Marshal.SizeOf(fmtRange));
            System.Runtime.InteropServices.Marshal.StructureToPtr(fmtRange, lparam, false);
            IntPtr res = SendMessage(richTextBox.Handle, EM_FORMATRANGE, new IntPtr(1), lparam);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(lparam);
            e.Graphics.ReleaseHdc(hdc);
            return res.ToInt32();
        }
    }

    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (MapsSplashScreen splash = new MapsSplashScreen())
            {
                splash.ShowDialog();
            }
            Application.Run(new MainForm());
        }
    }
}