using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TicketBookingStaffApp.BLL;
using TicketBookingStaffApp.DAL.repositories;

namespace TicketBookingStaffApp.GUI
{
    public partial class frmSeat : Form
    {
        private readonly SeatBLL _seatBLL = new SeatBLL();
        private Panel pnlMain;
        private Panel pnlScreen;
        private Panel pnlSeatMap;
        private Panel pnlLegend;
        private Panel pnlSidebar;
        private Label lblTitle;
        private Label lblMovieInfo;
        private Label lblTotalPrice;
        private ListBox lstSelectedSeats;
        private Button btnBookSeats;
        private Button btnClearSelection;

        private string _movieId;
        private string _hallId;
        private string _showtimeId;
        private decimal _pricePerSeat;
        private List<SeatInfo> allSeats = new List<SeatInfo>();
        private Dictionary<string, Button> seatButtons = new Dictionary<string, Button>();
        private List<SeatInfo> selectedSeats = new List<SeatInfo>();

        // Màu sắc cho từng loại ghế
        private readonly Color COLOR_NORMAL_EMPTY = Color.FromArgb(46, 204, 113);
        private readonly Color COLOR_VIP_EMPTY = Color.FromArgb(241, 196, 15);
        private readonly Color COLOR_COUPLE_EMPTY = Color.FromArgb(155, 89, 182);
        private readonly Color COLOR_BOOKED = Color.FromArgb(231, 76, 60);
        private readonly Color COLOR_SELECTED = Color.FromArgb(52, 152, 219);

        public frmSeat(string movieId, string hallId, string showtimeId, decimal price)
        {
            _movieId = movieId;
            _hallId = hallId;
            _showtimeId = showtimeId;
            _pricePerSeat = price;

            InitializeComponent();
            InitializeUI();
            LoadSeatsAsync();
        }

        private void InitializeUI()
        {
            this.Text = "Sơ Đồ Ghế Ngồi";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(30, 39, 46);
            this.MinimumSize = new Size(1200, 700);

            // Main container
            pnlMain = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 39, 46),
                Padding = new Padding(20)
            };
            this.Controls.Add(pnlMain);

            // ===== SIDEBAR (Bên phải) =====
            CreateSidebar();

