import re

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/RefundControl.cs', 'r', encoding='utf-8') as f:
    text = f.read()

old_load_data = """        private async Task LoadDataAsync()
        {
            var storeId = CurrentSession.StoreId ?? 1;
            try
            {
                var result = await _returnRepo.GetAllAsync();
                _allRefunds = result.Where(r => r.StoreId == storeId).ToList();"""

new_load_data = """        private async Task LoadDataAsync()
        {
            try
            {
                var result = await _returnRepo.GetAllAsync();
                if (CurrentSession.IsAdmin)
                {
                    _allRefunds = result.ToList();
                }
                else
                {
                    var storeId = CurrentSession.StoreId ?? 1;
                    _allRefunds = result.Where(r => r.StoreId == storeId).ToList();
                }"""

text = text.replace(old_load_data, new_load_data)

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/RefundControl.cs', 'w', encoding='utf-8') as f:
    f.write(text)
