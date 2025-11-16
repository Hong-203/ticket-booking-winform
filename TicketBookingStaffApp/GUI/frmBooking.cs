using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TicketBookingStaffApp.BLL;
using TicketBookingStaffApp.DAL.repositories; // Cần đảm bảo các class SeatInfo có ở đây
using TicketBookingStaffApp.Utils;
using static TicketBookingStaffApp.DAL.repositories.SeatBookRepository;

namespace TicketBookingStaffApp.GUI
{
    // Đổi tên Form từ frmSeat sang frmBooking
    public partial class frmBooking : Form
    {
        private readonly SeatBLL _seatBLL = new SeatBLL();

        // Khai báo User Control và các thành phần UI của Sidebar
        private SeatMapControl _seatMapControl;
        private Panel pnlMain;
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

        // Danh sách ghế đã chọn, được cập nhật từ SeatMapControl qua Event
        private List<SeatInfo> _currentSelectedSeats = new List<SeatInfo>();

        public frmBooking(string movieId, string hallId, string showtimeId, decimal price)
        {
            _movieId = movieId;
            _hallId = hallId;
            _showtimeId = showtimeId;
            _pricePerSeat = price;

            InitializeComponent();
            InitializeUI();

            // 1. Khởi tạo và thêm SeatMapControl
            _seatMapControl = new SeatMapControl();
            _seatMapControl.Dock = DockStyle.Fill;

            // 2. Đăng ký sự kiện từ User Control
            _seatMapControl.SelectionChanged += SeatMapControl_SelectionChanged;

            // Thêm _seatMapControl vào pnlMain, đặt dưới pnlSidebar (vì pnlSidebar có Dock = Right)
            // pnlMain là Container chính của Form
            pnlMain.Controls.Add(_seatMapControl);
            pnlMain.Controls.SetChildIndex(_seatMapControl, 0); // Đảm bảo nó nằm dưới pnlSidebar

            // 3. Gọi hàm LoadSeats của User Control
            _seatMapControl.LoadSeats(_movieId, _hallId, _showtimeId, _pricePerSeat);
        }

        // ====================================================================
        // KHỞI TẠO UI (Chỉ giữ lại các thành phần không thuộc sơ đồ ghế)
        // ====================================================================
        private void InitializeUI()
        {
            this.Text = "Sơ Đồ Ghế Ngồi & Đặt Vé";
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
            pnlMain.Controls.Add(pnlSidebar);

            // Header (Giữ lại để hiển thị thông tin phim/giá vé)
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
            pnlMain.Controls.SetChildIndex(headerPanel, pnlMain.Controls.Count - 1); // Đặt Header ở trên cùng
        }

