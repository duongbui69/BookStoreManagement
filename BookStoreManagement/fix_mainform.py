import os
import re

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
                BtnDashboard_Click(this, EventArgs.Empty);
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
                BtnDashboard_Click(this, EventArgs.Empty);
            }
        }
"""
# Note: depending on the exact indentation and newlines, we should use a more robust regex if replace fails.

repo_content = re.sub(
    r"// Load Dashboard by default for Admin, POS for Staff\s+if \(CurrentSession\.IsStaff\)\s*\{\s*BtnPOS_Click\(this, EventArgs\.Empty\);\s*\}\s*else\s*\{\s*BtnDashboard_Click\(this, EventArgs\.Empty\);\s*\}\s*\}",
    new_code.strip(),
    repo_content
)

with open(repo_path, 'w', encoding='utf-8') as f:
    f.write(repo_content)
print("Updated MainForm.cs")
