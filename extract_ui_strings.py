import os
import re
import json

strings = set()
root_dir = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement'

# Patterns that indicate UI strings
# e.g., Text = "Hello", PlaceholderText = "Hello", HeaderText = "Hello", MessageBox.Show("Hello")
# also consider DataGridViewTextBoxColumn { HeaderText = "..." }
patterns = [
    r'(?:\bText\s*=\s*|\bPlaceholderText\s*=\s*|\bHeaderText\s*=\s*|\bToolTipText\s*=\s*|\.Show\s*\(\s*)\"([^\"]+)\"',
    r'(?:\bTitle\s*=\s*|\bMessage\s*=\s*)\"([^\"]+)\"'
]

for subdir, dirs, files in os.walk(root_dir):
    if 'obj' in subdir or 'bin' in subdir or 'Migrations' in subdir or 'Properties' in subdir:
        continue
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(subdir, file)
            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()
                    
                    for pattern in patterns:
                        matches = re.findall(pattern, content)
                        for match in matches:
                            # Filter out empty strings or very short technical strings (like "Id" or "N0")
                            # But keep things like "OK"
                            if len(match) > 0 and not match.startswith('@'):
                                # Avoid translating SQL queries or known format strings if they slip in
                                if "SELECT " not in match.upper() and "INSERT " not in match.upper():
                                    strings.add(match)
                                    
                    # Let's also grab UI texts in MessageBox
                    # MessageBox.Show("Are you sure?", "Confirm", ...)
                    ms_matches = re.findall(r'MessageBox\.Show\s*\(\s*\"([^\"]+)\"\s*,\s*\"([^\"]+)\"', content)
                    for m in ms_matches:
                        strings.add(m[0])
                        strings.add(m[1])
                        
                    # Also look for btnX.Text = "..."
                    # this is already covered by Text = "..."
            except Exception as e:
                pass

output = list(strings)
output.sort()

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/en_strings.json', 'w', encoding='utf-8') as f:
    json.dump(output, f, ensure_ascii=False, indent=2)

print(f'Extracted {len(output)} strings.')
