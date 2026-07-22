using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        private Guna2Panel pnlLeft;
        private Guna2Panel pnlRight;
        
        // Left
        private Guna2TextBox txtSearch;
        private FlowLayoutPanel flpCategories;
        private TableLayoutPanel tlpProducts;
        private FlowLayoutPanel flpPagination;
        private Label lblPageInfo;

        // Right
        private Guna2TextBox txtCustomer;
        private FlowLayoutPanel flpCart;
        private Label lblTotalAmount;
        private Label lblDiscount;
        private Label lblFinalAmount;
        private Guna2Button btnCheckout;

        // Data
        private List<StoreBookInventoryViewModel> _allBooks = new List<StoreBookInventoryViewModel>();
        private List<StoreBookInventoryViewModel> _filteredBooks = new List<StoreBookInventoryViewModel>();
        private List<Category> _categories = new List<Category>();
        
        // Pagination
        private int _currentPage = 1;
        private const int _pageSize = 9;
        private string _currentCategory = "Tất cả";
        
        // Cart
        private Dictionary<int, CartItem> _cart = new Dictionary<int, CartItem>();

        public POSControl()
        {
            _inventoryService = new StoreBookInventoryService();
            _categoryService = new CategoryService();
            _orderService = new SalesOrderService();

            InitializeComponent();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += POSControl_Load;
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);
            this.BackColor = ThemeManager.Background;

            var tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            pnlLeft = new Guna2Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0), BorderRadius = 12, FillColor = ThemeManager.CardBackground, BorderColor = ThemeManager.TextBoxBorder, BorderThickness = 1 };
            pnlRight = new Guna2Panel { Dock = DockStyle.Fill, Margin = new Padding(10, 0, 0, 0), BorderRadius = 12, FillColor = Color.Transparent };

            tlpMain.Controls.Add(pnlLeft, 0, 0);
            tlpMain.Controls.Add(pnlRight, 1, 0);
            this.Controls.Add(tlpMain);

            BuildLeftPanel();
            BuildRightPanel();
        }

        private void BuildLeftPanel()
        {
            // Top search bar
            var pnlTop = new Guna2Panel { Dock = DockStyle.Top, Height = 70, CustomBorderThickness = new Padding(0, 0, 0, 1), CustomBorderColor = ThemeManager.TextBoxBorder, FillColor = Color.Transparent };
            txtSearch = new Guna2TextBox
            {
                Location = new Point(15, 15),
                Size = new Size(350, 40),
                BorderRadius = 8,
                PlaceholderText = "Tìm kiếm theo mã vạch, tên sách...",
                IconLeftOffset = new Point(5, 0)
            };
            txtSearch.TextChanged += (s, e) => { _currentPage = 1; FilterBooks(); };
            
            var btnScan = CreateOutlineButton("Quét mã", 380, 15);
            var btnFilter = CreateOutlineButton("Lọc", 490, 15);
            
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Controls.Add(btnScan);
            pnlTop.Controls.Add(btnFilter);
            pnlLeft.Controls.Add(pnlTop);

            // Categories
            var pnlCat = new Guna2Panel { Dock = DockStyle.Top, Height = 55, FillColor = Color.Transparent, Padding = new Padding(15, 10, 15, 5) };
            flpCategories = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = false, WrapContents = false };
            pnlCat.Controls.Add(flpCategories);
            pnlLeft.Controls.Add(pnlCat);

            // Pagination Bottom
            var pnlBot = new Guna2Panel { Dock = DockStyle.Bottom, Height = 60, CustomBorderThickness = new Padding(0, 1, 0, 0), CustomBorderColor = ThemeManager.TextBoxBorder, FillColor = Color.Transparent };
            lblPageInfo = new Label { Location = new Point(15, 20), AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = ThemeManager.TextSecondary };
            flpPagination = new FlowLayoutPanel { Location = new Point(300, 15), Size = new Size(300, 35), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            pnlBot.Controls.Add(lblPageInfo);
            pnlBot.Controls.Add(flpPagination);
            pnlLeft.Controls.Add(pnlBot);

            // 3x3 Grid
            tlpProducts = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                Padding = new Padding(10)
            };
            for (int i = 0; i < 3; i++)
            {
                tlpProducts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
                tlpProducts.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            }
            pnlLeft.Controls.Add(tlpProducts);

            // Bring to front properly
            pnlTop.BringToFront();
            pnlCat.BringToFront();
            pnlBot.BringToFront();
            tlpProducts.BringToFront();
        }

        private void BuildRightPanel()
        {
            // Customer Card
            var pnlCustomer = new Guna2Panel { Dock = DockStyle.Top, Height = 120, BorderRadius = 12, FillColor = ThemeManager.CardBackground, BorderColor = ThemeManager.TextBoxBorder, BorderThickness = 1, Margin = new Padding(0, 0, 0, 15) };
            var lblCustTitle = new Label { Text = "👤 Khách hàng", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
            var btnAddCust = new Label { Text = "Thêm mới", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.ButtonFill, Location = new Point(pnlCustomer.Width - 80, 18), AutoSize = true, Cursor = Cursors.Hand };
            
            txtCustomer = new Guna2TextBox
            {
                Location = new Point(15, 50),
                Size = new Size(pnlCustomer.Width - 30, 40),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 8,
                PlaceholderText = "Tìm theo SĐT hoặc tên..."
            };
            pnlCustomer.Controls.AddRange(new Control[] { lblCustTitle, btnAddCust, txtCustomer });
            pnlRight.Controls.Add(pnlCustomer);

            // Summary Card (Bottom)
            var pnlSummary = new Guna2Panel { Dock = DockStyle.Bottom, Height = 220, BorderRadius = 12, FillColor = ThemeManager.CardBackground, BorderColor = ThemeManager.TextBoxBorder, BorderThickness = 1, Margin = new Padding(0, 15, 0, 0) };
            
            var lblTotalText = new Label { Text = "Tổng tiền hàng", Font = new Font("Segoe UI", 10F), ForeColor = ThemeManager.TextSecondary, Location = new Point(15, 15), AutoSize = true };
            lblTotalAmount = new Label { Text = "0 ₫", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(pnlSummary.Width - 100, 15), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, TextAlign = ContentAlignment.MiddleRight };
            
            var lblDiscountText = new Label { Text = "Giảm giá", Font = new Font("Segoe UI", 10F), ForeColor = ThemeManager.TextSecondary, Location = new Point(15, 45), AutoSize = true };
            lblDiscount = new Label { Text = "- 0 ₫", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.Red, Location = new Point(pnlSummary.Width - 100, 45), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, TextAlign = ContentAlignment.MiddleRight };

            var line = new Guna2Panel { Location = new Point(15, 75), Size = new Size(pnlSummary.Width - 30, 1), FillColor = ThemeManager.TextBoxBorder, Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right };

            var lblFinalText = new Label { Text = "Khách cần trả", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(15, 90), AutoSize = true };
            lblFinalAmount = new Label { Text = "0 ₫", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = ThemeManager.ButtonFill, Location = new Point(pnlSummary.Width - 150, 85), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, TextAlign = ContentAlignment.MiddleRight };

            var btnCash = CreateOutlineButton("Tiền mặt", 15, 130, 120);
            btnCash.FillColor = Color.FromArgb(20, ThemeManager.ButtonFill);
            btnCash.ForeColor = ThemeManager.ButtonFill;
            var btnTransfer = CreateOutlineButton("Chuyển khoản", 145, 130, 120);

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

            pnlSummary.Controls.AddRange(new Control[] { lblTotalText, lblTotalAmount, lblDiscountText, lblDiscount, line, lblFinalText, lblFinalAmount, btnCash, btnTransfer, btnSave, btnCheckout });
            pnlRight.Controls.Add(pnlSummary);

            // Cart Items Container (Middle)
            var pnlCart = new Guna2Panel { Dock = DockStyle.Fill, BorderRadius = 12, FillColor = ThemeManager.CardBackground, BorderColor = ThemeManager.TextBoxBorder, BorderThickness = 1 };
            var pnlCartTop = new Guna2Panel { Dock = DockStyle.Top, Height = 40, CustomBorderThickness = new Padding(0, 0, 0, 1), CustomBorderColor = ThemeManager.TextBoxBorder };
            var lblCartTitle = new Label { Text = "🛒 Giỏ hàng (0)", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            var btnClearCart = new Label { Text = "Xóa hết", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.Red, Location = new Point(pnlCart.Width - 70, 12), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Cursor = Cursors.Hand };
            btnClearCart.Click += (s, e) => { _cart.Clear(); RenderCart(); };
            
            pnlCartTop.Controls.Add(lblCartTitle);
            pnlCartTop.Controls.Add(btnClearCart);
            pnlCart.Controls.Add(pnlCartTop);

            flpCart = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10)
            };
            pnlCart.Controls.Add(flpCart);
            flpCart.BringToFront();

            // Spacer between Customer and Cart
            var pnlSpacer = new Panel { Dock = DockStyle.Top, Height = 15, BackColor = Color.Transparent };
            pnlRight.Controls.Add(pnlSpacer);
            
            pnlRight.Controls.Add(pnlCart);
            
            // Re-order controls in Right Panel
            pnlCustomer.BringToFront();
            pnlSpacer.BringToFront();
            pnlCart.BringToFront();
            pnlSummary.BringToFront();
            
            // Resize handling
            pnlSummary.Resize += (s, e) => {
                txtCustomer.Width = pnlCustomer.Width - 30;
                btnAddCust.Left = pnlCustomer.Width - 80;
                lblTotalAmount.Left = pnlSummary.Width - lblTotalAmount.Width - 15;
                lblDiscount.Left = pnlSummary.Width - lblDiscount.Width - 15;
                lblFinalAmount.Left = pnlSummary.Width - lblFinalAmount.Width - 15;
                btnClearCart.Left = pnlCart.Width - 70;
                btnCheckout.Width = pnlSummary.Width - 140;
            };
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
                FillColor = Color.White,
                ForeColor = ThemeManager.TextPrimary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            this.BackColor = ThemeManager.Background;
            pnlLeft.FillColor = ThemeManager.CardBackground;
            pnlLeft.BorderColor = ThemeManager.TextBoxBorder;
            // Update other colors...
        }

        private async void POSControl_Load(object sender, EventArgs e)
        {
            _categories = await _categoryService.GetActiveAsync();
            RenderCategories();
            
            int currentStoreId = CurrentSession.StoreId ?? 1; // fallback
            
            _allBooks = await _inventoryService.GetByStoreIdAsync(currentStoreId);
            FilterBooks();
        }

        private void RenderCategories()
        {
            flpCategories.Controls.Clear();
            var allBtn = CreateCategoryButton("Tất cả", true);
            flpCategories.Controls.Add(allBtn);

            foreach (var cat in _categories)
            {
                var btn = CreateCategoryButton(cat.CategoryName, false);
                flpCategories.Controls.Add(btn);
            }
        }

        private Guna2Button CreateCategoryButton(string text, bool isActive)
        {
            var btn = new Guna2Button
            {
                Text = text,
                AutoSize = true,
                Height = 35,
                BorderRadius = 17,
                Margin = new Padding(0, 0, 8, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            if (isActive)
            {
                btn.FillColor = ThemeManager.ButtonFill;
                btn.ForeColor = Color.White;
            }
            else
            {
                btn.FillColor = ThemeManager.CardBackground;
                btn.BorderThickness = 1;
                btn.BorderColor = ThemeManager.TextBoxBorder;
                btn.ForeColor = ThemeManager.TextSecondary;
            }

            btn.Click += (s, e) =>
            {
                _currentCategory = text;
                foreach (Guna2Button c in flpCategories.Controls)
                {
                    c.FillColor = ThemeManager.CardBackground;
                    c.ForeColor = ThemeManager.TextSecondary;
                    c.BorderThickness = 1;
                }
                btn.FillColor = ThemeManager.ButtonFill;
                btn.ForeColor = Color.White;
                btn.BorderThickness = 0;
                
                _currentPage = 1;
                FilterBooks();
            };
            return btn;
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
            RenderProducts();
        }

        private void RenderProducts()
        {
            tlpProducts.Controls.Clear();
            
            int totalItems = _filteredBooks.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);
            if (totalPages == 0) totalPages = 1;
            if (_currentPage > totalPages) _currentPage = totalPages;

            var itemsToDisplay = _filteredBooks.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

            int col = 0;
            int row = 0;
            
            foreach (var item in itemsToDisplay)
            {
                var card = CreateProductCard(item);
                tlpProducts.Controls.Add(card, col, row);
                
                col++;
                if (col > 2)
                {
                    col = 0;
                    row++;
                }
            }

            // Update Pagination
            lblPageInfo.Text = $"Hiển thị {(_currentPage - 1) * _pageSize + 1}-{Math.Min(_currentPage * _pageSize, totalItems)} trong {totalItems} sản phẩm";
            RenderPagination(totalPages);
        }

        private Guna2Panel CreateProductCard(StoreBookInventoryViewModel item)
        {
            var pnl = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                BorderRadius = 8,
                BorderThickness = 1,
                BorderColor = ThemeManager.TextBoxBorder,
                FillColor = ThemeManager.CardBackground,
                Cursor = Cursors.Hand
            };

            var pic = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = ThemeManager.HoverColor,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            
            var lblTitle = new Label
            {
                Text = item.Title,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(10, 130),
                Width = 200,
                AutoEllipsis = true,
                BackColor = Color.Transparent
            };

            var lblPrice = new Label
            {
                Text = $"{item.SellingPrice:N0} ₫",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeManager.ButtonFill,
                Location = new Point(10, 160),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblStock = new Label
            {
                Text = $"Tồn: {item.Quantity}",
                Font = new Font("Segoe UI", 8F),
                Location = new Point(pnl.Width - 60, 165),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = item.Quantity > 0 ? ThemeManager.TextSecondary : Color.Red
            };
            
            pnl.Controls.AddRange(new Control[] { pic, lblTitle, lblPrice, lblStock });
            
            pnl.Click += (s, e) => AddToCart(item);
            pic.Click += (s, e) => AddToCart(item);
            lblTitle.Click += (s, e) => AddToCart(item);
            
            return pnl;
        }

        private void RenderPagination(int totalPages)
        {
            flpPagination.Controls.Clear();
            
            var btnPrev = new Guna2Button { Text = "<", Size = new Size(35, 35), BorderRadius = 4, FillColor = ThemeManager.CardBackground, BorderThickness = 1, BorderColor = ThemeManager.TextBoxBorder, ForeColor = ThemeManager.TextPrimary };
            btnPrev.Enabled = _currentPage > 1;
            btnPrev.Click += (s, e) => { _currentPage--; RenderProducts(); };
            flpPagination.Controls.Add(btnPrev);

            // Simple pagination (1 2 3)
            int start = Math.Max(1, _currentPage - 1);
            int end = Math.Min(totalPages, start + 2);
            if (end - start < 2 && start > 1) start = end - 2;

            for (int i = start; i <= end; i++)
            {
                var page = i;
                var btn = new Guna2Button { Text = page.ToString(), Size = new Size(35, 35), BorderRadius = 4 };
                if (page == _currentPage)
                {
                    btn.FillColor = ThemeManager.ButtonFill;
                    btn.ForeColor = Color.White;
                }
                else
                {
                    btn.FillColor = ThemeManager.CardBackground;
                    btn.BorderThickness = 1;
                    btn.BorderColor = ThemeManager.TextBoxBorder;
                    btn.ForeColor = ThemeManager.TextPrimary;
                }
                btn.Click += (s, e) => { _currentPage = page; RenderProducts(); };
                flpPagination.Controls.Add(btn);
            }

            var btnNext = new Guna2Button { Text = ">", Size = new Size(35, 35), BorderRadius = 4, FillColor = ThemeManager.CardBackground, BorderThickness = 1, BorderColor = ThemeManager.TextBoxBorder, ForeColor = ThemeManager.TextPrimary };
            btnNext.Enabled = _currentPage < totalPages;
            btnNext.Click += (s, e) => { _currentPage++; RenderProducts(); };
            flpPagination.Controls.Add(btnNext);
        }

        private void AddToCart(StoreBookInventoryViewModel item)
        {
            if (item.Quantity <= 0)
            {
                MessageBox.Show("Sách này đã hết hàng trong kho!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_cart.ContainsKey(item.BookId))
            {
                if (_cart[item.BookId].Quantity < item.Quantity)
                {
                    _cart[item.BookId].Quantity++;
                }
                else
                {
                    MessageBox.Show("Không đủ số lượng trong kho!", "Cảnh báo");
                }
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
            RenderCart();
        }

        private void RenderCart()
        {
            flpCart.Controls.Clear();
            flpCart.SuspendLayout();
            
            decimal total = 0;
            
            foreach (var item in _cart.Values)
            {
                total += item.Price * item.Quantity;
                
                var pnl = new Guna2Panel
                {
                    Width = flpCart.Width - 25,
                    Height = 80,
                    BorderRadius = 8,
                    BorderThickness = 1,
                    BorderColor = ThemeManager.TextBoxBorder,
                    FillColor = ThemeManager.CardBackground,
                    Margin = new Padding(0, 0, 0, 10)
                };

                var lblTitle = new Label { Text = item.Title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(60, 10), Width = pnl.Width - 100, AutoEllipsis = true };
                var lblCode = new Label { Text = item.Code, Font = new Font("Segoe UI", 8F), ForeColor = ThemeManager.TextSecondary, Location = new Point(60, 30), AutoSize = true };
                
                var lblPrice = new Label { Text = $"{item.Price * item.Quantity:N0} ₫", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = ThemeManager.ButtonFill, Location = new Point(pnl.Width - 120, 50), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, TextAlign = ContentAlignment.MiddleRight };
                
                // Quantity controls
                var pnlQty = new Guna2Panel { Location = new Point(60, 50), Size = new Size(90, 25), BorderRadius = 4, BorderThickness = 1, BorderColor = ThemeManager.TextBoxBorder };
                
                var btnMinus = new Button { Text = "-", Size = new Size(25, 25), Dock = DockStyle.Left, FlatStyle = FlatStyle.Flat, BackColor = Color.White };
                btnMinus.FlatAppearance.BorderSize = 0;
                btnMinus.Click += (s, e) => { 
                    if (item.Quantity > 1) { item.Quantity--; RenderCart(); } 
                    else { _cart.Remove(item.BookId); RenderCart(); }
                };

                var lblQty = new Label { Text = item.Quantity.ToString(), Size = new Size(40, 25), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                
                var btnPlus = new Button { Text = "+", Size = new Size(25, 25), Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, BackColor = Color.White };
                btnPlus.FlatAppearance.BorderSize = 0;
                btnPlus.Click += (s, e) => { 
                    if (item.Quantity < item.MaxStock) { item.Quantity++; RenderCart(); } 
                };

                pnlQty.Controls.AddRange(new Control[] { lblQty, btnMinus, btnPlus });
                
                // Delete button
                var btnDel = new Label { Text = "❌", Location = new Point(pnl.Width - 30, 10), Cursor = Cursors.Hand };
                btnDel.Click += (s, e) => { _cart.Remove(item.BookId); RenderCart(); };

                pnl.Controls.AddRange(new Control[] { lblTitle, lblCode, pnlQty, lblPrice, btnDel });
                
                // Align right manually
                pnl.Resize += (s, e) => {
                    lblPrice.Left = pnl.Width - lblPrice.Width - 10;
                    btnDel.Left = pnl.Width - 25;
                };

                flpCart.Controls.Add(pnl);
            }

            flpCart.ResumeLayout();

            lblTotalAmount.Text = $"{total:N0} ₫";
            lblFinalAmount.Text = $"{total:N0} ₫";
            
            // Adjust label positions
            lblTotalAmount.Left = lblTotalAmount.Parent.Width - lblTotalAmount.Width - 15;
            lblFinalAmount.Left = lblFinalAmount.Parent.Width - lblFinalAmount.Width - 15;
        }

        private async void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Xác nhận thanh toán hóa đơn này?", "Thanh toán", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    int storeId = CurrentSession.StoreId ?? 1;
                    int? customerId = 1; // Default customer
                    
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

                    int orderId = await _orderService.CreateOrderAsync(storeId, customerId, "Tiền mặt", "Bán tại quầy", details);

                    MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _cart.Clear();
                    RenderCart();
                    
                    // Reload inventory
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
