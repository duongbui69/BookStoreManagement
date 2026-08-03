import re
file_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/StaffMyInvoicesControl.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

# Hide dtpFrom, dtpTo, btnFilter
text = text.replace('dtpFrom.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);', 'dtpFrom.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);\n            dtpFrom.Visible = false;')
text = text.replace('Format = DateTimePickerFormat.Short,\n                Font = new Font("Segoe UI", 9F)\n            };\n\n            btnFilter', 'Format = DateTimePickerFormat.Short,\n                Font = new Font("Segoe UI", 9F),\n                Visible = false\n            };\n\n            btnFilter')
text = text.replace('Cursor = Cursors.Hand\n            };\n            btnFilter.Click', 'Cursor = Cursors.Hand,\n                Visible = false\n            };\n            btnFilter.Click')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)
