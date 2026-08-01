using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.Models;
using Guna.UI2.WinForms;
using System.Collections.Generic;

namespace BookStoreManagement.Forms
{
    public partial class SalaryForm : Form
    {
        private readonly HRService _hrService;
        private Guna2ComboBox cbMonth;
        private Guna2ComboBox cbYear;
        private Guna2Button btnCalculate;
        private Guna2DataGridView dgvData;
        private Label lblTotalSalary;

        public SalaryForm()
        {
            _hrService = new HRService();
            InitializeComponent();
            ApplyTheme();
            LoadMonthsAndYears();
            this.Load += SalaryForm_Load;
        }

        private void InitializeComponent()
        {
            this.Text = "Bảng Tính Lương NV";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ControlBox = false;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 18F, FontStyle.Bold), Location = new Point(24, 20), AutoSize = true };
            
            Label lblMonth = new Label { Text = "Tháng:", Location = new Point(24, 70), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            cbMonth = new Guna2ComboBox { Location = new Point(80, 65), Width = 100, Height = 36, Font = new Font("Segoe UI", 10F) };
            
            Label lblYear = new Label { Text = "Năm:", Location = new Point(200, 70), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            cbYear = new Guna2ComboBox { Location = new Point(250, 65), Width = 120, Height = 36, Font = new Font("Segoe UI", 10F) };
            
            btnCalculate = new Guna2Button { Text = "Tính lương", Location = new Point(390, 65), Width = 120, Height = 36, BorderRadius = 4, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnCalculate.Click += BtnCalculate_Click;
            
            dgvData = new Guna2DataGridView
            {
                Location = new Point(24, 120),
                Size = new Size(935, 380),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            
            lblTotalSalary = new Label { Text = "Tổng quỹ lương: 0 ₫", Location = new Point(24, 520), AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.Red };

            this.Controls.AddRange(new Control[] { lblTitle, lblMonth, cbMonth, lblYear, cbYear, btnCalculate, dgvData, lblTotalSalary });
        }

        private void LoadMonthsAndYears()
        {
            for (int i = 1; i <= 12; i++) cbMonth.Items.Add(i);
            for (int i = DateTime.Now.Year - 5; i <= DateTime.Now.Year + 1; i++) cbYear.Items.Add(i);
            
            cbMonth.SelectedItem = DateTime.Now.Month;
            cbYear.SelectedItem = DateTime.Now.Year;
        }

        private async void SalaryForm_Load(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void BtnCalculate_Click(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            if (cbMonth.SelectedItem == null || cbYear.SelectedItem == null) return;
            int m = (int)cbMonth.SelectedItem;
            int y = (int)cbYear.SelectedItem;
            
            var data = await _hrService.GetSalaryReportAsync(m, y);
            
            dgvData.DataSource = data;
            FormatGrid();
            
            decimal total = 0;
            foreach(var item in data) total += item.TotalSalary;
            lblTotalSalary.Text = $"Tổng quỹ lương: {total:N0} ₫";
        }

        private void FormatGrid()
        {
            if (dgvData.Columns["StaffId"] != null) dgvData.Columns["StaffId"].Visible = false;
            if (dgvData.Columns["UserCode"] != null) dgvData.Columns["UserCode"].HeaderText = "Mã NV";
            if (dgvData.Columns["FullName"] != null) dgvData.Columns["FullName"].HeaderText = "Họ và Tên";
            if (dgvData.Columns["StoreName"] != null) dgvData.Columns["StoreName"].HeaderText = "Chi nhánh";
            
            if (dgvData.Columns["HourlyRate"] != null) {
                dgvData.Columns["HourlyRate"].HeaderText = "Lương/giờ";
                dgvData.Columns["HourlyRate"].DefaultCellStyle.Format = "N0";
            }
            if (dgvData.Columns["TotalHours"] != null) {
                dgvData.Columns["TotalHours"].HeaderText = "Tổng giờ làm";
                dgvData.Columns["TotalHours"].DefaultCellStyle.Format = "N2";
            }
            if (dgvData.Columns["TotalSalary"] != null) {
                dgvData.Columns["TotalSalary"].HeaderText = "Tổng lương";
                dgvData.Columns["TotalSalary"].DefaultCellStyle.Format = "N0";
                dgvData.Columns["TotalSalary"].DefaultCellStyle.ForeColor = Color.Red;
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            this.ForeColor = ThemeManager.TextPrimary;
            
            foreach (Control c in this.Controls)
            {
                if (c is Label lbl) lbl.ForeColor = ThemeManager.TextPrimary;
            }
            lblTotalSalary.ForeColor = Color.FromArgb(220, 38, 38);
            
            ThemeManager.ApplyDataGridViewStyle(dgvData);
        }
    }
}
