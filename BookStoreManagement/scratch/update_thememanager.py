import re

file_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/Themes/ThemeManager.cs'

with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

# Replace colors
text = text.replace('public static Color LightTextPrimary = Color.FromArgb(30, 30, 30);', 'public static Color LightTextPrimary = Color.FromArgb(20, 20, 20);')
text = text.replace('public static Color LightTextSecondary = Color.Gray;', 'public static Color LightTextSecondary = Color.FromArgb(100, 100, 100);')
text = text.replace('public static Color DarkTextPrimary = Color.White;', 'public static Color DarkTextPrimary = Color.FromArgb(245, 245, 250);')
text = text.replace('public static Color DarkTextSecondary = Color.FromArgb(160, 160, 165);', 'public static Color DarkTextSecondary = Color.FromArgb(170, 170, 175);')

font_code = """        public static Font FontTitle => new Font("Inter", 18F, FontStyle.Bold);
        public static Font FontSubtitle => new Font("Inter", 12F, FontStyle.Regular);
        public static Font FontHeader => new Font("Inter", 10F, FontStyle.Bold);
        public static Font FontBody => new Font("Inter", 10F, FontStyle.Regular);
        public static Font FontButton => new Font("Inter", 9F, FontStyle.Bold);
        public static Font FontSmall => new Font("Inter", 8.5F, FontStyle.Regular);

        public static void ApplyTypography(System.Windows.Forms.Control parent)
        {
            if (parent == null) return;

            foreach (System.Windows.Forms.Control c in parent.Controls)
            {
                if (c is System.Windows.Forms.Label lbl)
                {
                    if (lbl.Name.StartsWith("lblTitle") || (lbl.Tag != null && lbl.Tag.ToString() == "Title"))
                    {
                        lbl.Font = FontTitle;
                    }
                    else if (lbl.Name.StartsWith("lblSubTitle") || (lbl.Tag != null && lbl.Tag.ToString() == "Subtitle"))
                    {
                        lbl.Font = FontSubtitle;
                    }
                    else
                    {
                        if (lbl.Font.Size >= 14) lbl.Font = FontTitle;
                        else if (lbl.Font.Size >= 11) lbl.Font = FontSubtitle;
                        else if (lbl.Font.Bold) lbl.Font = FontHeader;
                        else lbl.Font = FontBody;
                    }
                }
                else if (c is Guna.UI2.WinForms.Guna2Button btn)
                {
                    btn.Font = FontButton;
                }
                else if (c is System.Windows.Forms.Button sysBtn)
                {
                    sysBtn.Font = FontButton;
                }
                else if (c is Guna.UI2.WinForms.Guna2TextBox tb)
                {
                    tb.Font = FontBody;
                }
                else if (c is Guna.UI2.WinForms.Guna2ComboBox cb)
                {
                    cb.Font = FontBody;
                }
                
                if (c.HasChildren)
                {
                    ApplyTypography(c);
                }
            }
        }
"""

if "ApplyTypography" not in text:
    text = text.replace('public static void ApplyDataGridViewStyle(System.Windows.Forms.DataGridView dgv)', font_code + '\n        public static void ApplyDataGridViewStyle(System.Windows.Forms.DataGridView dgv)')

# Fix DataGridView fonts
text = re.sub(r'new System\.Drawing\.Font\("Segoe UI", \d+F, System\.Drawing\.FontStyle\.Bold\)', 'FontHeader', text)
text = re.sub(r'new System\.Drawing\.Font\("Segoe UI", \d+F\)', 'FontBody', text)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)
