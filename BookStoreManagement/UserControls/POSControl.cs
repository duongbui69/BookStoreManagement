using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.ViewModels;
using Guna.UI2.WinForms;

namespace BookStoreManagement.UserControls
{
    public partial class POSControl : UserControl
    {
        private StoreBookInventoryService _inventoryService;
        private CategoryService _categoryService;
        private SalesOrderService _orderService;

        private SplitContainer splitContainer;
        private Guna2Panel pnlLeft;
        private Guna2Panel pnlRight;
        
        // Left
        private Guna2TextBox txtSearch;
        private Guna2ComboBox cbCategories;
        private System.Windows.Forms.DataGridView dgvProducts;
        private Label lblPageInfo;
        private Guna.UI2.WinForms.Guna2Button btnPrevPage;
        private Guna.UI2.WinForms.Guna2Button btnNextPage;

        // Right
        private Guna.UI2.WinForms.Guna2ComboBox cboCustomer;
        private System.Windows.Forms.DataGridView dgvCart;
        private Label lblTotalAmount;
        private Label lblDiscount;
        private Label lblFinalAmount;
        private Guna2Button btnCheckout;
        private Guna2Button btnCash;
                private Guna.UI2.WinForms.Guna2Button btnTransfer;
        private Guna.UI2.WinForms.Guna2Panel pnlTop;
        private Guna.UI2.WinForms.Guna2Panel pnlBot;
        private Guna.UI2.WinForms.Guna2Panel pnlCustomer;
        private Guna.UI2.WinForms.Guna2Panel pnlSummary;
        private Guna.UI2.WinForms.Guna2Panel pnlCart;
        private Guna.UI2.WinForms.Guna2Panel pnlCartTop;
        private System.Windows.Forms.Label lblCustTitle;
        private System.Windows.Forms.Label lblCartTitle;
        private System.Windows.Forms.Label lblTotalText;
        private System.Windows.Forms.Label lblDiscountText;
        private System.Windows.Forms.Label lblFinalText;
        private Guna.UI2.WinForms.Guna2Panel lineSummary;

        // Data
        private List<StoreBookInventoryViewModel> _allBooks = new List<StoreBookInventoryViewModel>();
        private List<StoreBookInventoryViewModel> _filteredBooks = new List<StoreBookInventoryViewModel>();
        private List<Category> _categories = new List<Category>();
        
        private string _currentCategory = "Tất cả";
        private string _selectedPaymentMethod = "Tiền mặt";
        
        // Cart
        private Dictionary<int, CartItem> _cart = new Dictionary<int, CartItem>();

        // Pagination
        private int _currentPage = 1;
        private int _pageSize = 15;

        public POSControl()
        {
            _inventoryService = new StoreBookInventoryService();
            _categoryService = new CategoryService();
            _orderService = new SalesOrderService();

            InitializeComponent();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ThemeManager_ThemeChanged(null, EventArgs.Empty);
            this.Load += POSControl_Load;
            this.Resize += POSControl_Resize;
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24);
            this.BackColor = ThemeManager.Background;

            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                SplitterWidth = 12,
            };

            pnlLeft = new Guna2Panel { Dock = DockStyle.Fill, Margin = new Padding(0), BorderRadius = 12, FillColor = ThemeManager.CardBackground, BorderColor = ThemeManager.TextBoxBorder, BorderThickness = 1 };
            pnlRight = new Guna2Panel { Dock = DockStyle.Fill, Margin = new Padding(0), BorderRadius = 12, FillColor = Color.Transparent };

            splitContainer.Panel1.Controls.Add(pnlLeft);
            splitContainer.Panel2.Controls.Add(pnlRight);
            
            splitContainer.SplitterMoved += SplitContainer_SplitterMoved;

            this.Load += (s, e) => {
                if (this.Width > 0)
                    splitContainer.SplitterDistance = (int)(this.Width * 0.65);
            };

            this.Controls.Add(splitContainer);

