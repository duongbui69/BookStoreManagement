using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class CustomerForm : Form
    {
        private CustomerService _service;
        
        private Guna2TextBox txtCustomerCode, txtFullName, txtIdentityNumber, txtPhone, txtEmail, txtAddress;
        private Guna2NumericUpDown numPoints;
        private Guna2CheckBox chkIsActive;
        private Guna2Button btnSave, btnCancel;

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
            this.Text = CustomerModel == null ? "Thêm mới Khách hàng" : "Chỉnh sửa Khách hàng";
            this.Size = new Size(800, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 18F, FontStyle.Bold), Location = new Point(24, 20), AutoSize = true };
            this.Controls.Add(lblTitle);

            // Left Col (X = 24)
            int yLeft = 70;
            txtCustomerCode = CreateInput("Mã khách hàng (Để trống sẽ tự động tạo)", 24, ref yLeft);
            if (CustomerModel != null) txtCustomerCode.ReadOnly = true;
            
            txtFullName = CreateInput("Họ và Tên (*)", 24, ref yLeft);
            txtIdentityNumber = CreateInput("CMND / CCCD", 24, ref yLeft);
            txtPhone = CreateInput("Số điện thoại", 24, ref yLeft);

            // Right Col (X = 400)
            int yRight = 70;
            txtEmail = CreateInput("Email", 400, ref yRight);
            txtAddress = CreateInput("Địa chỉ", 400, ref yRight);

            Label lblPoints = new Label { Text = "Điểm thưởng", Location = new Point(400, yRight), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numPoints = new Guna2NumericUpDown { Location = new Point(400, yRight + 25), Width = 340, Height = 40, Font = new Font("Segoe UI", 10F), Maximum = 99999999, Minimum = 0, BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lblPoints, numPoints });
            yRight += 75;

            chkIsActive = new Guna2CheckBox { Text = "Đang hoạt động", Location = new Point(400, yRight + 10), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(chkIsActive);
            
            // Buttons at the bottom center
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
                MessageBox.Show("Vui lòng nhập họ tên khách hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("Đã thêm khách hàng thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else {
                    await _service.UpdateAsync(CustomerModel);
                    MessageBox.Show("Đã cập nhật khách hàng thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                else if (control is Guna2NumericUpDown num)
                {
                    num.FillColor = ThemeManager.TextBoxBackground;
                    num.ForeColor = ThemeManager.TextPrimary;
                    num.BorderColor = ThemeManager.TextBoxBorder;
                    num.UpDownButtonFillColor = ThemeManager.ButtonFill;
                    num.UpDownButtonForeColor = ThemeManager.ButtonText;
                }
                else if (control is Guna2CheckBox chk)
                {
                    chk.ForeColor = ThemeManager.TextPrimary;
                }
            }

            btnSave.FillColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;

            btnCancel.BorderColor = ThemeManager.TextBoxBorder;
            btnCancel.ForeColor = ThemeManager.TextPrimary;
        }
    }
}
