import os, glob, re

folder = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls'
files = glob.glob(os.path.join(folder, '*.cs'))

for file in files:
    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()

    # Standardize fonts for subtitles
    content = content.replace('Font("Segoe UI", 13)', 'Font("Segoe UI", 11F)')
    content = content.replace('Font("Segoe UI", 13F)', 'Font("Segoe UI", 11F)')
    content = content.replace('Font("Segoe UI", 10)', 'Font("Segoe UI", 11F)')
    content = content.replace('Font("Segoe UI", 10F)', 'Font("Segoe UI", 11F)')
    
    def replacer(match):
        dgv_name = match.group(1)
        return f"ThemeManager.ApplyDataGridViewStyle({dgv_name});"

    # Replace block of dgv custom styling
    content = re.sub(r'(\w+)\.BackgroundColor\s*=[^;]+;[\s\S]*?(?=\n\s*(?:lbl|this|btn|pnl|\}))', replacer, content)
    
    with open(file, 'w', encoding='utf-8') as f:
        f.write(content)

print("Done")
