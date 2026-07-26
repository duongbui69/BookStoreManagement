import os

translations = {
    " Filter": " Lọc",
    "0 VND": "0 VND",
    "0 items (Total Qty: 0)": "0 mặt hàng (Tổng SL: 0)",
    "ACTION": "THAO TÁC",
    "AD": "AD",
    "ADDRESS": "ĐỊA CHỈ",
    "AUTHOR": "TÁC GIẢ",
    "AUTHOR ID": "MÃ TÁC GIẢ",
    "AUTHOR NAME": "TÊN TÁC GIẢ",
    "AVATAR": "ẢNH ĐẠI DIỆN",
    "All Statuses": "Tất cả Trạng thái",
    "All statuses": "Tất cả trạng thái",
    "Are you sure you want to delete the inventory of this product?": "Bạn có chắc chắn muốn xóa tồn kho của sản phẩm này không?",
    "Are you sure you want to delete this book?": "Bạn có chắc chắn muốn xóa cuốn sách này không?",
    "Are you sure you want to delete this employee?": "Bạn có chắc chắn muốn xóa nhân viên này không?",
    "Are you sure you want to end the current shift?": "Bạn có chắc chắn muốn kết thúc ca hiện tại không?",
    "Author": "Tác giả",
    "Author Code": "Mã Tác giả",
    "Author data not found!": "Không tìm thấy dữ liệu Tác giả!",
    "Author name already exists!": "Tên tác giả đã tồn tại!",
    "Auto-generate": "Tạo tự động",
    "BOOKSTORE MANAGEMENT": "QUẢN LÝ NHÀ SÁCH",
    "BRANCH": "CHI NHÁNH",
    "Book Categories": "Danh mục Sách",
    "Book Code": "Mã Sách",
    "Book Details": "Chi tiết Sách",
    "Book Management": "Quản lý Sách",
    "Book added.": "Đã thêm sách.",
    "Book deleted successfully!": "Đã xóa sách thành công!",
    "Book not found.": "Không tìm thấy sách.",
    "Book quantity": "Số lượng sách",
    "Book title": "Tên sách",
    "Book updated.": "Đã cập nhật sách.",
    "BookStore Management": "Quản lý Nhà Sách",
    "Bookstore ERP": "Bookstore ERP",
    "Bookstore ERP - Login": "Bookstore ERP - Đăng nhập",
    "Bookstore ERP System": "Hệ thống Bookstore ERP",
    "Browse...": "Duyệt...",
    "CUSTOMER": "KHÁCH HÀNG",
    "Cancellation error: ": "Lỗi hủy: ",
    "Cancelled": "Đã hủy",
    "Cannot delete this author. Data might be in use elsewhere.": "Không thể xóa tác giả này. Dữ liệu có thể đang được sử dụng.",
    "Cannot delete this publisher. Data might be in use elsewhere.": "Không thể xóa NXB này. Dữ liệu có thể đang được sử dụng.",
    "Cannot delete this supplier. Data might be in use elsewhere.": "Không thể xóa NCC này. Dữ liệu có thể đang được sử dụng.",
    "Cart is empty!": "Giỏ hàng trống!",
    "Category": "Danh mục",
    "Category Code": "Mã Danh mục",
    "Category List": "Danh sách Danh mục",
    "Category Management": "Quản lý Danh mục",
    "Category Name *": "Tên Danh mục *",
    "Category added successfully!": "Thêm danh mục thành công!",
    "Category deleted successfully!": "Xóa danh mục thành công!",
    "Category name already exists!": "Tên danh mục đã tồn tại!",
    "Category name cannot be empty.": "Tên danh mục không được để trống.",
    "Category updated successfully!": "Cập nhật danh mục thành công!",
    "Clear all": "Xóa tất cả",
    "Configure SMTP": "Cấu hình SMTP",
    "Configure email server to send daily reports.": "Cấu hình server email để gửi báo cáo.",
    "Confirm": "Xác nhận",
    "Confirm Return & Refund": "Xác nhận Trả hàng & Hoàn tiền",
    "Confirm close shift": "Xác nhận đóng ca",
    "Confirm delete": "Xác nhận xóa",
    "Confirm payment for this invoice?": "Xác nhận thanh toán hóa đơn này?",
    "Cooperating": "Đang hợp tác",
    "Current stock": "Tồn kho hiện tại",
    "Customer added successfully.": "Đã thêm khách hàng thành công.",
    "Customer needs to pay": "Khách cần trả",
    "Customer updated successfully.": "Cập nhật khách hàng thành công.",
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
