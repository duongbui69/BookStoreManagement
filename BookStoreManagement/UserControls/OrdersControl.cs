using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.UserControls
{
    public partial class OrdersControl : UserControl
    {
        private OrderRepository _repo;
        
        private Panel pnlHeader;
        private Label lblTitle;
        private TextBox txtSearch;
        private Button btnExport;
        private Button btnFilter;

        private FlowLayoutPanel flpCards;
        private FlowLayoutPanel flpTabs;
        private Button btnTabPending;
        private Button btnTabCompleted;
        private Button btnTabRefunded;

        private DataGridView dgvOrders;
        private PaginationControl paginationControl;
        private FlowLayoutPanel flpFooterCards;

        private int _currentPage = 1;
        private int _pageSize = 10;
        private string _currentTab = "Pending Orders";

        public OrdersControl()
        {
            _repo = new OrderRepository();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += OrdersControl_Load;
            this.Resize += OrdersControl_Resize;
        }

        private void InitializeUI()
        {
            this.BackColor = ThemeManager.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(30);

            // Header
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
            lblTitle = new Label { Text = "Orders & Refunds", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            
            txtSearch = new TextBox { Width = 300, Font = new Font("Segoe UI", 12F), PlaceholderText = "Search Order ID, Customer..." };
            txtSearch.TextChanged += (s, e) => { _currentPage = 1; LoadData(); };

            btnExport = new Button { Text = "Export CSV", Width = 120, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnFilter = new Button { Text = "Advanced Filter", Width = 140, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(txtSearch);
            pnlHeader.Controls.Add(btnFilter);
            pnlHeader.Controls.Add(btnExport);
            this.Controls.Add(pnlHeader);

            // Cards
            flpCards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 130, Margin = new Padding(0, 20, 0, 20), WrapContents = false, BackColor = Color.Transparent };
            this.Controls.Add(flpCards);

            // Tabs
            flpTabs = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };
            btnTabPending = CreateTabButton("Pending Orders");
            btnTabCompleted = CreateTabButton("Completed");
            btnTabRefunded = CreateTabButton("Refunded");
            flpTabs.Controls.Add(btnTabPending);
            flpTabs.Controls.Add(btnTabCompleted);
            flpTabs.Controls.Add(btnTabRefunded);
            this.Controls.Add(flpTabs);

            // Pagination
            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadData(); };
            this.Controls.Add(paginationControl);

            // Footer Cards
            flpFooterCards = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 120, Margin = new Padding(0, 20, 0, 0), WrapContents = false, BackColor = Color.Transparent };
            this.Controls.Add(flpFooterCards);

            // DataGridView
            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 60 }
            };
            dgvOrders.CellPainting += DgvOrders_CellPainting;
            this.Controls.Add(dgvOrders);
            
            // Re-order controls
            dgvOrders.BringToFront();
            flpTabs.BringToFront();
            flpCards.BringToFront();
            pnlHeader.BringToFront();

            ApplyTheme();
        }

        private Button CreateTabButton(string text)
        {
            var btn = new Button { Text = text, AutoSize = true, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => { _currentTab = text; _currentPage = 1; UpdateTabs(); LoadData(); };
            return btn;
        }

        private void UpdateTabs()
        {
            btnTabPending.ForeColor = _currentTab == "Pending Orders" ? ThemeManager.TextPrimary : ThemeManager.TextSecondary;
            btnTabCompleted.ForeColor = _currentTab == "Completed" ? ThemeManager.TextPrimary : ThemeManager.TextSecondary;
            btnTabRefunded.ForeColor = _currentTab == "Refunded" ? ThemeManager.TextPrimary : ThemeManager.TextSecondary;
            
            // Underline effect could be added here by painting
        }

        private void OrdersControl_Resize(object sender, EventArgs e)
        {
            if (pnlHeader != null)
            {
                btnExport.Location = new Point(pnlHeader.Width - 120, 10);
                btnFilter.Location = new Point(pnlHeader.Width - 270, 10);
                txtSearch.Location = new Point(lblTitle.Right + 50, 10);
            }
        }

        private void OrdersControl_Load(object sender, EventArgs e)
        {
            UpdateTabs();
            LoadData();
        }

        private void LoadData()
        {
            var stats = _repo.GetStats();
            
            flpCards.Controls.Clear();
            int cardWidth = Math.Max(200, (this.Width - 140) / 4);

            flpCards.Controls.Add(CreateStatCard("Pending Orders", stats.PendingOrders.ToString("N0"), Color.FromArgb(41, 128, 185), cardWidth));
            flpCards.Controls.Add(CreateStatCard("Completed Today", stats.CompletedToday.ToString("N0"), Color.FromArgb(46, 204, 113), cardWidth));
            flpCards.Controls.Add(CreateStatCard("Refund Requests", stats.RefundRequests.ToString("N0"), Color.FromArgb(231, 76, 60), cardWidth));
            flpCards.Controls.Add(CreateStatCard("Revenue (24h)", $"${stats.Revenue24h:N2}", Color.FromArgb(128, 90, 213), cardWidth));

            var (items, totalCount) = _repo.GetPagedOrders(_currentPage, _pageSize, _currentTab, txtSearch.Text);
            dgvOrders.DataSource = items;
            
            if (dgvOrders.Columns["CustomerEmail"] != null) dgvOrders.Columns["CustomerEmail"].Visible = false; // Hide email column, we paint it in CustomerName
            if (dgvOrders.Columns["Total"] != null) dgvOrders.Columns["Total"].DefaultCellStyle.Format = "C2";
            if (dgvOrders.Columns["Date"] != null) dgvOrders.Columns["Date"].DefaultCellStyle.Format = "MMM dd, yyyy \n HH:mm";

            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);

            // Rebuild footer cards based on width
            flpFooterCards.Controls.Clear();
            int fCardWidth = Math.Max(200, (this.Width - 100) / 3);
            flpFooterCards.Controls.Add(CreateFooterCard("System Tip", "Pending orders over 48 hours are\nautomatically flagged for review.", Color.FromArgb(41, 128, 185), fCardWidth));
            flpFooterCards.Controls.Add(CreateFooterCard("Fraud Protection", "All orders over $500 require dual\nauthorization before processing.", Color.FromArgb(46, 204, 113), fCardWidth));
            flpFooterCards.Controls.Add(CreateFooterCard("Smart Sorting", "Orders are prioritized by delivery\ndeadline and customer loyalty.", Color.FromArgb(128, 90, 213), fCardWidth));
        }

        private Panel CreateStatCard(string title, string value, Color iconColor, int width)
        {
            var pnl = new Panel { Width = width, Height = 100, BackColor = ThemeManager.CardBackground, Margin = new Padding(0, 0, 20, 0) };
            pnl.Paint += (s, e) => 
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var p = new Pen(ThemeManager.TextBoxBorder, 1);
                e.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
                
                // Draw Icon Box
                using var b = new SolidBrush(Color.FromArgb(30, iconColor));
                e.Graphics.FillRoundedRectangle(b, 20, 25, 40, 40, 8);
            };
            
            pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9F), ForeColor = ThemeManager.TextSecondary, Location = new Point(70, 25), AutoSize = true });
            pnl.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = ThemeManager.TextPrimary, Location = new Point(68, 45), AutoSize = true });
            return pnl;
        }

        private Panel CreateFooterCard(string title, string text, Color color, int width)
        {
            var pnl = new Panel { Width = width, Height = 90, BackColor = Color.FromArgb(10, color), Margin = new Padding(0, 0, 20, 0) };
            pnl.Paint += (s, e) => 
            {
                using var p = new Pen(Color.FromArgb(50, color), 1);
                e.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };
            pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = color, Location = new Point(40, 15), AutoSize = true });
            pnl.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI", 8.5F), ForeColor = ThemeManager.TextSecondary, Location = new Point(40, 40), AutoSize = true });
            return pnl;
        }

        private void DgvOrders_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Custom Paint for Customer Name (Index 2)
            if (dgvOrders.Columns[e.ColumnIndex].Name == "CustomerName")
            {
                e.PaintBackground(e.CellBounds, true);
                
                string name = e.Value?.ToString() ?? "Unknown";
                string email = dgvOrders.Rows[e.RowIndex].Cells["CustomerEmail"].Value?.ToString() ?? "";
                string initials = name.Length > 0 ? name.Substring(0, 1).ToUpper() : "?";
                if (name.Contains(" ")) initials = name.Split(' ')[0].Substring(0, 1).ToUpper() + name.Split(' ')[1].Substring(0, 1).ToUpper();

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw Avatar Circle
                Rectangle avatarRect = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 10, 35, 35);
                using (var brush = new SolidBrush(ThemeManager.TextBoxBorder))
                {
                    g.FillEllipse(brush, avatarRect);
                }
                
                using (var brush = new SolidBrush(ThemeManager.TextPrimary))
                {
                    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(initials, new Font("Segoe UI", 9F, FontStyle.Bold), brush, avatarRect, format);
                }

                // Draw Name
                using (var brush = new SolidBrush(ThemeManager.TextPrimary))
                {
                    g.DrawString(name, new Font("Segoe UI", 9.5F, FontStyle.Bold), brush, e.CellBounds.X + 55, e.CellBounds.Y + 12);
                }

                // Draw Email
                using (var brush = new SolidBrush(ThemeManager.TextSecondary))
                {
                    g.DrawString(email, new Font("Segoe UI", 8.5F), brush, e.CellBounds.X + 55, e.CellBounds.Y + 32);
                }

                e.Handled = true;
            }
            else if (dgvOrders.Columns[e.ColumnIndex].Name == "Status")
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "";
                
                Color bgColor = ThemeManager.TextBoxBorder;
                Color textColor = ThemeManager.TextPrimary;

                if (status == "PROCESSING") { bgColor = Color.FromArgb(40, 41, 128, 185); textColor = Color.FromArgb(41, 128, 185); }
                else if (status == "AWAITING") { bgColor = Color.FromArgb(40, 46, 204, 113); textColor = Color.FromArgb(46, 204, 113); }
                else if (status == "FLAGGED" || status == "Refunded") { bgColor = Color.FromArgb(40, 231, 76, 60); textColor = Color.FromArgb(231, 76, 60); }
                else if (status == "Completed") { bgColor = Color.FromArgb(40, 46, 204, 113); textColor = Color.FromArgb(46, 204, 113); }

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                SizeF textSize = g.MeasureString(status.ToUpper(), new Font("Segoe UI", 8F, FontStyle.Bold));
                RectangleF badgeRect = new RectangleF(e.CellBounds.X + 10, e.CellBounds.Y + (e.CellBounds.Height - textSize.Height - 10) / 2, textSize.Width + 20, textSize.Height + 10);

                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillRoundedRectangle(brush, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height, 10);
                }

                using (var brush = new SolidBrush(textColor))
                {
                    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(status.ToUpper(), new Font("Segoe UI", 8F, FontStyle.Bold), brush, badgeRect, format);
                }

                e.Handled = true;
            }
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
            UpdateTabs();
            LoadData(); // Redraw cards and grid
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            
            btnExport.BackColor = ThemeManager.CardBackground;
            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            btnFilter.BackColor = ThemeManager.CardBackground;
            btnFilter.ForeColor = ThemeManager.TextPrimary;
            btnFilter.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            txtSearch.BackColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;

            btnTabPending.BackColor = Color.Transparent;
            btnTabCompleted.BackColor = Color.Transparent;
            btnTabRefunded.BackColor = Color.Transparent;

            dgvOrders.BackgroundColor = ThemeManager.CardBackground;
            dgvOrders.GridColor = ThemeManager.TextBoxBorder;
            dgvOrders.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvOrders.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvOrders.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvOrders.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }
}
