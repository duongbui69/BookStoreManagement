using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.UserControls
{
    public partial class DashboardControl : UserControl
    {
        private readonly DashboardRepository _dashboardRepo;
        private Label lblHeader;
        private FlowLayoutPanel flpCards;
        private Panel pnlChartsContainer;
        private Panel pnlLineChart;
        private Panel pnlPieChart;
        private DataGridView dgvLowStock;
        private Label lblLowStockTitle;
        private Panel pnlLowStockContainer;
        
        private DashboardStats currentStats;

        public DashboardControl()
        {
            _dashboardRepo = new DashboardRepository();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += DashboardControl_Load;
            this.Resize += DashboardControl_Resize;
        }

        private void InitializeUI()
        {
            this.BackColor = ThemeManager.Background;
            this.Dock = DockStyle.Fill;
            this.AutoScroll = true; // IMPORTANT for large dashboard
            this.Padding = new Padding(30);

            // Header
            lblHeader = new Label
            {
                Text = "Dashboard Overview",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                AutoSize = true,
                Location = new Point(30, 20)
            };
            this.Controls.Add(lblHeader);

            // FlowLayout for Cards
            flpCards = new FlowLayoutPanel
            {
                Location = new Point(30, 70),
                Width = this.Width - 60,
                Height = 150,
                WrapContents = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            this.Controls.Add(flpCards);

            // Charts Container
            pnlChartsContainer = new Panel
            {
                Location = new Point(30, 240),
                Width = this.Width - 60,
                Height = 350,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            
            pnlLineChart = new Panel
            {
                BackColor = ThemeManager.CardBackground,
                Dock = DockStyle.Fill
            };
            pnlLineChart.Paint += PnlLineChart_Paint;

            pnlPieChart = new Panel
            {
                BackColor = ThemeManager.CardBackground,
                Dock = DockStyle.Right,
                Width = 350
            };
            pnlPieChart.Paint += PnlPieChart_Paint;
            
            // Add margin between charts
            Panel chartSpacer = new Panel { Dock = DockStyle.Right, Width = 20, BackColor = Color.Transparent };

            pnlChartsContainer.Controls.Add(pnlLineChart);
            pnlChartsContainer.Controls.Add(chartSpacer);
            pnlChartsContainer.Controls.Add(pnlPieChart);
            this.Controls.Add(pnlChartsContainer);

            // Low Stock Grid Container
            pnlLowStockContainer = new Panel
            {
                Location = new Point(30, 620),
                Width = this.Width - 60,
                Height = 300,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = ThemeManager.CardBackground
            };

            lblLowStockTitle = new Label
            {
                Text = "Low Stock Inventory Alerts",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            pnlLowStockContainer.Controls.Add(lblLowStockTitle);

            dgvLowStock = new DataGridView
            {
                Location = new Point(20, 60),
                Width = pnlLowStockContainer.Width - 40,
                Height = 220,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackgroundColor = ThemeManager.CardBackground,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };
            dgvLowStock.CellFormatting += DgvLowStock_CellFormatting;
            pnlLowStockContainer.Controls.Add(dgvLowStock);
            
            this.Controls.Add(pnlLowStockContainer);
        }

        private void DashboardControl_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void DashboardControl_Resize(object sender, EventArgs e)
        {
            if (flpCards != null)
            {
                flpCards.Width = this.Width - 60;
                pnlChartsContainer.Width = this.Width - 60;
                pnlLowStockContainer.Width = this.Width - 60;
                pnlPieChart.Width = Math.Min(400, (this.Width - 60) / 3);
            }
        }

        private void LoadData()
        {
            try
            {
                currentStats = _dashboardRepo.GetStats();

                flpCards.Controls.Clear();
                int cardWidth = Math.Max(220, (this.Width - 140) / 4);

                // Revenue Card
                flpCards.Controls.Add(CreateSummaryCard("TOTAL REVENUE", $"${currentStats.TotalRevenue:N2}", currentStats.RevenueGrowth, Color.FromArgb(10, 50, 90), cardWidth));
                // Profit Card
                flpCards.Controls.Add(CreateSummaryCard("NET PROFIT", $"${currentStats.NetProfit:N2}", currentStats.ProfitGrowth, Color.FromArgb(128, 90, 213), cardWidth));
                // Orders Card
                flpCards.Controls.Add(CreateSummaryCard("TOTAL ORDERS", $"{currentStats.TotalOrders:N0}", currentStats.OrdersGrowth, Color.FromArgb(46, 204, 113), cardWidth));
                // Alerts Card
                flpCards.Controls.Add(CreateAlertCard("LOW STOCK ALERTS", $"{currentStats.LowStockCount} Items", "Requires action", Color.FromArgb(231, 76, 60), cardWidth));

                // Redraw charts
                pnlLineChart.Invalidate();
                pnlPieChart.Invalidate();

                // Bind grid
                dgvLowStock.DataSource = null;
                dgvLowStock.DataSource = currentStats.LowStockItems;

                ApplyThemeToGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dashboard data: " + ex.Message);
            }
        }

        private Panel CreateSummaryCard(string title, string value, decimal growth, Color borderColor, int width)
        {
            var pnl = new Panel { Width = width, Height = 130, BackColor = ThemeManager.CardBackground, Margin = new Padding(0, 0, 20, 0) };
            pnl.Controls.Add(new Panel { Width = 6, Dock = DockStyle.Left, BackColor = borderColor });
            pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.TextSecondary, Location = new Point(20, 20), AutoSize = true });
            pnl.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = ThemeManager.TextPrimary, Location = new Point(18, 50), AutoSize = true });

            string trendSymbol = growth >= 0 ? "↗" : "↘";
            Color trendColor = growth >= 0 ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);
            pnl.Controls.Add(new Label { Text = $"{trendSymbol} {Math.Abs(growth):N1}% from last month", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = trendColor, Location = new Point(20, 95), AutoSize = true });

            return pnl;
        }

        private Panel CreateAlertCard(string title, string value, string subtitle, Color borderColor, int width)
        {
            var pnl = new Panel { Width = width, Height = 130, BackColor = ThemeManager.CardBackground, Margin = new Padding(0, 0, 0, 0) };
            pnl.Controls.Add(new Panel { Width = 6, Dock = DockStyle.Left, BackColor = borderColor });
            pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.TextSecondary, Location = new Point(20, 20), AutoSize = true });
            pnl.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(231, 76, 60), Location = new Point(18, 50), AutoSize = true });
            pnl.Controls.Add(new Label { Text = "!" + subtitle, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(231, 76, 60), Location = new Point(20, 95), AutoSize = true });

            return pnl;
        }

        private void PnlLineChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = pnlLineChart.ClientRectangle;

            g.DrawString("Revenue Trends", new Font("Segoe UI", 14F, FontStyle.Bold), new SolidBrush(ThemeManager.TextPrimary), new Point(20, 20));
            g.DrawString("Annual sales performance overview", new Font("Segoe UI", 9F), new SolidBrush(ThemeManager.TextSecondary), new Point(20, 50));

            if (currentStats == null || currentStats.MonthlyRevenue.Count == 0) return;

            var sortedMonths = currentStats.MonthlyRevenue.OrderBy(x => x.Key).ToList();
            if (sortedMonths.Count < 2) return;

            float maxVal = (float)sortedMonths.Max(x => x.Value);
            if (maxVal == 0) maxVal = 1;

            float paddingX = 40f;
            float paddingY = 80f;
            float bottomY = rect.Height - 40f;
            float chartWidth = rect.Width - (paddingX * 2);
            float chartHeight = rect.Height - paddingY - 40f;

            PointF[] points = new PointF[sortedMonths.Count];
            for (int i = 0; i < sortedMonths.Count; i++)
            {
                float x = paddingX + (i * (chartWidth / (sortedMonths.Count - 1)));
                float y = bottomY - ((float)sortedMonths[i].Value / maxVal * chartHeight);
                points[i] = new PointF(x, y);

                // Draw X-axis labels (Month numbers for simplicity)
                g.DrawString($"Tháng {sortedMonths[i].Key}", new Font("Segoe UI", 8F), new SolidBrush(ThemeManager.TextSecondary), new PointF(x - 15, bottomY + 10));
            }

            // Draw shadow/gradient under the curve
            using (var path = new GraphicsPath())
            {
                path.AddCurve(points);
                path.AddLine(points.Last().X, bottomY, points.First().X, bottomY);
                path.CloseFigure();

                using (var brush = new LinearGradientBrush(new RectangleF(0, 0, rect.Width, rect.Height), Color.FromArgb(100, 41, 128, 185), Color.Transparent, LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }
            }

            // Draw the curve line
            using (var pen = new Pen(Color.FromArgb(41, 128, 185), 3f))
            {
                g.DrawCurve(pen, points);
            }
            
            // Draw dots at data points
            foreach (var p in points)
            {
                g.FillEllipse(Brushes.White, p.X - 4, p.Y - 4, 8, 8);
                g.DrawEllipse(Pens.DarkBlue, p.X - 4, p.Y - 4, 8, 8);
            }
        }

        private void PnlPieChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = pnlPieChart.ClientRectangle;

            g.DrawString("Sales by Category", new Font("Segoe UI", 12F, FontStyle.Bold), new SolidBrush(ThemeManager.TextPrimary), new Point(20, 20));

            if (currentStats == null || currentStats.SalesByCategory.Count == 0) return;

            decimal total = currentStats.SalesByCategory.Values.Sum();
            if (total == 0) return;

            float startAngle = -90f;
            Color[] colors = { Color.FromArgb(41, 128, 185), Color.FromArgb(128, 90, 213), Color.FromArgb(46, 204, 113), Color.FromArgb(241, 196, 15), Color.FromArgb(231, 76, 60), Color.Gray };
            int colorIndex = 0;

            float diameter = Math.Min(rect.Width - 40, rect.Height - 160);
            float cx = rect.Width / 2f;
            float cy = 60 + (diameter / 2f);
            var pieRect = new RectangleF(cx - (diameter / 2f), 60, diameter, diameter);

            // Calculate Legend Positions
            float legendY = cy + (diameter / 2f) + 30;
            float legendX = 20;

            foreach (var kvp in currentStats.SalesByCategory)
            {
                float sweepAngle = (float)((kvp.Value / total) * 360m);
                Color segmentColor = colors[colorIndex % colors.Length];
                
                using (var brush = new SolidBrush(segmentColor))
                {
                    g.FillPie(brush, pieRect.X, pieRect.Y, pieRect.Width, pieRect.Height, startAngle, sweepAngle);
                }

                // Legend
                using (var brush = new SolidBrush(segmentColor))
                {
                    g.FillEllipse(brush, legendX, legendY, 12, 12);
                }
                
                string legendText = $"{kvp.Key} ({(kvp.Value/total)*100:N0}%)";
                g.DrawString(legendText, new Font("Segoe UI", 9F), new SolidBrush(ThemeManager.TextPrimary), new PointF(legendX + 20, legendY - 2));

                legendX += 120;
                if (legendX + 100 > rect.Width)
                {
                    legendX = 20;
                    legendY += 25;
                }

                startAngle += sweepAngle;
                colorIndex++;
            }
            
            // Draw a donut hole to make it look modern
            using (var brush = new SolidBrush(ThemeManager.CardBackground))
            {
                float holeDiameter = diameter * 0.5f;
                g.FillEllipse(brush, cx - (holeDiameter / 2f), cy - (holeDiameter / 2f), holeDiameter, holeDiameter);
            }
        }

        private void DgvLowStock_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvLowStock.Columns[e.ColumnIndex].Name == "CurrentStock")
            {
                int currentStock = (int)e.Value;
                int threshold = (int)dgvLowStock.Rows[e.RowIndex].Cells["Threshold"].Value;
                
                e.CellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                if (currentStock == 0)
                {
                    e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60); // Red
                }
                else if (currentStock <= threshold)
                {
                    e.CellStyle.ForeColor = Color.FromArgb(243, 156, 18); // Orange/Warning
                }
            }
        }

        private void ApplyThemeToGrid()
        {
            dgvLowStock.BackgroundColor = ThemeManager.CardBackground;
            dgvLowStock.GridColor = ThemeManager.TextBoxBorder;
            dgvLowStock.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvLowStock.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvLowStock.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvLowStock.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            
            dgvLowStock.EnableHeadersVisualStyles = false;
            dgvLowStock.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvLowStock.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvLowStock.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvLowStock.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.CardBackground;
            
            dgvLowStock.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            this.BackColor = ThemeManager.Background;
            lblHeader.ForeColor = ThemeManager.TextPrimary;
            pnlLowStockContainer.BackColor = ThemeManager.CardBackground;
            lblLowStockTitle.ForeColor = ThemeManager.TextPrimary;
            pnlLineChart.BackColor = ThemeManager.CardBackground;
            pnlPieChart.BackColor = ThemeManager.CardBackground;

            ApplyThemeToGrid();
            LoadData(); 
        }
    }
}
