using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Themes;
using BookStoreManagement.Interfaces;
using Guna.UI2.WinForms;
using System.Collections.Generic;

namespace BookStoreManagement.UserControls
{
    public partial class SettingsControl : UserControl, ISearchableControl
    {
        private Guna2Panel pnlContent;
        
        // Header
        private Guna2Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubTitle;

        // Layout
        private TableLayoutPanel tlpMain;
        private Guna2Panel pnlSidebar;
        private Guna2Panel pnlMainContent;

        // Sidebar Buttons
        private Guna2Button btnGeneral;
        private Guna2Button btnAppearance;
        private Guna2Button btnSecurity;
        private Guna2Button btnAlerts;
        private Guna2Button btnBackup;
        private List<Guna2Button> menuButtons = new List<Guna2Button>();

        // Content Panels
        private Guna2Panel pnlGeneral;
        private Guna2Panel pnlAppearance;
        private Guna2Panel pnlSecurity;
        private Guna2Panel pnlAlerts;
        private Guna2Panel pnlBackup;

        // Controls to theme
        private List<Control> cards = new List<Control>();
        private List<Label> standardLabels = new List<Label>();
        private List<Label> primaryLabels = new List<Label>();
        private List<Label> iconLabels = new List<Label>();
        private List<Control> borderPanels = new List<Control>();
        private List<Guna2TextBox> textBoxes = new List<Guna2TextBox>();
        private List<Guna2ComboBox> comboBoxes = new List<Guna2ComboBox>();
        private List<Guna2Button> actionButtons = new List<Guna2Button>();
        private List<Guna2Button> outlineButtons = new List<Guna2Button>();

        public void PerformSearch(string keyword)
        {
            // Search not implemented
        }

        public SettingsControl()
        {
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);
            this.AutoScroll = true;

            int gutter = 20;

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = true };

