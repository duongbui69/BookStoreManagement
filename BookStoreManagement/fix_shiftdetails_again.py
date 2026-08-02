import os

repo_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Forms\ShiftDetailsForm.cs"
with open(repo_path, 'r', encoding='utf-8') as f:
    repo_content = f.read()

old_load_data = """                try
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

new_load_data = """                dgvOrders.Columns.Clear();
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
                }"""

if old_load_data in repo_content:
    repo_content = repo_content.replace(old_load_data, new_load_data)
    with open(repo_path, 'w', encoding='utf-8') as f:
        f.write(repo_content)
    print("Replaced LoadData inside ShiftDetailsForm")
else:
    print("Could not find block in ShiftDetailsForm")

