import os

repo_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Forms\ShiftDetailsForm.cs"
with open(repo_path, 'r', encoding='utf-8') as f:
    repo_content = f.read()

# Add AutoGenerateColumns = false
old_init = """            dgvOrders = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 40 }
            };"""

new_init = """            dgvOrders = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 40 },
                AutoGenerateColumns = false
            };"""

if old_init in repo_content:
    repo_content = repo_content.replace(old_init, new_init)
    with open(repo_path, 'w', encoding='utf-8') as f:
        f.write(repo_content)
    print("Updated ShiftDetailsForm!")
else:
    print("Could not find the target string in ShiftDetailsForm")
