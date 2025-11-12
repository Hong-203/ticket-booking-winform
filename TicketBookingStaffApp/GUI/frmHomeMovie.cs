using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Windows.Forms;
using TicketBookingStaffApp.BLL;
using TicketBookingStaffApp.Models;

namespace TicketBookingStaffApp.GUI
{
    public partial class frmHomeMovie : Form
    {
        private readonly movieBLL _movieBLL = new movieBLL();
        private int currentPage = 1;
        private int pageSize = 8; // 8 phim / trang

        private FlowLayoutPanel flowMovies;
        private Button btnPrev, btnNext;
        private Label lblPageInfo;

        public frmHomeMovie()
        {
            InitializeComponent();
            InitializeUI();
            LoadMovies();
        }

        private void InitializeUI()
        {
            this.Text = "🎬 Danh Sách Phim";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);

            // FlowLayoutPanel - grid style
            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
            };
            this.Controls.Add(container);

            flowMovies = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(120, 0, 40, 250),
                BackColor = Color.FromArgb(245, 247, 250),
                WrapContents = true
            };
            container.Controls.Add(flowMovies);


            // Pagination
            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.WhiteSmoke,
            };
            this.Controls.Add(bottomPanel);

            btnPrev = CreateButton("← Trước");
            btnPrev.Click += BtnPrev_Click;
            bottomPanel.Controls.Add(btnPrev);

            lblPageInfo = new Label
            {
                Text = "Trang 1/1",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            bottomPanel.Controls.Add(lblPageInfo);

            btnNext = CreateButton("Tiếp →");
            btnNext.Click += BtnNext_Click;
            bottomPanel.Controls.Add(btnNext);

            bottomPanel.Resize += (s, e) =>
            {
                int centerX = bottomPanel.Width / 2;
                btnPrev.Location = new Point(centerX - 220, 20);
                lblPageInfo.Location = new Point(centerX - lblPageInfo.Width / 2, 28);
                btnNext.Location = new Point(centerX + 120, 20);
            };
        }

        private Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
        }

        private void LoadMovies()
        {
            flowMovies.Controls.Clear();

            List<Movie> movies = _movieBLL.GetMovies(currentPage, pageSize);
            int totalPages = _movieBLL.GetTotalPages(pageSize);

            foreach (var movie in movies)
            {
                flowMovies.Controls.Add(CreateMovieCard(movie));
            }

            lblPageInfo.Text = $"Trang {currentPage}/{totalPages}";
            btnPrev.Enabled = currentPage > 1;
            btnNext.Enabled = currentPage < totalPages;
        }

        private Panel CreateMovieCard(Movie movie)
        {
            Panel card = new Panel
            {
                Width = 290,
                Height = 460,
                Margin = new Padding(15),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            // Bo góc & bóng đổ
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 12;
                    path.AddArc(0, 0, radius, radius, 180, 90);
                    path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
                    path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
                    path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
                    path.CloseFigure();
                    card.Region = new Region(path);
                }
            };

            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(237, 242, 255);
                card.Shadow(Color.FromArgb(180, 180, 180));
            };
            card.MouseLeave += (s, e) =>
            {
                card.BackColor = Color.White;
                card.Shadow(Color.LightGray);
            };

            // Ảnh
            PictureBox pic = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 300,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = LoadMovieImage(movie.ImagePath)
            };
            card.Controls.Add(pic);

            // Info panel
            Panel info = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 160,
                Padding = new Padding(8),
                BackColor = Color.White
            };

            Label lblName = new Label
            {
                Text = movie.Name ?? "(Không có tên)",
                Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
                ForeColor = Color.Gray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };
            info.Controls.Add(lblName);

            Label lblDuration = new Label
            {
                Text = $"⏱ {movie.Duration} phút",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 25
            };
            info.Controls.Add(lblDuration);

            // ✅ Hai nút chức năng
            FlowLayoutPanel actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.LeftToRight,
                Height = 50,
                Padding = new Padding(10, 5, 10, 5),
                BackColor = Color.White,
                AutoSize = false
            };

            Button btnDatVe = new Button
            {
                Text = "🎟 Đặt vé",
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 110,
                Height = 35,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDatVe.FlatAppearance.BorderSize = 0;
            btnDatVe.Click += (s, e) =>
            {
                MessageBox.Show($"Đặt vé cho phim: {movie.Name}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            Button btnXemPhim = new Button
            {
                Text = "▶️ Xem phim",
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 110,
                Height = 35,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnXemPhim.FlatAppearance.BorderSize = 0;
            btnXemPhim.Click += (s, e) =>
            {
                frmMovieDetail detailForm = new frmMovieDetail(movie.Id);
                detailForm.ShowDialog();
            };

            // Thêm nút vào panel
            actionPanel.Controls.Add(btnDatVe);
            actionPanel.Controls.Add(btnXemPhim);
            info.Controls.Add(actionPanel);

            card.Controls.Add(info);
            return card;
        }

        private Image LoadMovieImage(string imagePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var wc = new WebClient())
                        {
                            byte[] bytes = wc.DownloadData(imagePath);
                            using (var ms = new MemoryStream(bytes))
                            {
                                return Image.FromStream(ms);
                            }
                        }
                    }
                    if (File.Exists(imagePath))
                        return Image.FromFile(imagePath);
                }
            }
            catch { }

            // Placeholder
            Bitmap bmp = new Bitmap(260, 300);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                g.DrawString("🎬", new Font("Segoe UI", 48), Brushes.DarkGray, new PointF(70, 90));
                g.DrawString("No Image", new Font("Segoe UI", 12, FontStyle.Bold), Brushes.DimGray, new PointF(80, 210));
            }
            return bmp;
        }

        private void BtnPrev_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadMovies();
            }
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            int totalPages = _movieBLL.GetTotalPages(pageSize);
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadMovies();
            }
        }
    }

    // ✅ Extension để tạo shadow nhẹ
    public static class PanelExtensions
    {
        public static void Shadow(this Panel panel, Color shadowColor)
        {
            panel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(shadowColor, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                }
            };
            panel.Invalidate();
        }
    }
}
