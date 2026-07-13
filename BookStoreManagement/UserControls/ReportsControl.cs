using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using BookStoreManagement.Services;
using BookStoreManagement.Interfaces;
using Guna.UI2.WinForms;

namespace BookStoreManagement.UserControls
{
    public class MonthlyPerformance
    {
        public int Id { get; set; }
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal NetProfit => Revenue - Expenses;
        public string Status { get; set; } = string.Empty;
    }

    public partial class ReportsControl : UserControl, ISearchableControl
    {
        private readonly ReportService _service;
        private ReportStats _stats;
        private List<MonthlyPerformance> _allMonths = new List<MonthlyPerformance>();
        
        // Content Container
        private Guna2Panel pnlContent;

        // Header & Actions
        private Guna2Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        private Guna2ComboBox cbDateRange;
        private Guna2Button btnExportPDF;
        private Guna2Button btnExportExcel;

        // KPI Cards
        private Guna2Panel pnlMetrics;
        private Guna2Panel cardRevenue;
        private Guna2Panel cardExpenses;
        private Guna2Panel cardNetProfit;

        // Chart
        private Guna2Panel pnlChart;

        // Grid
        private Guna2Panel pnlGridContainer;
        private Guna2DataGridView dgvReports;
        
        // Pagination
        private PaginationControl paginationControl;
        private int _currentPage = 1;
        private int _pageSize = 5;

        public void PerformSearch(string keyword)
        {
            // Optional: filter months by name
        }

        public ReportsControl()
        {
            _service = new ReportService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += ReportsControl_Load;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 20;
            this.Padding = new Padding(0);
            this.AutoScroll = true;

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = true };

