using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using TicketBookingStaffApp.BLL;
using TicketBookingStaffApp.Models;
using System.Net.Http;

namespace TicketBookingStaffApp.GUI
{
    // Dù không dùng Designer, ta vẫn phải khai báo partial class nếu có Designer.cs rỗng
    public partial class frmTicketDetails : Form
    {
        private readonly TicketBLL _ticketBLL = new TicketBLL();
        private readonly string _ticketId;

        // Constructor: Nhận ID vé khi khởi tạo Form
        public frmTicketDetails(string ticketId)
        {
            // Gọi phương thức tự tạo giao diện thay cho InitializeComponent()
            InitializeComponent_Manual();

            _ticketId = ticketId;
            this.Text = "Chi tiết vé";

            // Yêu cầu Maximize Form
            this.WindowState = FormWindowState.Maximized;

            // Cần căn giữa lại Panel khi Form Maximize hoặc Resize
            this.Resize += new EventHandler(frmTicketDetails_Resize);
            this.Load += new EventHandler(frmTicketDetails_Load);
        }

        // Phương thức tự tạo giao diện (Thay thế InitializeComponent từ Designer)
        private void InitializeComponent_Manual()
        {
            // --- 1. KHỞI TẠO VÀ THIẾT LẬP FORM CHÍNH ---
            this.SuspendLayout();
            this.BackColor = System.Drawing.Color.LightGray; // Nền Form
            this.Name = "frmTicketDetails";
            this.Text = "Chi tiết vé";
            this.ClientSize = new System.Drawing.Size(1024, 768); // Kích thước ban đầu

            // --- 2. KHỞI TẠO CONTAINER VÉ (pnlTicketContainer) ---
            this.pnlTicketContainer = new System.Windows.Forms.Panel();
            this.pnlTicketContainer.Size = new System.Drawing.Size(350, 700); // Kích thước cố định của vé dọc
            this.pnlTicketContainer.BackColor = System.Drawing.Color.White;
            this.pnlTicketContainer.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(this.pnlTicketContainer);

            // Căn giữa Panel ngay khi khởi tạo
            this.pnlTicketContainer.Left = (this.ClientSize.Width - this.pnlTicketContainer.Width) / 2;
            this.pnlTicketContainer.Top = (this.ClientSize.Height - this.pnlTicketContainer.Height) / 2;

            int currentY = 15; // Điểm bắt đầu vẽ Controls trong Panel
            int paddingX = 20;

            // --- 3. KHỞI TẠO VÀ ĐỊNH VỊ CÁC CONTROLS CON TRONG PANEL ---

            // Tiêu đề
            this.labelTitle = CreateLabel("PHIẾU XÁC NHẬN VÉ", new Font("Arial", 16F, FontStyle.Bold), ContentAlignment.MiddleCenter, 310, 30);
            this.labelTitle.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.labelTitle);
            currentY += 40;

            // Poster Phim
            this.pbMovieImage = new PictureBox();
            this.pbMovieImage.Location = new Point(125, currentY);
            this.pbMovieImage.Size = new System.Drawing.Size(100, 150);
            this.pbMovieImage.SizeMode = PictureBoxSizeMode.Zoom;
            this.pbMovieImage.BorderStyle = BorderStyle.FixedSingle;
            this.pnlTicketContainer.Controls.Add(this.pbMovieImage);
            currentY += 160;

            // Tên Phim
            this.labelMovieNameTitle = CreateLabel("TÊN PHIM", new Font("Arial", 9F, FontStyle.Bold), ContentAlignment.MiddleLeft, 310, 15);
            this.labelMovieNameTitle.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.labelMovieNameTitle);
            currentY += 18;

