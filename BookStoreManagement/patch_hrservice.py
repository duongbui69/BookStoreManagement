import os

filepath = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Services\HRService.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

update_method = """
        public async System.Threading.Tasks.Task<bool> UpdateHourlyRateAsync(int staffId, decimal rate)
        {
            return await _userRepository.UpdateHourlyRateAsync(staffId, rate);
        }
"""

if "UpdateHourlyRateAsync" not in content:
    content = content.replace("public async System.Threading.Tasks.Task<List<BookStoreManagement.Models.SalaryViewModel>> GetSalaryReportAsync", update_method + "\n        public async System.Threading.Tasks.Task<List<BookStoreManagement.Models.SalaryViewModel>> GetSalaryReportAsync")
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Added to HRService")