            // 1. Page Header
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 70, Margin = new Padding(0, 0, 0, gutter) };
            lblTitle = new Label { Text = "Financial Reports", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Comprehensive overview of Q3 performance and revenue metrics.", Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(2, 40) };
            
            cbDateRange = new Guna2ComboBox { Size = new Size(200, 36), BorderRadius = 4, Font = new Font("Segoe UI", 9F) };
            cbDateRange.Items.Add("Jul 01, 2023 - Sep 30, 2023"); 
            cbDateRange.Items.Add("Past 12 Months");
            cbDateRange.SelectedIndex = 1;
            
            btnExportPDF = new Guna2Button { Text = "Export PDF", Size = new Size(120, 36), BorderRadius = 4, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnExportExcel = new Guna2Button { Text = "Export Excel", Size = new Size(120, 36), BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle, cbDateRange, btnExportPDF, btnExportExcel });
            pnlHeader.Resize += (s, e) => 
            {
                btnExportExcel.Location = new Point(pnlHeader.Width - 120, 10);
                btnExportPDF.Location = new Point(pnlHeader.Width - 250, 10);
                cbDateRange.Location = new Point(pnlHeader.Width - 470, 10);
            };

            // 2. Metric Cards
            pnlMetrics = new Guna2Panel { Dock = DockStyle.Top, Height = 130, Margin = new Padding(0, 0, 0, gutter) };
            cardRevenue = CreateMetricCard("Total Revenue", "payments", "+12.5%");
            cardExpenses = CreateMetricCard("Total Expenses", "receipt_long", "-2.4%");
            cardNetProfit = CreateMetricCard("Net Profit Margin", "donut_large", "+4.1%");
            
            pnlMetrics.Controls.AddRange(new Control[] { cardRevenue, cardExpenses, cardNetProfit });
            pnlMetrics.Resize += (s, e) => 
            {
                int cardWidth = (pnlMetrics.Width - (gutter * 2)) / 3;
                if (cardWidth > 0)
                {
                    cardRevenue.Width = cardWidth; cardRevenue.Left = 0;
                    cardExpenses.Width = cardWidth; cardExpenses.Left = cardWidth + gutter;
                    cardNetProfit.Width = cardWidth; cardNetProfit.Left = (cardWidth + gutter) * 2;
                }
            };

            // 3. Chart
            pnlChart = new Guna2Panel 
            { 
                Dock = DockStyle.Top, Height = 400, 
                BorderRadius = 12,
                BorderThickness = 1,
                Margin = new Padding(0, 0, 0, gutter)
            };
            pnlChart.Paint += PnlChart_Paint;

            // 4. Grid Container
            pnlGridContainer = new Guna2Panel { Dock = DockStyle.Top, Height = 450, BorderRadius = 12, BorderThickness = 1, Margin = new Padding(0, 0, 0, gutter) };
            
            var pnlTableToolbar = new Guna2Panel { Dock = DockStyle.Top, Height = 70, CustomBorderThickness = new Padding(0,0,0,1) };
            Label lblTableTitle = new Label { Text = "Monthly Growth Breakdown", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true, Name = "TableTitle" };
            Label lblTableSub = new Label { Text = "Detailed financial metrics per month.", Font = new Font("Segoe UI", 9F), Location = new Point(20, 40), AutoSize = true, Name = "TableSub" };
            pnlTableToolbar.Controls.AddRange(new Control[] { lblTableTitle, lblTableSub });

            dgvReports = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 50 },
                Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default
            };
            
            dgvReports.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Name = "Id", Visible = false });
            dgvReports.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Month", HeaderText = "MONTH", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvReports.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Revenue", HeaderText = "REVENUE", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "C2" } });
            dgvReports.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Expenses", HeaderText = "EXPENSES", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "C2" } });
            dgvReports.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NetProfit", HeaderText = "NET PROFIT", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "C2" } });
            dgvReports.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", Name = "Status", HeaderText = "STATUS", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            
            DataGridViewTextBoxColumn actionCol = new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "ACTIONS", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
            dgvReports.Columns.Add(actionCol);

            dgvReports.CellPainting += DgvReports_CellPainting;
            dgvReports.CellMouseClick += DgvReports_CellMouseClick;

            pnlGridContainer.Controls.Add(dgvReports);
            pnlGridContainer.Controls.Add(pnlTableToolbar);
            dgvReports.BringToFront();

            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; UpdateGrid(); };
            pnlGridContainer.Controls.Add(paginationControl);

            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(pnlChart);
            pnlContent.Controls.Add(pnlMetrics);
            pnlContent.Controls.Add(pnlHeader);

            this.Controls.Add(pnlContent);

            ApplyTheme();
        }

        private Guna2Panel CreateMetricCard(string title, string iconText, string growthText)
        {
            var card = new Guna2Panel { Height = 130, BorderRadius = 12, BorderThickness = 1 };
            
            var lblTitle = new Label { Name = "TitleLabel", Text = title.ToUpper(), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            var lblVal = new Label { Name = "ValueLabel", Text = "$0.00", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(16, 45) };
            
            var lblIcon = new Label { Name = "IconLabel", Text = iconText, Font = new Font("Segoe UI", 16F), AutoSize = true, Location = new Point(card.Width - 50, 20), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            
            var pnlBadge = new Guna2Panel { Name = "BadgePanel", BorderRadius = 4, AutoSize = true, Location = new Point(20, 95) };
            var lblBadge = new Label { Name = "BadgeLabel", Text = growthText, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Padding = new Padding(4) };
            pnlBadge.Controls.Add(lblBadge);

            var lblCompare = new Label { Name = "CompareLabel", Text = "vs previous quarter", Font = new Font("Segoe UI", 9F), AutoSize = true };
            
            card.Controls.AddRange(new Control[] { lblTitle, lblVal, lblIcon, pnlBadge, lblCompare });

            card.Resize += (s, e) => {
                lblCompare.Location = new Point(pnlBadge.Right + 5, 99);
            };

            return card;
        }

        private void ReportsControl_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            _stats = _service.GetFinancialReports();
            if (_stats == null) return;
            
            cardRevenue.Controls["ValueLabel"].Text = $"${_stats.TotalRevenue:N2}";
            cardExpenses.Controls["ValueLabel"].Text = $"${_stats.OperatingExpenses:N2}";
            cardNetProfit.Controls["ValueLabel"].Text = $"{_stats.AverageMargin:N1}%";

            // Process Monthly Data
            _allMonths.Clear();
            string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            
            int idCounter = 1;
            foreach (var kvp in _stats.MonthlyRevenue)
            {
                int m = kvp.Key;
                decimal rev = kvp.Value;
                decimal exp = _stats.MonthlyExpenses.ContainsKey(m) ? _stats.MonthlyExpenses[m] : 0;
                decimal profit = rev - exp;
                string status = profit >= 100000 ? "Exceeded" : (profit > 0 ? "On Target" : "Below Target");

                _allMonths.Add(new MonthlyPerformance
                {
                    Id = idCounter++,
                    Month = $"{monthNames[m - 1]} 2023",
                    Revenue = rev,
                    Expenses = exp,
                    Status = status
                });
            }

            // sort descending by ID
            _allMonths = _allMonths.OrderByDescending(x => x.Id).ToList();

            UpdateGrid();
            pnlChart.Invalidate();
        }

        private void UpdateGrid()
        {
            int total = _allMonths.Count;
            var paged = _allMonths.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
            dgvReports.DataSource = paged;
            paginationControl.UpdatePagination(total, _currentPage, _pageSize);
        }

        private void PnlChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var b = new SolidBrush(ThemeManager.TextPrimary))
                g.DrawString("Revenue vs Expenses", new Font("Segoe UI", 16F, FontStyle.Bold), b, 20, 20);
            
            using (var b = new SolidBrush(ThemeManager.TextSecondary))
                g.DrawString("Monthly comparison for Q3 2023", new Font("Segoe UI", 10F), b, 20, 50);
            
            using (var b = new SolidBrush(ThemeManager.TextSecondary))
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(27, 58, 87)), pnlChart.Width - 250, 25, 12, 12);
                g.DrawString("Revenue", new Font("Segoe UI", 9F, FontStyle.Bold), b, pnlChart.Width - 230, 22);
                
                g.FillRectangle(new SolidBrush(Color.FromArgb(172, 201, 237)), pnlChart.Width - 150, 25, 12, 12);
                g.DrawString("Expenses", new Font("Segoe UI", 9F, FontStyle.Bold), b, pnlChart.Width - 130, 22);
            }

            if (_stats == null || _stats.MonthlyRevenue.Count == 0) return;

            int paddingX = 80;
            int paddingY = 60;
            int chartWidth = pnlChart.Width - paddingX - 40;
            int chartHeight = pnlChart.Height - paddingY - 90; 
            int barWidth = Math.Min(30, chartWidth / 12 - 10);
            
            decimal maxVal = Math.Max(_stats.MonthlyRevenue.Values.Max(), _stats.MonthlyExpenses.Values.Max());
            if (maxVal == 0) maxVal = 500000;

            string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            
            using (var penGrid = new Pen(Color.FromArgb(50, ThemeManager.TextBoxBorder), 1))
            {
                penGrid.DashStyle = DashStyle.Dash;
                for (int i = 0; i <= 5; i++)
                {
                    int y = pnlChart.Height - paddingY - (i * chartHeight / 5);
                    g.DrawLine(penGrid, paddingX, y, pnlChart.Width - 40, y);
                    
                    using (var b = new SolidBrush(ThemeManager.TextSecondary))
                    {
                        string val = $"${(maxVal * i / 5 / 1000):N0}k";
                        g.DrawString(val, new Font("Segoe UI", 9F), b, 15, y - 8);
                    }
                }
            }

            for (int i = 0; i < 12; i++)
            {
                int x = paddingX + (i * chartWidth / 12) + (chartWidth / 12 / 2);
                
                using (var b = new SolidBrush(ThemeManager.TextSecondary))
                {
                    var size = g.MeasureString(months[i], new Font("Segoe UI", 9F, FontStyle.Bold));
                    g.DrawString(months[i], new Font("Segoe UI", 9F, FontStyle.Bold), b, x - size.Width / 2, pnlChart.Height - paddingY + 10);
                }

                decimal rev = _stats.MonthlyRevenue.ContainsKey(i + 1) ? _stats.MonthlyRevenue[i + 1] : 0;
                int hRev = (int)((rev / maxVal) * chartHeight);
                if (hRev > 0)
                {
                    using (var b = new SolidBrush(Color.FromArgb(27, 58, 87)))
                        g.FillRoundedRectangle(b, x - barWidth - 1, pnlChart.Height - paddingY - hRev, barWidth, hRev, 4);
                }

                decimal exp = _stats.MonthlyExpenses.ContainsKey(i + 1) ? _stats.MonthlyExpenses[i + 1] : 0;
                int hExp = (int)((exp / maxVal) * chartHeight);
                if (hExp > 0)
                {
                    using (var b = new SolidBrush(Color.FromArgb(172, 201, 237)))
                        g.FillRoundedRectangle(b, x + 1, pnlChart.Height - paddingY - hExp, barWidth, hExp, 4);
                }
            }
        }

        private void DgvReports_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvReports.Columns[e.ColumnIndex].Name == "Actions")
            {
                var cellRect = dgvReports.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                
                if (e.X < cellRect.Width / 2)
                {
                    MessageBox.Show("Cannot edit aggregated monthly report data.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Cannot delete aggregated monthly report data.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void DgvReports_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvReports.Columns[e.ColumnIndex].Name == "Actions")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                var rect = e.CellBounds;
                var editRect = new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height);
                var delRect = new Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height);
                
                TextRenderer.DrawText(e.Graphics, "✏️", e.CellStyle.Font, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                TextRenderer.DrawText(e.Graphics, "🗑️", e.CellStyle.Font, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                
                e.Handled = true;
            }
            else if (dgvReports.Columns[e.ColumnIndex].Name == "Status")
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "";

                Color bgColor = ThemeManager.TextBoxBorder;
                Color textColor = ThemeManager.TextPrimary;

                if (status == "Exceeded") { bgColor = Color.FromArgb(40, 46, 204, 113); textColor = Color.FromArgb(46, 204, 113); }
                else if (status == "Below Target") { bgColor = Color.FromArgb(40, 231, 76, 60); textColor = Color.FromArgb(231, 76, 60); }
                else { bgColor = Color.FromArgb(40, 41, 128, 185); textColor = Color.FromArgb(41, 128, 185); }

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                SizeF textSize = g.MeasureString(status, new Font("Segoe UI", 8F, FontStyle.Bold));
                RectangleF badgeRect = new RectangleF(e.CellBounds.X + (e.CellBounds.Width - textSize.Width - 20) / 2, e.CellBounds.Y + (e.CellBounds.Height - textSize.Height - 10) / 2, textSize.Width + 20, textSize.Height + 10);

                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillRoundedRectangle(brush, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height, 10);
                }

                using (var brush = new SolidBrush(textColor))
                {
                    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(status, new Font("Segoe UI", 8F, FontStyle.Bold), brush, badgeRect, format);
                }

                e.Handled = true;
            }
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;

            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;

            btnExportExcel.FillColor = ThemeManager.ButtonFill;
            btnExportExcel.ForeColor = ThemeManager.ButtonText;

            btnExportPDF.FillColor = ThemeManager.CardBackground;
            btnExportPDF.ForeColor = ThemeManager.TextPrimary;
            btnExportPDF.BorderColor = ThemeManager.TextBoxBorder;

            cbDateRange.FillColor = ThemeManager.TextBoxBackground;
            cbDateRange.ForeColor = ThemeManager.TextPrimary;
            cbDateRange.BorderColor = ThemeManager.TextBoxBorder;

            // Metrics Cards
            var cards = new[] { cardRevenue, cardExpenses, cardNetProfit };
            foreach (var card in cards)
            {
                card.FillColor = ThemeManager.CardBackground;
                card.CustomBorderColor = ThemeManager.TextBoxBorder;
                if (card.Controls["TitleLabel"] is Label lTitle) lTitle.ForeColor = ThemeManager.TextSecondary;
                if (card.Controls["ValueLabel"] is Label lVal) lVal.ForeColor = ThemeManager.TextPrimary;
                if (card.Controls["CompareLabel"] is Label lComp) lComp.ForeColor = ThemeManager.TextSecondary;
                
                if (card.Controls["BadgePanel"] is Guna2Panel bPnl)
                {
                    if (bPnl.Controls["BadgeLabel"] is Label bLbl)
                    {
                        if (bLbl.Text.Contains("+"))
                        {
                            bPnl.FillColor = Color.FromArgb(40, 46, 204, 113);
                            bLbl.ForeColor = Color.FromArgb(46, 204, 113);
                        }
                        else
                        {
                            bPnl.FillColor = Color.FromArgb(40, 231, 76, 60);
                            bLbl.ForeColor = Color.FromArgb(231, 76, 60);
                        }
                    }
                }
            }

            pnlChart.FillColor = ThemeManager.CardBackground;
            pnlChart.BorderColor = ThemeManager.TextBoxBorder;

            pnlGridContainer.FillColor = ThemeManager.CardBackground;
            pnlGridContainer.BorderColor = ThemeManager.TextBoxBorder;

            foreach (Control c in pnlGridContainer.Controls)
            {
                if (c is Guna2Panel tb && tb.Height == 70)
                {
                    tb.CustomBorderColor = ThemeManager.TextBoxBorder;
                    if (tb.Controls["TableTitle"] is Label l1) l1.ForeColor = ThemeManager.TextPrimary;
                    if (tb.Controls["TableSub"] is Label l2) l2.ForeColor = ThemeManager.TextSecondary;
                }
            }

            dgvReports.BackgroundColor = ThemeManager.CardBackground;
            dgvReports.GridColor = ThemeManager.TextBoxBorder;
            dgvReports.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvReports.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvReports.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvReports.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;

            dgvReports.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvReports.AlternatingRowsDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvReports.AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvReports.AlternatingRowsDefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;

            dgvReports.EnableHeadersVisualStyles = false;
            dgvReports.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvReports.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvReports.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }
}
