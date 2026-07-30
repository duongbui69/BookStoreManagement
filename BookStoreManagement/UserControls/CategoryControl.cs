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
        private TableLayoutPanel tlpStats;
        private Guna2Panel card1, card2, card3;
        private Guna2Button btnExport;
        private Guna2Button btnAdd;

        private Guna2Panel pnlFilters;
        private Label lblSearchTitle;
        private Guna2TextBox txtSearch;
        private Label lblStatusTitle;
        private Guna2ComboBox cbStatus;
        

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

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = false };

            // Page Header
            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(0, 0, 0, gutter) };
            
            lblTitle = new Label { Text = "Danh mục sách", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubtitle = new Label { Text = "Quản lý và phân loại các đầu sách trong hệ thống.", Font = new Font("Segoe UI", 11F), AutoSize = true, Location = new Point(0, 45) };
            
            btnExport = new Guna2Button { Text = "Xuất Excel", Size = new Size(120, 36), BorderRadius = 8, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnExport.Click += BtnExport_Click;

            btnAdd = new Guna2Button { Text = "+ Thêm mới", Size = new Size(120, 36), BorderRadius = 8, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.Click += BtnAdd_Click;

            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });

            

            tlpStats = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, Height = 100, 
                ColumnCount = 3, RowCount = 1,
                Margin = new Padding(0,0,0,gutter)
            };
            for(int i=0; i<3; i++) tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            
            card1 = CreateStatCard("TỔNG SỐ DANH MỤC", "0", "category", Color.FromArgb(41, 128, 185)); // Primary
            card2 = CreateStatCard("ĐANG HOẠT ĐỘNG", "0", "active", Color.FromArgb(0, 186, 97)); // Tertiary
            card3 = CreateStatCard("ĐÃ KHÓA", "0", "locked", Color.FromArgb(186, 26, 26)); // Error
            
            tlpStats.Controls.Add(card1, 0, 0);
            tlpStats.Controls.Add(card2, 1, 0);
            tlpStats.Controls.Add(card3, 2, 0);

