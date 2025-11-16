using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TicketBookingStaffApp.BLL;
using TicketBookingStaffApp.DAL.repositories;

public class SeatSelectionEventArgs : EventArgs
{
    public List<SeatInfo> SelectedSeats { get; }
    public decimal TotalPrice { get; }

    public SeatSelectionEventArgs(List<SeatInfo> selectedSeats, decimal totalPrice)
    {
        SelectedSeats = selectedSeats;
        TotalPrice = totalPrice;
    }
}
namespace TicketBookingStaffApp.GUI
{
    public partial class SeatMapControl : UserControl
    {
        public event EventHandler<SeatSelectionEventArgs> SelectionChanged;

        private readonly SeatBLL _seatBLL = new SeatBLL();

        public string ShowtimeId { get; set; }
        public decimal PricePerSeat { get; set; }

        private List<SeatInfo> allSeats = new List<SeatInfo>();
        private Dictionary<string, Button> seatButtons = new Dictionary<string, Button>();
        private List<SeatInfo> selectedSeats = new List<SeatInfo>();

        // Màu sắc
        private readonly Color COLOR_NORMAL_EMPTY = Color.FromArgb(46, 204, 113);
        private readonly Color COLOR_VIP_EMPTY = Color.FromArgb(241, 196, 15);
        private readonly Color COLOR_COUPLE_EMPTY = Color.FromArgb(155, 89, 182);
        private readonly Color COLOR_BOOKED = Color.FromArgb(231, 76, 60);
        private readonly Color COLOR_SELECTED = Color.FromArgb(52, 152, 219);

        // Controls
        private Panel pnlMainContainer;
        private Panel pnlScreen;
        private Panel pnlSeatMap;
        private Panel pnlLegend;

        public SeatMapControl()
        {
            InitializeControlUI();
        }

        private void InitializeControlUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(30, 39, 46);
            this.Padding = new Padding(10); // Giảm padding tổng thể

            pnlMainContainer = new Panel() { Dock = DockStyle.Fill, BackColor = this.BackColor };
            this.Controls.Add(pnlMainContainer);

            // --- 2. Legend Panel (Chú thích) ---
            // Đặt Legend ở dưới cùng (Dock.Bottom)
            pnlLegend = new Panel() { Dock = DockStyle.Bottom, Height = 80, BackColor = this.BackColor, Padding = new Padding(30, 10, 30, 10) };
            Label lblLegendTitle = new Label() { Text = "CHÚ THÍCH:", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(30, 5) };
            pnlLegend.Controls.Add(lblLegendTitle);
            FlowLayoutPanel legendFlow = new FlowLayoutPanel() { Location = new Point(30, 35), AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            legendFlow.Controls.Add(CreateLegendItem("Ghế Thường", COLOR_NORMAL_EMPTY));
            legendFlow.Controls.Add(CreateLegendItem("Ghế VIP (F-J) +50%", COLOR_VIP_EMPTY));
            legendFlow.Controls.Add(CreateLegendItem("Ghế Đôi (L+) +100%", COLOR_COUPLE_EMPTY));
            legendFlow.Controls.Add(CreateLegendItem("Đã Đặt", COLOR_BOOKED));
            legendFlow.Controls.Add(CreateLegendItem("Đang Chọn", COLOR_SELECTED));
            pnlLegend.Controls.Add(legendFlow);
            pnlMainContainer.Controls.Add(pnlLegend); // Legend added first

            // --- 1. Screen Panel (Màn hình chiếu) ---
            // Đặt Screen ở trên cùng (Dock.Top)
            pnlScreen = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = this.BackColor }; // Giảm chiều cao Screen
            Panel screenDisplay = new Panel() { Width = 800, Height = 40, BackColor = Color.FromArgb(52, 73, 94), Location = new Point(0, 10) };
            Label lblScreen = new Label() { Text = "🎦 MÀN HÌNH", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            screenDisplay.Controls.Add(lblScreen);
            screenDisplay.Paint += (s, e) => { using (Pen pen = new Pen(Color.FromArgb(149, 165, 166), 3)) { e.Graphics.DrawRectangle(pen, 0, 0, screenDisplay.Width - 1, screenDisplay.Height - 1); } };
            pnlScreen.Controls.Add(screenDisplay);
            pnlScreen.Resize += (s, e) => { screenDisplay.Left = (pnlScreen.Width - screenDisplay.Width) / 2; };
            pnlMainContainer.Controls.Add(pnlScreen); // Screen added second

            // --- 3. Seat Map Panel (Sơ đồ ghế) ---
            // Đặt Seat Map Fill không gian còn lại
            Panel seatMapContainer = new Panel() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 39, 46), AutoScroll = false, Padding = new Padding(20) }; // Bỏ AutoScroll

