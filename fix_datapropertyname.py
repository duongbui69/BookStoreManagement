import os
import re
import json

with open('translate_dict.json', 'r', encoding='utf-8') as f:
    vi_to_en = json.load(f)

# Some translations might not be an exact match in the dictionary due to case differences or specific UI mappings.
# Let's add common DataPropertyNames that might have been mistranslated.
# The keys should be the incorrectly translated Vietnamese strings, values are the correct C# property names.
fix_dict = {
    "Tiêu đề": "Title",
    "Tác giả": "Author",
    "Tên tác giả": "AuthorName",
    "Danh mục": "Category",
    "Tên danh mục": "CategoryName",
    "Nhà xuất bản": "Publisher",
    "Tên nhà xuất bản": "PublisherName",
    "Nhà cung cấp": "Supplier",
    "Tên nhà cung cấp": "SupplierName",
    "Trạng thái": "Status",
    "Mã nhân viên": "StaffCode",
    "Họ và tên": "FullName",
    "Tên đầy đủ": "FullName",
    "Mã Khách hàng": "CustomerCode",
    "SĐT": "Phone",
    "Điện thoại": "Phone",
    "Địa chỉ": "Address",
    "Điểm": "Points",
    "Email": "Email",
    "Vị trí": "Role",
    "Chi nhánh": "Branch",
    "Tên chi nhánh": "BranchName",
    "Mã chi nhánh": "BranchCode",
    "Người quản lý": "Manager",
    "Lý do": "Reason",
    "Tổng tiền": "TotalAmount",
    "Ngày nhập": "ImportDate",
    "Ngày xuất": "ExportDate",
    "Ngày lập": "CreatedDate",
    "Người lập": "UserName",
    "Người tạo": "CreatedBy",
    "Khách hàng": "CustomerName",
    "Mã sách": "BookCode",
    "Tồn kho": "Stock",
    "Tối thiểu": "MinStock",
    "Tên đăng nhập": "Username",
    "Mật khẩu": "Password"
}

# Also try to use vi_to_en as fallback
for vi, en in vi_to_en.items():
    if vi not in fix_dict:
        # Strip spaces and convert to PascalCase as a best guess for property name if needed, 
        # but usually it's a direct match. We'll just map exact string for now.
        fix_dict[vi] = en

folders_to_check = [
    r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\UserControls',
    r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Forms'
]

pattern = re.compile(r'DataPropertyName\s*=\s*"([^"]+)"')

changed_files_count = 0

for folder in folders_to_check:
    for root, dirs, files in os.walk(folder):
        for file in files:
            if file.endswith('.cs'):
                filepath = os.path.join(root, file)
                try:
                    with open(filepath, 'r', encoding='utf-8') as f:
                        content = f.read()

                    original_content = content

                    # Find all DataPropertyName occurrences
                    def repl(match):
                        val = match.group(1)
                        if val in fix_dict:
                            return f'DataPropertyName = "{fix_dict[val]}"'
                        return match.group(0)

                    content = pattern.sub(repl, content)

                    if content != original_content:
                        with open(filepath, 'w', encoding='utf-8') as f:
                            f.write(content)
                        changed_files_count += 1
                        print(f"Fixed DataPropertyName in {filepath}")
                except Exception as e:
                    print(f"Error processing {filepath}: {e}")

print(f"Fixed {changed_files_count} files.")
