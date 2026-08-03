import re

file_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/OrdersControl.cs'

with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

# Fix CustomerName color
text = re.sub(
    r'TextRenderer\.DrawText\(e\.Graphics, name, e\.CellStyle\.Font, textRect, BookStoreManagement\.Themes\.ThemeManager\.TextPrimary, TextFormatFlags\.Left \| TextFormatFlags\.VerticalCenter\);',
    r'bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;\n                Color textColor = isSelected ? e.CellStyle.SelectionForeColor : BookStoreManagement.Themes.ThemeManager.TextPrimary;\n                TextRenderer.DrawText(e.Graphics, name, e.CellStyle.Font, textRect, textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);',
    text
)

# Fix PaymentMethod color
text = re.sub(
    r'DrawIcon\(e\.Graphics, new Rectangle\(iconX, iconY, iconSize, iconSize\), icon, ThemeManager\.TextSecondary\);\s*Rectangle textRect = new Rectangle\(iconX \+ iconSize \+ 5, e\.CellBounds\.Top, e\.CellBounds\.Width - iconSize - 15, e\.CellBounds\.Height\);\s*TextRenderer\.DrawText\(e\.Graphics, payment, e\.CellStyle\.Font, textRect, BookStoreManagement\.Themes\.ThemeManager\.TextPrimary, TextFormatFlags\.Left \| TextFormatFlags\.VerticalCenter\);',
    r'bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;\n                Color iconColor = isSelected ? e.CellStyle.SelectionForeColor : ThemeManager.TextSecondary;\n                Color textColor = isSelected ? e.CellStyle.SelectionForeColor : BookStoreManagement.Themes.ThemeManager.TextPrimary;\n\n                DrawIcon(e.Graphics, new Rectangle(iconX, iconY, iconSize, iconSize), icon, iconColor);\n\n                Rectangle textRect = new Rectangle(iconX + iconSize + 5, e.CellBounds.Top, e.CellBounds.Width - iconSize - 15, e.CellBounds.Height);\n                TextRenderer.DrawText(e.Graphics, payment, e.CellStyle.Font, textRect, textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);',
    text
)

# Fix OrderStatus matching
text = text.replace(
    'if (status.ToLower().Contains("hoàn thành") || status.ToLower() == "hoàn thành")',
    'if (status.ToLower().Contains("hoàn thành") || status.ToLower() == "completed")'
)
text = text.replace(
    'else if (status.ToLower().Contains("hủy") || status.ToLower() == "hủy")',
    'else if (status.ToLower().Contains("hủy") || status.ToLower() == "cancelled" || status.ToLower() == "canceled")'
)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)
