using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TicketBookingStaffApp.BLL;
using TicketBookingStaffApp.Models;

namespace TicketBookingStaffApp.GUI
{
    public partial class frmShowtime : Form
    {
        private readonly ShowtimeBLL _showtimeBLL = new ShowtimeBLL();
        private FlowLayoutPanel pnlShowtimes;
        private ComboBox cboDateFilter;
        private ComboBox cboTheatreFilter;
        private Panel filterPanel;
        private Panel contentPanel;

        private List<Showtime> allShowtimes = new List<Showtime>();
        private string _movieId;

        public frmShowtime(string movieId)
        {
            _movieId = movieId;
            InitializeComponent();
            InitializeUI();
            LoadShowtimes();

            // Xử lý resize để responsive
            this.Resize += FrmShowtime_Resize;
        }

        private void InitializeUI()
        {
            this.Text = "Lịch Chiếu Phim";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.FromArgb(245, 247, 250);

            // ===== FILTER PANEL =====
            filterPanel = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 90,
                BackColor = Color.White,
                Padding = new Padding(30, 150, 30, 15)
            };

            // Container cho filters để căn giữa
            Panel filterContainer = new Panel()
            {
                Location = new Point(30, 15),
                Height = 60,
                AutoSize = true
            };

            Label lblDate = new Label()
            {
                Text = "📅 Ngày chiếu:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(0, 18),
                AutoSize = true
            };

            cboDateFilter = new ComboBox()
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(130, 15),
                Width = 220,
                Height = 30
            };
            cboDateFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            Label lblTheatre = new Label()
            {
                Text = "🏛️ Rạp chiếu:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(380, 18),
                AutoSize = true
            };

