import re

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/StaffMyInvoicesControl.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Fix 1: Add CellContentClick wireup
text = text.replace('dgvInvoices.CellPainting += DgvInvoices_CellPainting;', 'dgvInvoices.CellPainting += DgvInvoices_CellPainting;\n            dgvInvoices.CellContentClick += DgvInvoices_CellContentClick;')

# Fix 2: Also fix column name in RenderCurrentPage if it exists
text = text.replace('Name = "Action",', 'Name = "Thao tác",')

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/StaffMyInvoicesControl.cs', 'w', encoding='utf-8') as f:
    f.write(text)
