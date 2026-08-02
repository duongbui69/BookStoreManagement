import os

# Fix HRService
filepath = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Services\HRService.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()
content = content.replace("_userRepository", "_userRepo")
with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

# Fix SalaryForm
filepath = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Forms\SalaryForm.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

cell_end_edit = """
        private async void DgvData_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvData.Columns[e.ColumnIndex].Name == "HourlyRate")
            {
                var row = dgvData.Rows[e.RowIndex];
                if (row.Cells["StaffId"].Value != null && row.Cells["HourlyRate"].Value != null)
                {
                    if (decimal.TryParse(row.Cells["HourlyRate"].Value.ToString(), out decimal newRate))
                    {
                        int staffId = (int)row.Cells["StaffId"].Value;
                        await _hrService.UpdateHourlyRateAsync(staffId, newRate);
                        
                        decimal totalHours = Convert.ToDecimal(row.Cells["TotalHours"].Value);
                        row.Cells["TotalSalary"].Value = newRate * totalHours;
                        
                        decimal total = 0;
                        foreach(DataGridViewRow r in dgvData.Rows) 
                        {
                            if (r.Cells["TotalSalary"].Value != null)
                                total += Convert.ToDecimal(r.Cells["TotalSalary"].Value);
                        }
                        lblTotalSalary.Text = $"Tổng quỹ lương: {total:N0} đ";
                    }
                }
            }
        }
"""
if "DgvData_CellEndEdit(object sender" not in content:
    content = content.replace("private void FormatGrid()", cell_end_edit + "\n        private void FormatGrid()")
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Injected DgvData_CellEndEdit")

