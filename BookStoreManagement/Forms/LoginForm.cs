using BookStoreManagement.Models;
using BookStoreManagement.Services;

namespace BookStoreManagement.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;

        public LoginForm()
        {
            InitializeComponent();

            _authService = new AuthService();

            btnLogin.Click += BtnLogin_Click;
            btnExit.Click += BtnExit_Click;

            txtPassword.UseSystemPasswordChar = true;
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();

                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Vui lòng nhập tài khoản.");
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu.");
                    txtPassword.Focus();
                    return;
                }

                User? user = _authService.Login(username, password);

                if (user == null)
                {
                    MessageBox.Show("Sai tài khoản, mật khẩu hoặc tài khoản đã bị khóa.");
                    return;
                }

                MessageBox.Show("Đăng nhập thành công: " + user.FullName);

                DashboardForm dashboardForm = new DashboardForm();
                dashboardForm.Show();

                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message);
            }
        }

        private void BtnExit_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}