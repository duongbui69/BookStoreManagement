import os

filepath = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Repositories\UserRepository.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

update_method = """
        public async System.Threading.Tasks.Task<bool> UpdateHourlyRateAsync(int id, decimal rate)
        {
            const string sql = "UPDATE Users SET HourlyRate = @Rate WHERE Id = @Id";
            int rows = await ExecuteAsync(sql, new { Rate = rate, Id = id });
            return rows > 0;
        }
"""

if "UpdateHourlyRateAsync" not in content:
    content = content.replace("public async System.Threading.Tasks.Task<User?> GetByIdAsync(int id)", update_method + "\n        public async System.Threading.Tasks.Task<User?> GetByIdAsync(int id)")
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Added to UserRepository")