// Filter Bar
            pnlFilters = new Guna2Panel { Dock = DockStyle.Top, Height = 70, CustomBorderThickness = new Padding(1), Margin = new Padding(0, 0, 0, gutter), BorderRadius = 8 };
            
            txtSearch = new Guna2TextBox { Size = new Size(300, 36), Location = new Point(20, 16), BorderRadius = 8, PlaceholderText = "Nhập tên danh mục để tìm kiếm..." };
            
            cbStatus = new Guna2ComboBox { Size = new Size(160, 36), Location = new Point(340, 16), BorderRadius = 8, Cursor = Cursors.Hand };
            cbStatus.Items.Add(new { Text = "Tất cả trạng thái", Value = "" });
            cbStatus.Items.Add(new { Text = "Đang hoạt động", Value = "active" });
            cbStatus.Items.Add(new { Text = "Tạm khóa", Value = "locked" });
            cbStatus.DisplayMember = "Text";
            cbStatus.ValueMember = "Value";
            cbStatus.SelectedIndex = 0;

            txtSearch.TextChanged += (s, e) => {
                _currentSearchTerm = txtSearch.Text;
                _currentPage = 1;
                LoadData();
            };
            
            cbStatus.SelectedIndexChanged += (s, e) => {
                if (cbStatus.SelectedItem != null) {
                    _statusFilter = ((dynamic)cbStatus.SelectedItem).Value;
                    _currentPage = 1;
                    LoadData();
                }
            };

            pnlFilters.Controls.AddRange(new Control[] { txtSearch, cbStatus, btnExport, btnAdd });
            pnlFilters.Resize += (s, e) => {
                btnAdd.Location = new Point(pnlFilters.Width - 140, 16);
                btnExport.Location = new Point(pnlFilters.Width - 270, 16);
            };

            // DataGrid Container
            pnlGridContainer = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), Margin = new Padding(0, 0, 0, gutter), BorderRadius = 8 };
            
            dgvCategories = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                ScrollBars = ScrollBars.None,
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
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryCode", HeaderText = "MÃ DANH MỤC", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryName", HeaderText = "TÊN DANH MỤC", Width = 200, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BookCount", HeaderText = "SỐ LƯỢNG SÁCH", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "MÔ TẢ", Width = 250, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TRẠNG THÁI", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            
            var actionCol = new DataGridViewTextBoxColumn { Name = "Thao tác", HeaderText = "THAO TÁC", Width = 100, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
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

            Panel spacer1 = new Panel { Dock = DockStyle.Top, Height = gutter, BackColor = Color.Transparent };
            Panel spacer2 = new Panel { Dock = DockStyle.Top, Height = gutter, BackColor = Color.Transparent };
            Panel spacer3 = new Panel { Dock = DockStyle.Top, Height = gutter, BackColor = Color.Transparent };

            pnlContent.Controls.Add(paginationControl);
            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(spacer2);
            pnlContent.Controls.Add(pnlFilters);
            pnlContent.Controls.Add(spacer3);
            pnlContent.Controls.Add(tlpStats);
            pnlContent.Controls.Add(spacer1);
            pnlContent.Controls.Add(pnlPageHeader);
            
            pnlPageHeader.BringToFront();
            spacer1.BringToFront();
            tlpStats.BringToFront();
            spacer3.BringToFront();
            pnlFilters.BringToFront();
            spacer2.BringToFront();
            paginationControl.BringToFront();
            pnlGridContainer.BringToFront();

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
                
                var all = _categoryService.GetAll();
                int total = all.Count;
                int active = 0, locked = 0;
                foreach(var x in all) { if(x.IsActive) active++; else locked++; }
                foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = total.ToString("N0");
                foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = active.ToString("N0");
                foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = locked.ToString("N0");

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
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        excelService.ExportDataGridView(dgvCategories, sfd.FileName, "Thể Loại");
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
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCategories.Columns["Thao tác"].Index)
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
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCategories.Columns["Thao tác"].Index)
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
                    if (e.ColumnIndex >= 0) dgvCategories.InvalidateCell(dgvCategories.Columns["Thao tác"].Index, oldRow);
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
                dgvCategories.InvalidateCell(dgvCategories.Columns["Thao tác"].Index, oldRow);
            }
            dgvCategories.Cursor = Cursors.Default;
        }

        private void DgvCategories_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCategories.Columns["THAO TÁC"].Index)
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
                            MessageBox.Show("Lỗi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void DgvCategories_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvCategories.Columns[e.ColumnIndex].Name == "TRẠNG THÁI")
            {
                string? status = e.Value?.ToString();
                if (status == "Tạm khóa")
                {
                    e.CellStyle.ForeColor = Color.Red;
                }
                else if (status == "Đang hoạt động")
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

        private Guna2Panel CreateStatCard(string title, string value, string iconText, Color color)
        {
            var pnl = new Guna2Panel 
            { 
                Dock = DockStyle.Fill, 
                BorderThickness = 1, 
                Margin = new Padding(0,0,20,0), 
                BorderRadius = 10 
            };
            
            // Icon
            var pnlIcon = new Guna2Panel { Size = new Size(48, 48), Location = new Point(20, 15), BorderRadius = 24, FillColor = Color.FromArgb(30, color) };
            Label lblIcon = new Label { Text = iconText == "category" ? "📂" : (iconText == "active" ? "✅" : "🔒"), Font = new Font("Segoe UI Emoji", 16F), AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            pnlIcon.Controls.Add(lblIcon);
            
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(80, 14) };
            lblTitle.Tag = "CardTitle";
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 22F, FontStyle.Bold), AutoSize = true, Location = new Point(78, 32), ForeColor = color };
            lblValue.Tag = "CardValue";

            pnl.Controls.Add(pnlIcon);
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);
            return pnl;
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
            ApplyThemeToCard(card1);
            ApplyThemeToCard(card2);
            ApplyThemeToCard(card3);

            txtSearch.FillColor = isDark ? Color.FromArgb(40, 40, 40) : Color.White;
            txtSearch.ForeColor = ThemeManager.TextPrimary;
            
            cbStatus.FillColor = isDark ? Color.FromArgb(40, 40, 40) : Color.White;
            cbStatus.ForeColor = ThemeManager.TextPrimary;

            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.BorderColor = isDark ? Color.FromArgb(80, 80, 80) : Color.FromArgb(209, 213, 219);
            btnExport.FillColor = isDark ? Color.FromArgb(40, 40, 40) : Color.White;

            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = Color.White;

            // DGV
            ThemeManager.ApplyDataGridViewStyle(dgvCategories);
        }

        
        private void ApplyThemeToCard(Guna2Panel card)
        {
            card.BackColor = Color.Transparent;
            card.BorderColor = ThemeManager.TextBoxBorder;
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
                ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            }
            base.Dispose(disposing);
        }
    }
}
