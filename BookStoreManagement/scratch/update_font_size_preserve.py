import re

file_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/Themes/ThemeManager.cs'

with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

new_apply_typography = """        public static void ApplyTypography(System.Windows.Forms.Control parent)
        {
            if (parent == null) return;

            foreach (System.Windows.Forms.Control c in parent.Controls)
            {
                if (c.Font != null && c.Font.Name != "Segoe UI")
                {
                    c.Font = new System.Drawing.Font("Segoe UI", c.Font.Size, c.Font.Style);
                }
                
                if (c.HasChildren)
                {
                    ApplyTypography(c);
                }
            }
        }"""

text = re.sub(r'public static void ApplyTypography\(System\.Windows\.Forms\.Control parent\)\s*\{.*?\}(?=\s*public static void ApplyDataGridViewStyle)', new_apply_typography + '\n\n', text, flags=re.DOTALL)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)
