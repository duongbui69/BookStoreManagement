using BookStoreManagement.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BookStoreManagement.Events;
using System.Linq;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using BookStoreManagement.Services;
using BookStoreManagement.Interfaces;
using Guna.UI2.WinForms;

namespace BookStoreManagement.UserControls
{
    public partial class ReportsControl : UserControl, ISearchableControl
    {
        private readonly ReportService _service;
        private ReportStats _stats;
        
        // Content Container
        private Guna2Panel pnlContent;

        // Header & Actions
        private Guna2Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Guna2Button btnExportExcel;

        // KPI Cards
        private TableLayoutPanel pnlMetrics;
        private Guna2Panel cardAllTimeRevenue;
        private Guna2Panel cardRevenue;
        private Guna2Panel cardOrders;
        private Guna2Panel cardLowStock;

        // Chart
        private Guna2Panel pnlChart;

        // Bottom Tables
        private Guna2Panel pnlTables;
        
        private Guna2Panel pnlTopSelling;
        private DataGridView dgvTopSelling;
        
        private Guna2Panel pnlWarnings;
        private DataGridView dgvWarnings;

        // Monthly Revenue Table
        private Guna2Panel pnlRevenueTable;
        private DataGridView dgvRevenue;
        private Guna2DateTimePicker dtpFromDate;
        private Guna2DateTimePicker dtpToDate;
        private Guna2CheckBox chkByDay;
        private Guna2Button btnFilterRevenue;
        private Guna2Button btnClearFilterRevenue;

        public void PerformSearch(string keyword)
        {
            // Optional: search logic
        }

        public ReportsControl()
        {
            _service = new ReportService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += async (s, e) => { await LoadDataAsync(); };
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 30; // Increased spacing
            this.Padding = new Padding(0);
            this.AutoScroll = true;

            pnlContent = new Guna2Panel { Dock = DockStyle.Top, Padding = new Padding(gutter), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };

            // 1. Header
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(0, 0, 0, gutter + 10) };
            
