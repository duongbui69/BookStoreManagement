using System;
using System.Drawing;
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
    public partial class InventoryControl : UserControl, ISearchableControl
    {
        private readonly InventoryService _service;
        private readonly BookService _bookService;

        private Guna2Panel pnlContent;
        
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        
        private Guna2Panel pnlFilterBar;
        private Guna2ComboBox cbWarehouseFilter;
        private Guna2ComboBox cbCategoryFilter;
        private Label lblTotalItems;
        private Guna2Button btnExport;
        private Guna2Button btnAddNew;
        
        private Guna2Panel pnlGridContainer;
        private DataGridView dgvWarehouses;
        private PaginationControl paginationControl;
        
        private int _currentPage = 1;
        private int _pageSize = 5;
        private string _currentSearchTerm = "";
        
        public void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            LoadData();
        }

        public InventoryControl()
        {
            _service = new InventoryService();
            _bookService = new BookService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += InventoryControl_Load;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 20;
            this.Padding = new Padding(0);

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = true };
            
            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0,0,0,gutter) };
            lblTitle = new Label { Text = "Inventory Management", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(0,0) };
            lblSubTitle = new Label { Text = "View and manage stock across all warehouses.", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(0,30) };
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            pnlFilterBar = new Guna2Panel { Dock = DockStyle.Top, Height = 70, CustomBorderThickness = new Padding(1,1,1,0), Margin = new Padding(0) };
            
            Label lblWh = new Label { Text = "WAREHOUSE:", AutoSize = true, Location = new Point(20, 25), Font = new Font("Segoe UI", 8F, FontStyle.Bold) };
            cbWarehouseFilter = new Guna2ComboBox { Size = new Size(160, 36), Location = new Point(100, 17), BorderRadius = 4 };
            cbWarehouseFilter.Items.AddRange(new[] { "All Warehouses", "Downtown Main", "Westside Plaza", "University Ave" });
            cbWarehouseFilter.SelectedIndex = 0;

            Label lblCat = new Label { Text = "CATEGORY:", AutoSize = true, Location = new Point(280, 25), Font = new Font("Segoe UI", 8F, FontStyle.Bold) };
            cbCategoryFilter = new Guna2ComboBox { Size = new Size(160, 36), Location = new Point(350, 17), BorderRadius = 4 };
            cbCategoryFilter.Items.AddRange(new[] { "All Categories", "Fiction", "Non-Fiction", "Science" });
            cbCategoryFilter.SelectedIndex = 0;
            
            lblTotalItems = new Label { Text = "Total Items: 0", AutoSize = true, Location = new Point(550, 25), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            
            btnExport = new Guna2Button { Text = "Export CSV", Size = new Size(110, 36), BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold), BorderThickness = 1, FillColor = Color.Transparent };
            btnAddNew = new Guna2Button { Text = "+ New Item", Size = new Size(110, 36), BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnAddNew.Click += BtnAddNew_Click;
            
            pnlFilterBar.Controls.AddRange(new Control[] { lblWh, cbWarehouseFilter, lblCat, cbCategoryFilter, lblTotalItems, btnExport, btnAddNew });
            pnlFilterBar.Resize += (s, e) => {
                btnAddNew.Location = new Point(pnlFilterBar.Width - 130, 17);
                btnExport.Location = new Point(pnlFilterBar.Width - 250, 17);
                lblTotalItems.Location = new Point(pnlFilterBar.Width - 380, 25);
            };

            pnlGridContainer = new Guna2Panel { Dock = DockStyle.Top, Height = 400, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,0,gutter) };
            
            dgvWarehouses = new DataGridView
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
            
            dgvWarehouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Visible = false });
            dgvWarehouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Sku", HeaderText = "SKU", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, Width = 100 });
            dgvWarehouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Title", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, Width = 200 });
            dgvWarehouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Unit Price", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvWarehouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NorthHub", HeaderText = "North Hub", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvWarehouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "WestHub", HeaderText = "West Hub", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvWarehouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CentralHub", HeaderText = "Central Hub", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvWarehouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalStock", HeaderText = "Total Stock", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvWarehouses.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            
            DataGridViewTextBoxColumn actionCol = new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "Actions", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
            dgvWarehouses.Columns.Add(actionCol);

            dgvWarehouses.CellPainting += DgvWarehouses_CellPainting;
            dgvWarehouses.CellMouseClick += DgvWarehouses_CellMouseClick;
            dgvWarehouses.CellFormatting += DgvWarehouses_CellFormatting;
            
            pnlGridContainer.Controls.Add(dgvWarehouses);

            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadData(); };
            pnlGridContainer.Controls.Add(paginationControl);

            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(pnlFilterBar);
            pnlContent.Controls.Add(pnlPageHeader);
            
            pnlPageHeader.BringToFront();
            pnlFilterBar.BringToFront();
            pnlGridContainer.BringToFront();

            this.Controls.Add(pnlContent);

            ApplyTheme();
        }

        private void InventoryControl_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            var stats = _service.GetStats();
            lblTotalItems.Text = $"Total Items: {stats.TotalItems:N0}";

            var (items, totalCount) = _service.GetPagedInventoryItems(_currentPage, _pageSize, _currentSearchTerm);
            dgvWarehouses.DataSource = items;
            
            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
            
            if (dgvWarehouses.Columns["UnitPrice"] != null) 
                dgvWarehouses.Columns["UnitPrice"].DefaultCellStyle.Format = "C2";
        }
        
        private void BtnAddNew_Click(object sender, EventArgs e)
        {
            var frm = new Forms.BookForm(null);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void DgvWarehouses_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvWarehouses.Columns[e.ColumnIndex].Name == "Actions")
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

        private void DgvWarehouses_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvWarehouses.Columns[e.ColumnIndex].Name == "Actions")
            {
                int bookId = Convert.ToInt32(dgvWarehouses.Rows[e.RowIndex].Cells[0].Value);
                var cellRect = dgvWarehouses.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                
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

        private void DgvWarehouses_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvWarehouses.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                
                if (status == "IN STOCK") 
                    e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                else if (status == "OUT OF STOCK") 
                    e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                else 
                    e.CellStyle.ForeColor = Color.FromArgb(243, 156, 18);
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
            
            btnExport.FillColor = ThemeManager.CardBackground;
            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.BorderColor = ThemeManager.TextBoxBorder;

            btnAddNew.FillColor = ThemeManager.ButtonFill;
            btnAddNew.ForeColor = ThemeManager.ButtonText;
            
            pnlFilterBar.FillColor = ThemeManager.CardBackground;
            pnlFilterBar.CustomBorderColor = ThemeManager.TextBoxBorder;
            
            cbWarehouseFilter.FillColor = ThemeManager.TextBoxBackground;
            cbWarehouseFilter.ForeColor = ThemeManager.TextPrimary;
            cbWarehouseFilter.BorderColor = ThemeManager.TextBoxBorder;

            cbCategoryFilter.FillColor = ThemeManager.TextBoxBackground;
            cbCategoryFilter.ForeColor = ThemeManager.TextPrimary;
            cbCategoryFilter.BorderColor = ThemeManager.TextBoxBorder;
            
            foreach (Control c in pnlFilterBar.Controls)
            {
                if (c is Label lbl && lbl != lblTotalItems)
                    lbl.ForeColor = ThemeManager.TextSecondary;
            }
            lblTotalItems.ForeColor = ThemeManager.TextPrimary;

            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            dgvWarehouses.BackgroundColor = ThemeManager.CardBackground;
            dgvWarehouses.GridColor = ThemeManager.TextBoxBorder;
            
            dgvWarehouses.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvWarehouses.AlternatingRowsDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvWarehouses.AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvWarehouses.AlternatingRowsDefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;

            dgvWarehouses.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvWarehouses.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvWarehouses.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvWarehouses.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            
            dgvWarehouses.EnableHeadersVisualStyles = false;
            dgvWarehouses.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvWarehouses.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvWarehouses.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvWarehouses.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Background;
            dgvWarehouses.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        }
    }
}
