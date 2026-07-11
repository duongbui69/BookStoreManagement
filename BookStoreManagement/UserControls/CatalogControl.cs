using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.UserControls
{
    public partial class CatalogControl : UserControl
    {
        private CatalogRepository _repo;
        
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        private TextBox txtSearch;
        private Button btnExport;
        private Button btnAdd;

        private FlowLayoutPanel flpCards;
        private Panel pnlFilters;
        private DataGridView dgvBooks;
        private PaginationControl paginationControl;
        
        private int _currentPage = 1;
        private int _pageSize = 10;

        public CatalogControl()
        {
            _repo = new CatalogRepository();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += CatalogControl_Load;
            this.Resize += CatalogControl_Resize;
        }

        private void InitializeUI()
        {
            this.BackColor = ThemeManager.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(30);

            // Header
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
            
            lblTitle = new Label { Text = "Catalog Management", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "View and manage the comprehensive book repository.", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(0, 30) };
            
            btnAdd = new Button { Text = "+ Add New Book", Width = 150, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnExport = new Button { Text = "Export CSV", Width = 120, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

            txtSearch = new TextBox { Width = 300, Font = new Font("Segoe UI", 12F), PlaceholderText = "Search by book title, ISBN..." };
            txtSearch.TextChanged += (s, e) => { _currentPage = 1; LoadData(); };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubTitle);
            pnlHeader.Controls.Add(btnAdd);
            pnlHeader.Controls.Add(btnExport);
            pnlHeader.Controls.Add(txtSearch);
            this.Controls.Add(pnlHeader);

            // Cards
            flpCards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 130, Margin = new Padding(0, 20, 0, 20), WrapContents = false, BackColor = Color.Transparent };
            this.Controls.Add(flpCards);

            // Filters
            pnlFilters = new Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0, 0, 0, 20) };
            Label lblCat = new Label { Text = "Category:", AutoSize = true, Location = new Point(0, 20), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            ComboBox cbCat = new ComboBox { Width = 150, Location = new Point(70, 15), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            cbCat.Items.Add("All Categories"); cbCat.SelectedIndex = 0;
            pnlFilters.Controls.Add(lblCat); pnlFilters.Controls.Add(cbCat);
            this.Controls.Add(pnlFilters);

            // DataGridView
            dgvBooks = new DataGridView
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
            dgvBooks.CellFormatting += DgvBooks_CellFormatting;
            this.Controls.Add(dgvBooks);

            // Pagination
            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadData(); };
            this.Controls.Add(paginationControl);
            
            // Re-order controls because of Dock
            dgvBooks.BringToFront();
            pnlFilters.BringToFront();
            flpCards.BringToFront();
            pnlHeader.BringToFront();

            ApplyTheme();
        }

        private void CatalogControl_Resize(object sender, EventArgs e)
        {
            if (pnlHeader != null)
            {
                btnAdd.Location = new Point(pnlHeader.Width - 150, 10);
                btnExport.Location = new Point(pnlHeader.Width - 280, 10);
                txtSearch.Location = new Point(lblTitle.Right + 50, 15);
            }
        }

        private void CatalogControl_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            var stats = _repo.GetStats();
            
            flpCards.Controls.Clear();
            int cardWidth = Math.Max(200, (this.Width - 140) / 4);

            flpCards.Controls.Add(CreateCard("TOTAL TITLES", stats.TotalTitles.ToString("N0"), Color.FromArgb(41, 128, 185), cardWidth));
            flpCards.Controls.Add(CreateCard("ACTIVE CATEGORIES", stats.ActiveCategories.ToString("N0"), Color.FromArgb(128, 90, 213), cardWidth));
            flpCards.Controls.Add(CreateCard("IN STOCK VALUE", $"${stats.InStockValue:N2}", Color.FromArgb(46, 204, 113), cardWidth));
            flpCards.Controls.Add(CreateCard("LOW STOCK ALERTS", stats.LowStockAlerts.ToString(), Color.FromArgb(231, 76, 60), cardWidth));

            var (items, totalCount) = _repo.GetPagedCatalogBooks(_currentPage, _pageSize, txtSearch.Text);
            dgvBooks.DataSource = items;
            
            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
            
            if (dgvBooks.Columns["Price"] != null) dgvBooks.Columns["Price"].DefaultCellStyle.Format = "C2";
        }

        private Panel CreateCard(string title, string value, Color borderColor, int width)
        {
            var pnl = new Panel { Width = width, Height = 100, BackColor = ThemeManager.CardBackground, Margin = new Padding(0, 0, 20, 0) };
            pnl.Controls.Add(new Panel { Width = 4, Dock = DockStyle.Left, BackColor = borderColor });
            pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.TextSecondary, Location = new Point(20, 20), AutoSize = true });
            pnl.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = ThemeManager.TextPrimary, Location = new Point(18, 45), AutoSize = true });
            return pnl;
        }

        private void DgvBooks_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvBooks.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                if (status == "IN STOCK") e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                else if (status == "OUT OF STOCK") e.CellStyle.ForeColor = ThemeManager.TextSecondary;
                else e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
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
            
            btnAdd.BackColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;
            btnAdd.FlatAppearance.BorderSize = 0;

            btnExport.BackColor = ThemeManager.CardBackground;
            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            txtSearch.BackColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;

            pnlFilters.BackColor = ThemeManager.CardBackground;

            dgvBooks.BackgroundColor = ThemeManager.CardBackground;
            dgvBooks.GridColor = ThemeManager.TextBoxBorder;
            dgvBooks.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvBooks.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvBooks.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvBooks.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvBooks.EnableHeadersVisualStyles = false;
            dgvBooks.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvBooks.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }
}
