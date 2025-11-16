using System;
using System.Drawing;
using System.Windows.Forms;
using TicketBookingStaffApp.BLL; // Cần thiết nếu bạn muốn validate ID ở đây

namespace TicketBookingStaffApp.GUI
{
    public partial class frmTicketIdInput : Form
{
    private TextBox txtTicketId;
    private Button btnViewDetails;
    private Label lblInstruction;

        public frmTicketIdInput()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
    {
        this.Text = "Nhập Mã Vé";
        this.Size = new Size(450, 250);
        this.BackColor = Color.FromArgb(30, 39, 46); // Background tối
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        // Label hướng dẫn
        lblInstruction = new Label()
        {
            Text = "Nhập Ticket ID để xem chi tiết:",
            Location = new Point(50, 30),
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White
        };

        // Input Box
        txtTicketId = new TextBox()
        {
            Location = new Point(50, 70),
            Size = new Size(330, 30),
            Font = new Font("Segoe UI", 10),
        };

        // Button Xem Chi Tiết
        btnViewDetails = new Button()
        {
            Text = "🔎 Xem Chi Tiết Vé",
            Location = new Point(50, 130),
            Size = new Size(330, 45),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            BackColor = Color.FromArgb(39, 174, 96), // Màu xanh lá
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnViewDetails.FlatAppearance.BorderSize = 0;
        btnViewDetails.Click += BtnViewDetails_Click;

        this.Controls.Add(lblInstruction);
        this.Controls.Add(txtTicketId);
        this.Controls.Add(btnViewDetails);
    }

        private void BtnViewDetails_Click(object sender, EventArgs e)
        {
            string ticketId = txtTicketId.Text.Trim();

            if (string.IsNullOrEmpty(ticketId))
            {
                MessageBox.Show("Vui lòng nhập Mã Vé.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // <<< ĐÃ SỬA: DÒNG KIỂM TRA ĐẢM BẢO GIÁ TRỊ ĐƯỢC NHẬN CHÍNH XÁC >>>
                // Dòng này phải hiển thị đúng ID bạn nhập.
                //MessageBox.Show($"ID nhận được: {ticketId}", "Kiểm tra ID", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Sau khi kiểm tra, hãy xóa dòng MessageBox.Show ở trên

                this.Hide();

                // Đảm bảo frmTicketDetails được định nghĩa và biên dịch thành công
                frmTicketDetails detailsForm = new frmTicketDetails(ticketId);
                detailsForm.ShowDialog();

                this.Show();
                this.txtTicketId.Clear();
                this.txtTicketId.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở form chi tiết hoặc form chi tiết gặp lỗi: {ex.Message}", "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Show();
            }
        }
    }
}