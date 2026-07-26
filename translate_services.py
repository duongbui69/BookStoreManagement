import os
import re
import json

root = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Services'

translations = {
    "Author name already exists.": "Tên tác giả đã tồn tại.",
    "Author name cannot be empty.": "Tên tác giả không được để trống.",
    "Author name cannot exceed 150 chars.": "Tên tác giả không được vượt quá 150 ký tự.",
    "Book code already exists.": "Mã sách đã tồn tại.",
    "Book code cannot be empty.": "Mã sách không được để trống.",
    "Book title cannot be empty.": "Tên sách không được để trống.",
    "Category name already exists.": "Tên danh mục đã tồn tại.",
    "Category name cannot be empty.": "Tên danh mục không được để trống.",
    "Category name cannot exceed 100 chars.": "Tên danh mục không được vượt quá 100 ký tự.",
    "Confirmation password does not match.": "Mật khẩu xác nhận không khớp.",
    "Current account not found.": "Không tìm thấy tài khoản hiện tại.",
    "Customer code already exists.": "Mã khách hàng đã tồn tại.",
    "Customer name cannot be empty.": "Tên khách hàng không được để trống.",
    "Customer name cannot exceed 150 chars.": "Tên khách hàng không được vượt quá 150 ký tự.",
    "Employee can only operate in their store.": "Nhân viên chỉ có thể thao tác ở chi nhánh của mình.",
    "Employee can only view data of their store.": "Nhân viên chỉ có thể xem dữ liệu chi nhánh của mình.",
    "ID Card already exists.": "CCCD/CMND đã tồn tại.",
    "ISBN already exists.": "ISBN đã tồn tại.",
    "Import quantity must be greater than 0.": "Số lượng nhập phải lớn hơn 0.",
    "Import receipt must have at least one book.": "Phiếu nhập phải có ít nhất một quyển sách.",
    "Invalid Excel file path.": "Đường dẫn file Excel không hợp lệ.",
    "Invalid ID list.": "Danh sách ID không hợp lệ.",
    "Invalid ID.": "ID không hợp lệ.",
    "Invalid author ID.": "Mã tác giả không hợp lệ.",
    "Invalid author data.": "Dữ liệu tác giả không hợp lệ.",
    "Invalid book ID.": "Mã sách không hợp lệ.",
    "Invalid book category.": "Danh mục sách không hợp lệ.",
    "Invalid book data.": "Dữ liệu sách không hợp lệ.",
    "Invalid book.": "Sách không hợp lệ.",
    "Invalid category ID.": "Mã danh mục không hợp lệ.",
    "Invalid category data.": "Dữ liệu danh mục không hợp lệ.",
    "Invalid customer ID.": "Mã khách hàng không hợp lệ.",
    "Invalid customer data.": "Dữ liệu khách hàng không hợp lệ.",
    "Invalid date range.": "Khoảng thời gian không hợp lệ.",
    "Invalid discount.": "Giảm giá không hợp lệ.",
    "Invalid export receipt ID.": "Mã phiếu xuất không hợp lệ.",
    "Invalid import price.": "Giá nhập không hợp lệ.",
    "Invalid import receipt ID.": "Mã phiếu nhập không hợp lệ.",
    "Invalid inventory ID.": "Mã kho không hợp lệ.",
    "Invalid inventory data.": "Dữ liệu kho không hợp lệ.",
    "Invalid invoice ID.": "Mã hóa đơn không hợp lệ.",
    "Invalid min stock.": "Tồn kho tối thiểu không hợp lệ.",
    "Invalid or closed shift.": "Ca làm việc không hợp lệ hoặc đã đóng.",
    "Invalid publication year.": "Năm xuất bản không hợp lệ.",
    "Invalid publisher ID.": "Mã NXB không hợp lệ.",
    "Invalid publisher data.": "Dữ liệu NXB không hợp lệ.",
    "Invalid refund unit price.": "Đơn giá hoàn không hợp lệ.",
    "Invalid return receipt ID.": "Mã phiếu trả không hợp lệ.",
    "Invalid role ID.": "Mã vai trò không hợp lệ.",
    "Invalid selling price.": "Giá bán không hợp lệ.",
    "Invalid status.": "Trạng thái không hợp lệ.",
    "Invalid stock quantity.": "Số lượng tồn kho không hợp lệ.",
    "Invalid store ID.": "Mã chi nhánh không hợp lệ.",
    "Invalid store data.": "Dữ liệu chi nhánh không hợp lệ.",
    "Invalid store.": "Chi nhánh không hợp lệ.",
    "Invalid supplier ID.": "Mã nhà cung cấp không hợp lệ.",
    "Invalid supplier data.": "Dữ liệu nhà cung cấp không hợp lệ.",
    "Invalid supplier.": "Nhà cung cấp không hợp lệ.",
    "Invalid total stock.": "Tổng tồn kho không hợp lệ.",
    "Invalid unit price.": "Đơn giá không hợp lệ.",
    "Invoice must contain at least one book.": "Hóa đơn phải có ít nhất một quyển sách.",
    "Invoice not found.": "Không tìm thấy hóa đơn.",
    "No data table to export to Excel.": "Không có dữ liệu bảng để xuất Excel.",
    "No data to export to Excel.": "Không có dữ liệu để xuất Excel.",
    "Not enough stock to sell.": "Không đủ số lượng tồn kho để bán.",
    "Number of pages must be > 0.": "Số trang phải > 0.",
    "Old password is incorrect.": "Mật khẩu cũ không chính xác.",
    "Password cannot be empty.": "Mật khẩu không được để trống.",
    "Password must have at least 6 characters.": "Mật khẩu phải có ít nhất 6 ký tự.",
    "Please log in.": "Vui lòng đăng nhập.",
    "Publisher name already exists.": "Tên NXB đã tồn tại.",
    "Publisher name cannot be empty.": "Tên NXB không được để trống.",
    "Publisher name cannot exceed 150 chars.": "Tên NXB không được vượt quá 150 ký tự.",
    "Return quantity must be greater than 0.": "Số lượng trả phải lớn hơn 0.",
    "Return receipt must have at least one book.": "Phiếu trả phải có ít nhất một quyển sách.",
    "Return receipt not found.": "Không tìm thấy phiếu trả.",
    "Reward points cannot be < 0.": "Điểm thưởng không được < 0.",
    "Role name cannot be empty.": "Tên vai trò không được để trống.",
    "Sales quantity must be greater than 0.": "Số lượng bán phải lớn hơn 0.",
    "Store address cannot be empty.": "Địa chỉ chi nhánh không được để trống.",
    "Store code already exists.": "Mã chi nhánh đã tồn tại.",
    "Store code cannot be empty.": "Mã chi nhánh không được để trống.",
    "Store code cannot exceed 20 characters.": "Mã chi nhánh không được vượt quá 20 ký tự.",
    "Store name already exists.": "Tên chi nhánh đã tồn tại.",
    "Store name cannot be empty.": "Tên chi nhánh không được để trống.",
    "Store name cannot exceed 150 chars.": "Tên chi nhánh không được vượt quá 150 ký tự.",
    "Supplier name already exists.": "Tên nhà cung cấp đã tồn tại.",
    "Supplier name cannot be empty.": "Tên nhà cung cấp không được để trống.",
    "Supplier name cannot exceed 150 chars.": "Tên nhà cung cấp không được vượt quá 150 ký tự.",
    "This function is for Admin only.": "Chức năng này chỉ dành cho Admin.",
    "Type code cannot be empty.": "Mã loại không được để trống.",
    "Type name cannot be empty.": "Tên loại không được để trống.",
    "You are not logged in.": "Bạn chưa đăng nhập.",
    "You can only manipulate data from your store.": "Bạn chỉ có thể thao tác dữ liệu từ chi nhánh của mình.",
    "You can only view invoices you created.": "Bạn chỉ có thể xem hóa đơn do mình tạo.",
    "You do not have permission to use this function.": "Bạn không có quyền sử dụng chức năng này.",
    "You do not have permission to view or edit this information.": "Bạn không có quyền xem hoặc chỉnh sửa thông tin này.",
    "You have an open shift. Please close it before opening a new one.": "Bạn đang có ca làm việc mở. Vui lòng đóng ca trước khi mở ca mới."
}

changed_files_count = 0

sorted_keys = sorted(translations.keys(), key=len, reverse=True)

for subdir, dirs, files in os.walk(root):
    for f in files:
        if f.endswith('.cs'):
            filepath = os.path.join(subdir, f)
            with open(filepath, 'r', encoding='utf-8') as file:
                content = file.read()
            
            original = content
            for k, v in translations.items():
                content = content.replace(f'"{k}"', f'"{v}"')
                
            if content != original:
                with open(filepath, 'w', encoding='utf-8') as file:
                    file.write(content)
                changed_files_count += 1
                
print(f"Updated {changed_files_count} files in Services.")
