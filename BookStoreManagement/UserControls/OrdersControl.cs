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
    public partial class OrdersControl : UserControl, ISearchableControl
    {
        private readonly SalesOrderService _service;

        private Panel pnlContent;
        private Guna.UI2.WinForms.Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        
        private Guna.UI2.WinForms.Guna2Panel pnlFilters;
        private Guna.UI2.WinForms.Guna2TextBox txtSearch;
        private Guna.UI2.WinForms.Guna2Button btnDeleteMultiple;
        private Guna.UI2.WinForms.Guna2Button btnAdd;
        private ComboBox cbStatus;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;

        private Panel pnlGridContainer;
        private DataGridView dgvOrders;

        private PaginationControl pagination;

        private int _currentPage = 1;
        private int _pageSize = 10;
        private bool _isCalculatingPageSize = false;
        private string _currentSearchTerm = "";
        
        private int _hoveredRowIndex = -1;
        private int _hoveredAction = 0; // 1: Edit, 2: Delete

        public async void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            await LoadDataAsync();
        }

        public OrdersControl()
        {
            _service = new SalesOrderService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += OrdersControl_Load;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);
            this.AutoScroll = false;

            pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = false };

            // 1. Header
            pnlPageHeader = new Guna.UI2.WinForms.Guna2Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(0, 0, 0, 20) };

            lblTitle = new Label { Text = "Quản lý Giao dịch", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Danh sách tất cả các đơn hàng và hóa đơn bán hàng.", Font = new Font("Segoe UI", 11F), AutoSize = true, Location = new Point(2, 40) };
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            // 2. Filters Bar
            pnlFilters = new Guna.UI2.WinForms.Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0) };
            
            txtSearch = new Guna.UI2.WinForms.Guna2TextBox
            {
                PlaceholderText = "Tìm theo mã, tên khách...",
                Size = new Size(220, 36),
                Location = new Point(0, 12),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F)
            };
            txtSearch.TextChanged += async (s, e) => { _currentSearchTerm = txtSearch.Text; _currentPage = 1; await LoadDataAsync(); };

            Label lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, Location = new Point(230, 22), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbStatus = new ComboBox { Location = new Point(310, 18), Width = 140, Font = new Font("Segoe UI", 9F), DropDownStyle = ComboBoxStyle.DropDownList };
            cbStatus.Items.AddRange(new object[] { "Tất cả", "Chờ duyệt", "Hoàn thành", "Đã hủy" });
            cbStatus.SelectedIndex = 0;
            cbStatus.SelectedIndexChanged += async (s, e) => { _currentPage = 1; await LoadDataAsync(); };

            Label lblTime = new Label { Text = "Thời gian:", AutoSize = true, Location = new Point(470, 22), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dtpFrom = new DateTimePicker { Location = new Point(545, 18), Width = 110, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 9F) };
            dtpFrom.Value = DateTime.Now.AddDays(-30);
            dtpFrom.ValueChanged += async (s, e) => { _currentPage = 1; await LoadDataAsync(); };
            
            Label lblDash = new Label { Text = "-", AutoSize = true, Location = new Point(660, 20), Font = new Font("Segoe UI", 11F) };
            dtpTo = new DateTimePicker { Location = new Point(680, 18), Width = 110, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 9F) };
            dtpTo.ValueChanged += async (s, e) => { _currentPage = 1; await LoadDataAsync(); };

            btnDeleteMultiple = new Guna.UI2.WinForms.Guna2Button { Text = "Xóa mục đã chọn", Size = new Size(140, 36), BorderRadius = 6, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Visible = false };
            btnDeleteMultiple.Click += BtnDeleteMultiple_Click;

            btnAdd = new Guna.UI2.WinForms.Guna2Button { Text = "+ TẠO ĐƠN HÀNG MỚI", Size = new Size(180, 36), BorderRadius = 6, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.Click += BtnAdd_Click;

            pnlFilters.Controls.AddRange(new Control[] { txtSearch, lblStatus, cbStatus, lblTime, dtpFrom, lblDash, dtpTo, btnDeleteMultiple, btnAdd });
            pnlFilters.Resize += (s, e) =>
            {
                btnAdd.Location = new Point(pnlFilters.Width - 180, 12);
                btnDeleteMultiple.Location = new Point(pnlFilters.Width - 330, 12);
            };

            // 3. Grid Container
            pnlGridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1) };
            
            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
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
            dgvOrders.SetDoubleBuffered(true);
            dgvOrders.RowTemplate.Height = 50;
            dgvOrders.ColumnHeadersHeight = 45;

            // Set up columns
            DataGridViewCheckBoxColumn chkCol = new DataGridViewCheckBoxColumn();
            chkCol.Name = "chkSelect";
            chkCol.HeaderText = "";
            chkCol.Width = 40;
            chkCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvOrders.Columns.Add(chkCol);

            dgvOrders.Columns.Add("Id", "Id");
            dgvOrders.Columns["Id"].Visible = false;
            dgvOrders.Columns["Id"].ReadOnly = true;

            dgvOrders.Columns.Add("OrderCode", "Mã Đơn");
            dgvOrders.Columns.Add("OrderDate", "Ngày Tạo");
            dgvOrders.Columns.Add("CustomerName", "Khách Hàng");
            dgvOrders.Columns.Add("TotalAmount", "Tổng Tiền (VNĐ)");
            dgvOrders.Columns.Add("PaymentMethod", "Thanh Toán");
            dgvOrders.Columns.Add("OrderStatus", "Trạng thái");
            dgvOrders.Columns.Add("Actions", "Thao tác");

            foreach (DataGridViewColumn col in dgvOrders.Columns)
            {
                if (col.Name != "chkSelect")
                {
                    col.ReadOnly = true;
                }
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            dgvOrders.Columns["Actions"].Width = 100;
            dgvOrders.Columns["Actions"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            dgvOrders.CellPainting += DgvOrders_CellPainting;
            dgvOrders.CellMouseEnter += DgvOrders_CellMouseEnter;
            dgvOrders.CellMouseLeave += DgvOrders_CellMouseLeave;
            dgvOrders.CellClick += DgvOrders_CellClick;
            dgvOrders.CurrentCellDirtyStateChanged += DgvOrders_CurrentCellDirtyStateChanged;
            dgvOrders.CellValueChanged += DgvOrders_CellValueChanged;
            
            dgvOrders.Resize += DgvOrders_Resize;
            
            // 4. Pagination
            pagination = new PaginationControl { Dock = DockStyle.Bottom, Height = 50 };
            pagination.PageChanged += async (s, args) => { _currentPage = args.NewPage; await LoadDataAsync(); };

            // Layout assembly
            pnlGridContainer.Controls.Add(dgvOrders);
            
            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10 }); // Spacer
            pnlContent.Controls.Add(pnlFilters);
            pnlContent.Controls.Add(pnlPageHeader);
            pnlContent.Controls.Add(pagination);
            
            pnlGridContainer.BringToFront();

            this.Controls.Add(pnlContent);
            ApplyTheme();
        }

        private void DgvOrders_Resize(object sender, EventArgs e)
        {
            if (_isCalculatingPageSize) return;
            _isCalculatingPageSize = true;
            
            if (dgvOrders.Height > 0)
            {
                int availableHeight = dgvOrders.Height - dgvOrders.ColumnHeadersHeight;
                int newPageSize = availableHeight / dgvOrders.RowTemplate.Height;
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

        private async void OrdersControl_Load(object? sender, EventArgs e)
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
            pnlPageHeader.BackColor = ThemeManager.Background;
            pnlFilters.BackColor = ThemeManager.Background;

            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            
            if (txtSearch != null)
            {
                txtSearch.FillColor = ThemeManager.TextBoxBackground;
                txtSearch.ForeColor = ThemeManager.TextPrimary;
                txtSearch.BorderColor = ThemeManager.TextBoxBorder;
            }

            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;

            btnDeleteMultiple.FillColor = Color.Red;
            btnDeleteMultiple.ForeColor = Color.White;

            pnlGridContainer.BackColor = ThemeManager.TextBoxBorder;

            ThemeManager.ApplyDataGridViewStyle(dgvOrders);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var orders = await _service.GetByDateRangeAsync(dtpFrom.Value.Date, dtpTo.Value.Date.AddDays(1).AddTicks(-1), null);
                
                string statusFilter = cbStatus.SelectedItem?.ToString() ?? "Tất cả";
                if (statusFilter != "Tất cả")
                {
                    orders = orders.Where(o => o.OrderStatus == statusFilter).ToList();
                }

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

                dgvOrders.Rows.Clear();
                foreach (var ord in pagedData)
                {
                    dgvOrders.Rows.Add(
                        false,
                        ord.Id,
                        ord.OrderCode,
                        ord.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                        string.IsNullOrEmpty(ord.CustomerName) ? "Khách vãng lai" : ord.CustomerName,
                        ord.TotalAmount.ToString("N0") + " ₫",
                        ord.PaymentMethod,
                        ord.OrderStatus,
                        ""
                    );
                }

                pagination.UpdatePagination(totalRecords, _currentPage, _pageSize);
                UpdateDeleteMultipleButtonVisibility();
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

        private void DgvOrders_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvOrders.IsCurrentCellDirty && dgvOrders.CurrentCell.OwningColumn.Name == "chkSelect")
            {
                dgvOrders.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvOrders_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvOrders.Columns[e.ColumnIndex].Name == "chkSelect")
            {
                UpdateDeleteMultipleButtonVisibility();
            }
        }

        private void UpdateDeleteMultipleButtonVisibility()
        {
            bool hasChecked = false;
            foreach (DataGridViewRow row in dgvOrders.Rows)
            {
                if (Convert.ToBoolean(row.Cells["chkSelect"].Value) == true)
                {
                    hasChecked = true;
                    break;
                }
            }
            btnDeleteMultiple.Visible = hasChecked;
        }

        private async void BtnDeleteMultiple_Click(object sender, EventArgs e)
        {
            var selectedIds = new List<int>();
            foreach (DataGridViewRow row in dgvOrders.Rows)
            {
                if (Convert.ToBoolean(row.Cells["chkSelect"].Value) == true)
                {
                    selectedIds.Add(Convert.ToInt32(row.Cells["Id"].Value));
                }
            }

            if (selectedIds.Count > 0)
            {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn hủy {selectedIds.Count} đơn hàng đã chọn không?", "Xác nhận hủy", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        foreach (var id in selectedIds)
                        {
                            await _service.CancelOrderAsync(id);
                        }
                        MessageBox.Show("Hủy đơn hàng thành công.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi hủy đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DgvOrders_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvOrders.Columns["CustomerName"].Index && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                string name = e.Value.ToString() ?? "Khách vãng lai";
                string initial = name.Length > 0 ? name.Substring(0, 1).ToUpper() : "?";
                
                if (name.Contains(" "))
                {
                    var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        initial = parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1);
                    }
                }
                
                initial = initial.ToUpper();

                Color circleColor = GetColorFromInitial(initial);
                
                using (GraphicsPath path = new GraphicsPath())
                {
                    int circleSize = 24;
                    int x = e.CellBounds.Left + 15;
                    int y = e.CellBounds.Top + (e.CellBounds.Height - circleSize) / 2;
                    
                    path.AddEllipse(x, y, circleSize, circleSize);
                    
                    using (SolidBrush brush = new SolidBrush(circleColor))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                    }
                    
                    using (var font = new Font("Segoe UI", 8F, FontStyle.Bold))
                    {
                    TextRenderer.DrawText(e.Graphics, initial, font, new Rectangle(x, y, circleSize, circleSize), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }

                Rectangle textRect = new Rectangle(e.CellBounds.Left + 45, e.CellBounds.Top, e.CellBounds.Width - 45, e.CellBounds.Height);
                TextRenderer.DrawText(e.Graphics, name, e.CellStyle.Font, textRect, BookStoreManagement.Themes.ThemeManager.TextPrimary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                
                e.Handled = true;
            }

            if (e.RowIndex >= 0 && e.ColumnIndex == dgvOrders.Columns["PaymentMethod"].Index && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                string payment = e.Value.ToString() ?? "";
                
                string icon = "\uE8A5"; // default document
                if (payment.ToLower().Contains("tiền mặt") || payment.ToLower().Contains("tiền mặt")) icon = "\uE8CB"; // payments
                else if (payment.ToLower().Contains("chuyển khoản") || payment.ToLower().Contains("bank")) icon = "\uE8C7"; // account_balance
                else if (payment.ToLower().Contains("thẻ") || payment.ToLower().Contains("thẻ") || payment.ToLower().Contains("momo")) icon = "\uE8C9"; // credit_card

                int iconSize = 20;
                int iconX = e.CellBounds.Left + 10;
                int iconY = e.CellBounds.Top + (e.CellBounds.Height - iconSize) / 2;

                DrawIcon(e.Graphics, new Rectangle(iconX, iconY, iconSize, iconSize), icon, ThemeManager.TextSecondary);

                Rectangle textRect = new Rectangle(iconX + iconSize + 5, e.CellBounds.Top, e.CellBounds.Width - iconSize - 15, e.CellBounds.Height);
                TextRenderer.DrawText(e.Graphics, payment, e.CellStyle.Font, textRect, BookStoreManagement.Themes.ThemeManager.TextPrimary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }

            if (e.RowIndex >= 0 && e.ColumnIndex == dgvOrders.Columns["Actions"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                int iconSize = 20;
                int margin = 8;
                int totalWidth = (iconSize * 2) + margin;
                int startX = e.CellBounds.Left + (e.CellBounds.Width - totalWidth) / 2;
                int startY = e.CellBounds.Top + (e.CellBounds.Height - iconSize) / 2;

                Rectangle rectEdit = new Rectangle(startX, startY, iconSize, iconSize);
                Rectangle rectDelete = new Rectangle(startX + iconSize + margin, startY, iconSize, iconSize);

                bool isHoveringEdit = (_hoveredRowIndex == e.RowIndex && _hoveredAction == 1);
                bool isHoveringDelete = (_hoveredRowIndex == e.RowIndex && _hoveredAction == 2);

                DrawIcon(e.Graphics, rectEdit, "\uE70F", isHoveringEdit ? ThemeManager.ButtonFill : ThemeManager.TextSecondary); // Edit icon (pencil)
                DrawIcon(e.Graphics, rectDelete, "\uE74D", isHoveringDelete ? Color.FromArgb(239, 68, 68) : ThemeManager.TextSecondary); // Delete icon (trash)

                e.Handled = true;
            }
            
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvOrders.Columns["OrderStatus"].Index && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                string status = e.Value.ToString() ?? "";
                
                Color bgColor = Color.FromArgb(20, 100, 100, 100);
                Color textColor = Color.Gray;

                if (status.ToLower().Contains("hoàn thành") || status.ToLower() == "hoàn thành")
                {
                    bgColor = Color.FromArgb(20, 34, 197, 94);
                    textColor = Color.FromArgb(34, 197, 94);
                }
                else if (status.ToLower().Contains("đang giao") || status.ToLower() == "shipping")
                {
                    bgColor = Color.FromArgb(20, 59, 130, 246); // Blue
                    textColor = Color.FromArgb(59, 130, 246);
                }
                else if (status.ToLower().Contains("đang xử lý") || status.ToLower() == "pending")
                {
                    bgColor = Color.FromArgb(20, 245, 158, 11); // Orange
                    textColor = Color.FromArgb(245, 158, 11);
                }
                else if (status.ToLower().Contains("hủy") || status.ToLower() == "hủy")
                {
                    bgColor = Color.FromArgb(20, 239, 68, 68); // Red
                    textColor = Color.FromArgb(239, 68, 68);
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

        private Color GetColorFromInitial(string initial)
        {
            if (string.IsNullOrEmpty(initial)) return Color.Gray;
            char c = initial[0];
            if (c >= 'A' && c <= 'G') return Color.FromArgb(184, 137, 255); // secondary-container (purple)
            if (c >= 'H' && c <= 'N') return Color.FromArgb(27, 58, 87);    // primary-container (dark blue)
            if (c >= 'O' && c <= 'U') return Color.FromArgb(0, 66, 31);     // tertiary-container (green)
            return Color.FromArgb(160, 160, 165);
        }

        private void DrawIcon(Graphics g, Rectangle rect, string iconCode, Color color)
        {
            using (Font iconFont = new Font("Segoe MDL2 Assets", 12F, FontStyle.Regular))
            {
                TextRenderer.DrawText(g, iconCode, iconFont, rect, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DgvOrders_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvOrders.Columns["Actions"].Index)
            {
                dgvOrders.Cursor = Cursors.Hand;
            }
        }

        private void DgvOrders_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvOrders.Columns["Actions"].Index)
            {
                dgvOrders.Cursor = Cursors.Default;
                _hoveredRowIndex = -1;
                _hoveredAction = 0;
                dgvOrders.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }

        private async void DgvOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvOrders.Columns["Actions"].Index)
            {
                Rectangle cellBounds = dgvOrders.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                Point mousePos = dgvOrders.PointToClient(Cursor.Position);
                
                int iconSize = 20;
                int margin = 8;
                int totalWidth = (iconSize * 2) + margin;
                int startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2;
                int startY = cellBounds.Top + (cellBounds.Height - iconSize) / 2;

                Rectangle rectEdit = new Rectangle(startX, startY, iconSize, iconSize);
                Rectangle rectDelete = new Rectangle(startX + iconSize + margin, startY, iconSize, iconSize);

                if (rectEdit.Contains(mousePos))
                {
                    int id = Convert.ToInt32(dgvOrders.Rows[e.RowIndex].Cells["Id"].Value);
                    var order = await _service.GetByIdAsync(id);
                    if (order != null)
                    {
                        using (var form = new OrderForm(order))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                            {
                                await LoadDataAsync();
                            }
                        }
                    }
                }
                else if (rectDelete.Contains(mousePos))
                {
                    int id = Convert.ToInt32(dgvOrders.Rows[e.RowIndex].Cells["Id"].Value);
                    string code = dgvOrders.Rows[e.RowIndex].Cells["OrderCode"].Value.ToString();
                    var result = MessageBox.Show($"Bạn có chắc chắn muốn hủy đơn hàng '{code}'?", "Xác nhận hủy", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            await _service.CancelOrderAsync(id);
                            await LoadDataAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi hủy: " + ex.Message);
                        }
                    }
                }
            }
        }
    }
}