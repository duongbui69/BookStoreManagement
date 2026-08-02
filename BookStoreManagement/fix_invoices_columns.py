import os

repo_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\UserControls\StaffMyInvoicesControl.cs"
with open(repo_path, 'r', encoding='utf-8') as f:
    repo_content = f.read()

old_cols = """
            dgvInvoices.Columns.Add("Code", "MÃ HÓA ĐƠN");
            dgvInvoices.Columns.Add("Date", "NGÀY BÁN");
            dgvInvoices.Columns.Add("Customer", "KHÁCH HÀNG");
            dgvInvoices.Columns.Add("Amount", "TỔNG TIỀN");
"""
new_cols = """
            dgvInvoices.Columns.Add("Id", "Id");
            dgvInvoices.Columns["Id"].Visible = false;
            dgvInvoices.Columns.Add("Code", "MÃ HÓA ĐƠN");
            dgvInvoices.Columns.Add("Date", "NGÀY BÁN");
            dgvInvoices.Columns.Add("Customer", "KHÁCH HÀNG");
            dgvInvoices.Columns.Add("Amount", "TỔNG TIỀN");
            dgvInvoices.Columns.Add("PaymentMethod", "THANH TOÁN");
"""

repo_content = repo_content.replace(old_cols, new_cols)
with open(repo_path, 'w', encoding='utf-8') as f:
    f.write(repo_content)
print("Updated columns!")
