using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class AuthorForm : Form
    {
        private AuthorRepository _authorRepository;
        private Author _currentAuthor;
        public bool IsDataSaved { get; private set; } = false;

        private Label lblTitle;
        private Guna2TextBox txtId, txtName, txtNationality, txtDescription;
        private Guna2CheckBox chkIsActive;
        private Guna2Button btnSave, btnCancel;

        public AuthorForm(int? authorId = null)
        {
            _authorRepository = new AuthorRepository();
            InitializeComponentLayout();
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;

            if (authorId.HasValue)
            {
                lblTitle.Text = "Chỉnh sửa Tác giả";
                LoadAuthorData(authorId.Value);
            }
            else
            {
                lblTitle.Text = "Thêm mới Tác giả";
                _currentAuthor = new Author();
                chkIsActive.Checked = true;
                txtId.Text = "Tạo tự động";
            }
        }

        private void InitializeComponentLayout()
        {
            this.Text = "Cập nhật Tác giả";
            this.Size = new Size(500, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label { Location = new Point(24, 20), AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold) };
            this.Controls.Add(lblTitle);

            int startY = 70;

            txtId = CreateInput("Mã tác giả", ref startY);
            txtId.ReadOnly = true;
            txtId.Enabled = false;

            txtName = CreateInput("Tên Tác giả (*)", ref startY);
            txtNationality = CreateInput("Quốc tịch", ref startY);
            
            Label lblDesc = new Label { Text = "Mô tả", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtDescription = new Guna2TextBox { Location = new Point(24, startY + 25), Width = 436, Height = 80, Multiline = true, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lblDesc, txtDescription });
            startY += 120;

            chkIsActive = new Guna2CheckBox { Text = "Đang hoạt động", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(chkIsActive);

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

        private async void LoadAuthorData(int id)
        {
            try
            {
                _currentAuthor = await _authorRepository.GetByIdAsync(id);
                if (_currentAuthor != null)
                {
                    txtId.Text = $"TG{_currentAuthor.Id:D3}";
                    txtName.Text = _currentAuthor.AuthorName;
                    txtNationality.Text = _currentAuthor.Nationality;
                    txtDescription.Text = _currentAuthor.Description;
                    chkIsActive.Checked = _currentAuthor.IsActive;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy dữ liệu Tác giả!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Vui lòng nhập tên tác giả!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (await _authorRepository.IsNameExistsAsync(name, _currentAuthor?.Id > 0 ? _currentAuthor.Id : null))
            {
                MessageBox.Show("Tên tác giả đã tồn tại!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                _currentAuthor.AuthorName = name;
                _currentAuthor.Nationality = txtNationality.Text.Trim();
                _currentAuthor.Description = txtDescription.Text.Trim();
                _currentAuthor.IsActive = chkIsActive.Checked;

                if (_currentAuthor.Id == 0)
                {
                    await _authorRepository.AddAsync(_currentAuthor);
                    MessageBox.Show("Thêm mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    await _authorRepository.UpdateAsync(_currentAuthor);
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                IsDataSaved = true;
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
