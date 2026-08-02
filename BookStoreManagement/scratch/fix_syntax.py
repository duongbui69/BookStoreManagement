import re
with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/StaffMyShiftsControl.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Replace the duplicate else
text = re.sub(r'                    _timer\.Start\(\);\s*\}\s*else\s*\{\s*lblCurrentShiftTitle\.Text = "Kh[^"]+";\s*lblEmployeeInfo[^}]+}', '                    _timer.Start();\n                }', text)

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/StaffMyShiftsControl.cs', 'w', encoding='utf-8') as f:
    f.write(text)
