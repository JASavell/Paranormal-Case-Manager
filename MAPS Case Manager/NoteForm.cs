using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ParanormalInvestigator
{
    public class NoteForm : Form
    {
        public string NoteTitle { get; set; }
        public string NoteContent { get; set; }
        public string NoteImagePath { get; set; }

        private TextBox txtTitle;
        private TextBox txtContent;
        private PictureBox pbImage;
        private Button btnAddImage;

        public NoteForm() : this("", "", "") { }

        public NoteForm(string existingTitle, string existingContent, string existingImagePath)
        {
            InitializeForm();
            txtTitle.Text = existingTitle ?? "";
            txtContent.Text = existingContent ?? "";
            NoteImagePath = existingImagePath;
            
            if (!string.IsNullOrEmpty(existingImagePath) && File.Exists(existingImagePath))
            {
                try
                {
                    pbImage.Image = Image.FromFile(existingImagePath);
                }
                catch { }
            }
        }

        private void InitializeForm()
        {
            this.Text = "Add/Edit Note";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblTitle = new Label { Text = "Title:", Location = new Point(15, 15), Width = 60 };
            txtTitle = new TextBox { Location = new Point(80, 12), Width = 380 };

            Label lblContent = new Label { Text = "Content:", Location = new Point(15, 50), Width = 60 };
            txtContent = new TextBox { Location = new Point(80, 47), Width = 380, Height = 120, Multiline = true, ScrollBars = ScrollBars.Vertical };

            Label lblImage = new Label { Text = "Image (optional):", Location = new Point(15, 180), Width = 120 };
            pbImage = new PictureBox
            {
                Location = new Point(140, 180),
                Size = new Size(100, 100),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray
            };

            btnAddImage = new Button
            {
                Text = "Add Image",
                Location = new Point(250, 180),
                Width = 100,
                Height = 30
            };
            btnAddImage.Click += BtnAddImage_Click;

            Button btnRemoveImage = new Button
            {
                Text = "Remove Image",
                Location = new Point(250, 220),
                Width = 100,
                Height = 30
            };
            btnRemoveImage.Click += (s, e) =>
            {
                pbImage.Image = null;
                NoteImagePath = null;
            };

            Button btnOK = new Button { Text = "Save", Location = new Point(280, 310), Width = 90, DialogResult = DialogResult.OK };
            btnOK.Click += (s, e) =>
            {
                NoteTitle = txtTitle.Text.Trim();
                NoteContent = txtContent.Text;
            };

            Button btnCancel = new Button { Text = "Cancel", Location = new Point(380, 310), Width = 90, DialogResult = DialogResult.Cancel };

            this.Controls.Add(lblTitle);
            this.Controls.Add(txtTitle);
            this.Controls.Add(lblContent);
            this.Controls.Add(txtContent);
            this.Controls.Add(lblImage);
            this.Controls.Add(pbImage);
            this.Controls.Add(btnAddImage);
            this.Controls.Add(btnRemoveImage);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);
        }

        private void BtnAddImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    NoteImagePath = ofd.FileName;
                    pbImage.Image = Image.FromFile(ofd.FileName);
                }
            }
        }
    }
}