            cboTheatreFilter = new ComboBox()
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(500, 15),
                Width = 280,
                Height = 30
            };
            cboTheatreFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            filterContainer.Controls.Add(lblDate);
            filterContainer.Controls.Add(cboDateFilter);
            filterContainer.Controls.Add(lblTheatre);
            filterContainer.Controls.Add(cboTheatreFilter);

            filterPanel.Controls.Add(filterContainer);
            this.Controls.Add(filterPanel);

            // ===== CONTENT PANEL (chứa FlowLayoutPanel) =====
            contentPanel = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(20)
            };

            pnlShowtimes = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(245, 247, 250),
                WrapContents = true
            };

            contentPanel.Controls.Add(pnlShowtimes);
            this.Controls.Add(contentPanel);
        }

        private void FrmShowtime_Resize(object sender, EventArgs e)
        {
            // Điều chỉnh filter panel khi resize
            if (filterPanel != null && filterPanel.Controls.Count > 0)
            {
                Panel filterContainer = filterPanel.Controls[0] as Panel;
                if (filterContainer != null)
                {
                    // Căn giữa filter container
                    int centerX = (filterPanel.Width - 800) / 2;
                    if (centerX < 30) centerX = 30;
                    filterContainer.Location = new Point(centerX, 15);
                }
            }
        }

        private void LoadShowtimes()
        {
            pnlShowtimes.Controls.Clear();

            try
            {
                allShowtimes = _showtimeBLL.GetShowtimesByMovieId(_movieId);

                if (allShowtimes.Count == 0)
                {
                    ShowEmptyMessage("😥 Không có lịch chiếu nào cho phim này.");
                    return;
                }

                // Populate date filter
                cboDateFilter.Items.Clear();
                cboDateFilter.Items.Add("Tất cả ngày");
                foreach (var date in allShowtimes.Select(s => s.ShowtimeDate.Date).Distinct().OrderBy(d => d))
                    cboDateFilter.Items.Add(date.ToString("dd/MM/yyyy"));
                cboDateFilter.SelectedIndex = 0;

                // Populate theatre filter
                cboTheatreFilter.Items.Clear();
                cboTheatreFilter.Items.Add("Tất cả rạp");
                foreach (var theatre in allShowtimes.Select(s => s.TheatreName).Distinct().OrderBy(t => t))
                    cboTheatreFilter.Items.Add(theatre);
                cboTheatreFilter.SelectedIndex = 0;

                DisplayShowtimes(allShowtimes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters()
        {
            var filtered = allShowtimes.AsEnumerable();

            if (cboDateFilter.SelectedIndex > 0)
            {
                var selectedDate = DateTime.ParseExact(cboDateFilter.SelectedItem.ToString(), "dd/MM/yyyy", null);
                filtered = filtered.Where(s => s.ShowtimeDate.Date == selectedDate);
            }

            if (cboTheatreFilter.SelectedIndex > 0)
            {
                var selectedTheatre = cboTheatreFilter.SelectedItem.ToString();
                filtered = filtered.Where(s => s.TheatreName == selectedTheatre);
            }

            var result = filtered.ToList();

            if (result.Count == 0)
            {
                ShowEmptyMessage("😥 Không có lịch chiếu phù hợp với bộ lọc.");
            }
            else
            {
                DisplayShowtimes(result);
            }
        }

        private void ShowEmptyMessage(string message)
        {
            pnlShowtimes.Controls.Clear();

            Panel emptyPanel = new Panel()
            {
                Width = pnlShowtimes.Width - 40,
                Height = 150,
                BackColor = Color.White
            };

            Label emptyLabel = new Label()
            {
                Text = message,
                Font = new Font("Segoe UI", 14, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            emptyPanel.Controls.Add(emptyLabel);
            pnlShowtimes.Controls.Add(emptyPanel);
        }

        private void DisplayShowtimes(List<Showtime> showtimes)
        {
            pnlShowtimes.Controls.Clear();

            // Sắp xếp theo ngày và giờ
            var sortedShowtimes = showtimes.OrderBy(s => s.ShowtimeDate).ThenBy(s => s.MovieStartTime).ToList();

            foreach (var s in sortedShowtimes)
            {
                Panel card = CreateShowtimeCard(s);
                pnlShowtimes.Controls.Add(card);
            }
        }

        private Panel CreateShowtimeCard(Showtime s)
        {
            Panel card = new Panel()
            {
                Width = 340,
                Height = 180,
                BackColor = Color.White,
                Margin = new Padding(10),
                Padding = new Padding(0),
                Tag = s
            };
            card.MouseClick += Card_Click;
            // ===== Header =====
            Panel cardHeader = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(52, 152, 219)
            };
            Label lblDate = new Label()
            {
                Text = $"📅 {s.ShowtimeDate:dddd, dd/MM/yyyy}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, (cardHeader.Height - 20) / 2)
            };
            cardHeader.Controls.Add(lblDate);

            // ===== Footer =====
            Panel cardFooter = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.FromArgb(46, 204, 113)
            };
            Label lblPrice = new Label()
            {
                Text = $"💰 Giá vé: {s.PricePerSeat:N0} VND",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, (cardFooter.Height - 20) / 2)
            };
            cardFooter.Controls.Add(lblPrice);

            // ===== Body =====
            Panel cardBody = new Panel()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                BackColor = Color.White
            };

            int yPos = 0;
            int spacing = 28;

            Label lblTime = new Label()
            {
                Text = $"🕒 Giờ chiếu: {s.MovieStartTime}",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            cardBody.Controls.Add(lblTime);

            yPos += spacing;
            Label lblTheatre = new Label()
            {
                Text = $"🏛️ Rạp: {s.TheatreName}",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            cardBody.Controls.Add(lblTheatre);

            yPos += spacing;
            Label lblHall = new Label()
            {
                Text = $"🎫 Phòng chiếu: {s.HallName}",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            cardBody.Controls.Add(lblHall);

            yPos += spacing;
            Label lblType = new Label()
            {
                Text = $"🎥 Định dạng: {s.ShowType}",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            cardBody.Controls.Add(lblType);

            // ===== Add panels in correct order =====
            card.Controls.Add(cardBody);    // Fill remaining space first
            card.Controls.Add(cardFooter);  // Dock Bottom
            card.Controls.Add(cardHeader);  // Dock Top

            AddClickEventToAllControls(card, Card_Click);

            return card;
        }

        private void AddClickEventToAllControls(Control parent, EventHandler handler)
        {
            parent.Click += handler;
            foreach (Control c in parent.Controls)
            {
                AddClickEventToAllControls(c, handler);
            }
        }

        private void Card_Click(object sender, EventArgs e)
        {
            // Bắt đầu với sender, sau đó đi ngược lên để tìm Panel chứa Tag là Showtime.
            // Điều này xử lý tốt trường hợp clickedControl là Label, Panel con, hoặc chính Panel cha (card).

            Control clickedControl = sender as Control;
            Panel card = null;

            if (clickedControl != null)
            {
                // Duyệt ngược lên cây Controls để tìm Panel có chứa đối tượng Showtime (Tag)
                Control currentControl = clickedControl;
                while (currentControl != null)
                {
                    if (currentControl is Panel && currentControl.Tag is Showtime)
                    {
                        card = currentControl as Panel;
                        break; // Đã tìm thấy Panel chứa dữ liệu Showtime
                    }
                    // Ép kiểu Parent sang Control trước khi gán
                    currentControl = currentControl.Parent;
                }
            }

            if (card == null)
            {
                // Không tìm thấy Panel cha hợp lệ, dừng lại
                return;
            }

            Showtime s = card.Tag as Showtime;

            // Lấy ra movieId, hallId, showtimeId
            string movieId = _movieId;
            string hallId = s.HallId; // Thuộc tính HallId của Showtime
            string showtimeId = s.Id;   // Thuộc tính Id của Showtime
            string price = s.PricePerSeat.ToString("N0") + " VND";

            // Mở form Seat
            // Đảm bảo rằng frmSeat đã được định nghĩa
            // Nếu bạn đang mở form này từ một form cha (ví dụ: MainForm), bạn có thể cần truyền tham chiếu của nó.
            // Nếu frmSeat chưa được định nghĩa trong scope, bạn cần thêm using hoặc đảm bảo class đó có tồn tại.

            // Giả định frmSeat có sẵn trong scope:
            // Bạn cần định nghĩa class frmSeat và đảm bảo nó nhận các tham số này
            // Ví dụ: public partial class frmSeat : Form { public frmSeat(string movieId, string hallId, string showtimeId) { /* ... */ } }

            // Nếu class frmSeat không có trong dự án, nó sẽ báo lỗi.
            frmBooking seatForm = new frmBooking(movieId, hallId, showtimeId, s.PricePerSeat);
            seatForm.ShowDialog();
        }
    }
}