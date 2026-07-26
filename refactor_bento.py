import os
import re

root_dir = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\UserControls'

files_to_process = [
    ('CategoryControl.cs', 'TỔNG SỐ DANH MỤC'),
    ('AuthorControl.cs', 'TỔNG SỐ TÁC GIẢ'),
    ('PublisherControl.cs', 'TỔNG SỐ NXB'),
    ('SupplierControl.cs', 'TỔNG NHÀ CUNG CẤP')
]

for filename, title1 in files_to_process:
    filepath = os.path.join(root_dir, filename)
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. Add class level variables
    if 'private TableLayoutPanel tlpStats;' not in content:
        content = re.sub(
            r'(private Label lblSubtitle;)',
            r'\1\n        private TableLayoutPanel tlpStats;\n        private Guna2Panel card1, card2, card3;',
            content
        )

    # 2. Add layout code in InitializeUI
    layout_code = f"""
            tlpStats = new TableLayoutPanel 
            {{ 
                Dock = DockStyle.Top, Height = 100, 
                ColumnCount = 3, RowCount = 1,
                Margin = new Padding(0,0,0,gutter)
            }};
            for(int i=0; i<3; i++) tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            
            card1 = CreateStatCard("{title1}", "0", "category", Color.FromArgb(41, 128, 185)); // Primary
            card2 = CreateStatCard("ĐANG HOẠT ĐỘNG", "0", "active", Color.FromArgb(0, 186, 97)); // Tertiary
            card3 = CreateStatCard("ĐÃ KHÓA", "0", "locked", Color.FromArgb(186, 26, 26)); // Error
            
            tlpStats.Controls.Add(card1, 0, 0);
            tlpStats.Controls.Add(card2, 1, 0);
            tlpStats.Controls.Add(card3, 2, 0);
"""
    if 'tlpStats = new TableLayoutPanel' not in content:
        # Insert after pnlPageHeader... btnAdd }); or pnlPageHeader.Resize += ... };
        content = re.sub(
            r'(pnlPageHeader\.Resize \+=.*?;\s*};\s*)',
            r'\1\n' + layout_code + '\n',
            content,
            flags=re.DOTALL
        )

    # 3. Add to pnlContent and BringToFront
    if 'Panel spacer3' not in content:
        content = re.sub(
            r'(Panel spacer2 = new Panel \{ Dock = DockStyle\.Top, Height = gutter, BackColor = Color\.Transparent \};)',
            r'\1\n            Panel spacer3 = new Panel { Dock = DockStyle.Top, Height = gutter, BackColor = Color.Transparent };',
            content
        )
    
    if 'pnlContent.Controls.Add(tlpStats);' not in content:
        content = content.replace(
            'pnlContent.Controls.Add(pnlFilters);',
            'pnlContent.Controls.Add(pnlFilters);\n            pnlContent.Controls.Add(spacer3);\n            pnlContent.Controls.Add(tlpStats);'
        )
    
    if 'tlpStats.BringToFront();' not in content:
        content = content.replace(
            'pnlFilters.BringToFront();',
            'tlpStats.BringToFront();\n            spacer3.BringToFront();\n            pnlFilters.BringToFront();'
        )

    # 4. Add CreateStatCard method
    if 'private Guna2Panel CreateStatCard' not in content:
        stat_card_method = """
        private Guna2Panel CreateStatCard(string title, string value, string iconText, Color color)
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,20,0), BorderRadius = 12 };
            
            // Icon
            var pnlIcon = new Guna2Panel { Size = new Size(48, 48), Location = new Point(20, 26), BorderRadius = 24, FillColor = Color.FromArgb(30, color) };
            Label lblIcon = new Label { Text = iconText == "category" ? "📑" : (iconText == "active" ? "✨" : "⚠️"), Font = new Font("Segoe UI Emoji", 16F), AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            pnlIcon.Controls.Add(lblIcon);
            
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(80, 26) };
            lblTitle.Tag = "CardTitle";
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(78, 45), ForeColor = color };
            lblValue.Tag = "CardValue";

            pnl.Controls.Add(pnlIcon);
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);
            return pnl;
        }
"""
        # Insert before ApplyTheme
        content = re.sub(
            r'(private void ApplyTheme\(\))',
            stat_card_method + r'\1',
            content
        )

    # 5. ApplyThemeToCard
    if 'ApplyThemeToCard(card1);' not in content:
        apply_theme_calls = """
            ApplyThemeToCard(card1);
            ApplyThemeToCard(card2);
            ApplyThemeToCard(card3);
"""
        content = content.replace(
            'lblSubtitle.ForeColor = ThemeManager.TextSecondary;',
            'lblSubtitle.ForeColor = ThemeManager.TextSecondary;' + apply_theme_calls
        )

    if 'private void ApplyThemeToCard' not in content:
        apply_theme_to_card_method = """
        private void ApplyThemeToCard(Guna2Panel card)
        {
            card.BackColor = ThemeManager.CardBackground;
            card.CustomBorderColor = ThemeManager.TextBoxBorder;
            card.FillColor = ThemeManager.CardBackground;
            foreach (Control c in card.Controls)
            {
                if (c.Tag?.ToString() == "CardTitle") c.ForeColor = ThemeManager.TextSecondary;
            }
        }
"""
        content = re.sub(
            r'(protected override void Dispose\(bool disposing\))',
            apply_theme_to_card_method + r'\n        \1',
            content
        )

    # 6. LoadData stats calculation
    if filename == 'CategoryControl.cs':
        calc_logic = """
                var all = _categoryService.GetAll();
                int total = all.Count;
                int active = 0, locked = 0;
                foreach(var x in all) { if(x.IsActive) active++; else locked++; }
                foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = total.ToString("N0");
                foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = active.ToString("N0");
                foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = locked.ToString("N0");
"""
    elif filename == 'AuthorControl.cs':
        calc_logic = """
                var all = _authorService.GetAll();
                int total = all.Count;
                int active = 0, locked = 0;
                foreach(var x in all) { if(x.IsActive) active++; else locked++; }
                foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = total.ToString("N0");
                foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = active.ToString("N0");
                foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = locked.ToString("N0");
"""
    elif filename == 'PublisherControl.cs':
        calc_logic = """
                var all = _publisherService.GetAll();
                int total = all.Count;
                int active = 0, locked = 0;
                foreach(var x in all) { if(x.IsActive) active++; else locked++; }
                foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = total.ToString("N0");
                foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = active.ToString("N0");
                foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = locked.ToString("N0");
"""
    elif filename == 'SupplierControl.cs':
        calc_logic = """
                var all = _supplierService.GetAll();
                int total = all.Count;
                int active = 0, locked = 0;
                foreach(var x in all) { if(x.IsActive) active++; else locked++; }
                foreach(Control c in card1.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = total.ToString("N0");
                foreach(Control c in card2.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = active.ToString("N0");
                foreach(Control c in card3.Controls) if (c.Tag?.ToString() == "CardValue") c.Text = locked.ToString("N0");
"""

    if 'foreach(Control c in card1.Controls)' not in content:
        content = re.sub(
            r'(dgv[a-zA-Z]+\.DataSource = null;)',
            calc_logic + r'\n                \1',
            content
        )

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

print("Updated 4 UI controls successfully.")
