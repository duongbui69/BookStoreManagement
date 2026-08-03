import re
file_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/StaffMyInvoicesControl.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

# Replace LoadDataAsync
old_load = '''        private async Task LoadDataAsync()
        {
            try
            {
                // Load only today's data for current staff by default (shift)
                DateTime today = DateTime.Today;
                DateTime endOfToday = today.AddDays(1).AddTicks(-1);
                
                _allItems = await _salesOrderService.GetByDateRangeAsync(today, endOfToday, CurrentSession.StoreId);'''

new_load = '''        private async Task LoadDataAsync()
        {
            try
            {
                var shiftService = new ShiftService();
                var activeShift = shiftService.GetActiveShift();
                
                DateTime fromTime;
                DateTime toTime;
                
                if (activeShift != null)
                {
                    fromTime = activeShift.StartTime;
                    toTime = DateTime.Now;
                }
                else
                {
                    fromTime = dtpFrom.Value.Date;
                    toTime = dtpTo.Value.Date.AddDays(1).AddTicks(-1);
                }
                
                _allItems = (await _salesOrderService.GetByDateRangeAsync(fromTime, toTime, CurrentSession.StoreId))
                            .Where(x => x.StaffId == CurrentSession.UserId).ToList();'''

text = text.replace(old_load, new_load)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)