            // Header
            Panel headerPanel = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(30, 39, 46)
            };

            lblTitle = new Label()
            {
                Text = "🎬 SƠ ĐỒ GHẾ NGỒI",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(0, 10)
            };
            headerPanel.Controls.Add(lblTitle);

            lblMovieInfo = new Label()
            {
                Text = $"Giá vé cơ bản: {_pricePerSeat:N0} VND | Vui lòng chọn ghế",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(189, 195, 199),
                AutoSize = true,
                Location = new Point(0, 55)
            };
            headerPanel.Controls.Add(lblMovieInfo);

            pnlMain.Controls.Add(headerPanel);

            // Screen panel
            pnlScreen = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(30, 39, 46),
                Padding = new Padding(0, 10, 0, 10)
            };

            Panel screenDisplay = new Panel()
            {
                Width = 800,
                Height = 50,
                BackColor = Color.FromArgb(52, 73, 94),
                Location = new Point(0, 10)
            };

            Label lblScreen = new Label()
            {
                Text = "🎦 MÀN HÌNH",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            screenDisplay.Controls.Add(lblScreen);

            screenDisplay.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(149, 165, 166), 3))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, screenDisplay.Width - 1, screenDisplay.Height - 1);
                }
            };

            pnlScreen.Controls.Add(screenDisplay);
            pnlScreen.Resize += (s, e) =>
            {
                screenDisplay.Left = (pnlScreen.Width - screenDisplay.Width) / 2;
            };
            pnlMain.Controls.Add(pnlScreen);

            // Seat map container
            Panel seatMapContainer = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 39, 46),
                AutoScroll = true,
                Padding = new Padding(20)
            };

            pnlSeatMap = new Panel()
            {
                BackColor = Color.FromArgb(44, 62, 80),
                AutoSize = true,
                Padding = new Padding(30)
            };

            seatMapContainer.Controls.Add(pnlSeatMap);
            seatMapContainer.Resize += (s, e) =>
            {
                if (pnlSeatMap.Width < seatMapContainer.Width)
                {
                    pnlSeatMap.Left = (seatMapContainer.Width - pnlSeatMap.Width) / 2;
                }
            };
            pnlMain.Controls.Add(seatMapContainer);

            // Legend panel
            pnlLegend = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                BackColor = Color.FromArgb(44, 62, 80),
                Padding = new Padding(30, 15, 30, 15)
            };

            Label lblLegendTitle = new Label()
            {
                Text = "CHÚ THÍCH:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 15)
            };
            pnlLegend.Controls.Add(lblLegendTitle);

            FlowLayoutPanel legendFlow = new FlowLayoutPanel()
            {
                Location = new Point(30, 45),
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            legendFlow.Controls.Add(CreateLegendItem("Ghế Thường", COLOR_NORMAL_EMPTY));
            legendFlow.Controls.Add(CreateLegendItem("Ghế VIP (F-J) +50%", COLOR_VIP_EMPTY));
            legendFlow.Controls.Add(CreateLegendItem("Ghế Đôi (L+) +100%", COLOR_COUPLE_EMPTY));
            legendFlow.Controls.Add(CreateLegendItem("Đã Đặt", COLOR_BOOKED));
            legendFlow.Controls.Add(CreateLegendItem("Đang Chọn", COLOR_SELECTED));

            pnlLegend.Controls.Add(legendFlow);
            pnlMain.Controls.Add(pnlLegend);
        }

        private void CreateSidebar()
        {
            pnlSidebar = new Panel()
            {
                Dock = DockStyle.Right,
                Width = 380,
                BackColor = Color.FromArgb(44, 62, 80),
                Padding = new Padding(20)
            };

            // Title
            Label lblSidebarTitle = new Label()
            {
                Text = "🎫 THÔNG TIN ĐẶT VÉ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            pnlSidebar.Controls.Add(lblSidebarTitle);

            // Divider
            Panel divider1 = new Panel()
            {
                Height = 2,
                BackColor = Color.FromArgb(149, 165, 166),
                Location = new Point(20, 60),
                Width = 340
            };
            pnlSidebar.Controls.Add(divider1);

            // Selected seats label
            Label lblSelectedTitle = new Label()
            {
                Text = "Ghế đã chọn:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 80)
            };
            pnlSidebar.Controls.Add(lblSelectedTitle);

            // ListBox for selected seats
            lstSelectedSeats = new ListBox()
            {
                Location = new Point(20, 115),
                Width = 340,
                Height = 300,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            pnlSidebar.Controls.Add(lstSelectedSeats);

            // Total price panel
            Panel totalPanel = new Panel()
            {
                Location = new Point(20, 430),
                Width = 340,
                Height = 80,
                BackColor = Color.FromArgb(39, 174, 96),
                Padding = new Padding(15)
            };

            Label lblTotalLabel = new Label()
            {
                Text = "TỔNG TIỀN:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            totalPanel.Controls.Add(lblTotalLabel);

            lblTotalPrice = new Label()
            {
                Text = "0 VND",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 38)
            };
            totalPanel.Controls.Add(lblTotalPrice);

            pnlSidebar.Controls.Add(totalPanel);

            // Book button
            btnBookSeats = new Button()
            {
                Text = "🎬 ĐẶT VÉ NGAY",
                Location = new Point(20, 530),
                Width = 340,
                Height = 50,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnBookSeats.FlatAppearance.BorderSize = 0;
            btnBookSeats.Click += BtnBookSeats_Click;
            pnlSidebar.Controls.Add(btnBookSeats);

            // Clear button
            btnClearSelection = new Button()
            {
                Text = "🗑️ Xóa tất cả",
                Location = new Point(20, 590),
                Width = 340,
                Height = 45,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClearSelection.FlatAppearance.BorderSize = 0;
            btnClearSelection.Click += BtnClearSelection_Click;
            pnlSidebar.Controls.Add(btnClearSelection);

            pnlMain.Controls.Add(pnlSidebar);
        }

        private Panel CreateLegendItem(string text, Color color)
        {
            Panel item = new Panel()
            {
                Width = 200,
                Height = 40,
                Margin = new Padding(0, 0, 15, 0)
            };

            Panel colorBox = new Panel()
            {
                Width = 35,
                Height = 35,
                BackColor = color,
                Location = new Point(0, 2)
            };
            colorBox.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, colorBox.ClientRectangle,
                    Color.White, 2, ButtonBorderStyle.Solid,
                    Color.White, 2, ButtonBorderStyle.Solid,
                    Color.White, 2, ButtonBorderStyle.Solid,
                    Color.White, 2, ButtonBorderStyle.Solid);
            };

            Label label = new Label()
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(45, 8)
            };

            item.Controls.Add(colorBox);
            item.Controls.Add(label);

            return item;
        }

        private async void LoadSeatsAsync()
        {
            pnlSeatMap.Controls.Clear();
            seatButtons.Clear();

            try
            {
                allSeats = await _seatBLL.GetSeatsForShowtimeAsync(_movieId, _hallId, _showtimeId);

                if (allSeats.Count == 0)
                {
                    Label lblEmpty = new Label()
                    {
                        Text = "😥 Không có ghế nào",
                        Font = new Font("Segoe UI", 14, FontStyle.Italic),
                        ForeColor = Color.White,
                        AutoSize = true
                    };
                    pnlSeatMap.Controls.Add(lblEmpty);
                    return;
                }

                var seatsByRow = allSeats
                    .GroupBy(s => s.Name[0])
                    .OrderBy(g => g.Key)
                    .ToList();

                int yPosition = 0;
                int seatSize = 50;
                int seatSpacing = 8;

                foreach (var row in seatsByRow)
                {
                    Label lblRow = new Label()
                    {
                        Text = row.Key.ToString(),
                        Font = new Font("Segoe UI", 14, FontStyle.Bold),
                        ForeColor = Color.White,
                        AutoSize = false,
                        Width = 40,
                        Height = seatSize,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(0, yPosition)
                    };
                    pnlSeatMap.Controls.Add(lblRow);

                    var seatsInRow = row.OrderBy(s => ExtractSeatNumber(s.Name)).ToList();
                    int xPosition = 50;

                    for (int i = 0; i < seatsInRow.Count; i++)
                    {
                        var seat = seatsInRow[i];
                        char rowChar = seat.Name[0];
                        bool isVIP = "FGHIJ".Contains(rowChar);
                        bool isCouple = rowChar >= 'L';
                        bool isBooked = seat.Status == SeatBookingStatus.Booked;

                        Button btnSeat = new Button()
                        {
                            Text = seat.Name,
                            Width = isCouple ? (seatSize * 2 + seatSpacing) : seatSize,
                            Height = seatSize,
                            Location = new Point(xPosition, yPosition),
                            Tag = seat,
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Segoe UI", 9, FontStyle.Bold),
                            ForeColor = Color.White,
                            Cursor = isBooked ? Cursors.No : Cursors.Hand
                        };

                        if (isBooked)
                        {
                            btnSeat.BackColor = COLOR_BOOKED;
                            btnSeat.Enabled = false;
                        }
                        else if (isCouple)
                        {
                            btnSeat.BackColor = COLOR_COUPLE_EMPTY;
                        }
                        else if (isVIP)
                        {
                            btnSeat.BackColor = COLOR_VIP_EMPTY;
                        }
                        else
                        {
                            btnSeat.BackColor = COLOR_NORMAL_EMPTY;
                        }

                        btnSeat.FlatAppearance.BorderSize = 2;
                        btnSeat.FlatAppearance.BorderColor = Color.FromArgb(52, 73, 94);

                        btnSeat.Click += BtnSeat_Click;

                        if (!isBooked)
                        {
                            btnSeat.MouseEnter += (s, e) =>
                            {
                                if (btnSeat.BackColor != COLOR_SELECTED)
                                {
                                    btnSeat.FlatAppearance.BorderColor = Color.White;
                                }
                            };
                            btnSeat.MouseLeave += (s, e) =>
                            {
                                if (btnSeat.BackColor != COLOR_SELECTED)
                                {
                                    btnSeat.FlatAppearance.BorderColor = Color.FromArgb(52, 73, 94);
                                }
                            };
                        }

                        seatButtons[seat.Name] = btnSeat;
                        pnlSeatMap.Controls.Add(btnSeat);

                        xPosition += btnSeat.Width + seatSpacing;

                        if (isCouple && i + 1 < seatsInRow.Count)
                        {
                            i++;
                        }

                        if ((i + 1) % 4 == 0 && i + 1 < seatsInRow.Count)
                        {
                            xPosition += 20;
                        }
                    }

                    yPosition += seatSize + seatSpacing + 5;
                }

                pnlSeatMap.Width = 800;
                pnlSeatMap.Height = yPosition + 30;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải ghế: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int ExtractSeatNumber(string seatName)
        {
            string numberPart = new string(seatName.Where(char.IsDigit).ToArray());
            return int.TryParse(numberPart, out int number) ? number : 0;
        }

        private void BtnSeat_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            SeatInfo seat = btn.Tag as SeatInfo;
            if (seat == null || seat.Status == SeatBookingStatus.Booked) return;

            // Toggle selection
            if (selectedSeats.Contains(seat))
            {
                // Bỏ chọn
                selectedSeats.Remove(seat);
                char rowChar = seat.Name[0];
                bool isVIP = "FGHIJ".Contains(rowChar);
                bool isCouple = rowChar >= 'L';

                if (isCouple)
                    btn.BackColor = COLOR_COUPLE_EMPTY;
                else if (isVIP)
                    btn.BackColor = COLOR_VIP_EMPTY;
                else
                    btn.BackColor = COLOR_NORMAL_EMPTY;
            }
            else
            {
                // Chọn ghế
                selectedSeats.Add(seat);
                btn.BackColor = COLOR_SELECTED;
                btn.FlatAppearance.BorderColor = Color.White;
            }

            UpdateSidebar();
        }

        private void UpdateSidebar()
        {
            lstSelectedSeats.Items.Clear();
            decimal totalPrice = 0;

            foreach (var seat in selectedSeats.OrderBy(s => s.Name))
            {
                decimal seatPrice = CalculateSeatPrice(seat);
                totalPrice += seatPrice;

                string seatType = GetSeatType(seat.Name[0]);
                lstSelectedSeats.Items.Add($"{seat.Name} - {seatType} - {seatPrice:N0} VND");
            }

            lblTotalPrice.Text = $"{totalPrice:N0} VND";
            btnBookSeats.Enabled = selectedSeats.Count > 0;

            lblMovieInfo.Text = selectedSeats.Count > 0
                ? $"Giá vé cơ bản: {_pricePerSeat:N0} VND | Đã chọn {selectedSeats.Count} ghế"
                : $"Giá vé cơ bản: {_pricePerSeat:N0} VND | Vui lòng chọn ghế";
        }

        private decimal CalculateSeatPrice(SeatInfo seat)
        {
            char rowChar = seat.Name[0];

            if ("FGHIJ".Contains(rowChar)) // VIP
                return _pricePerSeat * 1.5m;
            else if (rowChar >= 'L') // Couple
                return _pricePerSeat * 2m;
            else // Normal
                return _pricePerSeat;
        }

        private string GetSeatType(char rowChar)
        {
            if ("FGHIJ".Contains(rowChar))
                return "Ghế VIP";
            else if (rowChar >= 'L')
                return "Ghế Đôi";
            else
                return "Ghế Thường";
        }

        private void BtnClearSelection_Click(object sender, EventArgs e)
        {
            if (selectedSeats.Count == 0) return;

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa tất cả ghế đã chọn?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                // Reset all selected seats
                foreach (var seat in selectedSeats.ToList())
                {
                    if (seatButtons.ContainsKey(seat.Name))
                    {
                        Button btn = seatButtons[seat.Name];
                        char rowChar = seat.Name[0];
                        bool isVIP = "FGHIJ".Contains(rowChar);
                        bool isCouple = rowChar >= 'L';

                        if (isCouple)
                            btn.BackColor = COLOR_COUPLE_EMPTY;
                        else if (isVIP)
                            btn.BackColor = COLOR_VIP_EMPTY;
                        else
                            btn.BackColor = COLOR_NORMAL_EMPTY;
                    }
                }

                selectedSeats.Clear();
                UpdateSidebar();
            }
        }

        private void BtnBookSeats_Click(object sender, EventArgs e)
        {
            if (selectedSeats.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 ghế!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal totalPrice = selectedSeats.Sum(s => CalculateSeatPrice(s));
            string seatList = string.Join(", ", selectedSeats.Select(s => s.Name).OrderBy(n => n));

            var result = MessageBox.Show(
                $"XÁC NHẬN ĐẶT VÉ\n\n" +
                $"Số ghế: {selectedSeats.Count}\n" +
                $"Danh sách ghế: {seatList}\n\n" +
                $"Tổng tiền: {totalPrice:N0} VND\n\n" +
                $"Xác nhận đặt vé?",
                "Xác nhận đặt vé",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    // TODO: Gọi API đặt vé ở đây
                    // await _seatBLL.BookSeatsAsync(selectedSeats, _showtimeId, ...);

                    MessageBox.Show(
                        $"✅ Đặt vé thành công!\n\n" +
                        $"Ghế: {seatList}\n" +
                        $"Tổng tiền: {totalPrice:N0} VND",
                        "Thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Reload seats to update status
                    selectedSeats.Clear();
                    UpdateSidebar();
                    LoadSeatsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đặt vé: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}