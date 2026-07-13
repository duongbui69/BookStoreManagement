using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;
using System.Collections.Generic;
using BookStoreManagement.Interfaces;

namespace BookStoreManagement.UserControls
{
    public partial class OrdersControl : UserControl, ISearchableControl
    {
        private readonly OrderService _service;
        private readonly SalesOrderRepository _salesRepo; // for cancel

        // Content Container
        private Guna2Panel pnlContent;

        // Page Header
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;

        // Filters Bar
        private Guna2Panel pnlFilters;
        private Guna2ComboBox cbStatus;
        private Guna2ComboBox cbDate;
        private Label lblTotalOrders;
        private Guna2Button btnExport;
        private Guna2Button btnAdd;

        // Grid
        private Guna2Panel pnlGridContainer;
        private Guna2DataGridView dgvOrders;

        // Pagination
        private PaginationControl paginationControl;

        private int _currentPage = 1;
        private int _pageSize = 5;
        private string _currentTab = "Pending Orders";
        private string _currentSearchTerm = "";

        public void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            LoadData();
        }

        public OrdersControl()
        {
            _service = new OrderService();
            _salesRepo = new SalesOrderRepository();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += OrdersControl_Load;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 20;
            this.Padding = new Padding(0);
            this.AutoScroll = true; // Make the page scrollable

            // Container for scrollable content
            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = true };

