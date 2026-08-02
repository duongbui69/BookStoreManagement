import os

repo_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\UserControls\StaffMyInvoicesControl.cs"
with open(repo_path, 'r', encoding='utf-8') as f:
    repo_content = f.read()

old_code = """                row.Cells["OrderCode"].Style.Font = new Font(dgvInvoices.Font, FontStyle.Bold);
                row.Cells["TotalAmount"].Style.Font = new Font(dgvInvoices.Font, FontStyle.Bold);"""
new_code = """                row.Cells["Code"].Style.Font = new Font(dgvInvoices.Font, FontStyle.Bold);
                row.Cells["Amount"].Style.Font = new Font(dgvInvoices.Font, FontStyle.Bold);"""

repo_content = repo_content.replace(old_code, new_code)
with open(repo_path, 'w', encoding='utf-8') as f:
    f.write(repo_content)
print("Fixed columns mapping in StaffMyInvoicesControl")
