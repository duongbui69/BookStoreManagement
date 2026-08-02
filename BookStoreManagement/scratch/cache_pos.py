import re

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/POSControl.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Make POSControl implement IRefreshable
text = text.replace('public partial class POSControl : UserControl', 'public partial class POSControl : UserControl, BookStoreManagement.Interfaces.IRefreshable')

# Remove _allBooks loading from Load and put it into RefreshDataAsync
refresh_func = """        public async Task RefreshDataAsync()
        {
            try {
                int currentStoreId = CurrentSession.StoreId ?? 1;
                _allBooks = await _inventoryService.GetByStoreIdAsync(currentStoreId);
                if (!this.IsDisposed) FilterBooks();
            } catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show("Refresh Data Error: " + ex.Message);
            }
        }
"""
# Add the RefreshDataAsync method before POSControl_Load
text = re.sub(r'(        private async void POSControl_Load\(object sender, EventArgs e\))', refresh_func + r'\n\1', text)

# Remove the data loading from POSControl_Load
text = re.sub(r'                int currentStoreId = CurrentSession\.StoreId \?\? 1;\s*_allBooks = await _inventoryService\.GetByStoreIdAsync\(currentStoreId\);\s*if \(!this\.IsDisposed\) FilterBooks\(\);', '', text)

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/POSControl.cs', 'w', encoding='utf-8') as f:
    f.write(text)
