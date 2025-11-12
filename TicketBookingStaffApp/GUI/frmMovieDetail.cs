using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Windows.Forms;
using TicketBookingStaffApp.BLL;
using TicketBookingStaffApp.Models;

namespace TicketBookingStaffApp.GUI
{
    public partial class frmMovieDetail : Form
    {
        private readonly movieBLL _movieBLL = new movieBLL();
        private Movie _movie;

        private Panel container;
        private Panel detailTextPanel;
        private Label lblSynopsis;
        private Button btnBack;
        private Panel footerPanel;

        public frmMovieDetail(string movieId)
        {
            // InitializeComponent(); // Giả sử tồn tại
            LoadMovieDetail(movieId);
            SetupResizeEvents();
        }

        private void SetupResizeEvents()
        {
            // Nút quay lại
            footerPanel.Resize += (s, e) =>
            {
                if (btnBack != null)
                {
                    btnBack.Location = new Point(40, (footerPanel.Height - btnBack.Height) / 2);
                }
            };

            // Label Tóm tắt
            detailTextPanel.Resize += (s, e) =>
            {
                if (lblSynopsis != null)
                {
                    // Đảm bảo MaximumSize luôn vừa với chiều rộng Panel
                    lblSynopsis.MaximumSize = new Size(detailTextPanel.ClientSize.Width - detailTextPanel.Padding.Horizontal, 0);
                    lblSynopsis.Invalidate();
                }
            };
        }

        private void LoadMovieDetail(string movieId)
        {
            _movie = _movieBLL.GetMovieDetail(movieId);

            if (_movie == null)
            {
                MessageBox.Show("Không tìm thấy phim!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            this.Text = $"🎬 {_movie.Name}";
            this.BackColor = Color.WhiteSmoke;
            this.WindowState = FormWindowState.Maximized;
            this.Controls.Clear();

            // Footer
            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.WhiteSmoke
            };
            this.Controls.Add(footerPanel);

            // Container
            container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                AutoScroll = true,
                BackColor = Color.WhiteSmoke
            };
            this.Controls.Add(container);

            // Main content panel
            Panel mainContentPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 700,
                BackColor = Color.White
            };
            mainContentPanel.Paint += (s, e) => DrawRoundedRectangle(e.Graphics, mainContentPanel.ClientRectangle, 20, Color.White);
            container.Controls.Add(mainContentPanel);

            // Right: Movie image
            Panel picContainer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 450,
                Padding = new Padding(30)
            };
            mainContentPanel.Controls.Add(picContainer);

            PictureBox pic = new PictureBox
            {
                Image = LoadMovieImage(_movie.ImagePath),
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BackColor = Color.Gainsboro
            };
            pic.Paint += (s, e) =>
            {
                using (GraphicsPath path = GetRoundedRectPath(pic.ClientRectangle, 15))
                {
                    pic.Region = new Region(path);
                }
            };
            picContainer.Controls.Add(pic);

