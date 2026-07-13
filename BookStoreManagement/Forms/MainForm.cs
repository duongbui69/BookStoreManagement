using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Interfaces;

namespace BookStoreManagement.Forms
{
    public partial class MainForm : Form
    {
        private User _currentUser;
        private Guna.UI2.WinForms.Guna2TextBox txtGlobalSearch;
        private Label lblIcon;
        private ISearchableControl _currentSearchableControl;

        public MainForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            lblUsername.Text = _currentUser.FullName;
            
            InitializeTopBar();
            InitializeSidebarFooter();

            // Adjust UI based on Role
            // 1: Admin, 2: Staff
            if (_currentUser.RoleId == 2)
            {
                lblRole.Text = "Staff";
                btnHR.Visible = false;
                btnReports.Visible = false;
                btnSettings.Visible = false;
            }
            else
            {
                lblRole.Text = "Admin";
            }

            // Set Avatar Initials
            if (!string.IsNullOrEmpty(_currentUser.FullName))
            {
                var names = _currentUser.FullName.Split(' ');
                btnAvatar.Text = names.Length > 1 ? $"{names[0][0]}{names[names.Length - 1][0]}".ToUpper() : names[0].Substring(0, 2).ToUpper();
            }

            // Hook up events
            btnDashboard.Click += BtnDashboard_Click;
            btnCatalog.Click += BtnCatalog_Click;
            btnInventory.Click += BtnInventory_Click;
            btnOrders.Click += BtnOrders_Click;
            btnHR.Click += BtnHR_Click;
            btnReports.Click += BtnReports_Click;
            btnSettings.Click += BtnSettings_Click;
            btnThemeToggle.Click += BtnThemeToggle_Click;

            BookStoreManagement.Themes.ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ApplyTheme();

            // Load Dashboard by default
            LoadDashboard();
        }

