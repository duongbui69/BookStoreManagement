using BookStoreManagement.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using BookStoreManagement.Models;
using BookStoreManagement.Interfaces;
using Guna.UI2.WinForms;
using System.Collections.Generic;

namespace BookStoreManagement.UserControls
{
    public partial class DashboardControl : UserControl, ISearchableControl
    {
        private readonly DashboardService _dashboardService;
        private BookService _bookService;


        private Guna2Panel pnlContent;
        
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;

        private TableLayoutPanel tlpStats;
        private Guna2Panel card1, card2, card3, card4;

        private TableLayoutPanel tlpMain;
        
        private Guna2Panel pnlChartContainer;
        private Label lblChartTitle;
        private Panel pnlLineChart;

        private Guna2Panel pnlTableContainer;
        private Guna2Panel pnlTableHeader;
        private Label lblTableTitle;
        private DataGridView dgvProducts;
        private PaginationControl paginationControl;
        
        private DashboardStats currentStats;
        private int _currentPage = 1;
        private int _pageSize = 5;

        public async void PerformSearch(string keyword)
        {
            // Dashboard search not specifically scoped, maybe reload data
            await LoadDataAsync();
        }

        public DashboardControl()
        {
            _dashboardService = new DashboardService();
            _bookService = new BookService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += DashboardControl_Load;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 20;
            this.Padding = new Padding(0);

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = false };

            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(0,0,0,gutter) };
            lblTitle = new Label { Text = "Tổng quan", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Chào mừng trở lại, đây là thông tin cửa hàng hôm nay.", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(0, 45) };
            
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            tlpStats = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, Height = 100, 
                ColumnCount = 4, RowCount = 1,
                Margin = new Padding(0,0,0,gutter)
            };
            for(int i=0; i<4; i++) tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            
            card1 = CreateStatCard("TỔNG DOANH THU", "$0", Color.FromArgb(41, 128, 185));
            card2 = CreateStatCard("LỢI NHUẬN RÒNG", "$0", Color.FromArgb(128, 90, 213));
            card3 = CreateStatCard("TỔNG ĐƠN HÀNG", "0", Color.FromArgb(46, 204, 113));
            card4 = CreateStatCard("SÁCH SẮP HẾT", "0", Color.FromArgb(231, 76, 60));
            
            card1.Margin = new Padding(0, 0, 10, 0);
            card2.Margin = new Padding(10, 0, 10, 0);
            card3.Margin = new Padding(10, 0, 10, 0);
            card4.Margin = new Padding(10, 0, 0, 0);
            
            tlpStats.Controls.Add(card1, 0, 0);
            tlpStats.Controls.Add(card2, 1, 0);
            tlpStats.Controls.Add(card3, 2, 0);
            tlpStats.Controls.Add(card4, 3, 0);

            tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, gutter),
                BackColor = Color.Transparent
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));

            pnlChartContainer = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,10,0) };
            lblChartTitle = new Label { Text = "Xu hướng Bán hàng", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            lblChartTitle.Tag = "ChartTitle";
            pnlChartContainer.Controls.Add(lblChartTitle);
            
            pnlLineChart = new Panel { Location = new Point(20, 60), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.Transparent };
            pnlLineChart.Paint += PnlLineChart_Paint;
            pnlChartContainer.Controls.Add(pnlLineChart);
            pnlChartContainer.Resize += (s, e) => {
                pnlLineChart.Size = new Size(pnlChartContainer.Width - 40, pnlChartContainer.Height - 80);
                pnlLineChart.Invalidate();
            };
            
            tlpMain.Controls.Add(pnlChartContainer, 0, 0);

            pnlTableContainer = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), Margin = new Padding(10,0,0,0) };
            
            pnlTableHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, CustomBorderThickness = new Padding(0,0,0,1) };
            lblTableTitle = new Label { Text = "Sản phẩm sắp hết hàng", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            lblTableTitle.Tag = "TableTitle";
            pnlTableHeader.Controls.Add(lblTableTitle);
            pnlTableContainer.Controls.Add(pnlTableHeader);
            
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 50 },
                AutoGenerateColumns = false,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            dgvProducts.SetDoubleBuffered(true);
            
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Visible = false });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BookCode", HeaderText = "Mã Sách", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tiêu đề", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, Width = 150 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "Danh mục", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CurrentStock", HeaderText = "Tồn kho", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 60 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Threshold", HeaderText = "Tối thiểu", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 50 });
            
            dgvProducts.CellFormatting += DgvProducts_CellFormatting;
            
            pnlTableContainer.Controls.Add(dgvProducts);

            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadTableData(); };
            pnlTableContainer.Controls.Add(paginationControl);
            
            pnlTableHeader.BringToFront();
            dgvProducts.BringToFront();
            paginationControl.BringToFront();

            tlpMain.Controls.Add(pnlTableContainer, 1, 0);

            Panel spacer1 = new Panel { Dock = DockStyle.Top, Height = gutter, BackColor = Color.Transparent };
            Panel spacer2 = new Panel { Dock = DockStyle.Top, Height = gutter, BackColor = Color.Transparent };

            pnlContent.Controls.Add(tlpMain);
            pnlContent.Controls.Add(spacer2);
            pnlContent.Controls.Add(tlpStats);
            pnlContent.Controls.Add(spacer1);
            pnlContent.Controls.Add(pnlPageHeader);
            
            pnlPageHeader.BringToFront();
            spacer1.BringToFront();
            tlpStats.BringToFront();
            spacer2.BringToFront();
            tlpMain.BringToFront();

            this.Controls.Add(pnlContent);

            ApplyTheme();
        }

        private Guna2Panel CreateStatCard(string title, string value, Color leftColor)
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), Margin = new Padding(0) };
            
            var leftBar = new Panel { Dock = DockStyle.Left, Width = 4, BackColor = leftColor };
            leftBar.Tag = "LeftBarColor";
            pnl.Controls.Add(leftBar);
            
            Label lblTitle = new Label { Text = title.ToUpper(), Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(15, 15) };
            lblTitle.Tag = "CardTitle";
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(12, 35) };
            lblValue.Tag = "CardValue";

            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);
            return pnl;
        }

        private async void DashboardControl_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private void LoadData()
        {
            currentStats = _dashboardService.GetStats();
            
            foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"{currentStats.TotalRevenue:N0} ₫";
            foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"{currentStats.NetProfit:N0} ₫";
            foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = currentStats.TotalOrders.ToString("N0");
            foreach(Control c in card4.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = currentStats.LowStockCount.ToString();
            
            pnlLineChart.Invalidate();
            
            _currentPage = 1;
            LoadTableData();
        }

        private async Task LoadDataAsync()
        {
            currentStats = await _dashboardService.GetStatsAsync();
            
            foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"{currentStats.TotalRevenue:N0} ₫";
            foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"{currentStats.NetProfit:N0} ₫";
            foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = currentStats.TotalOrders.ToString("N0");
            foreach(Control c in card4.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = currentStats.LowStockCount.ToString();
            
            pnlLineChart.Invalidate();
            
            _currentPage = 1;
            LoadTableData();
        }

        private void LoadTableData()
        {
            if (currentStats == null) return;
            
            var pagedItems = currentStats.LowStockItems.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
            dgvProducts.DataSource = pagedItems;
            
            paginationControl.UpdatePagination(currentStats.LowStockItems.Count, _currentPage, _pageSize);
        }



        private void DgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvProducts.Columns[e.ColumnIndex].Name == "CurrentStock")
            {
                int currentStock = (int)e.Value;
                int threshold = (int)dgvProducts.Rows[e.RowIndex].Cells["Threshold"].Value;
                
                e.CellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                if (currentStock == 0)
                {
                    e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60); // Red
                }
                else if (currentStock <= threshold)
                {
                    e.CellStyle.ForeColor = Color.FromArgb(243, 156, 18); // Orange
                }
            }
        }
        
        private void PnlLineChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = pnlLineChart.ClientRectangle;

            if (currentStats == null || currentStats.MonthlyRevenue.Count == 0) return;

            var sortedMonths = currentStats.MonthlyRevenue.OrderBy(x => x.Key).ToList();
            if (sortedMonths.Count < 2) return;

            float maxVal = (float)sortedMonths.Max(x => x.Value);
            if (maxVal == 0) maxVal = 1;

            float paddingX = 40f;
            float paddingY = 40f;
            float bottomY = rect.Height - paddingY;
            float chartWidth = rect.Width - (paddingX * 2);
            float chartHeight = rect.Height - paddingY - 20f;

            PointF[] points = new PointF[sortedMonths.Count];
            for (int i = 0; i < sortedMonths.Count; i++)
            {
                float x = paddingX + (i * (chartWidth / (sortedMonths.Count - 1)));
                float y = bottomY - ((float)sortedMonths[i].Value / maxVal * chartHeight);
                points[i] = new PointF(x, y);

                using (var font = new Font("Segoe UI", 8F))
                using (var brush = new SolidBrush(ThemeManager.TextSecondary))
                {
                    var format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    g.TranslateTransform(x, bottomY + 15);
                    g.RotateTransform(-45);
                    g.DrawString($"Tháng {sortedMonths[i].Key}", font, brush, 0, 0, format);
                    g.ResetTransform();
                }
            }

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

            using (var pen = new Pen(Color.FromArgb(41, 128, 185), 3f))
            {
                g.DrawCurve(pen, points);
            }
            
            foreach (var p in points)
            {
                g.FillEllipse(Brushes.White, p.X - 4, p.Y - 4, 8, 8);
                g.DrawEllipse(Pens.DarkBlue, p.X - 4, p.Y - 4, 8, 8);
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
            

            ApplyThemeToCard(card1);
            ApplyThemeToCard(card2);
            ApplyThemeToCard(card3);
            ApplyThemeToCard(card4);

            pnlChartContainer.FillColor = ThemeManager.CardBackground;
            pnlChartContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            lblChartTitle.ForeColor = ThemeManager.TextPrimary;
            
            pnlTableContainer.FillColor = ThemeManager.CardBackground;
            pnlTableContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            pnlTableHeader.FillColor = ThemeManager.Background;
            pnlTableHeader.CustomBorderColor = ThemeManager.TextBoxBorder;
            lblTableTitle.ForeColor = ThemeManager.TextPrimary;

            ThemeManager.ApplyDataGridViewStyle(dgvProducts);
            
            pnlLineChart.Invalidate();
        }
        
        private void ApplyThemeToCard(Guna2Panel card)
        {
            card.FillColor = ThemeManager.CardBackground;
            card.CustomBorderColor = ThemeManager.TextBoxBorder;

            foreach(Control ctrl in card.Controls)
            {
                if (ctrl.Tag?.ToString() == "CardTitle") ctrl.ForeColor = ThemeManager.TextSecondary;
                if (ctrl.Tag?.ToString() == "CardValue") ctrl.ForeColor = ThemeManager.TextPrimary;
            }
        }
}
}

