using BookStoreManagement.Helpers;
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
        private readonly CategoryService _categoryService;
        
        private Guna2Panel pnlContent;
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubtitle;

        private TableLayoutPanel tlpStats;
        private Guna2Panel card1, card2, card3;
        
        private Guna2Panel pnlFilters;
        private Guna2TextBox txtSearch;
        private Guna2ComboBox cbCategory;
        private Guna2ComboBox cbStockStatus;
        private Guna2Button btnExport;
        private Guna2Button btnAdd;

        private Guna2Panel pnlGridContainer;
        private Guna2DataGridView dgvBooks;
        
        private PaginationControl paginationControl;
        
        private int _currentPage = 1;
        private int _pageSize = 10;
        private bool _isCalculatingPageSize = false;
        private string _stockFilter = "";
        private int? _categoryFilter = null;
        private string _currentSearchTerm = "";

        private int _hoveredRowIndex = -1;
        private int _hoveredAction = 0;

        public async void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            await LoadDataAsync();
        }

        public CatalogControl()
        {
            _catalogService = new CatalogService();
            _bookService = new BookService();
            _categoryService = new CategoryService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += CatalogControl_Load;
        }

        private void DgvBooks_Resize(object sender, EventArgs e)
        {
            if (_isCalculatingPageSize) return;
            _isCalculatingPageSize = true;
            
            if (dgvBooks.Height > 0)
            {
                int availableHeight = dgvBooks.Height - dgvBooks.ColumnHeadersHeight;
                int newPageSize = availableHeight / dgvBooks.RowTemplate.Height;
                if (newPageSize < 1) newPageSize = 1;

                if (_pageSize != newPageSize)
                {
                    _pageSize = newPageSize;
                    _currentPage = 1;
                    _ = LoadDataAsync();
                }
            }
            _isCalculatingPageSize = false;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 20;
            this.Padding = new Padding(0);

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = false };

            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(0,0,0,10) };
            lblTitle = new Label { Text = "Quản lý Sách", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubtitle = new Label { Text = "Quản lý sách, tồn kho, và thông tin sách.", Font = new Font("Segoe UI", 11F), AutoSize = true, Location = new Point(0, 45) };
            pnlPageHeader.Controls.Add(lblTitle);
            pnlPageHeader.Controls.Add(lblSubtitle);

            tlpStats = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, Height = 100, 
                ColumnCount = 3, RowCount = 1,
                Margin = new Padding(0,0,0,gutter)
            };
            for(int i=0; i<3; i++) tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            
            card1 = CreateStatCard("TỔNG SỐ SÁCH", "0", "library_books", Color.FromArgb(41, 128, 185)); // Primary
            card2 = CreateStatCard("SẮP HẾT / HẾT HÀNG", "0", "warning", Color.FromArgb(186, 26, 26)); // Error
            card3 = CreateStatCard("SÁCH MỚI NHẬP", "0", "new_releases", Color.FromArgb(0, 186, 97)); // Tertiary
            
            tlpStats.Controls.Add(card1, 0, 0);
            tlpStats.Controls.Add(card2, 1, 0);
            tlpStats.Controls.Add(card3, 2, 0);

            pnlFilters = new Guna2Panel { Dock = DockStyle.Top, Height = 70, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,0,gutter), BorderRadius = 8 };
            
            txtSearch = new Guna2TextBox { Size = new Size(240, 36), Location = new Point(20, 16), BorderRadius = 8, PlaceholderText = "Tìm kiếm ISBN, Tên sách..." };
            txtSearch.TextChanged += async (s, e) => { _currentSearchTerm = txtSearch.Text; _currentPage = 1; await LoadDataAsync(); };

            cbCategory = new Guna2ComboBox { Size = new Size(160, 36), Location = new Point(280, 16), BorderRadius = 8, Cursor = Cursors.Hand };
            cbCategory.Items.Add(new { Text = "Tất cả Danh mục", Value = (int?)null }); cbCategory.SelectedIndex = 0;
            cbCategory.SelectedIndexChanged += async (s, e) => {
                _categoryFilter = cbCategory.SelectedIndex > 0 ? (int?)((dynamic)cbCategory.SelectedItem).Value : null;
                _currentPage = 1;
                await LoadDataAsync();
            };

            cbStockStatus = new Guna2ComboBox { Size = new Size(160, 36), Location = new Point(460, 16), BorderRadius = 8, Cursor = Cursors.Hand };
            cbStockStatus.Items.Add(new { Text = "Tất cả Trạng thái", Value = "" });
            cbStockStatus.Items.Add(new { Text = "Đủ hàng", Value = "instock" });
            cbStockStatus.Items.Add(new { Text = "Hết hàng", Value = "outstock" });
            cbStockStatus.Items.Add(new { Text = "Đã khóa", Value = "locked" });
            cbStockStatus.DisplayMember = "Text";
            cbStockStatus.ValueMember = "Value";
            cbStockStatus.SelectedIndex = 0;
            cbStockStatus.SelectedIndexChanged += async (s, e) => {
                _stockFilter = ((dynamic)cbStockStatus.SelectedItem).Value;
                _currentPage = 1;
                await LoadDataAsync();
            };

            btnExport = new Guna2Button { Text = "Xuất Excel", Size = new Size(120, 36), BorderRadius = 8, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnExport.Click += BtnExport_Click;

            btnAdd = new Guna2Button { Text = "+ Thêm Mới", Size = new Size(120, 36), BorderRadius = 8, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.Click += BtnAdd_Click;

            pnlFilters.Controls.AddRange(new Control[] { txtSearch, cbCategory, cbStockStatus, btnExport, btnAdd });
            pnlFilters.Resize += (s, e) => {
                btnAdd.Location = new Point(pnlFilters.Width - 140, 16);
                btnExport.Location = new Point(pnlFilters.Width - 270, 16);
            };

            pnlGridContainer = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), BorderRadius = 8 };
            
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
                AutoGenerateColumns = false,
                AlternatingRowsDefaultCellStyle = { BackColor = Color.Empty }, // Disable alternating colors
                GridColor = Color.LightGray // Outline variant
            };
            dgvBooks.SetDoubleBuffered(true);
            
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Visible = false });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "#", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, Width = 50 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Isbn13", HeaderText = "ISBN", Width = 150 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "TÊN SÁCH", Width = 250 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Author", HeaderText = "TÁC GIẢ", Width = 150 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "DANH MỤC", Width = 120 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "ĐƠN GIÁ", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleRight } }, Width = 120 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TRẠNG THÁI", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, Width = 120 });
            
            DataGridViewTextBoxColumn actionCol = new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "THAO TÁC", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
            dgvBooks.Columns.Add(actionCol);

            dgvBooks.CellPainting += DgvBooks_CellPainting;
            dgvBooks.CellMouseClick += DgvBooks_CellMouseClick;
            dgvBooks.CellFormatting += DgvBooks_CellFormatting;
            dgvBooks.CellMouseMove += DgvBooks_CellMouseMove;
            dgvBooks.CellMouseLeave += DgvBooks_CellMouseLeave;
            dgvBooks.Resize += DgvBooks_Resize;
            pnlGridContainer.Controls.Add(dgvBooks);

            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += async (s, e) => { _currentPage = e.NewPage; await LoadDataAsync(); };
            pnlGridContainer.Controls.Add(paginationControl);

            Panel spacer1 = new Panel { Dock = DockStyle.Top, Height = gutter, BackColor = Color.Transparent };
            Panel spacer2 = new Panel { Dock = DockStyle.Top, Height = gutter, BackColor = Color.Transparent };
            Panel spacer3 = new Panel { Dock = DockStyle.Top, Height = gutter, BackColor = Color.Transparent };

            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(spacer3);
            pnlContent.Controls.Add(pnlFilters);
            pnlContent.Controls.Add(spacer2);
            pnlContent.Controls.Add(tlpStats);
            pnlContent.Controls.Add(spacer1);
            pnlContent.Controls.Add(pnlPageHeader);
            
            pnlPageHeader.BringToFront();
            spacer1.BringToFront();
            tlpStats.BringToFront();
            spacer2.BringToFront();
            pnlFilters.BringToFront();
            spacer3.BringToFront();
            pnlGridContainer.BringToFront();

            this.Controls.Add(pnlContent);

            ApplyTheme();
        }

        private Guna2Panel CreateStatCard(string title, string value, string iconText, Color color)
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,20,0), BorderRadius = 12 };
            
            // Icon
            var pnlIcon = new Guna2Panel { Size = new Size(48, 48), Location = new Point(20, 26), BorderRadius = 24, FillColor = Color.FromArgb(30, color) };
            Label lblIcon = new Label { Text = iconText == "library_books" ? "📖" : (iconText == "warning" ? "⚠️" : "✨"), Font = new Font("Segoe UI Emoji", 16F), AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            pnlIcon.Controls.Add(lblIcon);
            
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(80, 26) };
            lblTitle.Tag = "CardTitle";
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(78, 45), ForeColor = color };
            lblValue.Tag = "CardValue";

            pnl.Controls.Add(pnlIcon);
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);
            return pnl;
        }

        private async void CatalogControl_Load(object sender, EventArgs e)
        {
            await LoadCategoriesAsync();
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadCategoriesAsync()
        {
            var cats = await _categoryService.GetAllAsync();
            cbCategory.Items.Clear();
            cbCategory.Items.Add(new { Text = "Tất cả Danh mục", Value = (int?)null });
            foreach (var c in cats)
            {
                if (c.IsActive)
                    cbCategory.Items.Add(new { Text = c.CategoryName, Value = (int?)c.Id });
            }
            cbCategory.DisplayMember = "Text";
            cbCategory.ValueMember = "Value";
            cbCategory.SelectedIndex = 0;
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            var frm = new Forms.BookForm(null);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                await LoadDataAsync();
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx", FileName = "DanhSachSach.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var excelService = new ExcelExportService();
                        excelService.ExportDataGridView(dgvBooks, sfd.FileName, "Sách");
                        MessageBox.Show("Xuất file Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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

        private void DgvBooks_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvBooks.Columns[e.ColumnIndex].Name == "Actions")
            {
                int action = (e.X < dgvBooks.Columns[e.ColumnIndex].Width / 2) ? 1 : 2;
                
                if (_hoveredRowIndex != e.RowIndex || _hoveredAction != action)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = e.RowIndex;
                    _hoveredAction = action;
                    
                    if (oldRow >= 0) dgvBooks.InvalidateCell(e.ColumnIndex, oldRow);
                    dgvBooks.InvalidateCell(e.ColumnIndex, _hoveredRowIndex);
                }
                dgvBooks.Cursor = Cursors.Hand;
            }
            else
            {
                if (_hoveredRowIndex >= 0)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = -1;
                    _hoveredAction = 0;
                    if (e.ColumnIndex >= 0) dgvBooks.InvalidateCell(dgvBooks.Columns["Actions"].Index, oldRow);
                }
                dgvBooks.Cursor = Cursors.Default;
            }
        }

        private void DgvBooks_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredRowIndex >= 0)
            {
                int oldRow = _hoveredRowIndex;
                _hoveredRowIndex = -1;
                _hoveredAction = 0;
                dgvBooks.InvalidateCell(dgvBooks.Columns["Actions"].Index, oldRow);
            }
            dgvBooks.Cursor = Cursors.Default;
        }

        private async void DgvBooks_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvBooks.Columns[e.ColumnIndex].Name == "Actions")
            {
                int bookId = Convert.ToInt32(dgvBooks.Rows[e.RowIndex].Cells[0].Value);
                
                if (e.X < dgvBooks.Columns[e.ColumnIndex].Width / 2)
                {
                    // Edit
                    var book = await _bookService.GetByIdAsync(bookId);
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
                        MessageBox.Show("Không tìm thấy sách.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Delete
                    if (MessageBox.Show("Bạn có chắc chắn muốn xóa cuốn sách này không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            await _bookService.SetActiveAsync(bookId, false);
                            MessageBox.Show("Đã xóa sách thành công!");
                            await LoadDataAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            var stats = await _catalogService.GetStatsAsync();
            
            foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = stats.TotalTitles.ToString("N0");
            foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = stats.ActiveCategories.ToString("N0"); // Sắp hết
            foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = stats.LowStockAlerts.ToString("N0"); // Mới nhập

            var (items, totalCount) = await _catalogService.GetPagedCatalogBooksAsync(_currentPage, _pageSize, _currentSearchTerm, _categoryFilter, _stockFilter);
            
            dgvBooks.DataSource = items;
            
            // Set Row index
            int index = (_currentPage - 1) * _pageSize + 1;
            foreach (DataGridViewRow row in dgvBooks.Rows)
            {
                row.Cells[1].Value = index++;
            }
            
            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
        }

        private void DgvBooks_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvBooks.Columns[e.ColumnIndex].Name == "Price" && e.Value != null)
            {
                e.Value = string.Format("{0:N0} ₫", e.Value);
                e.FormattingApplied = true;
            }

            if (dgvBooks.Columns[e.ColumnIndex].Name == "Trạng thái" && e.Value != null)
            {
                string status = e.Value.ToString();
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                if (status == "IN STOCK") e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                else if (status == "ĐÃ KHÓA") e.CellStyle.ForeColor = ThemeManager.TextSecondary;
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
            lblSubtitle.ForeColor = ThemeManager.TextSecondary;
            
            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;

            btnExport.FillColor = ThemeManager.CardBackground;
            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.BorderColor = ThemeManager.TextBoxBorder;

            ApplyThemeToCard(card1);
            ApplyThemeToCard(card2);
            ApplyThemeToCard(card3);

            pnlFilters.BackColor = ThemeManager.CardBackground;
            pnlFilters.CustomBorderColor = ThemeManager.TextBoxBorder;
            pnlFilters.FillColor = ThemeManager.CardBackground;
            
            txtSearch.FillColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;
            txtSearch.BorderColor = ThemeManager.TextBoxBorder;

            cbCategory.FillColor = ThemeManager.TextBoxBackground;
            cbCategory.ForeColor = ThemeManager.TextPrimary;
            cbCategory.BorderColor = ThemeManager.TextBoxBorder;
            
            cbStockStatus.FillColor = ThemeManager.TextBoxBackground;
            cbStockStatus.ForeColor = ThemeManager.TextPrimary;
            cbStockStatus.BorderColor = ThemeManager.TextBoxBorder;

            pnlGridContainer.BackColor = ThemeManager.CardBackground;
            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            pnlGridContainer.FillColor = ThemeManager.CardBackground;

            ThemeManager.ApplyDataGridViewStyle(dgvBooks);
        }

        private void ApplyThemeToCard(Guna2Panel card)
        {
            card.BackColor = ThemeManager.CardBackground;
            card.CustomBorderColor = ThemeManager.TextBoxBorder;
            card.FillColor = ThemeManager.CardBackground;
            foreach (Control c in card.Controls)
            {
                if (c.Tag?.ToString() == "CardTitle") c.ForeColor = ThemeManager.TextSecondary;
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

