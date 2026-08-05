# Goal Description

Rà soát toàn bộ mã nguồn của dự án BookStoreManagement để tìm ra các chức năng chưa hoàn thiện, các nút bấm rỗng (không có logic xử lý) và các điểm cần tối ưu về mặt giao diện/trải nghiệm người dùng (UX) cũng như kiến trúc mã nguồn. Từ đó đưa ra kế hoạch nâng cấp và hoàn thiện dự án.

## Open Questions

> [!IMPORTANT]
> **Giải thích về 2 lựa chọn lưu trữ Cài đặt (Settings):**
> 
> **1. Lưu trong cơ sở dữ liệu (SQL Server):**
> - *Lợi ích:* Phù hợp với các thiết lập mang tính "toàn hệ thống" (như Tên cửa hàng, Địa chỉ, Số lượng cảnh báo tồn kho). Nếu ứng dụng của bạn cài trên nhiều máy tính (1 máy quản lý, 2 máy thu ngân), khi sửa tên cửa hàng trên 1 máy, toàn bộ các máy khác đều đồng bộ ngay lập tức.
> - *Hạn chế:* Không phù hợp để lưu những thiết lập mang tính "sở thích cá nhân" (ví dụ chế độ Dark Mode), vì nếu lưu chung, 1 người bật Dark Mode thì toàn bộ các máy khác cũng bị đen theo.
> 
> **2. Lưu trong tệp tin cục bộ (appsettings.json):**
> - *Lợi ích:* Phù hợp với các thiết lập cục bộ cho từng thiết bị (Theme Sáng/Tối, Ngôn ngữ). Mỗi máy sẽ tự lưu 1 file cài đặt riêng.
> - *Hạn chế:* Nếu dùng nó để lưu "Tên cửa hàng", khi muốn đổi tên, bạn sẽ phải đi mở từng máy một để đổi lại.
> 
> **=> ĐỀ XUẤT CỦA TÔI (Giải pháp kết hợp tốt nhất):** 
> - Lưu các cài đặt hệ thống (Tên cửa hàng, Mức cảnh báo tồn kho, v.v) vào **SQL Server**. 
> - Lưu các cài đặt hiển thị (Chế độ Dark Mode/Light Mode) vào file **appsettings.json** cục bộ của máy tính đó.
> 
> **Bạn có đồng ý với phương án Kết hợp này không?**

## Phân tích hiện trạng và Các nút chức năng chưa có Logic

Qua rà soát, tôi phát hiện các vấn đề sau:

1. **Phân hệ Cài đặt (SettingsControl)**
   - **Tình trạng:** Toàn bộ giao diện `SettingsControl` hiện tại chỉ là "vỏ bọc". Các nút như `btnSave` ("Lưu thay đổi") hoàn toàn chưa được gán sự kiện Click. Việc đổi Theme hiện tại chưa được lưu vĩnh viễn (tắt app bật lại sẽ mất).
   
2. **Sự thiếu đồng bộ về Phân trang (Pagination)**
   - **Tình trạng:** Còn nhiều màn hình đang sử dụng code phân trang cũ hoặc không có phân trang: `PublisherControl`, `SupplierControl`, `MasterDataControl`, `StaffReturnControl`, và phần tìm kiếm sản phẩm trong `POSControl`.
   
3. **Phân hệ Tổng quan (DashboardControl)**
   - **Tình trạng:** Các con số thống kê đang bị cứng (fix cứng). Chưa có bộ lọc thời gian (Hôm nay, Tuần này...). Biểu đồ đang được vẽ thủ công bằng nét vẽ lập trình, rất tĩnh và khô khan.

4. **Bảo mật và Validation ở tầng Service**
   - **Tình trạng:** App chủ yếu bắt lỗi ở tầng UI. Tầng Service và Repository cần thêm các logic kiểm tra (Validation) chặt chẽ hơn để tránh lỗi SQL (ví dụ: xóa danh mục đang có chứa sách).

---

## Proposed Changes

Dưới đây là kế hoạch chi tiết để khắc phục các vấn đề trên:

### 1. Hoàn thiện tính năng Cài đặt (Settings)
- [NEW] Tạo bảng `SystemSettings` trong CSDL để lưu các thiết lập chung (StoreName, BranchId, Address, Threshold).
- [NEW] Tạo file config tĩnh để lưu cấu hình Dark Mode của máy.
- [MODIFY] `SettingsControl.cs`: Đấu nối sự kiện `btnSave.Click` ở tất cả các tab (General, Security, Alerts) gọi xuống `SettingsService`.
- [MODIFY] `ThemeManager.cs`: Tự động đọc file cấu hình Theme khi khởi động để áp dụng Dark/Light Mode.

### 2. Đồng bộ hóa Phân trang toàn hệ thống
- [MODIFY] `PublisherControl.cs`, `SupplierControl.cs`, `MasterDataControl.cs`: Thay thế toàn bộ logic phân trang cũ bằng component `PaginationControl` mới để đồng bộ UI/UX.

### 3. Nâng cấp Dashboard với LiveCharts (Đã chốt)
- [MODIFY] Cài đặt thư viện **LiveCharts** (hoặc **LiveCharts2**) vào dự án.
- [MODIFY] `DashboardControl.cs`: Xóa bỏ code vẽ biểu đồ thủ công. Thay thế bằng control của LiveCharts với tooltip và animation đẹp mắt.
- [MODIFY] `DashboardControl.cs`: Thêm ComboBox bộ lọc (Hôm nay, 7 ngày qua, Tháng này, Năm nay) và đấu nối với `DashboardService` để lấy dữ liệu động.

### 4. Tăng cường Validation logic
- [MODIFY] `BookService`, `OrderService`, `CategoryService`: Thêm các business rule validation trước khi gọi hàm Insert/Update.

## Verification Plan

### Manual Verification
- **Cài đặt:** Chạy ứng dụng, vào menu Cài đặt đổi tên cửa hàng và nhấn Save. Tắt app và mở lại, kiểm tra xem tên có giữ nguyên và Theme Sáng/Tối có đúng với lúc tắt app hay không.
- **Biểu đồ:** Vào Tổng quan, trỏ chuột vào biểu đồ mới xem có hiện Tooltip (số liệu động) không. Đổi bộ lọc thời gian để thấy biểu đồ uốn lượn (animation).
- **Phân trang:** Test tất cả các màn hình danh sách xem có chuyển trang mượt mà không.
