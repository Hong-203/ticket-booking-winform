using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TicketBookingStaffApp.BLL;
using TicketBookingStaffApp.Utils;

namespace TicketBookingStaffApp.GUI
{
    public partial class frmLogin : Form
    {
        private Panel pnlBackground;
        private Panel pnlLoginBox;
        private Label lblWelcome;
        private Label lblTitle;
        private Label lblIdentifier;
        private Label lblPassword;
        private TextBox txtIdentifier;
        private TextBox txtPassword;
        private Button btnLogin;
        private CheckBox chkShowPassword;
        private PictureBox picLogo;

        private readonly authBLL _auth = new authBLL();

        public frmLogin()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            // === Form chính ===
            this.Text = "Hệ thống đặt vé xem phim";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.Black;

            // === Panel nền với hình ảnh phim ===
            pnlBackground = new Panel();
            pnlBackground.Dock = DockStyle.Fill;
            pnlBackground.BackColor = Color.FromArgb(20, 20, 30);
            pnlBackground.Paint += PnlBackground_Paint;

            // === Panel box đăng nhập ===
            pnlLoginBox = new Panel();
            pnlLoginBox.Size = new Size(400, 480);
            pnlLoginBox.BackColor = Color.FromArgb(240, 255, 255, 255);
            pnlLoginBox.Paint += PnlLoginBox_Paint;

            // === Icon/Logo ===
            picLogo = new PictureBox();
            picLogo.Size = new Size(80, 80);
            picLogo.Location = new Point(160, 30);
            picLogo.SizeMode = PictureBoxSizeMode.CenterImage;
            picLogo.BackColor = Color.Transparent;
            picLogo.Paint += PicLogo_Paint;

            // === Chào mừng ===
            lblWelcome = new Label();
            lblWelcome.Text = "STAFF - CHÀO MỪNG QUAY TRỞ LẠI";
            lblWelcome.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblWelcome.ForeColor = Color.FromArgb(40, 40, 40);
            lblWelcome.Location = new Point(50, 120);
            lblWelcome.Size = new Size(300, 30);
            lblWelcome.TextAlign = ContentAlignment.MiddleCenter;

            // === Tiêu đề phụ ===
            lblTitle = new Label();
            lblTitle.Text = "Hệ thống quản lý đặt vé";
            lblTitle.Font = new Font("Segoe UI", 10);
            lblTitle.ForeColor = Color.FromArgb(100, 100, 100);
            lblTitle.Location = new Point(50, 155);
            lblTitle.Size = new Size(300, 20);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // === Nhãn Email/SĐT ===
            lblIdentifier = new Label();
            lblIdentifier.Text = "Email hoặc số điện thoại";
            lblIdentifier.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblIdentifier.ForeColor = Color.FromArgb(60, 60, 60);
            lblIdentifier.Location = new Point(50, 200);
            lblIdentifier.AutoSize = true;

            // === Ô nhập tài khoản ===
            txtIdentifier = new TextBox();
            txtIdentifier.Font = new Font("Segoe UI", 11);
            txtIdentifier.Location = new Point(50, 225);
            txtIdentifier.Size = new Size(300, 30);
            txtIdentifier.BorderStyle = BorderStyle.FixedSingle;
            txtIdentifier.BackColor = Color.White;

            // === Nhãn mật khẩu ===
            lblPassword = new Label();
            lblPassword.Text = "Mật khẩu";
            lblPassword.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(60, 60, 60);
            lblPassword.Location = new Point(50, 270);
            lblPassword.AutoSize = true;

            // === Ô nhập mật khẩu ===
            txtPassword = new TextBox();
            txtPassword.Font = new Font("Segoe UI", 11);
            txtPassword.Location = new Point(50, 295);
            txtPassword.Size = new Size(300, 30);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.PasswordChar = '●';
            txtPassword.BackColor = Color.White;

            // === Checkbox hiện mật khẩu ===
            chkShowPassword = new CheckBox();
            chkShowPassword.Text = "Hiển thị mật khẩu";
            chkShowPassword.Font = new Font("Segoe UI", 9);
            chkShowPassword.ForeColor = Color.FromArgb(80, 80, 80);
            chkShowPassword.Location = new Point(50, 335);
            chkShowPassword.AutoSize = true;
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '●';
            };

            // === Nút đăng nhập ===
            btnLogin = new Button();
            btnLogin.Text = "ĐĂNG NHẬP";
            btnLogin.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnLogin.BackColor = Color.FromArgb(220, 53, 69);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Location = new Point(50, 380);
            btnLogin.Size = new Size(300, 45);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(200, 43, 59);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(220, 53, 69);

            // === Thêm controls vào login box ===
            pnlLoginBox.Controls.Add(picLogo);
            pnlLoginBox.Controls.Add(lblWelcome);
            pnlLoginBox.Controls.Add(lblTitle);
            pnlLoginBox.Controls.Add(lblIdentifier);
            pnlLoginBox.Controls.Add(txtIdentifier);
            pnlLoginBox.Controls.Add(lblPassword);
            pnlLoginBox.Controls.Add(txtPassword);
            pnlLoginBox.Controls.Add(chkShowPassword);
            pnlLoginBox.Controls.Add(btnLogin);