            BuildLeftPanel();
            BuildRightPanel();
        }

        private void POSControl_Resize(object sender, EventArgs e)
        {
            // Adjust page size based on available height for the grid
            if (dgvProducts != null && pnlLeft.Height > 200)
            {
                // Roughly calculate how many rows fit in the grid
                int rowHeight = dgvProducts.RowTemplate.Height;
                int headerHeight = dgvProducts.ColumnHeadersHeight;
                int availableHeight = pnlLeft.Height - 160; // Subtract top panel and bottom panel heights roughly
                
                int newPageSize = Math.Max(5, (availableHeight - headerHeight) / rowHeight);
                if (_pageSize != newPageSize)
                {
                    _pageSize = newPageSize;
                    _currentPage = 1;
                    RenderProducts();
                }
            }
            
            EnforceSplitterLimit();
        }

        private void SplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            EnforceSplitterLimit();
        }
        
        private void EnforceSplitterLimit()
        {
            if (splitContainer.Width > 0)
            {
                int maxDistance = (int)(splitContainer.Width * 0.80);
                if (splitContainer.SplitterDistance > maxDistance)
                {
                    splitContainer.SplitterDistance = maxDistance;
                }
            }
        }

        private void BuildLeftPanel()
        {
            // Top search bar & category
            pnlTop = new Guna2Panel { Dock = DockStyle.Top, Height = 70, CustomBorderThickness = new Padding(0, 0, 0, 1), CustomBorderColor = ThemeManager.TextBoxBorder, FillColor = Color.Transparent };
            
            cbCategories = new Guna2ComboBox
            {
                Location = new Point(15, 15),
                Size = new Size(200, 40),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            cbCategories.SelectedIndexChanged += (s, e) => {
                if (cbCategories.SelectedItem != null)
                {
                    _currentCategory = cbCategories.SelectedItem.ToString();
                    _currentPage = 1;
                    FilterBooks();
                }
            };

            txtSearch = new Guna2TextBox
            {
                Location = new Point(230, 15),
                Size = new Size(300, 40),
                BorderRadius = 8,
                PlaceholderText = "Tìm theo mã vạch, tên sách...",
                IconLeftOffset = new Point(5, 0)
            };
            // Real-time search
            txtSearch.TextChanged += (s, e) => { 
                _currentPage = 1;
                FilterBooks(); 
            };
            
            var btnScan = CreateOutlineButton("Quét mã", 545, 15);
            
            pnlTop.Controls.Add(cbCategories);
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Controls.Add(btnScan);
            pnlLeft.Controls.Add(pnlTop);

            // Bottom info (Pagination)
            pnlBot = new Guna2Panel { Dock = DockStyle.Bottom, Height = 50, CustomBorderThickness = new Padding(0, 1, 0, 0), CustomBorderColor = ThemeManager.TextBoxBorder, FillColor = Color.Transparent };
            lblPageInfo = new Label { Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 10F), ForeColor = ThemeManager.TextSecondary };
            
            btnPrevPage = CreateOutlineButton("<", pnlBot.Width - 120, 10, 40);
            btnPrevPage.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnPrevPage.Click += (s, e) => { if (_currentPage > 1) { _currentPage--; RenderProducts(); } };
            
            btnNextPage = CreateOutlineButton(">", pnlBot.Width - 60, 10, 40);
            btnNextPage.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnNextPage.Click += (s, e) => { 
                int maxPage = (int)Math.Ceiling((double)_filteredBooks.Count / _pageSize);
                if (_currentPage < maxPage) { _currentPage++; RenderProducts(); } 
            };

            pnlBot.Controls.Add(lblPageInfo);
            pnlBot.Controls.Add(btnPrevPage);
            pnlBot.Controls.Add(btnNextPage);
            
            pnlLeft.Controls.Add(pnlBot);

            // DataGridView Products
            dgvProducts = new System.Windows.Forms.DataGridView
            {
                AutoGenerateColumns = false,
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowTemplate = { Height = 45 },
                Margin = new Padding(15),
                Cursor = Cursors.Hand
            };
            ThemeManager.ApplyDataGridViewStyle(dgvProducts);

            dgvProducts.Columns.Add("Code", "MÃ SÁCH");
            dgvProducts.Columns["Code"].Width = 100;
            dgvProducts.Columns.Add("Name", "TÊN SÁCH");
            dgvProducts.Columns["Name"].FillWeight = 200;
            dgvProducts.Columns.Add("Price", "GIÁ BÁN");
            dgvProducts.Columns["Price"].DefaultCellStyle.Format = "N0";
            dgvProducts.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns.Add("Stock", "TỒN KHO");
            dgvProducts.Columns["Stock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvProducts.Columns["Stock"].Width = 80;

            // Click to add
            dgvProducts.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var book = dgvProducts.Rows[e.RowIndex].Tag as StoreBookInventoryViewModel;
                    if (book != null) AddToCart(book);
                }
            };
            
            pnlLeft.Controls.Add(dgvProducts);

            pnlTop.BringToFront();
            pnlBot.BringToFront();
            dgvProducts.BringToFront();
        }

        private void BuildRightPanel()
        {
            // Customer Card
            pnlCustomer = new Guna2Panel { Dock = DockStyle.Top, Height = 120, BorderRadius = 12, FillColor = ThemeManager.CardBackground, BorderColor = ThemeManager.TextBoxBorder, BorderThickness = 1, Margin = new Padding(0, 0, 0, 12) };
            lblCustTitle = new Label { Text = "👤 Khách hàng", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
            var btnAddCust = new Label { Text = "Thêm mới", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.ButtonFill, Location = new Point(pnlCustomer.Width - 80, 18), AutoSize = true, Anchor = AnchorStyles.Right | AnchorStyles.Top, Cursor = Cursors.Hand };
            
            
            cboCustomer = new Guna.UI2.WinForms.Guna2ComboBox
            {
                Location = new Point(15, 50),
                Size = new Size(pnlCustomer.Width - 30, 40),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            
            // Allow adding new customer
            btnAddCust.Click += async (s, e) => {
                using (var form = new BookStoreManagement.Forms.CustomerForm(null))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        await LoadCustomersAsync();
                    }
                }
            };
pnlCustomer.Controls.AddRange(new Control[] { lblCustTitle, btnAddCust, cboCustomer });
            pnlRight.Controls.Add(pnlCustomer);

            // Summary Card (Bottom)
            pnlSummary = new Guna2Panel { Dock = DockStyle.Bottom, Height = 220, BorderRadius = 12, FillColor = ThemeManager.CardBackground, BorderColor = ThemeManager.TextBoxBorder, BorderThickness = 1, Margin = new Padding(0, 12, 0, 0) };
            
            lblTotalText = new Label { Text = "Tổng tiền hàng", Font = new Font("Segoe UI", 11F), ForeColor = ThemeManager.TextSecondary, Location = new Point(15, 15), AutoSize = true };
            lblTotalAmount = new Label { Text = "0 ₫", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(pnlSummary.Width - 100, 15), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, TextAlign = ContentAlignment.MiddleRight };
            
            lblDiscountText = new Label { Text = "Giảm giá", Font = new Font("Segoe UI", 11F), ForeColor = ThemeManager.TextSecondary, Location = new Point(15, 45), AutoSize = true };
            lblDiscount = new Label { Text = "- 0 ₫", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.Red, Location = new Point(pnlSummary.Width - 100, 45), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, TextAlign = ContentAlignment.MiddleRight };

            lineSummary = new Guna2Panel { Location = new Point(15, 75), Size = new Size(pnlSummary.Width - 30, 1), FillColor = ThemeManager.TextBoxBorder, Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right };

            lblFinalText = new Label { Text = "Khách cần trả", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(15, 90), AutoSize = true };
            lblFinalAmount = new Label { Text = "0 ₫", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = ThemeManager.ButtonFill, Location = new Point(pnlSummary.Width - 150, 85), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, TextAlign = ContentAlignment.MiddleRight };

            btnCash = CreateOutlineButton("Tiền mặt", 15, 130, 120);
            btnCash.Click += PaymentMethod_Click;
            
            btnTransfer = CreateOutlineButton("Chuyển khoản", 145, 130, 120);
            btnTransfer.Click += PaymentMethod_Click;

            UpdatePaymentMethodUI();

            var btnSave = CreateOutlineButton("Lưu tạm", 15, 175, 100);
            btnCheckout = new Guna2Button
            {
                Text = "Thanh toán (F9)",
                Location = new Point(125, 175),
                Size = new Size(pnlSummary.Width - 140, 35),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 8,
                FillColor = ThemeManager.ButtonFill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            btnCheckout.Click += BtnCheckout_Click;

            pnlSummary.Controls.AddRange(new Control[] { lblTotalText, lblTotalAmount, lblDiscountText, lblDiscount, lineSummary, lblFinalText, lblFinalAmount, btnCash, btnTransfer, btnSave, btnCheckout });
            pnlRight.Controls.Add(pnlSummary);

            // Cart Items Container (Middle)
            pnlCart = new Guna2Panel { Dock = DockStyle.Fill, BorderRadius = 12, FillColor = ThemeManager.CardBackground, BorderColor = ThemeManager.TextBoxBorder, BorderThickness = 1 };
            pnlCartTop = new Guna2Panel { Dock = DockStyle.Top, Height = 40, CustomBorderThickness = new Padding(0, 0, 0, 1), CustomBorderColor = ThemeManager.TextBoxBorder };
            lblCartTitle = new Label { Text = "🛒 Giỏ hàng", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            var btnClearCart = new Label { Text = "Xóa tất cả", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.Red, Location = new Point(pnlCart.Width - 70, 12), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Cursor = Cursors.Hand };
            btnClearCart.Click += (s, e) => { 
                // Return stock
                foreach (var item in _cart.Values)
                {
                    var book = _allBooks.FirstOrDefault(b => b.BookId == item.BookId);
                    if (book != null) book.Quantity += item.Quantity;
                }
                _cart.Clear(); 
                RenderCart(); 
                RenderProducts();
            };
            
            pnlCartTop.Controls.Add(lblCartTitle);
            pnlCartTop.Controls.Add(btnClearCart);
            pnlCart.Controls.Add(pnlCartTop);

            dgvCart = new System.Windows.Forms.DataGridView
            {
                AutoGenerateColumns = false,
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowTemplate = { Height = 45 }
            };
            ThemeManager.ApplyDataGridViewStyle(dgvCart);

            dgvCart.Columns.Add("Name", "TÊN SÁCH");
            dgvCart.Columns["Name"].ReadOnly = true;
            dgvCart.Columns["Name"].FillWeight = 200;
            
            var colQty = new DataGridViewTextBoxColumn
            {
                Name = "Qty",
                HeaderText = "SL",
                Width = 60
            };
            colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCart.Columns.Add(colQty);
            
            dgvCart.Columns.Add("Amount", "THÀNH TIỀN");
            dgvCart.Columns["Amount"].ReadOnly = true;
            dgvCart.Columns["Amount"].DefaultCellStyle.Format = "N0";
            dgvCart.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            var actionCol = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "",
                Text = "X",
                UseColumnTextForButtonValue = true,
                Width = 40,
                FlatStyle = FlatStyle.Flat
            };
            actionCol.DefaultCellStyle.ForeColor = Color.Red;
            dgvCart.Columns.Add(actionCol);

            dgvCart.CellValueChanged += DgvCart_CellValueChanged;
            dgvCart.CellValidating += DgvCart_CellValidating;
            dgvCart.CurrentCellDirtyStateChanged += DgvCart_CurrentCellDirtyStateChanged;
            dgvCart.CellContentClick += DgvCart_CellContentClick;

            pnlCart.Controls.Add(dgvCart);
            dgvCart.BringToFront();

            var pnlSpacer = new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Color.Transparent };
            pnlRight.Controls.Add(pnlSpacer);
            pnlRight.Controls.Add(pnlCart);
            
            pnlCustomer.BringToFront();
            pnlSpacer.BringToFront();
            pnlCart.BringToFront();
            pnlSummary.BringToFront();
        }

        private void PaymentMethod_Click(object sender, EventArgs e)
        {
            var btn = sender as Guna2Button;
            if (btn != null)
            {
                _selectedPaymentMethod = btn.Text;
                UpdatePaymentMethodUI();
            }
        }

        private void UpdatePaymentMethodUI()
        {
            if (btnCash == null || btnTransfer == null) return;
            
            if (_selectedPaymentMethod == "Tiền mặt")
            {
                btnCash.FillColor = Color.FromArgb(20, ThemeManager.ButtonFill);
                btnCash.ForeColor = ThemeManager.ButtonFill;
                btnCash.BorderColor = ThemeManager.ButtonFill;
                
                btnTransfer.FillColor = ThemeManager.Background;
                btnTransfer.ForeColor = ThemeManager.TextPrimary;
                btnTransfer.BorderColor = ThemeManager.TextBoxBorder;
            }
            else
            {
                btnTransfer.FillColor = Color.FromArgb(20, ThemeManager.ButtonFill);
                btnTransfer.ForeColor = ThemeManager.ButtonFill;
                btnTransfer.BorderColor = ThemeManager.ButtonFill;
                
                btnCash.FillColor = ThemeManager.Background;
                btnCash.ForeColor = ThemeManager.TextPrimary;
                btnCash.BorderColor = ThemeManager.TextBoxBorder;
            }
        }

        private void DgvCart_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgvCart.IsCurrentCellDirty) dgvCart.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DgvCart_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == dgvCart.Columns["Qty"].Index)
            {
                if (!int.TryParse(e.FormattedValue?.ToString(), out int qty) || qty <= 0)
                {
                    e.Cancel = true;
                    MessageBox.Show("Số lượng phải là số lớn hơn 0.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var item = dgvCart.Rows[e.RowIndex].Tag as CartItem;
                if (item != null)
                {
                    int diff = qty - item.Quantity; // if diff > 0, we need more stock
                    if (diff > 0 && diff > item.MaxStock) // wait, MaxStock is dynamic now since we change the main list
                    {
                        var book = _allBooks.FirstOrDefault(b => b.BookId == item.BookId);
                        if (book != null && diff > book.Quantity)
                        {
                            e.Cancel = true;
                            MessageBox.Show($"Tồn kho không đủ (chỉ còn {book.Quantity}).", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
        }

        private void DgvCart_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCart.Columns["Qty"].Index)
            {
                var row = dgvCart.Rows[e.RowIndex];
                var item = row.Tag as CartItem;
                
                if (item != null && int.TryParse(row.Cells["Qty"].Value?.ToString(), out int qty))
                {
                    int diff = qty - item.Quantity;
                    var book = _allBooks.FirstOrDefault(b => b.BookId == item.BookId);
                    if (book != null)
                    {
                        book.Quantity -= diff; // Update main memory stock
                    }
                    item.Quantity = qty;
                    RenderCart();
                    RenderProducts();
                }
            }
        }

        private void DgvCart_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCart.Columns["Delete"].Index)
            {
                var item = dgvCart.Rows[e.RowIndex].Tag as CartItem;
                if (item != null)
                {
                    var book = _allBooks.FirstOrDefault(b => b.BookId == item.BookId);
                    if (book != null) book.Quantity += item.Quantity; // return stock
                    
                    _cart.Remove(item.BookId);
                    RenderCart();
                    RenderProducts();
                }
            }
        }

        private Guna2Button CreateOutlineButton(string text, int x, int y, int width = 100)
        {
            return new Guna2Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 35),
                BorderRadius = 8,
                BorderThickness = 1,
                BorderColor = ThemeManager.TextBoxBorder,
                FillColor = ThemeManager.Background,
                ForeColor = ThemeManager.TextPrimary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            this.BackColor = ThemeManager.Background;
            if (pnlLeft != null) { pnlLeft.FillColor = ThemeManager.CardBackground; pnlLeft.BorderColor = ThemeManager.TextBoxBorder; }
            if (pnlRight != null) { pnlRight.FillColor = ThemeManager.Background; }
            if (pnlTop != null) { pnlTop.FillColor = ThemeManager.Background; pnlTop.CustomBorderColor = ThemeManager.TextBoxBorder; }
            if (pnlBot != null) { pnlBot.CustomBorderColor = ThemeManager.TextBoxBorder; }
            if (lblPageInfo != null) { lblPageInfo.ForeColor = ThemeManager.TextSecondary; }
            
            if (cbCategories != null) { cbCategories.FillColor = ThemeManager.Background; cbCategories.ForeColor = ThemeManager.TextPrimary; cbCategories.BorderColor = ThemeManager.TextBoxBorder; }
            if (txtSearch != null) { txtSearch.FillColor = ThemeManager.Background; txtSearch.ForeColor = ThemeManager.TextPrimary; txtSearch.BorderColor = ThemeManager.TextBoxBorder; }
            
            if (pnlCustomer != null) { pnlCustomer.FillColor = ThemeManager.CardBackground; pnlCustomer.BorderColor = ThemeManager.TextBoxBorder; }
            if (lblCustTitle != null) { lblCustTitle.ForeColor = ThemeManager.TextPrimary; }
            if (cboCustomer != null) { cboCustomer.FillColor = ThemeManager.Background; cboCustomer.ForeColor = ThemeManager.TextPrimary; cboCustomer.BorderColor = ThemeManager.TextBoxBorder; }
            
            if (pnlSummary != null) { pnlSummary.FillColor = ThemeManager.CardBackground; pnlSummary.BorderColor = ThemeManager.TextBoxBorder; }
            if (lblTotalText != null) { lblTotalText.ForeColor = ThemeManager.TextSecondary; }
            if (lblDiscountText != null) { lblDiscountText.ForeColor = ThemeManager.TextSecondary; }
            if (lblFinalText != null) { lblFinalText.ForeColor = ThemeManager.TextPrimary; }
            if (lblTotalAmount != null) { lblTotalAmount.ForeColor = ThemeManager.TextPrimary; }
            if (lineSummary != null) { lineSummary.FillColor = ThemeManager.TextBoxBorder; }
            
            if (pnlCart != null) { pnlCart.FillColor = ThemeManager.CardBackground; pnlCart.BorderColor = ThemeManager.TextBoxBorder; }
            if (pnlCartTop != null) { pnlCartTop.CustomBorderColor = ThemeManager.TextBoxBorder; }
            if (lblCartTitle != null) { lblCartTitle.ForeColor = ThemeManager.TextPrimary; }
            
            if (dgvProducts != null) { 
                ThemeManager.ApplyDataGridViewStyle(dgvProducts); 
                dgvProducts.BackgroundColor = ThemeManager.CardBackground;
            }
            if (dgvCart != null) { 
                ThemeManager.ApplyDataGridViewStyle(dgvCart); 
                dgvCart.BackgroundColor = ThemeManager.CardBackground;
            }
            UpdatePaymentMethodUI();
        }

        
        private BookStoreManagement.Services.CustomerService _customerService = new BookStoreManagement.Services.CustomerService();
        
        private async Task LoadCustomersAsync()
        {
            try
            {
                var customers = await _customerService.GetActiveAsync();
                
                // Add default guest customer at top
                customers.Insert(0, new BookStoreManagement.Models.Customer { Id = 0, FullName = "-- Khách vãng lai --" });
                
                if (cboCustomer != null)
                {
                    cboCustomer.DataSource = customers;
                    cboCustomer.DisplayMember = "FullName";
                    cboCustomer.ValueMember = "Id";
                    cboCustomer.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                // handle silently or log
            }
        }

        private async void POSControl_Load(object sender, EventArgs e)
        {
            await LoadCustomersAsync();
            if (this.IsDisposed) return;
            _categories = await _categoryService.GetActiveAsync();
            if (this.IsDisposed) return;
            
            cbCategories.Items.Clear();
            cbCategories.Items.Add("Tất cả");
            foreach (var cat in _categories)
            {
                cbCategories.Items.Add(cat.CategoryName);
            }
            cbCategories.SelectedIndex = 0;
            
            int currentStoreId = CurrentSession.StoreId ?? 1;
            
            _allBooks = await _inventoryService.GetByStoreIdAsync(currentStoreId);
            if (!this.IsDisposed) FilterBooks();
        }

        private void FilterBooks()
        {
            var query = _allBooks.AsEnumerable();
            
            if (_currentCategory != "Tất cả")
            {
                query = query.Where(b => b.CategoryName == _currentCategory);
            }

            string search = txtSearch.Text.ToLower().Trim();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.ToLower().Contains(search) || b.BookCode.ToLower().Contains(search));
            }

            _filteredBooks = query.ToList();
            
            // Adjust page if current page is empty after filter
            int maxPage = Math.Max(1, (int)Math.Ceiling((double)_filteredBooks.Count / _pageSize));
            if (_currentPage > maxPage) _currentPage = maxPage;
            
            if (!this.IsDisposed) RenderProducts();
        }

        private void RenderProducts()
        {
            dgvProducts.Rows.Clear();
            
            int skip = (_currentPage - 1) * _pageSize;
            var pageData = _filteredBooks.Skip(skip).Take(_pageSize).ToList();
            
            foreach (var item in pageData)
            {
                int rowIndex = dgvProducts.Rows.Add(
                    item.BookCode,
                    item.Title,
                    item.SellingPrice,
                    item.Quantity
                );
                dgvProducts.Rows[rowIndex].Tag = item;
                
                if (item.Quantity <= 0)
                {
                    dgvProducts.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Red;
                }
            }
            
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)_filteredBooks.Count / _pageSize));
            lblPageInfo.Text = $"Trang {_currentPage}/{totalPages} (Tổng: {_filteredBooks.Count} sản phẩm)";
            
            btnPrevPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < totalPages;
        }

        private void AddToCart(StoreBookInventoryViewModel item)
        {
            if (item.Quantity <= 0)
            {
                MessageBox.Show("Sách này đã hết hàng trong kho!", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_cart.ContainsKey(item.BookId))
            {
                _cart[item.BookId].Quantity++;
            }
            else
            {
                _cart.Add(item.BookId, new CartItem 
                { 
                    BookId = item.BookId, 
                    Title = item.Title, 
                    Code = item.BookCode, 
                    Price = item.SellingPrice, 
                    Quantity = 1,
                    MaxStock = item.Quantity
                });
            }
            
            // Reduce stock in memory
            item.Quantity--;
            
            RenderCart();
            if (!this.IsDisposed) RenderProducts();
        }

        private void RenderCart()
        {
            dgvCart.Rows.Clear();
            decimal total = 0;
            
            foreach (var item in _cart.Values)
            {
                total += item.Price * item.Quantity;
                
                int rowIndex = dgvCart.Rows.Add(
                    item.Title,
                    item.Quantity,
                    item.Price * item.Quantity
                );
                dgvCart.Rows[rowIndex].Tag = item;
            }

            lblTotalAmount.Text = $"{total:N0} ₫";
            lblFinalAmount.Text = $"{total:N0} ₫";
            
            lblTotalAmount.Left = lblTotalAmount.Parent.Width - lblTotalAmount.Width - 15;
            lblFinalAmount.Left = lblFinalAmount.Parent.Width - lblFinalAmount.Width - 15;
        }

        private async void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Giỏ hàng trống!", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Xác nhận thanh toán hóa đơn này bằng {_selectedPaymentMethod}?", "Thanh toán", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    int storeId = CurrentSession.StoreId ?? 1;
                    int? customerId = cboCustomer.SelectedValue != null && (int)cboCustomer.SelectedValue > 0 ? (int)cboCustomer.SelectedValue : null;
                    
                    var details = new List<SalesOrderDetail>();
                    foreach (var item in _cart.Values)
                    {
                        details.Add(new SalesOrderDetail
                        {
                            BookId = item.BookId,
                            Quantity = item.Quantity,
                            UnitPrice = item.Price,
                            LineTotal = item.Price * item.Quantity
                        });
                    }

                    string dbPaymentMethod = _selectedPaymentMethod == "Tiền mặt" ? BookStoreManagement.Models.AppConstants.PaymentMethods.Cash : BookStoreManagement.Models.AppConstants.PaymentMethods.Banking;
                    int orderId = await _orderService.CreateOrderAsync(storeId, customerId, dbPaymentMethod, "Bán tại quầy", details);

                    MessageBox.Show("Thanh toán thành công!", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _cart.Clear();
                    RenderCart();
                    
                    // Reload inventory from DB to ensure it matches DB Trigger deductions
                    _allBooks = await _inventoryService.GetByStoreIdAsync(storeId);
                    FilterBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi thanh toán: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    public class CartItem
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int MaxStock { get; set; }
    }
}
