using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public class EmployeeForm : Form
    {
        private HRService _hrService;
        private RoleService _roleService;
        private StoreService _storeService;

        private TextBox txtUserCode, txtUsername, txtPassword, txtFullName, txtPhone, txtEmail, txtAddress, txtIdentity;
        private ComboBox cbRole, cbStore;
        private CheckBox chkIsActive;
        private Button btnSave, btnCancel;

        public User? EmployeeModel { get; private set; }

        public EmployeeForm(User? userToEdit = null)
        {
            _hrService = new HRService();
            _roleService = new RoleService();
            _storeService = new StoreService();
            
            EmployeeModel = userToEdit;
            InitializeComponent();
            
            this.Load += EmployeeForm_Load;
            
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = EmployeeModel == null ? "Add New Employee" : "Edit Employee";
            this.Size = new Size(500, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitle);

            int y = 70;
            txtUserCode = CreateInput("User Code", ref y);
            txtUsername = CreateInput("Tên đăng nhập", ref y);
            
            Label lblPass = new Label { Text = "Mật khẩu (Để trống nếu không đổi)", Location = new Point(20, y), AutoSize = true };
            this.Controls.Add(lblPass);
            txtPassword = new TextBox { Location = new Point(20, y + 20), Width = 440, Font = new Font("Segoe UI", 10F), PasswordChar = '*' };
            this.Controls.Add(txtPassword);
            y += 60;

            txtFullName = CreateInput("Họ và Tên", ref y);
            txtIdentity = CreateInput("Identity Number", ref y);
            txtPhone = CreateInput("SĐT", ref y);
            txtEmail = CreateInput("Email", ref y);
            txtAddress = CreateInput("Địa chỉ", ref y);

            Label lblRole = new Label { Text = "Vai trò", Location = new Point(20, y), AutoSize = true };
            cbRole = new ComboBox { Location = new Point(20, y + 20), Width = 210, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.AddRange(new Control[] { lblRole, cbRole });

            Label lblStore = new Label { Text = "Chi nhánh", Location = new Point(250, y), AutoSize = true };
            cbStore = new ComboBox { Location = new Point(250, y + 20), Width = 210, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.AddRange(new Control[] { lblStore, cbStore });
            y += 60;

            chkIsActive = new CheckBox { Text = "Đang hoạt động", Location = new Point(20, y), AutoSize = true, Checked = true };
            this.Controls.Add(chkIsActive);
            y += 40;

            btnSave = new Button { Text = "Lưu (Save)", Location = new Point(130, y), Width = 100, Height = 40, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            btnCancel = new Button { Text = "Hủy bỏ", Location = new Point(250, y), Width = 100, Height = 40, FlatStyle = FlatStyle.Flat };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private TextBox CreateInput(string label, ref int y)
        {
            Label lbl = new Label { Text = label, Location = new Point(20, y), AutoSize = true };
            TextBox txt = new TextBox { Location = new Point(20, y + 20), Width = 440, Font = new Font("Segoe UI", 10F) };
            this.Controls.AddRange(new Control[] { lbl, txt });
            y += 60;
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
                stores.Insert(0, new Store { Id = 0, StoreName = "--- All Stores (Admin) ---" });
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
            txtPhone.Text = EmployeeModel.Phone;
            txtEmail.Text = EmployeeModel.Email;
            txtAddress.Text = EmployeeModel.Address;
            chkIsActive.Checked = EmployeeModel.IsActive;
            cbRole.SelectedValue = EmployeeModel.RoleId;
            cbStore.SelectedValue = EmployeeModel.StoreId ?? 0;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (EmployeeModel == null) EmployeeModel = new User();
            
            EmployeeModel.UserCode = txtUserCode.Text.Trim();
            EmployeeModel.Username = txtUsername.Text.Trim();
            EmployeeModel.FullName = txtFullName.Text.Trim();
            EmployeeModel.IdentityNumber = txtIdentity.Text.Trim();
            EmployeeModel.Phone = txtPhone.Text.Trim();
            EmployeeModel.Email = txtEmail.Text.Trim();
            EmployeeModel.Address = txtAddress.Text.Trim();
            EmployeeModel.IsActive = chkIsActive.Checked;
            
            if (cbRole.SelectedValue != null) EmployeeModel.RoleId = (int)cbRole.SelectedValue;
            if (cbStore.SelectedValue != null) {
                int storeId = (int)cbStore.SelectedValue;
                EmployeeModel.StoreId = storeId > 0 ? storeId : (int?)null;
            }

            try {
                if (EmployeeModel.Id == 0) {
                    if (string.IsNullOrEmpty(txtPassword.Text)) {
                        MessageBox.Show("Vui lòng nhập mật khẩu cho nhân viên mới.");
                        return;
                    }
                    _hrService.CreateUser(EmployeeModel, txtPassword.Text);
                    MessageBox.Show("Đã tạo nhân viên.");
                } else {
                    _hrService.UpdateUser(EmployeeModel);
                    if (!string.IsNullOrEmpty(txtPassword.Text)) {
                        _hrService.ResetPassword(EmployeeModel.Id, txtPassword.Text);
                    }
                    MessageBox.Show("Đã cập nhật nhân viên.");
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            } catch (Exception ex) {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            foreach (Control c in this.Controls) {
                if (c is Label l) l.ForeColor = ThemeManager.TextPrimary;
                if (c is TextBox t) { t.BackColor = ThemeManager.TextBoxBackground; t.ForeColor = ThemeManager.TextPrimary; }
                if (c is CheckBox cb) cb.ForeColor = ThemeManager.TextPrimary;
            }
            btnSave.BackColor = ThemeManager.ButtonFill; btnSave.ForeColor = ThemeManager.ButtonText; btnSave.FlatAppearance.BorderSize = 0;
            btnCancel.BackColor = ThemeManager.HoverColor; btnCancel.ForeColor = ThemeManager.TextPrimary; btnCancel.FlatAppearance.BorderSize = 0;
        }
    }
}

