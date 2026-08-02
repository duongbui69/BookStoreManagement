import os
import glob
import re

controls_dir = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\UserControls"
files = glob.glob(os.path.join(controls_dir, "*.cs"))

for file in files:
    if "Designer" in file: continue
    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # check for DataGridView
    match = re.search(r'private DataGridView (dgv\w+);', content)
    if not match:
        match = re.search(r'(dgv\w+) = new DataGridView', content)
        
    if match:
        dgv_name = match.group(1)
        # find the edit logic: 
ew XxxForm( or 
ew Forms.XxxForm(
        edit_logic = re.search(r'new\s+(?:BookStoreManagement\.)?(?:Forms\.)?(\w+Form)\s*\(', content)
        form_name = edit_logic.group(1) if edit_logic else "Unknown"
        
        # also find data source id field
        # usually ar obj = currentData[e.RowIndex]; obj.Id
        # or Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["Id"].Value)
        
        print(f"{os.path.basename(file)}: {dgv_name} -> {form_name}")
