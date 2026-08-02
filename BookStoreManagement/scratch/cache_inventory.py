import re

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/InventoryControl.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Make InventoryControl implement IRefreshable
text = text.replace('public partial class InventoryControl : UserControl', 'public partial class InventoryControl : UserControl, BookStoreManagement.Interfaces.IRefreshable')

# Add RefreshDataAsync
refresh_func = """        public async Task RefreshDataAsync()
        {
            if (cbCategory.Items.Count == 0 || cbWarehouse.Items.Count == 0)
            {
                await LoadFiltersAsync();
            }
            LoadData(); // LoadData is async void, we can await it if we change it or just leave it
            await Task.CompletedTask;
        }
"""
text = re.sub(r'(        private async System\.Threading\.Tasks\.Task LoadFiltersAsync\(\))', refresh_func + r'\n\1', text)

# Remove LoadFiltersAsync and LoadData from Load
text = re.sub(r'            this\.Load \+= async \(s, e\) => \{\s*await LoadFiltersAsync\(\);\s*LoadData\(\);\s*\};', r'            // Load is deferred to RefreshDataAsync', text)

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/InventoryControl.cs', 'w', encoding='utf-8') as f:
    f.write(text)
