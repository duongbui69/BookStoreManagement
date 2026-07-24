using BookStoreManagement.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using BookStoreManagement.Forms;
using Guna.UI2.WinForms;
using System.Collections.Generic;
using BookStoreManagement.Interfaces;

namespace BookStoreManagement.UserControls
{
    public partial class CategoryControl : UserControl, ISearchableControl
    {
        private readonly CategoryService _categoryService;
        
        private Guna2Panel pnlContent;
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Guna2Button btnExport;
        private Guna2Button btnAdd;

        private Guna2Panel pnlFilters;
        private Label lblSearchTitle;
        private Guna2TextBox txtSearch;
        private Label lblStatusTitle;
        private Guna2ComboBox cbStatus;
        private Guna2Button btnFilter;

        private Guna2Panel pnlGridContainer;
        private Guna2DataGridView dgvCategories;
        
        private PaginationControl paginationControl;
        
        private int _currentPage = 1;
        private int _pageSize = 5; 
        private string _statusFilter = "";
        private string _currentSearchTerm = "";
        
        private int _hoveredRowIndex = -1;
        private int _hoveredAction = 0;

        public void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            txtSearch.Text = keyword;
            _currentPage = 1;
            LoadData();
        }

        public CategoryControl()
        {
            _categoryService = new CategoryService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += CategoryControl_Load;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 20;
            this.Padding = new Padding(0);

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = true };

