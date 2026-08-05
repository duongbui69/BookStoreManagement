using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class AccountForm : Form
    {
        private UserService _userService;
        private RoleService _roleService;
        private StoreService _storeService;

        private Guna2TextBox txtUsername, txtPassword, txtFullName, txtPhone, txtEmail, txtAddress;
        private Guna2ComboBox cbRole;
        private Guna2CheckBox chkIsActive;
        private Guna2Button btnSave, btnCancel;
        private ErrorProvider _errorProvider;

        public User? AccountModel { get; private set; }

        public AccountForm(User? accountToEdit = null)
        {
            _userService = new UserService();
            _roleService = new RoleService();
            _storeService = new StoreService();
            
            AccountModel = accountToEdit;
            _errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            InitializeComponent();
            _errorProvider.ContainerControl = this;
            
            this.Load += AccountForm_Load;
            
            ApplyTheme();
            
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = AccountModel == null ? "Thêm mới Tài khoản" : "Chỉnh sửa Tài khoản";
            this.Size = new Size(800, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ControlBox = false;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 18F, FontStyle.Bold), Location = new Point(24, 20), AutoSize = true };
            this.Controls.Add(lblTitle);

            // Left Col (X = 24)
            int yLeft = 70;
            txtUsername = CreateInput("Tên đăng nhập (*)", 24, ref yLeft);
            txtUsername.MaxLength = 50;
            
            Label lblPass = new Label { Text = "Mật khẩu (Để trống nếu không đổi)", Location = new Point(24, yLeft), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            this.Controls.Add(lblPass);
            txtPassword = new Guna2TextBox { Location = new Point(24, yLeft + 25), Width = 340, Height = 40, Font = new Font("Segoe UI", 10F), PasswordChar = '*', BorderRadius = 4, MaxLength = 50 };
            this.Controls.Add(txtPassword);
            yLeft += 75;

            txtFullName = CreateInput("Họ và Tên", 24, ref yLeft);
            txtFullName.MaxLength = 100;
            ValidationHelper.WireTextOnly(txtFullName, _errorProvider);

            txtPhone = CreateInput("Số điện thoại", 24, ref yLeft);
            txtPhone.MaxLength = 20;
            ValidationHelper.WireDigitsOnly(txtPhone, _errorProvider);

            // Right Col (X = 400)
            int yRight = 70;
            txtEmail = CreateInput("Email", 400, ref yRight);
            txtEmail.MaxLength = 100;
            ValidationHelper.WireEmailValidation(txtEmail, _errorProvider);

            txtAddress = CreateInput("Địa chỉ", 400, ref yRight);
            txtAddress.MaxLength = 255;

            Label lblRole = new Label { Text = "Vai trò", Location = new Point(400, yRight), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbRole = new Guna2ComboBox { Location = new Point(400, yRight + 25), Width = 340, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lblRole, cbRole });
            yRight += 75;

            chkIsActive = new Guna2CheckBox { Text = "Đang hoạt động", Location = new Point(400, yRight + 10), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(chkIsActive);
            
            // Buttons at the bottom
            btnSave = new Guna2Button { Text = "Lưu thông tin", Location = new Point(280, 380), Width = 130, Height = 45, BorderRadius = 8, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.Click += BtnSave_Click;
            btnCancel = new Guna2Button { Text = "Hủy bỏ", Location = new Point(430, 380), Width = 110, Height = 45, BorderRadius = 8, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand, FillColor = Color.Transparent, BorderThickness = 1 };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private Guna2TextBox CreateInput(string label, int x, ref int y)
        {
            Label lbl = new Label { Text = label, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            Guna2TextBox txt = new Guna2TextBox { Location = new Point(x, y + 25), Width = 340, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lbl, txt });
            y += 75;
            return txt;
        }

        private async void AccountForm_Load(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try {
                var roles = await _roleService.GetAllAsync();
                cbRole.DataSource = roles;
                cbRole.DisplayMember = "RoleName";
                cbRole.ValueMember = "Id";
            } catch {}

            if (AccountModel != null) {
                BindData();
            }
        }

        private void BindData()
        {
            if (AccountModel == null) return;
            txtUsername.Text = AccountModel.Username;
            txtFullName.Text = AccountModel.FullName;
            txtPhone.Text = AccountModel.Phone;
            txtEmail.Text = AccountModel.Email;
            txtAddress.Text = AccountModel.Address;
            chkIsActive.Checked = AccountModel.IsActive;
            cbRole.SelectedValue = AccountModel.RoleId;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // --- Validation ---
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (!ValidationHelper.IsValidUsername(txtUsername.Text))
            {
                MessageBox.Show("Tên đăng nhập phải từ 4-50 ký tự, chỉ gồm chữ cái, chữ số, dấu gạch dưới và dấu chấm (không có khoảng trắng).", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (!ValidationHelper.IsValidPhone(txtPhone.Text))
            {
                MessageBox.Show("Số điện thoại không hợp lệ. Vui lòng nhập 10-11 chữ số.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }

            if (!ValidationHelper.IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Email không đúng định dạng. Vui lòng kiểm tra lại.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }
            // --- End Validation ---

            if (AccountModel == null) AccountModel = new User();
            
            AccountModel.Username = txtUsername.Text.Trim();
            AccountModel.FullName = txtFullName.Text.Trim();
            AccountModel.Phone = txtPhone.Text.Trim();
            AccountModel.Email = txtEmail.Text.Trim();
            AccountModel.Address = txtAddress.Text.Trim();
            AccountModel.IsActive = chkIsActive.Checked;
            
            if (cbRole.SelectedValue != null) AccountModel.RoleId = (int)cbRole.SelectedValue;

            try {
                if (AccountModel.Id == 0) {
                    if (string.IsNullOrEmpty(txtPassword.Text)) {
                        MessageBox.Show("Vui lòng nhập mật khẩu cho tài khoản mới.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    _userService.CreateUser(AccountModel, txtPassword.Text);
                    MessageBox.Show("Tạo tài khoản thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else {
                    _userService.Update(AccountModel);
                    if (!string.IsNullOrEmpty(txtPassword.Text)) {
                        _userService.ResetPassword(AccountModel.Id, txtPassword.Text);
                    }
                    MessageBox.Show("Cập nhật tài khoản thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            } catch (Exception ex) {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;

            foreach (Control control in this.Controls)
            {
                if (control is Label lbl)
                {
                    lbl.ForeColor = ThemeManager.TextPrimary;
                }
                else if (control is Guna2TextBox txt)
                {
                    txt.FillColor = ThemeManager.TextBoxBackground;
                    txt.ForeColor = ThemeManager.TextPrimary;
                    txt.BorderColor = ThemeManager.TextBoxBorder;
                    txt.FocusedState.BorderColor = ThemeManager.ButtonFill;
                }
                else if (control is Guna2ComboBox cb)
                {
                    cb.FillColor = ThemeManager.TextBoxBackground;
                    cb.ForeColor = ThemeManager.TextPrimary;
                    cb.BorderColor = ThemeManager.TextBoxBorder;
                }
                else if (control is Guna2CheckBox chk)
                {
                    chk.ForeColor = ThemeManager.TextPrimary;
                }
            }

            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.BorderColor = ThemeManager.TextBoxBorder;

            btnSave.FillColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;
        }
    }
}
