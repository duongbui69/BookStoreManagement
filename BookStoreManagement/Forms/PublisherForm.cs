using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class PublisherForm : Form
    {
        private PublisherRepository _publisherRepository;
        private Publisher _currentPublisher;
        public bool IsDataSaved { get; private set; } = false;

        private Label lblTitle;
        private Guna2TextBox txtId, txtName, txtPhone, txtEmail, txtAddress;
        private Guna2CheckBox chkIsActive;
        private Guna2Button btnSave, btnCancel;
        private ErrorProvider _errorProvider;

        public PublisherForm(int? publisherId = null)
        {
            _publisherRepository = new PublisherRepository();
            _errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            InitializeComponentLayout();
            _errorProvider.ContainerControl = this;
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;

            if (publisherId.HasValue)
            {
                lblTitle.Text = "Chỉnh sửa NXB";
                LoadPublisherData(publisherId.Value);
            }
            else
            {
                lblTitle.Text = "Thêm mới NXB";
                _currentPublisher = new Publisher();
                chkIsActive.Checked = true;
                txtId.Text = "Tạo tự động";
            }
        }

        private void InitializeComponentLayout()
        {
            this.Text = "Cập nhật Nhà Xuất Bản";
            this.Size = new Size(500, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ControlBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label { Location = new Point(24, 20), AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold) };
            this.Controls.Add(lblTitle);

            int startY = 70;

            txtId = CreateInput("Mã NXB", ref startY);
            txtId.ReadOnly = true;
            txtId.Enabled = false;

            txtName = CreateInput("Tên Nhà xuất bản (*)", ref startY);
            txtName.MaxLength = 100;
            ValidationHelper.WireTextOnly(txtName, _errorProvider);

            txtPhone = CreateInput("Số điện thoại", ref startY);
            txtPhone.MaxLength = 20;
            ValidationHelper.WireDigitsOnly(txtPhone, _errorProvider);

            txtEmail = CreateInput("Email", ref startY);
            txtEmail.MaxLength = 100;
            ValidationHelper.WireEmailValidation(txtEmail, _errorProvider);

            txtAddress = CreateInput("Địa chỉ", ref startY);
            txtAddress.MaxLength = 255;

            chkIsActive = new Guna2CheckBox { Text = "Đang hợp tác (Hoạt động)", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(chkIsActive);
            startY += 50;

            btnCancel = new Guna2Button
            {
                Text = "Hủy bỏ",
                Size = new Size(100, 45),
                Location = new Point(this.Width - 140, this.Height - 100),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FillColor = Color.Transparent,
                BorderThickness = 1
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            btnSave = new Guna2Button
            {
                Text = "Lưu thay đổi",
                Size = new Size(130, 45),
                Location = new Point(this.Width - 280, this.Height - 100),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private Guna2TextBox CreateInput(string label, ref int y)
        {
            Label lbl = new Label { Text = label, Location = new Point(24, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            Guna2TextBox txt = new Guna2TextBox { Location = new Point(24, y + 25), Width = 436, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lbl, txt });
            y += 75;
            return txt;
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            lblTitle.ForeColor = ThemeManager.TextPrimary;

            foreach (Control control in this.Controls)
            {
                if (control is Label lbl && lbl != lblTitle)
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

        private async void LoadPublisherData(int id)
        {
            try
            {
                _currentPublisher = await _publisherRepository.GetByIdAsync(id);
                if (_currentPublisher != null)
                {
                    txtId.Text = $"NXB{_currentPublisher.Id:D3}";
                    txtName.Text = _currentPublisher.PublisherName;
                    txtPhone.Text = _currentPublisher.Phone;
                    txtEmail.Text = _currentPublisher.Email;
                    txtAddress.Text = _currentPublisher.Address;
                    chkIsActive.Checked = _currentPublisher.IsActive;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy dữ liệu NXB!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập tên NXB!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (await _publisherRepository.IsNameExistsAsync(name, _currentPublisher?.Id > 0 ? _currentPublisher.Id : null))
            {
                MessageBox.Show("Tên NXB đã tồn tại!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
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

            try
            {
                _currentPublisher.PublisherName = name;
                _currentPublisher.Phone = txtPhone.Text.Trim();
                _currentPublisher.Email = txtEmail.Text.Trim();
                _currentPublisher.Address = txtAddress.Text.Trim();
                _currentPublisher.IsActive = chkIsActive.Checked;

                if (_currentPublisher.Id == 0)
                {
                    await _publisherRepository.AddAsync(_currentPublisher);
                    MessageBox.Show("Thêm mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    await _publisherRepository.UpdateAsync(_currentPublisher);
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                IsDataSaved = true;
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