            // Left: Movie details
            detailTextPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40, 30, 20, 30),
                AutoScroll = true
            };
            mainContentPanel.Controls.Add(detailTextPanel);

            // Content stack (vertical)
            FlowLayoutPanel contentStack = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false
            };
            detailTextPanel.Controls.Add(contentStack);

            // 🎬 Movie Title
            Label lblTitle = new Label
            {
                Text = _movie.Name,
                Font = new Font("Segoe UI", 34, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            contentStack.Controls.Add(lblTitle);

            // ⭐ Rating
            //Label lblRating = new Label
            //{
            //    Text = $"⭐ {_movie.Rating:0.0}/10",
            //    Font = new Font("Segoe UI", 16, FontStyle.Bold),
            //    ForeColor = Color.FromArgb(234, 179, 8),
            //    AutoSize = true,
            //    Margin = new Padding(0, 0, 0, 15)
            //};
            //contentStack.Controls.Add(lblRating);

            // 🎟 Badges
            FlowLayoutPanel badgesPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 25),
                BackColor = Color.Transparent
            };
            contentStack.Controls.Add(badgesPanel);

            void AddBadge(string icon, string text, Color color)
            {
                Panel badge = new Panel
                {
                    AutoSize = true,
                    BackColor = color,
                    Margin = new Padding(0, 0, 10, 10),
                    Padding = new Padding(15, 8, 15, 8)
                };
                badge.Paint += (s, e) => DrawRoundedRectangle(e.Graphics, badge.ClientRectangle, 8, color);

                Label lbl = new Label
                {
                    Text = $"{icon} {text}",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                badge.Controls.Add(lbl);
                badgesPanel.Controls.Add(badge);
            }

            AddBadge("⏱", $"{_movie.Duration} phút", Color.FromArgb(59, 130, 246));
            AddBadge("📅", _movie.ReleaseDate?.ToString("dd/MM/yyyy") ?? "N/A", Color.FromArgb(16, 185, 129));
            AddBadge("🗣", _movie.Language, Color.FromArgb(236, 72, 153));

            // 🎭 Info rows
            void AddInfoRow(string icon, string text)
            {
                Label lbl = new Label
                {
                    Text = $"{icon} {text}",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(55, 65, 81),
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 5)
                };
                contentStack.Controls.Add(lbl);
            }

            AddInfoRow("🎭", $"Thể loại: {_movie.Genres}");
            AddInfoRow("🎬", $"Đạo diễn: {_movie.Directors}");

            // Divider
            Panel divider = new Panel
            {
                Height = 2,
                Width = 400,
                BackColor = Color.LightGray,
                Margin = new Padding(0, 10, 0, 10)
            };
            contentStack.Controls.Add(divider);

            // 📝 Synopsis
            Label lblSynopsisTitle = new Label
            {
                Text = "📝 Tóm tắt nội dung",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            contentStack.Controls.Add(lblSynopsisTitle);

            lblSynopsis = new Label
            {
                Text = _movie.Synopsis,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(55, 65, 81),
                MaximumSize = new Size(800, 0),
                AutoSize = true
            };
            contentStack.Controls.Add(lblSynopsis);

            Button btnBook = new Button
            {
                Text = "🎟 Đặt vé ngay",
                AutoSize = true,
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Height = 45,
                Padding = new Padding(15, 5, 15, 5),
                Cursor = Cursors.Hand
            };
            btnBook.FlatAppearance.BorderSize = 0;
            btnBook.Location = new Point(lblTitle.Right + 20, 20);
            btnBook.MouseEnter += (s, e) => btnBook.BackColor = Color.FromArgb(37, 99, 235);
            btnBook.MouseLeave += (s, e) => btnBook.BackColor = Color.FromArgb(59, 130, 246);
            btnBook.Click += (s, e) =>
            {
                frmShowtime showtimeForm = new frmShowtime(_movie.Id);
                showtimeForm.Show();
            };
            contentStack.Controls.Add(btnBook);

            // ⬅ Back button
            btnBack = new Button
            {
                Text = "⬅  Quay lại",
                Width = 160,
                Height = 45,
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.MouseEnter += (s, e) => btnBack.BackColor = Color.FromArgb(209, 213, 219);
            btnBack.MouseLeave += (s, e) => btnBack.BackColor = Color.FromArgb(226, 232, 240);
            btnBack.Click += (s, e) => this.Close();
            footerPanel.Controls.Add(btnBack);
            btnBack.Location = new Point(50, (footerPanel.Height - btnBack.Height) / 2);
            footerPanel.Resize += (s, e) =>
            {
                btnBack.Location = new Point(50, (footerPanel.Height - btnBack.Height) / 2);
            };
        }

        // --- Hàm DrawRoundedRectangle, GetRoundedRectPath và LoadMovieImage (Giữ nguyên) ---

        private void DrawRoundedRectangle(Graphics g, Rectangle bounds, int radius, Color fillColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = GetRoundedRectPath(bounds, radius))
            using (SolidBrush brush = new SolidBrush(fillColor))
            {
                g.FillPath(brush, path);
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Image LoadMovieImage(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith("http"))
                    {
                        using (var wc = new WebClient())
                        {
                            byte[] bytes = wc.DownloadData(path);
                            using (var ms = new MemoryStream(bytes))
                            {
                                return Image.FromStream(ms);
                            }
                        }
                    }
                    else if (File.Exists(path))
                    {
                        return Image.FromFile(path);
                    }
                }
            }
            catch { }

            Bitmap bmp = new Bitmap(400, 600);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Rectangle(0, 0, 400, 600),
                    Color.LightGray,
                    Color.WhiteSmoke,
                    45f))
                {
                    g.FillRectangle(brush, 0, 0, 400, 600);
                }
                g.DrawString("🎬", new Font("Segoe UI", 80), Brushes.Gray, new PointF(130, 200));
                g.DrawString("No Image", new Font("Segoe UI", 20), Brushes.DimGray, new PointF(120, 320));
            }
            return bmp;
        }
    }
}