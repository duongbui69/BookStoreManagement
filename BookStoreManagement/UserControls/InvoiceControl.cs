using BookStoreManagement.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Models;
using BookStoreManagement.Themes;
using BookStoreManagement.Interfaces;
using BookStoreManagement.Forms;
using System.Collections.Generic;
using System.Linq;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.UserControls
{
    public partial class InvoiceControl : UserControl, ISearchableControl
    {
        private readonly SalesOrderService _service;

        private Panel pnlContent;
        private TableLayoutPanel tlpHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        private Button btnFilter;
        private Button btnAdd;
        private TextBox txtSearch;

        private Panel pnlGridContainer;
        private DataGridView dgvInvoices;
        private PaginationControl pagination;

        private int _currentPage = 1;
        private int _pageSize = 10;
        private bool _isCalculatingPageSize = false;
        private string _currentSearchTerm = "";
        
        private int _hoveredRowIndex = -1;

        public async void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            await LoadDataAsync();
        }

        public InvoiceControl()
        {
            _service = new SalesOrderService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += InvoiceControl_Load;
        }

        private void DgvInvoices_Resize(object sender, EventArgs e)
        {
            if (_isCalculatingPageSize) return;
            _isCalculatingPageSize = true;
            
            if (dgvInvoices.Height > 0)
            {
                int availableHeight = dgvInvoices.Height - dgvInvoices.ColumnHeadersHeight;
                int newPageSize = availableHeight / dgvInvoices.RowTemplate.Height;
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
            this.Padding = new Padding(0);
            this.AutoScroll = false;

            pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30), AutoScroll = false };

            // 1. Header
            tlpHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 4,
                RowCount = 2,
                Height = 80,
                Margin = new Padding(0, 0, 0, 20)
            };
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            lblTitle = new Label { Text = "Quản lý Hoá đơn", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Margin = new Padding(0) };
            lblSubTitle = new Label { Text = "Danh sách hoá đơn bán hàng từ khách hàng", Font = new Font("Segoe UI", 10F), AutoSize = true, Margin = new Padding(2, 5, 0, 0) };
            
            txtSearch = new TextBox { Width = 250, Font = new Font("Segoe UI", 10F), Margin = new Padding(0, 15, 10, 0), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.PlaceholderText = "Tìm kiếm hoá đơn...";
            txtSearch.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    _currentSearchTerm = txtSearch.Text;
                    _currentPage = 1;
                    await LoadDataAsync();
                }
            };

            btnFilter = new Button { Text = " Lọc", Size = new Size(100, 36), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 12, 10, 0) };
            
            btnAdd = new Button { Text = "+ Tạo Hoá đơn", Size = new Size(160, 36), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 12, 0, 0) };
            btnAdd.Click += BtnAdd_Click;

            tlpHeader.Controls.Add(lblTitle, 0, 0);
            tlpHeader.Controls.Add(lblSubTitle, 0, 1);
            
            tlpHeader.Controls.Add(txtSearch, 1, 0);
            tlpHeader.SetRowSpan(txtSearch, 2);
            
            tlpHeader.Controls.Add(btnFilter, 2, 0);
            tlpHeader.SetRowSpan(btnFilter, 2);
            
            tlpHeader.Controls.Add(btnAdd, 3, 0);
            tlpHeader.SetRowSpan(btnAdd, 2);

            // 2. Grid Container
            pnlGridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1) };
            
            dgvInvoices = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ScrollBars = ScrollBars.Both
            };
            dgvInvoices.SetDoubleBuffered(true);
            dgvInvoices.RowTemplate.Height = 50;
            dgvInvoices.ColumnHeadersHeight = 45;

            // Set up columns
            dgvInvoices.Columns.Add("Id", "Id");
            dgvInvoices.Columns["Id"].Visible = false;

            dgvInvoices.Columns.Add("OrderCode", "Mã HĐ");
            dgvInvoices.Columns.Add("OrderDate", "Ngày lập");
            dgvInvoices.Columns.Add("CustomerName", "Khách hàng");
            dgvInvoices.Columns.Add("SubTotal", "Tổng tiền (Chưa VAT)");
            dgvInvoices.Columns.Add("VAT", "VAT (8%)");
            dgvInvoices.Columns.Add("TotalAmount", "Tổng thanh toán");
            dgvInvoices.Columns.Add("OrderStatus", "Trạng thái");
            dgvInvoices.Columns.Add("Actions", "Thao tác");

            foreach (DataGridViewColumn col in dgvInvoices.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (col.Name == "CustomerName") col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                else if (col.Name.Contains("Total") || col.Name == "VAT" || col.Name == "SubTotal") col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                else col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            
            dgvInvoices.Columns["OrderCode"].Width = 90;
            dgvInvoices.Columns["Actions"].Width = 80;
            dgvInvoices.Columns["Actions"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            dgvInvoices.CellPainting += DgvInvoices_CellPainting;
            dgvInvoices.CellMouseEnter += DgvInvoices_CellMouseEnter;
            dgvInvoices.CellMouseLeave += DgvInvoices_CellMouseLeave;
            dgvInvoices.CellClick += DgvInvoices_CellClick;
            
            // 3. Pagination
            pagination = new PaginationControl { Dock = DockStyle.Bottom };
            pagination.PageChanged += async (s, args) => { _currentPage = args.NewPage; await LoadDataAsync(); };

            dgvInvoices.Resize += DgvInvoices_Resize;

            pnlGridContainer.Controls.Add(dgvInvoices);
            
            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(tlpHeader);
            pnlContent.Controls.Add(pagination);

            this.Controls.Add(pnlContent);
            ApplyTheme();
        }

        private async void InvoiceControl_Load(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            
            txtSearch.BackColor = ThemeManager.CardBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;
            
            btnAdd.BackColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatAppearance.BorderSize = 0;

            btnFilter.BackColor = ThemeManager.CardBackground;
            btnFilter.ForeColor = ThemeManager.TextPrimary;
            btnFilter.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            pnlGridContainer.BackColor = ThemeManager.TextBoxBorder;

            dgvInvoices.BackgroundColor = ThemeManager.CardBackground;
            dgvInvoices.GridColor = ThemeManager.TextBoxBorder;
            
            dgvInvoices.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvInvoices.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvInvoices.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvInvoices.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Background;

            dgvInvoices.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvInvoices.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvInvoices.DefaultCellStyle.SelectionBackColor = ThemeManager.CardBackground;
            dgvInvoices.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvInvoices.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            
            dgvInvoices.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvInvoices.AlternatingRowsDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var orders = await _service.GetAllAsync(); // Retrieve all or apply date filters as needed

                if (!string.IsNullOrWhiteSpace(_currentSearchTerm))
                {
                    string term = _currentSearchTerm.ToLower();
                    orders = orders.Where(o => o.OrderCode.ToLower().Contains(term) || (o.CustomerName != null && o.CustomerName.ToLower().Contains(term))).ToList();
                }

                int totalRecords = orders.Count;
                int totalPages = (int)Math.Ceiling(totalRecords / (double)_pageSize);
                
                if (totalPages > 0 && _currentPage > totalPages) _currentPage = totalPages;
                if (_currentPage < 1) _currentPage = 1;

                var pagedData = orders.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

                if (this.IsDisposed) return;
                dgvInvoices.Rows.Clear();
                foreach (var ord in pagedData)
                {
                    decimal totalAmount = ord.TotalAmount;
                    decimal subTotal = totalAmount / 1.08m;
                    decimal vatAmount = totalAmount - subTotal;

                    dgvInvoices.Rows.Add(
                        ord.Id,
                        ord.OrderCode,
                        ord.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                        string.IsNullOrEmpty(ord.CustomerName) ? "Khách vãng lai" : ord.CustomerName,
                        subTotal.ToString("N0") + " ₫",
                        vatAmount.ToString("N0") + " ₫",
                        totalAmount.ToString("N0") + " ₫",
                        ord.OrderStatus,
                        ""
                    );
                }

                pagination.UpdatePagination(totalRecords, _currentPage, _pageSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            using (var form = new OrderForm(null))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            }
        }

        private void DgvInvoices_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["OrderCode"].Index && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                using (var font = new Font("Segoe UI", 9.5F, FontStyle.Bold))
                {
                TextRenderer.DrawText(e.Graphics, e.Value.ToString(), font, e.CellBounds, ThemeManager.ButtonFill, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["TotalAmount"].Index && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                Rectangle textRect = new Rectangle(e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width - 15, e.CellBounds.Height);
                using (var font = new Font("Segoe UI", 9.5F, FontStyle.Bold))
                {
                TextRenderer.DrawText(e.Graphics, e.Value.ToString(), font, textRect, ThemeManager.ButtonFill, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }
                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["SubTotal"].Index && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                Rectangle textRect = new Rectangle(e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width - 15, e.CellBounds.Height);
                TextRenderer.DrawText(e.Graphics, e.Value.ToString(), e.CellStyle.Font, textRect, e.CellStyle.ForeColor, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["VAT"].Index && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                Rectangle textRect = new Rectangle(e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width - 15, e.CellBounds.Height);
                TextRenderer.DrawText(e.Graphics, e.Value.ToString(), e.CellStyle.Font, textRect, e.CellStyle.ForeColor, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["Actions"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                int iconSize = 24;
                int startX = e.CellBounds.Left + (e.CellBounds.Width - iconSize) / 2;
                int startY = e.CellBounds.Top + (e.CellBounds.Height - iconSize) / 2;

                Rectangle rectView = new Rectangle(startX, startY, iconSize, iconSize);
                bool isHoveringView = (_hoveredRowIndex == e.RowIndex);

                Color iconColor = isHoveringView ? ThemeManager.ButtonFill : ThemeManager.TextSecondary;
                
                using (Font iconFont = new Font("Segoe MDL2 Assets", 14F, FontStyle.Regular))
                {
                    TextRenderer.DrawText(e.Graphics, "\uE890", iconFont, rectView, iconColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["OrderStatus"].Index && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                string status = e.Value.ToString() ?? "";
                
                Color bgColor = Color.FromArgb(20, 100, 100, 100);
                Color textColor = Color.Gray;

                if (status.ToLower().Contains("hoàn thành") || status.ToLower() == "completed" || status.ToLower().Contains("đã thanh toán"))
                {
                    bgColor = Color.FromArgb(20, 0, 42, 17); // tertiary color
                    textColor = Color.FromArgb(0, 42, 17);
                }
                else if (status.ToLower().Contains("đang giao") || status.ToLower() == "shipping" || status.ToLower().Contains("đang xử lý"))
                {
                    bgColor = Color.FromArgb(20, 90, 42, 156); // Purple
                    textColor = Color.FromArgb(90, 42, 156);
                }
                else if (status.ToLower().Contains("chờ xử lý") || status.ToLower() == "pending" || status.ToLower().Contains("chưa thanh toán"))
                {
                    bgColor = Color.FromArgb(20, 147, 0, 10); // Red error container
                    textColor = Color.FromArgb(147, 0, 10);
                }
                
                using (GraphicsPath path = new GraphicsPath())
                {
                    int width = 90;
                    int height = 24;
                    int x = e.CellBounds.Left + (e.CellBounds.Width - width) / 2;
                    int y = e.CellBounds.Top + (e.CellBounds.Height - height) / 2;
                    int radius = 12;
                    
                    path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
                    path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
                    path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
                    path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
                    path.CloseFigure();
                    
                    using (SolidBrush brush = new SolidBrush(bgColor))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                    }
                    
                    using (var font = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                    {
                    TextRenderer.DrawText(e.Graphics, status, font, e.CellBounds, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
                e.Handled = true;
            }
        }

        private void DgvInvoices_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["Actions"].Index)
            {
                dgvInvoices.Cursor = Cursors.Hand;
                _hoveredRowIndex = e.RowIndex;
                dgvInvoices.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }

        private void DgvInvoices_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["Actions"].Index)
            {
                dgvInvoices.Cursor = Cursors.Default;
                _hoveredRowIndex = -1;
                dgvInvoices.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }

        private async void DgvInvoices_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["Actions"].Index)
            {
                int id = Convert.ToInt32(dgvInvoices.Rows[e.RowIndex].Cells["Id"].Value);
                using (var form = new InvoiceForm(id))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        await LoadDataAsync();
                    }
                }
            }
        }
    }
}