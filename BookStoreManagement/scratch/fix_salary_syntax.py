import re
with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/Forms/SalaryForm.cs', 'r', encoding='utf-8') as f:
    text = f.read()

bad_string = """            foreach (DataGridViewColumn col in dgvData.Columns)
            if (dgvData.Columns["HourlyRate"] != null)
            {
                if (col.Name != "HourlyRate") col.ReadOnly = true;
            }"""
good_string = """            foreach (DataGridViewColumn col in dgvData.Columns)
            {
                if (col.Name != "HourlyRate") col.ReadOnly = true;
            }"""
if bad_string in text:
    text = text.replace(bad_string, good_string)
else:
    print("Could not find string.")

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/Forms/SalaryForm.cs', 'w', encoding='utf-8') as f:
    f.write(text)
