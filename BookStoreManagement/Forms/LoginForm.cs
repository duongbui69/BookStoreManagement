using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using BookStoreManagement.Services;
using BookStoreManagement.Models;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;
        private bool isPasswordVisible = false;

        public LoginForm()
        {
            InitializeComponent();
            _authService = new AuthService();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            
            // Set initial state for icons
            txtUsername.IconLeft = CreateEmojiImage("👤", 20);
            txtPassword.IconLeft = CreateEmojiImage("🔒", 20);
            txtPassword.IconRight = CreateEmojiImage("👁", 18);
            
            ApplyTheme();
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            // Main Background
            this.BackColor = ThemeManager.Background;
            
            // Title Bar
            pnlTitleBar.BackColor = ThemeManager.CardBackground;
            lblTitleText.ForeColor = ThemeManager.TextPrimary;
            lblTitleIcon.ForeColor = ThemeManager.ButtonFill;
            pnlTitleBar.CustomBorderColor = ThemeManager.TextBoxBorder;

            btnMinimize.IconColor = ThemeManager.TextSecondary;
            btnMaximize.IconColor = ThemeManager.TextSecondary;
            btnClose.IconColor = ThemeManager.TextSecondary;
            btnClose.HoverState.FillColor = Color.FromArgb(200, 50, 50);
            btnClose.HoverState.IconColor = Color.White;

            // Login Card
            loginCard.FillColor = ThemeManager.CardBackground;
            
            // Header Section
            pnlHeaderIcon.FillColor = ThemeManager.ButtonFill;
            lblHeaderIcon.BackColor = Color.Transparent;
            lblHeaderIcon.ForeColor = ThemeManager.ButtonText;

            lblBrand.ForeColor = ThemeManager.ButtonFill;
            lblSubtitle.ForeColor = ThemeManager.TextSecondary;

            // Form Inputs
            lblUserLabel.ForeColor = ThemeManager.TextPrimary;
            lblPassLabel.ForeColor = ThemeManager.TextPrimary;

            txtUsername.FillColor = ThemeManager.TextBoxBackground;
            txtUsername.BorderColor = ThemeManager.TextBoxBorder;
            txtUsername.ForeColor = ThemeManager.TextPrimary;
            txtUsername.PlaceholderForeColor = ThemeManager.TextSecondary;
            txtUsername.FocusedState.BorderColor = ThemeManager.ButtonFill;
            txtUsername.HoverState.BorderColor = ThemeManager.ButtonFill;

            txtPassword.FillColor = ThemeManager.TextBoxBackground;
            txtPassword.BorderColor = ThemeManager.TextBoxBorder;
            txtPassword.ForeColor = ThemeManager.TextPrimary;
            txtPassword.PlaceholderForeColor = ThemeManager.TextSecondary;
            txtPassword.FocusedState.BorderColor = ThemeManager.ButtonFill;
            txtPassword.HoverState.BorderColor = ThemeManager.ButtonFill;

            // Options Row
            chkRemember.ForeColor = ThemeManager.TextSecondary;
            chkRemember.UncheckedState.FillColor = ThemeManager.TextBoxBackground;
            chkRemember.UncheckedState.BorderColor = ThemeManager.TextBoxBorder;
            chkRemember.CheckedState.FillColor = ThemeManager.ButtonFill;
            chkRemember.CheckedState.BorderColor = ThemeManager.ButtonFill;
            
            lblForgot.ForeColor = ThemeManager.ButtonFill;

            // Action Button
            btnLogin.FillColor = ThemeManager.ButtonFill;
            btnLogin.ForeColor = ThemeManager.ButtonText;
            
            // Footer
            lblVersion.ForeColor = ThemeManager.TextSecondary;
            lblCopyright.ForeColor = ThemeManager.TextSecondary;
        }

        private void txtPassword_IconRightClick(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            if (isPasswordVisible)
            {
                txtPassword.UseSystemPasswordChar = false;
                txtPassword.PasswordChar = '\0';
                txtPassword.IconRight = CreateEmojiImage("🙈", 18); // hidden icon
            }
            else
            {
                txtPassword.UseSystemPasswordChar = true;
                txtPassword.PasswordChar = '●';
                txtPassword.IconRight = CreateEmojiImage("👁", 18); // visible icon
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Vui lòng nhập tài khoản và mật khẩu.";
                lblError.Visible = true;
                return;
            }

            var user = _authService.Login(username, password);
            if (user != null)
            {
                // Login successful
                this.Hide();
                var mainForm = new MainForm(user);
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
            }
            else
            {
                lblError.Text = "Tên đăng nhập hoặc mật khẩu không đúng.";
                lblError.Visible = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private Image CreateEmojiImage(string emoji, int size)
        {
            Bitmap bmp = new Bitmap(size + 10, size + 10);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                using (Font font = new Font("Segoe UI Emoji", size - 4, FontStyle.Regular))
                using (Brush brush = new SolidBrush(Color.Gray))
                {
                    SizeF textSize = g.MeasureString(emoji, font);
                    g.DrawString(emoji, font, brush, (bmp.Width - textSize.Width) / 2, (bmp.Height - textSize.Height) / 2);
                }
            }
            return bmp;
        }
    }
}
