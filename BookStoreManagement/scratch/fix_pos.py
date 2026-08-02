import re

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/POSControl.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Fix RefreshDataAsync
replacement = """        public async Task RefreshDataAsync()
        {
            try {
                int currentStoreId = CurrentSession.StoreId ?? 1;
                _allBooks = await _inventoryService.GetByStoreIdAsync(currentStoreId);
                if (!this.IsDisposed) FilterBooks();
            } catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show("Refresh Data Error: " + ex.Message);
            }
        }"""

text = re.sub(r'        public async Task RefreshDataAsync\(\)\s*\{\s*try \{\s*\} catch \(Exception ex\) \{\s*System\.Windows\.Forms\.MessageBox\.Show\("Refresh Data Error: " \+ ex\.Message\);\s*\}\s*\}', replacement, text)

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/POSControl.cs', 'w', encoding='utf-8') as f:
    f.write(text)
