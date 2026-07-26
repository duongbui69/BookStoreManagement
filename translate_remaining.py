import os
import re

root_dir = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement'

replacements = {
    "0 VND": "0 VNĐ",
    "TOTAL REFUND:": "TỔNG HOÀN:",
    "TOTAL:": "TỔNG CỘNG:",
    "This book is out of stock!": "Sách này đã hết hàng!",
    "LOGIN": "ĐĂNG NHẬP",
    "Dashboard": "Tổng quan",
    "Settings": "Cài đặt",
    "HR": "Nhân sự",
    "Reports": "Báo cáo",
    "Catalog": "Danh mục",
    "Inventory": "Kho hàng",
    "Logout": "Đăng xuất",
    "Orders": "Đơn hàng"
}

changed_files_count = 0

for subdir, dirs, files in os.walk(root_dir):
    if 'obj' in subdir or 'bin' in subdir or 'Migrations' in subdir or 'Properties' in subdir:
        continue
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(subdir, file)
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            
            original = content
            
            # 1. Replace exact matches for the simple ones
            content = content.replace('"0 VND"', '"0 VNĐ"')
            content = content.replace('"TOTAL:"', '"TỔNG CỘNG:"')
            content = content.replace('"This book is out of stock!"', '"Sách này đã hết hàng!"')
            
            # 2. Replace substrings inside Text = "..." for the sidebar buttons
            # Example: btnDashboard.Text = "   Dashboard";
            # We use regex to carefully replace just the English word at the end of the string.
            for k, v in replacements.items():
                if k in ["Dashboard", "Settings", "HR", "Reports", "Catalog", "Inventory", "Logout", "Orders", "LOGIN"]:
                    # Match: Text = "(any icons or spaces)Keyword"
                    # We capture the prefix in group 1
                    pattern = r'(Text\s*=\s*\"[^\"]*?\b)' + k + r'(\b\")'
                    content = re.sub(pattern, r'\g<1>' + v + r'\g<2>', content)

            if content != original:
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(content)
                changed_files_count += 1

print(f"Updated {changed_files_count} files for remaining text.")
