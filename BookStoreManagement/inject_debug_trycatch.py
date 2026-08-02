import os

repo_path1 = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Forms\ShiftDetailsForm.cs"
with open(repo_path1, 'r', encoding='utf-8') as f:
    content1 = f.read()

old_load_data1 = """                dgvOrders.Rows.Clear();
                foreach (var order in orders)
                {
                    dgvOrders.Rows.Add(
                        order.Id,
                        order.OrderDate.ToString("HH:mm:ss"),
                        order.TotalAmount.ToString("N0") + " ₫"
                    );
                }"""

new_load_data1 = """                try
                {
                    dgvOrders.Rows.Clear();
                    foreach (var order in orders)
                    {
                        dgvOrders.Rows.Add(
                            order.Id,
                            order.OrderDate.ToString("HH:mm:ss"),
                            order.TotalAmount.ToString("N0") + " ₫"
                        );
                    }
                }
                catch (System.Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Lỗi dgvOrders (ShiftDetailsForm). Columns.Count = {dgvOrders.Columns.Count}. Lỗi: {ex.Message}", "Lỗi Debug");
                }"""

if old_load_data1 in content1:
    content1 = content1.replace(old_load_data1, new_load_data1)
    with open(repo_path1, 'w', encoding='utf-8') as f:
        f.write(content1)
    print("Injected try/catch into ShiftDetailsForm.cs")
else:
    print("Could not find block in ShiftDetailsForm.cs")

repo_path2 = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\UserControls\StaffMyShiftsControl.cs"
with open(repo_path2, 'r', encoding='utf-8') as f:
    content2 = f.read()

old_render2 = """            dgvHistory.Rows.Clear();
            if (_shiftHistory == null || _shiftHistory.Count == 0)
                return;

            foreach (var item in pageItems)
            {
                string timeStr = $"{item.StartTime:HH:mm} - {(item.EndTime.HasValue ? item.EndTime.Value.ToString("HH:mm") : "Đang mở")}";
                
                int rowIndex = dgvHistory.Rows.Add(
                    "#" + item.Id,
                    item.StartTime.ToString("dd/MM/yyyy"),
                    item.ShiftName,
                    timeStr,
                    item.Revenue.ToString("N0") + " đ",
                    item.Status == "Đã đóng" ? "Đã đóng" : "Đang mở",
                    "Chi tiết"
                );
                dgvHistory.Rows[rowIndex].Tag = item;
            }"""

new_render2 = """            try
            {
                dgvHistory.Rows.Clear();
                if (_shiftHistory == null || _shiftHistory.Count == 0)
                    return;

                foreach (var item in pageItems)
                {
                    string timeStr = $"{item.StartTime:HH:mm} - {(item.EndTime.HasValue ? item.EndTime.Value.ToString("HH:mm") : "Đang mở")}";
                    
                    int rowIndex = dgvHistory.Rows.Add(
                        "#" + item.Id,
                        item.StartTime.ToString("dd/MM/yyyy"),
                        item.ShiftName,
                        timeStr,
                        item.Revenue.ToString("N0") + " đ",
                        item.Status == "Đã đóng" ? "Đã đóng" : "Đang mở",
                        "Chi tiết"
                    );
                    dgvHistory.Rows[rowIndex].Tag = item;
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi dgvHistory (StaffMyShiftsControl). Columns.Count = {dgvHistory.Columns.Count}. Lỗi: {ex.Message}", "Lỗi Debug");
            }"""

if old_render2 in content2:
    content2 = content2.replace(old_render2, new_render2)
    with open(repo_path2, 'w', encoding='utf-8') as f:
        f.write(content2)
    print("Injected try/catch into StaffMyShiftsControl.cs")
else:
    print("Could not find block in StaffMyShiftsControl.cs")
