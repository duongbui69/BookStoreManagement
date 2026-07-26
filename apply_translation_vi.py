import os
import json

# Read the Vi -> En dictionary
with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/translate_dict.json', 'r', encoding='utf-8') as f:
    vi_to_en = json.load(f)

# Reverse it to En -> Vi
en_to_vi = {v: k for k, v in vi_to_en.items()}

# Add additional translations specifically for the bento cards and UI
en_to_vi.update({
    "Total categories": "TỔNG SỐ DANH MỤC",
    "Total authors": "TỔNG SỐ TÁC GIẢ",
    "Total publishers": "TỔNG SỐ NXB",
    "Total suppliers": "TỔNG NHÀ CUNG CẤP",
    "ACTIVE": "ĐANG HOẠT ĐỘNG",
    "LOCKED": "ĐÃ KHÓA"
})

# Add missing translations for MainForm that might have different casing
en_to_vi.update({
    "Overview": "Tổng quan",
    "Book": "Sách",
    "Book Management": "Quản lý Sách",
    "Stock card": "Thẻ kho",
    "Import": "Nhập kho",
    "Export": "Xuất kho",
    "Receipt type": "Loại phiếu",
    "Inventory management": "Quản lý Kho",
    "Customer Management": "Quản lý Khách hàng",
    "Order Management": "Quản lý Đơn hàng",
    "Refund Management": "Quản lý Đổi/Trả",
    "Author management": "Quản lý Tác giả",
    "Publisher management": "Quản lý NXB",
    "Supplier management": "Quản lý Nhà cung cấp",
    "Return": "Trả hàng",
    "Lookup inventory": "Tra cứu kho"
})

# Sort dictionary by length of keys descending to prevent partial replacements
sorted_keys = sorted(en_to_vi.keys(), key=len, reverse=True)

root_dir = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement'

changed_files_count = 0

for subdir, dirs, files in os.walk(root_dir):
    if 'obj' in subdir or 'bin' in subdir or 'Migrations' in subdir:
        continue
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(subdir, file)
            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                original_content = content
                for k in sorted_keys:
                    v = en_to_vi[k]
                    # Replace exact double-quoted strings or substrings as needed
                    # Since we want to be safe, replace `"English"` with `"Vietnamese"`
                    content = content.replace(f'"{k}"', f'"{v}"')
                
                if content != original_content:
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(content)
                    changed_files_count += 1
                    
            except Exception as e:
                print(f"Error processing {filepath}: {e}")

print(f"Translation (EN -> VI) applied to {changed_files_count} files.")
