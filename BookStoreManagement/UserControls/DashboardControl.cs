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

        private int _hoveredRowIndex = -1;
        private int _hoveredAction = 0;

        private Guna2Panel pnlContent;
        
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        private Guna2Button btnExport;

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

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = true };

            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0,0,0,gutter) };
            lblTitle = new Label { Text = "Dashboard Overview", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0,0) };
            lblSubTitle = new Label { Text = "Welcome back, here's what's happening with your store today.", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(0,30) };
            
            btnExport = new Guna2Button { Text = "Export Report", Size = new Size(130, 36), BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle, btnExport });
            pnlPageHeader.Resize += (s, e) => {
                btnExport.Location = new Point(pnlPageHeader.Width - 140, 12);
            };

            tlpStats = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, Height = 100, 
                ColumnCount = 4, RowCount = 1,
                Margin = new Padding(0,0,0,gutter)
            };
            for(int i=0; i<4; i++) tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            
            card1 = CreateStatCard("Total Sales", "$0", Color.FromArgb(41, 128, 185));
            card2 = CreateStatCard("Net Profit", "$0", Color.FromArgb(128, 90, 213));
            card3 = CreateStatCard("Total Orders", "0", Color.FromArgb(46, 204, 113));
            card4 = CreateStatCard("Low Stock Alerts", "0", Color.FromArgb(231, 76, 60));
            
            tlpStats.Controls.Add(card1, 0, 0);
            tlpStats.Controls.Add(card2, 1, 0);
            tlpStats.Controls.Add(card3, 2, 0);
            tlpStats.Controls.Add(card4, 3, 0);

            tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 450,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, gutter),
                BackColor = Color.Transparent
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));

            pnlChartContainer = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,10,0) };
            lblChartTitle = new Label { Text = "Sales Trends", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
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
            lblTableTitle = new Label { Text = "Low Stock Products", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
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
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BookCode", HeaderText = "Book Code", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Title", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, Width = 150 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "Category", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CurrentStock", HeaderText = "Stock", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 60 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Threshold", HeaderText = "Min", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 50 });
            
            DataGridViewTextBoxColumn actionCol = new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "Actions", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
            dgvProducts.Columns.Add(actionCol);

            dgvProducts.CellPainting += DgvTopProducts_CellPainting;
            dgvProducts.CellMouseClick += DgvTopProducts_CellMouseClick;
            dgvProducts.CellMouseMove += DgvTopProducts_CellMouseMove;
            dgvProducts.CellMouseLeave += DgvTopProducts_CellMouseLeave;
            dgvProducts.CellFormatting += DgvProducts_CellFormatting;
            
            pnlTableContainer.Controls.Add(dgvProducts);

            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadTableData(); };
            pnlTableContainer.Controls.Add(paginationControl);
            
            pnlTableHeader.BringToFront();
            dgvProducts.BringToFront();
            paginationControl.BringToFront();

            tlpMain.Controls.Add(pnlTableContainer, 1, 0);

            pnlContent.Controls.Add(tlpMain);
            pnlContent.Controls.Add(tlpStats);
            pnlContent.Controls.Add(pnlPageHeader);
            
            pnlPageHeader.BringToFront();
            tlpStats.BringToFront();
            tlpMain.BringToFront();

            this.Controls.Add(pnlContent);

            ApplyTheme();
        }

        private Guna2Panel CreateStatCard(string title, string value, Color leftColor)
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,10,0) };
            
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
            
            foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"${currentStats.TotalRevenue:N0}";
            foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"${currentStats.NetProfit:N0}";
            foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = currentStats.TotalOrders.ToString("N0");
            foreach(Control c in card4.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = currentStats.LowStockCount.ToString();
            
            pnlLineChart.Invalidate();
            
            _currentPage = 1;
            LoadTableData();
        }

        private async Task LoadDataAsync()
        {
            currentStats = await _dashboardService.GetStatsAsync();
            
            foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"${currentStats.TotalRevenue:N0}";
            foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"${currentStats.NetProfit:N0}";
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

        private void DgvTopProducts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvProducts.Columns[e.ColumnIndex].Name == "Actions")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                var rect = e.CellBounds;
                var editRect = new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height);
                var delRect = new Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height);
                
                if (e.RowIndex == _hoveredRowIndex)
                {
                    if (_hoveredAction == 1)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(30, ThemeManager.ButtonFill)))
                            e.Graphics.FillRectangle(brush, editRect);
                    }
                    else if (_hoveredAction == 2)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(30, Color.FromArgb(231, 76, 60))))
                            e.Graphics.FillRectangle(brush, delRect);
                    }
                }

                int editFontSize = (_hoveredRowIndex == e.RowIndex && _hoveredAction == 1) ? 14 : 12;
                int delFontSize = (_hoveredRowIndex == e.RowIndex && _hoveredAction == 2) ? 14 : 12;

                using (var font = new Font("Segoe UI Emoji", editFontSize)) { TextRenderer.DrawText(e.Graphics, "✏️", font, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter); }
                using (var font = new Font("Segoe UI Emoji", delFontSize)) { TextRenderer.DrawText(e.Graphics, "🗑️", font, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter); }
                
                using (var pen = new Pen(Color.LightGray))
                {
                    e.Graphics.DrawLine(pen, rect.X + rect.Width / 2, rect.Y + 8, rect.X + rect.Width / 2, rect.Bottom - 8);
                }
                
                e.Handled = true;
            }
        }

        private void DgvTopProducts_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvProducts.Columns[e.ColumnIndex].Name == "Actions")
            {
                int action = (e.X < dgvProducts.Columns[e.ColumnIndex].Width / 2) ? 1 : 2;
                
                if (_hoveredRowIndex != e.RowIndex || _hoveredAction != action)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = e.RowIndex;
                    _hoveredAction = action;
                    
                    if (oldRow >= 0) dgvProducts.InvalidateCell(e.ColumnIndex, oldRow);
                    dgvProducts.InvalidateCell(e.ColumnIndex, _hoveredRowIndex);
                }
                dgvProducts.Cursor = Cursors.Hand;
            }
            else
            {
                if (_hoveredRowIndex >= 0)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = -1;
                    _hoveredAction = 0;
                    if (e.ColumnIndex >= 0) dgvProducts.InvalidateCell(dgvProducts.Columns["Actions"].Index, oldRow);
                }
                dgvProducts.Cursor = Cursors.Default;
            }
        }

        private void DgvTopProducts_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredRowIndex >= 0)
            {
                int oldRow = _hoveredRowIndex;
                _hoveredRowIndex = -1;
                _hoveredAction = 0;
                dgvProducts.InvalidateCell(dgvProducts.Columns["Actions"].Index, oldRow);
            }
            dgvProducts.Cursor = Cursors.Default;
        }

        private async void DgvTopProducts_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvProducts.Columns[e.ColumnIndex].Name == "Actions")
            {
                int bookId = Convert.ToInt32(dgvProducts.Rows[e.RowIndex].Cells[0].Value);
                
                if (e.X < dgvProducts.Columns[e.ColumnIndex].Width / 2)
                {
                    // Edit
                    var book = _bookService.GetById(bookId);
                    if (book != null)
                    {
                        var frm = new Forms.BookForm(book);
                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadDataAsync();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Book not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Delete
                    if (MessageBox.Show("Are you sure you want to delete this book?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            _bookService.SetActive(bookId, false);
                            MessageBox.Show("Book deleted successfully!");
                            await LoadDataAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
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
                    g.DrawString($"Tháng {sortedMonths[i].Key}", font, brush, new PointF(x - 15, bottomY + 10));
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
            
            btnExport.FillColor = ThemeManager.ButtonFill;
            btnExport.ForeColor = ThemeManager.ButtonText;

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

            dgvProducts.BackgroundColor = ThemeManager.CardBackground;
            dgvProducts.GridColor = ThemeManager.TextBoxBorder;
            
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvProducts.AlternatingRowsDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvProducts.AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeManager.ButtonFill;
            dgvProducts.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

            dgvProducts.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvProducts.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvProducts.DefaultCellStyle.SelectionBackColor = ThemeManager.ButtonFill;
            dgvProducts.DefaultCellStyle.SelectionForeColor = Color.White;
            
            dgvProducts.EnableHeadersVisualStyles = false;
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvProducts.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Background;
            dgvProducts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            
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

