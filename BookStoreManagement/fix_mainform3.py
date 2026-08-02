import os

repo_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Forms\MainForm.cs"
with open(repo_path, 'r', encoding='utf-8') as f:
    repo_content = f.read()

old_code = """
            // Load Dashboard by default for Admin, POS for Staff
            if (CurrentSession.IsStaff)
            {

                BtnPOS_Click(this, EventArgs.Empty);
            }
            else
            {
                LoadDashboard();
            }
        }
"""
new_code = """
            this.Shown += MainForm_Shown;
        }

        private void MainForm_Shown(object? sender, EventArgs e)
        {
            if (CurrentSession.IsStaff)
            {
                BtnPOS_Click(this, EventArgs.Empty);
            }
            else
            {
                LoadDashboard();
            }
        }
"""

if old_code in repo_content:
    repo_content = repo_content.replace(old_code, new_code)
    with open(repo_path, 'w', encoding='utf-8') as f:
        f.write(repo_content)
    print("Updated MainForm.cs")
else:
    print("Still could not find it.")