        private void CreateSidebar()
        {
            // Tái sử dụng logic sidebar từ Form cũ
            pnlSidebar = new Panel()
            {
                Dock = DockStyle.Right,
                Width = 380,
                BackColor = Color.FromArgb(44, 62, 80),
                Padding = new Padding(20)
            };

            // Title, Divider, Labels, ListBox, Total Panel, Buttons
            // (Phần này giữ nguyên logic code UI của Sidebar từ code gốc của bạn)

            // ... (Thêm lblSidebarTitle)
            // ... (Thêm divider1)

            Label lblSelectedTitle = new Label()
            {
                Text = "Ghế đã chọn:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 80)
            };
            pnlSidebar.Controls.Add(lblSelectedTitle);

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

            Panel totalPanel = new Panel()
            {
                Location = new Point(20, 430),
                Width = 340,
                Height = 80,
                BackColor = Color.FromArgb(39, 174, 96),
                Padding = new Padding(15)
            };
            Label lblTotalLabel = new Label() { Text = "TỔNG TIỀN:", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 12) };
            lblTotalPrice = new Label() { Text = "0 VND", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 38) };
            totalPanel.Controls.Add(lblTotalLabel);
            totalPanel.Controls.Add(lblTotalPrice);
            pnlSidebar.Controls.Add(totalPanel);

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
        }

        // ====================================================================
        // XỬ LÝ SỰ KIỆN TỪ USER CONTROL
        // ====================================================================

        // 4. HÀM XỬ LÝ SỰ KIỆN KHI GHẾ BỊ THAY ĐỔI
        private void SeatMapControl_SelectionChanged(object sender, SeatSelectionEventArgs e)
        {
            // Cập nhật danh sách ghế và tổng tiền từ sự kiện
            _currentSelectedSeats = e.SelectedSeats;
            UpdateSidebarUI(e.SelectedSeats, e.TotalPrice);
        }

        // 5. HÀM CẬP NHẬT UI SIDEBAR
        private void UpdateSidebarUI(List<SeatInfo> selectedSeats, decimal totalPrice)
        {
            lstSelectedSeats.Items.Clear();

            foreach (var seat in selectedSeats.OrderBy(s => s.Name))
            {
                // Gọi CalculateSeatPrice/GetSeatType từ User Control để đảm bảo tính nhất quán
                decimal seatPrice = _seatMapControl.CalculateSeatPrice(seat);
                string seatType = GetSeatType(seat.Name[0]);
                lstSelectedSeats.Items.Add($"{seat.Name} - {seatType} - {seatPrice:N0} VND");
            }

            lblTotalPrice.Text = $"{totalPrice:N0} VND";
            btnBookSeats.Enabled = selectedSeats.Count > 0;

            lblMovieInfo.Text = selectedSeats.Count > 0
                ? $"Giá vé cơ bản: {_pricePerSeat:N0} VND | Đã chọn **{selectedSeats.Count}** ghế"
                : $"Giá vé cơ bản: {_pricePerSeat:N0} VND | Vui lòng chọn ghế";
        }

        // Hàm phụ trợ (giữ lại ở đây hoặc di chuyển hoàn toàn vào UC)
        private string GetSeatType(char rowChar)
        {
            if ("FGHIJ".Contains(rowChar))
                return "Ghế VIP";
            else if (rowChar >= 'L')
                return "Ghế Đôi";
            else
                return "Ghế Thường";
        }

        // ====================================================================
        // XỬ LÝ BUTTON
        // ====================================================================

        // 6. HÀM XÓA CHỌN (GỌI VÀO USER CONTROL)
        private void BtnClearSelection_Click(object sender, EventArgs e)
        {
            if (_currentSelectedSeats.Count == 0) return;

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa tất cả ghế đã chọn?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                // Gọi hàm của User Control để xử lý logic xóa chọn và kích hoạt sự kiện Update
                _seatMapControl.ClearSelection();
            }
        }

        // 7. HÀM ĐẶT VÉ (LOGIC CHÍNH CỦA FORM)
        // 7. HÀM ĐẶT VÉ (LOGIC CHÍNH CỦA FORM)
        // Trong frmBooking.cs

        private async void BtnBookSeats_Click(object sender, EventArgs e)
        {
            if (_currentSelectedSeats.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 ghế!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal totalPrice = _currentSelectedSeats.Sum(s => _seatMapControl.CalculateSeatPrice(s));
            // Dùng check null an toàn và kiểm tra ở dưới
            string currentUserId = SessionManager.CurrentUser?.Id;

            if (string.IsNullOrEmpty(currentUserId))
            {
                MessageBox.Show("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Lỗi User Session", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string seatListDetail = string.Join("\n", _currentSelectedSeats.OrderBy(s => s.Name).Select(s => $"   - {s.Name} (ID: {s.Id})"));
            string seatNames = string.Join(", ", _currentSelectedSeats.Select(s => s.Name).OrderBy(n => n));


            var result = MessageBox.Show(
                $"XÁC NHẬN ĐẶT VÉ\n\n" +
                $"Thông tin nhân viên:\n" +
                $"  - User ID: **{currentUserId}**\n\n" +
                $"Thông tin suất chiếu:\n" +
                $"  - Movie ID: **{_movieId}**\n" +
                $"  - Hall ID: **{_hallId}**\n" +
                $"  - Showtime ID: **{_showtimeId}**\n\n" +
                $"Ghế đã chọn ({_currentSelectedSeats.Count} ghế):\n" +
                $"{seatListDetail}\n\n" +
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
                    // ✨ Nhận kết quả BookingResult từ BLL
                    var bookingResult = await _seatBLL.BookSeatsDirectlyAsync(
                        _currentSelectedSeats,
                        _movieId,
                        _hallId,
                        _showtimeId,
                        currentUserId,
                        totalPrice
                    );

                    // Lấy Ticket ID để hiển thị
                    string newTicketId = bookingResult.Ticket.Id;

                    // Xóa danh sách ghế đã chọn local
                    _currentSelectedSeats.Clear();

                    MessageBox.Show(
                        $"✅ ĐẶT VÉ THÀNH CÔNG!\n\n" +
                        $"Mã Ticket: **{newTicketId}**\n" + // ✨ HIỂN THỊ TICKET ID
                        $"Nhân viên: {currentUserId}\n" +
                        $"Ghế: {seatNames}\n" +
                        $"Tổng tiền: {totalPrice:N0} VND",
                        "Thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    frmTicketDetails detailsForm = new frmTicketDetails(newTicketId);
                    detailsForm.ShowDialog();

                    // Sau khi đặt thành công, RELOAD lại sơ đồ ghế
                    _seatMapControl.LoadSeats(_movieId, _hallId, _showtimeId, _pricePerSeat);
                }
                catch (Exception ex)
                {
                    // Bắt lỗi khi truy cập DB (bao gồm lỗi trùng lặp từ Transaction Rollback)
                    MessageBox.Show($"Đặt vé thất bại! Chi tiết: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}