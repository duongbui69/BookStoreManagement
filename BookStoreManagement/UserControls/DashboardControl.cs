using BookStoreManagement.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D; using LiveCharts; using LiveCharts.Wpf; using LiveCharts.WinForms;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookStoreManagement.Events;
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
        private LiveCharts.WinForms.CartesianChart cartesianChart;
        private Guna2ComboBox cbTimeFilter;

        private Guna2Panel pnlTableContainer;
        private Guna2Panel pnlTableHeader;
        private Label lblTableTitle;
        private DataGridView dgvProducts;
        private PaginationControl paginationControl;
        
        private DashboardStats currentStats;
        private System.Windows.Forms.Timer _transactionDebounceTimer;
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
            _transactionDebounceTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _transactionDebounceTimer.Tick += (s, e) => {
                _transactionDebounceTimer.Stop();
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.Invoke((System.Windows.Forms.MethodInvoker)async delegate { await LoadDataAsync(); });
                }
            };
            this.Load += DashboardControl_Load;
            this.Disposed += (s, e) => {
                _resizeTimer?.Dispose();
                _transactionDebounceTimer?.Dispose();
                GlobalEvents.TransactionCompleted -= OnTransactionCompleted;
            };

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
            
            cbTimeFilter = new Guna2ComboBox
            {
                Location = new Point(pnlChartContainer.Width - 220, 15),
                Width = 200,
                Height = 36,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 4,
                Font = new Font("Segoe UI", 10F)
            };
            cbTimeFilter.Items.AddRange(new object[] { "Tháng này", "Tuần này", "Hôm nay", "Năm nay" });
            cbTimeFilter.SelectedIndex = 0;
            cbTimeFilter.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            pnlChartContainer.Controls.Add(cbTimeFilter);
            
            cartesianChart = new LiveCharts.WinForms.CartesianChart
            {
                Location = new Point(20, 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            pnlChartContainer.Controls.Add(cartesianChart);
            pnlChartContainer.Resize += (s, e) => {
                cartesianChart.Size = new Size(pnlChartContainer.Width - 40, pnlChartContainer.Height - 80);
                cbTimeFilter.Location = new Point(pnlChartContainer.Width - 220, 15);
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

        private Guna2Panel CreateStatCard(string title, string value, Color accentColor)
        {
            var pnl = new Guna2Panel 
            { 
                Dock = DockStyle.Fill, 
                BorderThickness = 1, 
                BorderRadius = 10,
                Margin = new Padding(0) 
            };
            
            Label lblTitle = new Label { Text = title.ToUpper(), Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(16, 14) };
            lblTitle.Tag = "CardTitle";
            
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 22F, FontStyle.Bold), AutoSize = true, Location = new Point(14, 32), ForeColor = accentColor };
            lblValue.Tag = "CardValue";

            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);
            return pnl;
        }

        private async void DashboardControl_Load(object sender, EventArgs e)
        {
            GlobalEvents.TransactionCompleted -= OnTransactionCompleted;
            GlobalEvents.TransactionCompleted += OnTransactionCompleted;
            await LoadDataAsync();
        }

        private void OnTransactionCompleted()
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.Invoke((System.Windows.Forms.MethodInvoker)delegate {
                    _transactionDebounceTimer.Stop();
                    _transactionDebounceTimer.Start();
                });
            }
        }

        private void LoadData()
        {
            currentStats = _dashboardService.GetStats(cbTimeFilter.Text);
            
            foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"{currentStats.TotalRevenue:N0} ₫";
            foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"{currentStats.NetProfit:N0} ₫";
            foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = currentStats.TotalOrders.ToString("N0");
            foreach(Control c in card4.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = currentStats.LowStockCount.ToString();
            
            SetupChart(currentStats);
            
            _currentPage = 1;
            LoadTableData();
        }

        private async Task LoadDataAsync()
        {
            currentStats = await _dashboardService.GetStatsAsync(cbTimeFilter.Text);
            
            foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"{currentStats.TotalRevenue:N0} ₫";
            foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"{currentStats.NetProfit:N0} ₫";
            foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = currentStats.TotalOrders.ToString("N0");
            foreach(Control c in card4.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = currentStats.LowStockCount.ToString();
            
            SetupChart(currentStats);
            
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

        private void SetupChart(DashboardStats stats)
        {
            if (stats == null) return;

            var series = new LineSeries
            {
                Title = "Doanh thu",
                Values = new ChartValues<decimal>(),
                PointGeometrySize = 15,
                LineSmoothness = 0.5,
                StrokeThickness = 3,
                Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 128, 185)),
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 41, 128, 185))
            };

            var labels = new List<string>();

            // Convert dictionary to sorted list
            var sortedData = stats.MonthlyRevenue.OrderBy(x => x.Key).ToList();
            
            foreach (var item in sortedData)
            {
                series.Values.Add(item.Value);
                if (cbTimeFilter.Text == "Hôm nay" || cbTimeFilter.Text == "Tuần này")
                {
                    labels.Add(item.Key.ToString("dd/MM"));
                }
                else
                {
                    labels.Add(item.Key.ToString("MM/yyyy"));
                }
            }

            cartesianChart.Series = new SeriesCollection { series };
            
            cartesianChart.AxisX.Clear();
            cartesianChart.AxisX.Add(new Axis
            {
                Labels = labels,
                Separator = new Separator { Step = 1, IsEnabled = false },
                LabelsRotation = 15
            });

            cartesianChart.AxisY.Clear();
            cartesianChart.AxisY.Add(new Axis
            {
                LabelFormatter = value => value.ToString("N0") + " ₫",
                Separator = new Separator { StrokeThickness = 1, StrokeDashArray = new System.Windows.Media.DoubleCollection { 2 } }
            });
            
            // Set chart colors based on theme
            cartesianChart.AxisX[0].Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(ThemeManager.TextSecondary.R, ThemeManager.TextSecondary.G, ThemeManager.TextSecondary.B));
            cartesianChart.AxisY[0].Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(ThemeManager.TextSecondary.R, ThemeManager.TextSecondary.G, ThemeManager.TextSecondary.B));
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
        }
        
        private void ApplyThemeToCard(Guna2Panel card)
        {
            card.BackColor = Color.Transparent;
            card.FillColor = ThemeManager.CardBackground;
            card.BorderColor = ThemeManager.TextBoxBorder;

            foreach(Control ctrl in card.Controls)
            {
                if (ctrl.Tag?.ToString() == "CardTitle") ctrl.ForeColor = ThemeManager.TextSecondary;
            }
        }
}
}

