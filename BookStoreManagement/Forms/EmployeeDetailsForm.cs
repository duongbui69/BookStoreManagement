using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.ViewModels;
using Guna.UI2.WinForms;
using System.Collections.Generic;
using System.Linq;

namespace BookStoreManagement.Forms
{
    public class EmployeeDetailsForm : Form
    {
        private int _employeeId;
        private HRService _hrService;
        private ShiftService _shiftService;

        private Guna2Panel pnlHeader;
        private Guna2HtmlLabel lblTitle;
        private Guna2Button btnClose;

        private Guna2Panel pnlInfo;
        private Guna2HtmlLabel lblName;
        private Guna2HtmlLabel lblRole;
        private Guna2HtmlLabel lblPhone;

        private Guna2Panel pnlGridHeader;
        private Guna2HtmlLabel lblShiftsTitle;
        private Guna2DateTimePicker dtpFrom;
        private Guna2DateTimePicker dtpTo;
        private Guna2Button btnFilter;

        private Guna2DataGridView dgvShifts;

        public EmployeeDetailsForm(int employeeId)
        {
            _employeeId = employeeId;
            _hrService = new HRService();
            _shiftService = new ShiftService();
            
            InitializeUI();
            ApplyTheme();
            LoadData();

            EventHandler themeHandler = (s, e) => ApplyTheme();
            ThemeManager.ThemeChanged += themeHandler;
            this.FormClosed += (s, e) => ThemeManager.ThemeChanged -= themeHandler;
        }

        private void InitializeUI()
        {
            this.Size = new Size(900, 700);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = ThemeManager.Background;
            this.Padding = new Padding(2);

            // Header
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60 };
            lblTitle = new Guna2HtmlLabel { Text = "Hồ sơ nhân viên", Font = new Font("Inter", 16F, FontStyle.Bold), Location = new Point(20, 15) };
            btnClose = new Guna2Button { Text = "X", Size = new Size(40, 40), Location = new Point(this.Width - 42 - 10, 10), FillColor = Color.Transparent, ForeColor = ThemeManager.TextPrimary, Font = new Font("Inter", 14F, FontStyle.Bold) };
            btnClose.Click += (s, e) => this.Close();
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnClose);
            this.Controls.Add(pnlHeader);

            // Info Panel (Bento style)
            pnlInfo = new Guna2Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(20) };
            lblName = new Guna2HtmlLabel { Text = "Tên: -", Font = new Font("Inter", 14F, FontStyle.Bold), Location = new Point(20, 10) };
            lblRole = new Guna2HtmlLabel { Text = "Vai trò: -", Font = new Font("Inter", 12F), Location = new Point(20, 45) };
            lblPhone = new Guna2HtmlLabel { Text = "SĐT: -", Font = new Font("Inter", 12F), Location = new Point(400, 45) };
            pnlInfo.Controls.AddRange(new Control[] { lblName, lblRole, lblPhone });
            this.Controls.Add(pnlInfo);

            // Spacing
            Guna2Panel spacer = new Guna2Panel { Dock = DockStyle.Top, Height = 20 };
            this.Controls.Add(spacer);

            // Shifts Header
            pnlGridHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60 };
            lblShiftsTitle = new Guna2HtmlLabel { Text = "Lịch sử ca làm việc", Font = new Font("Inter", 12F, FontStyle.Bold), Location = new Point(20, 20) };
            
            dtpFrom = new Guna2DateTimePicker { Size = new Size(150, 36), Location = new Point(this.Width - 430, 10), Format = DateTimePickerFormat.Short, Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) };
            dtpTo = new Guna2DateTimePicker { Size = new Size(150, 36), Location = new Point(this.Width - 270, 10), Format = DateTimePickerFormat.Short, Value = DateTime.Now };
            btnFilter = new Guna2Button { Text = "LỌC", Size = new Size(80, 36), Location = new Point(this.Width - 110, 10), BorderRadius = 4, Font = new Font("Inter", 9F, FontStyle.Bold) };
            btnFilter.Click += (s, e) => LoadShifts();

            pnlGridHeader.Controls.AddRange(new Control[] { lblShiftsTitle, dtpFrom, dtpTo, btnFilter });
            this.Controls.Add(pnlGridHeader);

            // Shifts Grid
            dgvShifts = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 40 }
            };
            dgvShifts.Columns.Add("Id", "ID Ca");
            dgvShifts.Columns.Add("Date", "Ngày");
            dgvShifts.Columns.Add("Time", "Thời gian");
            dgvShifts.Columns.Add("Revenue", "Doanh thu");
            dgvShifts.Columns["Revenue"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            
            Guna2Panel pnlGrid = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            pnlGrid.Controls.Add(dgvShifts);
            this.Controls.Add(pnlGrid);

            dgvShifts.CellDoubleClick += DgvShifts_CellDoubleClick;

            // Removed Guna2BorderlessForm because it can cause UI thread deadlocks when initialized inside the constructor without a handle.
            // Z-Order
            pnlHeader.SendToBack();
            pnlInfo.SendToBack();
            spacer.SendToBack();
            pnlGridHeader.SendToBack();
            pnlGrid.BringToFront();
        }

        private void LoadData()
        {
            var emp = _hrService.GetById(_employeeId);
            if (emp != null)
            {
                lblName.Text = $"{emp.FullName} (EMP-{emp.Id})";
                lblRole.Text = $"Vai trò: {emp.RoleName}";
                lblPhone.Text = $"SĐT: {emp.Phone}";
            }
            LoadShifts();
        }

        private void LoadShifts()
        {
            var allShifts = _shiftService.GetAllShifts();
            var filtered = allShifts.Where(s => s.StaffId == _employeeId && s.StartTime.Date >= dtpFrom.Value.Date && s.StartTime.Date <= dtpTo.Value.Date).ToList();

            dgvShifts.Rows.Clear();
            foreach (var shift in filtered)
            {
                dgvShifts.Rows.Add(
                    shift.Id,
                    shift.StartTime.ToString("dd/MM/yyyy"),
                    $"{shift.StartTime:HH:mm} - {(shift.EndTime.HasValue ? shift.EndTime.Value.ToString("HH:mm") : "Đang mở")}",
                    shift.Revenue.ToString("N0") + " đ"
                );
            }
        }

        private void DgvShifts_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int shiftId = Convert.ToInt32(dgvShifts.Rows[e.RowIndex].Cells["Id"].Value);
                using (var detailsForm = new ShiftDetailsForm(shiftId))
                {
                    detailsForm.ShowDialog(this);
                }
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            btnClose.ForeColor = ThemeManager.TextPrimary;
            
            lblName.ForeColor = ThemeManager.TextPrimary;
            lblRole.ForeColor = ThemeManager.TextSecondary;
            lblPhone.ForeColor = ThemeManager.TextSecondary;
            lblShiftsTitle.ForeColor = ThemeManager.TextPrimary;

            dtpFrom.FillColor = ThemeManager.TextBoxBackground;
            dtpFrom.ForeColor = ThemeManager.TextPrimary;
            dtpTo.FillColor = ThemeManager.TextBoxBackground;
            dtpTo.ForeColor = ThemeManager.TextPrimary;

            btnFilter.FillColor = ThemeManager.HoverColor;
            btnFilter.ForeColor = ThemeManager.TextPrimary;

            ThemeManager.ApplyDataGridViewStyle(dgvShifts);
        }
    }
}
