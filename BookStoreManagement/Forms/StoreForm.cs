using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class StoreForm : Form
    {
        private Store? _store;
        private StoreRepository _repository;
        private Label lblTitle;
        private Guna2TextBox txtStoreCode, txtStoreName, txtAddress, txtPhone, txtManagerName;
        private Guna2CheckBox chkIsActive;
        private Guna2Button btnSave, btnCancel;

        public StoreForm(Store? store = null)
        {
            _store = store;
            _repository = new StoreRepository();
            InitializeComponent();
            SetupTheme();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _store == null ? "Thêm mới Chi nhánh" : "Cập nhật Chi nhánh";
            this.Size = new Size(500, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ControlBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label
            {
                Text = this.Text,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                Location = new Point(24, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            int startY = 70;

            txtStoreCode = CreateInput("Mã Chi nhánh (*)", ref startY);
            txtStoreName = CreateInput("Tên Chi nhánh (*)", ref startY);
            txtAddress = CreateInput("Địa chỉ (*)", ref startY);
            txtPhone = CreateInput("Số điện thoại", ref startY);
            txtManagerName = CreateInput("Tên Người quản lý", ref startY);

            // Status
            chkIsActive = new Guna2CheckBox { Text = "Đang hoạt động", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10F), Checked = true };
            this.Controls.Add(chkIsActive);
            startY += 50;

            // Buttons
            btnCancel = new Guna2Button
            {
                Text = "Hủy bỏ",
                Location = new Point(this.Width - 140, this.Height - 100),
                Size = new Size(100, 45),
                BorderRadius = 8,
                FillColor = Color.Transparent,
                BorderThickness = 1,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            btnSave = new Guna2Button
            {
                Text = "Lưu thay đổi",
                Location = new Point(this.Width - 280, this.Height - 100),
                Size = new Size(130, 45),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private Guna2TextBox CreateInput(string label, ref int y)
        {
            Label lbl = new Label { Text = label, Location = new Point(24, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            Guna2TextBox txt = new Guna2TextBox { Location = new Point(24, y + 25), Width = 436, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lbl, txt });
            y += 75;
            return txt;
        }

        private void SetupTheme()
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

        private void LoadData()
        {
            if (_store != null)
            {
                txtStoreCode.Text = _store.StoreCode;
                txtStoreName.Text = _store.StoreName;
                txtAddress.Text = _store.Address;
                txtPhone.Text = _store.Phone;
                txtManagerName.Text = _store.ManagerName;
                chkIsActive.Checked = _store.IsActive;

                txtStoreCode.Enabled = false; // Usually don't allow changing the code once created
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStoreCode.Text) || 
                string.IsNullOrWhiteSpace(txtStoreName.Text) || 
                string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Vui lòng điền các trường bắt buộc (*).", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_store == null)
            {
                if (_repository.IsStoreCodeExists(txtStoreCode.Text))
                {
                    MessageBox.Show("Mã chi nhánh đã tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newStore = new Store
                {
                    StoreCode = txtStoreCode.Text,
                    StoreName = txtStoreName.Text,
                    Address = txtAddress.Text,
                    Phone = txtPhone.Text,
                    ManagerName = txtManagerName.Text,
                    IsActive = chkIsActive.Checked
                };
                _repository.Add(newStore);
            }
            else
            {
                _store.StoreName = txtStoreName.Text;
                _store.Address = txtAddress.Text;
                _store.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text;
                _store.ManagerName = string.IsNullOrWhiteSpace(txtManagerName.Text) ? null : txtManagerName.Text;
                _store.IsActive = chkIsActive.Checked;

                _repository.Update(_store);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