            // pnlSeatMap là nơi chứa các nút ghế, phải có kích thước tự động (AutoSize=true) để tính toán đúng
            pnlSeatMap = new Panel() { BackColor = Color.FromArgb(44, 62, 80), AutoSize = true, Padding = new Padding(15) };

            seatMapContainer.Controls.Add(pnlSeatMap);

            // Căn giữa pnlSeatMap trong seatMapContainer (căn ngang)
            seatMapContainer.Resize += (s, e) => {
                if (pnlSeatMap.Width < seatMapContainer.Width) pnlSeatMap.Left = (seatMapContainer.Width - pnlSeatMap.Width) / 2;
                // Căn giữa Vertically (tùy chọn, nếu không gian quá lớn)
                if (pnlSeatMap.Height < seatMapContainer.Height) pnlSeatMap.Top = (seatMapContainer.Height - pnlSeatMap.Height) / 2;
                else pnlSeatMap.Top = 0; // Ưu tiên top nếu không đủ chỗ
            };
            pnlMainContainer.Controls.Add(seatMapContainer); // Seat Map added last (Dock.Fill)
        }

        private Panel CreateLegendItem(string text, Color color)
        {
            Panel item = new Panel() { Width = 200, Height = 40, Margin = new Padding(0, 0, 15, 0) };
            Panel colorBox = new Panel() { Width = 35, Height = 35, BackColor = color, Location = new Point(0, 2) };
            colorBox.Paint += (s, e) => { ControlPaint.DrawBorder(e.Graphics, colorBox.ClientRectangle, Color.White, 2, ButtonBorderStyle.Solid, Color.White, 2, ButtonBorderStyle.Solid, Color.White, 2, ButtonBorderStyle.Solid, Color.White, 2, ButtonBorderStyle.Solid); };
            Label label = new Label() { Text = text, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(45, 8) };
            item.Controls.Add(colorBox);
            item.Controls.Add(label);
            return item;
        }

        // 3. HÀM CHÍNH ĐỂ LOAD DỮ LIỆU
        public async void LoadSeats(string movieId, string hallId, string showtimeId, decimal pricePerSeat)
        {
            this.ShowtimeId = showtimeId;
            this.PricePerSeat = pricePerSeat;
            pnlSeatMap.Controls.Clear();
            seatButtons.Clear();
            selectedSeats.Clear();

            try
            {
                // Thay thế logic LoadSeatsAsync cũ
                 allSeats = await _seatBLL.GetSeatsForShowtimeAsync(movieId, hallId, showtimeId);

                // TẠM THỜI SỬ DỤNG DỮ LIỆU GIẢ ĐỂ TEST UI:
                //allSeats = GenerateDummySeats();

                DrawSeatMap();
                OnSelectionChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải ghế: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Dữ liệu giả định để test layout
        private List<SeatInfo> GenerateDummySeats()
        {
            var seats = new List<SeatInfo>();

            // Hàng A-E: Normal (12 ghế)
            for (char row = 'A'; row <= 'E'; row++)
            {
                for (int num = 1; num <= 24; num++)
                {
                    seats.Add(new SeatInfo { Name = $"{row}{num}", Status = SeatBookingStatus.Empty });
                }
            }

            // Hàng F-J: VIP (12 ghế)
            for (char row = 'F'; row <= 'J'; row++)
            {
                for (int num = 1; num <= 24; num++)
                {
                    // Ghế J7, J8 đã đặt
                    if (row == 'J' && (num == 7 || num == 8))
                    {
                        seats.Add(new SeatInfo { Name = $"{row}{num}", Status = SeatBookingStatus.Booked });
                    }
                    else
                    {
                        seats.Add(new SeatInfo { Name = $"{row}{num}", Status = SeatBookingStatus.Empty });
                    }
                }
            }

            // Hàng L: Couple (12 ghế đôi, đánh số lẻ)
            for (int num = 1; num <= 24; num++)
            {
                seats.Add(new SeatInfo { Name = $"L{num}", Status = SeatBookingStatus.Empty });
                // Giả định ghế đôi là L1-L2, L3-L4...
            }

            return seats;
        }

        // 4. HÀM VẼ SƠ ĐỒ GHẾ (Phục hồi logic DrawSeatMap)
        private void DrawSeatMap()
        {
            if (allSeats.Count == 0)
            {
                Label lblEmpty = new Label() { Text = "😥 Không có ghế nào", Font = new Font("Segoe UI", 14, FontStyle.Italic), ForeColor = Color.White, AutoSize = true };
                pnlSeatMap.Controls.Add(lblEmpty);
                return;
            }

            var seatsByRow = allSeats
                .GroupBy(s => s.Name[0])
                .OrderBy(g => g.Key)
                .ToList();

            int yPosition = 0;
            int seatSize = 35; // Tăng kích thước ghế lên một chút để dễ nhìn hơn
            int seatSpacing = 5;

            foreach (var row in seatsByRow)
            {
                // Thêm nhãn hàng ghế bên trái
                Label lblRow = new Label() { Text = row.Key.ToString(), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, AutoSize = false, Width = seatSize, Height = seatSize, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, yPosition) };
                pnlSeatMap.Controls.Add(lblRow);

                var seatsInRow = row.OrderBy(s => ExtractSeatNumber(s.Name)).ToList();
                int xPosition = seatSize + 10; // Bắt đầu sau nhãn hàng ghế và 10px spacing

                for (int i = 0; i < seatsInRow.Count; i++)
                {
                    var seat = seatsInRow[i];
                    char rowChar = seat.Name[0];
                    bool isVIP = "FGHIJ".Contains(rowChar);
                    bool isCouple = rowChar >= 'L';
                    bool isBooked = seat.Status == SeatBookingStatus.Booked;

                    // Đối với ghế đôi (L+), nút sẽ rộng gấp đôi
                    int currentSeatWidth = isCouple ? (seatSize * 2 + seatSpacing) : seatSize;

                    Button btnSeat = new Button()
                    {
                        Text = isCouple ? seat.Name.Replace("L", "") : seat.Name.Substring(1), // Hiển thị số ghế thôi cho hàng đôi/thường
                        Width = currentSeatWidth,
                        Height = seatSize,
                        Location = new Point(xPosition, yPosition),
                        Tag = seat,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = Color.White,
                        Cursor = isBooked ? Cursors.No : Cursors.Hand
                    };

                    btnSeat.FlatAppearance.BorderSize = 2;
                    btnSeat.FlatAppearance.BorderColor = Color.FromArgb(52, 73, 94);
                    btnSeat.Click += BtnSeat_Click;

                    // Thiết lập màu ban đầu và sự kiện hover
                    UpdateSeatButtonColor(btnSeat, seat);

                    if (!isBooked)
                    {
                        btnSeat.MouseEnter += (s, e) => { if (btnSeat.BackColor != COLOR_SELECTED) { btnSeat.FlatAppearance.BorderColor = Color.White; } };
                        btnSeat.MouseLeave += (s, e) => { if (btnSeat.BackColor != COLOR_SELECTED) { btnSeat.FlatAppearance.BorderColor = Color.FromArgb(52, 73, 94); } };
                    }

                    seatButtons[seat.Name] = btnSeat;
                    pnlSeatMap.Controls.Add(btnSeat);

                    xPosition += currentSeatWidth + seatSpacing;

                    // Logic khoảng cách giữa
                    if (isCouple && i + 1 < seatsInRow.Count) { i++; } // Bỏ qua ghế tiếp theo nếu là ghế đôi

                    // Thêm khoảng trống giữa nếu tổng số ghế <= 12
                    // Giả sử có 12 ghế/hàng, thêm khoảng cách sau ghế thứ 6 (index 5)
                    if (seatsInRow.Count > 0 && (i + 1) == seatsInRow.Count / 2 && seatsInRow.Count > 4)
                    {
                        xPosition += 20; // Khoảng cách lớn hơn giữa
                    }
                }

                yPosition += seatSize + seatSpacing + 5;
            }

            // Tính toán lại kích thước tối đa cho pnlSeatMap dựa trên nội dung
            int maxSeatWidth = seatsByRow.Max(r =>
            {
                var seats = r.OrderBy(s => ExtractSeatNumber(s.Name)).ToList();
                int width = seatSize + 10; // Row Label + spacing start
                for (int i = 0; i < seats.Count; i++)
                {
                    bool isCouple = seats[i].Name[0] >= 'L';
                    width += (isCouple ? seatSize * 2 + seatSpacing : seatSize) + seatSpacing;

                    if (isCouple && i + 1 < seats.Count) i++;
                    if ((i + 1) == seats.Count / 2 && seats.Count > 4) width += 20; // Khoảng cách giữa
                }
                return width;
            });

            // Đảm bảo chiều rộng đủ để chứa sơ đồ
            pnlSeatMap.Width = maxSeatWidth + 30;
            pnlSeatMap.Height = yPosition + 15; // Chiều cao tối thiểu + padding dưới
        }

        // 5. HÀM XỬ LÝ CLICK GHẾ
        private void BtnSeat_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            SeatInfo seat = btn?.Tag as SeatInfo;
            if (seat == null || seat.Status == SeatBookingStatus.Booked) return;

            // Toggle selection
            if (selectedSeats.Contains(seat))
            {
                selectedSeats.Remove(seat);
                UpdateSeatButtonColor(btn, seat); // Đổi màu về trạng thái trống
            }
            else
            {
                selectedSeats.Add(seat);
                btn.BackColor = COLOR_SELECTED; // Đổi màu sang trạng thái đã chọn
                btn.FlatAppearance.BorderColor = Color.White;
            }

            OnSelectionChanged();
        }

        // 7. HÀM GỌI SỰ KIỆN RA FORM CHA
        private void OnSelectionChanged()
        {
            decimal totalPrice = selectedSeats.Sum(s => CalculateSeatPrice(s));
            SelectionChanged?.Invoke(this, new SeatSelectionEventArgs(selectedSeats, totalPrice));
        }

        // 8. HÀM TÍNH GIÁ VÀ LOẠI GHẾ
        public decimal CalculateSeatPrice(SeatInfo seat)
        {
            char rowChar = seat.Name[0];

            if ("FGHIJ".Contains(rowChar)) // VIP (+50%)
                return PricePerSeat * 1.5m;
            else if (rowChar >= 'L') // Couple (+100%)
                return PricePerSeat * 2m;
            else // Normal
                return PricePerSeat;
        }

        // 9. HÀM XÓA CHỌN
        public void ClearSelection()
        {
            foreach (var seat in selectedSeats.ToList())
            {
                if (seatButtons.TryGetValue(seat.Name, out Button btn))
                {
                    UpdateSeatButtonColor(btn, seat);
                }
            }
            selectedSeats.Clear();
            OnSelectionChanged();
        }

        // --- CÁC HÀM PHỤ TRỢ ---

        // Hàm set màu ghế
        private void UpdateSeatButtonColor(Button btn, SeatInfo seat)
        {
            char rowChar = seat.Name[0];
            bool isVIP = "FGHIJ".Contains(rowChar);
            bool isCouple = rowChar >= 'L';

            if (seat.Status == SeatBookingStatus.Booked)
            {
                btn.BackColor = COLOR_BOOKED;
                btn.Enabled = false;
            }
            else if (selectedSeats.Contains(seat))
            {
                btn.BackColor = COLOR_SELECTED;
                btn.FlatAppearance.BorderColor = Color.White;
            }
            else if (isCouple)
            {
                btn.BackColor = COLOR_COUPLE_EMPTY;
            }
            else if (isVIP)
            {
                btn.BackColor = COLOR_VIP_EMPTY;
            }
            else
            {
                btn.BackColor = COLOR_NORMAL_EMPTY;
            }

            if (!selectedSeats.Contains(seat) && !btn.Enabled)
            {
                btn.FlatAppearance.BorderColor = Color.FromArgb(52, 73, 94);
            }
        }

        private int ExtractSeatNumber(string seatName)
        {
            string numberPart = new string(seatName.Where(char.IsDigit).ToArray());
            return int.TryParse(numberPart, out int number) ? number : 0;
        }
    }
}