            this.lblMovieName = CreateLabel("[Bí Ẩn Thành Phố]", new Font("Arial", 14F, FontStyle.Bold), ContentAlignment.MiddleCenter, 310, 25);
            this.lblMovieName.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.lblMovieName);
            currentY += 35;

            // Thời lượng & Rating (Nằm ngang)
            this.lblDuration = CreateLabel("[Thời lượng: 2h05m]", new Font("Arial", 9F), ContentAlignment.MiddleLeft, 150, 15);
            this.lblDuration.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.lblDuration);

            this.lblRating = CreateLabel("[Rating: 10.0]", new Font("Arial", 9F), ContentAlignment.MiddleRight, 150, 15);
            this.lblRating.Location = new Point(paddingX + 160, currentY);
            this.pnlTicketContainer.Controls.Add(this.lblRating);
            currentY += 30;

            // Rạp
            this.labelTheatreTitle = CreateLabel("RẠP CHIẾU", new Font("Arial", 9F, FontStyle.Bold), ContentAlignment.MiddleLeft, 310, 15);
            this.labelTheatreTitle.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.labelTheatreTitle);
            currentY += 18;

            this.lblTheatreName = CreateLabel("[CGV Thanh Xuan]", new Font("Arial", 10F, FontStyle.Regular), ContentAlignment.MiddleCenter, 310, 20);
            this.lblTheatreName.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.lblTheatreName);
            currentY += 25;

            this.lblHallName = CreateLabel("[Room 1 (Thanh Xuân - HN)]", new Font("Arial", 9F, FontStyle.Italic), ContentAlignment.MiddleCenter, 310, 15);
            this.lblHallName.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.lblHallName);
            currentY += 30;

            // Suất Chiếu (Date & Time)
            this.labelShowtimeTitle = CreateLabel("SUẤT CHIẾU", new Font("Arial", 9F, FontStyle.Bold), ContentAlignment.MiddleLeft, 310, 15);
            this.labelShowtimeTitle.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.labelShowtimeTitle);
            currentY += 18;

            this.lblDate = CreateLabel("[04/06/2025]", new Font("Arial", 14F, FontStyle.Bold), ContentAlignment.MiddleLeft, 150, 25);
            this.lblDate.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.lblDate);

            this.lblShowTime = CreateLabel("[00:00]", new Font("Arial", 14F, FontStyle.Bold), ContentAlignment.MiddleRight, 150, 25);
            this.lblShowTime.Location = new Point(paddingX + 160, currentY);
            this.pnlTicketContainer.Controls.Add(this.lblShowTime);
            currentY += 35;

            this.lblShowType = CreateLabel("[Định dạng: 2D]", new Font("Arial", 9F, FontStyle.Regular), ContentAlignment.MiddleCenter, 310, 15);
            this.lblShowType.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.lblShowType);
            currentY += 30;

            // Ghế
            this.labelSeatsTitle = CreateLabel("GHẾ ĐÃ CHỌN", new Font("Arial", 9F, FontStyle.Bold), ContentAlignment.MiddleLeft, 310, 15);
            this.labelSeatsTitle.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.labelSeatsTitle);
            currentY += 18;

            this.lblSeats = CreateLabel("[A2, A3]", new Font("Arial", 18F, FontStyle.Bold), ContentAlignment.MiddleCenter, 310, 30);
            this.lblSeats.ForeColor = System.Drawing.Color.DarkRed;
            this.lblSeats.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.lblSeats);
            currentY += 45;

            // Tổng Tiền
            this.labelTotalPriceTitle = CreateLabel("TỔNG TIỀN", new Font("Arial", 9F, FontStyle.Bold), ContentAlignment.MiddleLeft, 310, 15);
            this.labelTotalPriceTitle.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.labelTotalPriceTitle);
            currentY += 18;

            this.lblTotalPrice = CreateLabel("[110.000 VNĐ]", new Font("Arial", 16F, FontStyle.Bold), ContentAlignment.MiddleCenter, 310, 25);
            this.lblTotalPrice.ForeColor = System.Drawing.Color.Green;
            this.lblTotalPrice.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.lblTotalPrice);
            currentY += 45;

            // Mã Vé
            this.labelTicketIdTitle = CreateLabel("MÃ VÉ (ID)", new Font("Arial", 7F, FontStyle.Regular), ContentAlignment.MiddleLeft, 310, 15);
            this.labelTicketIdTitle.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.labelTicketIdTitle);
            currentY += 15;

            this.lblTicketId = CreateLabel("[0d07f46b-de27...]", new Font("Arial", 8F, FontStyle.Regular), ContentAlignment.MiddleCenter, 310, 15);
            this.lblTicketId.Location = new Point(paddingX, currentY);
            this.pnlTicketContainer.Controls.Add(this.lblTicketId);
            // currentY += 20; // Kết thúc

            // Controls không dùng (Giữ lại để logic không lỗi)
            this.lblSynopsis = new System.Windows.Forms.Label();
            this.lblCast = new System.Windows.Forms.Label();
            this.Controls.Add(this.lblSynopsis);
            this.Controls.Add(this.lblCast);
            this.lblSynopsis.Visible = false;
            this.lblCast.Visible = false;

            this.ResumeLayout(false);
        }

        // Hàm helper để tạo Label nhanh chóng
        private Label CreateLabel(string text, Font font, ContentAlignment align, int width, int height)
        {
            return new Label
            {
                Text = text,
                Font = font,
                TextAlign = align,
                Size = new Size(width, height),
                AutoSize = false // Quan trọng để dùng Size cố định
            };
        }

        // Phương thức căn giữa Panel khi Form resize
        private void frmTicketDetails_Resize(object sender, EventArgs e)
        {
            if (this.pnlTicketContainer != null)
            {
                // Căn Panel vé vào giữa Form
                this.pnlTicketContainer.Left = (this.ClientSize.Width - this.pnlTicketContainer.Width) / 2;
                this.pnlTicketContainer.Top = (this.ClientSize.Height - this.pnlTicketContainer.Height) / 2;
            }
        }

        // Sự kiện Load Form (Async)
        private async void frmTicketDetails_Load(object sender, EventArgs e)
        {
            // Cần căn giữa Panel lần nữa sau khi Load và Maximize
            frmTicketDetails_Resize(sender, e);
            await LoadTicketDetailsAsync();
        }

        // Phương thức chính để tải dữ liệu bất đồng bộ (Giữ nguyên)
        private async Task LoadTicketDetailsAsync()
        {
            if (string.IsNullOrEmpty(_ticketId))
            {
                MessageBox.Show("Mã vé không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            try
            {
                TicketDetailsDto ticketDetails = await _ticketBLL.GetTicketDetailsAsync(_ticketId);
                DisplayTicketDetails(ticketDetails);

                if (!string.IsNullOrEmpty(ticketDetails.movie.image_path))
                {
                    await LoadMovieImageAsync(ticketDetails.movie.image_path);
                }
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                MessageBox.Show($"Lỗi: Không tìm thấy vé với ID: {_ticketId}. Vui lòng kiểm tra lại.", "Không tìm thấy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải chi tiết vé: {ex.InnerException?.Message ?? ex.Message}", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Phương thức ánh xạ dữ liệu lên controls (Giữ nguyên)
        private void DisplayTicketDetails(TicketDetailsDto data)
        {
            lblTicketId.Text = data.id;

            if (decimal.TryParse(data.total_price, out decimal totalPrice))
            {
                lblTotalPrice.Text = $"{totalPrice:N0} VNĐ";
            }
            else
            {
                lblTotalPrice.Text = data.total_price;
            }

            lblMovieName.Text = data.movie.name;
            lblDuration.Text = $"Thời lượng: {data.movie.duration}";
            lblRating.Text = $"Rating: {data.movie.rating}";
            lblSynopsis.Text = data.movie.synopsis;
            lblCast.Text = data.movie.top_cast;

            lblTheatreName.Text = data.hall.theatre.name;
            lblHallName.Text = $"Phòng: {data.hall.name} ({data.hall.theatre.locationDetails})";
            lblShowTime.Text = data.showtime.movie_start_time;

            if (DateTime.TryParse(data.showtime.showtime_date, out DateTime showDate))
            {
                lblDate.Text = showDate.ToString("dd/MM/yyyy");
            }
            else
            {
                lblDate.Text = data.showtime.showtime_date;
            }

            lblShowType.Text = $"Định dạng: {data.showtime.show_type}";

            string seatsList = string.Join(", ", data.seats.Select(s => s.seat.name));
            lblSeats.Text = seatsList;
        }

        // Phương thức tải ảnh từ URL bất đồng bộ (Giữ nguyên)
        private async Task LoadMovieImageAsync(string imageUrl)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] imageBytes = await client.DownloadDataTaskAsync(new Uri(imageUrl));

                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        pbMovieImage.Image = Image.FromStream(ms);
                        pbMovieImage.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
            }
            catch (Exception)
            {
                pbMovieImage.Image = null;
            }
        }
    }
}