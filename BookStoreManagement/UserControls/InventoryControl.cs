using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.UserControls
{
    public partial class InventoryControl : UserControl
    {
        private InventoryRepository _repo;
        
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        private TextBox txtSearch;
        private Button btnExport;
        private Button btnImport;

        private FlowLayoutPanel flpCards;
        private DataGridView dgvWarehouses;
        private PaginationControl paginationControl;
        
        private int _currentPage = 1;
        private int _pageSize = 10;

        public InventoryControl()
        {
            _repo = new InventoryRepository();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += InventoryControl_Load;
            this.Resize += InventoryControl_Resize;
        }

        private void InitializeUI()
        {
            this.BackColor = ThemeManager.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(30);

            // Header
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
            
            lblTitle = new Label { Text = "Overview & Stock Analysis", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Real-time inventory synchronization across 3 warehouses", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(0, 30) };
            
            btnImport = new Button { Text = "IMPORT INVENTORY", Width = 150, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnExport = new Button { Text = "EXPORT CSV", Width = 120, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

            txtSearch = new TextBox { Width = 300, Font = new Font("Segoe UI", 12F), PlaceholderText = "Search by ISBN, title or SKU..." };
            txtSearch.TextChanged += (s, e) => { _currentPage = 1; LoadData(); };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubTitle);
            pnlHeader.Controls.Add(btnImport);
            pnlHeader.Controls.Add(btnExport);
            pnlHeader.Controls.Add(txtSearch);
            this.Controls.Add(pnlHeader);

            // Cards
            flpCards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 130, Margin = new Padding(0, 20, 0, 20), WrapContents = false, BackColor = Color.Transparent };
            this.Controls.Add(flpCards);

            // DataGridView
            dgvWarehouses = new DataGridView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 45 }
            };
            dgvWarehouses.CellFormatting += DgvWarehouses_CellFormatting;
            this.Controls.Add(dgvWarehouses);

            // Pagination
            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadData(); };
            this.Controls.Add(paginationControl);
            
            // Re-order controls because of Dock
            dgvWarehouses.BringToFront();
            flpCards.BringToFront();
            pnlHeader.BringToFront();

            ApplyTheme();
        }

        private void InventoryControl_Resize(object sender, EventArgs e)
        {
            if (pnlHeader != null)
            {
                btnExport.Location = new Point(pnlHeader.Width - 130, 10);
                btnImport.Location = new Point(pnlHeader.Width - 290, 10);
                txtSearch.Location = new Point(lblTitle.Right + 50, 15);
            }
        }

        private void InventoryControl_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            var stats = _repo.GetStats();
            
            flpCards.Controls.Clear();
            int cardWidth = Math.Max(200, (this.Width - 140) / 4);

            flpCards.Controls.Add(CreateCard("TOTAL ITEMS", stats.TotalItems.ToString("N0"), Color.FromArgb(41, 128, 185), cardWidth));
            flpCards.Controls.Add(CreateCard("OUT OF STOCK", stats.OutOfStock.ToString("N0"), Color.FromArgb(231, 76, 60), cardWidth));
            flpCards.Controls.Add(CreateCard("STOCK VALUE", $"${stats.StockValue:N2}", Color.FromArgb(128, 90, 213), cardWidth));
            flpCards.Controls.Add(CreateCard("REORDER POINT", stats.ReorderPoint.ToString("N0"), Color.FromArgb(46, 204, 113), cardWidth));

            var (items, totalCount) = _repo.GetPagedInventoryItems(_currentPage, _pageSize, txtSearch.Text);
            dgvWarehouses.DataSource = items;
            
            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
            
            if (dgvWarehouses.Columns["UnitPrice"] != null) dgvWarehouses.Columns["UnitPrice"].DefaultCellStyle.Format = "C2";
        }

        private Panel CreateCard(string title, string value, Color borderColor, int width)
        {
            var pnl = new Panel { Width = width, Height = 100, BackColor = ThemeManager.CardBackground, Margin = new Padding(0, 0, 20, 0) };
            pnl.Controls.Add(new Panel { Width = 4, Dock = DockStyle.Left, BackColor = borderColor });
            pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.TextSecondary, Location = new Point(20, 20), AutoSize = true });
            pnl.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = ThemeManager.TextPrimary, Location = new Point(18, 45), AutoSize = true });
            return pnl;
        }

        private void DgvWarehouses_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvWarehouses.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                if (status == "IN STOCK") e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                else if (status == "OUT OF STOCK") e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                else e.CellStyle.ForeColor = Color.FromArgb(243, 156, 18); // Orange for Low Stock
            }
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
            LoadData(); // Redraw cards with new theme
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            
            btnExport.BackColor = ThemeManager.ButtonFill;
            btnExport.ForeColor = ThemeManager.ButtonText;
            btnExport.FlatAppearance.BorderSize = 0;

            btnImport.BackColor = ThemeManager.CardBackground;
            btnImport.ForeColor = ThemeManager.TextPrimary;
            btnImport.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            txtSearch.BackColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;

            dgvWarehouses.BackgroundColor = ThemeManager.CardBackground;
            dgvWarehouses.GridColor = ThemeManager.TextBoxBorder;
            dgvWarehouses.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvWarehouses.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvWarehouses.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvWarehouses.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvWarehouses.EnableHeadersVisualStyles = false;
            dgvWarehouses.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvWarehouses.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvWarehouses.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }
}
