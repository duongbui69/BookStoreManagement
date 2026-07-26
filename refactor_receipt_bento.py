import os
import re

files = [
    r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\UserControls\PurchaseReceiptControl.cs',
    r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\UserControls\ExportReceiptControl.cs'
]

stat_card_method = """
        private Guna2Panel CreateStatCard(string title, out Label lblValue)
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,20,0), BorderRadius = 12 };
            
            // Icon
            Color color = title.Contains("PHIẾU") ? Color.FromArgb(41, 128, 185) : 
                          (title.Contains("GIÁ TRỊ") ? Color.FromArgb(0, 186, 97) : Color.FromArgb(243, 156, 18));
            string iconText = title.Contains("PHIẾU") ? "🏷️" : (title.Contains("GIÁ TRỊ") ? "💰" : "⏳");
            
            var pnlIcon = new Guna2Panel { Size = new Size(48, 48), Location = new Point(20, 26), BorderRadius = 24, FillColor = Color.FromArgb(30, color) };
            Label lblIcon = new Label { Text = iconText, Font = new Font("Segoe UI Emoji", 16F), AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            pnlIcon.Controls.Add(lblIcon);
            
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(80, 26) };
            lblTitle.Tag = "CardTitle";
            lblValue = new Label { Text = "0", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(78, 45), ForeColor = color };
            lblValue.Tag = "CardValue";

            pnl.Controls.Add(pnlIcon);
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);
            return pnl;
        }
"""

apply_theme_to_card_method = """
        private void ApplyThemeToCard(Guna2Panel card)
        {
            if (card == null) return;
            card.BackColor = ThemeManager.CardBackground;
            card.CustomBorderColor = ThemeManager.TextBoxBorder;
            card.FillColor = ThemeManager.CardBackground;
            foreach (Control c in card.Controls)
            {
                if (c.Tag?.ToString() == "CardTitle") c.ForeColor = ThemeManager.TextSecondary;
            }
        }
"""

for filepath in files:
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Replace old CreateStatCard
    old_method_pattern = r'private Guna2Panel CreateStatCard.*?return card;\s*}'
    content = re.sub(old_method_pattern, stat_card_method.strip(), content, flags=re.DOTALL)

    # Add ApplyThemeToCard before ApplyThemeToGrid
    if 'private void ApplyThemeToCard' not in content:
        content = content.replace('private void ApplyThemeToGrid()', apply_theme_to_card_method.strip() + '\n\n        private void ApplyThemeToGrid()')

    # Update ApplyTheme to use ApplyThemeToCard instead of the messy loops
    old_apply_theme_loops = r'cardTotalReceipts\.BackColor = ThemeManager\.CardBackground;.*?foreach \(Control c in cardPending\.Controls\) if \(c is Label l\) l\.ForeColor = ThemeManager\.TextPrimary;'
    
    new_apply_theme_calls = """
            ApplyThemeToCard(cardTotalReceipts);
            ApplyThemeToCard(cardTotalValue);
            ApplyThemeToCard(cardPending);
"""
    content = re.sub(old_apply_theme_loops, new_apply_theme_calls.strip(), content, flags=re.DOTALL)

    # Write back
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
