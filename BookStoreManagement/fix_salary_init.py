import os

filepath = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Forms\SalaryForm.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("dgvData.CellEndEdit += DgvData_CellEndEdit;\n            dgvData.DataError += (s, e) => { e.Cancel = true; };", "")
content = content.replace("lblTotalSalary = new Label", "dgvData.CellEndEdit += DgvData_CellEndEdit;\n            dgvData.DataError += (s, e) => { e.Cancel = true; };\n            lblTotalSalary = new Label")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed dgvData init order in SalaryForm")
