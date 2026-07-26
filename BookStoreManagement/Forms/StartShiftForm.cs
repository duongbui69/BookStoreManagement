using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Themes;
using BookStoreManagement.Services;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class StartShiftForm : Form
    {
        private Guna2Panel pnlMain;
        private Guna2HtmlLabel lblTitle;
        private Guna2HtmlLabel lblShiftName;
        private Guna2ComboBox cboShiftName;
        private Guna2HtmlLabel lblInitialCash;
        private Guna2TextBox txtInitialCash;
        private Guna2Button btnStart;
        private Guna2Button btnCancel;

        private ShiftService _shiftService;

        public StartShiftForm()
        {
            _shiftService = new ShiftService();
            InitializeComponent();
            ApplyTheme();
            
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.FormClosed += (s, e) => ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = "Bắt đầu ca";
            this.Size = new Size(400, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            pnlMain = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            this.Controls.Add(pnlMain);

            lblTitle = new Guna2HtmlLabel
            {
                Text = "Bắt đầu ca mới",
                Font = new Font("Inter", 16F, FontStyle.Bold),
                Location = new Point(24, 24)
            };
            pnlMain.Controls.Add(lblTitle);

            lblShiftName = new Guna2HtmlLabel
            {
                Text = "Tên ca",
                Font = new Font("Inter", 10F, FontStyle.Bold),
                Location = new Point(24, 70)
            };
            pnlMain.Controls.Add(lblShiftName);

            cboShiftName = new Guna2ComboBox
            {
                Location = new Point(24, 95),
                Size = new Size(330, 40),
                BorderRadius = 4,
                Font = new Font("Inter", 10F)
            };
            cboShiftName.Items.AddRange(new string[] { "Sáng", "Chiều", "Tối" });
            cboShiftName.SelectedIndex = 0;
            pnlMain.Controls.Add(cboShiftName);

            lblInitialCash = new Guna2HtmlLabel
            {
                Text = "Tiền mặt đầu ca (VND)",
                Font = new Font("Inter", 10F, FontStyle.Bold),
                Location = new Point(24, 150)
            };
            pnlMain.Controls.Add(lblInitialCash);

            txtInitialCash = new Guna2TextBox
            {
                Location = new Point(24, 175),
                Size = new Size(330, 40),
                BorderRadius = 4,
                Font = new Font("Inter", 10F),
                PlaceholderText = "Nhập số lượng..."
            };
            txtInitialCash.KeyPress += (s, e) => {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
            };
            pnlMain.Controls.Add(txtInitialCash);

            btnCancel = new Guna2Button
            {
                Text = "Hủy bỏ",
                Size = new Size(100, 40),
                Location = new Point(144, 235),
                BorderRadius = 4,
                BorderThickness = 1,
                Font = new Font("Inter", 10F, FontStyle.Bold)
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; };
            pnlMain.Controls.Add(btnCancel);

            btnStart = new Guna2Button
            {
                Text = "Bắt đầu",
                Size = new Size(100, 40),
                Location = new Point(254, 235),
                BorderRadius = 4,
                Font = new Font("Inter", 10F, FontStyle.Bold)
            };
            btnStart.Click += BtnStart_Click;
            pnlMain.Controls.Add(btnStart);
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboShiftName.Text))
            {
                MessageBox.Show("Vui lòng chọn một ca.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtInitialCash.Text, out decimal initialCash) || initialCash < 0)
            {
                MessageBox.Show("Số tiền đầu ca không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _shiftService.OpenShift(cboShiftName.Text, initialCash);
                MessageBox.Show("Bắt đầu ca thành công!", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlMain.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblShiftName.ForeColor = ThemeManager.TextPrimary;
            lblInitialCash.ForeColor = ThemeManager.TextPrimary;

            cboShiftName.FillColor = ThemeManager.TextBoxBackground;
            cboShiftName.ForeColor = ThemeManager.TextPrimary;
            cboShiftName.BorderColor = ThemeManager.TextBoxBorder;

            txtInitialCash.FillColor = ThemeManager.TextBoxBackground;
            txtInitialCash.ForeColor = ThemeManager.TextPrimary;
            txtInitialCash.BorderColor = ThemeManager.TextBoxBorder;

            btnCancel.FillColor = ThemeManager.CardBackground;
            btnCancel.ForeColor = ThemeManager.TextSecondary;
            btnCancel.BorderColor = ThemeManager.TextBoxBorder;

            btnStart.FillColor = ThemeManager.ButtonFill;
            btnStart.ForeColor = ThemeManager.ButtonText;
        }
    }
}