            // Page Header
            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0, 0, 0, gutter) };
            
            lblTitle = new Label { Text = "Danh mục sách", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubtitle = new Label { Text = "Quản lý và phân loại các đầu sách trong hệ thống.", Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(0, 40) };
            
            btnExport = new Guna2Button { Text = "Xuất Excel", Size = new Size(120, 36), BorderRadius = 4, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnExport.Click += BtnExport_Click;

            btnAdd = new Guna2Button { Text = "+ Thêm mới", Size = new Size(120, 36), BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.Click += BtnAdd_Click;

            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, btnExport, btnAdd });
            pnlPageHeader.Resize += (s, e) => {
                btnAdd.Location = new Point(pnlPageHeader.Width - 120, 10);
                btnExport.Location = new Point(pnlPageHeader.Width - 250, 10);
            };

            // Filter Bar
            pnlFilters = new Guna2Panel { Dock = DockStyle.Top, Height = 90, CustomBorderThickness = new Padding(1), Margin = new Padding(0, 0, 0, gutter), BorderRadius = 8 };
            
            lblSearchTitle = new Label { Text = "Tên danh mục", Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 10) };
            txtSearch = new Guna2TextBox { Size = new Size(300, 36), Location = new Point(20, 35), BorderRadius = 4, PlaceholderText = "Nhập tên danh mục để tìm kiếm..." };
            
            lblStatusTitle = new Label { Text = "Trạng thái", Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(340, 10) };
            cbStatus = new Guna2ComboBox { Size = new Size(160, 36), Location = new Point(340, 35), BorderRadius = 4, Cursor = Cursors.Hand };
            cbStatus.Items.Add(new { Text = "Tất cả trạng thái", Value = "" });
            cbStatus.Items.Add(new { Text = "Hoạt động", Value = "active" });
            cbStatus.Items.Add(new { Text = "Khóa", Value = "locked" });
            cbStatus.DisplayMember = "Text";
            cbStatus.ValueMember = "Value";
            cbStatus.SelectedIndex = 0;

            btnFilter = new Guna2Button { Text = "Lọc", Size = new Size(100, 36), BorderRadius = 4, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Location = new Point(520, 35) };
            btnFilter.Click += (s, e) => {
                _currentSearchTerm = txtSearch.Text;
                _statusFilter = ((dynamic)cbStatus.SelectedItem).Value;
                _currentPage = 1;
                LoadData();
            };

            pnlFilters.Controls.AddRange(new Control[] { lblSearchTitle, txtSearch, lblStatusTitle, cbStatus, btnFilter });

            // DataGrid Container
            pnlGridContainer = new Guna2Panel { Dock = DockStyle.Top, Height = 400, CustomBorderThickness = new Padding(1), Margin = new Padding(0, 0, 0, gutter), BorderRadius = 8 };
            
            dgvCategories = new Guna2DataGridView
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
                AlternatingRowsDefaultCellStyle = { BackColor = Color.Empty }, 
            };
            dgvCategories.SetDoubleBuffered(true);
            
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Visible = false });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "#", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, Width = 50 });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryCode", HeaderText = "Mã danh mục", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryName", HeaderText = "Tên danh mục", Width = 200, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BookCount", HeaderText = "Số lượng sách", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Mô tả", Width = 250, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            
            var actionCol = new DataGridViewButtonColumn { Name = "Action", HeaderText = "Action", Width = 100, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
            dgvCategories.Columns.Add(actionCol);

            dgvCategories.CellPainting += DgvCategories_CellPainting;
            dgvCategories.CellMouseClick += DgvCategories_CellMouseClick;
            dgvCategories.CellFormatting += DgvCategories_CellFormatting;
            dgvCategories.CellMouseMove += DgvCategories_CellMouseMove;
            dgvCategories.CellMouseLeave += DgvCategories_CellMouseLeave;

            pnlGridContainer.Controls.Add(dgvCategories);

            // Pagination
            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, args) => { _currentPage = args.NewPage; LoadData(); };
            // += (s, size) => { _pageSize = size; _currentPage = 1; LoadData(); };

            pnlContent.Controls.Add(paginationControl);
            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(pnlFilters);
            pnlContent.Controls.Add(pnlPageHeader);

            this.Controls.Add(pnlContent);
            
            ApplyTheme();
        }

        private void CategoryControl_Load(object sender, EventArgs e)
        {
            // Initial pageSize
            //(_pageSize);
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var (items, totalCount) = _categoryService.GetPagedCategories(_currentPage, _pageSize, _currentSearchTerm, _statusFilter);
                dgvCategories.DataSource = null;
                if (this.IsDisposed) return;
                dgvCategories.Rows.Clear();

                foreach (var item in items)
                {
                    dgvCategories.Rows.Add(
                        item.Id,
                        dgvCategories.Rows.Count + 1 + (_currentPage - 1) * _pageSize,
                        item.CategoryCode,
                        item.CategoryName,
                        item.BookCount.ToString("N0"),
                        item.Description,
                        item.Status,
                        ""
                    );
                }

                paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx", FileName = "DanhSachDanhMuc.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var excelService = new ExcelExportService();
                        excelService.ExportDataGridView(dgvCategories, sfd.FileName, "Danh Mục");
                        MessageBox.Show("Xuất file Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new CategoryForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void DgvCategories_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCategories.Columns["Action"].Index)
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
                    e.Graphics.DrawLine(pen, rect.X + rect.Width / 2, rect.Y + 5, rect.X + rect.Width / 2, rect.Bottom - 5);

                e.Handled = true;
            }
        }

        private void DgvCategories_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCategories.Columns["Action"].Index)
            {
                int action = (e.X < dgvCategories.Columns[e.ColumnIndex].Width / 2) ? 1 : 2;
                
                if (_hoveredRowIndex != e.RowIndex || _hoveredAction != action)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = e.RowIndex;
                    _hoveredAction = action;
                    
                    if (oldRow >= 0) dgvCategories.InvalidateCell(e.ColumnIndex, oldRow);
                    dgvCategories.InvalidateCell(e.ColumnIndex, _hoveredRowIndex);
                }
                dgvCategories.Cursor = Cursors.Hand;
            }
            else
            {
                if (_hoveredRowIndex >= 0)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = -1;
                    _hoveredAction = 0;
                    if (e.ColumnIndex >= 0) dgvCategories.InvalidateCell(dgvCategories.Columns["Action"].Index, oldRow);
                }
                dgvCategories.Cursor = Cursors.Default;
            }
        }

        private void DgvCategories_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredRowIndex >= 0)
            {
                int oldRow = _hoveredRowIndex;
                _hoveredRowIndex = -1;
                _hoveredAction = 0;
                dgvCategories.InvalidateCell(dgvCategories.Columns["Action"].Index, oldRow);
            }
            dgvCategories.Cursor = Cursors.Default;
        }

        private void DgvCategories_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCategories.Columns["Action"].Index)
            {
                int id = Convert.ToInt32(dgvCategories.Rows[e.RowIndex].Cells[0].Value);
                if (e.X < dgvCategories.Columns[e.ColumnIndex].Width / 2)
                {
                    var form = new CategoryForm(_categoryService.GetById(id));
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadData();
                    }
                }
                else
                {
                    if (MessageBox.Show("Bạn có chắc chắn muốn xóa danh mục này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        try
                        {
                            _categoryService.SetActive(id, false);
                            MessageBox.Show("Xóa danh mục thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void DgvCategories_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvCategories.Columns[e.ColumnIndex].Name == "Status")
            {
                string? status = e.Value?.ToString();
                if (status == "Khóa")
                {
                    e.CellStyle.ForeColor = Color.Red;
                }
                else if (status == "Hoạt động")
                {
                    e.CellStyle.ForeColor = Color.Green;
                }
            }
            if (e.RowIndex >= 0 && dgvCategories.Columns[e.ColumnIndex].Name == "CategoryCode")
            {
                e.CellStyle.ForeColor = ThemeManager.ButtonFill;
                e.CellStyle.Font = new Font(dgvCategories.Font, FontStyle.Bold);
            }
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            
            pnlContent.BackColor = isDark ? Color.FromArgb(18, 18, 18) : Color.FromArgb(243, 244, 246);
            pnlFilters.BackColor = isDark ? Color.FromArgb(30, 30, 30) : Color.White;
            pnlFilters.CustomBorderColor = isDark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(225, 226, 228);
            
            pnlGridContainer.BackColor = isDark ? Color.FromArgb(30, 30, 30) : Color.White;
            pnlGridContainer.CustomBorderColor = isDark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(225, 226, 228);

            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubtitle.ForeColor = ThemeManager.TextSecondary;
            lblSearchTitle.ForeColor = ThemeManager.TextSecondary;
            lblStatusTitle.ForeColor = ThemeManager.TextSecondary;

            txtSearch.FillColor = isDark ? Color.FromArgb(40, 40, 40) : Color.White;
            txtSearch.ForeColor = ThemeManager.TextPrimary;
            
            cbStatus.FillColor = isDark ? Color.FromArgb(40, 40, 40) : Color.White;
            cbStatus.ForeColor = ThemeManager.TextPrimary;

            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.BorderColor = isDark ? Color.FromArgb(80, 80, 80) : Color.FromArgb(209, 213, 219);
            btnExport.FillColor = isDark ? Color.FromArgb(40, 40, 40) : Color.White;

            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = Color.White;

            btnFilter.ForeColor = ThemeManager.TextPrimary;
            btnFilter.BorderColor = isDark ? Color.FromArgb(80, 80, 80) : Color.FromArgb(209, 213, 219);
            btnFilter.FillColor = isDark ? Color.FromArgb(40, 40, 40) : Color.White;

            // DGV
            dgvCategories.BackgroundColor = isDark ? Color.FromArgb(30, 30, 30) : Color.White;
            dgvCategories.GridColor = isDark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(225, 226, 228);
            
            dgvCategories.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(40, 40, 40) : Color.FromArgb(243, 244, 246);
            dgvCategories.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvCategories.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgvCategories.ColumnHeadersDefaultCellStyle.BackColor;
            
            dgvCategories.DefaultCellStyle.BackColor = isDark ? Color.FromArgb(30, 30, 30) : Color.White;
            dgvCategories.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvCategories.DefaultCellStyle.SelectionBackColor = isDark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(248, 249, 251);
            dgvCategories.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            }
            base.Dispose(disposing);
        }
    }
}