import re

def fix_interface(file_path, class_name):
    with open(file_path, 'r', encoding='utf-8') as f:
        text = f.read()

    text = re.sub(r'public class ' + class_name + r' : UserControl\s*\{', 'public class ' + class_name + ' : UserControl, BookStoreManagement.Interfaces.IRefreshable\n{', text)
    text = re.sub(r'public partial class ' + class_name + r' : UserControl\s*\{', 'public partial class ' + class_name + ' : UserControl, BookStoreManagement.Interfaces.IRefreshable\n{', text)
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(text)

fix_interface('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/StaffInventoryControl.cs', 'StaffInventoryControl')
fix_interface('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/InventoryControl.cs', 'InventoryControl')

