using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Themes;
using BookStoreManagement.Interfaces;
using Guna.UI2.WinForms;

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
        private Guna2Panel pnlLeftCol;
        private Guna2Panel pnlRightCol;

        // Cards
        private Guna2Panel cardBranch;
        private Guna2Panel cardPermissions;
        private Guna2Panel cardAlerts;
        private Guna2Panel cardBackups;

        public void PerformSearch(string keyword)
        {
            // Search not implemented for this layout
        }

        public SettingsControl()
        {
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 20;
            this.Padding = new Padding(0);
            this.AutoScroll = true;

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = true };

            // 1. Header
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 70, Margin = new Padding(0, 0, 0, gutter) };
            lblTitle = new Label { Text = "System Settings", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Manage core configuration and operational parameters.", Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(2, 40) };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            // 2. Main Layout
            tlpMain = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, 
                Height = 850, 
                ColumnCount = 2, 
                RowCount = 1,
                Margin = new Padding(0)
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66.66F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            pnlLeftCol = new Guna2Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, gutter, 0) };
            pnlRightCol = new Guna2Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };

            // --- LEFT COLUMN ---
            
            // Branch Details
            cardBranch = CreateCard("Branch Details", "store", 350);
            cardBranch.Dock = DockStyle.Top;
            cardBranch.Margin = new Padding(0, 0, 0, gutter);
            
            var txtStoreName = CreateInputGroup("Store Name", "Bookwise Downtown", false);
            var txtBranchId = CreateInputGroup("Branch ID", "BW-NYC-001", true);
            var txtAddress = CreateInputGroup("Physical Address", "120 Broadway, New York, NY 10271", false);
            txtAddress.Width = 520; // span across
            var cbTimezone = CreateComboGroup("System Timezone", new[] { "Eastern Time (US & Canada)", "Central Time", "Pacific Time" });
            var cbCurrency = CreateComboGroup("Currency", new[] { "USD ($)" });
            cbCurrency.Enabled = false;

            txtStoreName.Location = new Point(20, 70);
            txtBranchId.Location = new Point(280, 70);
            txtAddress.Location = new Point(20, 150);
            cbTimezone.Location = new Point(20, 230);
            cbCurrency.Location = new Point(280, 230);

            var btnSave = new Guna2Button { Text = "Save Changes", Size = new Size(130, 40), BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(20, 300) };
            btnSave.Click += (s, e) => MessageBox.Show("Settings saved!");

            cardBranch.Controls.AddRange(new Control[] { txtStoreName, txtBranchId, txtAddress, cbTimezone, cbCurrency, btnSave });
            cardBranch.Resize += (s, e) => {
                int half = (cardBranch.Width - 60) / 2;
                txtStoreName.Width = half;
                txtBranchId.Width = half;
                txtBranchId.Left = 40 + half;
                txtAddress.Width = cardBranch.Width - 40;
                cbTimezone.Width = half;
                cbCurrency.Width = half;
                cbCurrency.Left = 40 + half;
                btnSave.Left = cardBranch.Width - btnSave.Width - 20;
            };

            // Default User Permissions
            cardPermissions = CreateCard("Default User Permissions", "admin_panel_settings", 330);
            cardPermissions.Dock = DockStyle.Top;
            cardPermissions.Margin = new Padding(0, 0, 0, gutter);

            var lblPermSub = new Label { Text = "These settings apply to newly created Staff accounts unless overridden specifically.", Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(20, 65) };
            cardPermissions.Controls.Add(lblPermSub);

            var row1 = CreateToggleRow("Inventory Modification", "Allow users to manually adjust stock levels.", false, 100);
            var row2 = CreateToggleRow("Price Overrides", "Permit changing listed prices at the Point of Sale.", true, 170);
            var row3 = CreateToggleRow("Void Transactions", "Allow users to cancel completed sales records.", false, 240);
            
            cardPermissions.Controls.AddRange(new Control[] { row1, row2, row3 });

            // Alert Rules
            cardAlerts = CreateCard("Alert Rules", "notifications_active", 170);
            cardAlerts.Dock = DockStyle.Top;

            var lblAlertTitle = new Label { Text = "Global Low Stock Threshold", Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 70) };
            var lblAlertSub = new Label { Text = "Trigger a system alert when a title falls below this quantity.", Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(20, 90) };
            
            var txtThreshold = new Guna2TextBox { Text = "15", Size = new Size(60, 30), Location = new Point(200, 70), TextAlign = HorizontalAlignment.Center, BorderRadius = 4 };
            var trackBar = new Guna2TrackBar { Location = new Point(20, 120), Width = 300, Minimum = 1, Maximum = 100, Value = 15 };
            trackBar.ValueChanged += (s, e) => txtThreshold.Text = trackBar.Value.ToString();
            
            cardAlerts.Controls.AddRange(new Control[] { lblAlertTitle, lblAlertSub, txtThreshold, trackBar });
            cardAlerts.Resize += (s, e) => {
                txtThreshold.Left = cardAlerts.Width - txtThreshold.Width - 20;
                trackBar.Width = cardAlerts.Width - 40;
            };

            // Order of insertion matters for Dock = Top (bottom up or bring to front)
            pnlLeftCol.Controls.Add(cardAlerts);
            pnlLeftCol.Controls.Add(cardPermissions);
            pnlLeftCol.Controls.Add(cardBranch);
            
            cardBranch.BringToFront();
            cardPermissions.BringToFront();
            cardAlerts.BringToFront();

            // --- RIGHT COLUMN ---
            cardBackups = CreateCard("System Backups", "backup", 500);
            cardBackups.Dock = DockStyle.Top;
            
            var pnlStatus = new Guna2Panel { Size = new Size(300, 90), Location = new Point(20, 70), BorderRadius = 8, BorderThickness = 1 };
            var iconStatus = new Label { Text = "cloud_done", Font = new Font("Material Symbols Outlined", 20F), ForeColor = Color.FromArgb(0, 186, 97), AutoSize = true, Location = new Point(15, 25) };
            var lblStatusTitle = new Label { Text = "STATUS", Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(60, 15) };
            var lblStatusVal = new Label { Text = "Healthy", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(58, 32) };
            var lblStatusSub = new Label { Text = "Last automated backup completed\nsuccessfully today at 02:00 AM.", Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(60, 55) };
            pnlStatus.Controls.AddRange(new Control[] { iconStatus, lblStatusTitle, lblStatusVal, lblStatusSub });

            var btnManual = new Guna2Button { Text = "▶ Run Manual Backup", Size = new Size(300, 40), Location = new Point(20, 180), BorderRadius = 4, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            
            var lblLogsTitle = new Label { Text = "RECENT LOGS", Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 240) };
            
            var log1 = CreateLogItem("Auto Backup", "Oct 24, 02:00 AM", "Success", 270);
            var log2 = CreateLogItem("Manual Backup", "Oct 23, 14:30 PM", "Success", 330);
            var log3 = CreateLogItem("Auto Backup", "Oct 23, 02:00 AM", "Success", 390);

            var linkAll = new LinkLabel { Text = "View all logs", Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 460), LinkBehavior = LinkBehavior.HoverUnderline };

            cardBackups.Controls.AddRange(new Control[] { pnlStatus, btnManual, lblLogsTitle, log1, log2, log3, linkAll });
            cardBackups.Resize += (s, e) => {
                pnlStatus.Width = cardBackups.Width - 40;
                btnManual.Width = cardBackups.Width - 40;
                log1.Width = cardBackups.Width - 40;
                log2.Width = cardBackups.Width - 40;
                log3.Width = cardBackups.Width - 40;
            };

            pnlRightCol.Controls.Add(cardBackups);

            tlpMain.Controls.Add(pnlLeftCol, 0, 0);
            tlpMain.Controls.Add(pnlRightCol, 1, 0);

            pnlContent.Controls.Add(tlpMain);
            pnlContent.Controls.Add(pnlHeader);

            this.Controls.Add(pnlContent);

            ApplyTheme();
        }

        private Guna2Panel CreateCard(string title, string iconText, int height)
        {
            var card = new Guna2Panel { Height = height, BorderRadius = 8, BorderThickness = 1 };
            
            var pnlTitle = new Guna2Panel { Dock = DockStyle.Top, Height = 50, CustomBorderThickness = new Padding(0,0,0,1) };
            var icon = new Label { Text = iconText, Font = new Font("Material Symbols Outlined", 16F), AutoSize = true, Location = new Point(20, 15) };
            var lbl = new Label { Text = title, Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(50, 15), Name = "CardTitle" };
            pnlTitle.Controls.AddRange(new Control[] { icon, lbl });
            
            card.Controls.Add(pnlTitle);
            return card;
        }

        private Guna2Panel CreateInputGroup(string label, string value, bool readOnly)
        {
            var pnl = new Guna2Panel { Height = 60, Width = 240 };
            var lbl = new Label { Text = label, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            var txt = new Guna2TextBox { Text = value, ReadOnly = readOnly, Location = new Point(0, 22), Size = new Size(240, 36), BorderRadius = 4 };
            pnl.Controls.AddRange(new Control[] { lbl, txt });
            pnl.Resize += (s, e) => txt.Width = pnl.Width;
            return pnl;
        }

        private Guna2Panel CreateComboGroup(string label, string[] items)
        {
            var pnl = new Guna2Panel { Height = 60, Width = 240 };
            var lbl = new Label { Text = label, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            var cb = new Guna2ComboBox { Location = new Point(0, 22), Size = new Size(240, 36), BorderRadius = 4, Font = new Font("Segoe UI", 9F) };
            cb.Items.AddRange(items);
            if (items.Length > 0) cb.SelectedIndex = 0;
            pnl.Controls.AddRange(new Control[] { lbl, cb });
            pnl.Resize += (s, e) => cb.Width = pnl.Width;
            return pnl;
        }

        private Guna2Panel CreateToggleRow(string title, string desc, bool isChecked, int y)
        {
            var pnl = new Guna2Panel { Location = new Point(20, y), Height = 60, Width = 500, BorderRadius = 8, BorderThickness = 1 };
            var lbl1 = new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(15, 10), Name = "ToggleTitle" };
            var lbl2 = new Label { Text = desc, Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(15, 30), Name = "ToggleDesc" };
            var toggle = new Guna2ToggleSwitch { Checked = isChecked, Location = new Point(440, 20) };
            pnl.Controls.AddRange(new Control[] { lbl1, lbl2, toggle });
            pnl.Resize += (s, e) => toggle.Left = pnl.Width - toggle.Width - 20;
            return pnl;
        }

        private Guna2Panel CreateLogItem(string title, string time, string status, int y)
        {
            var pnl = new Guna2Panel { Location = new Point(20, y), Height = 60, Width = 300, CustomBorderThickness = new Padding(0,0,0,1) };
            var lbl1 = new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 10), Name = "LogTitle" };
            var lbl2 = new Label { Text = time, Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(0, 30), Name = "LogTime" };
            var badge = new Guna2Panel { Size = new Size(60, 24), Location = new Point(240, 15), BorderRadius = 4, Name = "BadgePanel" };
            var lblStatus = new Label { Text = status, Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(5, 5), Name = "BadgeLabel" };
            badge.Controls.Add(lblStatus);
            pnl.Controls.AddRange(new Control[] { lbl1, lbl2, badge });
            pnl.Resize += (s, e) => badge.Left = pnl.Width - badge.Width;
            return pnl;
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

            foreach (var card in new[] { cardBranch, cardPermissions, cardAlerts, cardBackups })
            {
                card.FillColor = ThemeManager.CardBackground;
                card.BorderColor = ThemeManager.TextBoxBorder;
                
                foreach (Control c in card.Controls)
                {
                    if (c is Guna2Panel header && header.Height == 50)
                    {
                        header.CustomBorderColor = ThemeManager.TextBoxBorder;
                        foreach (Control hc in header.Controls)
                        {
                            if (hc is Label l && l.Name == "CardTitle") l.ForeColor = ThemeManager.TextPrimary;
                            else if (hc is Label) hc.ForeColor = Color.FromArgb(0, 36, 64); // Icons
                        }
                    }
                    else if (c is Label l)
                    {
                        l.ForeColor = (l.Font.Bold && l.Text.ToUpper() != l.Text) ? ThemeManager.TextPrimary : ThemeManager.TextSecondary;
                    }
                    else if (c is Guna2Panel group)
                    {
                        // Input Groups / Toggle Rows / Logs / Status
                        if (group.BorderThickness > 0)
                        {
                            group.BorderColor = ThemeManager.TextBoxBorder;
                            group.FillColor = ThemeManager.Background;
                        }
                        if (group.CustomBorderThickness.Bottom > 0)
                        {
                            group.CustomBorderColor = ThemeManager.TextBoxBorder;
                        }

                        foreach (Control gc in group.Controls)
                        {
                            if (gc is Label gl)
                            {
                                if (gl.Name == "ToggleTitle" || gl.Name == "LogTitle" || gl.Text == "Healthy" || (gl.Font.Bold && gl.Text.ToUpper() != gl.Text)) 
                                    gl.ForeColor = ThemeManager.TextPrimary;
                                else 
                                    gl.ForeColor = ThemeManager.TextSecondary;
                            }
                            else if (gc is Guna2TextBox txt)
                            {
                                txt.FillColor = txt.ReadOnly ? ThemeManager.Background : ThemeManager.TextBoxBackground;
                                txt.ForeColor = ThemeManager.TextPrimary;
                                txt.BorderColor = ThemeManager.TextBoxBorder;
                            }
                            else if (gc is Guna2ComboBox cb)
                            {
                                cb.FillColor = cb.Enabled ? ThemeManager.TextBoxBackground : ThemeManager.Background;
                                cb.ForeColor = ThemeManager.TextPrimary;
                                cb.BorderColor = ThemeManager.TextBoxBorder;
                            }
                            else if (gc is Guna2Panel badge && badge.Name == "BadgePanel")
                            {
                                badge.FillColor = Color.FromArgb(40, 0, 186, 97);
                                if (badge.Controls[0] is Label bl) bl.ForeColor = Color.FromArgb(0, 186, 97);
                            }
                        }
                    }
                    else if (c is Guna2Button btn)
                    {
                        if (btn.Text.Contains("Manual"))
                        {
                            btn.FillColor = ThemeManager.Background;
                            btn.ForeColor = ThemeManager.TextPrimary;
                            btn.BorderColor = ThemeManager.TextBoxBorder;
                        }
                        else
                        {
                            btn.FillColor = ThemeManager.ButtonFill;
                            btn.ForeColor = ThemeManager.ButtonText;
                        }
                    }
                    else if (c is LinkLabel ll)
                    {
                        ll.LinkColor = Color.FromArgb(0, 36, 64);
                    }
                }
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