        private void BtnInventory_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnInventory);
            panelMain.Controls.Clear();
            var invControl = new UserControls.InventoryControl();
            invControl.Dock = DockStyle.Fill;
            _currentSearchableControl = invControl;
            UpdateGlobalSearch("Search by ISBN, title or SKU...");
            panelMain.Controls.Add(invControl);
        }

        private void BtnCatalog_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnCatalog);
            panelMain.Controls.Clear();
            var catalogControl = new UserControls.CatalogControl();
            catalogControl.Dock = DockStyle.Fill;
            _currentSearchableControl = catalogControl;
            UpdateGlobalSearch("Search by book title, ISBN or author...");
            panelMain.Controls.Add(catalogControl);
        }

        private void BtnOrders_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnOrders);
            panelMain.Controls.Clear();
            var ordersControl = new UserControls.OrdersControl();
            ordersControl.Dock = DockStyle.Fill;
            _currentSearchableControl = ordersControl;
            UpdateGlobalSearch("Search orders, customers...");
            panelMain.Controls.Add(ordersControl);
        }

        private void BtnHR_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnHR);
            panelMain.Controls.Clear();
            var hrControl = new UserControls.HRControl();
            hrControl.Dock = DockStyle.Fill;
            _currentSearchableControl = hrControl;
            UpdateGlobalSearch("Search employees, roles, or departments...");
            panelMain.Controls.Add(hrControl);
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnReports);
            panelMain.Controls.Clear();
            var reportsControl = new UserControls.ReportsControl();
            reportsControl.Dock = DockStyle.Fill;
            _currentSearchableControl = reportsControl;
            UpdateGlobalSearch("Search reports...");
            panelMain.Controls.Add(reportsControl);
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnSettings);
            panelMain.Controls.Clear();
            var settingsControl = new UserControls.SettingsControl();
            settingsControl.Dock = DockStyle.Fill;
            _currentSearchableControl = settingsControl;
            UpdateGlobalSearch("Search system logs or settings...");
            panelMain.Controls.Add(settingsControl);
        }

        private void BtnDashboard_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnDashboard);
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            panelMain.Controls.Clear();
            var dashboardControl = new UserControls.DashboardControl();
            dashboardControl.Dock = DockStyle.Fill;
            _currentSearchableControl = dashboardControl;
            UpdateGlobalSearch("Search dashboard...");
            panelMain.Controls.Add(dashboardControl);
        }

        private void UpdateGlobalSearch(string placeholder)
        {
            if (txtGlobalSearch != null)
            {
                txtGlobalSearch.TextChanged -= TxtGlobalSearch_TextChanged;
                txtGlobalSearch.Text = "";
                
                if (string.IsNullOrEmpty(placeholder))
                {
                    txtGlobalSearch.Visible = false;
                    if (lblIcon != null) lblIcon.Visible = false;
                }
                else
                {
                    txtGlobalSearch.Visible = true;
                    txtGlobalSearch.PlaceholderText = placeholder;
                    if (lblIcon != null) lblIcon.Visible = true;
                }
                txtGlobalSearch.TextChanged += TxtGlobalSearch_TextChanged;
            }
        }

        private void TxtGlobalSearch_TextChanged(object sender, EventArgs e)
        {
            _currentSearchableControl?.PerformSearch(txtGlobalSearch.Text);
        }

        private void SetActiveTab(Guna.UI2.WinForms.Guna2Button activeButton)
        {
            foreach (Control c in panelSidebar.Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2Button btn && btn != btnLogout)
                {
                    if (btn == activeButton)
                    {
                        btn.CustomBorderThickness = new Padding(3, 0, 0, 0);
                        btn.CustomBorderColor = Themes.ThemeManager.ButtonFill;
                        btn.FillColor = Themes.ThemeManager.HoverColor;
                        btn.ForeColor = Themes.ThemeManager.ButtonFill;
                    }
                    else
                    {
                        btn.CustomBorderThickness = new Padding(0);
                        btn.FillColor = Color.Transparent;
                        btn.ForeColor = Themes.ThemeManager.TextSecondary;
                    }
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

        private void ApplyTheme()
        {
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
            foreach (Control c in panelSidebar.Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2Button btn && btn != btnLogout)
                {
                    btn.HoverState.FillColor = Themes.ThemeManager.HoverColor;
                    if (btn.CustomBorderThickness.Left > 0) // Active button check
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
            btnSettings.ForeColor = Themes.ThemeManager.TextSecondary;
            btnSettings.HoverState.FillColor = Themes.ThemeManager.HoverColor;
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
            else if (container.Tag?.ToString() == "ThemeSearch")
            {
                if (container is Guna.UI2.WinForms.Guna2TextBox txt)
                {
                    txt.FillColor = Themes.ThemeManager.TextBoxBackground;
                    txt.ForeColor = Themes.ThemeManager.TextPrimary;
                    txt.BorderColor = Themes.ThemeManager.TextBoxBorder;
                }
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
            this.Hide();
            var loginForm = new LoginForm();
            loginForm.ShowDialog();
            this.Close();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InitializeSidebarFooter()
        {
            panelSidebar.Controls.Remove(btnSettings);
            
            panelUserProfile.Height = 175;
            
            btnSettings.Parent = panelUserProfile;
            btnSettings.Location = new Point(0, 10);
            btnSettings.Size = new Size(260, 40);
            btnSettings.Text = "⚙   Settings";
            btnSettings.TextAlign = HorizontalAlignment.Left;
            btnSettings.TextOffset = new Point(20, 0);
            btnSettings.FillColor = Color.Transparent;
            btnSettings.Font = new Font("Segoe UI", 10F);
            btnSettings.ForeColor = Themes.ThemeManager.TextSecondary;
            btnSettings.HoverState.FillColor = Themes.ThemeManager.HoverColor;
            btnSettings.CustomBorderThickness = new Padding(0);

            btnLogout.Parent = panelUserProfile;
            btnLogout.Location = new Point(0, 50);
            btnLogout.Size = new Size(260, 40);
            btnLogout.Text = "🚪   Logout";
            btnLogout.TextAlign = HorizontalAlignment.Left;
            btnLogout.TextOffset = new Point(20, 0);
            btnLogout.FillColor = Color.Transparent;
            btnLogout.Font = new Font("Segoe UI", 10F);
            btnLogout.ForeColor = Themes.ThemeManager.TextSecondary;
            btnLogout.HoverState.FillColor = Themes.ThemeManager.HoverColor;

            Guna.UI2.WinForms.Guna2Panel sep = new Guna.UI2.WinForms.Guna2Panel 
            { 
                Name = "FooterSep",
                Location = new Point(20, 105), 
                Size = new Size(220, 1), 
                FillColor = Themes.ThemeManager.TextBoxBorder 
            };
            panelUserProfile.Controls.Add(sep);

            btnAvatar.Location = new Point(20, 120);
            lblUsername.Location = new Point(65, 120);
            lblRole.Location = new Point(65, 142);
            lblRole.Text = _currentUser.RoleId == 1 ? "System Administrator" : "Staff Member";
            lblRole.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblRole.Text = lblRole.Text.ToUpper();
            lblRole.ForeColor = Themes.ThemeManager.TextSecondary;
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
            
            lblIcon = new Label 
            { 
                Text = "🔍", 
                Font = new Font("Segoe UI Emoji", 14F),
                AutoSize = true,
                Location = new Point(20, 15),
                Visible = false
            };
            
            txtGlobalSearch = new Guna.UI2.WinForms.Guna2TextBox 
            { 
                PlaceholderText = "Search...",
                BorderRadius = 6,
                Location = new Point(60, 10),
                Size = new Size(320, 36),
                Font = new Font("Segoe UI", 10F),
                Visible = false
            };
            txtGlobalSearch.TextChanged += TxtGlobalSearch_TextChanged;

            pnlLeft.Controls.Add(lblIcon);
            pnlLeft.Controls.Add(txtGlobalSearch);
            
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

            Guna.UI2.WinForms.Guna2Panel pnlTheme = new Guna.UI2.WinForms.Guna2Panel 
            { 
                Location = new Point(20, 12),
                Size = new Size(130, 32),
                BorderRadius = 16,
                BorderThickness = 1,
            };
            Label lblLight = new Label { Text = "☀️", Font = new Font("Segoe UI Emoji", 10F), AutoSize = true, Location = new Point(10, 6) };
            Guna.UI2.WinForms.Guna2ToggleSwitch tglTheme = new Guna.UI2.WinForms.Guna2ToggleSwitch 
            { 
                Location = new Point(45, 6),
                Size = new Size(40, 20),
                Checked = Themes.ThemeManager.IsDarkMode
            };
            tglTheme.CheckedChanged += (s, e) => {
                if (tglTheme.Checked != Themes.ThemeManager.IsDarkMode)
                    Themes.ThemeManager.ToggleTheme();
            };
            Label lblDark = new Label { Text = "🌙", Font = new Font("Segoe UI Emoji", 10F), AutoSize = true, Location = new Point(95, 6) };
            pnlTheme.Controls.Add(lblLight);
            pnlTheme.Controls.Add(tglTheme);
            pnlTheme.Controls.Add(lblDark);
            pnlRight.Controls.Add(pnlTheme);

            Guna.UI2.WinForms.Guna2Panel pnlClock = new Guna.UI2.WinForms.Guna2Panel
            {
                Location = new Point(165, 12),
                Size = new Size(100, 32),
                BorderRadius = 4,
                BorderThickness = 1,
            };
            Label lblClock = new Label 
            { 
                Text = DateTime.Now.ToString("HH:mm:ss"),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(15, 6)
            };
            pnlClock.Controls.Add(lblClock);
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 1000, Enabled = true };
            timer.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
            pnlRight.Controls.Add(pnlClock);

            Guna.UI2.WinForms.Guna2Button btnNotif = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new Point(280, 12),
                Size = new Size(40, 32),
                BorderRadius = 4,
                FillColor = Color.Transparent,
                Text = "🔔",
                Font = new Font("Segoe UI Emoji", 12F)
            };
            pnlRight.Controls.Add(btnNotif);

            panelTop.Controls.Add(pnlLeft);
            panelTop.Controls.Add(pnlRight);
            
            // Tag for theming later
            pnlLeft.Tag = "TopLeft";
            pnlRight.Tag = "TopRight";
            lblIcon.Tag = "ThemeTextPrimary";
            txtGlobalSearch.Tag = "ThemeSearch";
            pnlTheme.Tag = "ThemePanel";
            lblLight.Tag = "ThemeTextSecondary";
            lblDark.Tag = "ThemeTextSecondary";
            tglTheme.Tag = "ThemeToggle";
            pnlClock.Tag = "ThemePanel";
            lblClock.Tag = "ThemeTextSecondary";
            btnNotif.Tag = "ThemeTextSecondary";
        }
    }
}
