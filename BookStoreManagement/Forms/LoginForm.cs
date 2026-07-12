using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using BookStoreManagement.Services;
using BookStoreManagement.Models;

namespace BookStoreManagement.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;

        public LoginForm()
        {
            InitializeComponent();
            _authService = new AuthService();
            BookStoreManagement.Themes.ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ApplyTheme();
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = Themes.ThemeManager.Background;
            loginCard.FillColor = Themes.ThemeManager.CardBackground;
            lblBrand.ForeColor = Themes.ThemeManager.TextPrimary;
            lblUserLabel.ForeColor = Themes.ThemeManager.TextPrimary;
            lblPassLabel.ForeColor = Themes.ThemeManager.TextPrimary;
            lblSubtitle.ForeColor = Themes.ThemeManager.TextSecondary;
            lblFooter.ForeColor = Themes.ThemeManager.TextSecondary;

            txtUsername.FillColor = Themes.ThemeManager.TextBoxBackground;
            txtUsername.BorderColor = Themes.ThemeManager.TextBoxBorder;
            txtUsername.ForeColor = Themes.ThemeManager.TextPrimary;

            txtPassword.FillColor = Themes.ThemeManager.TextBoxBackground;
            txtPassword.BorderColor = Themes.ThemeManager.TextBoxBorder;
            txtPassword.ForeColor = Themes.ThemeManager.TextPrimary;

            chkRemember.UncheckedState.FillColor = Themes.ThemeManager.TextBoxBackground;
            chkRemember.UncheckedState.BorderColor = Themes.ThemeManager.TextBoxBorder;
            
            btnLogin.FillColor = Themes.ThemeManager.ButtonFill;
            btnLogin.ForeColor = Themes.ThemeManager.ButtonText;
            lblForgot.ForeColor = Themes.ThemeManager.ButtonFill;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Please enter username and password.";
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
                lblError.Text = "Invalid username or password.";
                lblError.Visible = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
