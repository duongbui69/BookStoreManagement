import os

repo_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Forms\ShiftDetailsForm.cs"
with open(repo_path, 'r', encoding='utf-8') as f:
    repo_content = f.read()

import re

# Match the entire LoadData method
pattern = r"private void LoadData\(\)[\s\S]*?(?=\s+private void ApplyTheme\(\)|\s+protected override void OnPaint|\s+\})"

new_load_data = """private void LoadData()
        {
            try
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

                    dgvOrders.Columns.Clear();
                    dgvOrders.Columns.Add("Id", "Mã HĐ");
                    dgvOrders.Columns.Add("Time", "Thời gian");
                    dgvOrders.Columns.Add("Total", "Tổng tiền");
                    dgvOrders.Columns["Total"].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;

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
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Lỗi ShiftDetailsForm", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }"""

match = re.search(pattern, repo_content)
if match:
    repo_content = repo_content[:match.start()] + new_load_data + repo_content[match.end():]
    with open(repo_path, 'w', encoding='utf-8') as f:
        f.write(repo_content)
    print("Replaced LoadData with foolproof version")
else:
    print("Could not match LoadData")

