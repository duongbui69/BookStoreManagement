using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Themes;
using BookStoreManagement.Services;
using Guna.UI2.WinForms;
using BookStoreManagement.Helpers;

namespace BookStoreManagement.Forms
{
    public class CloseShiftDialog : Form
    {
        private Guna2Panel pnlMain;
        private Guna2HtmlLabel lblTitle;
        private Guna2HtmlLabel lblMessage;
        private Guna2HtmlLabel lblRevenue;
        private Guna2HtmlLabel lblOrders;
        private Guna2Button btnConfirm;
        private Guna2Button btnCancel;

        private ShiftService _shiftService;
        private int _shiftId;

        public CloseShiftDialog(int shiftId, decimal currentRevenue, int currentOrders)
        {
            _shiftService = new ShiftService();
            _shiftId = shiftId;
            InitializeComponent(currentRevenue, currentOrders);
            ApplyTheme();
            
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.FormClosed += (s, e) => ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void InitializeComponent(decimal currentRevenue, int currentOrders)
        {
            this.Text = "Kết thúc ca làm việc";
            this.Size = new Size(400, 260);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            pnlMain = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            this.Controls.Add(pnlMain);

            lblTitle = new Guna2HtmlLabel
            {
                Text = "Xác nhận đóng ca",
                Font = new Font("Inter", 16F, FontStyle.Bold),
                Location = new Point(24, 24)
            };
            pnlMain.Controls.Add(lblTitle);

            lblMessage = new Guna2HtmlLabel
            {
                Text = "Bạn có chắc chắn muốn kết thúc ca hiện tại không?",
                Font = new Font("Inter", 10F),
                Location = new Point(24, 65)
            };
            pnlMain.Controls.Add(lblMessage);

            lblRevenue = new Guna2HtmlLabel
            {
                Text = $"Doanh số thực tế: <span style='font-weight:bold;color:#00ba61;'>{currentRevenue.ToString("N0")} ₫</span>",
                Font = new Font("Inter", 10F),
                Location = new Point(24, 95),
                UseGdiPlusTextRendering = true
            };
            pnlMain.Controls.Add(lblRevenue);

            lblOrders = new Guna2HtmlLabel
            {
                Text = $"Tổng số đơn hàng: <span style='font-weight:bold;'>{currentOrders}</span>",
                Font = new Font("Inter", 10F),
                Location = new Point(24, 120),
                UseGdiPlusTextRendering = true
            };
            pnlMain.Controls.Add(lblOrders);

            btnCancel = new Guna2Button
            {
                Text = "Hủy bỏ",
                Size = new Size(100, 40),
                Location = new Point(144, 165),
                BorderRadius = 4,
                BorderThickness = 1,
                Font = new Font("Inter", 10F, FontStyle.Bold)
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; };
            pnlMain.Controls.Add(btnCancel);

            btnConfirm = new Guna2Button
            {
                Text = "Kết thúc",
                Size = new Size(100, 40),
                Location = new Point(254, 165),
                BorderRadius = 4,
                FillColor = Color.FromArgb(186, 26, 26), // Error color for dangerous action
                ForeColor = Color.White,
                Font = new Font("Inter", 10F, FontStyle.Bold)
            };
            btnConfirm.Click += BtnConfirm_Click;
            pnlMain.Controls.Add(btnConfirm);
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            try
            {
                _shiftService.CloseShift(_shiftId);
                MessageBox.Show("Đóng ca thành công!", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlMain.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblMessage.ForeColor = ThemeManager.TextPrimary;
            lblRevenue.ForeColor = ThemeManager.TextPrimary;
            lblOrders.ForeColor = ThemeManager.TextPrimary;

            btnCancel.FillColor = ThemeManager.CardBackground;
            btnCancel.ForeColor = ThemeManager.TextSecondary;
            btnCancel.BorderColor = ThemeManager.TextBoxBorder;
        }
    }
}
