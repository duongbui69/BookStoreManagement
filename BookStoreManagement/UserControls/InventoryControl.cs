using BookStoreManagement.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.Interfaces;
using Guna.UI2.WinForms;

namespace BookStoreManagement.UserControls
{
    public partial class InventoryControl : UserControl, BookStoreManagement.Interfaces.IRefreshable, ISearchableControl
    {
        private readonly InventoryService _inventoryService;
        private readonly CategoryService _categoryService;
        private readonly StoreService _storeService;

        private Guna2Panel pnlContent;
        
        // Page Header
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        private Label lblLastUpdated;
        
        // KPI Cards
        private Guna2Panel pnlKpiContainer;
        private Guna2Panel cardTotalStock;
        private Label lblTotalStockTitle;
        private Label lblTotalStockValue;
        
        private Guna2Panel cardCriticalStock;
        private Label lblCriticalStockTitle;
        private Label lblCriticalStockValue;
        
        private Guna2Panel cardStockValue;
        private Label lblStockValueTitle;
        private Label lblStockValue;
        
        // Toolbar
        private Guna2Panel pnlToolbar;
        private Guna2TextBox txtSearch;
        private Guna2ComboBox cbWarehouse;
        private Guna2ComboBox cbCategory;
        private Guna2Button btnExport;
        
        // Grid
        private Guna2Panel pnlGridContainer;
        private DataGridView dgvInventory;
        private PaginationControl paginationControl;
        
        private int _currentPage = 1;
        private int _pageSize = 10;
        private string _currentSearchTerm = "";
        
        private bool _isCalculatingPageSize = false;
        private System.Windows.Forms.Timer _resizeTimer;
        
        private int _hoveredRowIndex = -1;
        private int _hoveredAction = 0; // 1 = View/Edit, 2 = Delete

        public void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            txtSearch.Text = keyword;
            _currentPage = 1;
            LoadData();
        }

        public InventoryControl()
        {
            _resizeTimer = new System.Windows.Forms.Timer { Interval = 150 };
            _resizeTimer.Tick += ResizeTimer_Tick;

            _inventoryService = new InventoryService();
            _categoryService = new CategoryService();
            _storeService = new StoreService();
            
            InitializeUI();
            ApplyTheme();
            
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            // Load is deferred to RefreshDataAsync
        }
        
        public async Task RefreshDataAsync()
        {
            if (cbCategory.Items.Count == 0 || cbWarehouse.Items.Count == 0)
            {
                await LoadFiltersAsync();
            }
            LoadData(); // LoadData is async void, we can await it if we change it or just leave it
            await Task.CompletedTask;
        }

