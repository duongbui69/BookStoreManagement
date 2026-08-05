using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;
using System.Linq;

namespace BookStoreManagement.Forms
{
    public class ShiftDetailsForm : Form
    {
        private int _shiftId;
        private ShiftService _shiftService;
        private SalesOrderService _orderService;

        private Guna2Panel pnlHeader;
        private Guna2HtmlLabel lblTitle;
        private Guna2Button btnClose;

        private Guna2Panel pnlInfo;
        private Guna2HtmlLabel lblShiftId;
        private Guna2HtmlLabel lblTime;
        private Guna2HtmlLabel lblRevenue;
        private Guna2HtmlLabel lblStatus;

        private Guna2DataGridView dgvOrders;
        private Guna2HtmlLabel lblOrdersTitle;

        public ShiftDetailsForm(int shiftId)
        {
            _shiftId = shiftId;
            _shiftService = new ShiftService();
            _orderService = new SalesOrderService();
            
            InitializeUI();
            ApplyTheme();
            LoadData();

            EventHandler themeHandler = (s, e) => ApplyTheme();
            ThemeManager.ThemeChanged += themeHandler;
            this.FormClosed += (s, e) => ThemeManager.ThemeChanged -= themeHandler;
        }

        private void InitializeUI()
        {
            this.Size = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = ThemeManager.Background;
            this.Padding = new Padding(2);

            // Header
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60 };
            lblTitle = new Guna2HtmlLabel { Text = "Chi tiết ca làm việc #" + _shiftId, Font = new Font("Inter", 16F, FontStyle.Bold), Location = new Point(20, 15) };
            btnClose = new Guna2Button { Text = "X", Size = new Size(40, 40), Location = new Point(this.Width - 42 - 10, 10), FillColor = Color.Transparent, ForeColor = ThemeManager.TextPrimary, Font = new Font("Inter", 14F, FontStyle.Bold) };
            btnClose.Click += (s, e) => this.Close();
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnClose);
            this.Controls.Add(pnlHeader);

            // Info Panel
            pnlInfo = new Guna2Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(20) };
            lblShiftId = new Guna2HtmlLabel { Text = "Ca: -", Font = new Font("Inter", 12F), Location = new Point(20, 20) };
            lblStatus = new Guna2HtmlLabel { Text = "Trạng thái: -", Font = new Font("Inter", 12F), Location = new Point(400, 20) };
            lblTime = new Guna2HtmlLabel { Text = "Thời gian: -", Font = new Font("Inter", 12F), Location = new Point(20, 50) };
            lblRevenue = new Guna2HtmlLabel { Text = "Tổng tiền ban đầu: 0 đ", Font = new Font("Inter", 12F, FontStyle.Bold), Location = new Point(20, 80) };
            pnlInfo.Controls.AddRange(new Control[] { lblShiftId, lblTime, lblRevenue, lblStatus });
            this.Controls.Add(pnlInfo);

            // Orders Grid
            Guna2Panel pnlGridHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 40 };
            lblOrdersTitle = new Guna2HtmlLabel { Text = "Danh sách hóa đơn trong ca", Font = new Font("Inter", 12F, FontStyle.Bold), Location = new Point(20, 10) };
            pnlGridHeader.Controls.Add(lblOrdersTitle);
            this.Controls.Add(pnlGridHeader);

            dgvOrders = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 40 }
            };
            dgvOrders.Columns.Add("Id", "Mã HĐ");
            dgvOrders.Columns.Add("Time", "Thời gian");
            dgvOrders.Columns.Add("Total", "Tổng tiền");
            dgvOrders.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            
            Guna2Panel pnlGrid = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            pnlGrid.Controls.Add(dgvOrders);
            this.Controls.Add(pnlGrid);

            pnlHeader.BringToFront();
            pnlInfo.BringToFront();
            pnlGridHeader.BringToFront();
            pnlGrid.BringToFront();

            // Removed Guna2BorderlessForm to prevent UI thread deadlock
            // Guna2BorderlessForm borderlessForm = new Guna2BorderlessForm { ContainerControl = this, BorderRadius = 12 };

        }

        private void LoadData()
        {
            var shift = _shiftService.GetShiftById(_shiftId);
            if (shift != null)
            {
                lblShiftId.Text = $"Ca: {shift.ShiftName} (Nhân viên: {shift.StaffId})";
                lblTime.Text = $"Thời gian: {shift.StartTime:dd/MM/yyyy HH:mm} - {(shift.EndTime.HasValue ? shift.EndTime.Value.ToString("HH:mm") : "Đang mở")}";
                lblStatus.Text = $"Trạng thái: {(shift.EndTime.HasValue ? "Đã kết thúc" : "Đang mở")}";

                var orders = _orderService.GetByDateRange(shift.StartTime, shift.EndTime ?? DateTime.Now)
                             .Where(o => o.StaffId == shift.StaffId).ToList();

                decimal totalRevenue = orders.Sum(o => o.TotalAmount);
                decimal totalRefund = orders.Sum(o => o.RefundAmount);
                decimal actualRevenue = orders.Sum(o => o.ActualAmount);

                lblRevenue.Text = $"Tổng tiền ban đầu: {totalRevenue:N0} đ | Hoàn: {totalRefund:N0} đ | Thực nhận (Doanh số): {actualRevenue:N0} đ";

                try
                {
                    dgvOrders.Columns.Clear();
                    dgvOrders.Columns.Add("Code", "Mã HĐ");
                    dgvOrders.Columns.Add("Time", "Thời gian");
                    dgvOrders.Columns.Add("Total", "Tổng tiền");
                    dgvOrders.Columns["Total"].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                    
                    dgvOrders.Columns.Add("Refund", "Tiền hoàn");
                    dgvOrders.Columns["Refund"].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                    
                    dgvOrders.Columns.Add("Actual", "Thực nhận");
                    dgvOrders.Columns["Actual"].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;

                    dgvOrders.Rows.Clear();
                    foreach (var order in orders)
                    {
                        dgvOrders.Rows.Add(
                            order.OrderCode,
                            order.OrderDate.ToString("HH:mm:ss"),
                            order.TotalAmount.ToString("N0") + " đ",
                            order.RefundAmount > 0 ? "-" + order.RefundAmount.ToString("N0") + " đ" : "0 đ",
                            order.ActualAmount.ToString("N0") + " đ"
                        );
                    }
                }
                catch (System.Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Lỗi ShiftDetailsForm", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            btnClose.ForeColor = ThemeManager.TextPrimary;
            
            lblShiftId.ForeColor = ThemeManager.TextPrimary;
            lblTime.ForeColor = ThemeManager.TextSecondary;
            lblRevenue.ForeColor = ThemeManager.TextPrimary;
            lblStatus.ForeColor = ThemeManager.TextSecondary;
            lblOrdersTitle.ForeColor = ThemeManager.TextPrimary;

            ThemeManager.ApplyDataGridViewStyle(dgvOrders);
        }
    }
}
