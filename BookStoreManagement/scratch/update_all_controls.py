import os
import re

base_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement'

def patch_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        text = f.read()

    match = re.search(r'private void ApplyTheme\(\)\s*\{', text)
    if not match:
        return

    if 'ThemeManager.ApplyTypography(this);' in text:
        return

    idx = match.end()
    patched_text = text[:idx] + '\n            BookStoreManagement.Themes.ThemeManager.ApplyTypography(this);' + text[idx:]

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(patched_text)
    print(f'Patched {filepath}')

uc_path = os.path.join(base_path, 'UserControls')
for root, _, files in os.walk(uc_path):
    for file in files:
        if file.endswith('.cs') and not file.endswith('.Designer.cs'):
            patch_file(os.path.join(root, file))

patch_file(os.path.join(base_path, 'Forms', 'MainForm.cs'))
