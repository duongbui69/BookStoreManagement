using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Interfaces;
using BookStoreManagement.Helpers;

namespace BookStoreManagement.Forms
{
    public partial class MainForm : Form
    {
        private User _currentUser;
        private Label _notifBadge;
        private NotificationPanel _notifPanel;


        public MainForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            lblUsername.Text = _currentUser.FullName;
            if (!string.IsNullOrEmpty(_currentUser.FullName)) btnAvatar.Text = _currentUser.FullName.Substring(0, 1).ToUpper();
            
            InitializeTopBar();
            InitializeSidebarFooter();

            
            // Initialize Dynamic Accordion Sidebar
            InitializeAccordionSidebar();

            btnLogout.Click += btnLogout_Click;
            btnThemeToggle.Click += BtnThemeToggle_Click;


            BookStoreManagement.Themes.ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            BookStoreManagement.Events.GlobalEvents.RequestMyInvoicesView += GlobalEvents_RequestMyInvoicesView;
            ApplyTheme();

            this.Shown += MainForm_Shown;
            this.FormClosed += (s, e) => { BookStoreManagement.Events.GlobalEvents.RequestMyInvoicesView -= GlobalEvents_RequestMyInvoicesView; };
        }

        private void GlobalEvents_RequestMyInvoicesView()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(GlobalEvents_RequestMyInvoicesView));
                return;
            }
            BtnMyInvoices_Click(this, EventArgs.Empty);
            foreach (var btn in allMenuButtons)
            {
                if (btn.Text.Contains("Hóa đơn của tôi") || btn.Text.Contains("Search my invoices"))
                {
                    SetActiveTab(btn);
                    break;
                }
            }
        }

        private void MainForm_Shown(object? sender, EventArgs e)
        {
            if (CurrentSession.IsStaff)
            {
                BtnPOS_Click(this, EventArgs.Empty);
            }
            else
            {
                LoadDashboard();
            }
        }

                private Dictionary<Type, Control> _controlCache = new Dictionary<Type, Control>();

        private async void LoadControl<T>(string placeholder) where T : Control, new()
        {
            Type type = typeof(T);
            
            if (CurrentSession.IsStaff && type != typeof(UserControls.StaffMyShiftsControl))
            {
                var shiftService = new BookStoreManagement.Services.ShiftService();
                if (shiftService.GetActiveShift() == null)
                {
                    MessageBox.Show("Vui lòng bắt đầu ca làm việc trước khi thực hiện các thao tác khác!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    type = typeof(UserControls.StaffMyShiftsControl);
                    placeholder = "Ca của tôi";
                }
            }

            if (!_controlCache.ContainsKey(type))
            {
                Control newControl = (Control)Activator.CreateInstance(type);
                newControl.Dock = DockStyle.Fill;
                _controlCache[type] = newControl;
            }

            Control controlToLoad = _controlCache[type];

            // Don't dispose existing controls, just remove them from visual tree
            panelMain.Controls.Clear();
            panelMain.Controls.Add(controlToLoad);
            
            if (controlToLoad is BookStoreManagement.Interfaces.IRefreshable refreshable)
            {
                await refreshable.RefreshDataAsync();
            }
        }

        private void BtnInventory_Click(object sender, EventArgs e)
        {
            if (CurrentSession.IsAdmin)
            {
                LoadControl<UserControls.InventoryControl>("Search by ISBN, title or SKU...");
            }
            else
            {
                LoadControl<UserControls.StaffInventoryControl>("Tra cứu kho");
            }
        }

        private void BtnInventoryLedger_Click(object sender, EventArgs e)
        {
            LoadControl<UserControls.InventoryLedgerControl>("Sổ kho — nhật ký nhập xuất...");
        }

        private void BtnPurchaseReceipts_Click(object sender, EventArgs e)
        {
            LoadControl<UserControls.PurchaseReceiptControl>("Tìm kiếm phiếu nhập...");
        }

        private void BtnExportReceipts_Click(object sender, EventArgs e)
        {
            LoadControl<UserControls.ExportReceiptControl>("Tìm kiếm phiếu xuất...");
        }


        private void BtnCatalog_Click(object sender, EventArgs e)
        {

            LoadControl<UserControls.CatalogControl>("Search by book title, ISBN or author...");
        }

        private void BtnOrders_Click(object sender, EventArgs e)
        {

            LoadControl<UserControls.OrdersControl>("Search orders, customers...");
        }

        private void BtnInvoices_Click(object sender, EventArgs e)
        {

            LoadControl<UserControls.OrdersControl>("Search invoices...");
        }

        private void BtnRefunds_Click(object sender, EventArgs e)
        {

            if (CurrentSession.IsAdmin)
            {
                LoadControl<UserControls.RefundControl>("Search return receipts...");
            }
            else
            {
                LoadControl<UserControls.StaffReturnControl>("Xử lý Trả hàng");
            }
        }

        private void BtnPOS_Click(object sender, EventArgs e)
        {
            // SetActiveTab or handling
            LoadControl<UserControls.POSControl>("Tìm kiếm theo mã vạch, tên sách, tác giả...");
        }

        private void BtnMyInvoices_Click(object sender, EventArgs e)
        {
            if (CurrentSession.IsAdmin)
            {
                LoadControl<UserControls.OrdersControl>("Search my invoices...");
            }
            else
            {
                LoadControl<UserControls.StaffMyInvoicesControl>("Lịch sử hóa đơn");
            }
        }

        private void BtnMyShifts_Click(object sender, EventArgs e)
        {
            LoadControl<UserControls.StaffMyShiftsControl>("Ca của tôi");
        }

        private void BtnCustomer_Click(object sender, EventArgs e)
        {
            LoadControl<UserControls.CustomerControl>("Search customers...");
        }

        private void BtnHR_Click(object sender, EventArgs e)
        {

            LoadControl<UserControls.HRControl>("Search employees, roles, or departments...");
        }

        private void BtnAccount_Click(object sender, EventArgs e)
        {

            LoadControl<UserControls.AccountControl>("Search accounts by username, name, email...");
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {

            LoadControl<UserControls.ReportsControl>("Search reports...");
        }

        private void BtnStores_Click(object sender, EventArgs e)
        {

            LoadControl<UserControls.StoresControl>("Search stores...");
        }

        private void BtnDashboard_Click(object sender, EventArgs e)
        {

            LoadDashboard();
        }

        private void LoadDashboard()
        {
            LoadControl<UserControls.DashboardControl>("Search dashboard...");
        }

        private void BtnCategory_Click(object sender, EventArgs e)
        {
            LoadControl<UserControls.CategoryControl>("Tìm kiếm danh mục...");
        }

        private void BtnAuthor_Click(object sender, EventArgs e)
        {
            LoadControl<UserControls.AuthorControl>("Tìm theo tên, mã...");
        }

        private void BtnPublisher_Click(object sender, EventArgs e)
        {
            LoadControl<UserControls.PublisherControl>("Tìm kiếm nhà xuất bản...");
        }

        private void BtnSupplier_Click(object sender, EventArgs e)
        {
            LoadControl<UserControls.SupplierControl>("Tìm theo tên, mã, email, sđt...");
        }



        private void SetActiveTab(Guna.UI2.WinForms.Guna2Button activeButton)
        {
            foreach (var btn in allMenuButtons)
            {
                if (btn == activeButton)
                {
                    btn.CustomBorderThickness = new Padding(3, 0, 0, 0);
                    btn.CustomBorderColor = Themes.ThemeManager.ButtonFill;
                    btn.FillColor = Themes.ThemeManager.ButtonFill;
                    btn.ForeColor = Themes.ThemeManager.ButtonText;
                    btn.Font = new Font(btn.Font, FontStyle.Bold);
                }
                else
                {
                    btn.CustomBorderThickness = new Padding(0);
                    btn.FillColor = Color.Transparent;
                    btn.ForeColor = Themes.ThemeManager.TextSecondary;
                    btn.Font = new Font(btn.Font, FontStyle.Regular);
                }
            }
        }

        private void BtnThemeToggle_Click(object sender, EventArgs e)
        {
            BookStoreManagement.Themes.ThemeManager.ToggleTheme();
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        
        private FlowLayoutPanel navPanel;
        private List<Guna.UI2.WinForms.Guna2Button> allMenuButtons = new List<Guna.UI2.WinForms.Guna2Button>();

        private void InitializeAccordionSidebar()
        {
            panelSidebar.Controls.Remove(btnDashboard);
            panelSidebar.Controls.Remove(btnCatalog);
            panelSidebar.Controls.Remove(btnInventory);
            panelSidebar.Controls.Remove(btnOrders);
            panelSidebar.Controls.Remove(btnHR);
            panelSidebar.Controls.Remove(btnReports);

            if (navPanel != null)
            {
                panelSidebar.Controls.Remove(navPanel);
                navPanel.Dispose();
            }
            allMenuButtons.Clear();

            lblBrand.Visible = false;
            lblSubBrand.Visible = false;

            navPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 5, 0, 0)
            };
            panelSidebar.Controls.Add(navPanel);
            navPanel.BringToFront(); // Dock between pnlBrand and panelUserProfile

            if (_currentUser.RoleId == 1)
            {
                // Admin Menu
                AddMenu("Tổng quan", "dashboard", BtnDashboard_Click);

                var catalogSub = new Dictionary<string, EventHandler>
                {
                    { "Sách", BtnCatalog_Click },
                    { "Danh mục Sách", BtnCategory_Click }
                };
                AddAccordionMenu("Quản lý Sách", "menu_book", catalogSub);

                var inventorySub = new Dictionary<string, EventHandler>
                {
                    { "Sổ kho", BtnInventoryLedger_Click },
                    { "Nhập kho", BtnPurchaseReceipts_Click },
                    { "Xuất kho", BtnExportReceipts_Click },
                    { "Thống kê kho", BtnInventory_Click }
                };
                AddAccordionMenu("Quản lý Kho", "inventory_2", inventorySub);

                var hrSub = new Dictionary<string, EventHandler>
                {
                    { "Nhân viên", BtnHR_Click },
                    { "Tài khoản", BtnAccount_Click },
                    { "Chi nhánh", BtnStores_Click }
                };
                AddAccordionMenu("Quản lý Nhân sự", "group", hrSub);

                var customerSub = new Dictionary<string, EventHandler>
                {
                    { "Khách hàng", BtnCustomer_Click },
                    { "Giao dịch", BtnOrders_Click },
                    { "Đổi/Trả", BtnRefunds_Click }
                };
                AddAccordionMenu("Quản lý Khách hàng", "groups", customerSub);

                var masterSub = new Dictionary<string, EventHandler>
                {
                    { "Tác giả", BtnAuthor_Click },
                    { "NXB", BtnPublisher_Click },
                    { "Nhà cung cấp", BtnSupplier_Click }
                };
                AddAccordionMenu("Quản lý Danh mục", "category", masterSub);
                
                AddMenu("Báo cáo Thống kê", "assessment", BtnReports_Click);
            }
            else
            {
                // Staff Menu
                AddMenu("Bán hàng (POS)", "point_of_sale", BtnPOS_Click);
                AddMenu("Trả hàng", "assignment_return", BtnRefunds_Click);
                AddMenu("Tra cứu kho", "inventory_2", BtnInventory_Click);
                AddMenu("Hóa đơn của tôi", "receipt_long", BtnMyInvoices_Click);
                AddMenu("Ca của tôi", "schedule", BtnMyShifts_Click);
            }
        }

        private Guna.UI2.WinForms.Guna2Button CreateMenuButton(string text, string iconText, bool isSubMenu = false)
        {
            var btn = new Guna.UI2.WinForms.Guna2Button
            {
                Text = $"{(iconText != "" ? iconText + "   " : "")}{text}",
                Size = new Size(240, 45), // Width reduced slightly to fit scrollbar nicely
                FillColor = Color.Transparent,
                Font = isSubMenu ? new Font("Segoe UI", 9.5F) : new Font("Segoe UI", 11F),
                ForeColor = Themes.ThemeManager.TextSecondary,
                TextAlign = HorizontalAlignment.Left,
                TextOffset = isSubMenu ? new Point(45, 0) : new Point(15, 0),
                CustomBorderThickness = new Padding(0),
                Margin = new Padding(0)
            };
            btn.HoverState.FillColor = Themes.ThemeManager.HoverColor;
            allMenuButtons.Add(btn);
            return btn;
        }

        private void AddMenu(string title, string icon, EventHandler onClick)
        {
            var btn = CreateMenuButton(title, GetEmojiForMaterialIcon(icon));
            btn.Click += (s, e) => SetActiveTab(btn);
            if (onClick != null)
                btn.Click += onClick;
            navPanel.Controls.Add(btn);
        }

        private void AddAccordionMenu(string title, string icon, Dictionary<string, EventHandler> subItems)
        {
            var btnMain = CreateMenuButton($"{title} ▾", GetEmojiForMaterialIcon(icon));
            
            var pnlSub = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                AutoSize = false,
                Height = 0,
                Margin = new Padding(0),
                Visible = false
            };

            foreach (var item in subItems)
            {
                var btnSub = CreateMenuButton(item.Key, "", true);
                btnSub.Click += (s, e) => SetActiveTab(btnSub);
                if (item.Value != null)
                    btnSub.Click += item.Value;
                pnlSub.Controls.Add(btnSub);
            }

            int targetHeight = subItems.Count * 45;
            System.Windows.Forms.Timer animTimer = new System.Windows.Forms.Timer { Interval = 15 };
            bool isExpanding = false;

            animTimer.Tick += (s, ev) =>
            {
                if (isExpanding)
                {
                    pnlSub.Height += 25;
                    if (pnlSub.Height >= targetHeight)
                    {
                        pnlSub.Height = targetHeight;
                        animTimer.Stop();
                    }
                }
                else
                {
                    pnlSub.Height -= 25;
                    if (pnlSub.Height <= 0)
                    {
                        pnlSub.Height = 0;
                        pnlSub.Visible = false;
                        animTimer.Stop();
                    }
                }
            };

            pnlSub.Tag = new Action<bool>((expand) => {
                isExpanding = expand;
                if (expand) pnlSub.Visible = true;
                animTimer.Start();
            });

            btnMain.Click += (s, e) => {
                bool wasVisible = pnlSub.Visible && pnlSub.Height > 0;
                
                // Close all other open sub-panels
                foreach (Control c in navPanel.Controls)
                {
                    if (c is FlowLayoutPanel subPanel && c != pnlSub && subPanel.Visible)
                    {
                        if (subPanel.Tag is Action<bool> toggleAction)
                        {
                            toggleAction(false);
                        }
                        
                        int index = navPanel.Controls.GetChildIndex(subPanel);
                        if (index > 0 && navPanel.Controls[index - 1] is Guna.UI2.WinForms.Guna2Button mainBtn)
                        {
                            mainBtn.Text = mainBtn.Text.Replace("▴", "▾");
                        }
                    }
                }

                if (!wasVisible)
                {
                    if (pnlSub.Tag is Action<bool> toggleAction) toggleAction(true);
                    btnMain.Text = btnMain.Text.Replace("▾", "▴");
                }
                else
                {
                    if (pnlSub.Tag is Action<bool> toggleAction) toggleAction(false);
                    btnMain.Text = btnMain.Text.Replace("▴", "▾");
                }
            };

            navPanel.Controls.Add(btnMain);
            navPanel.Controls.Add(pnlSub);
        }

        private string GetEmojiForMaterialIcon(string icon)
        {
            switch (icon)
            {
                case "dashboard": return "📊";
                case "menu_book": return "📖";
                case "inventory_2": return "📦";
                case "group": return "👥";
                case "groups": return "🤝";
                case "category": return "📑";
                case "assessment": return "📈";
                default: return "🔹";
            }
        }

        private void ApplyTheme()
        {
            BookStoreManagement.Themes.ThemeManager.ApplyTypography(this);
            this.BackColor = Themes.ThemeManager.Background;
            panelMain.BackColor = Themes.ThemeManager.Background;
            panelSidebar.BackColor = Themes.ThemeManager.Sidebar;
            panelTop.BackColor = Themes.ThemeManager.Sidebar;
            panelUserProfile.BackColor = Themes.ThemeManager.Sidebar;

            lblBrand.ForeColor = Themes.ThemeManager.TextPrimary;
            lblSubBrand.ForeColor = Themes.ThemeManager.TextSecondary;
            
            lblUsername.ForeColor = Themes.ThemeManager.TextPrimary;
            lblRole.ForeColor = Themes.ThemeManager.TextSecondary;

            btnThemeToggle.Text = Themes.ThemeManager.IsDarkMode ? "☀️" : "🌙";
            btnThemeToggle.FillColor = Themes.ThemeManager.HoverColor;
            btnThemeToggle.ForeColor = Themes.ThemeManager.TextPrimary;

            // Re-apply active tab styling
            if (allMenuButtons != null)
            {
                foreach (var btn in allMenuButtons)
                {
                    btn.HoverState.FillColor = Themes.ThemeManager.HoverColor;
                    if (btn.CustomBorderThickness.Left > 0)
                    {
                        btn.CustomBorderColor = Themes.ThemeManager.ButtonFill;
                        btn.FillColor = Themes.ThemeManager.HoverColor;
                        btn.ForeColor = Themes.ThemeManager.ButtonFill;
                    }
                    else
                    {
                        btn.FillColor = Color.Transparent;
                        btn.ForeColor = Themes.ThemeManager.TextSecondary;
                    }
                }
            }
            
            // Re-apply footer styling
            btnLogout.ForeColor = Themes.ThemeManager.TextSecondary;
            btnLogout.HoverState.FillColor = Themes.ThemeManager.HoverColor;
            if (panelUserProfile.Controls.ContainsKey("FooterSep"))
            {
                panelUserProfile.Controls["FooterSep"].BackColor = Themes.ThemeManager.TextBoxBorder;
            }

            // Dynamic topbar theming
            panelTop.CustomBorderColor = Themes.ThemeManager.TextBoxBorder;
            foreach (Control c in panelTop.Controls)
            {
                ApplyThemeToDynamicControls(c);
            }
        }
        
        private void ApplyThemeToDynamicControls(Control container)
        {
            if (container.Tag?.ToString() == "ThemePanel")
            {
                if (container is Guna.UI2.WinForms.Guna2Panel pnl)
                {
                    pnl.FillColor = Themes.ThemeManager.CardBackground;
                    pnl.BorderColor = Themes.ThemeManager.TextBoxBorder;
                }
            }
            else if (container.Tag?.ToString() == "ThemeTextPrimary")
            {
                container.ForeColor = Themes.ThemeManager.TextPrimary;
            }
            else if (container.Tag?.ToString() == "ThemeTextSecondary")
            {
                container.ForeColor = Themes.ThemeManager.TextSecondary;
            }
            else if (container.Tag?.ToString() == "ThemeClockPanel")
            {
                if (container is Guna.UI2.WinForms.Guna2Panel pnl)
                {
                    pnl.FillColor = Themes.ThemeManager.ButtonFill;
                    pnl.BorderThickness = 0;
                }
            }
            else if (container.Tag?.ToString() == "ThemeClockText")
            {
                container.ForeColor = Color.White;
            }

            
            if (container is Guna.UI2.WinForms.Guna2ToggleSwitch tgl)
            {
                tgl.CheckedState.FillColor = Themes.ThemeManager.ButtonFill;
                tgl.CheckedState.BorderColor = Themes.ThemeManager.ButtonFill;
                tgl.UncheckedState.FillColor = Color.LightGray;
                tgl.UncheckedState.BorderColor = Color.LightGray;
            }
            
            foreach (Control child in container.Controls)
            {
                ApplyThemeToDynamicControls(child);
            }
        }

                private void btnLogout_Click(object sender, EventArgs e)
        {
            Application.Restart();
            Environment.Exit(0);
        }



        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InitializeSidebarFooter()
        {
            panelUserProfile.Dock = DockStyle.Top;
            panelUserProfile.Height = 80;
            
            btnAvatar.Parent = panelUserProfile;
            lblUsername.Parent = panelUserProfile;
            lblRole.Parent = panelUserProfile;

            btnAvatar.Location = new Point(20, 20);
            btnAvatar.TextOffset = new Point(1, 1);
            lblUsername.Location = new Point(65, 20);
            lblRole.Location = new Point(65, 42);
            lblRole.Text = _currentUser.RoleId == 1 ? "Quản trị hệ thống" : "Nhân viên";
            lblRole.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblRole.Text = lblRole.Text.ToUpper();
            lblRole.ForeColor = Themes.ThemeManager.TextSecondary;

            Guna.UI2.WinForms.Guna2Panel panelFooter = new Guna.UI2.WinForms.Guna2Panel
            {
                Name = "panelFooter",
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.Transparent
            };
            panelSidebar.Controls.Add(panelFooter);

            Guna.UI2.WinForms.Guna2Panel sep = new Guna.UI2.WinForms.Guna2Panel 
            { 
                Name = "FooterSep",
                Location = new Point(20, 0), 
                Size = new Size(220, 1), 
                FillColor = Themes.ThemeManager.TextBoxBorder 
            };
            panelFooter.Controls.Add(sep);
            btnLogout.Parent = panelFooter;
            btnLogout.Location = new Point(0, 10);
            btnLogout.Size = new Size(260, 40);
            btnLogout.Text = "🚪   Đăng xuất";
            btnLogout.TextAlign = HorizontalAlignment.Left;
            btnLogout.TextOffset = new Point(20, 0);
            btnLogout.FillColor = Color.Transparent;
            btnLogout.Font = new Font("Segoe UI", 10F);
            btnLogout.ForeColor = Themes.ThemeManager.TextSecondary;
            btnLogout.HoverState.FillColor = Themes.ThemeManager.HoverColor;
        }

        private void InitializeTopBar()
        {
            if (panelTop.Controls.Contains(btnThemeToggle))
            {
                panelTop.Controls.Remove(btnThemeToggle);
            }
            
            panelTop.Height = 56;
            panelTop.CustomBorderThickness = new Padding(0, 0, 0, 1);

            Guna.UI2.WinForms.Guna2Panel pnlLeft = new Guna.UI2.WinForms.Guna2Panel { Dock = DockStyle.Left, Width = 400 };
            
            Label lblTopTitle = new Label 
            {
                Text = "QUẢN LÝ NHÀ SÁCH",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 12),
                Tag = "ThemeTextPrimary"
            };
            pnlLeft.Controls.Add(lblTopTitle);
            Guna.UI2.WinForms.Guna2Panel pnlRight = new Guna.UI2.WinForms.Guna2Panel { Dock = DockStyle.Right, Width = 480 };
            
            panelTop.Controls.Remove(btnMinimize);
            panelTop.Controls.Remove(btnMaximize);
            panelTop.Controls.Remove(btnExit);
            btnExit.Location = new Point(430, 10);
            btnMaximize.Location = new Point(380, 10);
            btnMinimize.Location = new Point(330, 10);
            pnlRight.Controls.Add(btnExit);
            pnlRight.Controls.Add(btnMaximize);
            pnlRight.Controls.Add(btnMinimize);

            btnThemeToggle.Location = new Point(230, 12);
            btnThemeToggle.Size = new Size(40, 32);
            btnThemeToggle.BorderRadius = 4;
            btnThemeToggle.Font = new Font("Segoe UI Emoji", 12F);
            btnThemeToggle.TextOffset = new Point(0, 0);
            pnlRight.Controls.Add(btnThemeToggle);

            Label lblClock = new Label 
            { 
                Text = DateTime.Now.ToString("HH:mm:ss"),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(80, 16),
                BackColor = Color.Transparent
            };
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 1000, Enabled = true };
            timer.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
            pnlRight.Controls.Add(lblClock);

            if (_currentUser.RoleId == 1)
            {
                Guna.UI2.WinForms.Guna2Button btnRoleToggle = new Guna.UI2.WinForms.Guna2Button
                {
                    Location = new Point(180, 12),
                    Size = new Size(40, 32),
                    BorderRadius = 4,
                    Font = new Font("Segoe UI Emoji", 12F),
                    TextOffset = new Point(0, 0),
                    Text = CurrentSession.ViewAsStaff ? "👤" : "👑",
                    FillColor = Color.Transparent,
                    ForeColor = Themes.ThemeManager.TextPrimary,
                    Cursor = Cursors.Hand
                };
                btnRoleToggle.HoverState.FillColor = Themes.ThemeManager.HoverColor;
                btnRoleToggle.Click += (s, e) => {
                    CurrentSession.ViewAsStaff = !CurrentSession.ViewAsStaff;
                    btnRoleToggle.Text = CurrentSession.ViewAsStaff ? "👤" : "👑";
                    lblRole.Text = CurrentSession.ViewAsStaff ? "NHÂN VIÊN" : "QUẢN TRỊ HỆ THỐNG";
                    InitializeAccordionSidebar();
                    ApplyTheme();
                    if (CurrentSession.IsStaff) {
                        BtnPOS_Click(this, EventArgs.Empty);
                    } else {
                        LoadDashboard();
                    }
                };
                pnlRight.Controls.Add(btnRoleToggle);
                btnRoleToggle.Tag = "ThemeTextPrimary";
            }

            Guna.UI2.WinForms.Guna2Button btnNotif = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new Point(280, 12),
                Size = new Size(40, 32),
                BorderRadius = 4,
                FillColor = Color.Transparent,
                Text = "🔔",
                Font = new Font("Segoe UI Emoji", 12F),
                Cursor = Cursors.Hand
            };

            // Badge label for unread count
            _notifBadge = new Label
            {
                Size = new Size(18, 18),
                Location = new Point(313, 8),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "0",
                Visible = false
            };
            _notifBadge.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, 18, 18, 18, 18));

            btnNotif.Click += (s, e) =>
            {
                if (_notifPanel != null && !_notifPanel.IsDisposed)
                {
                    _notifPanel.Close();
                    _notifPanel = null;
                    return;
                }
                _notifPanel = new NotificationPanel(btnNotif);
                _notifPanel.FormClosed += async (ns, ne) =>
                {
                    // refresh badge after closing
                    await System.Threading.Tasks.Task.Delay(100);
                };
                _notifPanel.Show(this);
            };

            pnlRight.Controls.Add(btnNotif);
            pnlRight.Controls.Add(_notifBadge);

            panelTop.Controls.Add(pnlLeft);
            panelTop.Controls.Add(pnlRight);
            
            // Tag for theming later
            pnlLeft.Tag = "TopLeft";
            pnlRight.Tag = "TopRight";

            lblClock.Tag = "ThemeTextPrimary";
            btnNotif.Tag = "ThemeTextSecondary";
        }

        [DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
    }
}
