using BookStoreManagement.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
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
        private Guna2Button btnExportExcel;

        // KPI Cards
        private Guna2Panel pnlMetrics;
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
            int gutter = 20;
            this.Padding = new Padding(0);
            this.AutoScroll = true;

            pnlContent = new Guna2Panel { Dock = DockStyle.Top, Padding = new Padding(gutter), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };

            // 1. Page Header
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0, 0, 0, gutter) };
            lblTitle = new Label { Text = "Thống kê tổng hợp", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            
            btnExportExcel = new Guna2Button { 
                Text = "Xuất Excel", 
                Size = new Size(130, 36), 
                BorderRadius = 4, 
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, btnExportExcel });
            pnlHeader.Resize += (s, e) => { btnExportExcel.Location = new Point(pnlHeader.Width - 130, 10); };

            // 2. Metric Cards
            pnlMetrics = new Guna2Panel { Dock = DockStyle.Top, Height = 130, Margin = new Padding(0, 0, 0, gutter) };
            cardRevenue = CreateMetricCard("Tổng Doanh Thu", "1,245,000,000 đ", "+12.5% so với tháng trước", true);
            cardOrders = CreateMetricCard("Tổng Đơn Hàng", "4,521", "-2.1% so với tháng trước", false);
            cardLowStock = CreateMetricCard("Sản Phẩm Sắp Hết", "34", "Cần nhập hàng khẩn cấp", false, true);
            
            pnlMetrics.Controls.AddRange(new Control[] { cardRevenue, cardOrders, cardLowStock });
            pnlMetrics.Resize += (s, e) => 
            {
                int cardWidth = (pnlMetrics.Width - (gutter * 2)) / 3;
                if (cardWidth > 0)
                {
                    cardRevenue.Width = cardWidth; cardRevenue.Left = 0;
                    cardOrders.Width = cardWidth; cardOrders.Left = cardWidth + gutter;
                    cardLowStock.Width = cardWidth; cardLowStock.Left = (cardWidth + gutter) * 2;
                }
            };

            // 3. Chart
            pnlChart = new Guna2Panel 
            { 
                Dock = DockStyle.Top, Height = 350, 
                BorderRadius = 4,
                BorderThickness = 1,
                Margin = new Padding(0, 0, 0, gutter)
            };
            var lblChartTitle = new Label { Text = "Biểu đồ doanh thu 12 tháng gần nhất", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20), Name = "ChartTitle" };
            pnlChart.Controls.Add(lblChartTitle);
            pnlChart.Paint += PnlChart_Paint;

            // 4. Tables Container
            pnlTables = new Guna2Panel { Dock = DockStyle.Top, Height = 400, Margin = new Padding(0, 0, 0, gutter) };
            
            pnlTopSelling = CreateTableContainer("Top 5 Sách bán chạy", out dgvTopSelling);
            pnlWarnings = CreateTableContainer("Cảnh báo tồn kho", out dgvWarnings);

            // Configure Top Selling Grid
            dgvTopSelling.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Rank", HeaderText = "Hạng", Width = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvTopSelling.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tên sách", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvTopSelling.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "QuantitySold", HeaderText = "Số lượng bán", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 11, FontStyle.Bold) } });
            dgvTopSelling.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Revenue", HeaderText = "Doanh thu", Width = 150, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N0" } });
            
            // Configure Warnings Grid
            dgvWarnings.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Sku", HeaderText = "ISBN", Width = 120 });
            dgvWarnings.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tên sách", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvWarnings.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CurrentStock", HeaderText = "Tồn hiện tại", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 11, FontStyle.Bold) } });
            dgvWarnings.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });

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

            // Fix DataGridView Column count bug (AutoGenerateColumns = false must be set correctly for manually added columns)
            dgvTopSelling.AutoGenerateColumns = false;
            dgvWarnings.AutoGenerateColumns = false;

            pnlContent.Controls.Add(pnlTables);
            pnlContent.Controls.Add(pnlChart);
            pnlContent.Controls.Add(pnlMetrics);
            pnlContent.Controls.Add(pnlHeader);

            this.Controls.Add(pnlContent);
            ApplyTheme();
        }

        private Guna2Panel CreateTableContainer(string title, out DataGridView grid)
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Left, BorderRadius = 4, BorderThickness = 1 };
            
            var pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 50, CustomBorderThickness = new Padding(0,0,0,1) };
            var lblTitle = new Label { Name = "TableTitle", Text = title, Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(15, 12) };
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
                ScrollBars = ScrollBars.Vertical
            };
            grid.SetDoubleBuffered(true);
            
            pnl.Controls.Add(grid);
            pnl.Controls.Add(pnlHeader);
            grid.BringToFront();
            return pnl;
        }

        private Guna2Panel CreateMetricCard(string title, string val, string subText, bool isPositive, bool isError = false)
        {
            var card = new Guna2Panel { Height = 130, BorderRadius = 4, BorderThickness = 1 };
            
            var lblTitle = new Label { Name = "TitleLabel", Text = title, Font = new Font("Segoe UI", 14F), AutoSize = true, Location = new Point(20, 20) };
            var lblVal = new Label { Name = "ValueLabel", Text = val, Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(16, 45) };
            
            var pnlBadge = new Guna2Panel { Name = "BadgePanel", BorderRadius = 4, AutoSize = true, Location = new Point(20, 95) };
            var lblBadge = new Label { Name = "BadgeLabel", Text = subText, Font = new Font("Segoe UI", 11F, FontStyle.Bold), AutoSize = true, Padding = new Padding(4) };
            pnlBadge.Controls.Add(lblBadge);

            // Store info in Tag
            card.Tag = new Tuple<bool, bool>(isPositive, isError);

            card.Controls.AddRange(new Control[] { lblTitle, lblVal, pnlBadge });
            return card;
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            _stats = await _service.GetFinancialReportsAsync();
            if (_stats == null) return;
            
            cardRevenue.Controls["ValueLabel"].Text = $"{_stats.TotalRevenue:N0} đ";
            
            string revGrowthSign = _stats.RevenueGrowth >= 0 ? "+" : "";
            (cardRevenue.Controls["BadgePanel"].Controls["BadgeLabel"] as Label).Text = $"{revGrowthSign}{_stats.RevenueGrowth:N1}% so với tháng trước";
            cardRevenue.Tag = new Tuple<bool, bool>(_stats.RevenueGrowth >= 0, false);

            cardOrders.Controls["ValueLabel"].Text = $"{_stats.TotalOrders:N0}";
            string ordGrowthSign = _stats.OrdersGrowth >= 0 ? "+" : "";
            (cardOrders.Controls["BadgePanel"].Controls["BadgeLabel"] as Label).Text = $"{ordGrowthSign}{_stats.OrdersGrowth:N1}% so với tháng trước";
            cardOrders.Tag = new Tuple<bool, bool>(_stats.OrdersGrowth >= 0, false);

            cardLowStock.Controls["ValueLabel"].Text = $"{_stats.LowStockCount}";
            
            UpdateGrids();
            ApplyTheme(); // re-apply colors based on updated tags
            pnlChart.Invalidate();
        }

        private void UpdateGrids()
        {
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

        private void DgvWarnings_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dgvWarnings.Columns[e.ColumnIndex].Name == "Status")
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
                if (numPoints > 1)
                {
                    PointF[] points = new PointF[numPoints];
                    int idx = 0;
                    string[] monthNames = { "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" };

                    foreach (var kvp in _stats.MonthlyRevenue.OrderBy(k => k.Key))
                    {
                        int m = kvp.Key;
                        decimal rev = kvp.Value;
                        
                        int x = paddingX + (idx * chartWidth / (numPoints - 1));
                        int hRev = (int)((rev / maxVal) * chartHeight);
                        int y = pnlChart.Height - paddingY - hRev;
                        
                        points[idx] = new PointF(x, y);

                        using (var b = new SolidBrush(ThemeManager.TextSecondary))
                        {
                            var size = g.MeasureString(monthNames[m - 1], font2);
                            g.DrawString(monthNames[m - 1], font2, b, x - size.Width / 2, pnlChart.Height - paddingY + 10);
                        }
                        idx++;
                    }

                    // Draw line
                    using (var penLine = new Pen(ThemeManager.ButtonFill, 3))
                    {
                        g.DrawLines(penLine, points);
                    }

                    // Draw points
                    using (var brushPoint = new SolidBrush(ThemeManager.CardBackground))
                    using (var penPoint = new Pen(ThemeManager.ButtonFill, 2))
                    {
                        foreach (var p in points)
                        {
                            g.FillEllipse(brushPoint, p.X - 4, p.Y - 4, 8, 8);
                            g.DrawEllipse(penPoint, p.X - 4, p.Y - 4, 8, 8);
                        }
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
            this.BackColor = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;

            lblTitle.ForeColor = ThemeManager.TextPrimary;

            btnExportExcel.FillColor = ThemeManager.ButtonFill;
            btnExportExcel.ForeColor = ThemeManager.ButtonText;

            // Metrics Cards
            var cards = new[] { cardRevenue, cardOrders, cardLowStock };
            foreach (var card in cards)
            {
                card.FillColor = ThemeManager.CardBackground;
                card.CustomBorderColor = ThemeManager.TextBoxBorder;
                if (card.Controls["TitleLabel"] is Label lTitle) lTitle.ForeColor = ThemeManager.TextSecondary;
                
                var tag = card.Tag as Tuple<bool, bool>;
                bool isPositive = tag?.Item1 ?? true;
                bool isError = tag?.Item2 ?? false;

                if (card.Controls["ValueLabel"] is Label lVal) 
                {
                    if (isError) lVal.ForeColor = Color.FromArgb(186, 26, 26); // error color
                    else lVal.ForeColor = ThemeManager.ButtonFill; // primary color
                }
                
                if (card.Controls["BadgePanel"] is Guna2Panel bPnl)
                {
                    if (bPnl.Controls["BadgeLabel"] is Label bLbl)
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
                grid.BackgroundColor = ThemeManager.CardBackground;
                grid.GridColor = ThemeManager.TextBoxBorder;
                grid.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
                grid.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
                grid.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
                grid.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;

                grid.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
                grid.AlternatingRowsDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
                grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
                grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;

                grid.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
                grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
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
    }
}
