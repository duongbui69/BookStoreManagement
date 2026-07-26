using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;

namespace BookStoreManagement.UserControls
{
    public class VoucherTypeEditForm : Form
    {
        private readonly VoucherTypeService _service;
        private readonly VoucherType _vt;
        
        private Guna2Panel pnlHeader;
        private Label lblTitle;
        private Guna2Button btnClose;

        private Guna2Panel pnlContent;
        private Guna2TextBox txtCode;
        private Guna2TextBox txtName;
        private Guna2TextBox txtDesc;
        private Guna2ComboBox cbGroup;
        private Guna2ComboBox cbStatus;

        private Guna2Panel pnlFooter;
        private Guna2Button btnSave;
        private Guna2Button btnCancel;

        public VoucherTypeEditForm(VoucherType? vt)
        {
            _service = new VoucherTypeService();
            _vt = vt ?? new VoucherType();
            InitializeComponent();
            ApplyTheme();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 520);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;

            // Header
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, BorderThickness = 1 };
            lblTitle = new Label { Text = _vt.Id == 0 ? "Thêm Loại phiếu" : "Sửa Loại phiếu", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            btnClose = new Guna2Button { Text = "X", Size = new Size(40, 40), Location = new Point(this.Width - 50, 10), FillColor = Color.Transparent, Cursor = Cursors.Hand };
            btnClose.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, btnClose });

            // Content
            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };

            txtCode = CreateInput("Mã loại", 20);
            txtName = CreateInput("Tên loại", 90);
            txtDesc = CreateInput("Mô tả", 160);
            
            cbGroup = CreateDropdown("Nhóm", 230, new[] { "Nhập kho", "Xuất kho", "Khác" });
            cbStatus = CreateDropdown("Trạng thái", 300, new[] { "Đang hoạt động", "Đã khóa" });

            // Footer
            pnlFooter = new Guna2Panel { Dock = DockStyle.Bottom, Height = 70, BorderThickness = 1 };
            btnCancel = new Guna2Button { Text = "Hủy bỏ", Size = new Size(100, 40), Location = new Point(this.Width - 250, 15), BorderRadius = 4, Cursor = Cursors.Hand };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            btnSave = new Guna2Button { Text = "Lưu (Save)", Size = new Size(120, 40), Location = new Point(this.Width - 140, 15), BorderRadius = 4, Cursor = Cursors.Hand };
            btnSave.Click += BtnSave_Click;

            pnlFooter.Controls.AddRange(new Control[] { btnCancel, btnSave });

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);
        }

        private Guna2TextBox CreateInput(string label, int y)
        {
            var lbl = new Label { Text = label, Location = new Point(24, y), AutoSize = true, Font = new Font("Segoe UI", 11F) };
            var txt = new Guna2TextBox { Location = new Point(24, y + 25), Size = new Size(452, 36), BorderRadius = 4 };
            pnlContent.Controls.AddRange(new Control[] { lbl, txt });
            return txt;
        }

        private Guna2ComboBox CreateDropdown(string label, int y, string[] items)
        {
            var lbl = new Label { Text = label, Location = new Point(24, y), AutoSize = true, Font = new Font("Segoe UI", 11F) };
            var cb = new Guna2ComboBox { Location = new Point(24, y + 25), Size = new Size(452, 36), BorderRadius = 4 };
            cb.Items.AddRange(items);
            pnlContent.Controls.AddRange(new Control[] { lbl, cb });
            return cb;
        }

        private void LoadData()
        {
            if (_vt.Id > 0)
            {
                txtCode.Text = _vt.Code;
                txtName.Text = _vt.Name;
                txtDesc.Text = _vt.Description;
                cbGroup.SelectedItem = _vt.GroupType;
                cbStatus.SelectedItem = _vt.Status;
            }
            else
            {
                cbGroup.SelectedIndex = 0;
                cbStatus.SelectedIndex = 0;
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                _vt.Code = txtCode.Text;
                _vt.Name = txtName.Text;
                _vt.Description = txtDesc.Text;
                _vt.GroupType = cbGroup.SelectedItem?.ToString() ?? "Khác";
                _vt.Status = cbStatus.SelectedItem?.ToString() ?? "Đang hoạt động";

                await _service.SaveAsync(_vt);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            pnlHeader.BackColor = ThemeManager.CardBackground;
            pnlHeader.CustomBorderColor = ThemeManager.TextBoxBorder;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            btnClose.ForeColor = ThemeManager.TextSecondary;

            pnlContent.BackColor = ThemeManager.CardBackground;
            foreach (Control c in pnlContent.Controls)
            {
                if (c is Label l) l.ForeColor = ThemeManager.TextPrimary;
                if (c is Guna2TextBox t) { t.FillColor = ThemeManager.TextBoxBackground; t.ForeColor = ThemeManager.TextPrimary; t.BorderColor = ThemeManager.TextBoxBorder; }
                if (c is Guna2ComboBox cb) { cb.FillColor = ThemeManager.TextBoxBackground; cb.ForeColor = ThemeManager.TextPrimary; cb.BorderColor = ThemeManager.TextBoxBorder; }
            }

            pnlFooter.BackColor = ThemeManager.CardBackground;
            pnlFooter.CustomBorderColor = ThemeManager.TextBoxBorder;

            btnCancel.FillColor = ThemeManager.CardBackground;
            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.BorderThickness = 1;
            btnCancel.BorderColor = ThemeManager.TextBoxBorder;

            btnSave.FillColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;
        }
    }
}