        private async System.Threading.Tasks.Task LoadFiltersAsync()
        {
            var categories = await _categoryService.GetActiveAsync();
            categories.Insert(0, new Models.Category { Id = 0, CategoryName = "Tất cả Danh mục" });
            cbCategory.DataSource = categories;
            cbCategory.DisplayMember = "CategoryName";
            cbCategory.ValueMember = "CategoryName";
            
            var stores = await _storeService.GetActiveAsync();
            stores.Insert(0, new Models.Store { Id = 0, StoreName = "Cửa hàng: Tất cả" });
            cbWarehouse.DataSource = stores;
            cbWarehouse.DisplayMember = "StoreName";
            cbWarehouse.ValueMember = "StoreName";
            
            cbWarehouse.SelectedIndexChanged += (s, e) => { _currentPage = 1; LoadData(); };
            cbCategory.SelectedIndexChanged += (s, e) => { _currentPage = 1; LoadData(); };
            txtSearch.TextChanged += (s, e) => { _currentSearchTerm = txtSearch.Text; _currentPage = 1; LoadData(); };
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ThemeManager.Background;
            
            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            
            // 1. Header
            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 76 };
            lblTitle = new Label { Text = "Thống kê kho", Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Tổng quan và phân tích dữ liệu kho hiện tại.", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(2, 44) };
            lblLastUpdated = new Label { Text = $"🕒 Cập nhật lần cuối: Hôm nay, {DateTime.Now:hh:mm tt}", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(700, 20) };
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle, lblLastUpdated });
            
            // Spacer between header and KPI cards
            var spacerKpi = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Color.Transparent };

            // 2. KPI Cards
            pnlKpiContainer = new Guna2Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(0) };
            
            cardTotalStock = CreateKpiCard("Tổng tồn kho", "0", 0);
            cardCriticalStock = CreateKpiCard("Sách sắp hết (Critical)", "0", 1);
            cardStockValue = CreateKpiCard("Giá trị tồn kho", "0 ₫", 2);
            
            pnlKpiContainer.Controls.AddRange(new Control[] { cardTotalStock, cardCriticalStock, cardStockValue });
            // set card heights properly inside the container
            pnlKpiContainer.Resize += (s, e) => 
            {
                if (pnlKpiContainer.Width <= 0) return; // Guard: tránh set size âm khi chưa layout
                int cardWidth = (pnlKpiContainer.Width - 40) / 3;
                int cardH = pnlKpiContainer.Height;
                cardTotalStock.Width = cardWidth; cardTotalStock.Height = cardH; cardTotalStock.Left = 0; cardTotalStock.Top = 0;
                cardCriticalStock.Width = cardWidth; cardCriticalStock.Height = cardH; cardCriticalStock.Left = cardWidth + 20; cardCriticalStock.Top = 0;
                cardStockValue.Width = cardWidth; cardStockValue.Height = cardH; cardStockValue.Left = (cardWidth + 20) * 2; cardStockValue.Top = 0;
            };
            
            // 3. Toolbar (Filters)
            pnlToolbar = new Guna2Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(0, 15, 0, 15) };
            
            txtSearch = new Guna2TextBox { PlaceholderText = "Tìm sản phẩm (SKU, Tên)...", Size = new Size(250, 40), Location = new Point(0, 15), BorderRadius = 4, Font = new Font("Segoe UI", 11F) };
            cbWarehouse = new Guna2ComboBox { Size = new Size(200, 40), Location = new Point(270, 15), BorderRadius = 4, Font = new Font("Segoe UI", 11F) };
            cbCategory = new Guna2ComboBox { Size = new Size(200, 40), Location = new Point(490, 15), BorderRadius = 4, Font = new Font("Segoe UI", 11F) };
            
            btnExport = new Guna2Button { Text = "Xuất Excel", Size = new Size(120, 36), BorderRadius = 8, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            
            btnExport.Click += BtnExport_Click;

            pnlToolbar.Controls.AddRange(new Control[] { txtSearch, cbWarehouse, cbCategory, btnExport });
            pnlToolbar.Resize += (s, e) => {
                btnExport.Location = new Point(pnlToolbar.Width - 140, 17);
            };

            // 4. Grid Container
            pnlGridContainer = new Guna2Panel { Dock = DockStyle.Fill, BorderRadius = 8, BorderThickness = 1 };
            
            dgvInventory = new DataGridView
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, // Allows horizontal scrolling
                RowTemplate = { Height = 55 },
                EnableHeadersVisualStyles = false,
                ScrollBars = ScrollBars.Both // Both scrollbars enabled
            };
            dgvInventory.SetDoubleBuffered(true);
            
            SetupGridColumns();
            
            dgvInventory.CellPainting += DgvInventory_CellPainting;
            dgvInventory.CellMouseMove += DgvInventory_CellMouseMove;
            dgvInventory.CellMouseLeave += DgvInventory_CellMouseLeave;
            dgvInventory.CellMouseClick += DgvInventory_CellMouseClick;
            
            pnlGridContainer.Controls.Add(dgvInventory);
            
            // Pagination
            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadData(); };
            pnlGridContainer.Controls.Add(paginationControl);
            
            dgvInventory.Resize += DgvInventory_Resize;

            // Đảm bảo Z-order chuẩn (Top to Bottom, then Fill)
            pnlContent.Controls.AddRange(new Control[] {
                pnlPageHeader, pnlKpiContainer, spacerKpi, pnlToolbar, pnlGridContainer
            });

            pnlPageHeader.BringToFront();
            pnlKpiContainer.BringToFront();
            spacerKpi.BringToFront();
            pnlToolbar.BringToFront();
            pnlGridContainer.BringToFront();
            
            this.Controls.Add(pnlContent);
        }
        
        private Guna2Panel CreateKpiCard(string title, string value, int type)
        {
            var card = new Guna2Panel { BorderRadius = 10, BorderThickness = 1 };
            var lblT = new Label { Text = title.ToUpper(), Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 14) };
            var lblV = new Label { Text = value, Font = new Font("Segoe UI", 22F, FontStyle.Bold), AutoSize = true, Location = new Point(18, 32) };
            card.Controls.AddRange(new Control[] { lblT, lblV });
            
            // Add a reference to the value label so we can update it
            if (type == 0) { lblTotalStockTitle = lblT; lblTotalStockValue = lblV; }
            else if (type == 1) { lblCriticalStockTitle = lblT; lblCriticalStockValue = lblV; }
            else { lblStockValueTitle = lblT; lblStockValue = lblV; }
            
            return card;
        }

        private void SetupGridColumns()
        {
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "BookId", Visible = false });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Index", HeaderText = "#", Width = 50, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sku", HeaderText = "SKU", Width = 120 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tiêu đề", HeaderText = "Tên sách", Width = 280 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kho hàng", HeaderText = "Kho", Width = 150 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentStock", HeaderText = "Tồn kho hiện tại", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 11, FontStyle.Bold) } });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "MinStock", HeaderText = "Tồn tối thiểu", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Danh mục", HeaderText = "Danh mục", Width = 150 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tác giả", HeaderText = "Tác giả", Width = 150 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nhà xuất bản", HeaderText = "Nhà xuất bản", Width = 150 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Giá bán", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Trạng thái", HeaderText = "Trạng thái", Width = 140, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "Thao tác", Width = 100, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            
            foreach (DataGridViewColumn col in dgvInventory.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void DgvInventory_Resize(object sender, EventArgs e)
        {
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();
            if (this.IsDisposed) return;
            
            if (dgvInventory.Height > 0)
            {
                int availableHeight = dgvInventory.Height - dgvInventory.ColumnHeadersHeight;
                int newPageSize = availableHeight / dgvInventory.RowTemplate.Height;
                if (newPageSize < 1) newPageSize = 1;

                if (_pageSize != newPageSize)
                {
                    _pageSize = newPageSize;
                    _currentPage = 1;
                    LoadData();
                }
            }
        }

        private async void LoadData()
        {
            if (this.IsDisposed) return;
            // Load KPIs
            var stats = await _inventoryService.GetStatsAsync();
            if (this.IsDisposed) return;
            lblTotalStockValue.Text = stats.TotalItems.ToString("N0");
            lblCriticalStockValue.Text = stats.OutOfStock.ToString("N0");
            lblStockValue.Text = stats.StockValue.ToString("N0") + " ₫";
            
            // Load Grid Data
            string wFilter = cbWarehouse.SelectedIndex > 0 ? cbWarehouse.SelectedValue.ToString() : "";
            string cFilter = cbCategory.SelectedIndex > 0 ? cbCategory.SelectedValue.ToString() : "";
            
            var result = await _inventoryService.GetPagedInventoryItemsAsync(_currentPage, _pageSize, _currentSearchTerm, wFilter, cFilter);
            
            if (this.IsDisposed) return;
            dgvInventory.Rows.Clear();
            int index = (_currentPage - 1) * _pageSize + 1;
            foreach (var item in result.Items)
            {
                dgvInventory.Rows.Add(
                    item.BookId,
                    index++,
                    item.Sku,
                    item.Title,
                    item.Warehouse,
                    item.CurrentStock,
                    item.MinStock,
                    item.CategoryName,
                    item.AuthorName,
                    item.PublisherName,
                    item.SellingPrice.ToString("N0") + " ₫",
                    item.Status,
                    ""
                );
            }
            
            paginationControl.UpdatePagination(result.TotalCount, _currentPage, _pageSize);
            lblLastUpdated.Text = $"🕒 Cập nhật lần cuối: Hôm nay, {DateTime.Now:hh:mm tt}";
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx", FileName = "ThongKeTonKho.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var excelService = new BookStoreManagement.Services.ExcelExportService();
                        excelService.ExportDataGridView(dgvInventory, sfd.FileName, "Tồn Kho");
                        MessageBox.Show("Xuất file Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvInventory_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvInventory.Columns[e.ColumnIndex].Name == "Actions")
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

                int editFontSize = (_hoveredRowIndex == e.RowIndex && _hoveredAction == 1) ? 16 : 14;
                int delFontSize = (_hoveredRowIndex == e.RowIndex && _hoveredAction == 2) ? 16 : 14;

                using (var font = new Font("Segoe UI Emoji", editFontSize))
                {
                TextRenderer.DrawText(e.Graphics, "✏️", font, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                using (var font = new Font("Segoe UI Emoji", delFontSize))
                {
                    TextRenderer.DrawText(e.Graphics, "🗑️", font, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                
                e.Handled = true;
            }
            else if (dgvInventory.Columns[e.ColumnIndex].Name == "Trạng thái")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                string status = e.FormattedValue?.ToString() ?? "";
                
                Color bgColor, textColor;
                if (status == "Đủ hàng") { bgColor = Color.FromArgb(210, 248, 226); textColor = Color.FromArgb(0, 100, 40); }
                else if (status == "Hết hàng") { bgColor = Color.FromArgb(250, 200, 200); textColor = Color.FromArgb(180, 0, 0); }
                else { bgColor = Color.FromArgb(255, 230, 200); textColor = Color.FromArgb(180, 100, 0); }
                
                Size size;
                using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
                {
                size = TextRenderer.MeasureText(status, font);
                }
                var rect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width - size.Width - 16) / 2, e.CellBounds.Y + (e.CellBounds.Height - size.Height - 8) / 2, size.Width + 16, size.Height + 8);
                
                using (var path = new GraphicsPath())
                {
                    int r = rect.Height;
                    path.AddArc(rect.X, rect.Y, r, r, 180, 90);
                    path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
                    path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
                    path.CloseFigure();
                    
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var brush = new SolidBrush(bgColor)) e.Graphics.FillPath(brush, path);
                    using (var pen = new Pen(textColor, 1)) e.Graphics.DrawPath(pen, path);
                }
                
                using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
                {
                TextRenderer.DrawText(e.Graphics, status, font, rect, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                e.Handled = true;
            }
        }

        private void DgvInventory_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvInventory.Columns[e.ColumnIndex].Name == "Actions")
            {
                int action = (e.X < dgvInventory.Columns[e.ColumnIndex].Width / 2) ? 1 : 2;
                if (_hoveredRowIndex != e.RowIndex || _hoveredAction != action)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = e.RowIndex;
                    _hoveredAction = action;
                    if (oldRow >= 0) dgvInventory.InvalidateCell(e.ColumnIndex, oldRow);
                    dgvInventory.InvalidateCell(e.ColumnIndex, _hoveredRowIndex);
                }
                dgvInventory.Cursor = Cursors.Hand;
            }
            else
            {
                if (_hoveredRowIndex >= 0)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = -1;
                    _hoveredAction = 0;
                    if (e.ColumnIndex >= 0) dgvInventory.InvalidateCell(dgvInventory.Columns["Actions"].Index, oldRow);
                }
                dgvInventory.Cursor = Cursors.Default;
            }
        }

        private void DgvInventory_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredRowIndex >= 0)
            {
                int oldRow = _hoveredRowIndex;
                _hoveredRowIndex = -1;
                _hoveredAction = 0;
                dgvInventory.InvalidateCell(dgvInventory.Columns["Actions"].Index, oldRow);
            }
            dgvInventory.Cursor = Cursors.Default;
        }

        private void DgvInventory_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvInventory.Columns[e.ColumnIndex].Name == "Actions")
            {
                int bookId = Convert.ToInt32(dgvInventory.Rows[e.RowIndex].Cells[0].Value);
                string warehouse = dgvInventory.Rows[e.RowIndex].Cells["Kho hàng"].Value.ToString();
                
                if (e.X < dgvInventory.Columns[e.ColumnIndex].Width / 2)
                {
                    // Edit
                    int qty = Convert.ToInt32(dgvInventory.Rows[e.RowIndex].Cells["CurrentStock"].Value);
                    int minStock = Convert.ToInt32(dgvInventory.Rows[e.RowIndex].Cells["MinStock"].Value);
                    
                    var frm = new Forms.InventoryEditForm(bookId, warehouse, qty, minStock);
                    if (frm.ShowDialog() == DialogResult.OK) LoadData();
                }
                else
                {
                    // Delete
                    if (MessageBox.Show("Bạn có chắc chắn muốn xóa tồn kho của sản phẩm này không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        try
                        {
                            _inventoryService.DeleteStock(bookId, warehouse);
                            LoadData();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e) => ApplyTheme();

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlContent.FillColor = ThemeManager.Background;

            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            lblLastUpdated.ForeColor = ThemeManager.TextSecondary;
            
            // Cards
            cardTotalStock.BackColor = Color.Transparent;
            cardTotalStock.FillColor = ThemeManager.CardBackground;
            cardTotalStock.BorderColor = ThemeManager.TextBoxBorder;
            lblTotalStockTitle.ForeColor = ThemeManager.TextSecondary;
            lblTotalStockValue.ForeColor = Color.FromArgb(41, 128, 185);
            
            cardCriticalStock.BackColor = Color.Transparent;
            cardCriticalStock.FillColor = ThemeManager.CardBackground;
            cardCriticalStock.BorderColor = ThemeManager.TextBoxBorder;
            lblCriticalStockTitle.ForeColor = ThemeManager.TextSecondary;
            lblCriticalStockValue.ForeColor = Color.FromArgb(231, 76, 60);
            
            cardStockValue.BackColor = Color.Transparent;
            cardStockValue.FillColor = ThemeManager.CardBackground;
            cardStockValue.BorderColor = ThemeManager.TextBoxBorder;
            lblStockValueTitle.ForeColor = ThemeManager.TextSecondary;
            lblStockValue.ForeColor = Color.FromArgb(0, 186, 97);
            
            // Toolbar
            pnlToolbar.FillColor = ThemeManager.Background;
            
            txtSearch.FillColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;
            txtSearch.BorderColor = ThemeManager.TextBoxBorder;
            
            cbWarehouse.FillColor = ThemeManager.TextBoxBackground;
            cbWarehouse.ForeColor = ThemeManager.TextPrimary;
            cbWarehouse.BorderColor = ThemeManager.TextBoxBorder;

            cbCategory.FillColor = ThemeManager.TextBoxBackground;
            cbCategory.ForeColor = ThemeManager.TextPrimary;
            cbCategory.BorderColor = ThemeManager.TextBoxBorder;
            
            btnExport.FillColor = ThemeManager.CardBackground;
            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.BorderColor = ThemeManager.TextBoxBorder;

            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            pnlGridContainer.FillColor = ThemeManager.CardBackground;
            
            ThemeManager.ApplyDataGridViewStyle(dgvInventory);
        }
    }
}