            // === Thêm vào form ===
            pnlBackground.Controls.Add(pnlLoginBox);
            this.Controls.Add(pnlBackground);
            this.Load += (s, e) => CenterLoginBox();
            this.Resize += (s, e) => CenterLoginBox();
        }

        // Thêm phương thức này vào lớp frmLogin
        private void CenterLoginBox()
        {
            // Căn giữa pnlLoginBox so với pnlBackground (là cả Form)
            if (pnlBackground != null && pnlLoginBox != null)
            {
                int x = (pnlBackground.Width - pnlLoginBox.Width) / 2;
                int y = (pnlBackground.Height - pnlLoginBox.Height) / 2;

                // Đảm bảo không bị tràn màn hình (chẳng hạn nếu form quá nhỏ)
                if (x < 0) x = 0;
                if (y < 0) y = 0;

                pnlLoginBox.Location = new Point(x, y);
            }
        }

        // Vẽ nền với hiệu ứng phim
        private void PnlBackground_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Gradient nền tối
            using (LinearGradientBrush brush = new LinearGradientBrush(
                pnlBackground.ClientRectangle,
                Color.FromArgb(15, 15, 25),
                Color.FromArgb(30, 30, 45),
                45f))
            {
                g.FillRectangle(brush, pnlBackground.ClientRectangle);
            }

            // Vẽ các icon phim trang trí
            DrawMovieIcons(g);

            // Vẽ text trang trí
            using (Font font = new Font("Segoe UI", 120, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(15, 255, 255, 255)))
            {
                g.DrawString("CINEMA", font, brush, new PointF(-20, 400));
            }
        }

        // Vẽ các icon phim
        private void DrawMovieIcons(Graphics g)
        {
            // Vẽ film strips
            using (Pen pen = new Pen(Color.FromArgb(30, 255, 255, 255), 3))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
            {
                // Film strip trái
                Rectangle[] leftStrip = new Rectangle[]
                {
                    new Rectangle(50, 100, 30, 40),
                    new Rectangle(50, 160, 30, 40),
                    new Rectangle(50, 220, 30, 40),
                    new Rectangle(50, 280, 30, 40)
                };

                foreach (var rect in leftStrip)
                {
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(pen, rect);
                }

                // Film strip phải
                Rectangle[] rightStrip = new Rectangle[]
                {
                    new Rectangle(920, 150, 30, 40),
                    new Rectangle(920, 210, 30, 40),
                    new Rectangle(920, 270, 30, 40),
                    new Rectangle(920, 330, 30, 40)
                };

                foreach (var rect in rightStrip)
                {
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(pen, rect);
                }
            }

            // Vẽ ngôi sao
            DrawStar(g, 100, 50, 20, Color.FromArgb(50, 255, 215, 0));
            DrawStar(g, 870, 100, 25, Color.FromArgb(50, 255, 215, 0));
            DrawStar(g, 150, 450, 15, Color.FromArgb(40, 255, 215, 0));
            DrawStar(g, 900, 450, 18, Color.FromArgb(40, 255, 215, 0));
        }

        // Vẽ ngôi sao 5 cánh
        private void DrawStar(Graphics g, float centerX, float centerY, float size, Color color)
        {
            PointF[] points = new PointF[10];
            double angle = -Math.PI / 2;
            double angleStep = Math.PI / 5;

            for (int i = 0; i < 10; i++)
            {
                float radius = (i % 2 == 0) ? size : size / 2;
                points[i] = new PointF(
                    centerX + (float)(radius * Math.Cos(angle)),
                    centerY + (float)(radius * Math.Sin(angle))
                );
                angle += angleStep;
            }

            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, points);
            }
        }

        // Vẽ bo góc cho login box
        private void PnlLoginBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = pnlLoginBox.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            using (GraphicsPath path = GetRoundedRectangle(rect, 20))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(250, 255, 255, 255)))
            {
                g.FillPath(brush, path);
            }

            // Đổ bóng nhẹ
            using (GraphicsPath path = GetRoundedRectangle(rect, 20))
            using (Pen pen = new Pen(Color.FromArgb(30, 0, 0, 0), 1))
            {
                g.DrawPath(pen, path);
            }
        }

        // Vẽ icon logo phim
        private void PicLogo_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Vẽ biểu tượng vé phim
            int centerX = picLogo.Width / 2;
            int centerY = picLogo.Height / 2;

            // Hình chữ nhật vé
            Rectangle ticketRect = new Rectangle(15, 20, 50, 40);
            using (LinearGradientBrush brush = new LinearGradientBrush(
                ticketRect,
                Color.FromArgb(220, 53, 69),
                Color.FromArgb(253, 126, 20),
                LinearGradientMode.Horizontal))
            using (GraphicsPath path = GetRoundedRectangle(ticketRect, 8))
            {
                g.FillPath(brush, path);
            }

            // Vẽ các lỗ ở giữa vé
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, new Rectangle(25, 35, 8, 8));
                g.FillEllipse(brush, new Rectangle(47, 35, 8, 8));
            }
        }

        // Tạo hình chữ nhật bo góc
        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string identifier = txtIdentifier.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isLogin = _auth.Login(identifier, password);

            if (isLogin)
            {
                MessageBox.Show($"Đăng nhập thành công! Xin chào {SessionManager.CurrentUser.FullName}",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Hide();

                // Mở form chính
                Form main = new frmHomeMovie();
                main.Text = "Trang chính";
                main.Size = new Size(600, 400);
                main.StartPosition = FormStartPosition.CenterScreen;
                main.FormClosed += (s, ev) => this.Close();
                main.Show();
            }
            else
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}