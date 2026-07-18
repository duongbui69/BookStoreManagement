using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public class CustomerForm : Form
    {
        private CustomerService _service;
        
        private TextBox txtCustomerCode, txtFullName, txtIdentityNumber, txtPhone, txtEmail, txtAddress;
        private NumericUpDown numPoints;
        private CheckBox chkIsActive;
        private Button btnSave, btnCancel;

        public Customer? CustomerModel { get; private set; }

        public CustomerForm(Customer? customerToEdit = null)
        {
            _service = new CustomerService();
            CustomerModel = customerToEdit;
            
            InitializeComponent();
            ApplyTheme();
            
            this.Load += CustomerForm_Load;
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = CustomerModel == null ? "Thêm Mới Khách Hàng" : "Chỉnh Sửa Khách Hàng";
            this.Size = new Size(500, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitle);

            int y = 70;
            txtCustomerCode = CreateInput("Mã khách hàng (Bỏ trống sẽ tự tạo)", ref y);
            if (CustomerModel != null) txtCustomerCode.ReadOnly = true;
            
            txtFullName = CreateInput("Họ và tên *", ref y);
            txtIdentityNumber = CreateInput("CMND/CCCD", ref y);
            txtPhone = CreateInput("Số điện thoại", ref y);
            txtEmail = CreateInput("Email", ref y);
            txtAddress = CreateInput("Địa chỉ", ref y);

            Label lblPoints = new Label { Text = "Điểm tích lũy", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
            numPoints = new NumericUpDown { Location = new Point(20, y + 20), Width = 440, Font = new Font("Segoe UI", 10F), Maximum = 99999999, Minimum = 0 };
            this.Controls.AddRange(new Control[] { lblPoints, numPoints });
            y += 60;

            chkIsActive = new CheckBox { Text = "Hoạt động", Location = new Point(20, y), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(chkIsActive);
            y += 40;

            btnSave = new Button { Text = "Lưu", Location = new Point(130, y), Width = 100, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnSave.Click += BtnSave_Click;
            btnCancel = new Button { Text = "Hủy", Location = new Point(250, y), Width = 100, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F) };
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

        private void CustomerForm_Load(object? sender, EventArgs e)
        {
            if (CustomerModel != null) {
                BindData();
            }
        }

        private void BindData()
        {
            if (CustomerModel == null) return;
            txtCustomerCode.Text = CustomerModel.CustomerCode;
            txtFullName.Text = CustomerModel.FullName;
            txtIdentityNumber.Text = CustomerModel.IdentityNumber;
            txtPhone.Text = CustomerModel.Phone;
            txtEmail.Text = CustomerModel.Email;
            txtAddress.Text = CustomerModel.Address;
            numPoints.Value = CustomerModel.Points;
            chkIsActive.Checked = CustomerModel.IsActive;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ và tên khách hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (CustomerModel == null) CustomerModel = new Customer();
            
            CustomerModel.CustomerCode = txtCustomerCode.Text.Trim();
            CustomerModel.FullName = txtFullName.Text.Trim();
            CustomerModel.IdentityNumber = txtIdentityNumber.Text.Trim();
            CustomerModel.Phone = txtPhone.Text.Trim();
            CustomerModel.Email = txtEmail.Text.Trim();
            CustomerModel.Address = txtAddress.Text.Trim();
            CustomerModel.Points = (int)numPoints.Value;
            CustomerModel.IsActive = chkIsActive.Checked;

            try {
                if (CustomerModel.Id == 0) {
                    await _service.AddAsync(CustomerModel);
                    MessageBox.Show("Thêm khách hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else {
                    await _service.UpdateAsync(CustomerModel);
                    MessageBox.Show("Cập nhật khách hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            this.ForeColor = ThemeManager.TextPrimary;

            foreach (Control control in this.Controls)
            {
                if (control is Label lbl)
                {
                    lbl.ForeColor = ThemeManager.TextPrimary;
                }
                else if (control is TextBox txt)
                {
                    txt.BackColor = ThemeManager.CardBackground;
                    txt.ForeColor = ThemeManager.TextPrimary;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (control is NumericUpDown num)
                {
                    num.BackColor = ThemeManager.CardBackground;
                    num.ForeColor = ThemeManager.TextPrimary;
                    num.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (control is CheckBox chk)
                {
                    chk.ForeColor = ThemeManager.TextPrimary;
                }
            }

            btnSave.BackColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = Color.White;
            btnSave.FlatAppearance.BorderColor = ThemeManager.ButtonFill;

            btnCancel.BackColor = ThemeManager.CardBackground;
            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;
        }
    }
}