            // 1. Page Header
            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0, 0, 0, gutter) };
            lblTitle = new Label { Text = "Orders Management", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "View and manage customer orders across all channels.", Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(2, 40) };
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            // 2. Filters Bar
            pnlFilters = new Guna2Panel { Dock = DockStyle.Top, Height = 70, CustomBorderThickness = new Padding(1, 1, 1, 0), Margin = new Padding(0), BorderRadius = 6 };
            pnlFilters.CustomizableEdges.BottomLeft = false;
            pnlFilters.CustomizableEdges.BottomRight = false;

            Label lblStatus = new Label { Text = "STATUS", AutoSize = true, Location = new Point(20, 27), Font = new Font("Segoe UI", 8F, FontStyle.Bold) };
            cbStatus = new Guna2ComboBox { Size = new Size(140, 36), Location = new Point(70, 17), BorderRadius = 4, Font = new Font("Segoe UI", 9F) };
            cbStatus.Items.AddRange(new object[] { "All Statuses", "Pending Orders", "Completed", "Refunded" });
            cbStatus.SelectedIndex = 0;
            cbStatus.SelectedIndexChanged += (s, e) =>
            {
                _currentTab = cbStatus.SelectedItem.ToString();
                if (_currentTab == "All Statuses") _currentTab = "Pending Orders";
                _currentPage = 1;
                LoadData();
            };

            Panel pnlSep = new Panel { Width = 1, Height = 24, Location = new Point(230, 23), BackColor = Color.LightGray };

            Label lblDate = new Label { Text = "DATE", AutoSize = true, Location = new Point(250, 27), Font = new Font("Segoe UI", 8F, FontStyle.Bold) };
            cbDate = new Guna2ComboBox { Size = new Size(140, 36), Location = new Point(290, 17), BorderRadius = 4, Font = new Font("Segoe UI", 9F) };
            cbDate.Items.AddRange(new object[] { "Last 7 Days", "Last 30 Days", "This Month" });
            cbDate.SelectedIndex = 0;

            lblTotalOrders = new Label { Text = "Total Orders: 0", AutoSize = true, Font = new Font("Segoe UI", 9F), Padding = new Padding(10, 5, 10, 5) };
            lblTotalOrders.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var p = new Pen(ThemeManager.TextBoxBorder, 1);
                e.Graphics.DrawRoundedRectangle(p, 0, 0, lblTotalOrders.Width - 1, lblTotalOrders.Height - 1, 12);
            };

            btnExport = new Guna2Button { Text = "Export CSV", Size = new Size(120, 36), BorderRadius = 4, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnExport.Click += BtnExport_Click;
            
            btnAdd = new Guna2Button { Text = "+ New Order", Size = new Size(130, 36), BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnAdd.Click += BtnAdd_Click;

            pnlFilters.Controls.AddRange(new Control[] { lblStatus, cbStatus, pnlSep, lblDate, cbDate, lblTotalOrders, btnExport, btnAdd });

            // 3. Grid Container
            pnlGridContainer = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1, 0, 1, 1), Margin = new Padding(0, 0, 0, gutter), BorderRadius = 6 };
            pnlGridContainer.CustomizableEdges.TopLeft = false;
            pnlGridContainer.CustomizableEdges.TopRight = false;
            
            dgvOrders = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 60 },
                Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default
            };
            
            // Define Columns
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Name = "Id", Visible = false });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OrderId", HeaderText = "Order ID", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 100 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerName", Name = "CustomerName", HeaderText = "Customer", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerEmail", Name = "CustomerEmail", Visible = false });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Date", HeaderText = "Date", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "MMM dd, yyyy \n HH:mm" }, Width = 120 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ItemsCount", HeaderText = "Items", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 80 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Total", HeaderText = "Total Amount", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "C2" }, Width = 120 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", Name = "Status", HeaderText = "Status", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 120 });
            
            DataGridViewTextBoxColumn actionCol = new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "Actions", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
            dgvOrders.Columns.Add(actionCol);

            dgvOrders.CellPainting += DgvOrders_CellPainting;
            dgvOrders.CellMouseClick += DgvOrders_CellMouseClick;
            
            pnlGridContainer.Controls.Add(dgvOrders);

            // Pagination
            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadData(); };
            pnlGridContainer.Controls.Add(paginationControl);

            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(pnlFilters);
            pnlContent.Controls.Add(pnlPageHeader);

            this.Controls.Add(pnlContent);

            this.Resize += (s, e) =>
            {
                btnExport.Location = new Point(pnlFilters.Width - 280, 17);
                btnAdd.Location = new Point(pnlFilters.Width - 150, 17);
                lblTotalOrders.Location = new Point(pnlFilters.Width - 430, 22);
            };

            ApplyTheme();
        }

        private void OrdersControl_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            MessageBox.Show("New Order feature is under development.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void BtnExport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Export CSV feature is under development.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadData()
        {
            var (items, totalCount) = _service.GetPagedOrders(_currentPage, _pageSize, _currentTab, _currentSearchTerm);
            dgvOrders.DataSource = items;

            lblTotalOrders.Text = $"Total Orders: {totalCount:N0}";

            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
        }

        private void DgvOrders_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvOrders.Columns[e.ColumnIndex].Name == "Actions")
            {
                int orderId = Convert.ToInt32(dgvOrders.Rows[e.RowIndex].Cells["Id"].Value);
                var cellRect = dgvOrders.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                
                if (e.X < cellRect.Width / 2)
                {
                    // Edit
                    MessageBox.Show("Edit Order form is under development.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Delete / Cancel Order
                    if (MessageBox.Show("Are you sure you want to cancel this order?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            bool success = _salesRepo.CancelOrder(orderId);
                            if (success)
                            {
                                MessageBox.Show("Order cancelled successfully!");
                            }
                            else
                            {
                                MessageBox.Show("Failed to cancel order. It might already be cancelled or not allowed.");
                            }
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

        private void DgvOrders_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Custom Paint for Actions
            if (dgvOrders.Columns[e.ColumnIndex].Name == "Actions")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                var rect = e.CellBounds;
                var editRect = new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height);
                var delRect = new Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height);
                
                TextRenderer.DrawText(e.Graphics, "✏️", e.CellStyle.Font, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                TextRenderer.DrawText(e.Graphics, "🗑️", e.CellStyle.Font, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                
                e.Handled = true;
            }
            // Custom Paint for Customer Name
            else if (dgvOrders.Columns[e.ColumnIndex].Name == "CustomerName")
            {
                e.PaintBackground(e.CellBounds, true);

                string name = e.Value?.ToString() ?? "Unknown";
                string email = dgvOrders.Rows[e.RowIndex].Cells["CustomerEmail"].Value?.ToString() ?? "";

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                
                using (var brush = new SolidBrush(ThemeManager.TextPrimary))
                {
                    g.DrawString(name, new Font("Segoe UI", 9.5F, FontStyle.Bold), brush, new RectangleF(e.CellBounds.X, e.CellBounds.Y + 12, e.CellBounds.Width, e.CellBounds.Height), format);
                }

                using (var brush = new SolidBrush(ThemeManager.TextSecondary))
                {
                    g.DrawString(email, new Font("Segoe UI", 8.5F), brush, new RectangleF(e.CellBounds.X, e.CellBounds.Y + 32, e.CellBounds.Width, e.CellBounds.Height), format);
                }

                e.Handled = true;
            }
            else if (dgvOrders.Columns[e.ColumnIndex].Name == "Status")
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "";

                Color bgColor = ThemeManager.TextBoxBorder;
                Color textColor = ThemeManager.TextPrimary;

                if (status.ToUpper() == "PENDING" || status.ToUpper() == "PENDING ORDERS" || status.ToUpper() == "PROCESSING" || status.ToUpper() == "AWAITING") { bgColor = Color.FromArgb(40, 41, 128, 185); textColor = Color.FromArgb(41, 128, 185); }
                else if (status.ToUpper() == "SHIPPED") { bgColor = Color.FromArgb(40, 243, 156, 18); textColor = Color.FromArgb(243, 156, 18); }
                else if (status.ToUpper() == "DELIVERED" || status.ToUpper() == "COMPLETED") { bgColor = Color.FromArgb(40, 46, 204, 113); textColor = Color.FromArgb(46, 204, 113); }
                else if (status.ToUpper() == "CANCELLED" || status.ToUpper() == "REFUNDED" || status.ToUpper() == "FLAGGED") { bgColor = Color.FromArgb(40, 231, 76, 60); textColor = Color.FromArgb(231, 76, 60); }

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                SizeF textSize = g.MeasureString(status, new Font("Segoe UI", 8F, FontStyle.Bold));
                RectangleF badgeRect = new RectangleF(e.CellBounds.X + (e.CellBounds.Width - textSize.Width - 20) / 2, e.CellBounds.Y + (e.CellBounds.Height - textSize.Height - 10) / 2, textSize.Width + 20, textSize.Height + 10);

                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillRoundedRectangle(brush, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height, 10);
                }

                using (var brush = new SolidBrush(textColor))
                {
                    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(status, new Font("Segoe UI", 8F, FontStyle.Bold), brush, badgeRect, format);
                }

                e.Handled = true;
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

            // Filters
            pnlFilters.BackColor = ThemeManager.Background;
            pnlFilters.CustomBorderColor = ThemeManager.TextBoxBorder;
            pnlFilters.FillColor = ThemeManager.CardBackground;

            cbStatus.FillColor = ThemeManager.TextBoxBackground;
            cbStatus.ForeColor = ThemeManager.TextPrimary;
            cbStatus.BorderColor = ThemeManager.TextBoxBorder;

            cbDate.FillColor = ThemeManager.TextBoxBackground;
            cbDate.ForeColor = ThemeManager.TextPrimary;
            cbDate.BorderColor = ThemeManager.TextBoxBorder;

            foreach (Control c in pnlFilters.Controls)
            {
                if (c is Label l && l.Text != "Total Orders: 0") l.ForeColor = ThemeManager.TextSecondary;
            }

            lblTotalOrders.BackColor = ThemeManager.TextBoxBackground;
            lblTotalOrders.ForeColor = ThemeManager.TextSecondary;
            lblTotalOrders.Invalidate();

            // Grid
            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            dgvOrders.BackgroundColor = ThemeManager.CardBackground;
            dgvOrders.GridColor = ThemeManager.TextBoxBorder;
            dgvOrders.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvOrders.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvOrders.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvOrders.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            
            dgvOrders.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvOrders.AlternatingRowsDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvOrders.AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvOrders.AlternatingRowsDefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;

            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }
}
