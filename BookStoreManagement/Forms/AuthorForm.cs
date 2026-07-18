using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public partial class AuthorForm : Form
    {
        private AuthorRepository _authorRepository;
        private Author? _currentAuthor;
        public bool IsDataSaved { get; private set; }

        private Label lblTitle, lblId, lblName, lblNationality, lblDescription, lblStatus;
        private TextBox txtId, txtName, txtNationality, txtDescription;
        private CheckBox chkIsActive;
        private Button btnSave, btnCancel;

        public AuthorForm(int? authorId = null)
        {
            InitializeComponentLayout();
            _authorRepository = new AuthorRepository();
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;

            if (authorId.HasValue)
            {
                lblTitle.Text = "Chỉnh sửa Tác giả";
                LoadAuthorData(authorId.Value);
            }
            else
            {
                lblTitle.Text = "Thêm Tác giả mới";
                _currentAuthor = new Author();
                chkIsActive.Checked = true;
                txtId.Text = "Tạo tự động";
            }
        }

        private void InitializeComponentLayout()
        {
            this.Text = "Cập nhật Tác giả";
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label { Location = new Point(24, 24), AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold) };
            this.Controls.Add(lblTitle);

            int startY = 80;
            int padding = 20;

            lblId = new Label { Text = "Mã tác giả", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtId = new TextBox { Location = new Point(24, startY + 25), Width = 436, Font = new Font("Segoe UI", 10), ReadOnly = true, Enabled = false };
            this.Controls.AddRange(new Control[] { lblId, txtId });
            startY += 70;

            lblName = new Label { Text = "Tên tác giả (*)", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtName = new TextBox { Location = new Point(24, startY + 25), Width = 436, Font = new Font("Segoe UI", 10) };
            this.Controls.AddRange(new Control[] { lblName, txtName });
            startY += 70;

            lblNationality = new Label { Text = "Quốc tịch", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtNationality = new TextBox { Location = new Point(24, startY + 25), Width = 436, Font = new Font("Segoe UI", 10) };
            this.Controls.AddRange(new Control[] { lblNationality, txtNationality });
            startY += 70;

            lblDescription = new Label { Text = "Mô tả", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtDescription = new TextBox { Location = new Point(24, startY + 25), Width = 436, Height = 60, Multiline = true, Font = new Font("Segoe UI", 10) };
            this.Controls.AddRange(new Control[] { lblDescription, txtDescription });
            startY += 100;

            lblStatus = new Label { Text = "Trạng thái", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            chkIsActive = new CheckBox { Text = "Đang hoạt động", Location = new Point(24, startY + 25), AutoSize = true, Font = new Font("Segoe UI", 10) };
            this.Controls.AddRange(new Control[] { lblStatus, chkIsActive });

            btnCancel = new Button
            {
                Text = "Hủy",
                Size = new Size(100, 36),
                Location = new Point(this.Width - 250, this.Height - 80),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            btnSave = new Button
            {
                Text = "Lưu thay đổi",
                Size = new Size(120, 36),
                Location = new Point(this.Width - 140, this.Height - 80),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            
            Label[] labels = { lblId, lblName, lblNationality, lblDescription, lblStatus };
            foreach (var lbl in labels)
            {
                lbl.ForeColor = ThemeManager.TextSecondary;
            }

            TextBox[] textBoxes = { txtId, txtName, txtNationality, txtDescription };
            foreach (var txt in textBoxes)
            {
                txt.BackColor = ThemeManager.TextBoxBackground;
                txt.ForeColor = ThemeManager.TextPrimary;
                txt.BorderStyle = BorderStyle.FixedSingle;
            }

            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;
            btnCancel.BackColor = ThemeManager.CardBackground;

            btnSave.BackColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;
            btnSave.FlatAppearance.BorderSize = 0;
            
            chkIsActive.ForeColor = ThemeManager.TextPrimary;
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
                    MessageBox.Show("Không tìm thấy dữ liệu tác giả!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Tên tác giả này đã tồn tại!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                }
                else
                {
                    await _authorRepository.UpdateAsync(_currentAuthor);
                }

                IsDataSaved = true;
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

