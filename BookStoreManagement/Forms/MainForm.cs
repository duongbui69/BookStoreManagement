using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;

namespace BookStoreManagement.Forms
{
    public partial class MainForm : Form
    {
        private User _currentUser;

        public MainForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            lblUsername.Text = _currentUser.FullName;

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
            panelMain.Controls.Add(invControl);
        }

        private void BtnCatalog_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnCatalog);
            panelMain.Controls.Clear();
            var catalogControl = new UserControls.CatalogControl();
            catalogControl.Dock = DockStyle.Fill;
            panelMain.Controls.Add(catalogControl);
        }

        private void BtnOrders_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnOrders);
            panelMain.Controls.Clear();
            var ordersControl = new UserControls.OrdersControl();
            ordersControl.Dock = DockStyle.Fill;
            panelMain.Controls.Add(ordersControl);
        }

        private void BtnHR_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnHR);
            panelMain.Controls.Clear();
            var hrControl = new UserControls.HRControl();
            hrControl.Dock = DockStyle.Fill;
            panelMain.Controls.Add(hrControl);
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnReports);
            panelMain.Controls.Clear();
            var reportsControl = new UserControls.ReportsControl();
            reportsControl.Dock = DockStyle.Fill;
            panelMain.Controls.Add(reportsControl);
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            SetActiveTab(btnSettings);
            panelMain.Controls.Clear();
            var settingsControl = new UserControls.SettingsControl();
            settingsControl.Dock = DockStyle.Fill;
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
            panelMain.Controls.Add(dashboardControl);
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
    }
}
