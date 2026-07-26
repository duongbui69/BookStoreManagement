import re

file_path = 'BookStoreManagement/UserControls/InventoryControl.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Add StoreService initialization
content = content.replace('_categoryService = new CategoryService();', '_categoryService = new CategoryService();\n            _storeService = new StoreService();')

# Modify LoadFilters for cbWarehouse
old_load_filters = '''cbWarehouse.Items.AddRange(new string[] { "Kho hàng: Tất cả", "Kho Tổng (Hà Nội)", "Kho Chi Nhánh (HCM)", "Kho Miền Trung" });
            cbWarehouse.SelectedIndex = 0;'''

new_load_filters = '''var stores = _storeService.GetActive();
            stores.Insert(0, new Models.Store { Id = 0, StoreName = "Cửa hàng: Tất cả" });
            cbWarehouse.DataSource = stores;
            cbWarehouse.DisplayMember = "StoreName";
            cbWarehouse.ValueMember = "StoreName";'''

content = content.replace(old_load_filters, new_load_filters)

# Modify LoadData to get cbWarehouse selected value correctly
old_wfilter = '''string wFilter = cbWarehouse.SelectedIndex > 0 ? cbWarehouse.SelectedItem.ToString() : "";'''
new_wfilter = '''string wFilter = cbWarehouse.SelectedIndex > 0 ? cbWarehouse.SelectedValue.ToString() : "";'''

content = content.replace(old_wfilter, new_wfilter)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)