            // 1. Header
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(0, 0, 0, gutter) };
            lblTitle = new Label { Text = "Cài đặt Hệ thống", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Quản lý cấu hình, bảo mật, và hệ thống.", Font = new Font("Segoe UI", 11F), AutoSize = true, Location = new Point(2, 40) };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });
            
            primaryLabels.Add(lblTitle);
            standardLabels.Add(lblSubTitle);

            // 2. Main Layout (Sidebar + Content)
            tlpMain = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, 
                Height = 850, 
                ColumnCount = 2, 
                RowCount = 1,
                Margin = new Padding(0)
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            pnlSidebar = new Guna2Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, gutter, 0) };
            pnlMainContent = new Guna2Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };

            // --- 3. Sidebar Menu ---
            int btnY = 0;
            btnGeneral = CreateMenuButton("ðŸ¢", "General Settings", btnY); btnY += 50;
            btnAppearance = CreateMenuButton("ðŸŽ¨", "Appearance", btnY); btnY += 50;
            btnSecurity = CreateMenuButton("ðŸ”", "Security & Roles", btnY); btnY += 50;
            btnAlerts = CreateMenuButton("ðŸ””", "Alerts & Notifications", btnY); btnY += 50;
            btnBackup = CreateMenuButton("ðŸ’¾", "Data & Backups", btnY); btnY += 50;

            btnGeneral.Click += (s, e) => ShowPanel(pnlGeneral, btnGeneral);
            btnAppearance.Click += (s, e) => ShowPanel(pnlAppearance, btnAppearance);
            btnSecurity.Click += (s, e) => ShowPanel(pnlSecurity, btnSecurity);
            btnAlerts.Click += (s, e) => ShowPanel(pnlAlerts, btnAlerts);
            btnBackup.Click += (s, e) => ShowPanel(pnlBackup, btnBackup);

            pnlSidebar.Controls.AddRange(new Control[] { btnGeneral, btnAppearance, btnSecurity, btnAlerts, btnBackup });

            // --- 4. Content Panels ---
            pnlGeneral = CreateGeneralPanel();
            pnlAppearance = CreateAppearancePanel();
            pnlSecurity = CreateSecurityPanel();
            pnlAlerts = CreateAlertsPanel();
            pnlBackup = CreateBackupPanel();

            pnlMainContent.Controls.AddRange(new Control[] { pnlBackup, pnlAlerts, pnlSecurity, pnlAppearance, pnlGeneral });

            tlpMain.Controls.Add(pnlSidebar, 0, 0);
            tlpMain.Controls.Add(pnlMainContent, 1, 0);

            pnlContent.Controls.Add(tlpMain);
            pnlContent.Controls.Add(pnlHeader);
            this.Controls.Add(pnlContent);

            // Default view
            ShowPanel(pnlGeneral, btnGeneral);
            ApplyTheme();
        }

        private Guna2Button CreateMenuButton(string emoji, string text, int y)
        {
            var btn = new Guna2Button
            {
                Text = $"{emoji}    {text}",
                Location = new Point(0, y),
                Size = new Size(260, 45),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Left,
                TextOffset = new Point(15, 0),
                Cursor = Cursors.Hand,
                Animated = true,
                FillColor = Color.Transparent,
                CustomBorderThickness = new Padding(4, 0, 0, 0),
                CustomBorderColor = Color.Transparent
            };
            menuButtons.Add(btn);
            return btn;
        }

        private void ShowPanel(Guna2Panel pnlToShow, Guna2Button activeBtn)
        {
            pnlToShow.BringToFront();
            foreach (var btn in menuButtons)
            {
                if (btn == activeBtn)
                {
                    btn.FillColor = ThemeManager.HoverColor;
                    btn.CustomBorderColor = ThemeManager.ButtonFill;
                    btn.ForeColor = ThemeManager.TextPrimary;
                }
                else
                {
                    btn.FillColor = Color.Transparent;
                    btn.CustomBorderColor = Color.Transparent;
                    btn.ForeColor = ThemeManager.TextSecondary;
                }
            }
        }

        private Guna2Panel CreateCard(string title, string iconText, int height, int y)
        {
            var card = new Guna2Panel { Location = new Point(0, y), Width = 700, Height = height, BorderRadius = 8, BorderThickness = 1 };
            
            var pnlTitle = new Guna2Panel { Dock = DockStyle.Top, Height = 50, CustomBorderThickness = new Padding(0,0,0,1) };
            var icon = new Label { Text = iconText, Font = new Font("Material Symbols Outlined", 14F), AutoSize = true, Location = new Point(20, 15) };
            var lbl = new Label { Text = title, Font = new Font("Segoe UI", 11F, FontStyle.Bold), AutoSize = true, Location = new Point(50, 15) };
            
            pnlTitle.Controls.AddRange(new Control[] { icon, lbl });
            card.Controls.Add(pnlTitle);
            
            cards.Add(card);
            borderPanels.Add(pnlTitle);
            iconLabels.Add(icon);
            primaryLabels.Add(lbl);

            card.Resize += (s, e) => {
                pnlTitle.Width = card.Width;
            };

            return card;
        }

        // --- Panel Builders ---

        private Guna2Panel CreateGeneralPanel()
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Fill };
            
            var card1 = CreateCard("Store Information", "store", 300, 0);
            var txtStoreName = CreateInputGroup("Store Name", "Bookwise Downtown", false, 20, 70);
            var txtBranchId = CreateInputGroup("Branch ID", "BW-NYC-001", true, 340, 70);
            var txtAddress = CreateInputGroup("Physical Address", "120 Broadway, New York, NY 10271", false, 20, 150);
            txtAddress.Width = 560; 
            var btnSave = new Guna2Button { Text = "Lưu thay đổi", Size = new Size(130, 40), BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(20, 230) };
            
            card1.Controls.AddRange(new Control[] { txtStoreName, txtBranchId, txtAddress, btnSave });
            actionButtons.Add(btnSave);

            var card2 = CreateCard("Region & Language", "public", 180, 320);
            var cbTimezone = CreateComboGroup("System Timezone", new[] { "Eastern Time (US)", "Central Time", "Pacific Time", "Vietnam (GMT+7)" }, 20, 70);
            var cbCurrency = CreateComboGroup("Currency", new[] { "USD ($)", "₫ (â‚«)" }, 340, 70);
            
            card2.Controls.AddRange(new Control[] { cbTimezone, cbCurrency });

            pnl.Controls.AddRange(new Control[] { card1, card2 });
            pnl.Resize += (s, e) => {
                card1.Width = pnl.Width - 20;
                card2.Width = pnl.Width - 20;
                if (card1.Width > 100)
                {
                    int half = (card1.Width - 60) / 2;
                    txtStoreName.Width = half;
                    txtBranchId.Width = half;
                    txtBranchId.Left = 40 + half;
                    txtAddress.Width = card1.Width - 40;
                    
                    cbTimezone.Width = half;
                    cbCurrency.Width = half;
                    cbCurrency.Left = 40 + half;
                }
            };
            return pnl;
        }

        private Guna2Panel CreateAppearancePanel()
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Fill };
            
            var card1 = CreateCard("Theme Preferences", "dark_mode", 180, 0);
            var lblDesc = new Label { Text = "Chuyển đổi giữa chế độ Sáng và Tối.", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(20, 70) };
            var toggleTheme = new Guna2ToggleSwitch { Checked = ThemeManager.IsDarkMode, Location = new Point(20, 100), Size = new Size(50, 25) };
            var lblMode = new Label { Text = ThemeManager.IsDarkMode ? "Dark Mode Active" : "Light Mode Active", Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(80, 103) };
            
            toggleTheme.CheckedChanged += (s, e) => {
                ThemeManager.ToggleTheme();
                lblMode.Text = ThemeManager.IsDarkMode ? "Dark Mode Active" : "Light Mode Active";
                toggleTheme.Checked = ThemeManager.IsDarkMode;
            };
            
            card1.Controls.AddRange(new Control[] { lblDesc, toggleTheme, lblMode });
            standardLabels.Add(lblDesc);
            primaryLabels.Add(lblMode);

            pnl.Controls.Add(card1);
            pnl.Resize += (s, e) => card1.Width = pnl.Width - 20;
            return pnl;
        }

        private Guna2Panel CreateSecurityPanel()
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Fill };
            
            var card1 = CreateCard("Password Policy", "password", 260, 0);
            var row1 = CreateToggleRow("Require Strong Passwords", "Enforce uppercase, numbers, and special characters.", true, 70);
            var row2 = CreateToggleRow("Two-Factor Authentication", "Require 2FA for administrative accounts.", false, 140);
            
            card1.Controls.AddRange(new Control[] { row1, row2 });

            var card2 = CreateCard("Default Role Permissions", "manage_accounts", 260, 280);
            var row3 = CreateToggleRow("Allow Price Overrides", "Permit staff to change prices at POS.", false, 70);
            var row4 = CreateToggleRow("Allow Void Transactions", "Permit staff to cancel completed sales.", false, 140);
            
            card2.Controls.AddRange(new Control[] { row3, row4 });

            pnl.Controls.AddRange(new Control[] { card1, card2 });
            pnl.Resize += (s, e) => {
                card1.Width = pnl.Width - 20;
                card2.Width = pnl.Width - 20;
                row1.Width = card1.Width - 40;
                row2.Width = card1.Width - 40;
                row3.Width = card2.Width - 40;
                row4.Width = card2.Width - 40;
            };
            return pnl;
        }

        private Guna2Panel CreateAlertsPanel()
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Fill };
            var card1 = CreateCard("Inventory Alerts", "notifications_active", 200, 0);
            var lblTitle = new Label { Text = "Mức cảnh báo sắp hết", Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 70) };
            var lblSub = new Label { Text = "Báo động khi số lượng thấp hơn mức này.", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(20, 95) };
            
            var txtThreshold = new Guna2TextBox { Text = "15", Size = new Size(60, 30), Location = new Point(200, 70), TextAlign = HorizontalAlignment.Center, BorderRadius = 4 };
            var trackBar = new Guna2TrackBar { Location = new Point(20, 130), Width = 300, Minimum = 1, Maximum = 100, Value = 15 };
            trackBar.ValueChanged += (s, e) => txtThreshold.Text = trackBar.Value.ToString();

            textBoxes.Add(txtThreshold);
            primaryLabels.Add(lblTitle);
            standardLabels.Add(lblSub);

            card1.Controls.AddRange(new Control[] { lblTitle, lblSub, txtThreshold, trackBar });
            
            var card2 = CreateCard("Email Settings (SMTP)", "mail", 200, 220);
            var lblSmtp = new Label { Text = "Cấu hình server email để gửi báo cáo.", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(20, 70) };
            var btnConfig = new Guna2Button { Text = "Cấu hình SMTP", Size = new Size(150, 40), BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(20, 110) };
            
            actionButtons.Add(btnConfig);
            standardLabels.Add(lblSmtp);

            card2.Controls.AddRange(new Control[] { lblSmtp, btnConfig });

            pnl.Controls.AddRange(new Control[] { card1, card2 });
            pnl.Resize += (s, e) => {
                card1.Width = pnl.Width - 20;
                card2.Width = pnl.Width - 20;
                txtThreshold.Left = card1.Width - txtThreshold.Width - 20;
                trackBar.Width = card1.Width - 40;
            };
            return pnl;
        }

        private Guna2Panel CreateBackupPanel()
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Fill };
            var card1 = CreateCard("Data Backups", "cloud_sync", 350, 0);
            
            var pnlStatus = new Guna2Panel { Size = new Size(300, 90), Location = new Point(20, 70), BorderRadius = 8, BorderThickness = 1 };
            var iconStatus = new Label { Text = "cloud_done", Font = new Font("Material Symbols Outlined", 20F), ForeColor = Color.FromArgb(0, 186, 97), AutoSize = true, Location = new Point(15, 25) };
            var lblStatusTitle = new Label { Text = "TRẠNG THÁI", Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(60, 15) };
            var lblStatusVal = new Label { Text = "Bình thường", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(58, 32) };
            var lblStatusSub = new Label { Text = "Bản sao lưu tự động hoàn tất lúc 02:00 SA.", Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(60, 55) };
            
            pnlStatus.Controls.AddRange(new Control[] { iconStatus, lblStatusTitle, lblStatusVal, lblStatusSub });
            borderPanels.Add(pnlStatus);
            primaryLabels.Add(lblStatusTitle);
            primaryLabels.Add(lblStatusVal);
            standardLabels.Add(lblStatusSub);

            var btnManual = new Guna2Button { Text = "â–¶ Run Manual Backup", Size = new Size(300, 40), Location = new Point(20, 180), BorderRadius = 4, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            var btnRestore = new Guna2Button { Text = "Khôi phục từ File", Size = new Size(300, 40), Location = new Point(20, 230), BorderRadius = 4, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            
            outlineButtons.Add(btnManual);
            outlineButtons.Add(btnRestore);

            card1.Controls.AddRange(new Control[] { pnlStatus, btnManual, btnRestore });
            pnl.Controls.Add(card1);
            pnl.Resize += (s, e) => {
                card1.Width = pnl.Width - 20;
                pnlStatus.Width = card1.Width - 40;
                btnManual.Width = card1.Width - 40;
                btnRestore.Width = card1.Width - 40;
            };
            return pnl;
        }

        // --- Helpers ---

        private Guna2Panel CreateInputGroup(string label, string value, bool readOnly, int x, int y)
        {
            var pnl = new Guna2Panel { Height = 65, Width = 240, Location = new Point(x, y) };
            var lbl = new Label { Text = label, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 10) };
            var txt = new Guna2TextBox { Text = value, ReadOnly = readOnly, Location = new Point(0, 25), Size = new Size(240, 36), BorderRadius = 4 };
            pnl.Controls.AddRange(new Control[] { lbl, txt });
            pnl.Resize += (s, e) => txt.Width = pnl.Width;
            
            primaryLabels.Add(lbl);
            textBoxes.Add(txt);
            return pnl;
        }

        private Guna2Panel CreateComboGroup(string label, string[] items, int x, int y)
        {
            var pnl = new Guna2Panel { Height = 65, Width = 240, Location = new Point(x, y) };
            var lbl = new Label { Text = label, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 10) };
            var cb = new Guna2ComboBox { Location = new Point(0, 25), Size = new Size(240, 36), BorderRadius = 4, Font = new Font("Segoe UI", 9F) };
            cb.Items.AddRange(items);
            if (items.Length > 0) cb.SelectedIndex = 0;
            pnl.Controls.AddRange(new Control[] { lbl, cb });
            pnl.Resize += (s, e) => cb.Width = pnl.Width;

            primaryLabels.Add(lbl);
            comboBoxes.Add(cb);
            return pnl;
        }

        private Guna2Panel CreateToggleRow(string title, string desc, bool isChecked, int y)
        {
            var pnl = new Guna2Panel { Location = new Point(20, y), Height = 60, Width = 500, BorderRadius = 8, BorderThickness = 1 };
            var lbl1 = new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(15, 10) };
            var lbl2 = new Label { Text = desc, Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(15, 30) };
            var toggle = new Guna2ToggleSwitch { Checked = isChecked, Location = new Point(440, 20) };
            pnl.Controls.AddRange(new Control[] { lbl1, lbl2, toggle });
            pnl.Resize += (s, e) => toggle.Left = pnl.Width - toggle.Width - 20;

            borderPanels.Add(pnl);
            primaryLabels.Add(lbl1);
            standardLabels.Add(lbl2);
            return pnl;
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            BookStoreManagement.Themes.ThemeManager.ApplyTypography(this);
            this.BackColor = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;

            // Re-apply hover colors to sidebar
            foreach (var btn in menuButtons)
            {
                if (btn.FillColor == Color.Transparent)
                {
                    btn.ForeColor = ThemeManager.TextSecondary;
                }
                else
                {
                    btn.FillColor = ThemeManager.HoverColor;
                    btn.CustomBorderColor = ThemeManager.ButtonFill;
                    btn.ForeColor = ThemeManager.TextPrimary;
                }
            }

            foreach (var lbl in primaryLabels) lbl.ForeColor = ThemeManager.TextPrimary;
            foreach (var lbl in standardLabels) lbl.ForeColor = ThemeManager.TextSecondary;
            foreach (var icon in iconLabels) icon.ForeColor = ThemeManager.TextSecondary; // Or use Accent color
            
            foreach (var card in cards)
            {
                var pnl = card as Guna2Panel;
                pnl.FillColor = ThemeManager.CardBackground;
                pnl.BorderColor = ThemeManager.TextBoxBorder;
            }

            foreach (var bp in borderPanels)
            {
                var pnl = bp as Guna2Panel;
                if (pnl.CustomBorderThickness.Bottom > 0)
                {
                    pnl.CustomBorderColor = ThemeManager.TextBoxBorder;
                }
                if (pnl.BorderThickness > 0)
                {
                    pnl.BorderColor = ThemeManager.TextBoxBorder;
                    pnl.FillColor = ThemeManager.Background;
                }
            }

            foreach (var txt in textBoxes)
            {
                txt.FillColor = txt.ReadOnly ? ThemeManager.Background : ThemeManager.TextBoxBackground;
                txt.ForeColor = ThemeManager.TextPrimary;
                txt.BorderColor = ThemeManager.TextBoxBorder;
            }

            foreach (var cb in comboBoxes)
            {
                cb.FillColor = cb.Enabled ? ThemeManager.TextBoxBackground : ThemeManager.Background;
                cb.ForeColor = ThemeManager.TextPrimary;
                cb.BorderColor = ThemeManager.TextBoxBorder;
            }

            foreach (var btn in actionButtons)
            {
                btn.FillColor = ThemeManager.ButtonFill;
                btn.ForeColor = ThemeManager.ButtonText;
            }

            foreach (var btn in outlineButtons)
            {
                btn.FillColor = ThemeManager.Background;
                btn.ForeColor = ThemeManager.TextPrimary;
                btn.BorderColor = ThemeManager.TextBoxBorder;
            }
        }
    
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BookStoreManagement.Themes.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            }
            base.Dispose(disposing);
        }
    }
}

