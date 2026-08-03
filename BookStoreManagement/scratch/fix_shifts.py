import re

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/StaffMyShiftsControl.cs', 'r', encoding='utf-8') as f:
    text = f.read()

replacement = """                    decimal displayRevenue = item.Revenue;
                    if (_activeShift != null && item.Id == _activeShift.Id) {
                        displayRevenue = _currentRevenue;
                    }

                    int rowIndex = dgvHistory.Rows.Add(
                        "#" + item.Id,
                        item.StartTime.ToString("dd/MM/yyyy"),
                        item.ShiftName,
                        timeStr,
                        displayRevenue.ToString("N0") + " đ",
                        item.Status == "Đã đóng" ? "Đã đóng" : "Đang mở",
                        "Chi tiết"
                    );"""

text = re.sub(r'                    int rowIndex = dgvHistory\.Rows\.Add\(\s*"#" \+ item\.Id,\s*item\.StartTime\.ToString\("dd/MM/yyyy"\),\s*item\.ShiftName,\s*timeStr,\s*item\.Revenue\.ToString\("N0"\) \+ " đ",\s*item\.Status == "Đã đóng" \? "Đã đóng" : "Đang mở",\s*"Chi tiết"\s*\);', replacement, text)

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/StaffMyShiftsControl.cs', 'w', encoding='utf-8') as f:
    f.write(text)
