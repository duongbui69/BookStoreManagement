using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.UserControls
{
    public partial class ReportsControl : UserControl
    {
        private ReportRepository _repo;
        private ReportStats _stats;
        
        private Panel pnlHeader;
        private Label lblTitle;
        private TextBox txtSearch;
        private ComboBox cbDateRange;
        private ComboBox cbCategory;
        private Button btnExportPDF;
        private Button btnExportExcel;

        private FlowLayoutPanel flpCards;
        private Panel pnlChart;
        private Panel pnlBottom;
        private Panel pnlDeptPerf;
        private Panel pnlInsights;

        private DataGridView dgvDept;

        public ReportsControl()
        {
            _repo = new ReportRepository();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += ReportsControl_Load;
            this.Resize += ReportsControl_Resize;
        }

        private void InitializeUI()
        {
            this.BackColor = ThemeManager.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(30);

            // Header
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.Transparent };
            lblTitle = new Label { Text = "Financial Reports", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            txtSearch = new TextBox { Width = 300, Font = new Font("Segoe UI", 12F), PlaceholderText = "Search reports...", Location = new Point(250, 0) };
            
            cbDateRange = new ComboBox { Width = 220, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(0, 50) };
            cbDateRange.Items.Add("Jan 1, 2024 - Dec 31, 2024"); cbDateRange.SelectedIndex = 0;
            
            cbCategory = new ComboBox { Width = 180, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(240, 50) };
            cbCategory.Items.Add("All Categories"); cbCategory.SelectedIndex = 0;

            btnExportPDF = new Button { Text = "Export PDF", Width = 110, Height = 35, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnExportExcel = new Button { Text = "Export Excel", Width = 120, Height = 35, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(txtSearch);
            pnlHeader.Controls.Add(cbDateRange);
            pnlHeader.Controls.Add(cbCategory);
            pnlHeader.Controls.Add(btnExportPDF);
            pnlHeader.Controls.Add(btnExportExcel);
            this.Controls.Add(pnlHeader);

            // Cards
            flpCards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 130, Margin = new Padding(0, 20, 0, 20), WrapContents = false, BackColor = Color.Transparent };
            this.Controls.Add(flpCards);

            // Chart
            pnlChart = new Panel { Dock = DockStyle.Top, Height = 350, BackColor = ThemeManager.CardBackground, Margin = new Padding(0, 20, 0, 20) };
            pnlChart.Paint += PnlChart_Paint;
            this.Controls.Add(pnlChart);

            // Bottom Area
            pnlBottom = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 20, 0, 0) };
            
            // Insights (Right)
            pnlInsights = new Panel { Dock = DockStyle.Right, Width = 350, BackColor = ThemeManager.CardBackground };
            pnlInsights.Paint += PnlInsights_Paint;
            
            // Dept Performance (Left)
            pnlDeptPerf = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.CardBackground, Padding = new Padding(20) };
            Label lblDeptTitle = new Label { Text = "Departmental Performance", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 15) };
            Label lblViewAll = new Label { Text = "View All", Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, ForeColor = ThemeManager.TextSecondary, Cursor = Cursors.Hand };
            pnlDeptPerf.Controls.Add(lblDeptTitle);
            pnlDeptPerf.Controls.Add(lblViewAll);
            pnlDeptPerf.Resize += (s, e) => { lblViewAll.Location = new Point(pnlDeptPerf.Width - 80, 20); };

            dgvDept = new DataGridView
            {
                Location = new Point(20, 60),
                Width = pnlDeptPerf.Width - 40,
                Height = pnlDeptPerf.Height - 80,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 50 }
            };
            dgvDept.CellFormatting += DgvDept_CellFormatting;
            pnlDeptPerf.Controls.Add(dgvDept);

            pnlBottom.Controls.Add(pnlDeptPerf);
            // Add a splitter panel for margin
            Panel pnlMargin = new Panel { Dock = DockStyle.Right, Width = 20, BackColor = Color.Transparent };
            pnlBottom.Controls.Add(pnlMargin);
            pnlBottom.Controls.Add(pnlInsights);

            this.Controls.Add(pnlBottom);

            // Re-order
            pnlBottom.BringToFront();
            pnlChart.BringToFront();
            flpCards.BringToFront();
            pnlHeader.BringToFront();

            ApplyTheme();
        }

        private void ReportsControl_Resize(object sender, EventArgs e)
        {
            if (pnlHeader != null)
            {
                btnExportExcel.Location = new Point(pnlHeader.Width - 120, 50);
                btnExportPDF.Location = new Point(pnlHeader.Width - 250, 50);
            }
        }

        private void ReportsControl_Load(object sender, EventArgs e)
        {
            _stats = _repo.GetFinancialReports();
            LoadData();
        }

        private void LoadData()
        {
            if (_stats == null) return;
            
            flpCards.Controls.Clear();
            int cardWidth = Math.Max(200, (this.Width - 140) / 4);

            flpCards.Controls.Add(CreateStatCard("TOTAL REVENUE", $"${_stats.TotalRevenue:N2}", _stats.RevenueGrowth, Color.FromArgb(46, 204, 113), cardWidth));
            flpCards.Controls.Add(CreateStatCard("OPERATING EXPENSES", $"${_stats.OperatingExpenses:N2}", _stats.ExpensesGrowth, Color.FromArgb(231, 76, 60), cardWidth));
            flpCards.Controls.Add(CreateStatCard("NET PROFIT", $"${_stats.NetProfit:N2}", _stats.ProfitGrowth, Color.FromArgb(128, 90, 213), cardWidth));
            
            string marginText = _stats.MarginGrowth == 0 ? "Stable vs last period" : $"{Math.Abs(_stats.MarginGrowth):N1}% vs last period";
            flpCards.Controls.Add(CreateStatCard("AVERAGE MARGIN", $"{_stats.AverageMargin:N1}%", _stats.MarginGrowth, Color.FromArgb(41, 128, 185), cardWidth, true));

            dgvDept.DataSource = _stats.DepartmentPerformances;
            if (dgvDept.Columns["GrossSales"] != null) dgvDept.Columns["GrossSales"].DefaultCellStyle.Format = "C0";
            if (dgvDept.Columns["COGS"] != null) dgvDept.Columns["COGS"].DefaultCellStyle.Format = "C0";
            if (dgvDept.Columns["Margin"] != null) dgvDept.Columns["Margin"].DefaultCellStyle.Format = "N1";

            pnlChart.Invalidate();
            pnlInsights.Invalidate();
        }

        private Panel CreateStatCard(string title, string value, decimal growth, Color iconColor, int width, bool isMargin = false)
        {
            var pnl = new Panel { Width = width, Height = 110, BackColor = ThemeManager.CardBackground, Margin = new Padding(0, 0, 20, 0) };
            pnl.Paint += (s, e) => 
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var p = new Pen(ThemeManager.TextBoxBorder, 1);
                e.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
                
                // Draw Icon Box
                using var b = new SolidBrush(Color.FromArgb(30, iconColor));
                e.Graphics.FillRoundedRectangle(b, pnl.Width - 50, 20, 35, 35, 5);
            };
            
            pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = ThemeManager.TextSecondary, Location = new Point(20, 20), AutoSize = true });
            pnl.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = ThemeManager.TextPrimary, Location = new Point(15, 40), AutoSize = true });
            
            string growthText = "";
            Color growthColor = ThemeManager.TextSecondary;
            if (isMargin && growth == 0)
            {
                growthText = "Stable vs last period";
            }
            else
            {
                growthText = growth > 0 ? $"↑ {Math.Abs(growth):N1}% vs last period" : (growth < 0 ? $"↓ {Math.Abs(growth):N1}% vs last period" : "Stable vs last period");
                growthColor = growth > 0 ? Color.FromArgb(46, 204, 113) : (growth < 0 ? Color.FromArgb(231, 76, 60) : ThemeManager.TextSecondary);
                if (title.Contains("EXPENSES")) growthColor = growth > 0 ? Color.FromArgb(231, 76, 60) : Color.FromArgb(46, 204, 113); // expenses up = bad
            }

            pnl.Controls.Add(new Label { Text = growthText, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = growthColor, Location = new Point(20, 80), AutoSize = true });
            return pnl;
        }

        private void PnlChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            using var p = new Pen(ThemeManager.TextBoxBorder, 1);
            g.DrawRectangle(p, 0, 0, pnlChart.Width - 1, pnlChart.Height - 1);

            // Title and Legend
            using (var b = new SolidBrush(ThemeManager.TextPrimary))
                g.DrawString("Monthly Financials (Revenue vs Expenses)", new Font("Segoe UI", 12F, FontStyle.Bold), b, 20, 20);
            
            using (var b = new SolidBrush(ThemeManager.TextSecondary))
            {
                g.FillEllipse(new SolidBrush(ThemeManager.Sidebar), pnlChart.Width - 250, 25, 10, 10);
                g.DrawString("Revenue", new Font("Segoe UI", 9F, FontStyle.Bold), b, pnlChart.Width - 235, 20);
                
                g.FillEllipse(new SolidBrush(Color.FromArgb(128, 90, 213)), pnlChart.Width - 150, 25, 10, 10);
                g.DrawString("Expenses", new Font("Segoe UI", 9F, FontStyle.Bold), b, pnlChart.Width - 135, 20);
            }

            if (_stats == null || _stats.MonthlyRevenue.Count == 0) return;

            // Draw Grid & Bars
            int padding = 40;
            int chartWidth = pnlChart.Width - padding * 2;
            int chartHeight = pnlChart.Height - padding * 2 - 40; // 40 for title
            int barWidth = Math.Min(30, chartWidth / 24 - 5);
            
            decimal maxVal = Math.Max(_stats.MonthlyRevenue.Values.Max(), _stats.MonthlyExpenses.Values.Max());
            if (maxVal == 0) maxVal = 1000;

            string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            
            for (int i = 0; i < 12; i++)
            {
                int x = padding + (i * chartWidth / 12) + (chartWidth / 12 / 2);
                
                // Draw Grid line
                using (var penGrid = new Pen(ThemeManager.TextBoxBorder, 1))
                    g.DrawLine(penGrid, x, 60, x, pnlChart.Height - padding);

                // Month Label
                using (var b = new SolidBrush(ThemeManager.TextSecondary))
                    g.DrawString(months[i], new Font("Segoe UI", 8F, FontStyle.Bold), b, x - 10, pnlChart.Height - padding + 10);

                // Draw Revenue Bar
                decimal rev = _stats.MonthlyRevenue.ContainsKey(i + 1) ? _stats.MonthlyRevenue[i + 1] : 0;
                int hRev = (int)((rev / maxVal) * chartHeight);
                if (hRev > 0)
                {
                    using (var b = new SolidBrush(ThemeManager.Sidebar))
                        g.FillRectangle(b, x - barWidth - 2, pnlChart.Height - padding - hRev, barWidth, hRev);
                }

                // Draw Expense Bar
                decimal exp = _stats.MonthlyExpenses.ContainsKey(i + 1) ? _stats.MonthlyExpenses[i + 1] : 0;
                int hExp = (int)((exp / maxVal) * chartHeight);
                if (hExp > 0)
                {
                    using (var b = new SolidBrush(Color.FromArgb(128, 90, 213)))
                        g.FillRectangle(b, x + 2, pnlChart.Height - padding - hExp, barWidth, hExp);
                }
            }
        }

        private void PnlInsights_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var p = new Pen(ThemeManager.TextBoxBorder, 1);
            g.DrawRectangle(p, 0, 0, pnlInsights.Width - 1, pnlInsights.Height - 1);

            using (var b = new SolidBrush(ThemeManager.TextPrimary))
                g.DrawString("Financial Insights", new Font("Segoe UI", 12F, FontStyle.Bold), b, 20, 20);

            // Insight Card 1
            RectangleF card1Rect = new RectangleF(20, 60, pnlInsights.Width - 40, 90);
            using (var b = new SolidBrush(Color.FromArgb(20, 46, 204, 113)))
                g.FillRoundedRectangle(b, card1Rect.X, card1Rect.Y, card1Rect.Width, card1Rect.Height, 8);
            using (var pen = new Pen(Color.FromArgb(80, 46, 204, 113), 1))
                g.DrawRoundedRectangle(pen, card1Rect.X, card1Rect.Y, card1Rect.Width, card1Rect.Height, 8);

            using (var b = new SolidBrush(Color.FromArgb(46, 204, 113)))
                g.DrawString("Optimization Opportunity", new Font("Segoe UI", 9F, FontStyle.Bold), b, 45, 70);
            
            using (var b = new SolidBrush(ThemeManager.TextSecondary))
                g.DrawString("Fiction margins are 5% above\nindustry average. Consider\nincreasing inventory depth for\ntop 10 bestsellers.", new Font("Segoe UI", 8.5F), b, 45, 90);
        }

        private void DgvDept_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvDept.Columns[e.ColumnIndex].Name == "Status")
            {
                string status = e.Value?.ToString() ?? "";
                if (status == "OPTIMAL") e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                else if (status == "NEEDS REVIEW") e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                else e.CellStyle.ForeColor = Color.FromArgb(128, 90, 213);
                e.CellStyle.Font = new Font(dgvDept.Font, FontStyle.Bold);
            }
            if (dgvDept.Columns[e.ColumnIndex].Name == "Margin")
            {
                e.Value = e.Value?.ToString() + "%";
                e.FormattingApplied = true;
            }
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
            LoadData(); 
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            
            btnExportPDF.BackColor = ThemeManager.CardBackground;
            btnExportPDF.ForeColor = ThemeManager.TextPrimary;
            btnExportPDF.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            btnExportExcel.BackColor = ThemeManager.Sidebar;
            btnExportExcel.ForeColor = Color.White;
            btnExportExcel.FlatAppearance.BorderSize = 0;

            txtSearch.BackColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;

            cbDateRange.BackColor = ThemeManager.TextBoxBackground;
            cbDateRange.ForeColor = ThemeManager.TextPrimary;
            cbCategory.BackColor = ThemeManager.TextBoxBackground;
            cbCategory.ForeColor = ThemeManager.TextPrimary;

            pnlChart.BackColor = ThemeManager.CardBackground;
            pnlDeptPerf.BackColor = ThemeManager.CardBackground;
            pnlInsights.BackColor = ThemeManager.Background; // Wait, insight panel should have background to match image? Actually card background is fine.

            dgvDept.BackgroundColor = ThemeManager.CardBackground;
            dgvDept.GridColor = ThemeManager.TextBoxBorder;
            dgvDept.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvDept.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvDept.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvDept.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvDept.EnableHeadersVisualStyles = false;
            dgvDept.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvDept.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvDept.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }
}
