using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public class AccountForm : Form
    {
        private UserService _userService;
        private RoleService _roleService;
        private StoreService _storeService;

        private TextBox txtUsername, txtPassword, txtFullName, txtPhone, txtEmail, txtAddress;
        private ComboBox cbRole;
        private CheckBox chkIsActive;
        private Button btnSave, btnCancel;

        public User? AccountModel { get; private set; }

        public AccountForm(User? accountToEdit = null)
        {
            _userService = new UserService();
            _roleService = new RoleService();
            _storeService = new StoreService();
            
            AccountModel = accountToEdit;
            InitializeComponent();
            
            this.Load += AccountForm_Load;
            
            ApplyTheme();
            
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = AccountModel == null ? "Add New Account" : "Edit Account";
            this.Size = new Size(500, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitle);

            int y = 70;
            txtUsername = CreateInput("Username", ref y);
            
            Label lblPass = new Label { Text = "Password (Leave blank if unchanged)", Location = new Point(20, y), AutoSize = true };
            this.Controls.Add(lblPass);
            txtPassword = new TextBox { Location = new Point(20, y + 20), Width = 440, Font = new Font("Segoe UI", 10F), PasswordChar = '*' };
            this.Controls.Add(txtPassword);
            y += 60;

            txtFullName = CreateInput("Full name", ref y);
            txtPhone = CreateInput("Phone number", ref y);
            txtEmail = CreateInput("Email", ref y);
            txtAddress = CreateInput("Address", ref y);

            Label lblRole = new Label { Text = "Role", Location = new Point(20, y), AutoSize = true };
            cbRole = new ComboBox { Location = new Point(20, y + 20), Width = 440, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.AddRange(new Control[] { lblRole, cbRole });
            y += 60;

            chkIsActive = new CheckBox { Text = "Active", Location = new Point(20, y), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(chkIsActive);
            y += 40;

            btnSave = new Button { Text = "Save", Location = new Point(130, y), Width = 100, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnSave.Click += BtnSave_Click;
            btnCancel = new Button { Text = "Cancel", Location = new Point(250, y), Width = 100, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F) };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private TextBox CreateInput(string label, ref int y)
        {
            Label lbl = new Label { Text = label, Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
            TextBox txt = new TextBox { Location = new Point(20, y + 20), Width = 440, Font = new Font("Segoe UI", 10F) };
            this.Controls.AddRange(new Control[] { lbl, txt });
            y += 60;
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

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
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
                        MessageBox.Show("Please enter password for new account.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    // Currently no async version of CreateUser in UserService
                    _userService.CreateUser(AccountModel, txtPassword.Text);
                    MessageBox.Show("Account created successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else {
                    _userService.Update(AccountModel);
                    if (!string.IsNullOrEmpty(txtPassword.Text)) {
                        _userService.ResetPassword(AccountModel.Id, txtPassword.Text);
                    }
                    MessageBox.Show("Account updated successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            foreach (Control c in this.Controls) {
                if (c is Label l) l.ForeColor = ThemeManager.TextPrimary;
                if (c is TextBox t) { 
                    t.BackColor = ThemeManager.TextBoxBackground; 
                    t.ForeColor = ThemeManager.TextPrimary; 
                }
                if (c is CheckBox cb) cb.ForeColor = ThemeManager.TextPrimary;
            }
            btnSave.BackColor = ThemeManager.ButtonFill; 
            btnSave.ForeColor = Color.White; 
            btnSave.FlatAppearance.BorderSize = 0;
            
            btnCancel.BackColor = ThemeManager.HoverColor; 
            btnCancel.ForeColor = ThemeManager.TextPrimary; 
            btnCancel.FlatAppearance.BorderSize = 0;
        }
    }
}

