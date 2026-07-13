using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;
using System.Collections.Generic;
using BookStoreManagement.Interfaces;

namespace BookStoreManagement.UserControls
{
    public partial class CatalogControl : UserControl, ISearchableControl
    {
        private readonly CatalogService _catalogService;
        private readonly BookService _bookService;
        
        private Guna2Panel pnlContent;
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;

        private TableLayoutPanel tlpStats;
        private Guna2Panel card1, card2, card3, card4;
        
        private Guna2Panel pnlFilters;
        private Guna2ComboBox cbCategory;
        private Guna2ComboBox cbPriceRange;
        private Guna2Button btnFilterAll, btnFilterIn, btnFilterOut;
        private Guna2Button btnExport;
        private Guna2Button btnAdd;

        private Guna2Panel pnlGridContainer;
        private Guna2DataGridView dgvBooks;
        
        private PaginationControl paginationControl;
        
        private TableLayoutPanel tlpBento;
        private Guna2Panel pnlInsights;
        private Guna2Panel pnlShortcuts;
        
        private int _currentPage = 1;
        private int _pageSize = 5; 
        private string _stockFilter = "ALL";
        private string _currentSearchTerm = "";

        public void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            LoadData();
        }

        public CatalogControl()
        {
            _catalogService = new CatalogService();
            _bookService = new BookService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += CatalogControl_Load;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 20;
            this.Padding = new Padding(0);

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = true };

            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0,0,0,gutter) };
            lblTitle = new Label { Text = "Catalog Management", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(0,0) };
            lblSubTitle = new Label { Text = "View and manage the comprehensive book repository.", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(0,30) };
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            tlpStats = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, Height = 100, 
                ColumnCount = 4, RowCount = 1,
                Margin = new Padding(0,0,0,gutter)
            };
            for(int i=0; i<4; i++) tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            
            card1 = CreateStatCard("Total Titles", "0", Color.FromArgb(41, 128, 185));
            card2 = CreateStatCard("Active Categories", "0", Color.FromArgb(115, 69, 182));
            card3 = CreateStatCard("In Stock Value", "$0", Color.FromArgb(0, 186, 97));
            card4 = CreateStatCard("Low Stock Alerts", "0", Color.FromArgb(186, 26, 26));
            
            tlpStats.Controls.Add(card1, 0, 0);
            tlpStats.Controls.Add(card2, 1, 0);
            tlpStats.Controls.Add(card3, 2, 0);
            tlpStats.Controls.Add(card4, 3, 0);

            pnlFilters = new Guna2Panel { Dock = DockStyle.Top, Height = 60, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,0,gutter) };
            
            Label lblCat = new Label { Text = "Category:", AutoSize = true, Location = new Point(10, 20), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbCategory = new Guna2ComboBox { Size = new Size(130, 36), Location = new Point(80, 12), BorderRadius = 4 };
            cbCategory.Items.Add("All Categories"); cbCategory.SelectedIndex = 0;

            Label lblPrice = new Label { Text = "Price Range:", AutoSize = true, Location = new Point(220, 20), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbPriceRange = new Guna2ComboBox { Size = new Size(130, 36), Location = new Point(300, 12), BorderRadius = 4 };
            cbPriceRange.Items.Add("Any Price"); cbPriceRange.SelectedIndex = 0;

            Label lblStatus = new Label { Text = "Stock Status:", AutoSize = true, Location = new Point(440, 20), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            
            btnFilterAll = CreateFilterButton("ALL", 530, true);
            btnFilterIn = CreateFilterButton("IN STOCK", 575, false);
            btnFilterIn.Width = 90;
            btnFilterOut = CreateFilterButton("OUT", 670, false);
            btnFilterOut.Width = 60;
            
            btnExport = new Guna2Button { Text = "Export CSV", Size = new Size(100, 36), BorderRadius = 4, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnAdd = new Guna2Button { Text = "+ Add New Book", Size = new Size(130, 36), BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnAdd.Click += BtnAdd_Click;

            pnlFilters.Controls.AddRange(new Control[] { lblCat, cbCategory, lblPrice, cbPriceRange, lblStatus, btnFilterAll, btnFilterIn, btnFilterOut, btnExport, btnAdd });
            pnlFilters.Resize += (s, e) => {
                btnAdd.Location = new Point(pnlFilters.Width - 140, 12);
                btnExport.Location = new Point(pnlFilters.Width - 250, 12);
            };

            pnlGridContainer = new Guna2Panel { Dock = DockStyle.Top, Height = 360, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,0,gutter) };
            
            dgvBooks = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 50 },
                AutoGenerateColumns = false
            };
            
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Visible = false });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Isbn13", HeaderText = "ISBN-13", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Book Title", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, Width = 250 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Author", HeaderText = "Author", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "Category", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "Price", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            
            DataGridViewTextBoxColumn actionCol = new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "Actions", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
            dgvBooks.Columns.Add(actionCol);

            dgvBooks.CellPainting += DgvBooks_CellPainting;
            dgvBooks.CellMouseClick += DgvBooks_CellMouseClick;
            dgvBooks.CellFormatting += DgvBooks_CellFormatting;
            pnlGridContainer.Controls.Add(dgvBooks);

            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadData(); };
            pnlGridContainer.Controls.Add(paginationControl);

            tlpBento = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, Height = 200, 
                ColumnCount = 2, RowCount = 1,
                Margin = new Padding(0,0,0,gutter),
                BackColor = Color.Transparent
            };
            tlpBento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tlpBento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            pnlInsights = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,10,0) };
            Label lblInsightsTitle = new Label { Text = "Stock Insights", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20,15), AutoSize=true };
            lblInsightsTitle.Tag = "InsightsTitle";
            pnlInsights.Controls.Add(lblInsightsTitle);
            
            var pnlChart = new Panel { Location = new Point(20, 55), Size = new Size(220, 100) };
            pnlChart.Tag = "ChartBg";
            int[] barHeights = { 40, 70, 50, 90, 60, 40 };
            for (int i = 0; i < barHeights.Length; i++) {
                var bar = new Panel { 
                    Width = 20, Height = barHeights[i], 
                    Location = new Point(10 + i * 32, 100 - barHeights[i])
                };
                bar.Tag = (i == 3) ? "BarHigh" : "BarNormal";
                pnlChart.Controls.Add(bar);
            }
            Label lblChartSub = new Label { Text = "7-Day Sales Velocity", Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(60, 160) };
            lblChartSub.Tag = "ChartSubText";

            var pnlBars = new Panel { Location = new Point(300, 55), Size = new Size(300, 100) };
            pnlBars.Tag = "ProgressBarsContainer";
            
            Label lblCat1 = new Label { Text = "Top Category", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(0,0), Tag = "BarLabelText" };
            Label lblVal1 = new Label { Text = "Technology (34%)", Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(170, 0), Tag = "BarValueText" };
            var bar1Bg = new Panel { Size = new Size(290, 8), Location = new Point(0, 25), Tag = "BarBg" };
            var bar1Fg = new Panel { Size = new Size(100, 8), Location = new Point(0, 0), Tag = "Bar1Fg" };
            bar1Bg.Controls.Add(bar1Fg);
            
            Label lblCat2 = new Label { Text = "Inventory Turn Rate", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(0,60), Tag = "BarLabelText" };
            Label lblVal2 = new Label { Text = "High (4.2x)", Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(210, 60), Tag = "BarValueText" };
            var bar2Bg = new Panel { Size = new Size(290, 8), Location = new Point(0, 85), Tag = "BarBg" };
            var bar2Fg = new Panel { Size = new Size(220, 8), Location = new Point(0, 0), Tag = "Bar2Fg" };
            bar2Bg.Controls.Add(bar2Fg);

            pnlBars.Controls.AddRange(new Control[] { lblCat1, lblVal1, bar1Bg, lblCat2, lblVal2, bar2Bg });

            pnlInsights.Controls.Add(pnlChart);
            pnlInsights.Controls.Add(lblChartSub);
            pnlInsights.Controls.Add(pnlBars);
            pnlInsights.Resize += (s, e) => { pnlBars.Location = new Point(pnlInsights.Width - 340, 55); };

            pnlShortcuts = new Guna2Panel { Dock = DockStyle.Fill, Margin = new Padding(10,0,0,0) };
            Label lblShortcutsTitle = new Label { Text = "Quick Shortcuts", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20,15), AutoSize=true };
            
            var tlpBtns = new TableLayoutPanel { 
                Dock = DockStyle.Bottom, Height = 140, 
                ColumnCount = 2, RowCount = 2, 
                Padding = new Padding(15, 0, 15, 15),
                BackColor = Color.Transparent
            };
            tlpBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpBtns.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpBtns.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var btnScan = CreateShortcutButton("🔍 Scan ISBN");
            var btnCats = CreateShortcutButton("📁 Categories");
            var btnBulk = CreateShortcutButton("📋 Bulk Update");
            var btnPrint = CreateShortcutButton("🖨️ Print Labels");
            
            tlpBtns.Controls.Add(btnScan, 0, 0);
            tlpBtns.Controls.Add(btnCats, 1, 0);
            tlpBtns.Controls.Add(btnBulk, 0, 1);
            tlpBtns.Controls.Add(btnPrint, 1, 1);
            
            pnlShortcuts.Controls.Add(lblShortcutsTitle);
            pnlShortcuts.Controls.Add(tlpBtns);

            tlpBento.Controls.Add(pnlInsights, 0, 0);
            tlpBento.Controls.Add(pnlShortcuts, 1, 0);

            pnlContent.Controls.Add(tlpBento);
            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(pnlFilters);
            pnlContent.Controls.Add(tlpStats);
            pnlContent.Controls.Add(pnlPageHeader);
            
            pnlPageHeader.BringToFront();
            tlpStats.BringToFront();
            pnlFilters.BringToFront();
            pnlGridContainer.BringToFront();
            tlpBento.BringToFront();

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

        private Guna2Button CreateFilterButton(string text, int x, bool active)
        {
            var btn = new Guna2Button { Text = text, Location = new Point(x, 14), Size = new Size(45, 32), BorderRadius = 4, Font = new Font("Segoe UI", 8F, FontStyle.Bold), CustomBorderThickness = new Padding(1) };
            btn.Tag = active ? "ActiveFilter" : "InactiveFilter";
            btn.Click += FilterBtn_Click;
            return btn;
        }

        private Guna2Button CreateShortcutButton(string text)
        {
            return new Guna2Button { 
                Text = text, 
                Dock = DockStyle.Fill, 
                Margin = new Padding(5),
                BorderRadius = 4, 
                Font = new Font("Segoe UI", 8F, FontStyle.Bold), 
                FillColor = Color.FromArgb(20, 255,255,255),
                ForeColor = Color.White,
                BorderThickness = 1,
                BorderColor = Color.FromArgb(40, 255,255,255),
                Cursor = Cursors.Hand
            };
        }

        private void FilterBtn_Click(object sender, EventArgs e)
        {
            var btn = (Guna2Button)sender;
            btnFilterAll.Tag = "InactiveFilter";
            btnFilterIn.Tag = "InactiveFilter";
            btnFilterOut.Tag = "InactiveFilter";
            
            btn.Tag = "ActiveFilter";
            _stockFilter = btn.Text;
            
            ApplyThemeToFilterButton(btnFilterAll);
            ApplyThemeToFilterButton(btnFilterIn);
            ApplyThemeToFilterButton(btnFilterOut);
            
            _currentPage = 1;
            LoadData();
        }

        private void CatalogControl_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var frm = new Forms.BookForm(null);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }
        
        private void DgvBooks_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvBooks.Columns[e.ColumnIndex].Name == "Actions")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                var rect = e.CellBounds;
                var editRect = new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height);
                var delRect = new Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height);
                
                TextRenderer.DrawText(e.Graphics, "✏️", e.CellStyle.Font, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                TextRenderer.DrawText(e.Graphics, "🗑️", e.CellStyle.Font, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                
                e.Handled = true;
            }
        }

        private void DgvBooks_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvBooks.Columns[e.ColumnIndex].Name == "Actions")
            {
                int bookId = Convert.ToInt32(dgvBooks.Rows[e.RowIndex].Cells[0].Value);
                var cellRect = dgvBooks.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                
                if (e.X < cellRect.Width / 2)
                {
                    // Edit
                    var book = _bookService.GetById(bookId);
                    if (book != null)
                    {
                        var frm = new Forms.BookForm(book);
                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            LoadData();
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
                            LoadData();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void LoadData()
        {
            var stats = _catalogService.GetStats();
            
            foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = stats.TotalTitles.ToString("N0");
            foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = stats.ActiveCategories.ToString("N0");
            foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = $"${stats.InStockValue:N0}";
            foreach(Control c in card4.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = stats.LowStockAlerts.ToString();

            var (items, totalCount) = _catalogService.GetPagedCatalogBooks(_currentPage, _pageSize, _currentSearchTerm);
            dgvBooks.DataSource = items;
            
            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
            
            if (dgvBooks.Columns["Price"] != null) dgvBooks.Columns["Price"].DefaultCellStyle.Format = "C2";
        }

        private void DgvBooks_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvBooks.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                if (status == "IN STOCK") e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                else if (status == "OUT OF STOCK") e.CellStyle.ForeColor = ThemeManager.TextSecondary;
                else e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
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
            
            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;

            btnExport.FillColor = ThemeManager.CardBackground;
            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.BorderColor = ThemeManager.TextBoxBorder;

            ApplyThemeToCard(card1);
            ApplyThemeToCard(card2);
            ApplyThemeToCard(card3);
            ApplyThemeToCard(card4);

            pnlFilters.BackColor = ThemeManager.CardBackground;
            pnlFilters.CustomBorderColor = ThemeManager.TextBoxBorder;
            pnlFilters.FillColor = ThemeManager.CardBackground;
            
            cbCategory.FillColor = ThemeManager.TextBoxBackground;
            cbCategory.ForeColor = ThemeManager.TextPrimary;
            cbCategory.BorderColor = ThemeManager.TextBoxBorder;
            
            cbPriceRange.FillColor = ThemeManager.TextBoxBackground;
            cbPriceRange.ForeColor = ThemeManager.TextPrimary;
            cbPriceRange.BorderColor = ThemeManager.TextBoxBorder;
            
            foreach(Control c in pnlFilters.Controls) {
                if (c is Label l) l.ForeColor = ThemeManager.TextSecondary;
            }

            ApplyThemeToFilterButton(btnFilterAll);
            ApplyThemeToFilterButton(btnFilterIn);
            ApplyThemeToFilterButton(btnFilterOut);

            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            dgvBooks.BackgroundColor = ThemeManager.CardBackground;
            dgvBooks.GridColor = ThemeManager.TextBoxBorder;
            dgvBooks.BorderStyle = BorderStyle.None;
            dgvBooks.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            dgvBooks.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvBooks.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvBooks.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvBooks.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            
            dgvBooks.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvBooks.AlternatingRowsDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvBooks.AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvBooks.AlternatingRowsDefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            
            dgvBooks.EnableHeadersVisualStyles = false;
            dgvBooks.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvBooks.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvBooks.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Background;
            dgvBooks.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            pnlInsights.FillColor = ThemeManager.CardBackground;
            pnlInsights.CustomBorderColor = ThemeManager.TextBoxBorder;
            foreach(Control c in pnlInsights.Controls) {
                if (c.Tag?.ToString() == "InsightsTitle") c.ForeColor = ThemeManager.TextPrimary;
                if (c.Tag?.ToString() == "ChartBg") c.BackColor = ThemeManager.Background;
                if (c.Tag?.ToString() == "ChartSubText") c.ForeColor = ThemeManager.TextSecondary;
                
                if (c.Tag?.ToString() == "ProgressBarsContainer")
                {
                    foreach (Control child in c.Controls)
                    {
                        if (child.Tag?.ToString() == "BarLabelText") child.ForeColor = ThemeManager.TextSecondary;
                        if (child.Tag?.ToString() == "BarValueText") child.ForeColor = ThemeManager.TextPrimary;
                        if (child.Tag?.ToString() == "BarBg") child.BackColor = ThemeManager.TextBoxBackground;
                        
                        if (child is Panel barBg)
                        {
                            foreach(Control fg in barBg.Controls)
                            {
                                if (fg.Tag?.ToString() == "Bar1Fg") fg.BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(100, 150, 255) : Color.FromArgb(0, 36, 64);
                                if (fg.Tag?.ToString() == "Bar2Fg") fg.BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(80, 200, 120) : Color.FromArgb(17, 72, 38);
                            }
                        }
                    }
                }
            }
            
            foreach(Control c in pnlInsights.Controls) {
                if (c.Tag?.ToString() == "ChartBg") {
                    foreach(Control bar in c.Controls) {
                        if (bar.Tag?.ToString() == "BarNormal") bar.BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(80, 90, 100) : Color.FromArgb(200, 210, 220);
                        if (bar.Tag?.ToString() == "BarHigh") bar.BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(100, 150, 255) : Color.FromArgb(41, 128, 185);
                    }
                }
            }
            
            pnlShortcuts.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(15, 25, 35) : Color.FromArgb(0, 36, 64);
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
        
        private void ApplyThemeToFilterButton(Guna2Button btn)
        {
            if (btn.Tag?.ToString() == "ActiveFilter")
            {
                btn.FillColor = ThemeManager.CardBackground;
                btn.ForeColor = ThemeManager.ButtonFill; 
                btn.CustomBorderColor = ThemeManager.ButtonFill;
            }
            else
            {
                btn.FillColor = ThemeManager.HoverColor;
                btn.ForeColor = ThemeManager.TextSecondary;
                btn.CustomBorderColor = ThemeManager.TextBoxBorder;
            }
        }
    }
}
