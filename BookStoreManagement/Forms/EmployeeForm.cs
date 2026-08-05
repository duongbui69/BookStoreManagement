using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.Models;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public partial class EmployeeForm : Form
    {
        private readonly HRService _hrService;
        private readonly RoleService _roleService;
        private readonly StoreService _storeService;

        private Guna2TextBox txtUserCode, txtUsername, txtPassword, txtFullName, txtIdentity, txtPhone, txtEmail, txtAddress, txtHourlyRate;
        private Guna2ComboBox cbRole, cbStore;
        private Guna2CheckBox chkIsActive;
        private Guna2Button btnSave, btnCancel;
        private ErrorProvider _errorProvider;

        public User? EmployeeModel { get; private set; }

        public EmployeeForm(User? userToEdit = null)
        {
            _hrService = new HRService();
            _roleService = new RoleService();
            _storeService = new StoreService();
            
            EmployeeModel = userToEdit;
            _errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            InitializeComponent();
            _errorProvider.ContainerControl = this;
            
            this.Load += EmployeeForm_Load;
            
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = EmployeeModel == null ? "Thêm nhân viên mới" : "Chỉnh sửa nhân viên";
            this.Size = new Size(800, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ControlBox = false;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 18F, FontStyle.Bold), Location = new Point(24, 20), AutoSize = true };
            this.Controls.Add(lblTitle);

            // Left Col (X = 24)
            int yLeft = 70;
            txtUserCode = CreateInput("Mã nhân viên", 24, ref yLeft);
            txtUserCode.MaxLength = 50;

            txtUsername = CreateInput("Tên đăng nhập", 24, ref yLeft);
            txtUsername.MaxLength = 50;
            
            Label lblPass = new Label { Text = "Mật khẩu (Để trống nếu không đổi)", Location = new Point(24, yLeft), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            this.Controls.Add(lblPass);
            txtPassword = new Guna2TextBox { Location = new Point(24, yLeft + 25), Width = 340, Height = 40, Font = new Font("Segoe UI", 10F), PasswordChar = '*', BorderRadius = 4, MaxLength = 50 };
            this.Controls.Add(txtPassword);
            yLeft += 75;

            txtFullName = CreateInput("Họ và Tên", 24, ref yLeft);
            txtFullName.MaxLength = 100;
            ValidationHelper.WireTextOnly(txtFullName, _errorProvider);

            txtIdentity = CreateInput("CCCD / CMND", 24, ref yLeft);
            txtIdentity.MaxLength = 20;
            ValidationHelper.WireDigitsOnly(txtIdentity, _errorProvider);

            txtHourlyRate = CreateInput("Lương theo giờ (VNĐ)", 24, ref yLeft);
            txtHourlyRate.MaxLength = 20;
            ValidationHelper.WireDecimalOnly(txtHourlyRate, _errorProvider);

            // Right Col (X = 400)
            int yRight = 70;
            txtPhone = CreateInput("Số điện thoại", 400, ref yRight);
            txtPhone.MaxLength = 20;
            ValidationHelper.WireDigitsOnly(txtPhone, _errorProvider);

            txtEmail = CreateInput("Email", 400, ref yRight);
            txtEmail.MaxLength = 100;
            ValidationHelper.WireEmailValidation(txtEmail, _errorProvider);

            txtAddress = CreateInput("Địa chỉ", 400, ref yRight);
            txtAddress.MaxLength = 255;

            Label lblRole = new Label { Text = "Vai trò", Location = new Point(400, yRight), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbRole = new Guna2ComboBox { Location = new Point(400, yRight + 25), Width = 340, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lblRole, cbRole });
            yRight += 75;

            Label lblStore = new Label { Text = "Chi nhánh", Location = new Point(400, yRight), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbStore = new Guna2ComboBox { Location = new Point(400, yRight + 25), Width = 340, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lblStore, cbStore });
            yRight += 75;

            chkIsActive = new Guna2CheckBox { Text = "Đang hoạt động", Location = new Point(24, 520), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(chkIsActive);

            btnSave = new Guna2Button { Text = "Lưu (Save)", Location = new Point(240, 560), Width = 140, Height = 45, BorderRadius = 8, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.Click += BtnSave_Click;
            btnCancel = new Guna2Button { Text = "Hủy bỏ", Location = new Point(420, 560), Width = 140, Height = 45, BorderRadius = 8, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand, FillColor = Color.Transparent, BorderThickness = 1 };
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

        private async void EmployeeForm_Load(object? sender, EventArgs e)
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

                var stores = _storeService.GetAll();
                stores.Insert(0, new Store { Id = 0, StoreName = "--- Tất cả chi nhánh (Admin) ---" });
                cbStore.DataSource = stores;
                cbStore.DisplayMember = "StoreName";
                cbStore.ValueMember = "Id";
            } catch {}

            if (EmployeeModel != null) {
                BindData();
            }
        }

        private void BindData()
        {
            if (EmployeeModel == null) return;
            txtUserCode.Text = EmployeeModel.UserCode;
            txtUsername.Text = EmployeeModel.Username;
            txtFullName.Text = EmployeeModel.FullName;
            txtIdentity.Text = EmployeeModel.IdentityNumber;
            txtHourlyRate.Text = EmployeeModel.HourlyRate.ToString();
            txtPhone.Text = EmployeeModel.Phone;
            txtEmail.Text = EmployeeModel.Email;
            txtAddress.Text = EmployeeModel.Address;
            chkIsActive.Checked = EmployeeModel.IsActive;
            cbRole.SelectedValue = EmployeeModel.RoleId;
            cbStore.SelectedValue = EmployeeModel.StoreId ?? 0;
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
                MessageBox.Show("Tên đăng nhập phải từ 4-50 ký tự, chỉ gồm chữ cái, chữ số, dấu gạch dưới và dấu chấm.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (!ValidationHelper.IsValidIdentity(txtIdentity.Text))
            {
                MessageBox.Show("CMND/CCCD không hợp lệ. Phải là 9 hoặc 12 chữ số.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdentity.Focus();
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

            if (EmployeeModel == null) EmployeeModel = new User();
            
            EmployeeModel.UserCode = txtUserCode.Text.Trim();
            EmployeeModel.Username = txtUsername.Text.Trim();
            EmployeeModel.FullName = txtFullName.Text.Trim();
            EmployeeModel.IdentityNumber = txtIdentity.Text.Trim();
            EmployeeModel.Phone = txtPhone.Text.Trim();
            EmployeeModel.Email = txtEmail.Text.Trim();
            EmployeeModel.Address = txtAddress.Text.Trim();
            if (decimal.TryParse(txtHourlyRate.Text.Trim(), out decimal hr)) {
                EmployeeModel.HourlyRate = hr;
            } else {
                EmployeeModel.HourlyRate = 0;
            }
            EmployeeModel.IsActive = chkIsActive.Checked;
            
            if (cbRole.SelectedValue != null) EmployeeModel.RoleId = (int)cbRole.SelectedValue;
            if (cbStore.SelectedValue != null) {
                int storeId = (int)cbStore.SelectedValue;
                EmployeeModel.StoreId = storeId > 0 ? storeId : (int?)null;
            }

            try {
                if (EmployeeModel.Id == 0) {
                    if (string.IsNullOrEmpty(txtPassword.Text)) {
                        MessageBox.Show("Vui lòng nhập mật khẩu cho nhân viên mới.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    _hrService.CreateUser(EmployeeModel, txtPassword.Text);
                    MessageBox.Show("Đã tạo nhân viên thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else {
                    _hrService.UpdateUser(EmployeeModel);
                    if (!string.IsNullOrEmpty(txtPassword.Text)) {
                        _hrService.ResetPassword(EmployeeModel.Id, txtPassword.Text);
                    }
                    MessageBox.Show("Đã cập nhật nhân viên thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            foreach (Control c in this.Controls) {
                if (c is Label l) l.ForeColor = ThemeManager.TextPrimary;
                if (c is Guna2TextBox t) { 
                    t.FillColor = ThemeManager.TextBoxBackground; 
                    t.ForeColor = ThemeManager.TextPrimary; 
                    t.BorderColor = ThemeManager.TextBoxBorder; 
                    t.FocusedState.BorderColor = ThemeManager.ButtonFill;
                }
                if (c is Guna2ComboBox cb) {
                    cb.FillColor = ThemeManager.TextBoxBackground; 
                    cb.ForeColor = ThemeManager.TextPrimary; 
                    cb.BorderColor = ThemeManager.TextBoxBorder;
                }
                if (c is Guna2CheckBox chk) chk.ForeColor = ThemeManager.TextPrimary;
            }
            btnSave.FillColor = ThemeManager.ButtonFill; 
            btnSave.ForeColor = ThemeManager.ButtonText; 
            
            btnCancel.BorderColor = ThemeManager.TextBoxBorder; 
            btnCancel.ForeColor = ThemeManager.TextPrimary; 
        }
    }
}
