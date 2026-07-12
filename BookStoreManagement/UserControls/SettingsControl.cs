using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BookStoreManagement.Themes;

namespace BookStoreManagement.UserControls
{
    public partial class SettingsControl : UserControl
    {
        private Panel pnlHeader;
        private TextBox txtSearch;
        private Label lblTitle;
        private Label lblSubTitle;

        private Panel pnlLeftMenu;
        private Panel pnlRightContent;

        private System.Collections.Generic.List<Button> menuButtons = new System.Collections.Generic.List<Button>();
        private System.Collections.Generic.List<Panel> sectionCards = new System.Collections.Generic.List<Panel>();
        private bool isScrollingProgrammatically = false;

        public SettingsControl()
        {
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        }

        private void InitializeUI()
        {
            this.BackColor = ThemeManager.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(30);

            // Top Header
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
            txtSearch = new TextBox { Width = 400, Font = new Font("Segoe UI", 11F), PlaceholderText = "Search system logs or settings...", Location = new Point(0, 10) };
            pnlHeader.Controls.Add(txtSearch);
            this.Controls.Add(pnlHeader);

            // Container for Two Columns
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            this.Controls.Add(pnlMain);
            pnlMain.BringToFront();

            // Left Sidebar
            pnlLeftMenu = new Panel { Dock = DockStyle.Left, Width = 280, BackColor = ThemeManager.CardBackground };
            lblTitle = new Label { Text = "System Settings", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            lblSubTitle = new Label { Text = "Manage global configurations", Font = new Font("Segoe UI", 9F), ForeColor = ThemeManager.TextSecondary, AutoSize = true, Location = new Point(22, 50) };
            pnlLeftMenu.Controls.Add(lblTitle);
            pnlLeftMenu.Controls.Add(lblSubTitle);

            // Menu Items
            string[] menuItems = { "Store Information", "Financial Configuration", "System Preferences", "Security & Access", "Integrations", "Audit Logs" };
            int y = 90;
            for (int i = 0; i < menuItems.Length; i++)
            {
                Button btnMenu = new Button
                {
                    Text = "  " + menuItems[i],
                    Font = new Font("Segoe UI", 10F, i == 0 ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = i == 0 ? ThemeManager.TextPrimary : ThemeManager.TextSecondary,
                    BackColor = i == 0 ? ThemeManager.HoverColor : Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Width = 240,
                    Height = 45,
                    Location = new Point(20, y),
                    Cursor = Cursors.Hand,
                    Tag = i
                };
                btnMenu.FlatAppearance.BorderSize = 0;
                btnMenu.Click += BtnMenu_Click;
                pnlLeftMenu.Controls.Add(btnMenu);
                menuButtons.Add(btnMenu);
                y += 50;
            }

            // Warning Box
            Panel pnlWarning = new Panel { Width = 240, Height = 80, Location = new Point(20, pnlLeftMenu.Height - 100), Anchor = AnchorStyles.Bottom | AnchorStyles.Left, BackColor = ThemeManager.HoverColor };
            Label lblWarning = new Label { Text = "Changes here affect all\nbookstore branches globally.\nUse caution.", Font = new Font("Segoe UI", 8.5F), ForeColor = ThemeManager.Sidebar, Location = new Point(50, 15), AutoSize = true };
            pnlWarning.Controls.Add(lblWarning);
            pnlLeftMenu.Controls.Add(pnlWarning);

            // Right Content Area (Scrollable)
            pnlRightContent = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(30, 0, 0, 0) };
            pnlRightContent.Scroll += PnlRightContent_Scroll;
            pnlRightContent.MouseWheel += PnlRightContent_Scroll;
            
            // Add Cards to Right Content
            int currentY = 0;
            sectionCards.Add(CreateCard1(ref currentY));
            sectionCards.Add(CreateCard2(ref currentY));
            sectionCards.Add(CreateCard3(ref currentY));
            sectionCards.Add(CreatePlaceholderCard("Security & Access", ref currentY));
            sectionCards.Add(CreatePlaceholderCard("Integrations", ref currentY));
            sectionCards.Add(CreatePlaceholderCard("Audit Logs", ref currentY));

            // Add extra space at bottom so last item can scroll to top
            currentY += 400;
            Panel pnlSpacer = new Panel { Location = new Point(30, currentY), Width = 10, Height = 10 };
            pnlRightContent.Controls.Add(pnlSpacer);

            foreach(var card in sectionCards) {
                pnlRightContent.Controls.Add(card);
            }

            pnlMain.Controls.Add(pnlRightContent);
            pnlMain.Controls.Add(pnlLeftMenu);

            ApplyTheme();
        }

        private Panel CreateCard1(ref int y)
        {
            Panel card = new Panel { Width = 800, Height = 280, Location = new Point(30, y), BackColor = ThemeManager.CardBackground };
            y += 310;
            
            // Title
            Label lbl = new Label { Text = "Store Information", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            card.Controls.Add(lbl);

            // Fields
            card.Controls.Add(CreateInputField("Store Display Name", "Bibliotech Central Plaza", 20, 70, 360));
            card.Controls.Add(CreateInputField("Business Registration ID", "REG-9920-X1", 400, 70, 360));
            
            card.Controls.Add(CreateInputField("Operational Address", "122 Inventory Boulevard, Book District, Metropolis 90210", 20, 140, 740));
            
            card.Controls.Add(CreateInputField("Official Website", "bibliotech-internal.com", 20, 210, 360, "https://"));
            card.Controls.Add(CreateInputField("Store Contact Email", "admin@bibliotech.com", 400, 210, 360));

            return card;
        }

        private Panel CreateCard2(ref int y)
        {
            Panel card = new Panel { Width = 800, Height = 220, Location = new Point(30, y), BackColor = ThemeManager.CardBackground };
            y += 250;

            Label lbl = new Label { Text = "Financial Configuration", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            card.Controls.Add(lbl);

            // Dropdowns
            card.Controls.Add(CreateDropdownField("Base Currency", new[] { "USD ($) - United States Dollar", "VND (₫) - Vietnam Dong" }, 20, 70, 360));
            card.Controls.Add(CreateDropdownField("Tax Profile", new[] { "Standard Retail Tax (8.5%)", "VAT (10%)" }, 20, 140, 360));

            // Checkboxes
            Panel pnlReporting = new Panel { Width = 360, Height = 140, Location = new Point(400, 60), BackColor = Color.Transparent };
            pnlReporting.Paint += (s, e) => {
                using var p = new Pen(ThemeManager.TextBoxBorder, 1) { DashStyle = DashStyle.Dash };
                e.Graphics.DrawRectangle(p, 0, 0, pnlReporting.Width - 1, pnlReporting.Height - 1);
            };
            Label lblRep = new Label { Text = "Automatic Reporting", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
            pnlReporting.Controls.Add(lblRep);
            
            CheckBox chk1 = new CheckBox { Text = "Generate Daily P&L Statement", Checked = true, Location = new Point(15, 45), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            CheckBox chk2 = new CheckBox { Text = "Email EOD reports to Treasury", Checked = true, Location = new Point(15, 75), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            CheckBox chk3 = new CheckBox { Text = "Sync with QuickBooks Cloud", Checked = false, Location = new Point(15, 105), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            
            pnlReporting.Controls.Add(chk1);
            pnlReporting.Controls.Add(chk2);
            pnlReporting.Controls.Add(chk3);
            card.Controls.Add(pnlReporting);

            return card;
        }

        private Panel CreateCard3(ref int y)
        {
            Panel card = new Panel { Width = 800, Height = 250, Location = new Point(30, y), BackColor = ThemeManager.CardBackground };
            y += 280;

            Label lbl = new Label { Text = "System Preferences", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            card.Controls.Add(lbl);

            Label l1 = new Label { Text = "INVENTORY LOGIC", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.TextSecondary, Location = new Point(20, 70), AutoSize = true };
            card.Controls.Add(l1);
            card.Controls.Add(CreateRadioBlock("FIFO", "First-in, First-out", true, 20, 95, 240));
            card.Controls.Add(CreateRadioBlock("LIFO", "Last-in, First-out", false, 20, 165, 240));

            Label l2 = new Label { Text = "UI SCALING", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.TextSecondary, Location = new Point(280, 70), AutoSize = true };
            card.Controls.Add(l2);
            card.Controls.Add(CreateRadioBlock("Standard", "14px Base Typography", true, 280, 95, 240));
            card.Controls.Add(CreateRadioBlock("Compact (Guna2)", "Smaller spacing", false, 280, 165, 240));

            Label l3 = new Label { Text = "SECURITY MODE", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.TextSecondary, Location = new Point(540, 70), AutoSize = true };
            card.Controls.Add(l3);
            card.Controls.Add(CreateRadioBlock("Standard", "Default system rules", true, 540, 95, 240));
            card.Controls.Add(CreateRadioBlock("Lockdown", "High security mode", false, 540, 165, 240, true));

            return card;
        }

        private Panel CreatePlaceholderCard(string title, ref int y)
        {
            Panel card = new Panel { Width = 800, Height = 150, Location = new Point(30, y), BackColor = ThemeManager.CardBackground };
            y += 180;
            Label lbl = new Label { Text = title, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            card.Controls.Add(lbl);
            Label desc = new Label { Text = "Configuration options for " + title + " will appear here.", Font = new Font("Segoe UI", 10F), ForeColor = ThemeManager.TextSecondary, Location = new Point(20, 60), AutoSize = true };
            card.Controls.Add(desc);
            return card;
        }

        private void BtnMenu_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is int index)
            {
                if (index >= 0 && index < sectionCards.Count)
                {
                    isScrollingProgrammatically = true;
                    UpdateActiveMenu(index);
                    
                    // Scroll to card
                    pnlRightContent.AutoScrollPosition = new Point(
                        Math.Abs(pnlRightContent.AutoScrollPosition.X), 
                        sectionCards[index].Location.Y
                    );
                    
                    // Delay to reset programmatic scroll flag so user scroll takes over again
                    var t = new System.Windows.Forms.Timer { Interval = 100 };
                    t.Tick += (s, ev) => { isScrollingProgrammatically = false; t.Stop(); };
                    t.Start();
                }
            }
        }

        private void PnlRightContent_Scroll(object sender, EventArgs e)
        {
            if (isScrollingProgrammatically) return;

            int scrollY = Math.Abs(pnlRightContent.AutoScrollPosition.Y);
            
            // Find which card is currently at the top
            int activeIndex = 0;
            for (int i = 0; i < sectionCards.Count; i++)
            {
                // If scroll position is past the top of this card (with a 50px buffer)
                if (scrollY >= sectionCards[i].Location.Y - 50)
                {
                    activeIndex = i;
                }
            }
            
            UpdateActiveMenu(activeIndex);
        }

        private void UpdateActiveMenu(int index)
        {
            for (int i = 0; i < menuButtons.Count; i++)
            {
                if (i == index)
                {
                    menuButtons[i].Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    menuButtons[i].ForeColor = ThemeManager.TextPrimary;
                    menuButtons[i].BackColor = ThemeManager.HoverColor;
                }
                else
                {
                    menuButtons[i].Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                    menuButtons[i].ForeColor = ThemeManager.TextSecondary;
                    menuButtons[i].BackColor = Color.Transparent;
                }
            }
        }

        private Panel CreateInputField(string label, string value, int x, int y, int width, string prefix = "")
        {
            Panel pnl = new Panel { Location = new Point(x, y), Width = width, Height = 65, BackColor = Color.Transparent };
            Label lbl = new Label { Text = label, Font = new Font("Segoe UI", 9F), ForeColor = ThemeManager.TextSecondary, Location = new Point(0, 0), AutoSize = true };
            pnl.Controls.Add(lbl);

            TextBox txt = new TextBox { Text = value, Font = new Font("Segoe UI", 10F), Location = new Point(0, 25), Width = width };
            if (!string.IsNullOrEmpty(prefix))
            {
                Label lblPrefix = new Label { Text = prefix, Font = new Font("Segoe UI", 10F), BackColor = ThemeManager.HoverColor, TextAlign = ContentAlignment.MiddleCenter, Width = 60, Height = 25, Location = new Point(0, 25) };
                txt.Location = new Point(60, 25);
                txt.Width = width - 60;
                pnl.Controls.Add(lblPrefix);
            }
            pnl.Controls.Add(txt);

            return pnl;
        }

        private Panel CreateDropdownField(string label, string[] items, int x, int y, int width)
        {
            Panel pnl = new Panel { Location = new Point(x, y), Width = width, Height = 65, BackColor = Color.Transparent };
            Label lbl = new Label { Text = label, Font = new Font("Segoe UI", 9F), ForeColor = ThemeManager.TextSecondary, Location = new Point(0, 0), AutoSize = true };
            pnl.Controls.Add(lbl);

            ComboBox cb = new ComboBox { Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(0, 25), Width = width };
            cb.Items.AddRange(items);
            if (items.Length > 0) cb.SelectedIndex = 0;
            pnl.Controls.Add(cb);

            return pnl;
        }

        private Panel CreateRadioBlock(string title, string desc, bool isChecked, int x, int y, int width, bool isAlert = false)
        {
            Panel pnl = new Panel { Location = new Point(x, y), Width = width, Height = 60, BackColor = Color.Transparent };
            pnl.Paint += (s, e) => {
                using var p = new Pen(isAlert ? Color.FromArgb(231, 76, 60) : ThemeManager.TextBoxBorder, 1);
                e.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            RadioButton rb = new RadioButton { Checked = isChecked, Location = new Point(15, 20), AutoSize = true };
            pnl.Controls.Add(rb);

            Label lbl1 = new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = isAlert ? Color.FromArgb(231, 76, 60) : ThemeManager.TextPrimary, Location = new Point(40, 12), AutoSize = true };
            Label lbl2 = new Label { Text = desc, Font = new Font("Segoe UI", 8F), ForeColor = ThemeManager.TextSecondary, Location = new Point(40, 32), AutoSize = true };
            
            pnl.Controls.Add(lbl1);
            pnl.Controls.Add(lbl2);

            return pnl;
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlLeftMenu.BackColor = ThemeManager.Background; // Actually left menu has no distinct background in the image, it's just divided by a line. Let's make it background color and draw line
            pnlLeftMenu.Paint += (s, e) => {
                using var p = new Pen(ThemeManager.TextBoxBorder, 1);
                e.Graphics.DrawLine(p, pnlLeftMenu.Width - 1, 0, pnlLeftMenu.Width - 1, pnlLeftMenu.Height);
            };
            
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            txtSearch.BackColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;

            foreach (Control c in pnlRightContent.Controls)
            {
                if (c is Panel card)
                {
                    card.BackColor = ThemeManager.CardBackground;
                    card.Paint += (s, e) => {
                        using var p = new Pen(ThemeManager.TextBoxBorder, 1);
                        e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                    };
                    
                    foreach (Control child in card.Controls)
                    {
                        if (child is Label l) l.ForeColor = ThemeManager.TextPrimary;
                    }
                }
            }
            
            // Re-apply to textboxes, combos, etc.
            UpdateChildThemes(this);
        }

        private void UpdateChildThemes(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox txt)
                {
                    txt.BackColor = ThemeManager.TextBoxBackground;
                    txt.ForeColor = ThemeManager.TextPrimary;
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = ThemeManager.TextBoxBackground;
                    cb.ForeColor = ThemeManager.TextPrimary;
                }
                else if (c is CheckBox chk)
                {
                    chk.ForeColor = ThemeManager.TextPrimary;
                }
                if (c.HasChildren) UpdateChildThemes(c);
            }
        }
    }
}
