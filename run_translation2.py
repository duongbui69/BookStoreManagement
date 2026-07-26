import os

translations = {
    "+ Add Customer": "+ Thêm Khách hàng",
    "+ Add Employee": "+ Thêm Nhân viên",
    "+ Add New": "+ Thêm Mới",
    "+ Add Store": "+ Thêm Chi nhánh",
    "+ Add User": "+ Thêm Người dùng",
    "+ CREATE NEW ORDER": "+ TẠO ĐƠN HÀNG MỚI",
    "+ Create Export Receipt": "+ Tạo Phiếu Xuất",
    "+ Create Import Receipt": "+ Tạo Phiếu Nhập",
    "+ Create Invoice": "+ Tạo Hóa Đơn",
    "+ Create Return Receipt": "+ Tạo Phiếu Trả",
    "Account Management": "Quản lý Tài khoản",
    "Account created successfully.": "Tạo tài khoản thành công.",
    "Account updated successfully.": "Cập nhật tài khoản thành công.",
    "Action": "Thao tác",
    "Active": "Hoạt động",
    "Active Status": "Trạng thái",
    "Add New": "Thêm mới",
    "Add New Author": "Thêm mới Tác giả",
    "Add New Category": "Thêm mới Danh mục",
    "Add New Publisher": "Thêm mới NXB",
    "Add New Supplier": "Thêm mới NCC",
    "Add new inventory": "Thêm mới kho",
    "Address": "Địa chỉ",
    "Address *": "Địa chỉ *",
    "Admin Terminal": "Giao diện Admin",
    "Admin User": "Người dùng Admin",
    "Admin manages document types for accounting/inventory.": "Admin quản lý loại chứng từ kế toán/kho.",
    "All Categories": "Tất cả Danh mục",
    "All Shifts": "Tất cả ca",
    "All Stores": "Tất cả Chi nhánh",
    "All Suppliers": "Tất cả Nhà cung cấp",
    "Any Role": "Tất cả Vai trò",
    "Any Status": "Mọi Trạng thái",
    "Are you sure you want to cancel this order? This action cannot be undone.": "Bạn có chắc chắn muốn hủy đơn hàng này không? Hành động này không thể hoàn tác.",
    "Are you sure you want to delete this category?": "Bạn có chắc chắn muốn xóa danh mục này?",
    "Are you sure you want to delete this customer?": "Bạn có chắc chắn muốn xóa khách hàng này?",
    "Are you sure you want to delete this product?": "Bạn có chắc muốn xóa sản phẩm này?",
    "Are you sure you want to delete this publisher?": "Bạn có chắc muốn xóa nhà xuất bản này?",
    "Are you sure you want to delete this receipt?": "Bạn có chắc chắn muốn xóa phiếu này không?",
    "Are you sure you want to delete this shift?": "Bạn có chắc chắn muốn xóa ca này?",
    "Are you sure you want to delete this store?": "Bạn có chắc chắn muốn xóa chi nhánh này?",
    "Are you sure you want to delete this supplier?": "Bạn có chắc muốn xóa nhà cung cấp này?",
    "Are you sure you want to delete this type?": "Bạn có chắc muốn xóa loại chứng từ này?",
    "Are you sure you want to delete this user?": "Bạn có chắc muốn xóa người dùng này?",
    "Author Name": "Tên Tác giả",
    "Author Management": "Quản lý Tác giả",
    "Author management": "Quản lý tác giả",
    "Author name (*)": "Tên Tác giả (*)",
    "Author: ": "Tác giả: ",
    "Authors": "Tác giả",
    "BACK": "QUAY LẠI",
    "BARCODE": "MÃ VẠCH",
    "BOOK TITLE": "TÊN SÁCH",
    "Back": "Quay lại",
    "Backup & Restore": "Sao lưu & Khôi phục",
    "Barcode": "Mã vạch",
    "Barcode is required.": "Vui lòng nhập mã vạch.",
    "Book List": "Danh sách Sách",
    "Book Title": "Tên Sách",
    "Business configuration and receipt settings.": "Cấu hình kinh doanh và thiết lập phiếu.",
    "Buy List": "Danh sách Mua",
    "Buy Qty": "Số lượng mua",
    "CATEGORY": "DANH MỤC",
    "CODE": "MÃ",
    "CONTACT": "LIÊN HỆ",
    "CREATED AT": "NGÀY TẠO",
    "CUSTOMER ID": "MÃ KHÁCH HÀNG",
    "CUSTOMER NAME": "TÊN KHÁCH HÀNG",
    "Cancel": "Hủy",
    "Cancel Order": "Hủy Đơn Hàng",
    "Cancel Order (F8)": "Hủy Đơn Hàng (F8)",
    "Canceled": "Đã hủy",
    "Cash": "Tiền mặt",
    "Catalog Management": "Quản lý Danh mục",
    "Categories": "Danh mục",
    "Category Name": "Tên Danh mục",
    "Category name (*)": "Tên danh mục (*)",
    "Category: ": "Danh mục: ",
    "Change Password": "Đổi Mật khẩu",
    "Choose shift": "Chọn ca",
    "Clear": "Xóa",
    "Clear list": "Xóa danh sách",
    "Close": "Đóng",
    "Close Shift": "Đóng ca",
    "Closed": "Đã đóng",
    "Code": "Mã",
    "Completed": "Hoàn thành",
    "Confirm Delete": "Xác nhận xóa",
    "Could not export empty data.": "Không thể xuất dữ liệu trống.",
    "Create Receipt": "Tạo Phiếu",
    "Create and manage user accounts and system roles.": "Tạo và quản lý tài khoản người dùng và vai trò.",
    "Current Store": "Chi nhánh hiện tại",
    "Customer": "Khách hàng",
    "Customer ID": "Mã Khách hàng",
    "Customer Management": "Quản lý Khách hàng",
    "Customer Name": "Tên Khách hàng",
    "Customer not found": "Không tìm thấy khách hàng",
    "DATE": "NGÀY",
    "DESCRIPTION": "MÔ TẢ",
    "Dashboard": "Tổng quan",
    "Data successfully exported to Excel!": "Xuất dữ liệu ra Excel thành công!",
    "Database backup & restore functionality for admin.": "Chức năng sao lưu & khôi phục DB cho admin.",
    "Date": "Ngày",
    "Date:": "Ngày:",
    "Delete": "Xóa",
    "Delete Category": "Xóa Danh mục",
    "Deleted successfully.": "Đã xóa thành công.",
    "Description": "Mô tả",
    "Details": "Chi tiết",
    "Discount": "Giảm giá",
    "Document Types": "Loại chứng từ",
    "Edit": "Sửa",
    "Email": "Email",
    "Email address": "Địa chỉ Email",
    "Dashboard Overview": "Tổng quan Dashboard",
    "Delete selected": "Xóa mục đã chọn",
    "Deleted successfully!": "Đã xóa thành công!",
    "EMAIL": "EMAIL",
    "EMPLOYEE ID": "MÃ NHÂN VIÊN",
    "Edit Author": "Sửa Tác giả",
    "Edit Publisher": "Sửa NXB",
    "Edit Supplier": "Sửa NCC",
    "Employee Management": "Quản lý Nhân viên",
    "Employee created.": "Đã tạo nhân viên.",
    "Employee deleted successfully!": "Xóa nhân viên thành công!",
    "Time:": "Thời gian:"
}

root_dir = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement'

changed_files_count = 0

sorted_keys = sorted(translations.keys(), key=len, reverse=True)

for subdir, dirs, files in os.walk(root_dir):
    if 'obj' in subdir or 'bin' in subdir or 'Migrations' in subdir or 'Properties' in subdir:
        continue
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(subdir, file)
            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                original_content = content
                for k in sorted_keys:
                    v = translations[k]
                    content = content.replace(f'"{k}"', f'"{v}"')
                
                if content != original_content:
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(content)
                    changed_files_count += 1
                    
            except Exception as e:
                print(f"Error processing {filepath}: {e}")

print(f"Translation applied to {changed_files_count} files.")
