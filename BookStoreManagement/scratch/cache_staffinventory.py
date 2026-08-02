import re

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/StaffInventoryControl.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Make StaffInventoryControl implement IRefreshable
text = text.replace('public partial class StaffInventoryControl : UserControl', 'public partial class StaffInventoryControl : UserControl, BookStoreManagement.Interfaces.IRefreshable')

# Add RefreshDataAsync
refresh_func = """        public async Task RefreshDataAsync()
        {
            await LoadDataAsync();
        }
"""
text = re.sub(r'(        protected override async void OnLoad\(EventArgs e\))', refresh_func + r'\n\1', text)

# Remove LoadDataAsync from OnLoad
text = re.sub(r'        protected override async void OnLoad\(EventArgs e\)\s*\{\s*base\.OnLoad\(e\);\s*await LoadDataAsync\(\);\s*\}', r'        protected override void OnLoad(EventArgs e)\n        {\n            base.OnLoad(e);\n        }', text)

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/StaffInventoryControl.cs', 'w', encoding='utf-8') as f:
    f.write(text)
