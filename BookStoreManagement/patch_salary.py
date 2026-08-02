import os

filepath = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Forms\SalaryForm.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add Close button
close_btn = """
            Guna2Button btnClose = new Guna2Button { Text = "Đóng", Location = new Point(840, 520), Width = 120, Height = 36, BorderRadius = 4, Font = new Font("Segoe UI", 10F, FontStyle.Bold), FillColor = Color.Gray };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
"""

if "btnClose" not in content:
    content = content.replace("this.Controls.AddRange(new Control[] { lblTitle, lblMonth, cbMonth, lblYear, cbYear, btnCalculate, dgvData, lblTotalSalary });", "this.Controls.AddRange(new Control[] { lblTitle, lblMonth, cbMonth, lblYear, cbYear, btnCalculate, dgvData, lblTotalSalary });\n" + close_btn)

# Make editable
content = content.replace("ReadOnly = true,", "ReadOnly = false,\n                EditMode = DataGridViewEditMode.EditOnEnter,")
if "dgvData.CellEndEdit +=" not in content:
    content = content.replace("btnCalculate.Click += BtnCalculate_Click;", "btnCalculate.Click += BtnCalculate_Click;\n            dgvData.CellEndEdit += DgvData_CellEndEdit;\n            dgvData.DataError += (s, e) => { e.Cancel = true; };")

# Add CellEndEdit handler
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
if "DgvData_CellEndEdit" not in content:
    content = content.replace("private void FormatGrid()", cell_end_edit + "\n        private void FormatGrid()")

# Make other columns readonly
format_grid_updates = """
            foreach (DataGridViewColumn col in dgvData.Columns)
            {
                if (col.Name != "HourlyRate") col.ReadOnly = true;
            }
            dgvData.Columns["HourlyRate"].DefaultCellStyle.BackColor = Color.LightYellow;
"""
if "col.ReadOnly = true;" not in content:
    content = content.replace("if (dgvData.Columns[\"StaffId\"] != null) dgvData.Columns[\"StaffId\"].Visible = false;", format_grid_updates + "\n            if (dgvData.Columns[\"StaffId\"] != null) dgvData.Columns[\"StaffId\"].Visible = false;")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Patched SalaryForm")
