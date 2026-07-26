import os

root_dir = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement'

replacements = {
    '"SYSTEM ADMINISTRATOR"': '"QUẢN TRỊ HỆ THỐNG"',
    '"Book management"': '"Quản lý Sách"',
    '"HR management"': '"Quản lý Nhân sự"',
    '"Employee management"': '"Quản lý Nhân viên"',
    '"Account management"': '"Quản lý Tài khoản"',
    '"Manage stores"': '"Quản lý Chi nhánh"',
    '"Customer management"': '"Quản lý Khách hàng"',
    '"Manage Categories"': '"Quản lý Danh mục"',
    '"Statistical Reports"': '"Báo cáo Thống kê"',
    '"Order management"': '"Quản lý Đơn hàng"',
    '"Invoice management"': '"Quản lý Hóa đơn"',
    '"Refund management"': '"Quản lý Hoàn tiền"',
    '"TOTAL BOOKS"': '"TỔNG SỐ SÁCH"',
    '"LOW STOCK / OUT OF STOCK"': '"SẮP HẾT / HẾT HÀNG"',
    '"NEWLY IMPORTED"': '"SÁCH MỚI NHẬP"',
    '"Showing {startRec} to {endRec} of {_totalRecords} results (Page {_currentPage} of {_totalPages})"': '"Hiển thị {startRec} đến {endRec} trong số {_totalRecords} kết quả (Trang {_currentPage}/{_totalPages})"',
    '"Overview"': '"Tổng quan"'
}

changed_files_count = 0

for subdir, dirs, files in os.walk(root_dir):
    if 'obj' in subdir or 'bin' in subdir or 'Migrations' in subdir or 'Properties' in subdir:
        continue
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(subdir, file)
            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                original = content
                for k, v in replacements.items():
                    content = content.replace(k, v)
                
                if content != original:
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(content)
                    changed_files_count += 1
            except Exception as e:
                pass

print(f"Fixed missing translations in {changed_files_count} files.")
