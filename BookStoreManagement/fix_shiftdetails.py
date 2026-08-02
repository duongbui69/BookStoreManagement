import os

repo_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Forms\ShiftDetailsForm.cs"
with open(repo_path, 'r', encoding='utf-8') as f:
    repo_content = f.read()

old_load_data = """        private void LoadData()
        {
            var shift = _shiftService.GetShiftById(_shiftId);
            if (shift != null)
            {
                lblShiftId.Text = $"Ca: {shift.ShiftName} (Nhân viên: {shift.StaffId})";
                lblTime.Text = $"Thời gian: {shift.StartTime:dd/MM/yyyy HH:mm} - {(shift.EndTime.HasValue ? shift.EndTime.Value.ToString("HH:mm") : "Đang mở")}";
                lblStatus.Text = $"Trạng thái: {(shift.EndTime.HasValue ? "Đã kết thúc" : "Đang mở")}";

                var orders = _orderService.GetByDateRange(shift.StartTime, shift.EndTime ?? DateTime.Now)
                             .Where(o => o.StaffId == shift.StaffId).ToList();

                lblRevenue.Text = $"Doanh số: {orders.Sum(o => o.TotalAmount):N0} đ ({orders.Count} đơn)";

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
        }"""

new_load_data = """        private void LoadData()
        {
            var shift = _shiftService.GetShiftById(_shiftId);
            if (shift != null)
            {
                lblShiftId.Text = $"Ca: {shift.ShiftName} (Nhân viên: {shift.StaffId})";
                lblTime.Text = $"Thời gian: {shift.StartTime:dd/MM/yyyy HH:mm} - {(shift.EndTime.HasValue ? shift.EndTime.Value.ToString("HH:mm") : "Đang mở")}";
                lblStatus.Text = $"Trạng thái: {(shift.EndTime.HasValue ? "Đã kết thúc" : "Đang mở")}";

                var orders = _orderService.GetByDateRange(shift.StartTime, shift.EndTime ?? DateTime.Now)
                             .Where(o => o.StaffId == shift.StaffId).ToList();

                lblRevenue.Text = $"Doanh số: {orders.Sum(o => o.TotalAmount):N0} đ ({orders.Count} đơn)";

                if (dgvOrders.Columns.Count == 0)
                {
                    dgvOrders.Columns.Add("Id", "Mã HĐ");
                    dgvOrders.Columns.Add("Time", "Thời gian");
                    dgvOrders.Columns.Add("Total", "Tổng tiền");
                    dgvOrders.Columns["Total"].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                }

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
        }"""

repo_content = repo_content.replace(old_load_data, new_load_data)
with open(repo_path, 'w', encoding='utf-8') as f:
    f.write(repo_content)
print("Updated ShiftDetailsForm!")
