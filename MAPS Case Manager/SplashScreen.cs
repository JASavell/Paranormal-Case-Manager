using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ParanormalInvestigator
{
    public class MapsSplashScreen : Form
    {
        private Timer closeTimer;

        public MapsSplashScreen()
        {
            InitializeSplashScreen();
        }

        private void InitializeSplashScreen()
        {
            // Form settings
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(600, 500);
            this.BackColor = Color.Black;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // Set icon if available
            if (File.Exists("myicon.ico"))
            {
                try
                {
                    this.Icon = new Icon("myicon.ico");
                }
                catch { }
            }

            // Main container panel
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };

            // Try to load splash.png
            PictureBox pbSplash = new PictureBox
            {
                Location = new Point(50, 20),
                Size = new Size(500, 250),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black
            };

            bool imageLoaded = false;
            if (File.Exists("splash.png"))
            {
                try
                {
                    pbSplash.Image = Image.FromFile("splash.png");
                    imageLoaded = true;
                }
                catch
                {
                    // Image failed to load
                }
            }

            // Add picture box if image loaded
            if (imageLoaded)
            {
                mainPanel.Controls.Add(pbSplash);
            }

            // Title label (always show below image or at top if no image)
            int startY = imageLoaded ? 280 : 120;

            Label lblTitle = new Label
            {
                Text = "Paranormal Case Manager Version 1.0",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(50, startY),
                Size = new Size(500, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Author label - using Panel to hold multiple labels for colored text
            Panel authorPanel = new Panel
            {
                Location = new Point(50, startY + 40),
                Size = new Size(500, 30),
                BackColor = Color.Transparent
            };

            // Calculate positions for inline text with different colors
            Font authorFont = new Font("Arial", 11, FontStyle.Regular);

            string text1 = "By ";
            string text2 = "Josh Savell";
            string text3 = " of The Mobile Area Paranormal Society";

            // Measure text widths
            using (Graphics g = authorPanel.CreateGraphics())
            {
                SizeF size1 = g.MeasureString(text1, authorFont);
                SizeF size2 = g.MeasureString(text2, authorFont);
                SizeF size3 = g.MeasureString(text3, authorFont);

                float totalWidth = size1.Width + size2.Width + size3.Width;
                float startX = (500 - totalWidth) / 2; // Center the entire text

                Label lblBy = new Label
                {
                    Text = text1,
                    Font = authorFont,
                    ForeColor = Color.White,
                    Location = new Point((int)startX, 5),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                Label lblJoshSavell = new Label
                {
                    Text = text2,
                    Font = authorFont,
                    ForeColor = Color.Red,
                    Location = new Point((int)(startX + size1.Width), 5),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                Label lblOf = new Label
                {
                    Text = text3,
                    Font = authorFont,
                    ForeColor = Color.White,
                    Location = new Point((int)(startX + size1.Width + size2.Width), 5),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                authorPanel.Controls.Add(lblBy);
                authorPanel.Controls.Add(lblJoshSavell);
                authorPanel.Controls.Add(lblOf);
            }

            // Website label
            Label lblWebsite = new Label
            {
                Text = "https://maps-paranormal.com",
                Font = new Font("Arial", 11, FontStyle.Italic),
                ForeColor = Color.Red,
                Location = new Point(50, startY + 75),
                Size = new Size(500, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            // Make website clickable
            lblWebsite.Click += (s, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start("https://maps-paranormal.com");
                }
                catch
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://maps-paranormal.com") { UseShellExecute = true });
                    }
                    catch { }
                }
            };

            // Loading label
            Label lblLoading = new Label
            {
                Text = "Loading...",
                Font = new Font("Arial", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(50, startY + 120),
                Size = new Size(500, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            mainPanel.Controls.AddRange(new Control[] {
                lblTitle,
                authorPanel,
                lblWebsite,
                lblLoading
            });

            this.Controls.Add(mainPanel);

            // Timer to close splash screen after 5 seconds
            closeTimer = new Timer
            {
                Interval = 5000 // 5 seconds
            };
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                this.Close();
            };
        }

        public void StartSplash()
        {
            closeTimer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartSplash();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (closeTimer != null)
                {
                    closeTimer.Stop();
                    closeTimer.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}