            lblTitle = new Label { Text = "Thống kê chung", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubtitle = new Label { Text = "Tổng quan doanh thu, đơn hàng và cảnh báo kho.", Font = new Font("Segoe UI", 11F), AutoSize = true, Location = new Point(0, 45) };
            
            btnExportExcel = new Guna2Button { 
                Text = "Xuất Excel", 
                Size = new Size(130, 36), 
                BorderRadius = 4, 
                BorderThickness = 1, 
                FillColor = Color.Transparent, 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), 
                Cursor = Cursors.Hand 
            };
            btnExportExcel.Click += BtnExport_Click;

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, btnExportExcel });
            pnlHeader.Resize += (s, e) => { btnExportExcel.Location = new Point(pnlHeader.Width - 130, 22); };

            // 2. Metric Cards
            pnlMetrics = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, Height = 100, Margin = new Padding(0, 0, 0, gutter + 20),
                ColumnCount = 4, RowCount = 1,
                BackColor = Color.Transparent
            };
            for(int i=0; i<4; i++) pnlMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            cardAllTimeRevenue = CreateMetricCard("Tổng DT (Tất Cả)", "0 đ", "", true);
            cardRevenue = CreateMetricCard("DT Tháng Này", "0 đ", "", true);
            cardOrders = CreateMetricCard("Đơn Tháng Này", "0", "", true);
            cardLowStock = CreateMetricCard("Sắp hết hàng", "0", "", false, true);
            
            cardAllTimeRevenue.Margin = new Padding(0, 0, 10, 0);
            cardRevenue.Margin = new Padding(10, 0, 10, 0);
            cardOrders.Margin = new Padding(10, 0, 10, 0);
            cardLowStock.Margin = new Padding(10, 0, 0, 0);

            cardAllTimeRevenue.Dock = DockStyle.Fill;
            cardRevenue.Dock = DockStyle.Fill;
            cardOrders.Dock = DockStyle.Fill;
            cardLowStock.Dock = DockStyle.Fill;

            pnlMetrics.Controls.Add(cardAllTimeRevenue, 0, 0);
            pnlMetrics.Controls.Add(cardRevenue, 1, 0);
            pnlMetrics.Controls.Add(cardOrders, 2, 0);
            pnlMetrics.Controls.Add(cardLowStock, 3, 0);

            // 3. Chart
            pnlChart = new Guna2Panel 
            { 
                Dock = DockStyle.Top, Height = 350, 
                BorderRadius = 4,
                BorderThickness = 1,
                Margin = new Padding(0, 0, 0, gutter + 20)
            };
            var lblChartTitle = new Label { Text = "Biểu đồ doanh thu 12 tháng qua", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20), Name = "ChartTitle" };
            pnlChart.Controls.Add(lblChartTitle);
            pnlChart.Paint += PnlChart_Paint;

            // 4. Tables Container
            pnlTables = new Guna2Panel { Dock = DockStyle.Top, Height = 400, Margin = new Padding(0, 0, 0, gutter) };
            
            pnlTopSelling = CreateTableContainer("Top 5 Sách bán chạy", out dgvTopSelling);
            pnlWarnings = CreateTableContainer("Cảnh báo tồn kho", out dgvWarnings);

            // Configure Top Selling Grid
            dgvTopSelling.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rank", DataPropertyName = "Rank", HeaderText = "Hạng", Width = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvTopSelling.Columns.Add(new DataGridViewTextBoxColumn { Name = "Title", DataPropertyName = "Title", HeaderText = "Tên sách", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvTopSelling.Columns.Add(new DataGridViewTextBoxColumn { Name = "QuantitySold", DataPropertyName = "QuantitySold", HeaderText = "SL Bán", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 11, FontStyle.Bold) } });
            dgvTopSelling.Columns.Add(new DataGridViewTextBoxColumn { Name = "Revenue", DataPropertyName = "Revenue", HeaderText = "Doanh thu", Width = 150, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N0" } });
            
            // Configure Warnings Grid
            dgvWarnings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sku", DataPropertyName = "Sku", HeaderText = "ISBN", Width = 120 });
            dgvWarnings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Title", DataPropertyName = "Title", HeaderText = "Tên sách", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvWarnings.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentStock", DataPropertyName = "CurrentStock", HeaderText = "Tồn kho hiện tại", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 11, FontStyle.Bold) } });
            dgvWarnings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Trạng thái", DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });

            dgvWarnings.CellPainting += DgvWarnings_CellPainting;

            pnlTables.Controls.Add(pnlTopSelling);
            pnlTables.Controls.Add(pnlWarnings);

            pnlTables.Resize += (s, e) => 
            {
                int halfWidth = (pnlTables.Width - gutter) / 2;
                if (halfWidth > 0)
                {
                    pnlTopSelling.Width = halfWidth; pnlTopSelling.Left = 0;
                    pnlWarnings.Width = halfWidth; pnlWarnings.Left = halfWidth + gutter;
                }
            };

            // 5. Revenue Table with Date Filter
            pnlRevenueTable = new Guna2Panel { Dock = DockStyle.Top, Height = 400, BorderRadius = 4, BorderThickness = 1, Margin = new Padding(0, 0, 0, gutter + 20) };
            
            var pnlRevHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, CustomBorderThickness = new Padding(0,0,0,1) };
            var lblRevTitle = new Label { Name = "TableTitle", Text = "Bảng thống kê doanh thu", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 30) };
            pnlRevHeader.Controls.Add(lblRevTitle);

            var lblFrom = new Label { Text = "Từ ngày:", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(250, 32) };
            dtpFromDate = new Guna2DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(320, 25), Width = 130, Height = 36, BorderRadius = 4, FillColor = Color.White, BorderColor = Color.LightGray, BorderThickness = 1 };
            
            var lblTo = new Label { Text = "Đến ngày:", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(470, 32) };
            dtpToDate = new Guna2DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(540, 25), Width = 130, Height = 36, BorderRadius = 4, FillColor = Color.White, BorderColor = Color.LightGray, BorderThickness = 1 };

            chkByDay = new Guna2CheckBox { Text = "Xem theo ngày", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(690, 32), Cursor = Cursors.Hand };

            btnFilterRevenue = new Guna2Button { Text = "Lọc", Size = new Size(80, 36), Location = new Point(820, 25), BorderRadius = 4, Cursor = Cursors.Hand };
            btnClearFilterRevenue = new Guna2Button { Text = "Bỏ lọc", Size = new Size(80, 36), Location = new Point(910, 25), BorderRadius = 4, FillColor = Color.Gray, Cursor = Cursors.Hand };

            btnFilterRevenue.Click += async (s, e) => await LoadRevenueTableAsync(false);
            btnClearFilterRevenue.Click += async (s, e) => { chkByDay.Checked = false; await LoadRevenueTableAsync(true); };

            pnlRevHeader.Controls.AddRange(new Control[] { lblFrom, dtpFromDate, lblTo, dtpToDate, chkByDay, btnFilterRevenue, btnClearFilterRevenue });

            dgvRevenue = new DataGridView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 45 },
                EnableHeadersVisualStyles = false,
                ScrollBars = ScrollBars.Vertical,
                AutoGenerateColumns = false
            };
            dgvRevenue.SetDoubleBuffered(true);

            dgvRevenue.Columns.Add(new DataGridViewTextBoxColumn { Name = "Period", DataPropertyName = "Period", HeaderText = "Thời gian", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvRevenue.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalOrders", DataPropertyName = "TotalOrders", HeaderText = "Tổng đơn hàng", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 11, FontStyle.Bold) } });
            dgvRevenue.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalRevenue", DataPropertyName = "TotalRevenue", HeaderText = "Tổng doanh thu", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N0 đ", Font = new Font("Segoe UI", 11, FontStyle.Bold) } });

            pnlRevenueTable.Controls.Add(dgvRevenue);
            pnlRevenueTable.Controls.Add(pnlRevHeader);
            dgvRevenue.BringToFront();

            pnlContent.Controls.Add(pnlTables);
            pnlContent.Controls.Add(pnlRevenueTable);
            pnlContent.Controls.Add(pnlChart);
            pnlContent.Controls.Add(pnlMetrics);
            pnlContent.Controls.Add(pnlHeader);

            this.Controls.Add(pnlContent);
            ApplyTheme();
        }

        private Guna2Panel CreateTableContainer(string title, out DataGridView grid)
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Left, BorderRadius = 4, BorderThickness = 1 };
            
            var pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, CustomBorderThickness = new Padding(0,0,0,1) };
            var lblTitle = new Label { Name = "TableTitle", Text = title, Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            pnlHeader.Controls.Add(lblTitle);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowTemplate = { Height = 45 },
                EnableHeadersVisualStyles = false,
                ScrollBars = ScrollBars.Vertical,
                AutoGenerateColumns = false
            };
            grid.SetDoubleBuffered(true);
            
            pnl.Controls.Add(grid);
            pnl.Controls.Add(pnlHeader);
            grid.BringToFront();
            return pnl;
        }

        private Guna2Panel CreateMetricCard(string title, string val, string subText, bool isPositive, bool isError = false)
        {
            Color accent = isError ? Color.FromArgb(231, 76, 60) : (isPositive ? Color.FromArgb(46, 204, 113) : Color.FromArgb(52, 152, 219));
            
            var card = new Guna2Panel { Height = 100, BorderRadius = 10, BorderThickness = 1, BackColor = Color.Transparent };
            
            var iconPanel = new Guna2Panel { Size = new Size(48, 48), Location = new Point(16, 17), BorderRadius = 8, FillColor = Color.FromArgb(20, accent), Name = "IconPanel" };
            string iconStr = isError ? "!" : (isPositive ? "▲" : "▼");
            if (string.IsNullOrEmpty(subText) && !isError) iconStr = "●";
            var iconLabel = new Label { Text = iconStr, Font = new Font("Segoe UI", 16F), ForeColor = accent, AutoSize = true, Location = new Point(12, 8), BackColor = Color.Transparent, Name = "IconLabel" };
            iconPanel.Controls.Add(iconLabel);
            
            var lblTitle = new Label { Name = "TitleLabel", Text = title, Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Location = new Point(76, 17), BackColor = Color.Transparent };
            var lblVal = new Label { Name = "ValueLabel", Text = val, Font = new Font("Segoe UI", 18F, FontStyle.Bold), AutoSize = true, Location = new Point(74, 38), BackColor = Color.Transparent };
            
            var pnlBadge = new Guna2Panel { Name = "BadgePanel", BorderRadius = 4, AutoSize = true, Location = new Point(76, 70), BackColor = Color.Transparent, Visible = false };
            var lblBadge = new Label { Name = "BadgeLabel", Text = subText, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Padding = new Padding(2), BackColor = Color.Transparent, Visible = false };
            pnlBadge.Controls.Add(lblBadge);

            // Store info in Tag
            card.Tag = new Tuple<bool, bool>(isPositive, isError);

            card.Controls.AddRange(new Control[] { iconPanel, lblTitle, lblVal, pnlBadge });
            return card;
        }

        private void UpdateMetricCard(Guna2Panel card, string valueText, string badgeText, bool isPositive, bool isError = false)
        {
            if (card == null) return;
            var valLbl = card.Controls.Find("ValueLabel", true).FirstOrDefault() as Label;
            if (valLbl != null) valLbl.Text = valueText;
            
            var badgeLbl = card.Controls.Find("BadgeLabel", true).FirstOrDefault() as Label;
            if (badgeLbl != null && badgeText != null) badgeLbl.Text = badgeText;
            
            card.Tag = new Tuple<bool, bool>(isPositive, isError);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            _stats = await _service.GetFinancialReportsAsync();
            if (this.IsDisposed) return;
            if (_stats == null) return;
            

            UpdateMetricCard(cardAllTimeRevenue, 
                $"{_stats.AllTimeRevenue:N0} đ", 
                "", 
                true);

            UpdateMetricCard(cardRevenue, 
                $"{_stats.TotalRevenue:N0} đ", 
                "", 
                _stats.RevenueGrowth >= 0);

            UpdateMetricCard(cardOrders, 
                $"{_stats.TotalOrders:N0}", 
                "", 
                _stats.OrdersGrowth >= 0);

            UpdateMetricCard(cardLowStock, 
                $"{_stats.LowStockCount}", 
                "", 
                false, true);
            
            UpdateGrids();
            ApplyTheme(); // re-apply colors based on updated tags
            pnlChart.Invalidate();
            
            await LoadRevenueTableAsync(true);
        }

        private async System.Threading.Tasks.Task LoadRevenueTableAsync(bool reset = false)
        {
            if (reset)
            {
                dtpFromDate.Value = new DateTime(DateTime.Now.Year, 1, 1);
                dtpToDate.Value = DateTime.Now;
                chkByDay.Checked = false;
            }

            DateTime? from = reset ? null : (DateTime?)dtpFromDate.Value;
            DateTime? to = reset ? null : (DateTime?)dtpToDate.Value;

            var data = await _service.GetRevenueTableAsync(from, to, chkByDay.Checked);
            dgvRevenue.Rows.Clear();
            if (data != null)
            {
                foreach (var item in data)
                {
                    dgvRevenue.Rows.Add(item.Period, item.TotalOrders, item.TotalRevenue);
                }
            }
        }

        private void UpdateGrids()
        {
            if (this.IsDisposed) return;
            try
            {
                if (dgvTopSelling.Columns.Count == 0) return;
                
                dgvTopSelling.Rows.Clear();
                foreach (var item in _stats.TopSellingBooks)
                {
                    dgvTopSelling.Rows.Add(item.Rank, item.Title, item.QuantitySold, $"{item.Revenue:N0} đ");
                }

                dgvWarnings.Rows.Clear();
                foreach (var item in _stats.InventoryWarnings)
                {
                    dgvWarnings.Rows.Add(item.Sku, item.Title, item.CurrentStock, item.Status);
                }
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error(ex, $"UpdateGrids failed. dgvTopSelling columns: {dgvTopSelling?.Columns.Count}, dgvWarnings columns: {dgvWarnings?.Columns.Count}");
                throw; // rethrow to keep original behavior or we can swallow it
            }
        }

        private void DgvWarnings_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dgvWarnings.Columns[e.ColumnIndex].Name == "Trạng thái")
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "";

                Color bgColor = ThemeManager.TextBoxBorder;
                Color textColor = ThemeManager.TextPrimary;

                if (status == "URGENT" || status == "OUT OF STOCK") 
                { 
                    bgColor = Color.FromArgb(40, 186, 26, 26); // error-container
                    textColor = Color.FromArgb(186, 26, 26); // error
                }
                else if (status == "WARNING") 
                { 
                    bgColor = Color.FromArgb(40, 230, 126, 34); 
                    textColor = Color.FromArgb(230, 126, 34); 
                }

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var statusFont = new Font("Segoe UI", 10F, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString(status, statusFont);
                    RectangleF badgeRect = new RectangleF(
                        e.CellBounds.X + (e.CellBounds.Width - textSize.Width - 16) / 2, 
                        e.CellBounds.Y + (e.CellBounds.Height - textSize.Height - 6) / 2, 
                        textSize.Width + 16, 
                        textSize.Height + 6);

                    using (var brush = new SolidBrush(bgColor))
                    {
                        g.FillRoundedRectangle(brush, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height, 4);
                    }

                    using (var brush = new SolidBrush(textColor))
                    {
                        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString(status, statusFont, brush, badgeRect, format);
                    }
                }

                e.Handled = true;
            }
        }

        private void PnlChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_stats == null || _stats.MonthlyRevenue.Count == 0) return;

            int paddingX = 60;
            int paddingY = 40;
            int chartWidth = pnlChart.Width - paddingX - 40;
            int chartHeight = pnlChart.Height - paddingY - 80;
            
            decimal maxVal = _stats.MonthlyRevenue.Values.Count > 0 ? _stats.MonthlyRevenue.Values.Max() : 0;
            if (maxVal == 0) maxVal = 100000;
            // Round up to nearest nice number
            decimal steps = maxVal / 5;
            
            using (var font1 = new Font("Segoe UI", 9F))
            using (var font2 = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                using (var penGrid = new Pen(Color.FromArgb(80, ThemeManager.TextBoxBorder), 1))
                {
                    penGrid.DashStyle = DashStyle.Dash;
                    for (int i = 0; i <= 5; i++)
                    {
                        int y = pnlChart.Height - paddingY - (i * chartHeight / 5);
                        g.DrawLine(penGrid, paddingX, y, pnlChart.Width - 40, y);

                        using (var b = new SolidBrush(ThemeManager.TextSecondary))
                        {
                            decimal val = maxVal * i / 5;
                            string valStr = val >= 1000000 ? $"{(val/1000000):N1}M" : $"{(val/1000):N0}k";
                            g.DrawString(valStr, font1, b, 10, y - 8);
                        }
                    }
                }

                int numPoints = _stats.MonthlyRevenue.Count;
                if (numPoints > 0)
                {
                    float stepX = (float)chartWidth / numPoints;
                    float barWidth = stepX * 0.6f;
                    int idx = 0;

                    foreach (var kvp in _stats.MonthlyRevenue.OrderBy(k => k.Key))
                    {
                        DateTime m = kvp.Key;
                        decimal rev = kvp.Value;
                        
                        float xCenter = paddingX + (idx * stepX) + (stepX / 2);
                        float barHeight = (float)(rev / maxVal) * chartHeight;
                        float x = xCenter - (barWidth / 2);
                        float y = pnlChart.Height - paddingY - barHeight;
                        
                        // Draw Bar
                        if (barHeight > 0)
                        {
                            RectangleF barRect = new RectangleF(x, y, barWidth, barHeight);
                            using (var brush = new LinearGradientBrush(barRect, ThemeManager.ButtonFill, Color.FromArgb(141, 188, 215), LinearGradientMode.Vertical))
                            {
                                g.FillRectangle(brush, barRect);
                            }
                        }

                        // Draw Label
                        using (var b = new SolidBrush(ThemeManager.TextSecondary))
                        {
                            string lbl = m.ToString("MM/yy");
                            var size = g.MeasureString(lbl, font2);
                            g.DrawString(lbl, font2, b, xCenter - size.Width / 2, pnlChart.Height - paddingY + 10);
                        }
                        idx++;
                    }
                }
            }
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            BookStoreManagement.Themes.ThemeManager.ApplyTypography(this);
            this.BackColor = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;
            
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubtitle.ForeColor = ThemeManager.TextSecondary;

            btnExportExcel.FillColor = ThemeManager.ButtonFill;
            btnExportExcel.ForeColor = ThemeManager.ButtonText;

            // Metrics Cards
            var cards = new[] { cardAllTimeRevenue, cardRevenue, cardOrders, cardLowStock };
            foreach (var card in cards)
            {
                if (card == null) continue;
                
                card.FillColor = ThemeManager.CardBackground;
                card.BorderColor = ThemeManager.TextBoxBorder;
                
                var lTitle = card.Controls.Find("TitleLabel", true).FirstOrDefault() as Label;
                if (lTitle != null) lTitle.ForeColor = ThemeManager.TextSecondary;
                
                var tag = card.Tag as Tuple<bool, bool>;
                bool isPositive = tag?.Item1 ?? true;
                bool isError = tag?.Item2 ?? false;

                var lVal = card.Controls.Find("ValueLabel", true).FirstOrDefault() as Label;
                if (lVal != null) 
                {
                    lVal.ForeColor = ThemeManager.TextPrimary;
                }
                
                var bPnl = card.Controls.Find("BadgePanel", true).FirstOrDefault() as Guna2Panel;
                if (bPnl != null)
                {
                    var bLbl = bPnl.Controls.Find("BadgeLabel", true).FirstOrDefault() as Label;
                    if (bLbl != null)
                    {
                        if (isError)
                        {
                            bPnl.FillColor = Color.FromArgb(40, 186, 26, 26);
                            bLbl.ForeColor = Color.FromArgb(186, 26, 26);
                        }
                        else if (isPositive)
                        {
                            bPnl.FillColor = Color.FromArgb(40, 0, 186, 97); // success
                            bLbl.ForeColor = Color.FromArgb(0, 186, 97);
                        }
                        else
                        {
                            bPnl.FillColor = Color.FromArgb(40, 186, 26, 26); // negative
                            bLbl.ForeColor = Color.FromArgb(186, 26, 26);
                        }
                    }
                }
            }

            pnlChart.FillColor = ThemeManager.CardBackground;
            pnlChart.BorderColor = ThemeManager.TextBoxBorder;
            if (pnlChart.Controls["ChartTitle"] is Label cTitle) cTitle.ForeColor = ThemeManager.TextPrimary;

            foreach (var tblPnl in new[] { pnlTopSelling, pnlWarnings })
            {
                tblPnl.FillColor = ThemeManager.CardBackground;
                tblPnl.BorderColor = ThemeManager.TextBoxBorder;
                foreach (Control c in tblPnl.Controls)
                {
                    if (c is Guna2Panel hdr)
                    {
                        hdr.CustomBorderColor = ThemeManager.TextBoxBorder;
                        if (hdr.Controls["TableTitle"] is Label tTitle) tTitle.ForeColor = ThemeManager.TextPrimary;
                    }
                }
            }

            foreach (var grid in new[] { dgvTopSelling, dgvWarnings })
            {
                ThemeManager.ApplyDataGridViewStyle(grid);
            }
        }
    
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BookStoreManagement.Themes.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            }
            base.Dispose(disposing);
        }
        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx", FileName = "BaoCaoDoanhThu.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var excelService = new BookStoreManagement.Services.ExcelExportService();
                        excelService.ExportDataGridView(dgvTopSelling, sfd.FileName, "Báo Cáo");
                        MessageBox.Show("Xuất file Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
